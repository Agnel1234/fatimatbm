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
	coordinator_email NVARCHAR(100) UNIQUE NULL,
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

--CREATE TABLE church_events (
--    event_id INT PRIMARY KEY IDENTITY(1,1),
--    event_name NVARCHAR(100) NOT NULL,
--    event_date DATE NOT NULL,
--    event_time TIME NULL,
--    event_location NVARCHAR(200) NOT NULL,
--    description NVARCHAR(500) NULL,
--    created_at DATETIME DEFAULT GETDATE(),
--    modified DATETIME DEFAULT GETDATE()
--);
