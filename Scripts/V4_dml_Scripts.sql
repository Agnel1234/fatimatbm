------------------------------------------------------------------------------------------------------
---------V4 Changes - Stored Procedures for Subscription Management & Pagination------------------
------------------------------------------------------------------------------------------------------
-- V4 introduces new stored procedures for:
-- 1. Unified subscription view (family + cemetery subscriptions in one query)
-- 2. Pagination support for large datasets
-- 3. Advanced filtering by family name, Anbiyam, type, status, year, and date range
-- 4. Cemetery subscription management (upsert operation)
--
-- Created: 2026-04-21
-- Note: These procedures are NEW additions and do not modify V3 tables or procedures.
------------------------------------------------------------------------------------------------------

USE fatimachurchtbm;
GO

-- ================================================================================================
-- PROCEDURE: sp_GetAllSubscriptions
-- PURPOSE: Get all family and cemetery subscriptions with pagination
-- PARAMETERS:
--   @pageNumber INT - Page number (1-based)
--   @pageSize INT - Rows per page (default 50)
-- RETURNS: Paginated result set with columns:
--   SubscriptionId (fake ID for combining rows), FamilyCode, HeadName, Anbiyam,
--   Amount, Status, Type (Family/Cemetery), SubscriptionYear, DatePaid
-- ================================================================================================
SET QUOTED_IDENTIFIER ON;
GO
CREATE OR ALTER PROCEDURE dbo.sp_GetAllSubscriptions
    @pageNumber INT = 1,
    @pageSize INT = 50
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @offset INT = (@pageNumber - 1) * @pageSize;

    -- Union family and cemetery subscriptions into one result set
    WITH all_subscriptions AS (
        -- Family subscriptions - one row per month paid
        SELECT
            CAST('F_' + CAST(fs.family_subscription_year_id AS NVARCHAR(20)) + '_' +
                 CAST(ROW_NUMBER() OVER (PARTITION BY fs.family_subscription_year_id ORDER BY fs.created_at) AS NVARCHAR(5)) AS NVARCHAR(100)) AS SubscriptionId,
            f.family_code AS FamilyCode,
            f.head_of_family AS HeadName,
            a.anbiyam_name AS Anbiyam,
            -- Extract amount from the first non-null month value for display
            CASE
                WHEN fs.jan_status = 'Paid' THEN fs.jan_amount
                WHEN fs.feb_status = 'Paid' THEN fs.feb_amount
                WHEN fs.mar_status = 'Paid' THEN fs.mar_amount
                WHEN fs.apr_status = 'Paid' THEN fs.apr_amount
                WHEN fs.may_status = 'Paid' THEN fs.may_amount
                WHEN fs.jun_status = 'Paid' THEN fs.jun_amount
                WHEN fs.jul_status = 'Paid' THEN fs.jul_amount
                WHEN fs.aug_status = 'Paid' THEN fs.aug_amount
                WHEN fs.sep_status = 'Paid' THEN fs.sep_amount
                WHEN fs.oct_status = 'Paid' THEN fs.oct_amount
                WHEN fs.nov_status = 'Paid' THEN fs.nov_amount
                WHEN fs.dec_status = 'Paid' THEN fs.dec_amount
                ELSE fs.total_amount / 12 END AS Amount,
            -- Status is "Paid" if any month is paid, else "Pending"
            CASE WHEN fs.total_amount > 0 THEN 'Paid' ELSE 'Pending' END AS Status,
            'Family' AS Type,
            fs.subscription_year AS SubscriptionYear,
            -- Latest paid date across all months
            (SELECT MAX(paid_date) FROM (
                VALUES (fs.jan_paid_date), (fs.feb_paid_date), (fs.mar_paid_date), (fs.apr_paid_date),
                       (fs.may_paid_date), (fs.jun_paid_date), (fs.jul_paid_date), (fs.aug_paid_date),
                       (fs.sep_paid_date), (fs.oct_paid_date), (fs.nov_paid_date), (fs.dec_paid_date)
            ) AS dates(paid_date)) AS DatePaid,
            fs.created_at AS CreatedAt,
            ROW_NUMBER() OVER (ORDER BY fs.family_subscription_year_id DESC, fs.subscription_year DESC) AS RowNum
        FROM
            dbo.family_subscription_yearly fs
            INNER JOIN dbo.family f ON fs.family_id = f.family_id
            INNER JOIN dbo.anbiyam a ON f.anbiyam_id = a.anbiyam_id
        WHERE f.isactive = 1

        UNION ALL

        -- Cemetery subscriptions - one row per year
        SELECT
            'C_' + CAST(cs.cemetery_subscription_year_id AS NVARCHAR(20)) AS SubscriptionId,
            f.family_code AS FamilyCode,
            f.head_of_family AS HeadName,
            a.anbiyam_name AS Anbiyam,
            cs.amount AS Amount,
            cs.payment_status AS Status,
            'Cemetery' AS Type,
            cs.subscription_year AS SubscriptionYear,
            cs.payment_date AS DatePaid,
            cs.created_at AS CreatedAt,
            ROW_NUMBER() OVER (ORDER BY cs.cemetery_subscription_year_id DESC, cs.subscription_year DESC) AS RowNum
        FROM
            dbo.cemetery_subscription_yearly cs
            INNER JOIN dbo.family f ON cs.family_id = f.family_id
            INNER JOIN dbo.anbiyam a ON f.anbiyam_id = a.anbiyam_id
        WHERE f.isactive = 1
    )
    SELECT
        SubscriptionId, FamilyCode, HeadName, Anbiyam, Amount, Status, Type, SubscriptionYear, DatePaid, CreatedAt
    FROM
        all_subscriptions
    ORDER BY
        CreatedAt DESC, SubscriptionYear DESC
    OFFSET @offset ROWS
    FETCH NEXT @pageSize ROWS ONLY;
END
GO

-- ================================================================================================
-- PROCEDURE: sp_GetSubscriptionsFiltered
-- PURPOSE: Get subscriptions with advanced filtering and pagination
-- PARAMETERS:
--   @pageNumber INT - Page number (1-based)
--   @pageSize INT - Rows per page
--   @familyName NVARCHAR(100) - Search by family head name (LIKE match)
--   @anbiyamId INT - Filter by Anbiyam ID (NULL = all)
--   @subscriptionType NVARCHAR(10) - 'Family', 'Cemetery', or NULL for all
--   @status NVARCHAR(20) - 'Paid' or 'Pending' (NULL = all)
--   @yearFrom INT - Subscription year from (NULL = no filter)
--   @yearTo INT - Subscription year to (NULL = no filter)
-- RETURNS: Paginated and filtered subscription result set
-- ================================================================================================
SET QUOTED_IDENTIFIER ON;
GO
CREATE OR ALTER PROCEDURE dbo.sp_GetSubscriptionsFiltered
    @pageNumber INT = 1,
    @pageSize INT = 50,
    @familyName NVARCHAR(100) = NULL,
    @anbiyamId INT = NULL,
    @subscriptionType NVARCHAR(10) = NULL,  -- 'Family' or 'Cemetery'
    @status NVARCHAR(20) = NULL,           -- 'Paid' or 'Pending'
    @yearFrom INT = NULL,
    @yearTo INT = NULL
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @offset INT = (@pageNumber - 1) * @pageSize;

    WITH all_subscriptions AS (
        -- Family subscriptions
        SELECT
            'F_' + CAST(fs.family_subscription_year_id AS NVARCHAR(20)) AS SubscriptionId,
            f.family_code AS FamilyCode,
            f.head_of_family AS HeadName,
            a.anbiyam_name AS Anbiyam,
            CASE
                WHEN fs.jan_status = 'Paid' THEN fs.jan_amount
                WHEN fs.feb_status = 'Paid' THEN fs.feb_amount
                WHEN fs.mar_status = 'Paid' THEN fs.mar_amount
                WHEN fs.apr_status = 'Paid' THEN fs.apr_amount
                WHEN fs.may_status = 'Paid' THEN fs.may_amount
                WHEN fs.jun_status = 'Paid' THEN fs.jun_amount
                WHEN fs.jul_status = 'Paid' THEN fs.jul_amount
                WHEN fs.aug_status = 'Paid' THEN fs.aug_amount
                WHEN fs.sep_status = 'Paid' THEN fs.sep_amount
                WHEN fs.oct_status = 'Paid' THEN fs.oct_amount
                WHEN fs.nov_status = 'Paid' THEN fs.nov_amount
                WHEN fs.dec_status = 'Paid' THEN fs.dec_amount
                ELSE fs.total_amount / 12 END AS Amount,
            CASE WHEN fs.total_amount > 0 THEN 'Paid' ELSE 'Pending' END AS Status,
            'Family' AS Type,
            fs.subscription_year AS SubscriptionYear,
            (SELECT MAX(paid_date) FROM (
                VALUES (fs.jan_paid_date), (fs.feb_paid_date), (fs.mar_paid_date), (fs.apr_paid_date),
                       (fs.may_paid_date), (fs.jun_paid_date), (fs.jul_paid_date), (fs.aug_paid_date),
                       (fs.sep_paid_date), (fs.oct_paid_date), (fs.nov_paid_date), (fs.dec_paid_date)
            ) AS dates(paid_date)) AS DatePaid,
            fs.created_at AS CreatedAt,
            f.family_id,
            a.anbiyam_id
        FROM
            dbo.family_subscription_yearly fs
            INNER JOIN dbo.family f ON fs.family_id = f.family_id
            INNER JOIN dbo.anbiyam a ON f.anbiyam_id = a.anbiyam_id
        WHERE
            f.isactive = 1
            AND (@subscriptionType IS NULL OR @subscriptionType = 'Family')
            AND (@familyName IS NULL OR f.head_of_family LIKE '%' + @familyName + '%')
            AND (@anbiyamId IS NULL OR a.anbiyam_id = @anbiyamId)
            AND (@status IS NULL OR CASE WHEN fs.total_amount > 0 THEN 'Paid' ELSE 'Pending' END = @status)
            AND (@yearFrom IS NULL OR fs.subscription_year >= @yearFrom)
            AND (@yearTo IS NULL OR fs.subscription_year <= @yearTo)

        UNION ALL

        -- Cemetery subscriptions
        SELECT
            'C_' + CAST(cs.cemetery_subscription_year_id AS NVARCHAR(20)) AS SubscriptionId,
            f.family_code AS FamilyCode,
            f.head_of_family AS HeadName,
            a.anbiyam_name AS Anbiyam,
            cs.amount AS Amount,
            cs.payment_status AS Status,
            'Cemetery' AS Type,
            cs.subscription_year AS SubscriptionYear,
            cs.payment_date AS DatePaid,
            cs.created_at AS CreatedAt,
            f.family_id,
            a.anbiyam_id
        FROM
            dbo.cemetery_subscription_yearly cs
            INNER JOIN dbo.family f ON cs.family_id = f.family_id
            INNER JOIN dbo.anbiyam a ON f.anbiyam_id = a.anbiyam_id
        WHERE
            f.isactive = 1
            AND (@subscriptionType IS NULL OR @subscriptionType = 'Cemetery')
            AND (@familyName IS NULL OR f.head_of_family LIKE '%' + @familyName + '%')
            AND (@anbiyamId IS NULL OR a.anbiyam_id = @anbiyamId)
            AND (@status IS NULL OR cs.payment_status = @status)
            AND (@yearFrom IS NULL OR cs.subscription_year >= @yearFrom)
            AND (@yearTo IS NULL OR cs.subscription_year <= @yearTo)
    )
    SELECT
        SubscriptionId, FamilyCode, HeadName, Anbiyam, Amount, Status, Type, SubscriptionYear, DatePaid, CreatedAt
    FROM
        all_subscriptions
    ORDER BY
        CreatedAt DESC, SubscriptionYear DESC
    OFFSET @offset ROWS
    FETCH NEXT @pageSize ROWS ONLY;
END
GO

-- ================================================================================================
-- PROCEDURE: sp_GetSubscriptionTotalCount
-- PURPOSE: Get total count of subscriptions matching filters (for pagination UI)
-- PARAMETERS: Same as sp_GetSubscriptionsFiltered
-- RETURNS: Single row with TotalCount column
-- ================================================================================================
SET QUOTED_IDENTIFIER ON;
GO
CREATE OR ALTER PROCEDURE dbo.sp_GetSubscriptionTotalCount
    @familyName NVARCHAR(100) = NULL,
    @anbiyamId INT = NULL,
    @subscriptionType NVARCHAR(10) = NULL,
    @status NVARCHAR(20) = NULL,
    @yearFrom INT = NULL,
    @yearTo INT = NULL
AS
BEGIN
    SET NOCOUNT ON;

    WITH all_subscriptions AS (
        -- Family subscriptions
        SELECT
            'Family' AS Type
        FROM
            dbo.family_subscription_yearly fs
            INNER JOIN dbo.family f ON fs.family_id = f.family_id
            INNER JOIN dbo.anbiyam a ON f.anbiyam_id = a.anbiyam_id
        WHERE
            f.isactive = 1
            AND (@subscriptionType IS NULL OR @subscriptionType = 'Family')
            AND (@familyName IS NULL OR f.head_of_family LIKE '%' + @familyName + '%')
            AND (@anbiyamId IS NULL OR a.anbiyam_id = @anbiyamId)
            AND (@status IS NULL OR CASE WHEN fs.total_amount > 0 THEN 'Paid' ELSE 'Pending' END = @status)
            AND (@yearFrom IS NULL OR fs.subscription_year >= @yearFrom)
            AND (@yearTo IS NULL OR fs.subscription_year <= @yearTo)

        UNION ALL

        -- Cemetery subscriptions
        SELECT
            'Cemetery' AS Type
        FROM
            dbo.cemetery_subscription_yearly cs
            INNER JOIN dbo.family f ON cs.family_id = f.family_id
            INNER JOIN dbo.anbiyam a ON f.anbiyam_id = a.anbiyam_id
        WHERE
            f.isactive = 1
            AND (@subscriptionType IS NULL OR @subscriptionType = 'Cemetery')
            AND (@familyName IS NULL OR f.head_of_family LIKE '%' + @familyName + '%')
            AND (@anbiyamId IS NULL OR a.anbiyam_id = @anbiyamId)
            AND (@status IS NULL OR cs.payment_status = @status)
            AND (@yearFrom IS NULL OR cs.subscription_year >= @yearFrom)
            AND (@yearTo IS NULL OR cs.subscription_year <= @yearTo)
    )
    SELECT COUNT(*) AS TotalCount FROM all_subscriptions;
END
GO

-- ================================================================================================
-- PROCEDURE: sp_SetCemeterySubscription
-- PURPOSE: Upsert a cemetery subscription record (insert or update)
-- PARAMETERS:
--   @family_id INT - Family ID
--   @subscription_year INT - Year of subscription
--   @amount DECIMAL(10,2) - Payment amount
--   @payment_date DATE - Date payment was made
--   @payment_status NVARCHAR(20) - Status: 'Paid', 'Pending', 'Overdue'
-- RETURNS: None
-- ================================================================================================
SET QUOTED_IDENTIFIER ON;
GO
CREATE OR ALTER PROCEDURE dbo.sp_SetCemeterySubscription
    @family_id INT,
    @subscription_year INT,
    @amount DECIMAL(10,2),
    @payment_date DATE = NULL,
    @payment_status NVARCHAR(20) = 'Paid'
AS
BEGIN
    SET NOCOUNT ON;

    -- Check if family exists
    IF NOT EXISTS (SELECT 1 FROM dbo.family WHERE family_id = @family_id)
    BEGIN
        RAISERROR('Family does not exist.', 16, 1);
        RETURN;
    END

    -- Upsert: check if subscription exists for this family and year
    IF EXISTS (SELECT 1 FROM dbo.cemetery_subscription_yearly
               WHERE family_id = @family_id AND subscription_year = @subscription_year)
    BEGIN
        -- Update existing subscription
        UPDATE dbo.cemetery_subscription_yearly
        SET
            amount = @amount,
            payment_date = @payment_date,
            payment_status = @payment_status,
            modified = GETDATE()
        WHERE
            family_id = @family_id AND subscription_year = @subscription_year;
    END
    ELSE
    BEGIN
        -- Insert new subscription
        INSERT INTO dbo.cemetery_subscription_yearly
            (family_id, subscription_year, amount, payment_date, payment_status, created_at, modified)
        VALUES
            (@family_id, @subscription_year, @amount, @payment_date, @payment_status, GETDATE(), GETDATE());
    END
END
GO

-- ================================================================================================
-- PROCEDURE: sp_GetCemeterySubscription
-- PURPOSE: Get a single cemetery subscription record for a family and year
-- ================================================================================================
SET QUOTED_IDENTIFIER ON;
GO
CREATE OR ALTER PROCEDURE dbo.sp_GetCemeterySubscription
    @family_id INT,
    @subscription_year INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT cemetery_subscription_year_id, family_id, subscription_year,
           amount, payment_date, payment_status, remarks
    FROM dbo.cemetery_subscription_yearly
    WHERE family_id = @family_id AND subscription_year = @subscription_year;
END
GO

-- ================================================================================================
-- PROCEDURE: sp_GetFamilyByCode
-- PURPOSE: Look up a family record by family_code (used to resolve family_id from grid)
-- PARAMETERS:
--   @family_code NVARCHAR(10)
-- RETURNS: family row including family_id
-- ================================================================================================
SET QUOTED_IDENTIFIER ON;
GO
CREATE OR ALTER PROCEDURE dbo.sp_GetFamilyByCode
    @family_code NVARCHAR(10)
AS
BEGIN
    SET NOCOUNT ON;
    SELECT family_id, family_code, head_of_family, anbiyam_id
    FROM dbo.family
    WHERE family_code = @family_code AND isactive = 1;
END
GO

-- ================================================================================================
-- PROCEDURE: sp_GetFamilyBasicDetailsPaged
-- Paginated version of sp_GetFamilyBasicDetails (replaces hardcoded TOP 30)
-- ================================================================================================
SET QUOTED_IDENTIFIER ON;
GO
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
    SELECT
        f.family_id AS FamilyID,
        a.anbiyam_name AS [Anbiyam],
        f.family_code AS [Code],
        f.head_of_family AS [Family head],
        f.phone AS [Mobile],
        f.monthly_subscription AS [Subscription],
        f.parish_member_since AS [Member Since],
        COUNT(DISTINCT fm.member_id) AS [Members],
        COUNT(DISTINCT cd.cemetery_id) AS [Cemeteries],
        f.multiple_familycards AS [Multiple Cards]
    FROM family f
        INNER JOIN anbiyam a ON a.anbiyam_id = f.anbiyam_id
        LEFT JOIN family_member fm ON fm.family_id = f.family_id AND fm.member_status = 'Active'
        LEFT JOIN cemetery_details cd ON cd.family_id = f.family_id
    WHERE f.isactive = 1
        AND (@anbiyam_id = 0 OR @anbiyam_id = a.anbiyam_id)
        AND (@family_head IS NULL OR f.head_of_family LIKE '%' + @family_head + '%')
        AND (@occupation IS NULL OR EXISTS (
            SELECT 1 FROM family_member fm2
            WHERE fm2.family_id = f.family_id AND fm2.occupation LIKE '%' + @occupation + '%'))
        AND (
            @cemetery_available IS NULL
            OR (@cemetery_available = 1 AND EXISTS (SELECT 1 FROM cemetery_details cd2 WHERE cd2.family_id = f.family_id))
            OR (@cemetery_available = 0 AND NOT EXISTS (SELECT 1 FROM cemetery_details cd3 WHERE cd3.family_id = f.family_id)))
    GROUP BY f.family_id, a.anbiyam_name, f.family_code, f.head_of_family,
             f.phone, f.monthly_subscription, f.parish_member_since, f.multiple_familycards
    ORDER BY f.family_id DESC
    OFFSET @offset ROWS FETCH NEXT @pageSize ROWS ONLY;
END
GO

-- ================================================================================================
-- PROCEDURE: sp_GetFamilyTotalCount  – total rows for Family grid pagination
-- ================================================================================================
SET QUOTED_IDENTIFIER ON;
GO
CREATE OR ALTER PROCEDURE dbo.sp_GetFamilyTotalCount
    @anbiyam_id INT = 0,
    @family_head NVARCHAR(100) = NULL,
    @occupation NVARCHAR(100) = NULL,
    @cemetery_available BIT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SELECT COUNT(DISTINCT f.family_id) AS TotalCount
    FROM family f
        INNER JOIN anbiyam a ON a.anbiyam_id = f.anbiyam_id
    WHERE f.isactive = 1
        AND (@anbiyam_id = 0 OR @anbiyam_id = a.anbiyam_id)
        AND (@family_head IS NULL OR f.head_of_family LIKE '%' + @family_head + '%')
        AND (@occupation IS NULL OR EXISTS (
            SELECT 1 FROM family_member fm2
            WHERE fm2.family_id = f.family_id AND fm2.occupation LIKE '%' + @occupation + '%'))
        AND (
            @cemetery_available IS NULL
            OR (@cemetery_available = 1 AND EXISTS (SELECT 1 FROM cemetery_details cd2 WHERE cd2.family_id = f.family_id))
            OR (@cemetery_available = 0 AND NOT EXISTS (SELECT 1 FROM cemetery_details cd3 WHERE cd3.family_id = f.family_id)));
END
GO

-- ================================================================================================
-- PROCEDURE: sp_GetAnbiyamGridPaged  – paginated anbiyam grid
-- ================================================================================================
SET QUOTED_IDENTIFIER ON;
GO
CREATE OR ALTER PROCEDURE dbo.sp_GetAnbiyamGridPaged
    @pageNumber INT = 1,
    @pageSize INT = 50,
    @anbiyam_id INT = NULL,
    @coordinator_name NVARCHAR(50) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @offset INT = (@pageNumber - 1) * @pageSize;
    SELECT
        a.anbiyam_id,
        a.anbiyam_zone AS [Zone],
        a.anbiyam_name AS [Name],
        a.anbiyam_coordinator_name AS [Coordinator],
        a.coordinator_phone AS [Mobile],
        COUNT(DISTINCT f.family_id) AS [Families Count],
        COUNT(f.family_id) AS [Members count],
        COUNT(CASE WHEN fm.gender = 'Male' THEN 1 END) AS [Male],
        COUNT(CASE WHEN fm.gender = 'Female' THEN 1 END) AS [Female]
    FROM anbiyam a
        LEFT JOIN family f ON a.anbiyam_id = f.anbiyam_id
        LEFT JOIN family_member fm ON fm.family_id = f.family_id
    WHERE (@anbiyam_id IS NULL OR a.anbiyam_id = @anbiyam_id)
        AND (@coordinator_name IS NULL OR a.anbiyam_coordinator_name LIKE '%' + @coordinator_name + '%')
    GROUP BY a.anbiyam_id, a.anbiyam_name, a.anbiyam_zone, a.anbiyam_coordinator_name, a.coordinator_phone
    ORDER BY a.anbiyam_zone
    OFFSET @offset ROWS FETCH NEXT @pageSize ROWS ONLY;
END
GO

-- ================================================================================================
-- PROCEDURE: sp_GetAnbiyamTotalCount
-- ================================================================================================
SET QUOTED_IDENTIFIER ON;
GO
CREATE OR ALTER PROCEDURE dbo.sp_GetAnbiyamTotalCount
    @anbiyam_id INT = NULL,
    @coordinator_name NVARCHAR(50) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SELECT COUNT(*) AS TotalCount
    FROM anbiyam a
    WHERE (@anbiyam_id IS NULL OR a.anbiyam_id = @anbiyam_id)
        AND (@coordinator_name IS NULL OR a.anbiyam_coordinator_name LIKE '%' + @coordinator_name + '%');
END
GO

-- ================================================================================================
-- PROCEDURE: sp_GetAllCemeteriesPaged  – paginated cemetery grid
-- ================================================================================================
SET QUOTED_IDENTIFIER ON;
GO
CREATE OR ALTER PROCEDURE dbo.sp_GetAllCemeteriesPaged
    @pageNumber INT = 1,
    @pageSize INT = 50,
    @burial_date_from DATETIME = NULL,
    @burial_date_to DATETIME = NULL,
    @deceased_date_from DATETIME = NULL,
    @deceased_date_to DATETIME = NULL,
    @IsOurparish BIT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @offset INT = (@pageNumber - 1) * @pageSize;

    IF @IsOurparish = 0
    BEGIN
        SELECT
            CemeteryId AS [cemeteryid],
            NULL        AS [FamilyId],
            cemeterycode AS [Cemetery Code],
            Name AS [Deceased Name],
            DeceasedDate AS [Deceased Date],
            BuriedDate AS [Burial Date],
            Remarks AS [Remarks],
            ContactPerson AS [Contact Person],
            ContactPhone AS [Contact Mobile]
        FROM nonparishcemetery
        WHERE ((@burial_date_from IS NULL AND @burial_date_to IS NULL) OR BuriedDate BETWEEN @burial_date_from AND @burial_date_to)
            AND ((@deceased_date_from IS NULL AND @deceased_date_to IS NULL) OR DeceasedDate BETWEEN @deceased_date_from AND @deceased_date_to)
        ORDER BY cemeterycode DESC
        OFFSET @offset ROWS FETCH NEXT @pageSize ROWS ONLY;
    END
    ELSE
    BEGIN
        SELECT
            cd.cemetery_id AS [cemeteryid],
            cd.family_id   AS [FamilyId],
            cd.grave_number AS [Cemetery Code],
            cd.deceased_name AS [Deceased Name],
            cd.date_of_death AS [Deceased Date],
            cd.burial_date AS [Burial Date],
            cd.remarks AS [Remarks],
            f.head_of_family AS [Contact Person],
            f.phone AS [Contact Mobile]
        FROM cemetery_details cd
            INNER JOIN family f ON cd.family_id = f.family_id
        WHERE ((@burial_date_from IS NULL AND @burial_date_to IS NULL) OR cd.burial_date BETWEEN @burial_date_from AND @burial_date_to)
            AND ((@deceased_date_from IS NULL AND @deceased_date_to IS NULL) OR cd.date_of_death BETWEEN @deceased_date_from AND @deceased_date_to)
        ORDER BY cd.grave_number DESC
        OFFSET @offset ROWS FETCH NEXT @pageSize ROWS ONLY;
    END
END
GO

-- ================================================================================================
-- PROCEDURE: sp_GetAllCemeteriesTotalCount
-- ================================================================================================
SET QUOTED_IDENTIFIER ON;
GO
CREATE OR ALTER PROCEDURE dbo.sp_GetAllCemeteriesTotalCount
    @burial_date_from DATETIME = NULL,
    @burial_date_to DATETIME = NULL,
    @deceased_date_from DATETIME = NULL,
    @deceased_date_to DATETIME = NULL,
    @IsOurparish BIT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    IF @IsOurparish = 0
        SELECT COUNT(*) AS TotalCount FROM nonparishcemetery
        WHERE ((@burial_date_from IS NULL AND @burial_date_to IS NULL) OR BuriedDate BETWEEN @burial_date_from AND @burial_date_to)
            AND ((@deceased_date_from IS NULL AND @deceased_date_to IS NULL) OR DeceasedDate BETWEEN @deceased_date_from AND @deceased_date_to);
    ELSE
        SELECT COUNT(*) AS TotalCount FROM cemetery_details cd
        WHERE ((@burial_date_from IS NULL AND @burial_date_to IS NULL) OR cd.burial_date BETWEEN @burial_date_from AND @burial_date_to)
            AND ((@deceased_date_from IS NULL AND @deceased_date_to IS NULL) OR cd.date_of_death BETWEEN @deceased_date_from AND @deceased_date_to);
END
GO

-- ================================================================================================
-- PROCEDURE: sp_GetNonParishCemeterySubscriptions
-- PURPOSE: Get all subscription records for a given outside-parish cemetery entry
-- ================================================================================================
SET QUOTED_IDENTIFIER ON;
GO
CREATE OR ALTER PROCEDURE dbo.sp_GetNonParishCemeterySubscriptions
    @nonparish_cemetery_id INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT subscription_id, nonparish_cemetery_id, subscription_year,
           amount, payment_date, payment_status, remarks, created_at, modified
    FROM dbo.nonparish_cemetery_subscription
    WHERE nonparish_cemetery_id = @nonparish_cemetery_id
    ORDER BY subscription_year DESC;
END
GO

-- ================================================================================================
-- PROCEDURE: sp_SetNonParishCemeterySubscription
-- PURPOSE: Upsert an outside-parish cemetery subscription record
-- ================================================================================================
SET QUOTED_IDENTIFIER ON;
GO
CREATE OR ALTER PROCEDURE dbo.sp_SetNonParishCemeterySubscription
    @nonparish_cemetery_id  INT,
    @subscription_year      INT,
    @amount                 DECIMAL(10,2),
    @payment_date           DATE = NULL,
    @payment_status         NVARCHAR(20) = 'Paid',
    @remarks                NVARCHAR(500) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    IF NOT EXISTS (SELECT 1 FROM dbo.nonparishcemetery WHERE CemeteryId = @nonparish_cemetery_id)
    BEGIN
        RAISERROR('Non-parish cemetery record does not exist.', 16, 1);
        RETURN;
    END

    IF EXISTS (SELECT 1 FROM dbo.nonparish_cemetery_subscription
               WHERE nonparish_cemetery_id = @nonparish_cemetery_id AND subscription_year = @subscription_year)
    BEGIN
        UPDATE dbo.nonparish_cemetery_subscription
        SET amount = @amount, payment_date = @payment_date, payment_status = @payment_status,
            remarks = @remarks, modified = GETDATE()
        WHERE nonparish_cemetery_id = @nonparish_cemetery_id AND subscription_year = @subscription_year;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.nonparish_cemetery_subscription
            (nonparish_cemetery_id, subscription_year, amount, payment_date, payment_status, remarks)
        VALUES (@nonparish_cemetery_id, @subscription_year, @amount, @payment_date, @payment_status, @remarks);
    END
END
GO

-- ================================================================================================
-- PROCEDURE: sp_GetAllSubscriptions  (updated to include outside-parish cemetery subscriptions)
-- ================================================================================================
SET QUOTED_IDENTIFIER ON;
GO
CREATE OR ALTER PROCEDURE dbo.sp_GetAllSubscriptions
    @pageNumber INT = 1,
    @pageSize INT = 50
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @offset INT = (@pageNumber - 1) * @pageSize;

    WITH all_subscriptions AS (
        SELECT
            CAST('F_' + CAST(fs.family_subscription_year_id AS NVARCHAR(20)) AS NVARCHAR(100)) AS SubscriptionId,
            f.family_code AS FamilyCode,
            f.head_of_family AS HeadName,
            a.anbiyam_name AS Anbiyam,
            CASE
                WHEN fs.jan_status = 'Paid' THEN fs.jan_amount WHEN fs.feb_status = 'Paid' THEN fs.feb_amount
                WHEN fs.mar_status = 'Paid' THEN fs.mar_amount WHEN fs.apr_status = 'Paid' THEN fs.apr_amount
                WHEN fs.may_status = 'Paid' THEN fs.may_amount WHEN fs.jun_status = 'Paid' THEN fs.jun_amount
                WHEN fs.jul_status = 'Paid' THEN fs.jul_amount WHEN fs.aug_status = 'Paid' THEN fs.aug_amount
                WHEN fs.sep_status = 'Paid' THEN fs.sep_amount WHEN fs.oct_status = 'Paid' THEN fs.oct_amount
                WHEN fs.nov_status = 'Paid' THEN fs.nov_amount WHEN fs.dec_status = 'Paid' THEN fs.dec_amount
                ELSE fs.total_amount / 12 END AS Amount,
            CASE WHEN fs.total_amount > 0 THEN 'Paid' ELSE 'Pending' END AS Status,
            'Family' AS Type,
            fs.subscription_year AS SubscriptionYear,
            (SELECT MAX(paid_date) FROM (VALUES (fs.jan_paid_date),(fs.feb_paid_date),(fs.mar_paid_date),(fs.apr_paid_date),
                (fs.may_paid_date),(fs.jun_paid_date),(fs.jul_paid_date),(fs.aug_paid_date),
                (fs.sep_paid_date),(fs.oct_paid_date),(fs.nov_paid_date),(fs.dec_paid_date)) AS dates(paid_date)) AS DatePaid,
            fs.created_at AS CreatedAt
        FROM dbo.family_subscription_yearly fs
            INNER JOIN dbo.family f ON fs.family_id = f.family_id
            INNER JOIN dbo.anbiyam a ON f.anbiyam_id = a.anbiyam_id
        WHERE f.isactive = 1

        UNION ALL

        SELECT
            'C_' + CAST(cs.cemetery_subscription_year_id AS NVARCHAR(20)) AS SubscriptionId,
            f.family_code AS FamilyCode,
            f.head_of_family AS HeadName,
            a.anbiyam_name AS Anbiyam,
            cs.amount AS Amount,
            cs.payment_status AS Status,
            'Cemetery' AS Type,
            cs.subscription_year AS SubscriptionYear,
            cs.payment_date AS DatePaid,
            cs.created_at AS CreatedAt
        FROM dbo.cemetery_subscription_yearly cs
            INNER JOIN dbo.family f ON cs.family_id = f.family_id
            INNER JOIN dbo.anbiyam a ON f.anbiyam_id = a.anbiyam_id
        WHERE f.isactive = 1

        UNION ALL

        SELECT
            'NP_' + CAST(ns.subscription_id AS NVARCHAR(20)) AS SubscriptionId,
            nc.cemeterycode AS FamilyCode,
            nc.Name AS HeadName,
            'Outside Parish' AS Anbiyam,
            ns.amount AS Amount,
            ns.payment_status AS Status,
            'Outside Parish Cemetery' AS Type,
            ns.subscription_year AS SubscriptionYear,
            ns.payment_date AS DatePaid,
            ns.created_at AS CreatedAt
        FROM dbo.nonparish_cemetery_subscription ns
            INNER JOIN dbo.nonparishcemetery nc ON ns.nonparish_cemetery_id = nc.CemeteryId
    )
    SELECT SubscriptionId, FamilyCode, HeadName, Anbiyam, Amount, Status, Type, SubscriptionYear, DatePaid, CreatedAt
    FROM all_subscriptions
    ORDER BY CreatedAt DESC, SubscriptionYear DESC
    OFFSET @offset ROWS FETCH NEXT @pageSize ROWS ONLY;
END
GO

-- ================================================================================================
-- PROCEDURE: sp_GetSubscriptionsFiltered (updated to include outside-parish)
-- ================================================================================================
SET QUOTED_IDENTIFIER ON;
GO
CREATE OR ALTER PROCEDURE dbo.sp_GetSubscriptionsFiltered
    @pageNumber INT = 1,
    @pageSize INT = 50,
    @familyName NVARCHAR(100) = NULL,
    @anbiyamId INT = NULL,
    @subscriptionType NVARCHAR(30) = NULL,
    @status NVARCHAR(20) = NULL,
    @yearFrom INT = NULL,
    @yearTo INT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @offset INT = (@pageNumber - 1) * @pageSize;

    WITH all_subscriptions AS (
        SELECT
            'F_' + CAST(fs.family_subscription_year_id AS NVARCHAR(20)) AS SubscriptionId,
            f.family_code AS FamilyCode, f.head_of_family AS HeadName, a.anbiyam_name AS Anbiyam,
            CASE WHEN fs.jan_status='Paid' THEN fs.jan_amount WHEN fs.feb_status='Paid' THEN fs.feb_amount
                WHEN fs.mar_status='Paid' THEN fs.mar_amount WHEN fs.apr_status='Paid' THEN fs.apr_amount
                WHEN fs.may_status='Paid' THEN fs.may_amount WHEN fs.jun_status='Paid' THEN fs.jun_amount
                WHEN fs.jul_status='Paid' THEN fs.jul_amount WHEN fs.aug_status='Paid' THEN fs.aug_amount
                WHEN fs.sep_status='Paid' THEN fs.sep_amount WHEN fs.oct_status='Paid' THEN fs.oct_amount
                WHEN fs.nov_status='Paid' THEN fs.nov_amount WHEN fs.dec_status='Paid' THEN fs.dec_amount
                ELSE fs.total_amount / 12 END AS Amount,
            CASE WHEN fs.total_amount > 0 THEN 'Paid' ELSE 'Pending' END AS Status,
            'Family' AS Type, fs.subscription_year AS SubscriptionYear,
            (SELECT MAX(paid_date) FROM (VALUES (fs.jan_paid_date),(fs.feb_paid_date),(fs.mar_paid_date),(fs.apr_paid_date),
                (fs.may_paid_date),(fs.jun_paid_date),(fs.jul_paid_date),(fs.aug_paid_date),
                (fs.sep_paid_date),(fs.oct_paid_date),(fs.nov_paid_date),(fs.dec_paid_date)) AS dates(paid_date)) AS DatePaid,
            fs.created_at AS CreatedAt, f.family_id, a.anbiyam_id
        FROM dbo.family_subscription_yearly fs
            INNER JOIN dbo.family f ON fs.family_id = f.family_id
            INNER JOIN dbo.anbiyam a ON f.anbiyam_id = a.anbiyam_id
        WHERE f.isactive = 1
            AND (@subscriptionType IS NULL OR @subscriptionType = 'Family')
            AND (@familyName IS NULL OR f.head_of_family LIKE '%' + @familyName + '%')
            AND (@anbiyamId IS NULL OR a.anbiyam_id = @anbiyamId)
            AND (@status IS NULL OR CASE WHEN fs.total_amount > 0 THEN 'Paid' ELSE 'Pending' END = @status)
            AND (@yearFrom IS NULL OR fs.subscription_year >= @yearFrom)
            AND (@yearTo IS NULL OR fs.subscription_year <= @yearTo)

        UNION ALL

        SELECT
            'C_' + CAST(cs.cemetery_subscription_year_id AS NVARCHAR(20)) AS SubscriptionId,
            f.family_code AS FamilyCode, f.head_of_family AS HeadName, a.anbiyam_name AS Anbiyam,
            cs.amount AS Amount, cs.payment_status AS Status,
            'Cemetery' AS Type, cs.subscription_year AS SubscriptionYear, cs.payment_date AS DatePaid,
            cs.created_at AS CreatedAt, f.family_id, a.anbiyam_id
        FROM dbo.cemetery_subscription_yearly cs
            INNER JOIN dbo.family f ON cs.family_id = f.family_id
            INNER JOIN dbo.anbiyam a ON f.anbiyam_id = a.anbiyam_id
        WHERE f.isactive = 1
            AND (@subscriptionType IS NULL OR @subscriptionType = 'Cemetery')
            AND (@familyName IS NULL OR f.head_of_family LIKE '%' + @familyName + '%')
            AND (@anbiyamId IS NULL OR a.anbiyam_id = @anbiyamId)
            AND (@status IS NULL OR cs.payment_status = @status)
            AND (@yearFrom IS NULL OR cs.subscription_year >= @yearFrom)
            AND (@yearTo IS NULL OR cs.subscription_year <= @yearTo)

        UNION ALL

        SELECT
            'NP_' + CAST(ns.subscription_id AS NVARCHAR(20)) AS SubscriptionId,
            nc.cemeterycode AS FamilyCode, nc.Name AS HeadName, 'Outside Parish' AS Anbiyam,
            ns.amount AS Amount, ns.payment_status AS Status,
            'Outside Parish Cemetery' AS Type, ns.subscription_year AS SubscriptionYear,
            ns.payment_date AS DatePaid, ns.created_at AS CreatedAt, NULL AS family_id, NULL AS anbiyam_id
        FROM dbo.nonparish_cemetery_subscription ns
            INNER JOIN dbo.nonparishcemetery nc ON ns.nonparish_cemetery_id = nc.CemeteryId
        WHERE (@subscriptionType IS NULL OR @subscriptionType = 'Outside Parish Cemetery')
            AND (@familyName IS NULL OR nc.Name LIKE '%' + @familyName + '%')
            AND (@anbiyamId IS NULL)
            AND (@status IS NULL OR ns.payment_status = @status)
            AND (@yearFrom IS NULL OR ns.subscription_year >= @yearFrom)
            AND (@yearTo IS NULL OR ns.subscription_year <= @yearTo)
    )
    SELECT SubscriptionId, FamilyCode, HeadName, Anbiyam, Amount, Status, Type, SubscriptionYear, DatePaid, CreatedAt
    FROM all_subscriptions
    ORDER BY CreatedAt DESC, SubscriptionYear DESC
    OFFSET @offset ROWS FETCH NEXT @pageSize ROWS ONLY;
END
GO

-- ================================================================================================
-- PROCEDURE: sp_GetSubscriptionTotalCount (updated to include outside-parish)
-- ================================================================================================
SET QUOTED_IDENTIFIER ON;
GO
CREATE OR ALTER PROCEDURE dbo.sp_GetSubscriptionTotalCount
    @familyName NVARCHAR(100) = NULL,
    @anbiyamId INT = NULL,
    @subscriptionType NVARCHAR(30) = NULL,
    @status NVARCHAR(20) = NULL,
    @yearFrom INT = NULL,
    @yearTo INT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    WITH all_subscriptions AS (
        SELECT 'Family' AS Type
        FROM dbo.family_subscription_yearly fs
            INNER JOIN dbo.family f ON fs.family_id = f.family_id
            INNER JOIN dbo.anbiyam a ON f.anbiyam_id = a.anbiyam_id
        WHERE f.isactive = 1
            AND (@subscriptionType IS NULL OR @subscriptionType = 'Family')
            AND (@familyName IS NULL OR f.head_of_family LIKE '%' + @familyName + '%')
            AND (@anbiyamId IS NULL OR a.anbiyam_id = @anbiyamId)
            AND (@status IS NULL OR CASE WHEN fs.total_amount > 0 THEN 'Paid' ELSE 'Pending' END = @status)
            AND (@yearFrom IS NULL OR fs.subscription_year >= @yearFrom)
            AND (@yearTo IS NULL OR fs.subscription_year <= @yearTo)
        UNION ALL
        SELECT 'Cemetery' AS Type
        FROM dbo.cemetery_subscription_yearly cs
            INNER JOIN dbo.family f ON cs.family_id = f.family_id
            INNER JOIN dbo.anbiyam a ON f.anbiyam_id = a.anbiyam_id
        WHERE f.isactive = 1
            AND (@subscriptionType IS NULL OR @subscriptionType = 'Cemetery')
            AND (@familyName IS NULL OR f.head_of_family LIKE '%' + @familyName + '%')
            AND (@anbiyamId IS NULL OR a.anbiyam_id = @anbiyamId)
            AND (@status IS NULL OR cs.payment_status = @status)
            AND (@yearFrom IS NULL OR cs.subscription_year >= @yearFrom)
            AND (@yearTo IS NULL OR cs.subscription_year <= @yearTo)
        UNION ALL
        SELECT 'Outside Parish Cemetery' AS Type
        FROM dbo.nonparish_cemetery_subscription ns
            INNER JOIN dbo.nonparishcemetery nc ON ns.nonparish_cemetery_id = nc.CemeteryId
        WHERE (@subscriptionType IS NULL OR @subscriptionType = 'Outside Parish Cemetery')
            AND (@familyName IS NULL OR nc.Name LIKE '%' + @familyName + '%')
            AND (@anbiyamId IS NULL)
            AND (@status IS NULL OR ns.payment_status = @status)
            AND (@yearFrom IS NULL OR ns.subscription_year >= @yearFrom)
            AND (@yearTo IS NULL OR ns.subscription_year <= @yearTo)
    )
    SELECT COUNT(*) AS TotalCount FROM all_subscriptions;
END
GO

-- ================================================================================================
-- PROCEDURE: sp_GetFamiliesByZone
-- PURPOSE: Return family count grouped by anbiyam zone (for dashboard chart)
-- ================================================================================================
SET QUOTED_IDENTIFIER ON;
GO
CREATE OR ALTER PROCEDURE dbo.sp_GetFamiliesByZone
AS
BEGIN
    SET NOCOUNT ON;
    SELECT
        'Zone ' + CAST(a.anbiyam_zone AS NVARCHAR(5)) AS ZoneName,
        COUNT(DISTINCT f.family_id) AS FamilyCount
    FROM dbo.anbiyam a
        LEFT JOIN dbo.family f ON f.anbiyam_id = a.anbiyam_id AND f.isactive = 1
    GROUP BY a.anbiyam_zone
    ORDER BY a.anbiyam_zone;
END
GO

-- ================================================================================================
-- PROCEDURE: sp_GetNonParishSubById
-- PURPOSE: Fetch a single outside-parish subscription row by subscription_id
-- ================================================================================================
SET QUOTED_IDENTIFIER ON;
GO
CREATE OR ALTER PROCEDURE dbo.sp_GetNonParishSubById
    @subscription_id INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT subscription_id, nonparish_cemetery_id, subscription_year,
           amount, payment_date, payment_status, remarks
    FROM dbo.nonparish_cemetery_subscription
    WHERE subscription_id = @subscription_id;
END
GO

-- ================================================================================================
-- PROCEDURE: sp_GetNonParishCemeteryList
-- PURPOSE: List all non-parish cemetery records with optional name filter (for subscription lookup)
-- ================================================================================================
SET QUOTED_IDENTIFIER ON;
GO
CREATE OR ALTER PROCEDURE dbo.sp_GetNonParishCemeteryList
    @name NVARCHAR(100) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SELECT CemeteryId, cemeterycode, Name, DeceasedDate, BuriedDate, ContactPerson, ContactPhone
    FROM dbo.nonparishcemetery
    WHERE (@name IS NULL OR Name LIKE '%' + @name + '%')
    ORDER BY CemeteryId DESC;
END
GO

------------------------------------------------------------------------------------------------------
-------- End of V4 Changes-----------------------------------------------------------------------------
------------------------------------------------------------------------------------------------------
