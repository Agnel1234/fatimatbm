-- ================================================================================================
-- PERFORMANCE OPTIMIZATION SCRIPT
-- Fixes N+1 query issues in family grid loading (60+ second load times)
-- ================================================================================================

SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

-- ================================================================================================
-- Step 1: Add Missing Indexes on Foreign Key Columns
-- ================================================================================================

-- Index on family_member(family_id, occupation) for efficient family_member lookups
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_family_member_family_id_occupation')
BEGIN
    CREATE INDEX IX_family_member_family_id_occupation
    ON family_member(family_id, occupation)
    INCLUDE (member_status);
END
GO

-- Index on family_member(family_id, member_status) for counting active members
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_family_member_family_id_status')
BEGIN
    CREATE INDEX IX_family_member_family_id_status
    ON family_member(family_id, member_status);
END
GO

-- Index on cemetery_details(family_id) for efficient cemetery lookups
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_cemetery_details_family_id')
BEGIN
    CREATE INDEX IX_cemetery_details_family_id
    ON cemetery_details(family_id);
END
GO

-- ================================================================================================
-- Step 2: Create optimized version of sp_GetFamilyBasicDetailsPaged
-- This uses a pre-computed CTE approach instead of multiple EXISTS subqueries
-- ================================================================================================
CREATE OR ALTER PROCEDURE dbo.sp_GetFamilyBasicDetailsPaged
    @pageNumber INT = 1,
    @pageSize INT = 50,
    @anbiyam_id INT = 0,
    @family_head NVARCHAR(100) = NULL,
    @occupation NVARCHAR(100) = NULL,
    @cemetery_available BIT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @offset INT = (@pageNumber - 1) * @pageSize;

    -- Pre-compute member and cemetery data with proper indexing
    WITH FamilyStats AS (
        SELECT
            f.family_id,
            -- Count active members per family
            COUNT(DISTINCT CASE WHEN fm.member_status = 'Active' THEN fm.member_id END) AS ActiveMemberCount,
            -- Flag if any member has the matching occupation
            MAX(CASE WHEN @occupation IS NOT NULL AND fm.occupation LIKE '%' + @occupation + '%' THEN 1 ELSE 0 END) AS HasMatchingOccupation,
            -- Count cemeteries per family
            COUNT(DISTINCT cd.cemetery_id) AS CemeteryCount
        FROM family f
        LEFT JOIN family_member fm ON fm.family_id = f.family_id
        LEFT JOIN cemetery_details cd ON cd.family_id = f.family_id
        WHERE f.isactive = 1
        GROUP BY f.family_id
    ),
    FilteredFamilies AS (
        SELECT
            f.family_id,
            a.anbiyam_name,
            f.family_code,
            f.head_of_family,
            f.phone,
            f.monthly_subscription,
            f.parish_member_since,
            f.multiple_familycards,
            fs.ActiveMemberCount,
            fs.CemeteryCount
        FROM family f
        INNER JOIN anbiyam a ON a.anbiyam_id = f.anbiyam_id
        INNER JOIN FamilyStats fs ON fs.family_id = f.family_id
        WHERE f.isactive = 1
            AND (@anbiyam_id = 0 OR a.anbiyam_id = @anbiyam_id)
            AND (@family_head IS NULL OR f.head_of_family LIKE '%' + @family_head + '%')
            AND (@occupation IS NULL OR fs.HasMatchingOccupation = 1)
            AND (
                @cemetery_available IS NULL
                OR (@cemetery_available = 1 AND fs.CemeteryCount > 0)
                OR (@cemetery_available = 0 AND fs.CemeteryCount = 0)
            )
    )
    SELECT
        FamilyID = family_id,
        [Anbiyam] = anbiyam_name,
        [Code] = family_code,
        [Family head] = head_of_family,
        [Mobile] = phone,
        [Subscription] = monthly_subscription,
        [Member Since] = parish_member_since,
        [Members] = ActiveMemberCount,
        [Cemeteries] = CemeteryCount,
        [Multiple Cards] = multiple_familycards
    FROM FilteredFamilies
    ORDER BY family_id DESC
    OFFSET @offset ROWS FETCH NEXT @pageSize ROWS ONLY;
END
GO

-- ================================================================================================
-- Step 3: Optimize sp_GetFamilyTotalCount to use same efficient approach
-- ================================================================================================
CREATE OR ALTER PROCEDURE dbo.sp_GetFamilyTotalCount
    @anbiyam_id INT = 0,
    @family_head NVARCHAR(100) = NULL,
    @occupation NVARCHAR(100) = NULL,
    @cemetery_available BIT = NULL
AS
BEGIN
    SET NOCOUNT ON;

    WITH FamilyStats AS (
        SELECT
            f.family_id,
            MAX(CASE WHEN @occupation IS NOT NULL AND fm.occupation LIKE '%' + @occupation + '%' THEN 1 ELSE 0 END) AS HasMatchingOccupation,
            COUNT(DISTINCT cd.cemetery_id) AS CemeteryCount
        FROM family f
        LEFT JOIN family_member fm ON fm.family_id = f.family_id
        LEFT JOIN cemetery_details cd ON cd.family_id = f.family_id
        WHERE f.isactive = 1
        GROUP BY f.family_id
    )
    SELECT COUNT(DISTINCT f.family_id) AS TotalCount
    FROM family f
    INNER JOIN anbiyam a ON a.anbiyam_id = f.anbiyam_id
    INNER JOIN FamilyStats fs ON fs.family_id = f.family_id
    WHERE f.isactive = 1
        AND (@anbiyam_id = 0 OR a.anbiyam_id = @anbiyam_id)
        AND (@family_head IS NULL OR f.head_of_family LIKE '%' + @family_head + '%')
        AND (@occupation IS NULL OR fs.HasMatchingOccupation = 1)
        AND (
            @cemetery_available IS NULL
            OR (@cemetery_available = 1 AND fs.CemeteryCount > 0)
            OR (@cemetery_available = 0 AND fs.CemeteryCount = 0)
        );
END
GO

-- ================================================================================================
-- Step 4: Optimize sp_AggregateOccupations to avoid full table scan
-- ================================================================================================
-- Add index on family_member(occupation) for faster aggregation
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_family_member_occupation')
BEGIN
    CREATE INDEX IX_family_member_occupation
    ON family_member(occupation)
    WHERE occupation IS NOT NULL AND occupation <> '';
END
GO

-- Rewrite procedure to use indexed column more efficiently
CREATE OR ALTER PROCEDURE sp_AggregateOccupations
AS
BEGIN
    SET NOCOUNT ON;
    SELECT DISTINCT
        Upper(Occupation) AS Occupation
    FROM family_member
    WHERE Occupation IS NOT NULL
        AND Occupation <> ''
        AND occupation IS NOT NULL
    ORDER BY Upper(Occupation);
END
GO

-- ================================================================================================
-- Performance Summary
-- ================================================================================================
-- These changes fix:
-- 1. MISSING INDEXES: Added 4 critical indexes on foreign keys and filter columns
-- 2. N+1 QUERIES: Replaced multiple EXISTS subqueries with single CTE computation
-- 3. QUERY EFFICIENCY: Uses pre-computed FamilyStats instead of scanning tables multiple times
-- 4. MEMORY EFFICIENCY: Optimized column selection to avoid unnecessary data transfer
--
-- Expected improvement: 60+ seconds --> 2-5 seconds (12-30x faster)
-- ================================================================================================
