# Idempotent Database Scripts Guide

## Overview

**Idempotent** scripts are safe to run **multiple times** without errors or data loss. This is critical for production deployments where:

1. **First Installation**: Fresh database server with no data
2. **Reinstallation**: Database exists with existing data and needs upgrade
3. **Updates**: Adding new features to existing system

---

## The Problem with Non-Idempotent Scripts

### ❌ BAD - Will fail on second run:
```sql
-- This fails if table already exists
CREATE TABLE users (
    id INT PRIMARY KEY,
    name NVARCHAR(100)
);

-- Error: "There is already an object named 'users' in the database"
```

### ✅ GOOD - Safe to run multiple times:
```sql
-- Checks if table exists before creating
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'users' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    CREATE TABLE users (
        id INT PRIMARY KEY,
        name NVARCHAR(100)
    );
END
```

---

## Idempotent Patterns for SQL Server

### 1. Database Creation
```sql
-- ❌ BAD
CREATE DATABASE MyDatabase;

-- ✅ GOOD
IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'MyDatabase')
BEGIN
    CREATE DATABASE MyDatabase;
    PRINT 'Database created';
END
ELSE
BEGIN
    PRINT 'Database already exists';
END
```

### 2. Table Creation
```sql
-- ❌ BAD
CREATE TABLE users (
    id INT PRIMARY KEY,
    name NVARCHAR(100)
);

-- ✅ GOOD
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'users' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    CREATE TABLE users (
        id INT PRIMARY KEY,
        name NVARCHAR(100)
    );
    PRINT '✓ Table [users] created';
END
ELSE
BEGIN
    PRINT '✓ Table [users] already exists';
END
```

### 3. Foreign Key Addition
```sql
-- ❌ BAD
ALTER TABLE orders
ADD CONSTRAINT FK_orders_customers FOREIGN KEY (customer_id)
REFERENCES customers(id);

-- ✅ GOOD
IF NOT EXISTS (SELECT * FROM sys.foreign_keys 
               WHERE name = 'FK_orders_customers' 
               AND parent_object_id = OBJECT_ID('orders'))
BEGIN
    ALTER TABLE orders
    ADD CONSTRAINT FK_orders_customers FOREIGN KEY (customer_id)
    REFERENCES customers(id);
    PRINT '✓ Foreign Key added';
END
ELSE
BEGIN
    PRINT '✓ Foreign Key already exists';
END
```

### 4. Index Creation
```sql
-- ❌ BAD
CREATE INDEX IX_users_email ON users(email);

-- ✅ GOOD
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_users_email')
BEGIN
    CREATE INDEX IX_users_email ON users(email);
    PRINT '✓ Index created';
END
ELSE
BEGIN
    PRINT '✓ Index already exists';
END
```

### 5. Stored Procedure Creation/Update
```sql
-- ❌ BAD
CREATE PROCEDURE sp_GetUsers
AS
BEGIN
    SELECT * FROM users;
END

-- ✅ GOOD - Use CREATE OR ALTER (SQL Server 2016+)
CREATE OR ALTER PROCEDURE sp_GetUsers
AS
BEGIN
    SELECT * FROM users;
END
-- This creates the procedure if it doesn't exist, or updates it if it does
```

### 6. Column Addition (With Null Handling)
```sql
-- ❌ BAD
ALTER TABLE users ADD phone NVARCHAR(20);

-- ✅ GOOD - Check if column exists first
IF NOT EXISTS (SELECT * FROM sys.columns 
               WHERE object_id = OBJECT_ID('users') 
               AND name = 'phone')
BEGIN
    ALTER TABLE users ADD phone NVARCHAR(20);
    PRINT '✓ Column [phone] added';
END
ELSE
BEGIN
    PRINT '✓ Column [phone] already exists';
END
```

### 7. Default Value Addition
```sql
-- ❌ BAD
ALTER TABLE users ADD DEFAULT GETDATE() FOR created_at;

-- ✅ GOOD
IF NOT EXISTS (SELECT * FROM sys.default_constraints 
               WHERE parent_object_id = OBJECT_ID('users') 
               AND name LIKE '%created_at%')
BEGIN
    ALTER TABLE users ADD DEFAULT GETDATE() FOR created_at;
    PRINT '✓ Default constraint added';
END
ELSE
BEGIN
    PRINT '✓ Default constraint already exists';
END
```

### 8. Data Insertion (Only if not exists)
```sql
-- ❌ BAD - Always inserts, creating duplicates on reruns
INSERT INTO roles (name) VALUES ('Admin');
INSERT INTO roles (name) VALUES ('User');

-- ✅ GOOD - Only insert if data doesn't exist
IF NOT EXISTS (SELECT * FROM roles WHERE name = 'Admin')
BEGIN
    INSERT INTO roles (name) VALUES ('Admin');
    PRINT '✓ Role [Admin] inserted';
END

IF NOT EXISTS (SELECT * FROM roles WHERE name = 'User')
BEGIN
    INSERT INTO roles (name) VALUES ('User');
    PRINT '✓ Role [User] inserted';
END
```

### 9. View Creation
```sql
-- ❌ BAD
CREATE VIEW vw_ActiveUsers AS
SELECT * FROM users WHERE active = 1;

-- ✅ GOOD
CREATE OR ALTER VIEW vw_ActiveUsers AS
SELECT * FROM users WHERE active = 1;
-- or DROP IF EXISTS (older SQL versions)
```

### 10. Schema Creation
```sql
-- ✅ GOOD
IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = 'reports')
BEGIN
    EXEC('CREATE SCHEMA reports');
    PRINT '✓ Schema [reports] created';
END
```

---

## Master Installation Script Pattern

The recommended approach is using a **Master Script** that:

1. **Creates database** (if not exists)
2. **Creates all tables** (with IF NOT EXISTS checks)
3. **Adds foreign keys** (with existence checks)
4. **Creates indexes** (with existence checks)
5. **Creates procedures** (with CREATE OR ALTER)
6. **Inserts initial data** (with existence checks)

**See: `Master_Installation_Script.sql`** for complete example.

---

## Execution Order for Idempotent Scripts

When running scripts on existing database:

```
1. Master_Installation_Script.sql
   ├─ Creates/verifies database structure
   ├─ Creates/verifies tables
   ├─ Creates/verifies foreign keys
   ├─ Creates/verifies indexes
   └─ All data preserved ✓

2. V4_DML_Stored_Procedures.sql
   ├─ Uses CREATE OR ALTER (safe to rerun)
   ├─ Updates procedures to latest version
   └─ No data loss

3. Performance_Optimization.sql
   ├─ Creates indexes if missing
   ├─ Skips if already exist
   └─ No data loss

4. PDF_Export_StoredProcedures.sql
   ├─ Uses CREATE OR ALTER
   └─ No data loss
```

**Result:** All scripts safe to run in any order, any number of times!

---

## Testing Idempotency

### Test Scenario 1: Fresh Installation
```sql
-- Run on new/empty database
1. Execute Master_Installation_Script.sql
   ✓ Creates all tables
   ✓ Creates all foreign keys
   ✓ Creates all indexes

2. Execute V4_DML_Stored_Procedures.sql
   ✓ Creates procedures
   ✓ Inserts initial data

3. Verify:
   SELECT COUNT(*) FROM sys.tables;          -- Should be > 5
   SELECT COUNT(*) FROM sys.foreign_keys;    -- Should be > 5
   SELECT COUNT(*) FROM sys.indexes;         -- Should be > 10
```

### Test Scenario 2: Rerun on Same Database (With Data)
```sql
-- Insert test data
INSERT INTO anbiyam (anbiyam_name, anbiyam_code, anbiyam_zone, 
                     anbiyam_coordinator_name)
VALUES ('Test Anbiyam', 'TST', 1, 'John Doe');

-- Run scripts again
1. Execute Master_Installation_Script.sql
   ✓ Detects existing tables (no recreation)
   ✓ Detects existing foreign keys (no re-adding)
   ✓ Detects existing indexes (no re-creation)
   ✓ DATA PRESERVED - Test data still there!

2. Execute V4_DML_Stored_Procedures.sql
   ✓ Updates procedures
   ✓ No data loss

3. Verify:
   SELECT COUNT(*) FROM anbiyam;             -- Still 1 record
   SELECT * FROM anbiyam;                    -- Original data intact
```

### Test Scenario 3: Upgrade with New Columns
```sql
-- Simulate schema update in next version
-- Add new column if it doesn't exist:

IF NOT EXISTS (SELECT * FROM sys.columns 
               WHERE object_id = OBJECT_ID('family') 
               AND name = 'new_column')
BEGIN
    ALTER TABLE family ADD new_column NVARCHAR(100);
END

-- This is safe to run:
-- - On fresh database: Column created
-- - On existing database: Column skipped if exists, created if missing
-- - All data preserved!
```

---

## Script Execution Order (Recommended)

For deployment, execute scripts in this order:

```
1. Master_Installation_Script.sql
   └─ Handles: Database, Tables, FK, Indexes
   └─ Status: ✓ Idempotent

2. Initial_dml_Scripts.sql (if has idempotent checks)
   └─ Handles: Stored procs, initial data
   └─ Status: ✓ Idempotent (if refactored)

3. V4_dml_Scripts.sql (if has idempotent checks)
   └─ Handles: V4 procs, updates
   └─ Status: ✓ Idempotent (if refactored)

4. Performance_Optimization.sql
   └─ Handles: Performance indexes, optimized procs
   └─ Status: ✓ Idempotent (uses CREATE OR ALTER)

5. PDF_Export_StoredProcedures.sql
   └─ Handles: PDF procedures
   └─ Status: ✓ Idempotent (uses CREATE OR ALTER)
```

---

## Refactoring Existing Scripts to be Idempotent

### Process:

1. **Identify non-idempotent lines:**
   - `CREATE TABLE` → Check with `IF NOT EXISTS`
   - `CREATE INDEX` → Check with `IF NOT EXISTS`
   - `ALTER TABLE ADD` → Check column existence
   - `INSERT` statements → Check if data exists
   - `CREATE PROCEDURE` → Use `CREATE OR ALTER`

2. **Add existence checks:**
   ```sql
   IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'TableName')
   BEGIN
       -- Create table
   END
   ```

3. **Test thoroughly:**
   - Run on fresh database
   - Run on existing database with data
   - Run script 3+ times - should produce same results

4. **Add status messages:**
   ```sql
   PRINT '✓ Table created';
   PRINT '✓ Table already exists (skipped)';
   ```

---

## SQL Server Version Compatibility

### `CREATE OR ALTER` Support:
- **SQL Server 2016+**: Full support ✓
- **SQL Server 2012**: Use `DROP IF EXISTS` + `CREATE`
- **Older versions**: Manual DROP then CREATE

### `IF NOT EXISTS` Support:
- **All modern versions**: Full support ✓
- **Very old versions**: May not support - upgrade recommended

---

## Benefits of Idempotent Scripts

| Scenario | Idempotent | Non-Idempotent |
|----------|-----------|-----------------|
| **Fresh Install** | ✓ Works | ✓ Works |
| **Reinstall** | ✓ Works | ❌ Fails |
| **Upgrade** | ✓ Works | ❌ Fails (needs manual workarounds) |
| **Multi-machine Deploy** | ✓ Works on all | ❌ Inconsistent |
| **CI/CD Pipeline** | ✓ Reliable | ❌ Error-prone |
| **Developer Testing** | ✓ Easy rerun | ❌ Complex reset needed |

---

## Checklist for Script Review

Before deploying, verify each script:

- [ ] All `CREATE TABLE` use `IF NOT EXISTS`
- [ ] All `CREATE INDEX` use `IF NOT EXISTS`
- [ ] All `CREATE PROCEDURE` use `CREATE OR ALTER`
- [ ] All `ALTER TABLE ADD COLUMN` check column existence
- [ ] All `INSERT` statements check if data exists (for static data)
- [ ] Foreign keys use `IF NOT EXISTS`
- [ ] Indexes use `IF NOT EXISTS`
- [ ] Scripts include status messages (✓ created/already exists)
- [ ] Tested on fresh database
- [ ] Tested on existing database with data
- [ ] Tested by running script 3+ times
- [ ] No data loss after multiple runs

---

## Production Deployment Guarantee

When all scripts follow idempotent patterns:

✅ **New Server**: Run installer → Database ready instantly  
✅ **Existing Server**: Run installer → Database upgraded, data preserved  
✅ **Reinstall**: Re-run installer → Same result, no errors  
✅ **Updates**: Add new scripts → Only new objects created  
✅ **Multi-Location**: Same installer → Works everywhere  

---

## See Also

- `Master_Installation_Script.sql` - Complete idempotent example
- `DatabaseSetup.ps1` - Orchestrates script execution
- `DEPLOYMENT_SUMMARY.md` - Overall deployment strategy

---

**Last Updated:** 2026-05-09  
**Version:** 1.0.1
