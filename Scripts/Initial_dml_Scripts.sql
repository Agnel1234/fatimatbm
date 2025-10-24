CREATE OR ALTER PROCEDURE sp_GetAllAnbiyam
AS
BEGIN
    SELECT concat(a.anbiyam_code, '-', a.anbiyam_name), a.anbiyam_id FROM anbiyam a;
END
GO

CREATE OR ALTER PROCEDURE sp_GetAnbiyamById
    @anbiyam_id INT
AS
BEGIN
    SELECT * FROM anbiyam WHERE anbiyam_id = @anbiyam_id;
END
GO

-- =========================
-- family procedures
-- =========================

CREATE OR ALTER PROCEDURE sp_GetAllFamilies
AS
BEGIN
    SELECT * FROM family;
END
GO

CREATE OR ALTER PROCEDURE sp_GetFamilyById
    @family_id INT
AS
BEGIN
    SELECT * FROM family WHERE family_id = @family_id;
END
GO

CREATE OR ALTER PROCEDURE sp_GetFamiliesByAnbiyam
    @anbiyam_id INT
AS
BEGIN
    SELECT * FROM family WHERE anbiyam_id = @anbiyam_id;
END
GO

-- =========================
-- family_member procedures
-- =========================

CREATE OR ALTER PROCEDURE sp_GetFamilyMemberById
    @member_id INT
AS
BEGIN
    SELECT * FROM family_member WHERE member_id = @member_id;
END
GO

-- =========================
-- family_subscription procedures
-- =========================

CREATE OR ALTER PROCEDURE sp_GetSubscriptionsByFamilyId
    @family_id INT
AS
BEGIN
    SELECT * FROM family_subscription WHERE family_id = @family_id;
END
GO

CREATE OR ALTER PROCEDURE sp_GetSubscriptionById
    @subscription_id INT
AS
BEGIN
    SELECT * FROM family_subscription WHERE subscription_id = @subscription_id;
END
GO

-- =========================
-- cemetery_details procedures
-- =========================

CREATE OR ALTER PROCEDURE sp_GetCemeteryDetailsByFamilyId
    @family_id INT
AS
BEGIN
    SELECT * FROM cemetery_details WHERE family_id = @family_id;
END
GO

CREATE OR ALTER PROCEDURE sp_GetCemeteryDetailsByMemberId
    @member_id INT
AS
BEGIN
    SELECT * FROM cemetery_details WHERE member_id = @member_id;
END
GO

CREATE OR ALTER PROCEDURE sp_GetFamilyWithAnbiyam
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

CREATE OR ALTER PROCEDURE sp_SearchFamilyWithAnbiyam
    @anbiyam_id INT = NULL,
    @coordinator_name NVARCHAR(50) = NULL
AS
BEGIN
	   SELECT 
		a.anbiyam_id,
		a.anbiyam_zone as [Zone],
		a.anbiyam_name as [Name],
		a.anbiyam_coordinator_name as [Coordinator],
		a.coordinator_phone as [Mobile],
		COUNT(DISTINCT f.family_id) AS [Families Count],
		COUNT(f.family_id) AS [Members count],
		COUNT(CASE WHEN fm.gender = 'Male' THEN 1 END) AS [Male],
		COUNT(CASE WHEN fm.gender = 'Female' THEN 1 END) AS [Female]
	FROM anbiyam a
		left JOIN family f  ON a.anbiyam_id = f.anbiyam_id
		LEFT JOIN family_member fm ON fm.family_id = f.family_id
	WHERE
		(@anbiyam_id IS NULL OR a.anbiyam_id = @anbiyam_id)
		AND (@coordinator_name IS NULL OR a.anbiyam_coordinator_name LIKE '%' + @coordinator_name + '%')
	GROUP BY 
		a.anbiyam_id,
		a.anbiyam_name,
		a.anbiyam_zone,
		a.anbiyam_coordinator_name,
		a.coordinator_phone 

END
GO


CREATE OR ALTER PROCEDURE sp_GetAllAnbiyams
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

CREATE OR ALTER PROCEDURE sp_GetFamilyCountByZone
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


CREATE OR ALTER PROCEDURE sp_InsertAnbiyam
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

CREATE OR ALTER PROCEDURE sp_GetTotalAnbiyamsCount
AS
BEGIN
    SELECT count(a.anbiyam_name) FROM anbiyam a;
END
GO

CREATE OR ALTER PROCEDURE sp_GetSelectedAnbiyam
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

CREATE OR ALTER PROCEDURE sp_InsertOrUpdateAnbiyam
    @anbiyam_id INT = 0,
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

    -- Prevent duplicate anbiyam_code for other records (and for inserts)
    IF EXISTS (
        SELECT 1 FROM anbiyam
        WHERE anbiyam_code = @anbiyam_code
          AND (@anbiyam_id IS NULL OR anbiyam_id <> @anbiyam_id)
    )
    BEGIN
        RAISERROR('Please Change the Anbiyam Name to avoid the duplicate anbiyam code: %s', 16, 1, @anbiyam_code);
        RETURN;
    END

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


CREATE OR ALTER PROCEDURE sp_DeleteAnbiyam
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
GO


CREATE OR ALTER PROCEDURE sp_GetTotalFamilyCount
    @anbiyam_id INT
AS
BEGIN
    SELECT count(a.family_code) FROM family a where a.anbiyam_id = @anbiyam_id;
END
GO

CREATE OR ALTER PROCEDURE sp_SaveFamily
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


CREATE OR ALTER PROCEDURE sp_SaveFamilyMember
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
    @isadmin BIT = 0,
    @legionofmary BIT = 0,
    @isyouth BIT = 0,
    @isalterservices BIT = 0,
    @isvencentdepaul BIT = 0,
    @iswomenassoc BIT = 0,
    @islitergycouncil BIT = 0,
    @ischoir BIT = 0,
    @iscatechismteacher BIT = 0,
    @iscatechismstudent BIT = 0,
	@childclass NVARCHAR(10) = NULL,
	@child_institution NVARCHAR(100) = NULL,
    @member_group NVARCHAR(50) = NULL -- e.g., Adult, Child, Senior, etc.

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
        is_women_assoc,
        is_litergy_council,
		child_class,
		child_institution,
        member_group
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
        @iswomenassoc,
        @islitergycouncil,
		@childclass,
		@child_institution,
        @member_group
    );
END
GO


CREATE OR ALTER PROCEDURE sp_GetFamilyMembersByFamilyId
    @family_id INT
AS
BEGIN
 SELECT 
    member_id as memberID,
    first_name as 'Name',
    relationship as Relation,
    gender as Gender,

	case
		when member_status = 'Deceased' then '-'
	else
		(    DATEDIFF(YEAR, dob, GETDATE()) 
        - CASE 
        WHEN DATEADD(YEAR, DATEDIFF(YEAR, dob, GETDATE()), dob) > GETDATE() 
        THEN 1 ELSE 0 
        END ) 
	end AS Age,

    member_status as Status,
    occupation as Occupation,
    CASE
        WHEN marriage_date IS NOT NULL THEN 'Married'
        ELSE  
			CASE 
              WHEN relationship IN ('Spouse', 'Head','Mother','Father','Father-In-Law','Mother-In-Law') THEN 'Married'
              ELSE 'Un-Married'
			END
    END AS [Married?],
    CASE
        WHEN is_admin_council = 1 THEN 'Yes'
        ELSE 'No'
    END AS [AdminCou..],
    CASE
        WHEN is_alter_services = 1 THEN 'Yes'
        ELSE 'No'
    END AS [AlterServ..],
    CASE
        WHEN is_legion_of_mary = 1 THEN 'Yes'
        ELSE 'No'
    END AS [Legion of Mary],
    CASE
        WHEN is_youth_group = 1 THEN 'Yes'
        ELSE 'No'
    END AS [Youth],
    CASE
        WHEN is_vencent_de_paul_soc = 1 THEN 'Yes'
        ELSE 'No'
    END AS [VincentDe..],
    CASE
        WHEN is_choir = 1 THEN 'Yes'
        ELSE 'No'
    END AS [Choir],
    CASE
        WHEN is_catechism_student = 1 THEN 'Yes'
        ELSE 'No'
    END AS [Catechism Student],
    CASE
        WHEN is_catechism_teacher = 1 THEN 'Yes'
        ELSE 'No'
    END AS [Catechism Teacher],
    CASE
        WHEN is_women_assoc = 1 THEN 'Yes'
        ELSE 'No'
    END AS [Women's Assoc]
FROM family_member
WHERE family_id = @family_id;
END
GO


CREATE OR ALTER PROCEDURE sp_GetAnbiyamCodeById
    @anbiyam_id INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT anbiyam_code
    FROM anbiyam
    WHERE anbiyam_id = @anbiyam_id;
END
GO


CREATE OR ALTER PROCEDURE sp_GetFamilyDetailsById
    @family_id INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT 
        family_code,
        anbiyam_id,
        Zone = (SELECT anbiyam_zone FROM anbiyam WHERE anbiyam_id = family.anbiyam_id),
        family_permanant_address,
        family_temp_address,
        family_city,
        family_state,
        zip_code,
        family_temp_city,
        family_temp_state,
        family_temp_zipcode,
        monthly_subscription
    FROM family
    WHERE family_id = @family_id;
END
GO


CREATE OR ALTER PROCEDURE sp_GetFamilyMembersDetailsByFamilyId
    @family_id INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT 
        first_name AS Name,
        relationship,
        dob,
        phone,
        email,
        blood_group,
        occupation,
        qualification,
        blood_group,
        marriage_date,
        baptized_date,
        first_communion_date,
        first_confirmation_date,
        priesthood_date,
        is_admin_council,
        is_legion_of_mary,
        is_youth_group,
        is_alter_services,
        is_vencent_de_paul_soc,
        is_choir,
        is_catechism_teacher,
        is_catechism_student,
        is_women_assoc,
        child_class,
        is_litergy_council
        child_institution,
        member_group
    FROM family_member
    WHERE family_id = @family_id;
END
GO

CREATE OR ALTER PROCEDURE sp_DeleteFamily
    @familyID INT
AS
BEGIN
    SET NOCOUNT ON;

	DELETE FROM family_subscription WHERE family_id = @familyID;
	DELETE FROM cemetery_details where family_id = @familyID;
	DELETE FROM family_member WHERE family_id = @familyID;

    -- Check if any families are linked to this anbiyam
    IF EXISTS (SELECT 1 FROM family_member WHERE family_id = @familyID)
    BEGIN
        -- Optionally, you can raise an error or return a status code
        RAISERROR('Cannot delete: Members are linked to this Family.', 16, 1);
        RETURN;
    END

    DELETE FROM family WHERE family_id = @familyID;
END
GO


CREATE OR ALTER PROCEDURE sp_GetFamilyCemetery
    @familyID INT
AS
BEGIN
	SELECT
		cemetery_id AS [cemeteryid],
		deceased_name AS [Deceased Name],
		date_of_death AS [Date of Death],
		burial_date AS [Burial Date],
		grave_number AS [Cemetery Code],
		remarks as [Remarks]
	FROM cemetery_details where family_id = @familyID;
END
GO


CREATE OR ALTER PROCEDURE sp_InsertOrUpdateCemetery
    @cemeteryID INT = NULL,
    @memberID INT = NULL,
    @burialDate DATE,
    @deathDate DATE,
    @cemeteryCode NVARCHAR(50),
    @remarks NVARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;

    IF @cemeteryID IS NULL OR @cemeteryID = 0
		BEGIN
			INSERT INTO cemetery_details 
				(
				family_id, 
				member_id, 
				deceased_name, 
				date_of_birth, 
				date_of_death, 
				burial_date, 
				burial_place,
				grave_number,
				remarks,
				created_at,
				modified
				)
			SELECT 
				family_id, 
				member_id, 
				first_name, 
				NULL,
				@deathDate,
				@burialDate,
				'Tambaram',
				@cemeteryCode,
				@remarks, 
				GETDATE(),
				GETDATE()
			FROM family_member
				WHERE member_id = @memberID;

			UPDATE family_member SET member_status = 'Deceased' WHERE member_id = @memberID;
			-- Insert new record
		END
    ELSE
		BEGIN
			-- Update existing record
			UPDATE cemetery_details
			SET
				date_of_death = @deathDate,
				burial_date = @burialDate,
				grave_number = @cemeteryCode,
				modified = GETDATE(),
				remarks = @remarks
			WHERE cemetery_id = @cemeteryID;

			UPDATE family_member SET member_status = 'Deceased' WHERE member_id = @memberID;
		END
END
GO


CREATE OR ALTER PROCEDURE sp_GetFamilyMembersForDropdown
    @family_id INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT 
        member_id,
        first_name AS membername
    FROM family_member
    WHERE family_id = @family_id and member_id not in 
	(
		SELECT member_id FROM cemetery_details where family_id = @family_id
	);
END
GO


CREATE OR ALTER PROCEDURE sp_GetAllCemeteries
AS
BEGIN
	SELECT
		cemetery_id AS [cemeteryid],
        grave_number AS [Cemetery Code],
		deceased_name AS [Deceased Name],
		date_of_death AS [Date of Death],
		burial_date AS [Burial Date],		
		remarks as [Remarks]
	FROM cemetery_details
END
GO


CREATE OR ALTER PROCEDURE sp_GetFamilyWithAnbiyam
AS
BEGIN
    SELECT 
        a.anbiyam_id,
        a.anbiyam_zone AS [Zone],
        a.anbiyam_name AS [Name],
        a.anbiyam_code AS [Code],
        a.anbiyam_coordinator_name AS [Coordinator],
        a.coordinator_phone AS [Mobile],
        COUNT(DISTINCT f.family_id) AS [Families],
        COUNT(CASE WHEN fm.member_id IS NOT NULL AND fm.member_status != 'Deceased' THEN fm.member_id END) AS [Members],
        COUNT(CASE WHEN fm.gender = 'Male' AND fm.member_status != 'Deceased' THEN 1 END) AS [Male],
        COUNT(CASE WHEN fm.gender = 'Female' AND fm.member_status != 'Deceased' THEN 1 END) AS [Female]
    FROM anbiyam a
    LEFT JOIN family f ON a.anbiyam_id = f.anbiyam_id
    LEFT JOIN family_member fm ON fm.family_id = f.family_id
    GROUP BY 
        a.anbiyam_id,
        a.anbiyam_name,
        a.anbiyam_zone,
        a.anbiyam_coordinator_name,
        a.coordinator_phone,
        a.anbiyam_code
END
GO


CREATE OR ALTER PROCEDURE sp_GetAgeGroupChartData
AS
BEGIN
    SELECT
        AgeGroupWithDesc,
	    AgeGroup,
        COUNT(*) AS MemberCount
    FROM (
        SELECT
            DATEDIFF(YEAR, dob, GETDATE()) 
                - CASE WHEN DATEADD(YEAR, DATEDIFF(YEAR, dob, GETDATE()), dob) > GETDATE() THEN 1 ELSE 0 END AS Age,
            CASE
                WHEN DATEDIFF(YEAR, dob, GETDATE()) < 13 THEN 'Little Sprouts'
                WHEN DATEDIFF(YEAR, dob, GETDATE()) BETWEEN 13 AND 18 THEN 'Rising Teens'
                WHEN DATEDIFF(YEAR, dob, GETDATE()) BETWEEN 19 AND 35 THEN 'Young Achivers'
                WHEN DATEDIFF(YEAR, dob, GETDATE()) BETWEEN 36 AND 60 THEN 'Prive Movers'
                WHEN DATEDIFF(YEAR, dob, GETDATE()) > 60 THEN 'Wisdom Circle'
                ELSE 'Unknown Age Group'
            END AS AgeGroup,
		            CASE
                WHEN DATEDIFF(YEAR, dob, GETDATE()) < 13 THEN 'Little Sprouts (0-12)'
                WHEN DATEDIFF(YEAR, dob, GETDATE()) BETWEEN 13 AND 18 THEN 'Rising Teens (13-19)'
                WHEN DATEDIFF(YEAR, dob, GETDATE()) BETWEEN 19 AND 35 THEN 'Young Achivers (20-35)'
                WHEN DATEDIFF(YEAR, dob, GETDATE()) BETWEEN 36 AND 60 THEN 'Prive Movers (36-59)'
                WHEN DATEDIFF(YEAR, dob, GETDATE()) > 60 THEN 'Wisdom Circle (60+)'
                ELSE 'Unknown Age Group'
            END AS AgeGroupWithDesc
        FROM family_member where member_status != 'Deceased'
    ) AS AgeData
    GROUP BY AgeGroup, AgeGroupWithDesc;
END
GO


CREATE OR ALTER PROCEDURE sp_GenderData
AS
BEGIN
    SELECT SUM(CASE WHEN gender = 'Male' THEN 1 ELSE 0 END) AS MaleCount, SUM(CASE WHEN gender = 'Female' THEN 1 ELSE 0 END) AS FemaleCount
    FROM family_member WHERE member_status != 'Deceased';
END
GO



CREATE OR ALTER PROCEDURE sp_GetFamilyBasicDetails
    @anbiyam_id INT = 0,
    @family_head NVARCHAR(100) = NULL,
    @occupation NVARCHAR(100) = NULL,
    @cemetery_available BIT = NULL
AS
BEGIN
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
        LEFT JOIN family_member fm ON fm.family_id = f.family_id
        LEFT JOIN cemetery_details cd ON cd.family_id = f.family_id
    WHERE fm.member_status = 'Active'
        AND (@anbiyam_id = 0 OR @anbiyam_id = a.anbiyam_id)
        AND (@family_head IS NULL OR f.head_of_family LIKE '%' + @family_head + '%')
        AND (@occupation IS NULL OR EXISTS (
            SELECT 1 FROM family_member fm2 
            WHERE fm2.family_id = f.family_id AND fm2.occupation LIKE '%' + @occupation + '%'
        ))
        AND (
            @cemetery_available IS NULL
            OR (@cemetery_available = 1 AND EXISTS (
                SELECT 1 FROM cemetery_details cd2 WHERE cd2.family_id = f.family_id
            ))
            OR (@cemetery_available = 0 AND NOT EXISTS (
                SELECT 1 FROM cemetery_details cd3 WHERE cd3.family_id = f.family_id
            ))
        )
    GROUP BY 
        f.family_id,
        a.anbiyam_name,
        f.family_code,
        f.head_of_family,
        f.phone,
        f.monthly_subscription,
        f.parish_member_since,
        f.multiple_familycards
END
GO


CREATE OR ALTER PROCEDURE sp_AggregateOccupations
AS
BEGIN
    SET NOCOUNT ON;
    SELECT 
        Distinct Upper(Occupation)
    FROM 
        dbo.family_member 	
    WHERE 
        Occupation IS NOT NULL AND Occupation <> ''
    GROUP BY 
        Occupation 
END

GO


CREATE OR ALTER PROCEDURE sp_GetAllCemeteries
    @burial_date_from datetime = null,
    @burial_date_to datetime = null,
    @deceased_date_from datetime = null,
    @deceased_date_to datetime = null,
    @IsOurparish BIT = null
AS
BEGIN
    IF @IsOurparish = 0
		BEGIN
			SELECT
				CemeteryId AS [cemeteryid],
				cemeterycode AS [Cemetery Code],
				Name AS [Deceased Name],
				DeceasedDate AS [Deceased Date],
				BuriedDate AS [Burial Date],
				Remarks AS [Remarks],
                ContactPerson AS [Contact Person],
                ContactPhone AS [Contact Mobile]
			FROM nonparishcemetery
			WHERE
				((@burial_date_from IS NULL AND @burial_date_to IS NULL) OR BuriedDate BETWEEN @burial_date_from AND @burial_date_to)
				AND
				((@deceased_date_from IS NULL AND @deceased_date_to IS NULL) OR DeceasedDate BETWEEN @deceased_date_from AND @deceased_date_to)
			ORDER BY cemeterycode DESC
		END
    ELSE
		BEGIN
			SELECT
				cemetery_id AS [cemeteryid],
				grave_number AS [Cemetery Code],
				deceased_name AS [Deceased Name],
				date_of_death AS [Deceased Date],
				burial_date AS [Burial Date],
				remarks AS [Remarks],
                f.head_of_family AS [Contact Person],
                f.phone AS [Contact Mobile]
			FROM cemetery_details cd
			INNER JOIN family f ON cd.family_id = f.family_id
			WHERE
				((@burial_date_from IS NULL AND @burial_date_to IS NULL) OR cd.burial_date BETWEEN @burial_date_from AND @burial_date_to)
				AND
				((@deceased_date_from IS NULL AND @deceased_date_to IS NULL) OR cd.date_of_death BETWEEN @deceased_date_from AND @deceased_date_to)
			ORDER BY grave_number DESC
		END
END
GO

CREATE OR ALTER PROCEDURE sp_SaveFamily
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
    @is_multiple_cards bit,
	@family_notes NVARCHAR(200),
	@last_subscriptin_date DATE,
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
		parish_member_since,
		family_notes,
		multiple_familycards,
		last_subscription_date
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
        YEAR(GETDATE()),
		@family_notes,
		@is_multiple_cards,
		@last_subscriptin_date
    );

    SET @family_id = SCOPE_IDENTITY();
END
GO


CREATE OR ALTER PROCEDURE sp_SaveNonParishCemetery
    @Name NVARCHAR(100),
    @DOB DATE = NULL,
    @Address NVARCHAR(200) = NULL,
    @City NVARCHAR(100) = NULL,
    @State NVARCHAR(50) = NULL,
    @ZipCode NVARCHAR(20) = NULL,
    @Remarks NVARCHAR(500) = NULL,
    @ContactPerson NVARCHAR(100) = NULL,
    @ContactPhone NVARCHAR(20) = NULL,
    @DeceasedDate DATE = NULL,
    @BuriedDate DATE = NULL,
    @gender NVARCHAR(6) = NULL,
    @cemeterycode NVARCHAR(6) = NULL
AS
BEGIN
    INSERT INTO nonparishcemetery (
        Name, DOB, Address, City, State, ZipCode, Remarks,
        ContactPerson, ContactPhone, DeceasedDate, BuriedDate,gender,cemeterycode
    )
    VALUES (
        @Name, @DOB, @Address, @City, @State, @ZipCode, @Remarks,
        @ContactPerson, @ContactPhone, @DeceasedDate, @BuriedDate,@gender,@cemeterycode
    );
END
GO

