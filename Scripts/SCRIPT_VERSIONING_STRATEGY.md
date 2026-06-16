# Database Script Versioning & Upgrade Strategy

## Overview

As the application evolves, database scripts must be managed carefully to ensure:
- ✅ Fresh installations work perfectly
- ✅ Upgrades preserve all existing data
- ✅ Scripts can be re-run safely (idempotent)
- ✅ Version tracking and rollback capability

---

## Current Script Architecture (v1.0.1)

```
┌─────────────────────────────────────────────────────────────┐
│ Master_Installation_Script.sql (NEW)                        │
│ ─────────────────────────────────────────────────────────── │
│ • Idempotent database and schema creation                  │
│ • Creates all tables (IF NOT EXISTS checks)                │
│ • Adds all foreign keys (IF NOT EXISTS checks)             │
│ • Creates all performance indexes                          │
│ • Status: ✓ IDEMPOTENT (safe to run multiple times)        │
└─────────────────────────────────────────────────────────────┘
                           ↓
┌─────────────────────────────────────────────────────────────┐
│ Initial_dml_Scripts.sql                                     │
│ ─────────────────────────────────────────────────────────── │
│ • Stored procedures (CREATE OR ALTER)                      │
│ • Initial static data (IF NOT EXISTS checks)               │
│ • Views and utility procedures                             │
│ • Status: ✓ IDEMPOTENT (uses CREATE OR ALTER)              │
└─────────────────────────────────────────────────────────────┘
                           ↓
┌─────────────────────────────────────────────────────────────┐
│ V4_ddl_Scripts.sql                                          │
│ ─────────────────────────────────────────────────────────── │
│ • V4 schema enhancements                                   │
│ • New tables/columns (IF NOT EXISTS checks)                │
│ • Status: ✓ IDEMPOTENT (checks existence)                   │
└─────────────────────────────────────────────────────────────┘
                           ↓
┌─────────────────────────────────────────────────────────────┐
│ V4_dml_Scripts.sql                                          │
│ ─────────────────────────────────────────────────────────── │
│ • V4 stored procedures                                     │
│ • V4 data transformations                                  │
│ • Status: ✓ IDEMPOTENT (uses CREATE OR ALTER)              │
└─────────────────────────────────────────────────────────────┘
                           ↓
┌─────────────────────────────────────────────────────────────┐
│ Performance_Optimization.sql                                │
│ ─────────────────────────────────────────────────────────── │
│ • Performance-critical indexes                             │
│ • Optimized stored procedures                              │
│ • Status: ✓ IDEMPOTENT (uses CREATE OR ALTER + IF EXISTS)   │
└─────────────────────────────────────────────────────────────┘
                           ↓
┌─────────────────────────────────────────────────────────────┐
│ PDF_Export_StoredProcedures.sql                             │
│ ─────────────────────────────────────────────────────────── │
│ • PDF generation procedures                                │
│ • Status: ✓ IDEMPOTENT (uses CREATE OR ALTER)              │
└─────────────────────────────────────────────────────────────┘
```

---

## Script Execution Scenarios

### Scenario 1: Fresh Installation (New Server)

**Execution:**
```
1. Master_Installation_Script.sql → Creates everything from scratch ✓
2. Initial_dml_Scripts.sql → Creates procedures + inserts initial data ✓
3. V4_ddl_Scripts.sql → Applies V4 schema updates ✓
4. V4_dml_Scripts.sql → Creates V4 procedures ✓
5. Performance_Optimization.sql → Adds indexes ✓
6. PDF_Export_StoredProcedures.sql → Adds PDF procedures ✓
```

**Result:** Complete, optimized database ready to use ✓

### Scenario 2: Reinstallation (Database Already Exists with Data)

**Execution:**
```
1. Master_Installation_Script.sql
   → Detects existing tables
   → Skips creation (already exist)
   → Adds missing foreign keys
   → Adds missing indexes
   → DATA PRESERVED ✓

2. Initial_dml_Scripts.sql
   → Updates existing procedures to latest version
   → Skips initial data (already inserted)
   → DATA PRESERVED ✓

3. V4_ddl_Scripts.sql
   → Checks for new columns/tables
   → Only adds what's missing
   → DATA PRESERVED ✓

4. V4_dml_Scripts.sql
   → Updates procedures
   → DATA PRESERVED ✓

5. Performance_Optimization.sql
   → Verifies all indexes exist
   → Skips if already present
   → DATA PRESERVED ✓

6. PDF_Export_StoredProcedures.sql
   → Updates procedures
   → DATA PRESERVED ✓
```

**Result:** Database upgraded, all existing data intact ✓

### Scenario 3: Multi-Machine Deployment

**Machine A (Server with central database):**
```
→ Run installer → Database created/upgraded once
```

**Machine B (User workstation):**
```
→ Run installer → Connects to Machine A's database
→ Scripts still run but detect existing structure
→ Ensures schema is complete (if any tables missing)
```

**Machine C (Another user):**
```
→ Same as Machine B
→ All connect to same database
→ No conflicts, all scripts idempotent
```

**Result:** Consistent database across all installations ✓

---

## Version Evolution Example

### v1.0.0 → v1.0.1 (Current)

**New in v1.0.1:**
- Master_Installation_Script.sql (consolidates DDL)
- Performance_Optimization.sql (adds critical indexes)
- Idempotent refactoring of all scripts

**User with v1.0.0 installed:**
```
1. Download v1.0.1 installer
2. Run installer
3. Master_Installation_Script runs first
   → Detects existing tables (v1.0.0 created them)
   → Skips table creation
   → Adds new indexes from Performance_Optimization
4. Database upgraded to v1.0.1
5. All data preserved ✓
6. New performance features available immediately ✓
```

---

## Planning v1.0.2 (Future Example)

Let's say we need to:
- Add a new table: `liturgy_participation`
- Add a stored procedure: `sp_GetLiturgyParticipants`
- Add an index on `family_member(dob)`

**Create: `V4_v1_0_2_ddl_upgrade.sql`**
```sql
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

-- Idempotent: Only if new table doesn't exist
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'liturgy_participation')
BEGIN
    CREATE TABLE liturgy_participation (
        participation_id INT PRIMARY KEY IDENTITY(1,1),
        member_id INT NOT NULL,
        liturgy_type NVARCHAR(50) NOT NULL,
        participation_date DATE NOT NULL,
        role NVARCHAR(50) NULL,
        created_at DATETIME DEFAULT GETDATE(),
        FOREIGN KEY (member_id) REFERENCES family_member(member_id)
    );
    PRINT '✓ Table [liturgy_participation] created';
END
ELSE
BEGIN
    PRINT '✓ Table [liturgy_participation] already exists';
END
GO

-- Idempotent: Only if index doesn't exist
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_family_member_dob')
BEGIN
    CREATE INDEX IX_family_member_dob ON family_member(dob);
    PRINT '✓ Index [IX_family_member_dob] created';
END
GO

-- Idempotent: CREATE OR ALTER (updates if exists, creates if not)
CREATE OR ALTER PROCEDURE sp_GetLiturgyParticipants
    @member_id INT = NULL
AS
BEGIN
    SELECT 
        lp.participation_id,
        fm.first_name,
        fm.last_name,
        lp.liturgy_type,
        lp.participation_date,
        lp.role
    FROM liturgy_participation lp
    INNER JOIN family_member fm ON fm.member_id = lp.member_id
    WHERE (@member_id IS NULL OR lp.member_id = @member_id)
    ORDER BY lp.participation_date DESC;
END
GO

PRINT '✓ v1.0.2 upgrade script completed';
```

**Add to DatabaseSetup.ps1 execution list:**
```powershell
$Scripts = @(
    "Master_Installation_Script.sql",
    "Initial_dml_Scripts.sql",
    "V4_ddl_Scripts.sql",
    "V4_dml_Scripts.sql",
    "V4_v1_0_2_ddl_upgrade.sql",          # NEW!
    "Performance_Optimization.sql",
    "PDF_Export_StoredProcedures.sql"
)
```

**Result:** 
- v1.0.0 users: Upgrade by running installer, new features added ✓
- v1.0.1 users: Just add new features, database enhanced ✓
- v1.0.2 fresh install: Everything created in correct order ✓

---

## Naming Convention for Future Scripts

When creating new upgrade scripts, follow this naming:

```
V4_v{version}_ddl_upgrade.sql      - Schema changes (new tables, columns)
V4_v{version}_dml_upgrade.sql      - Procedure/data changes
V4_v{version}_hotfix.sql           - Emergency bug fixes
Performance_v{version}_upgrade.sql - Performance enhancements
```

**Examples:**
- `V4_v1_0_2_ddl_upgrade.sql` - Tables/columns for v1.0.2
- `V4_v1_0_2_dml_upgrade.sql` - Procedures for v1.0.2
- `V4_v1_1_0_ddl_upgrade.sql` - Major schema changes for v1.1.0
- `Performance_v1_0_2_upgrade.sql` - New indexes for v1.0.2

---

## Rules for Safe Upgrades

When writing upgrade scripts, follow these rules:

### ✅ DO:
1. Check existence before creating:
   ```sql
   IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'new_table')
   BEGIN
       CREATE TABLE new_table (...);
   END
   ```

2. Use CREATE OR ALTER for procedures:
   ```sql
   CREATE OR ALTER PROCEDURE sp_MyProc AS
   BEGIN
       -- Your code
   END
   ```

3. Check columns before altering tables:
   ```sql
   IF NOT EXISTS (SELECT * FROM sys.columns 
                  WHERE object_id = OBJECT_ID('my_table') 
                  AND name = 'new_column')
   BEGIN
       ALTER TABLE my_table ADD new_column NVARCHAR(50);
   END
   ```

4. Data migration with care:
   ```sql
   -- Only if data doesn't exist
   IF NOT EXISTS (SELECT * FROM new_table WHERE id = 1)
   BEGIN
       INSERT INTO new_table SELECT * FROM old_table;
   END
   ```

5. Include status messages:
   ```sql
   PRINT '✓ Table created' or '✓ Table already exists'
   ```

### ❌ DON'T:
1. ❌ Use plain `CREATE TABLE` without checking
2. ❌ Use `DROP TABLE` (data loss!)
3. ❌ Use `ALTER TABLE DROP COLUMN` (data loss!)
4. ❌ Make assumptions about what exists
5. ❌ Forget to add existence checks to old scripts

---

## Testing Upgrade Scripts

Before deploying any upgrade script:

### Test 1: Fresh Installation
```sql
-- 1. Create fresh test database
CREATE DATABASE TestDB;
USE TestDB;

-- 2. Run all scripts (fresh install scenario)
-- 3. Verify tables, procedures, indexes exist
-- 4. Verify no errors
```

### Test 2: Rerun on Existing Database
```sql
-- 1. Keep TestDB from Test 1 (has data)
-- 2. Run all scripts AGAIN
-- 3. Verify:
--    - Existing data preserved
--    - No duplicate inserts
--    - No "already exists" errors
--    - All new objects created
```

### Test 3: Backup & Restore
```sql
-- 1. Backup TestDB
-- 2. Restore to another database
-- 3. Run scripts on restored database
-- 4. Verify identical results
```

---

## Rollback Plan

If something goes wrong during upgrade:

### Option 1: Use SQL Server Backup
```sql
-- Before running upgrade
BACKUP DATABASE [fatimachurchtbm]
TO DISK = 'C:\Backups\fatimachurchtbm_before_upgrade.bak';

-- If upgrade fails, restore
RESTORE DATABASE [fatimachurchtbm]
FROM DISK = 'C:\Backups\fatimachurchtbm_before_upgrade.bak'
WITH REPLACE;
```

### Option 2: Manual Rollback (if no backup)
```sql
-- Remove new objects that were created
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'problem_table')
BEGIN
    DROP TABLE problem_table;
END

-- Drop new procedures
IF EXISTS (SELECT * FROM sys.objects WHERE name = 'sp_ProblemProc' AND type = 'P')
BEGIN
    DROP PROCEDURE sp_ProblemProc;
END
```

### Best Practice:
**Always backup before major upgrades!**

---

## Version Tracking Query

Track which version scripts have been applied:

```sql
-- Suggested: Add version tracking table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'schema_version')
BEGIN
    CREATE TABLE schema_version (
        version_id INT PRIMARY KEY IDENTITY(1,1),
        version_number NVARCHAR(20) NOT NULL UNIQUE,
        script_name NVARCHAR(255) NOT NULL,
        applied_date DATETIME DEFAULT GETDATE(),
        applied_by NVARCHAR(100) DEFAULT SYSTEM_USER
    );
END
GO

-- Insert records as scripts are applied
INSERT INTO schema_version (version_number, script_name)
VALUES ('1.0.1', 'Master_Installation_Script.sql');

-- Query applied versions
SELECT version_number, script_name, applied_date FROM schema_version ORDER BY applied_date;
```

---

## Checklist for Production Deployments

Before deploying any upgrade:

- [ ] All scripts are idempotent (tested multiple runs)
- [ ] Scripts are tested on fresh database
- [ ] Scripts are tested on database with existing data
- [ ] No data loss after running scripts
- [ ] All new objects created correctly
- [ ] Indexes created for performance
- [ ] Backup taken before running
- [ ] Rollback plan documented
- [ ] Scripts added to version control
- [ ] Release notes updated
- [ ] DatabaseSetup.ps1 updated with new scripts
- [ ] setup_with_db.iss updated (if needed)
- [ ] Documentation updated
- [ ] User communication prepared

---

## Summary

**v1.0.1 Architecture:**
- ✅ Master script handles all initial DDL (idempotent)
- ✅ All subsequent scripts use CREATE OR ALTER (idempotent)
- ✅ All data checks before INSERT (idempotent)
- ✅ Safe to run on fresh or existing databases
- ✅ Safe to run multiple times without issues
- ✅ Easy to extend with new upgrade scripts

**Your database is now production-ready for:**
- Fresh installations on new servers
- Reinstallations on existing servers
- Upgrades with data preservation
- Multi-machine deployments
- Future version updates

---

**Last Updated:** 2026-05-09  
**Version:** 1.0.1  
**Status:** Production Ready
