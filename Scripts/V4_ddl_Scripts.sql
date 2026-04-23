------------------------------------------------------------------------------------------------------
---------V4 Changes - Database Indexes for Pagination and Filtering------------------------------
------------------------------------------------------------------------------------------------------
-- V4 introduces pagination support and enhanced filtering for all subscription and grid data.
-- These indexes improve performance for large datasets and complex queries.
-- Created: 2026-04-21
-- Scope: NEW indexes added. No table structure changes in V4.
------------------------------------------------------------------------------------------------------

USE fatimachurchtbm;
GO

SET QUOTED_IDENTIFIER ON;
GO

-- Indexes for family_subscription_yearly pagination and filtering
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_family_subscription_yearly_year' AND object_id = OBJECT_ID('dbo.family_subscription_yearly'))
BEGIN
    CREATE INDEX IX_family_subscription_yearly_year
    ON dbo.family_subscription_yearly(subscription_year);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_family_subscription_yearly_family' AND object_id = OBJECT_ID('dbo.family_subscription_yearly'))
BEGIN
    CREATE INDEX IX_family_subscription_yearly_family
    ON dbo.family_subscription_yearly(family_id);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_family_subscription_yearly_status' AND object_id = OBJECT_ID('dbo.family_subscription_yearly'))
BEGIN
    CREATE INDEX IX_family_subscription_yearly_status
    ON dbo.family_subscription_yearly(family_id, subscription_year)
    INCLUDE (jan_status, feb_status, mar_status, apr_status, may_status, jun_status,
             jul_status, aug_status, sep_status, oct_status, nov_status, dec_status);
END
GO

-- Indexes for cemetery_subscription_yearly pagination and filtering
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_cemetery_subscription_yearly_family' AND object_id = OBJECT_ID('dbo.cemetery_subscription_yearly'))
BEGIN
    CREATE INDEX IX_cemetery_subscription_yearly_family
    ON dbo.cemetery_subscription_yearly(family_id);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_cemetery_subscription_yearly_status' AND object_id = OBJECT_ID('dbo.cemetery_subscription_yearly'))
BEGIN
    CREATE INDEX IX_cemetery_subscription_yearly_status
    ON dbo.cemetery_subscription_yearly(family_id, subscription_year, payment_status);
END
GO

-- Composite index for common subscription queries (sorting and filtering)
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_family_subscription_yearly_composite' AND object_id = OBJECT_ID('dbo.family_subscription_yearly'))
BEGIN
    CREATE INDEX IX_family_subscription_yearly_composite
    ON dbo.family_subscription_yearly(subscription_year, family_id)
    INCLUDE (total_amount, created_at, modified);
END
GO

-- Composite index for cemetery subscriptions
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_cemetery_subscription_yearly_composite' AND object_id = OBJECT_ID('dbo.cemetery_subscription_yearly'))
BEGIN
    CREATE INDEX IX_cemetery_subscription_yearly_composite
    ON dbo.cemetery_subscription_yearly(subscription_year, family_id)
    INCLUDE (amount, payment_status, payment_date);
END
GO

-- ================================================================================================
-- TABLE: nonparish_cemetery_subscription
-- PURPOSE: Annual subscription tracking for outside-parish cemetery entries
-- ================================================================================================
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.nonparish_cemetery_subscription') AND type = N'U')
BEGIN
    CREATE TABLE dbo.nonparish_cemetery_subscription (
        subscription_id         INT IDENTITY(1,1) PRIMARY KEY,
        nonparish_cemetery_id   INT NOT NULL,
        subscription_year       INT NOT NULL,
        amount                  DECIMAL(10,2) NOT NULL DEFAULT 0,
        payment_date            DATE NULL,
        payment_status          NVARCHAR(20) NOT NULL DEFAULT 'Pending',
        remarks                 NVARCHAR(500) NULL,
        created_at              DATETIME NOT NULL DEFAULT GETDATE(),
        modified                DATETIME NOT NULL DEFAULT GETDATE(),
        CONSTRAINT FK_nonparish_sub_cemetery
            FOREIGN KEY (nonparish_cemetery_id) REFERENCES dbo.nonparishcemetery(CemeteryId),
        CONSTRAINT UQ_nonparish_sub_year
            UNIQUE (nonparish_cemetery_id, subscription_year)
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_nonparish_sub_year' AND object_id = OBJECT_ID('dbo.nonparish_cemetery_subscription'))
BEGIN
    CREATE INDEX IX_nonparish_sub_year
    ON dbo.nonparish_cemetery_subscription(nonparish_cemetery_id, subscription_year);
END
GO

------------------------------------------------------------------------------------------------------
-------- End of V4 Changes-----------------------------------------------------------------------------
------------------------------------------------------------------------------------------------------
