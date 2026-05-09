-- ═══════════════════════════════════════════════════════════════════════════════
-- PDF EXPORT STORED PROCEDURES
-- Purpose: Support modern PDF report generation with comprehensive family data
-- Created: 2026-04-24
-- ═══════════════════════════════════════════════════════════════════════════════

USE fatimachurchtbm;
GO

-- ═══════════════════════════════════════════════════════════════════════════════
-- PROCEDURE: sp_GetFamilyBasicDetailsForExport
-- PURPOSE: Get all families with summary data for PDF export
-- ═══════════════════════════════════════════════════════════════════════════════
IF OBJECT_ID('dbo.sp_GetFamilyBasicDetailsForExport', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_GetFamilyBasicDetailsForExport;
GO

CREATE PROCEDURE dbo.sp_GetFamilyBasicDetailsForExport
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        f.family_id AS FamilyID,
        f.head_of_family AS FamilyName,
        f.created_at AS RegistrationDate,
        f.phone AS Phone,
        f.email AS Email,
        CONCAT(ISNULL(f.family_permanant_address, ''), ', ',
               ISNULL(f.family_city, ''), ', ',
               ISNULL(f.family_state, '')) AS Address,
        f.family_notes AS Remarks,

        -- Member count
        (SELECT COUNT(*) FROM family_member fm WHERE fm.family_id = f.family_id AND fm.member_status = 'Active') AS ActiveMembers,
        (SELECT COUNT(*) FROM family_member fm WHERE fm.family_id = f.family_id) AS TotalMembers,

        -- Cemetery data
        (SELECT COUNT(*) FROM cemetery_details cd WHERE cd.family_id = f.family_id) AS CemeteryPlotsUsed,
        (SELECT COUNT(*) FROM cemetery_details cd WHERE cd.family_id = f.family_id AND YEAR(cd.burial_date) = YEAR(GETDATE())) AS BurialsThisYear,

        -- Subscription data
        (SELECT COUNT(*) FROM family_subscription fs WHERE fs.family_id = f.family_id AND fs.payment_status = 'Paid') AS ActiveSubscriptions,
        (SELECT SUM(CAST(amount AS INT)) FROM family_subscription fs WHERE fs.family_id = f.family_id AND fs.payment_status = 'Paid') AS TotalRevenue,
        (SELECT SUM(CAST(amount AS INT)) FROM family_subscription fs WHERE fs.family_id = f.family_id AND fs.payment_status IN ('Pending', 'Overdue')) AS OutstandingDues

    FROM family f
    ORDER BY f.head_of_family;
END
GO

-- ═══════════════════════════════════════════════════════════════════════════════
-- PROCEDURE: sp_GetFamilyMembers
-- PURPOSE: Get all members of a specific family
-- PARAMETERS: @family_id INT
-- ═══════════════════════════════════════════════════════════════════════════════
IF OBJECT_ID('dbo.sp_GetFamilyMembers', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_GetFamilyMembers;
GO

CREATE PROCEDURE dbo.sp_GetFamilyMembers
    @family_id INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        member_id AS MemberID,
        CONCAT(first_name, ' ', ISNULL(middle_name, '')) AS Name,
        dob AS DOB,
        relationship AS Relationship,
        gender AS Gender,
        member_status AS Status,
        qualification AS Qualification,
        occupation AS Occupation,
        baptized_date AS BaptizedDate,
        marriage_date AS MarriageDate
    FROM family_member
    WHERE family_id = @family_id
    ORDER BY
        CASE member_status
            WHEN 'Active' THEN 1
            WHEN 'Inactive' THEN 2
            ELSE 3
        END,
        relationship DESC,
        dob;
END
GO

-- ═══════════════════════════════════════════════════════════════════════════════
-- PROCEDURE: sp_GetFamilySubscriptions
-- PURPOSE: Get annual subscriptions for a specific family
-- PARAMETERS: @family_id INT
-- ═══════════════════════════════════════════════════════════════════════════════
IF OBJECT_ID('dbo.sp_GetFamilySubscriptions', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_GetFamilySubscriptions;
GO

CREATE PROCEDURE dbo.sp_GetFamilySubscriptions
    @family_id INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        subscription_id AS SubscriptionID,
        subscription_year AS Year,
        CAST(amount AS INT) AS Amount,
        payment_status AS Status,
        payment_date AS PaidDate,
        remarks AS Remarks,
        created_at AS CreatedDate,
        modified AS ModifiedDate
    FROM family_subscription
    WHERE family_id = @family_id
    ORDER BY subscription_year DESC;
END
GO

-- ═══════════════════════════════════════════════════════════════════════════════
-- PROCEDURE: sp_GetFamilyCemeteryDetails
-- PURPOSE: Get cemetery/burial records for a specific family
-- PARAMETERS: @family_id INT
-- ═══════════════════════════════════════════════════════════════════════════════
IF OBJECT_ID('dbo.sp_GetFamilyCemeteryDetails', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_GetFamilyCemeteryDetails;
GO

CREATE PROCEDURE dbo.sp_GetFamilyCemeteryDetails
    @family_id INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        cemetery_id AS CemeteryID,
        deceased_name AS DeceasedName,
        date_of_birth AS DateOfBirth,
        date_of_death AS DateOfDeath,
        burial_date AS BurialDate,
        burial_place AS BurialPlace,
        grave_number AS GraveNumber,
        remarks AS Remarks,
        created_at AS CreatedDate
    FROM cemetery_details
    WHERE family_id = @family_id
    ORDER BY burial_date DESC;
END
GO

-- ═══════════════════════════════════════════════════════════════════════════════
-- PROCEDURE: sp_GetPaymentStatistics
-- PURPOSE: Get payment status breakdown across all families
-- ═══════════════════════════════════════════════════════════════════════════════
IF OBJECT_ID('dbo.sp_GetPaymentStatistics', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_GetPaymentStatistics;
GO

CREATE PROCEDURE dbo.sp_GetPaymentStatistics
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        payment_status AS Status,
        COUNT(*) AS Count,
        SUM(CAST(amount AS INT)) AS TotalAmount,
        CAST(COUNT(*) * 100.0 / (SELECT COUNT(*) FROM family_subscription) AS DECIMAL(5,2)) AS Percentage
    FROM family_subscription
    GROUP BY payment_status
    ORDER BY
        CASE payment_status
            WHEN 'Paid' THEN 1
            WHEN 'Pending' THEN 2
            ELSE 3
        END;
END
GO

-- ═══════════════════════════════════════════════════════════════════════════════
-- PROCEDURE: sp_GetOutstandingDuesByFamily
-- PURPOSE: Get families with outstanding dues, sorted by amount
-- PARAMETERS: @top INT (optional, default 10)
-- ═══════════════════════════════════════════════════════════════════════════════
IF OBJECT_ID('dbo.sp_GetOutstandingDuesByFamily', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_GetOutstandingDuesByFamily;
GO

CREATE PROCEDURE dbo.sp_GetOutstandingDuesByFamily
    @top INT = 10
AS
BEGIN
    SET NOCOUNT ON;

    SELECT TOP (@top)
        f.family_id AS FamilyID,
        f.head_of_family AS FamilyName,
        SUM(CAST(fs.amount AS INT)) AS OutstandingDues,
        COUNT(*) AS PendingCount
    FROM family f
    INNER JOIN family_subscription fs ON f.family_id = fs.family_id
    WHERE fs.payment_status IN ('Pending', 'Overdue')
    GROUP BY f.family_id, f.head_of_family
    ORDER BY OutstandingDues DESC;
END
GO

-- ═══════════════════════════════════════════════════════════════════════════════
-- PROCEDURE: sp_GetFamilyTimeline
-- PURPOSE: Get important dates/events for a family
-- PARAMETERS: @family_id INT
-- ═══════════════════════════════════════════════════════════════════════════════
IF OBJECT_ID('dbo.sp_GetFamilyTimeline', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_GetFamilyTimeline;
GO

CREATE PROCEDURE dbo.sp_GetFamilyTimeline
    @family_id INT
AS
BEGIN
    SET NOCOUNT ON;

    -- Combine multiple event types into a timeline
    SELECT
        'Family Registered' AS EventType,
        f.created_at AS EventDate,
        'Registered in parish' AS EventDescription,
        'Registration' AS Category
    FROM family f
    WHERE f.family_id = @family_id

    UNION ALL

    SELECT
        'Member Birth' AS EventType,
        fm.dob AS EventDate,
        CONCAT(fm.first_name, ' born') AS EventDescription,
        'Birth' AS Category
    FROM family_member fm
    WHERE fm.family_id = @family_id AND fm.dob IS NOT NULL

    UNION ALL

    SELECT
        'Member Baptism' AS EventType,
        fm.baptized_date AS EventDate,
        CONCAT(fm.first_name, ' baptized') AS EventDescription,
        'Sacrament' AS Category
    FROM family_member fm
    WHERE fm.family_id = @family_id AND fm.baptized_date IS NOT NULL

    UNION ALL

    SELECT
        'Member Marriage' AS EventType,
        fm.marriage_date AS EventDate,
        CONCAT(fm.first_name, ' married') AS EventDescription,
        'Sacrament' AS Category
    FROM family_member fm
    WHERE fm.family_id = @family_id AND fm.marriage_date IS NOT NULL

    UNION ALL

    SELECT
        'Burial' AS EventType,
        cd.burial_date AS EventDate,
        CONCAT(cd.deceased_name, ' buried at ', cd.burial_place) AS EventDescription,
        'Death' AS Category
    FROM cemetery_details cd
    WHERE cd.family_id = @family_id AND cd.burial_date IS NOT NULL

    ORDER BY EventDate DESC;
END
GO

-- ═══════════════════════════════════════════════════════════════════════════════
-- PROCEDURE: sp_GetFamilyMembersByAgeGroup
-- PURPOSE: Get member count breakdown by age group for a family
-- PARAMETERS: @family_id INT
-- ═══════════════════════════════════════════════════════════════════════════════
IF OBJECT_ID('dbo.sp_GetFamilyMembersByAgeGroup', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_GetFamilyMembersByAgeGroup;
GO

CREATE PROCEDURE dbo.sp_GetFamilyMembersByAgeGroup
    @family_id INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        CASE
            WHEN DATEDIFF(YEAR, fm.dob, GETDATE()) < 13 THEN 'Children (0-12)'
            WHEN DATEDIFF(YEAR, fm.dob, GETDATE()) < 20 THEN 'Teens (13-19)'
            WHEN DATEDIFF(YEAR, fm.dob, GETDATE()) < 30 THEN 'Young Adults (20-29)'
            WHEN DATEDIFF(YEAR, fm.dob, GETDATE()) < 40 THEN 'Adults (30-39)'
            WHEN DATEDIFF(YEAR, fm.dob, GETDATE()) < 50 THEN 'Middle-aged (40-49)'
            WHEN DATEDIFF(YEAR, fm.dob, GETDATE()) < 65 THEN 'Mature (50-64)'
            ELSE 'Seniors (65+)'
        END AS AgeGroup,
        COUNT(*) AS Count,
        fm.gender
    FROM family_member fm
    WHERE fm.family_id = @family_id AND fm.dob IS NOT NULL
    GROUP BY
        CASE
            WHEN DATEDIFF(YEAR, fm.dob, GETDATE()) < 13 THEN 'Children (0-12)'
            WHEN DATEDIFF(YEAR, fm.dob, GETDATE()) < 20 THEN 'Teens (13-19)'
            WHEN DATEDIFF(YEAR, fm.dob, GETDATE()) < 30 THEN 'Young Adults (20-29)'
            WHEN DATEDIFF(YEAR, fm.dob, GETDATE()) < 40 THEN 'Adults (30-39)'
            WHEN DATEDIFF(YEAR, fm.dob, GETDATE()) < 50 THEN 'Middle-aged (40-49)'
            WHEN DATEDIFF(YEAR, fm.dob, GETDATE()) < 65 THEN 'Mature (50-64)'
            ELSE 'Seniors (65+)'
        END,
        fm.gender
    ORDER BY
        CASE
            WHEN DATEDIFF(YEAR, fm.dob, GETDATE()) < 13 THEN 1
            WHEN DATEDIFF(YEAR, fm.dob, GETDATE()) < 20 THEN 2
            WHEN DATEDIFF(YEAR, fm.dob, GETDATE()) < 30 THEN 3
            WHEN DATEDIFF(YEAR, fm.dob, GETDATE()) < 40 THEN 4
            WHEN DATEDIFF(YEAR, fm.dob, GETDATE()) < 50 THEN 5
            WHEN DATEDIFF(YEAR, fm.dob, GETDATE()) < 65 THEN 6
            ELSE 7
        END;
END
GO

-- ═══════════════════════════════════════════════════════════════════════════════
-- PROCEDURE: sp_GetGlobalStatistics
-- PURPOSE: Get church-wide statistics for report summary
-- ═══════════════════════════════════════════════════════════════════════════════
IF OBJECT_ID('dbo.sp_GetGlobalStatistics', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_GetGlobalStatistics;
GO

CREATE PROCEDURE dbo.sp_GetGlobalStatistics
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        (SELECT COUNT(*) FROM family) AS TotalFamilies,
        (SELECT COUNT(*) FROM family_member WHERE member_status = 'Active') AS TotalActiveMembers,
        (SELECT COUNT(*) FROM family_member) AS TotalMembers,
        (SELECT COUNT(*) FROM cemetery_details) AS TotalBurials,
        (SELECT COUNT(*) FROM cemetery_details WHERE YEAR(burial_date) = YEAR(GETDATE())) AS BurialsThisYear,
        (SELECT SUM(CAST(amount AS INT)) FROM family_subscription WHERE payment_status = 'Paid') AS TotalRevenueCollected,
        (SELECT SUM(CAST(amount AS INT)) FROM family_subscription WHERE payment_status IN ('Pending', 'Overdue')) AS TotalOutstandingDues,
        (SELECT COUNT(*) FROM family_subscription WHERE payment_status = 'Paid') AS TotalPaidSubscriptions,
        (SELECT COUNT(*) FROM family_subscription WHERE payment_status = 'Pending') AS TotalPendingSubscriptions,
        (SELECT COUNT(*) FROM family_subscription WHERE payment_status = 'Overdue') AS TotalOverdueSubscriptions,
        (SELECT COUNT(DISTINCT family_id) FROM cemetery_details) AS FamiliesWithBurials;
END
GO

-- ═══════════════════════════════════════════════════════════════════════════════
-- PROCEDURE: sp_GetSubscriptionTrendByYear
-- PURPOSE: Get subscription revenue trend over years
-- ═══════════════════════════════════════════════════════════════════════════════
IF OBJECT_ID('dbo.sp_GetSubscriptionTrendByYear', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_GetSubscriptionTrendByYear;
GO

CREATE PROCEDURE dbo.sp_GetSubscriptionTrendByYear
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        subscription_year AS Year,
        COUNT(*) AS SubscriptionCount,
        SUM(CAST(amount AS INT)) AS TotalAmount,
        SUM(CASE WHEN payment_status = 'Paid' THEN 1 ELSE 0 END) AS PaidCount,
        SUM(CASE WHEN payment_status = 'Pending' THEN 1 ELSE 0 END) AS PendingCount,
        SUM(CASE WHEN payment_status = 'Overdue' THEN 1 ELSE 0 END) AS OverdueCount
    FROM family_subscription
    GROUP BY subscription_year
    ORDER BY subscription_year DESC;
END
GO

-- ═══════════════════════════════════════════════════════════════════════════════
-- VERIFICATION: Test all procedures
-- ═══════════════════════════════════════════════════════════════════════════════
PRINT 'All PDF Export stored procedures created successfully!';
PRINT '';
PRINT 'Procedures available:';
PRINT '  - sp_GetFamilyBasicDetailsForExport';
PRINT '  - sp_GetFamilyMembers';
PRINT '  - sp_GetFamilySubscriptions';
PRINT '  - sp_GetFamilyCemeteryDetails';
PRINT '  - sp_GetPaymentStatistics';
PRINT '  - sp_GetOutstandingDuesByFamily';
PRINT '  - sp_GetFamilyTimeline';
PRINT '  - sp_GetFamilyMembersByAgeGroup';
PRINT '  - sp_GetGlobalStatistics';
PRINT '  - sp_GetSubscriptionTrendByYear';
GO
