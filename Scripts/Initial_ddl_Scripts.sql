CREATE DATABASE fatimachurchtbm;
GO

USE fatimachurchtbm;
GO

CREATE TABLE anbiyam (
	anbiyam_id INT PRIMARY KEY IDENTITY(1,1),
	anbiyam_name NVARCHAR(50) NOT NULL,
	anbiyam_code NVARCHAR(10) NOT NULL,
	created_at DATETIME DEFAULT GETDATE(),
	anbiyam_zone INT NOT NULL,
	anbiyam_coordinator_name NVARCHAR(50) NOT NULL,
	anbiyam_ass_coordinator_name NVARCHAR(50) NULL,
	coordinator_email NVARCHAR(100) NULL,
	coordinator_phone NVARCHAR(15),
	modified DATE DEFAULT GETDATE()
);
GO

CREATE TABLE family (
    family_id INT PRIMARY KEY IDENTITY(1,1),
	anbiyam_id INT NOT NULL,
	family_code NVARCHAR(10) NOT NULL,
    head_of_family NVARCHAR(100) NOT NULL,
	gender NVARCHAR(10) NOT NULL,
    family_permanant_address NVARCHAR(200) NOT NULL,
	family_temp_address NVARCHAR(200) NOT NULL,
    family_city NVARCHAR(50) NOT NULL,
    family_state NVARCHAR(50) NOT NULL,
    zip_code NVARCHAR(10) NOT NULL,
    phone NVARCHAR(20) NULL,
    email NVARCHAR(100) NULL,
	occupation NVARCHAR(20) NULL,
	dob DATE NULL,
	qualification NVARCHAR(20) NULL,
	blood_group NVARCHAR(5) NULL,
	marriage_date DATE NULL,
	monthly_subscription INT NULL,
	parish_member_since INT NULL,
    created_at DATETIME DEFAULT GETDATE(),
	modified DATE DEFAULT GETDATE(),
	FOREIGN KEY (anbiyam_id) REFERENCES anbiyam(anbiyam_id)
);	
GO	

CREATE TABLE family_member (
    member_id INT PRIMARY KEY IDENTITY(1,1),
    family_id INT NOT NULL,
    first_name NVARCHAR(50) NOT NULL,
    last_name NVARCHAR(50) NOT NULL,
    relationship NVARCHAR(30) NOT NULL,         -- e.g., Son, Daughter, Spouse, etc.
    gender NVARCHAR(10) NOT NULL,
    dob DATE NULL,
    member_status NVARCHAR(20) NOT NULL DEFAULT 'Active', -- e.g., Active, Inactive, Deceased
    occupation NVARCHAR(50) NULL,
    qualification NVARCHAR(50) NULL,
    blood_group NVARCHAR(5) NULL,
    email NVARCHAR(100) NULL,
    phone NVARCHAR(20) NULL,
	marital_status NVARCHAR(20) NULL,          -- e.g., Single, Married, Divorced, etc.
    baptized_date DATETIME NULL,
    created_at DATETIME DEFAULT GETDATE(),
    modified DATE DEFAULT GETDATE(),
    FOREIGN KEY (family_id) REFERENCES family(family_id)
);
GO

CREATE TABLE family_subscription (
    subscription_id INT PRIMARY KEY IDENTITY(1,1),
    family_id INT NOT NULL,
    subscription_month TINYINT NOT NULL,         -- 1=Jan, 2=Feb, ..., 12=Dec
    subscription_year INT NOT NULL,
    amount INT NOT NULL,
    payment_date DATE NULL,
    payment_status NVARCHAR(20) NOT NULL DEFAULT 'Pending', -- e.g., Paid, Pending, Overdue
    remarks NVARCHAR(255) NULL,
    created_at DATETIME DEFAULT GETDATE(),
    modified DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (family_id) REFERENCES family(family_id)
);
GO 

CREATE TABLE cemetery_details (
    cemetery_id INT PRIMARY KEY IDENTITY(1,1),
    family_id INT NOT NULL,
    member_id INT NULL, -- Optional: link to specific family member if available
    deceased_name NVARCHAR(100) NOT NULL,
    date_of_birth DATE NULL,
    date_of_death DATE NOT NULL,
    burial_date DATE NULL,
    burial_place NVARCHAR(200) NOT NULL,
    grave_number NVARCHAR(50) NULL,
    remarks NVARCHAR(255) NULL,
    created_at DATETIME DEFAULT GETDATE(),
    modified DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (family_id) REFERENCES family(family_id),
    FOREIGN KEY (member_id) REFERENCES family_member(member_id)
);
GO

ALTER TABLE family ADD family_temp_city NVARCHAR(50);
ALTER TABLE family ADD family_temp_state NVARCHAR(50);
ALTER TABLE family ADD family_temp_zipcode NVARCHAR(50);
ALTER TABLE family DROP COLUMN occupation;
ALTER TABLE family DROP COLUMN dob;
ALTER TABLE family DROP COLUMN qualification;
ALTER TABLE family DROP COLUMN blood_group;
ALTER TABLE family DROP COLUMN marriage_date;


ALTER TABLE family_member ADD marriage_date DATETIME NULL;
ALTER TABLE family_member ADD first_communion_date DATETIME NULL;
ALTER TABLE family_member ADD first_confirmation_date DATETIME NULL;
ALTER TABLE family_member ADD priesthood_date DATETIME NULL;
ALTER TABLE family_member ADD is_admin_council bit NOT NULL DEFAULT (0);
ALTER TABLE family_member ADD is_legion_of_mary bit NOT NULL DEFAULT (0);
ALTER TABLE family_member ADD is_youth_group bit NOT NULL DEFAULT (0);
ALTER TABLE family_member ADD is_alter_services bit NOT NULL DEFAULT (0);
ALTER TABLE family_member ADD is_vencent_de_paul_soc bit NOT NULL DEFAULT (0);
ALTER TABLE family_member ADD is_choir bit NOT NULL DEFAULT (0);
ALTER TABLE family_member ADD is_catechism_student bit NOT NULL DEFAULT (0);
ALTER TABLE family_member ADD is_catechism_teacher bit NOT NULL DEFAULT (0);
ALTER TABLE family_member ADD is_women_assoc bit NOT NULL DEFAULT (0);
ALTER TABLE family_member ADD is_litergy_council bit NOT NULL DEFAULT (0);
ALTER TABLE family_member ADD child_class NVARCHAR(50);
ALTER TABLE family_member ADD child_institution NVARCHAR(100);
ALTER TABLE family_member ADD member_group NVARCHAR(50);
ALTER TABLE family_member DROP COLUMN last_name;
ALTER TABLE family_member DROP COLUMN marital_status;


ALTER TABLE family ADD family_notes NVARCHAR(200);
ALTER TABLE family ADD multiple_familycards bit NOT NULL DEFAULT (0);
ALTER TABLE family ADD last_subscription_date DATETIME NULL;

-- Table creation
CREATE TABLE nonparishcemetery (
    CemeteryId INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(100) NOT NULL,
    DOB DATE NULL,
    Address NVARCHAR(200) NULL,
    City NVARCHAR(100) NULL,
    State NVARCHAR(50) NULL,
    ZipCode NVARCHAR(20) NULL,
    Remarks NVARCHAR(500) NULL,
    ContactPerson NVARCHAR(100) NULL,
    ContactPhone NVARCHAR(20) NULL,
    DeceasedDate DATE NULL,
    BuriedDate DATE NULL
);
GO

ALTER TABLE nonparishcemetery ADD gender nvarchar(6);
GO
ALTER TABLE nonparishcemetery ADD cemeterycode nvarchar(6);
GO

ALTER TABLE family ADD isactive BIT NOT NULL DEFAULT (1);
GO
ALTER TABLE family ADD disabled_date DATETIME NULL;
GO


------------------------------------------------------------------------------------------------------
---------V2 Changes----------------------------------------------------------------------------------
------------------------------------------------------------------------------------------------------

ALTER TABLE family ADD ishusbandactive BIT NOT NULL DEFAULT (1);
GO
ALTER TABLE family ADD iswifeactive BIT NOT NULL DEFAULT (1);
GO

IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[users]') AND type = N'U')
BEGIN
    CREATE TABLE dbo.[users] (
        userid INT IDENTITY(1,1) PRIMARY KEY,
        username NVARCHAR(100) NOT NULL CONSTRAINT UQ_users_username UNIQUE,
        password NVARCHAR(256) NOT NULL,           -- store hashes in production
        userrole NVARCHAR(50) NOT NULL DEFAULT('User'),
        created_at DATETIME NOT NULL DEFAULT(GETDATE())
    );
END
GO

-- Insert default users if they don't already exist.
-- Passwords are stored as SHA-256 hex strings. Replace plaintext passwords below.
IF NOT EXISTS (SELECT 1 FROM dbo.[users] WHERE username = 'Admin')
BEGIN
    DECLARE @pwd_admin NVARCHAR(400) = N'Admin@123'; -- change to your desired admin password
    DECLARE @hash_admin VARBINARY(8000) = HASHBYTES('SHA2_256', CONVERT(VARBINARY(MAX), @pwd_admin));
    INSERT INTO dbo.[users] (username, password, userrole)
    VALUES ('Admin', CONVERT(NVARCHAR(256), @hash_admin, 2), 'Admin');
END

IF NOT EXISTS (SELECT 1 FROM dbo.[users] WHERE username = 'Guest')
BEGIN
    DECLARE @pwd_guest NVARCHAR(400) = N'Guest@123'; -- change to your desired guest password
    DECLARE @hash_guest VARBINARY(8000) = HASHBYTES('SHA2_256', CONVERT(VARBINARY(MAX), @pwd_guest));
    INSERT INTO dbo.[users] (username, password, userrole)
    VALUES ('Guest', CONVERT(NVARCHAR(256), @hash_guest, 2), 'Guest');
END
GO


------------------------------------------------------------------------------------------------------
---------V3 Changes----------------------------------------------------------------------------------
------------------------------------------------------------------------------------------------------

ALTER TABLE [dbo].[family_subscription] DROP CONSTRAINT [FK__family_su__famil__36B12243]
GO

ALTER TABLE [dbo].[family_subscription] DROP CONSTRAINT [DF__family_su__modif__35BCFE0A]
GO

ALTER TABLE [dbo].[family_subscription] DROP CONSTRAINT [DF__family_su__creat__34C8D9D1]
GO

ALTER TABLE [dbo].[family_subscription] DROP CONSTRAINT [DF__family_su__payme__33D4B598]
GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[family_subscription]') AND type in (N'U'))
DROP TABLE [dbo].[family_subscription]
GO


CREATE TABLE dbo.family_subscription_yearly (
    family_subscription_year_id INT IDENTITY(1,1) PRIMARY KEY,
    family_id INT NOT NULL,
    subscription_year INT NOT NULL, -- e.g. 2025

    jan_amount DECIMAL(10,2) NULL, jan_paid_date DATE NULL, jan_status NVARCHAR(20) NOT NULL DEFAULT('Pending'),
    feb_amount DECIMAL(10,2) NULL, feb_paid_date DATE NULL, feb_status NVARCHAR(20) NOT NULL DEFAULT('Pending'),
    mar_amount DECIMAL(10,2) NULL, mar_paid_date DATE NULL, mar_status NVARCHAR(20) NOT NULL DEFAULT('Pending'),
    apr_amount DECIMAL(10,2) NULL, apr_paid_date DATE NULL, apr_status NVARCHAR(20) NOT NULL DEFAULT('Pending'),
    may_amount DECIMAL(10,2) NULL, may_paid_date DATE NULL, may_status NVARCHAR(20) NOT NULL DEFAULT('Pending'),
    jun_amount DECIMAL(10,2) NULL, jun_paid_date DATE NULL, jun_status NVARCHAR(20) NOT NULL DEFAULT('Pending'),
    jul_amount DECIMAL(10,2) NULL, jul_paid_date DATE NULL, jul_status NVARCHAR(20) NOT NULL DEFAULT('Pending'),
    aug_amount DECIMAL(10,2) NULL, aug_paid_date DATE NULL, aug_status NVARCHAR(20) NOT NULL DEFAULT('Pending'),
    sep_amount DECIMAL(10,2) NULL, sep_paid_date DATE NULL, sep_status NVARCHAR(20) NOT NULL DEFAULT('Pending'),
    oct_amount DECIMAL(10,2) NULL, oct_paid_date DATE NULL, oct_status NVARCHAR(20) NOT NULL DEFAULT('Pending'),
    nov_amount DECIMAL(10,2) NULL, nov_paid_date DATE NULL, nov_status NVARCHAR(20) NOT NULL DEFAULT('Pending'),
    dec_amount DECIMAL(10,2) NULL, dec_paid_date DATE NULL, dec_status NVARCHAR(20) NOT NULL DEFAULT('Pending'),

    total_amount AS (
        COALESCE(jan_amount,0)+COALESCE(feb_amount,0)+COALESCE(mar_amount,0)+COALESCE(apr_amount,0)+
        COALESCE(may_amount,0)+COALESCE(jun_amount,0)+COALESCE(jul_amount,0)+COALESCE(aug_amount,0)+
        COALESCE(sep_amount,0)+COALESCE(oct_amount,0)+COALESCE(nov_amount,0)+COALESCE(dec_amount,0)
    ) PERSISTED,

    created_at DATETIME DEFAULT GETDATE(),
    modified DATETIME DEFAULT GETDATE(),

    CONSTRAINT UQ_family_subscription_year UNIQUE (family_id, subscription_year),
    CONSTRAINT FK_family_subscription_year_family FOREIGN KEY (family_id) REFERENCES dbo.family(family_id)
);
GO


CREATE TABLE dbo.cemetery_subscription_yearly (
    cemetery_subscription_year_id INT IDENTITY(1,1) PRIMARY KEY,
    family_id INT NULL,
    subscription_year INT NOT NULL,           -- e.g. 2025
    amount DECIMAL(10,2) NOT NULL DEFAULT(0), -- yearly amount
    payment_date DATE NULL,                   -- last payment date for the year
    payment_status NVARCHAR(20) NOT NULL DEFAULT('Pending'), -- Paid, Pending, Overdue
    remarks NVARCHAR(255) NULL,
    created_at DATETIME NOT NULL DEFAULT(GETDATE()),
    modified DATETIME NOT NULL DEFAULT(GETDATE())
);
GO

-- Ensure a subscription is tied to a family OR a member (not both null)
ALTER TABLE dbo.cemetery_subscription_yearly
ADD CONSTRAINT CK_cemetery_subscription_year_requires_owner CHECK (family_id IS NOT NULL);
GO

-- Add persisted computed columns for safe uniqueness (NULL handling)
ALTER TABLE dbo.cemetery_subscription_yearly
ADD owner_family_id AS ISNULL(family_id, 0) PERSISTED
GO

-- Foreign keys (optional: choose ON DELETE behavior as required)
ALTER TABLE dbo.cemetery_subscription_yearly
ADD CONSTRAINT FK_cemetery_subscription_year_family FOREIGN KEY (family_id) REFERENCES dbo.family(family_id);
GO

-- Unique index to prevent duplicate (family/member, year) rows
CREATE UNIQUE INDEX UX_cemetery_subscription_year_owner_year
ON dbo.cemetery_subscription_yearly (owner_family_id, subscription_year);
GO

-- Helpful indexes
CREATE INDEX IX_cemetery_subscription_year_year ON dbo.cemetery_subscription_yearly(subscription_year);
CREATE INDEX IX_cemetery_subscription_year_family ON dbo.cemetery_subscription_yearly(family_id);
GO



------------------------------------------------------------------------------------------------------------
-------- End of V3 Changes-----------------------------------------------------------------------------------
-------------------------------------------------------------------------------------------------------------
