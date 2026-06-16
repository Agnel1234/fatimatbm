@echo off
REM ================================================================================================
REM Fatima Church TBM - Database Setup Batch Script
REM Wrapper for DatabaseSetup.ps1 PowerShell script
REM Called by the installer
REM ================================================================================================

setlocal enabledelayedexpansion

REM Get parameters from installer or use defaults
set "SQL_SERVER=%1"
set "DATABASE=%2"
set "USERNAME=%3"
set "PASSWORD=%4"
set "SCRIPTS_PATH=%5"

REM Use defaults if not provided
if "!SQL_SERVER!"=="" set "SQL_SERVER=(localdb)\MSSQLLocalDB"
if "!DATABASE!"=="" set "DATABASE=fatimachurchtbm"
if "!USERNAME!"=="" set "USERNAME=sa"
if "!PASSWORD!"=="" set "PASSWORD=Cts190588!1"
if "!SCRIPTS_PATH!"=="" set "SCRIPTS_PATH=Scripts"

echo.
echo ========================================
echo Fatima Church TBM - Database Setup
echo ========================================
echo SQL Server: !SQL_SERVER!
echo Database: !DATABASE!
echo Scripts Path: !SCRIPTS_PATH!
echo.

REM Get the script directory
set "SCRIPT_DIR=%~dp0"

REM Run PowerShell script
powershell -NoProfile -ExecutionPolicy Bypass -File "!SCRIPT_DIR!DatabaseSetup.ps1" -SqlServer "!SQL_SERVER!" -Database "!DATABASE!" -Username "!USERNAME!" -Password "!PASSWORD!" -ScriptsPath "!SCRIPTS_PATH!"

set "ERRORLEVEL=%ERRORCODE%"
if "!ERRORLEVEL!"=="0" (
    echo.
    echo ========================================
    echo Setup completed successfully!
    echo ========================================
    exit /b 0
) else (
    echo.
    echo ========================================
    echo Setup failed with errors!
    echo ========================================
    pause
    exit /b 1
)
