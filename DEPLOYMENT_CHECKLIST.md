# Fatima Church TBM - Deployment & Build Checklist

## Pre-Build Verification Checklist

### Code & Database
- [ ] All source code committed to git
- [ ] Database scripts tested on clean environment
- [ ] Performance optimizations applied (verify indexes exist)
- [ ] All stored procedures validated
- [ ] App.config.template created with placeholders
- [ ] Connection string format verified

### Application Build
- [ ] Build in Release mode: `dotnet build -c Release` or use Visual Studio
- [ ] Output directory: `bin\Debug\` contains:
  - [ ] `FatimaChurch.exe`
  - [ ] `FatimaChurch.exe.config`
  - [ ] All Syncfusion DLLs
  - [ ] All dependent DLLs
- [ ] No build warnings or errors
- [ ] Test run on developer machine passes

### Installer Files
- [ ] `setup_with_db.iss` configured correctly
- [ ] SQL Scripts packaged:
  - [ ] `Initial_ddl_Scripts.sql`
  - [ ] `Initial_dml_Scripts.sql`
  - [ ] `V4_ddl_Scripts.sql`
  - [ ] `V4_dml_Scripts.sql`
  - [ ] `Performance_Optimization.sql`
  - [ ] `PDF_Export_StoredProcedures.sql`
- [ ] Setup helpers included:
  - [ ] `DatabaseSetup.ps1`
  - [ ] `DatabaseSetup.bat`
- [ ] `App.config.template` in root directory
- [ ] Icon files included (church-2.ico, icon.ico)

### Documentation
- [ ] `INSTALLATION_GUIDE.md` complete
- [ ] `DEPLOYMENT_CHECKLIST.md` (this file) ready
- [ ] `README.md` updated with version
- [ ] Release notes prepared

---

## Building the Installer

### Option 1: Using Inno Setup Compiler (GUI)

**Prerequisites:**
- Download and install Inno Setup 6.x from https://jrsoftware.org/isdl.php

**Steps:**
1. Open Inno Setup Compiler
2. File → Open → `setup_with_db.iss`
3. Press **F9** (or Build → Compile)
4. Compiler will:
   - Validate syntax
   - Package files
   - Create self-extracting EXE
   - Output to: `Installer\FatimaChurchTBM_Setup_v1.0.1.exe`

**Expected output:**
```
Compiler report
===============
Lines processed: 250
Tokens processed: 500
...
Successfully created installer: Installer\FatimaChurchTBM_Setup_v1.0.1.exe
```

### Option 2: Using Command Line (ISCC.exe)

**Batch script to build installer:**

Create `build_installer.bat`:
```batch
@echo off
REM Build installer using ISCC command line

set "INNO_PATH=C:\Program Files (x86)\Inno Setup 6"
set "SETUP_FILE=setup_with_db.iss"

if not exist "%INNO_PATH%\ISCC.exe" (
    echo Error: Inno Setup not found at %INNO_PATH%
    echo Please install Inno Setup 6 from: https://jrsoftware.org/isdl.php
    pause
    exit /b 1
)

echo Building installer...
"%INNO_PATH%\ISCC.exe" "%SETUP_FILE%"

if %ERRORLEVEL% EQU 0 (
    echo.
    echo Installer built successfully!
    echo Output: Installer\FatimaChurchTBM_Setup_v1.0.1.exe
) else (
    echo.
    echo Installer build FAILED!
    pause
    exit /b 1
)
```

**Run:**
```batch
build_installer.bat
```

---

## Testing the Installer

### Test Environment Setup
- [ ] Fresh Windows 10/11 VM or physical machine
- [ ] No prior installation of Fatima Church TBM
- [ ] .NET Framework 4.8 installed
- [ ] SQL Server LocalDB or Express installed

### Installation Test

1. **Run installer:**
   ```
   FatimaChurchTBM_Setup_v1.0.1.exe
   ```

2. **Step through wizard:**
   - [ ] Welcome screen appears
   - [ ] Install path page shows correct default
   - [ ] Database Configuration page accepts inputs
   - [ ] SQL Server instance: `(localdb)\MSSQLLocalDB`
   - [ ] Database name: `fatimachurchtbm`
   - [ ] Credentials page accepts sa/password
   - [ ] Task selection page appears

3. **Database initialization:**
   - [ ] Installer reports "Setting up database..."
   - [ ] Scripts execute in order (watch for errors)
   - [ ] Final message: "Database setup completed successfully!"
   - [ ] Application starts (or "Launch application" appears)

4. **Post-installation verification:**
   - [ ] Application folder created: `C:\Program Files\Fatima Church TBM\`
   - [ ] App executable exists: `FatimaChurch.exe`
   - [ ] Config file exists: `FatimaChurch.exe.config`
   - [ ] Scripts folder: `C:\Program Files\Fatima Church TBM\Scripts\`
   - [ ] All SQL scripts present in Scripts folder

5. **Application launch test:**
   - [ ] Double-click `FatimaChurch.exe`
   - [ ] Application window appears (no crashes)
   - [ ] Login form displays
   - [ ] Can log in with default credentials
   - [ ] Families grid loads (verify performance improvement)
   - [ ] Database connectivity confirmed

6. **Database verification (optional):**
   ```sql
   -- Run in SQL Server Management Studio:
   USE fatimachurchtbm;
   
   -- Check tables created
   SELECT COUNT(*) AS TableCount FROM sys.tables;
   -- Should be > 20
   
   -- Check indexes created
   SELECT COUNT(*) AS IndexCount FROM sys.indexes WHERE name LIKE 'IX_%';
   -- Should be > 10
   
   -- Check stored procedures
   SELECT COUNT(*) AS ProcCount FROM sys.objects WHERE type = 'P';
   -- Should be > 30
   ```

### Test Scenarios

#### Scenario 1: Fresh Install
- [ ] No database exists
- [ ] Installer creates database
- [ ] All scripts execute successfully
- [ ] Application starts and works

#### Scenario 2: Reinstall
- [ ] Database exists from previous install
- [ ] Installer detects and reuses database
- [ ] Scripts re-run (idempotent)
- [ ] Application starts and works
- [ ] Existing data preserved

#### Scenario 3: Different SQL Server Instance
- [ ] Test with `COMPUTERNAME\SQLEXPRESS` instead of LocalDB
- [ ] Installer accepts the instance name
- [ ] Database created on correct instance
- [ ] Application connects successfully

#### Scenario 4: Custom Database Name
- [ ] Installer accepts custom database name: `MyCustomDB`
- [ ] Database created with custom name
- [ ] App.config updated with custom name
- [ ] Application connects successfully

---

## Performance Testing

After installation, verify performance optimization is working:

### Test 1: Family Grid Load Time
1. Launch application
2. Navigate to Families tab
3. Measure load time (should be **< 5 seconds**)
4. Compare to old version (was **60+ seconds**)

### Test 2: Database Indexes
```sql
USE fatimachurchtbm;

-- Verify critical indexes exist:
SELECT name FROM sys.indexes 
WHERE name LIKE 'IX_family_member%' 
   OR name LIKE 'IX_cemetery%'
ORDER BY name;

-- Expected output:
-- IX_cemetery_details_family_id
-- IX_family_member_family_id_occupation
-- IX_family_member_family_id_status
-- IX_family_member_occupation
```

### Test 3: Query Execution Time
```sql
-- Test the optimized stored procedure
SET STATISTICS TIME ON;
EXEC sp_GetFamilyBasicDetailsPaged @pageNumber=1, @pageSize=25;
SET STATISTICS TIME OFF;

-- Should complete in < 1 second
-- Check message tab: "SQL Server parse and compile time: X ms"
```

---

## Building Release Media

### Create Distribution Package

1. **Create release folder structure:**
   ```
   FatimaChurchTBM_v1.0.1_Release\
   ├── FatimaChurchTBM_Setup_v1.0.1.exe
   ├── INSTALLATION_GUIDE.md
   ├── SYSTEM_REQUIREMENTS.txt
   ├── README.txt
   └── License.txt (if applicable)
   ```

2. **Create README.txt:**
   ```
   ========================================
   Fatima Church TBM v1.0.1
   ========================================
   
   QUICK START:
   1. Ensure .NET Framework 4.8+ is installed
   2. Ensure SQL Server or LocalDB is installed
   3. Run: FatimaChurchTBM_Setup_v1.0.1.exe
   4. Follow the installation wizard
   5. Database will be set up automatically
   
   REQUIREMENTS:
   - Windows 7 or later
   - .NET Framework 4.8+
   - SQL Server 2016+ or LocalDB
   - 500MB disk space
   
   SUPPORT:
   See INSTALLATION_GUIDE.md for detailed instructions
   and troubleshooting.
   
   RELEASE DATE: 2026-05-09
   ```

3. **Create SYSTEM_REQUIREMENTS.txt:**
   ```
   SYSTEM REQUIREMENTS - Fatima Church TBM v1.0.1
   ================================================
   
   MINIMUM:
   - OS: Windows 7 SP1 or later
   - CPU: Intel Core 2 Duo or equivalent (1.5 GHz+)
   - RAM: 4 GB
   - Disk: 500 MB free space
   - .NET Framework 4.8 or later
   - SQL Server 2016 or LocalDB
   
   RECOMMENDED:
   - OS: Windows 10 (21H2) or Windows 11
   - CPU: Intel Core i5 or AMD Ryzen 5 (2.4 GHz+)
   - RAM: 8 GB or more
   - Disk: SSD with 1 GB free space
   - .NET Framework 4.8.1 or later
   - SQL Server 2019 or later / LocalDB
   
   DATABASE:
   - SQL Server 2016 SP2+
   - SQL Server 2017+
   - SQL Server 2019+
   - SQL Server 2022+
   - SQL Server Express / LocalDB
   - Azure SQL Database (remote installations)
   
   NETWORK:
   - For local: Direct SQL Server connection required
   - For remote: TCP/IP enabled on SQL Server
   - For Azure: Azure SQL Database firewall rules
   ```

4. **Compress for distribution:**
   ```batch
   REM Create ZIP file for easy distribution
   "C:\Program Files\7-Zip\7z.exe" a -tzip ^
     FatimaChurchTBM_v1.0.1_Release.zip ^
     FatimaChurchTBM_v1.0.1_Release\*
   ```

---

## Version Control & Release Management

### Git Setup
```bash
# Tag the release version
git tag -a v1.0.1 -m "Fatima Church TBM v1.0.1 - Database installation automation"

# Push tag to repository
git push origin v1.0.1

# Create release branch (optional)
git checkout -b release/v1.0.1
git push origin release/v1.0.1
```

### Release Notes Template
```
# Fatima Church TBM v1.0.1 Release Notes

## New Features
- Automated database installation with deployment wizard
- Database schema includes all tables, indexes, and stored procedures
- Performance optimizations for family grid loading (12-20x faster)

## Bug Fixes
- Fixed N+1 query performance issue (60+ seconds → 3-5 seconds)
- Added missing database indexes for optimal query performance

## Installation Changes
- New Inno Setup installer with custom database configuration pages
- Automatic database creation and schema initialization
- Connection string management during installation

## Database
- Performance_Optimization.sql applied automatically
- All indexes created during installation
- Sample data pre-loaded

## System Requirements
- Windows 7 or later
- .NET Framework 4.8 or later
- SQL Server 2016+ or LocalDB
- 500 MB disk space

## Known Issues
None

## Upgrade Path
Fresh installation recommended. For upgrades from v1.0.0:
1. Uninstall previous version
2. Run new installer
3. Database will be initialized automatically

## Support
See INSTALLATION_GUIDE.md for detailed installation and troubleshooting.

---
Release Date: 2026-05-09
```

---

## Continuous Integration Setup (Optional)

### GitHub Actions Example (for automated builds)

Create `.github/workflows/build-installer.yml`:
```yaml
name: Build Installer

on:
  push:
    branches: [main, release/*]
  pull_request:
    branches: [main]

jobs:
  build:
    runs-on: windows-latest
    steps:
      - uses: actions/checkout@v2
      
      - name: Setup .NET
        uses: actions/setup-dotnet@v1
        with:
          dotnet-version: '4.8.x'
      
      - name: Build application
        run: dotnet build -c Release
      
      - name: Install Inno Setup
        run: |
          choco install innosetup -y
      
      - name: Build installer
        run: |
          "C:\Program Files (x86)\Inno Setup 6\ISCC.exe" setup_with_db.iss
      
      - name: Upload artifacts
        uses: actions/upload-artifact@v2
        with:
          name: installer
          path: Installer/FatimaChurchTBM_Setup_v1.0.1.exe
```

---

## Deployment Summary

| Step | Responsibility | Status |
|------|----------------|--------|
| Code commit | Developer | ✓ |
| Build application | Build system | ✓ |
| Test on dev machine | Developer | ✓ |
| Create installer | Build system | ⏳ |
| Test on fresh VM | QA/Tester | ⏳ |
| Performance verification | QA | ⏳ |
| Document release | Documentation | ⏳ |
| Distribution | IT/Release Manager | ⏳ |

---

## Post-Deployment

### First-Time Users
1. Send installation guide: `INSTALLATION_GUIDE.md`
2. Provide installer file
3. Provide support contact information
4. Monitor for support requests

### Feedback Loop
- Track installation issues
- Collect user feedback
- Plan improvements for next version
- Document workarounds if needed

---

**Last Updated:** 2026-05-09  
**Version:** 1.0.1  
**Status:** Ready for Deployment

