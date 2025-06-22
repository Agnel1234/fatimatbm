-- =========================
-- anbiyam procedures
-- =========================
DROP PROCEDURE sp_GetAllAnbiyam;
GO

CREATE PROCEDURE sp_GetAllAnbiyam
AS
BEGIN
    SELECT concat(a.anbiyam_code, '-', a.anbiyam_name), a.anbiyam_id FROM anbiyam a;
END
GO

CREATE PROCEDURE sp_GetAnbiyamById
    @anbiyam_id INT
AS
BEGIN
    SELECT * FROM anbiyam WHERE anbiyam_id = @anbiyam_id;
END
GO

-- =========================
-- family procedures
-- =========================

CREATE PROCEDURE sp_GetAllFamilies
AS
BEGIN
    SELECT * FROM family;
END
GO

CREATE PROCEDURE sp_GetFamilyById
    @family_id INT
AS
BEGIN
    SELECT * FROM family WHERE family_id = @family_id;
END
GO

CREATE PROCEDURE sp_GetFamiliesByAnbiyam
    @anbiyam_id INT
AS
BEGIN
    SELECT * FROM family WHERE anbiyam_id = @anbiyam_id;
END
GO

-- =========================
-- family_member procedures
-- =========================

CREATE PROCEDURE sp_GetFamilyMembersByFamilyId
    @family_id INT
AS
BEGIN
    SELECT * FROM family_member WHERE family_id = @family_id;
END
GO

CREATE PROCEDURE sp_GetFamilyMemberById
    @member_id INT
AS
BEGIN
    SELECT * FROM family_member WHERE member_id = @member_id;
END
GO

-- =========================
-- family_subscription procedures
-- =========================

CREATE PROCEDURE sp_GetSubscriptionsByFamilyId
    @family_id INT
AS
BEGIN
    SELECT * FROM family_subscription WHERE family_id = @family_id;
END
GO

CREATE PROCEDURE sp_GetSubscriptionById
    @subscription_id INT
AS
BEGIN
    SELECT * FROM family_subscription WHERE subscription_id = @subscription_id;
END
GO

-- =========================
-- cemetery_details procedures
-- =========================

CREATE PROCEDURE sp_GetCemeteryDetailsByFamilyId
    @family_id INT
AS
BEGIN
    SELECT * FROM cemetery_details WHERE family_id = @family_id;
END
GO

CREATE PROCEDURE sp_GetCemeteryDetailsByMemberId
    @member_id INT
AS
BEGIN
    SELECT * FROM cemetery_details WHERE member_id = @member_id;
END
GO

CREATE PROCEDURE sp_GetFamilyWithAnbiyam
AS
BEGIN
    SELECT
        a.anbiyam_name As Anbiyam,
        a.anbiyam_code As Code,
        a.anbiyam_zone As Zone,
        a.anbiyam_coordinator_name As Coordinator,
        f.head_of_family As FamilyHead,
        f.family_code As FamilyCode,
        f.gender As Gender,
        f.phone As Mobile,
        f.parish_member_since AS Member_since
    FROM
        family f
        INNER JOIN anbiyam a ON f.anbiyam_id = a.anbiyam_id;
END
GO

CREATE PROCEDURE sp_SearchFamilyWithAnbiyam
    @anbiyam_id INT = NULL,
    @coordinator_name NVARCHAR(50) = NULL,
    @head_of_family NVARCHAR(100) = NULL
AS
BEGIN
    SELECT
        a.anbiyam_name AS Anbiyam,
        a.anbiyam_code AS Code,
        a.anbiyam_zone AS Zone,
        a.anbiyam_coordinator_name AS Coordinator,
        f.head_of_family AS FamilyHead,
        f.family_code AS FamilyCode,
        f.gender AS Gender,
        f.phone AS Mobile,
        f.parish_member_since AS Member_since
    FROM
        family f
        INNER JOIN anbiyam a ON f.anbiyam_id = a.anbiyam_id
    WHERE
        (@anbiyam_id IS NULL OR a.anbiyam_id = @anbiyam_id)
        AND (@coordinator_name IS NULL OR a.anbiyam_coordinator_name LIKE '%' + @coordinator_name + '%')
        AND (@head_of_family IS NULL OR f.head_of_family LIKE '%' + @head_of_family + '%');
END
GO

DROP PROCEDURE [dbo].[sp_GetAllAnbiyams];
GO

CREATE PROCEDURE sp_GetAllAnbiyams
AS
BEGIN
    SELECT
        a.anbiyam_id as anbiyamID,
        a.anbiyam_code as Code, 
        a.anbiyam_name as AnbiyamName, 
        a.created_at as Created, 
        a.anbiyam_zone as Zone, 
        a.anbiyam_coordinator_name as Coordinator, 
        a.coordinator_phone as Phone 
   FROM anbiyam a;
END
GO

CREATE PROCEDURE sp_GetFamilyCountByZone
AS
BEGIN
    SELECT 
        a.anbiyam_zone AS Zone,
        COUNT(f.family_id) AS FamilyCount
    FROM 
        anbiyam a
        LEFT JOIN family f ON a.anbiyam_id = f.anbiyam_id
    GROUP BY 
        a.anbiyam_zone
    ORDER BY 
        a.anbiyam_zone
END
GO


CREATE PROCEDURE sp_InsertAnbiyam
    @anbiyam_name NVARCHAR(50),
    @anbiyam_zone INT,
    @anbiyam_coordinator_name NVARCHAR(50),
    @anbiyam_ass_coordinator_name NVARCHAR(50),
    @coordinator_email NVARCHAR(100),
    @coordinator_phone NVARCHAR(15),
    @anbiyam_code NVARCHAR(10)
AS
BEGIN
    INSERT INTO anbiyam (
        anbiyam_name,
        anbiyam_zone,
        anbiyam_coordinator_name,
        anbiyam_ass_coordinator_name,
        coordinator_email,
        coordinator_phone,
        anbiyam_code,
        created_at,
        modified
    )
    VALUES (
        @anbiyam_name,
        @anbiyam_zone,
        @anbiyam_coordinator_name,
        @anbiyam_ass_coordinator_name,
        @coordinator_email,
        @coordinator_phone,
        @anbiyam_code,
        GETDATE(),
        GETDATE()
    );
END
GO

CREATE PROCEDURE sp_GetTotalAnbiyamsCount
AS
BEGIN
    SELECT count(a.anbiyam_name) FROM anbiyam a;
END
GO

CREATE PROCEDURE sp_GetSelectedAnbiyam
    @anbiyam_id INT
AS
BEGIN
SELECT 
    anbiyam_name,
    anbiyam_zone,
    anbiyam_coordinator_name,
    anbiyam_ass_coordinator_name,
    coordinator_phone,
    anbiyam_code,
    coordinator_email
FROM Anbiyam Where anbiyam_id = @anbiyam_id
END
GO

CREATE PROCEDURE sp_InsertOrUpdateAnbiyam
    @anbiyam_id INT = NULL,
    @anbiyam_name NVARCHAR(50),
    @anbiyam_zone INT,
    @anbiyam_coordinator_name NVARCHAR(50),
    @anbiyam_ass_coordinator_name NVARCHAR(50),
    @coordinator_email NVARCHAR(100),
    @coordinator_phone NVARCHAR(15),
    @anbiyam_code NVARCHAR(10)
AS
BEGIN
    SET NOCOUNT ON;

    IF @anbiyam_id IS NULL OR @anbiyam_id = 0
    BEGIN
        -- Insert new record
        INSERT INTO anbiyam (
            anbiyam_name,
            anbiyam_zone,
            anbiyam_coordinator_name,
            anbiyam_ass_coordinator_name,
            coordinator_email,
            coordinator_phone,
            anbiyam_code,
            created_at,
            modified
        )
        VALUES (
            @anbiyam_name,
            @anbiyam_zone,
            @anbiyam_coordinator_name,
            @anbiyam_ass_coordinator_name,
            @coordinator_email,
            @coordinator_phone,
            @anbiyam_code,
            GETDATE(),
            GETDATE()
        );
    END
    ELSE
    BEGIN
        -- Update existing record
        UPDATE anbiyam
        SET
            anbiyam_name = @anbiyam_name,
            anbiyam_zone = @anbiyam_zone,
            anbiyam_coordinator_name = @anbiyam_coordinator_name,
            anbiyam_ass_coordinator_name = @anbiyam_ass_coordinator_name,
            coordinator_email = @coordinator_email,
            coordinator_phone = @coordinator_phone,
            anbiyam_code = @anbiyam_code,
            modified = GETDATE()
        WHERE anbiyam_id = @anbiyam_id;
    END
END
GO

Create PROCEDURE sp_GetFamilyBasicDetails
AS
BEGIN
    SELECT 
        family_id as FamilyID,
        family_code as FamilyCode, 
        head_of_family as FamilyHead, 
        gender as Gender, 
        family_permanant_address as Address, 
        phone as Phone
    FROM family;
END
GO


ALTER PROCEDURE sp_GetFamilyMembersByFamilyId
    @family_id INT
AS
BEGIN
    SELECT 
        member_id as memberID,
        first_name as MemberName,
        relationship as Relationship,
        gender as Gender,
        dob as MemberDOB,
        member_status as Status,
        phone as Phone,
        marital_status as MaritalStatus
    FROM family_member
    WHERE family_id = @family_id;
END
GO

CREATE PROCEDURE sp_DeleteAnbiyam
    @anbiyamID INT
AS
BEGIN
    SET NOCOUNT ON;

    DELETE FROM Anbiyam WHERE anbiyam_id = @anbiyamID;
END
GO

ALTER PROCEDURE sp_DeleteAnbiyam
    @anbiyamID INT
AS
BEGIN
    SET NOCOUNT ON;

    -- Check if any families are linked to this anbiyam
    IF EXISTS (SELECT 1 FROM family WHERE anbiyam_id = @anbiyamID)
    BEGIN
        -- Optionally, you can raise an error or return a status code
        RAISERROR('Cannot delete: Families are linked to this Anbiyam.', 16, 1);
        RETURN;
    END

    DELETE FROM anbiyam
    WHERE anbiyam_id = @anbiyamID;
END


CREATE PROCEDURE sp_GetTotalFamilyCount
AS
BEGIN
    SELECT count(a.family_code) FROM family a;
END
GO

CREATE PROCEDURE sp_SaveFamily
    @family_code NVARCHAR(50),
	@anbiyam_id INT,
    @head_of_family NVARCHAR(100),
    @gender NVARCHAR(10),
    @family_permanant_address NVARCHAR(200),
	@family_temp_address NVARCHAR(200),
	@family_perm_city NVARCHAR(50),
	@family_perm_state NVARCHAR(50),
	@family_perm_zipcode NVARCHAR(10),
	@family_temp_city NVARCHAR(50),
	@family_temp_state NVARCHAR(50),
	@family_temp_zipcode NVARCHAR(10),
    @phone NVARCHAR(20),
	@email NVARCHAR(100),
	@monthly_subscription INT,
    @family_id INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO family (
        family_code,
		anbiyam_id,
        head_of_family,
        gender,
        family_permanant_address,
		family_temp_address,
		family_city,
		family_state,
		zip_code,
		family_temp_city,
		family_temp_state,
		family_temp_zipcode,
        phone,
		email,
        monthly_subscription,
        created_at,
        modified,
		parish_member_since
    )
    VALUES (
        @family_code,
		@anbiyam_id,
        @head_of_family,
        @gender,
        @family_permanant_address,
		@family_temp_address,
		@family_perm_city,
		@family_perm_state,
		@family_perm_zipcode,
		@family_temp_city,
		@family_temp_state,
		@family_temp_zipcode,
        @phone,
		@email,
		@monthly_subscription,
        GETDATE(),
        GETDATE(),
        YEAR(GETDATE())
    );

    SET @family_id = SCOPE_IDENTITY();
END
GO


CREATE PROCEDURE sp_SaveFamilyMember
    @family_id INT,
    @first_name NVARCHAR(100),
    @relationship NVARCHAR(50), -- e.g., Head, Spouse, Child, Other
    @gender NVARCHAR(10),
    @dob DATE = NULL,
    @member_status NVARCHAR(20) = NULL,
	@occupation NVARCHAR(20) = NULL,
	@qualification NVARCHAR(30) = NULL,
	@blood_group NVARCHAR(5) = NULL,
	@email NVARCHAR(100) = NULL,
    @phone NVARCHAR(20) = NULL,
	@baptized_date DATETIME = NULL,
	@marriage_date DATETIME = NULL,
	@first_communion_date DATETIME = NULL,
	@confirmation_date DATETIME = NULL,
	@priesthood_date DATETIME = NULL,
	@isadmin BIT,
	@legionofmary BIT,
	@isyouth BIT,
	@isalterservices BIT,
	@isvencentdepaul BIT,
	@ischoir BIT,
	@iscatechismteacher BIT,
	@iscatechismstudent BIT,
	@childclass NVARCHAR(10) = NULL,
	@child_institution NVARCHAR(100) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO family_member (
        family_id,
        first_name,
        relationship,
        gender,
        dob,
        member_status,
		occupation,
		qualification,
		blood_group,
		email,
        phone,
		baptized_date,
		created_at,
		modified,
		marriage_date,
		first_communion_date,
		first_confirmation_date,
		priesthood_date,
		is_admin_council,
		is_legion_of_mary,
		is_youth_group,
		is_alter_services,
		is_vencent_de_paul_soc,
		is_choir,
		is_catechism_student,
		is_catechism_teacher,
		child_class,
		child_institution
    )
    VALUES (
        @family_id,
        @first_name,
        @relationship,
        @gender,
        @dob,
        @member_status,
		@occupation,
		@qualification,
		@blood_group,
		@email,
        @phone,
		@baptized_date,
		GETDATE(),
		GETDATE(),
        @marriage_date,
        @first_communion_date,
        @confirmation_date,
		@priesthood_date,
		@isadmin,
		@legionofmary,
		@isyouth,
		@isalterservices,
		@isvencentdepaul,
		@ischoir,
		@iscatechismteacher,
		@iscatechismstudent,
		@childclass,
		@child_institution
    );
END
GO