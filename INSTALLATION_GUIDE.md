# Fatima Church TBM - Installation Guide

## Overview
This guide covers installing the Fatima Church TBM application on a new Windows machine with **automated database setup**.

---

## System Requirements

### Minimum Requirements
- **Windows 7 or Later** (Windows 10/11 recommended)
- **.NET Framework 4.8** or later
- **SQL Server 2016+** or **LocalDB** (SQL Express equivalent)
- **4GB RAM** minimum
- **500MB** disk space for application + database

### Supported SQL Server Versions
- SQL Server 2016 or later
- SQL Server LocalDB (part of SQL Server Express or Visual Studio)
- Azure SQL Database (with connectivity)

---

## Installation Steps

### Step 1: Prepare SQL Server (First-Time Setup)

**Option A: Using LocalDB (Recommended for Single-Machine Setup)**
```
LocalDB is included with Visual Studio and SQL Server Express.
Default instance: (localdb)\MSSQLLocalDB
```

**Option B: Using SQL Server Express**
1. Download SQL Server Express from: https://www.microsoft.com/en-us/sql-server/sql-server-express
2. Install with default settings
3. Note the instance name during installation (e.g., `COMPUTERNAME\SQLEXPRESS`)

**Option C: Using Full SQL Server**
- Use your existing SQL Server instance name

### Step 2: Download the Installer

1. Get the installer file: `FatimaChurchTBM_Setup_v1.0.1.exe`
2. Save to a temporary folder on your desktop

### Step 3: Run the Installer

1. **Double-click** `FatimaChurchTBM_Setup_v1.0.1.exe`
2. Windows may display a security warning - click **"Run anyway"**
3. Click **"Yes"** when prompted for Admin Rights

### Step 4: Follow the Installation Wizard

The installer presents several screens:

#### Screen 1: Welcome
- Review the installation information
- Click **Next**

#### Screen 2: Choose Install Location
- Default: `C:\Program Files\Fatima Church TBM`
- Click **Next** (or change path if desired)

#### Screen 3: Database Configuration ⭐
**This is the most important step:**

```
SQL Server Instance: (localdb)\MSSQLLocalDB
  └─ For LocalDB: use (localdb)\MSSQLLocalDB
  └─ For SQL Express: use COMPUTERNAME\SQLEXPRESS
  └─ For Full SQL Server: use SERVER_NAME or IP_ADDRESS

Database Name: fatimachurchtbm
  └─ Leave as default unless you prefer a different name
```

**Click Next** to validate the connection

#### Screen 4: Database Credentials
```
Username: sa
  └─ Default SQL Server system admin account

Password: [Your SQL Server Password]
  └─ Enter the password for the 'sa' account
```

**Click Next**

#### Screen 5: Select Additional Tasks
- Choose whether to create a **Desktop Icon** (recommended)
- Click **Next**

#### Screen 6: Ready to Install
- Review all settings
- Click **Install**

### Step 5: Database Initialization

The installer will:
1. ✅ Test the SQL Server connection
2. ✅ Create the database (if it doesn't exist)
3. ✅ Execute all SQL scripts in order:
   - Initial schema (DDL)
   - Initial data (DML)
   - V4 updates
   - Performance optimizations
   - PDF export procedures
4. ✅ Update the connection string in `App.config`

**Status Messages (you should see):**
```
✓ Connected to SQL Server successfully
ℹ Creating database 'fatimachurchtbm'...
✓ Database 'fatimachurchtbm' created successfully
ℹ Executing: Initial_ddl_Scripts.sql
✓ Completed: Initial_ddl_Scripts.sql
ℹ Executing: Initial_dml_Scripts.sql
✓ Completed: Initial_dml_Scripts.sql
...
✓ Database setup completed successfully!
```

### Step 6: Launch the Application

- The installer automatically launches the application
- **Login Screen** appears - use the default credentials or admin account
- **Success!** You're now ready to use Fatima Church TBM

---

## Troubleshooting

### Issue 1: SQL Server Connection Error

**Symptom:** "Cannot connect to SQL Server at..."

**Solutions:**
1. **Verify SQL Server is running:**
   - Open **SQL Server Configuration Manager**
   - Ensure `SQL Server (LOCALDB)` or your instance is **Started**

2. **Check instance name:**
   - Open **SQL Server Management Studio**
   - Look at the server name in the connection dialog
   - Use exactly that name in the installer

3. **Verify credentials:**
   - Default username: `sa`
   - Check if `sa` account is enabled in your SQL Server
   - If password is different, update it in installer

4. **Firewall/Network:**
   - If using remote SQL Server, ensure TCP/IP is enabled
   - Check Windows Firewall allows SQL Server port (default: 1433)

**If all else fails:** Contact your SQL Server administrator for connection details

---

### Issue 2: Database Already Exists

**Symptom:** Setup completes but tables already exist

**Solution (Database will be reused):**
- The installer detects existing database and skips creation
- All scripts are re-run (idempotent - safe to run multiple times)
- Existing data is preserved

---

### Issue 3: Scripts Fail to Execute

**Symptom:** "Setup failed with errors" message

**Common Causes & Solutions:**
1. **Permission Denied**
   - Ensure user has `CREATE DATABASE` permission
   - Contact your DBA to grant permissions

2. **Invalid SQL Syntax**
   - Verify SQL Server version supports T-SQL syntax
   - Check SQL Server is version 2016 or later

3. **Disk Space**
   - Ensure server has at least 1GB free disk space
   - Database will grow as data is added

**Manual Recovery:**
```sql
-- If automatic setup fails, run manually:
-- 1. Connect to SQL Server as admin
-- 2. Execute scripts from: C:\Program Files\Fatima Church TBM\Scripts\
-- 3. Run in this order:
--    Initial_ddl_Scripts.sql
--    Initial_dml_Scripts.sql
--    V4_ddl_Scripts.sql
--    V4_dml_Scripts.sql
--    Performance_Optimization.sql
--    PDF_Export_StoredProcedures.sql
```

---

### Issue 4: Application Won't Start

**Symptom:** Application launches then closes immediately

**Solutions:**
1. **Check .NET Framework:**
   - Open Control Panel → Programs → Programs and Features
   - Look for ".NET Framework 4.8"
   - If missing, download and install from Microsoft

2. **Check database connection:**
   - Open file: `C:\Program Files\Fatima Church TBM\FatimaChurch.exe.config`
   - Look for `<connectionStrings>` section
   - Verify connection string has correct server/database names

3. **Check application log:**
   - Look in: `C:\Program Files\Fatima Church TBM\`
   - Check for any `.log` files
   - Review error messages

4. **Repair installation:**
   - Uninstall the application
   - Delete folder: `C:\Program Files\Fatima Church TBM\`
   - Restart computer
   - Re-run installer

---

### Issue 5: Performance is Slow

**Already Optimized!**
- The installer includes performance optimizations
- If slow after fresh install, ensure:
  1. All scripts executed successfully (check logs)
  2. All indexes were created
  3. SQL Server has adequate resources (RAM, CPU)

**Verify indexes:**
```sql
-- Run this in SQL Server Management Studio:
USE fatimachurchtbm;
SELECT name FROM sys.indexes WHERE name LIKE 'IX_%' ORDER BY name;

-- Should see 15+ indexes (performance-critical ones marked with IX_)
```

---

## Post-Installation Configuration

### Access Control Settings
1. Open application
2. Go to **Settings/Admin Panel**
3. Set user roles and permissions

### Database Backup
- **Important:** Set up regular SQL Server backups
- Backup location: Ask your IT department or DBA
- Recommended: Daily backups

### Connection String Modifications

If you need to change the database connection later:
1. Stop the application
2. Edit: `C:\Program Files\Fatima Church TBM\FatimaChurch.exe.config`
3. Modify the `<connectionStrings>` section:
   ```xml
   <add name="DefaultConnection" 
        connectionString="Data Source=YOUR_SERVER;
                         Initial Catalog=YOUR_DATABASE;
                         Persist Security Info=False;
                         User ID=YOUR_USER;
                         Password=YOUR_PASSWORD;
                         ..." />
   ```
4. Save and restart application

---

## Uninstalling

### Method 1: Using Control Panel
1. Open **Control Panel** → **Programs and Features**
2. Find **"Fatima Church TBM"**
3. Click **Uninstall**
4. Confirm deletion

### Method 2: Using Installer
- Run the `.exe` installer again
- Click **Uninstall**

**Note:** Uninstalling only removes the application, not the database.

### Removing the Database

If you want to completely remove the database:
1. Open **SQL Server Management Studio**
2. Right-click database `fatimachurchtbm`
3. Select **Delete**
4. Check **"Delete backup and restore history"**
5. Click **OK**

---

## Silent/Automated Installation

For IT administrators installing on multiple machines:

```batch
REM Silent installation with pre-configured database
FatimaChurchTBM_Setup_v1.0.1.exe /VERYSILENT /NORESTART ^
  /SQLSERVER="(localdb)\MSSQLLocalDB" ^
  /DATABASE="fatimachurchtbm" ^
  /USERNAME="sa" ^
  /PASSWORD="YourPassword"
```

---

## Support

If you encounter issues not covered here:

1. **Check Event Viewer:**
   - Right-click **My Computer** → **Manage**
   - Navigate to **Event Viewer** → **Windows Logs** → **Application**
   - Look for errors from "FatimaChurch"

2. **Review Installation Log:**
   - Installer creates log: `Setup Log 2024-XX-XX #001.txt`
   - Check for error details

3. **Contact Support:**
   - Provide:
     - Windows version
     - SQL Server version
     - Installation error message
     - Event Viewer errors
   - Email to: [Your Support Email]

---

## Quick Reference

| Component | Details |
|-----------|---------|
| **Application Name** | Fatima Church TBM |
| **Version** | 1.0.1 |
| **Install Folder** | C:\Program Files\Fatima Church TBM |
| **Config File** | FatimaChurch.exe.config |
| **Script Folder** | \Scripts (inside install folder) |
| **Default Database** | fatimachurchtbm |
| **Default Server** | (localdb)\MSSQLLocalDB |
| **Default Port** | 1433 (SQL Server) |
| **.NET Requirement** | 4.8+ |

---

**Last Updated:** 2026-05-09  
**Created for:** Fatima Church TBM v1.0.1
