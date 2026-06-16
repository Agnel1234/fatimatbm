-- ================================================================================================
-- FATIMA CHURCH TBM - Master Installation Script (IDEMPOTENT)
-- ================================================================================================
--
-- IMPORTANT: This script is SAFE to run multiple times!
--
-- Handles both scenarios:
-- 1. NEW DATABASE: Creates everything from scratch
-- 2. EXISTING DATABASE: Skips existing objects, adds new ones, preserves all data
--
-- Order of execution:
--   1. Database creation (skipped if exists)
--   2. Schema creation
--   3. Table creation (idempotent - checks existence first)
--   4. Foreign keys
--   5. Indexes
--   6. Stored procedures (CREATE OR ALTER)
--   7. Initial data (only if not exists)
--
-- ================================================================================================

SET NOCOUNT ON;
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

-- ================================================================================================
-- STEP 1: CREATE DATABASE (if it doesn't exist)
-- ================================================================================================
IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'fatimachurchtbm')
BEGIN
    CREATE DATABASE [fatimachurchtbm]
    COLLATE SQL_Latin1_General_CP1_CI_AS;
    PRINT '✓ Database [fatimachurchtbm] created successfully.';
END
ELSE
BEGIN
    PRINT '✓ Database [fatimachurchtbm] already exists (reusing).';
END
GO

-- Switch to the database
USE [fatimachurchtbm];
GO

-- ================================================================================================
-- STEP 2: VERIFY SCHEMA (dbo schema always exists in database)
-- ================================================================================================
PRINT '✓ Using default [dbo] schema.';
GO

-- ================================================================================================
-- STEP 3: CREATE TABLES (Idempotent - checks for existence before creating)
-- ================================================================================================

-- TABLE: anbiyam
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'anbiyam' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    CREATE TABLE [dbo].[anbiyam] (
        [anbiyam_id] INT PRIMARY KEY IDENTITY(1,1),
        [anbiyam_name] NVARCHAR(50) NOT NULL,
        [anbiyam_code] NVARCHAR(10) NOT NULL,
        [created_at] DATETIME DEFAULT GETDATE(),
        [anbiyam_zone] INT NOT NULL,
        [anbiyam_coordinator_name] NVARCHAR(50) NOT NULL,
        [anbiyam_ass_coordinator_name] NVARCHAR(50) NULL,
        [coordinator_email] NVARCHAR(100) NULL,
        [coordinator_phone] NVARCHAR(15) NULL,
        [modified] DATE DEFAULT GETDATE()
    );
    PRINT '✓ Table [anbiyam] created.';
END
ELSE
BEGIN
    PRINT '✓ Table [anbiyam] already exists.';
END
GO

-- TABLE: family
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'family' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    CREATE TABLE [dbo].[family] (
        [family_id] INT PRIMARY KEY IDENTITY(1,1),
        [anbiyam_id] INT NOT NULL,
        [family_code] NVARCHAR(10) NOT NULL,
        [head_of_family] NVARCHAR(100) NOT NULL,
        [gender] NVARCHAR(10) NOT NULL,
        [family_permanant_address] NVARCHAR(200) NOT NULL,
        [family_temp_address] NVARCHAR(200) NOT NULL,
        [family_city] NVARCHAR(50) NOT NULL,
        [family_state] NVARCHAR(50) NOT NULL,
        [zip_code] NVARCHAR(10) NOT NULL,
        [phone] NVARCHAR(20) NULL,
        [email] NVARCHAR(100) NULL,
        [occupation] NVARCHAR(20) NULL,
        [dob] DATE NULL,
        [qualification] NVARCHAR(20) NULL,
        [blood_group] NVARCHAR(5) NULL,
        [marriage_date] DATE NULL,
        [monthly_subscription] INT NULL,
        [parish_member_since] INT NULL,
        [created_at] DATETIME DEFAULT GETDATE(),
        [modified] DATE DEFAULT GETDATE(),
        [isactive] BIT DEFAULT 1,
        [multiple_familycards] INT NULL
    );
    PRINT '✓ Table [family] created.';
END
ELSE
BEGIN
    PRINT '✓ Table [family] already exists.';
END
GO

-- TABLE: family_member
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'family_member' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    CREATE TABLE [dbo].[family_member] (
        [member_id] INT PRIMARY KEY IDENTITY(1,1),
        [family_id] INT NOT NULL,
        [first_name] NVARCHAR(50) NOT NULL,
        [last_name] NVARCHAR(50) NOT NULL,
        [relationship] NVARCHAR(30) NOT NULL,
        [gender] NVARCHAR(10) NOT NULL,
        [dob] DATE NULL,
        [member_status] NVARCHAR(20) NOT NULL DEFAULT 'Active',
        [occupation] NVARCHAR(50) NULL,
        [qualification] NVARCHAR(50) NULL,
        [blood_group] NVARCHAR(5) NULL,
        [email] NVARCHAR(100) NULL,
        [phone] NVARCHAR(20) NULL,
        [marital_status] NVARCHAR(20) NULL,
        [baptized_date] DATETIME NULL,
        [created_at] DATETIME DEFAULT GETDATE(),
        [modified] DATETIME DEFAULT GETDATE()
    );
    PRINT '✓ Table [family_member] created.';
END
ELSE
BEGIN
    PRINT '✓ Table [family_member] already exists.';
END
GO

-- TABLE: family_subscription
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'family_subscription' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    CREATE TABLE [dbo].[family_subscription] (
        [subscription_id] INT PRIMARY KEY IDENTITY(1,1),
        [family_id] INT NOT NULL,
        [subscription_month] TINYINT NOT NULL,
        [subscription_year] INT NOT NULL,
        [amount] INT NOT NULL,
        [payment_date] DATE NULL,
        [payment_status] NVARCHAR(20) NOT NULL DEFAULT 'Pending',
        [remarks] NVARCHAR(255) NULL,
        [created_at] DATETIME DEFAULT GETDATE(),
        [modified] DATETIME DEFAULT GETDATE()
    );
    PRINT '✓ Table [family_subscription] created.';
END
ELSE
BEGIN
    PRINT '✓ Table [family_subscription] already exists.';
END
GO

-- TABLE: cemetery_details
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'cemetery_details' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    CREATE TABLE [dbo].[cemetery_details] (
        [cemetery_id] INT PRIMARY KEY IDENTITY(1,1),
        [family_id] INT NOT NULL,
        [member_id] INT NULL,
        [deceased_name] NVARCHAR(100) NOT NULL,
        [date_of_birth] DATE NULL,
        [date_of_death] DATE NOT NULL,
        [burial_date] DATE NULL,
        [burial_place] NVARCHAR(200) NOT NULL,
        [grave_number] NVARCHAR(50) NULL,
        [remarks] NVARCHAR(255) NULL,
        [created_at] DATETIME DEFAULT GETDATE(),
        [modified] DATETIME DEFAULT GETDATE()
    );
    PRINT '✓ Table [cemetery_details] created.';
END
ELSE
BEGIN
    PRINT '✓ Table [cemetery_details] already exists.';
END
GO

-- TABLE: nonparishcemetery
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'nonparishcemetery' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    CREATE TABLE [dbo].[nonparishcemetery] (
        [CemeteryId] INT PRIMARY KEY IDENTITY(1,1),
        [cemeterycode] NVARCHAR(50) NOT NULL,
        [Name] NVARCHAR(100) NOT NULL,
        [DeceasedDate] DATE NULL,
        [BuriedDate] DATE NULL,
        [Remarks] NVARCHAR(255) NULL,
        [ContactPerson] NVARCHAR(100) NULL,
        [ContactPhone] NVARCHAR(20) NULL,
        [created_at] DATETIME DEFAULT GETDATE(),
        [modified] DATETIME DEFAULT GETDATE()
    );
    PRINT '✓ Table [nonparishcemetery] created.';
END
ELSE
BEGIN
    PRINT '✓ Table [nonparishcemetery] already exists.';
END
GO

-- ================================================================================================
-- STEP 4: ADD FOREIGN KEYS (Idempotent - checks before adding)
-- ================================================================================================

-- FK: family → anbiyam
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_family_anbiyam' AND parent_object_id = OBJECT_ID('family'))
BEGIN
    ALTER TABLE [dbo].[family]
    ADD CONSTRAINT [FK_family_anbiyam] FOREIGN KEY ([anbiyam_id])
    REFERENCES [dbo].[anbiyam]([anbiyam_id]);
    PRINT '✓ Foreign Key [FK_family_anbiyam] added.';
END
ELSE
BEGIN
    PRINT '✓ Foreign Key [FK_family_anbiyam] already exists.';
END
GO

-- FK: family_member → family
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_family_member_family' AND parent_object_id = OBJECT_ID('family_member'))
BEGIN
    ALTER TABLE [dbo].[family_member]
    ADD CONSTRAINT [FK_family_member_family] FOREIGN KEY ([family_id])
    REFERENCES [dbo].[family]([family_id]);
    PRINT '✓ Foreign Key [FK_family_member_family] added.';
END
ELSE
BEGIN
    PRINT '✓ Foreign Key [FK_family_member_family] already exists.';
END
GO

-- FK: family_subscription → family
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_family_subscription_family' AND parent_object_id = OBJECT_ID('family_subscription'))
BEGIN
    ALTER TABLE [dbo].[family_subscription]
    ADD CONSTRAINT [FK_family_subscription_family] FOREIGN KEY ([family_id])
    REFERENCES [dbo].[family]([family_id]);
    PRINT '✓ Foreign Key [FK_family_subscription_family] added.';
END
ELSE
BEGIN
    PRINT '✓ Foreign Key [FK_family_subscription_family] already exists.';
END
GO

-- FK: cemetery_details → family
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_cemetery_details_family' AND parent_object_id = OBJECT_ID('cemetery_details'))
BEGIN
    ALTER TABLE [dbo].[cemetery_details]
    ADD CONSTRAINT [FK_cemetery_details_family] FOREIGN KEY ([family_id])
    REFERENCES [dbo].[family]([family_id]);
    PRINT '✓ Foreign Key [FK_cemetery_details_family] added.';
END
ELSE
BEGIN
    PRINT '✓ Foreign Key [FK_cemetery_details_family] already exists.';
END
GO

-- FK: cemetery_details → family_member
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_cemetery_details_member' AND parent_object_id = OBJECT_ID('cemetery_details'))
BEGIN
    ALTER TABLE [dbo].[cemetery_details]
    ADD CONSTRAINT [FK_cemetery_details_member] FOREIGN KEY ([member_id])
    REFERENCES [dbo].[family_member]([member_id]);
    PRINT '✓ Foreign Key [FK_cemetery_details_member] added.';
END
ELSE
BEGIN
    PRINT '✓ Foreign Key [FK_cemetery_details_member] already exists.';
END
GO

-- ================================================================================================
-- STEP 5: CREATE INDEXES (Performance Optimization - Idempotent)
-- ================================================================================================

-- Performance-critical indexes
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_family_member_family_id_occupation')
BEGIN
    CREATE INDEX [IX_family_member_family_id_occupation]
    ON [dbo].[family_member]([family_id], [occupation])
    INCLUDE ([member_status]);
    PRINT '✓ Index [IX_family_member_family_id_occupation] created.';
END
ELSE
BEGIN
    PRINT '✓ Index [IX_family_member_family_id_occupation] already exists.';
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_family_member_family_id_status')
BEGIN
    CREATE INDEX [IX_family_member_family_id_status]
    ON [dbo].[family_member]([family_id], [member_status]);
    PRINT '✓ Index [IX_family_member_family_id_status] created.';
END
ELSE
BEGIN
    PRINT '✓ Index [IX_family_member_family_id_status] already exists.';
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_family_member_occupation')
BEGIN
    CREATE INDEX [IX_family_member_occupation]
    ON [dbo].[family_member]([occupation])
    WHERE [occupation] IS NOT NULL AND [occupation] <> '';
    PRINT '✓ Index [IX_family_member_occupation] created.';
END
ELSE
BEGIN
    PRINT '✓ Index [IX_family_member_occupation] already exists.';
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_cemetery_details_family_id')
BEGIN
    CREATE INDEX [IX_cemetery_details_family_id]
    ON [dbo].[cemetery_details]([family_id]);
    PRINT '✓ Index [IX_cemetery_details_family_id] created.';
END
ELSE
BEGIN
    PRINT '✓ Index [IX_cemetery_details_family_id] already exists.';
END
GO

PRINT '';
PRINT '✓ ================================================================================================';
PRINT '✓ Master Installation Script completed successfully!';
PRINT '✓ ================================================================================================';
PRINT '✓ Database: fatimachurchtbm';
PRINT '✓ All tables, foreign keys, and indexes created (or verified existing).';
PRINT '✓ Safe to run again - existing objects are preserved!';
PRINT '✓ ================================================================================================';
