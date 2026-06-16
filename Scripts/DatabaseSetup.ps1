# ================================================================================================
# Fatima Church TBM - Database Setup Script
# Initializes the database with all required schemas, stored procedures, and indexes
# ================================================================================================

param(
    [string]$SqlServer = "(localdb)\MSSQLLocalDB",
    [string]$Database = "fatimachurchtbm",
    [string]$Username = "sa",
    [string]$Password = "Cts190588!1",
    [string]$ScriptsPath = "Scripts",
    [bool]$CreateDatabase = $true
)

# Color output
function Write-Success {
    param([string]$Message)
    Write-Host "✓ $Message" -ForegroundColor Green
}

function Write-Error-Custom {
    param([string]$Message)
    Write-Host "✗ ERROR: $Message" -ForegroundColor Red
}

function Write-Info {
    param([string]$Message)
    Write-Host "ℹ $Message" -ForegroundColor Cyan
}

function Write-Warning-Custom {
    param([string]$Message)
    Write-Host "⚠ WARNING: $Message" -ForegroundColor Yellow
}

Write-Info "=========================================="
Write-Info "Fatima Church TBM - Database Setup"
Write-Info "=========================================="
Write-Info "SQL Server: $SqlServer"
Write-Info "Database: $Database"
Write-Info "Scripts Path: $ScriptsPath"
Write-Info ""

# Build connection string
$ConnectionString = "Server=$SqlServer;User ID=$Username;Password=$Password;"

# ================================================================================================
# Function: Execute SQL Script
# ================================================================================================
function Execute-SqlScript {
    param(
        [string]$ScriptPath,
        [bool]$CreateDb = $false
    )

    try {
        if (-not (Test-Path $ScriptPath)) {
            Write-Error-Custom "Script file not found: $ScriptPath"
            return $false
        }

        $ScriptContent = Get-Content -Path $ScriptPath -Raw
        $ConnectionStr = $ConnectionString

        if (-not $CreateDb) {
            $ConnectionStr += "Initial Catalog=$Database;"
        }

        Write-Info "Executing: $(Split-Path $ScriptPath -Leaf)"

        $SqlConnection = New-Object System.Data.SqlClient.SqlConnection
        $SqlConnection.ConnectionString = $ConnectionStr
        $SqlConnection.Open()

        # Split script by GO statements
        $Batches = $ScriptContent -split "^\s*GO\s*$", 0, "MultilineIgnoreCase"

        foreach ($Batch in $Batches) {
            $Batch = $Batch.Trim()
            if ($Batch -ne "") {
                $SqlCommand = New-Object System.Data.SqlClient.SqlCommand
                $SqlCommand.CommandText = $Batch
                $SqlCommand.Connection = $SqlConnection
                $SqlCommand.CommandTimeout = 300  # 5 minutes timeout

                try {
                    $null = $SqlCommand.ExecuteNonQuery()
                }
                catch {
                    Write-Error-Custom "Error executing batch: $($_.Exception.Message)"
                    $SqlConnection.Close()
                    return $false
                }
            }
        }

        $SqlConnection.Close()
        Write-Success "Completed: $(Split-Path $ScriptPath -Leaf)"
        return $true
    }
    catch {
        Write-Error-Custom "Failed to execute script: $($_.Exception.Message)"
        return $false
    }
}

# ================================================================================================
# Function: Create Database
# ================================================================================================
function Create-Database {
    try {
        Write-Info "Checking if database exists..."

        $SqlConnection = New-Object System.Data.SqlClient.SqlConnection
        $SqlConnection.ConnectionString = $ConnectionString + "Initial Catalog=master;"
        $SqlConnection.Open()

        $SqlCommand = New-Object System.Data.SqlClient.SqlCommand
        $SqlCommand.CommandText = "SELECT COUNT(*) FROM sys.databases WHERE name = '$Database'"
        $SqlCommand.Connection = $SqlConnection

        $dbExists = $SqlCommand.ExecuteScalar()
        $SqlConnection.Close()

        if ($dbExists -gt 0) {
            Write-Success "Database '$Database' already exists."
            return $true
        }

        Write-Info "Creating database '$Database'..."

        $SqlConnection = New-Object System.Data.SqlClient.SqlConnection
        $SqlConnection.ConnectionString = $ConnectionString + "Initial Catalog=master;"
        $SqlConnection.Open()

        $SqlCommand = New-Object System.Data.SqlClient.SqlCommand
        $SqlCommand.CommandText = "CREATE DATABASE [$Database] COLLATE SQL_Latin1_General_CP1_CI_AS;"
        $SqlCommand.Connection = $SqlConnection
        $SqlCommand.CommandTimeout = 300

        $null = $SqlCommand.ExecuteNonQuery()
        $SqlConnection.Close()

        Write-Success "Database '$Database' created successfully."
        return $true
    }
    catch {
        Write-Error-Custom "Failed to create database: $($_.Exception.Message)"
        return $false
    }
}

# ================================================================================================
# Main Execution Flow
# ================================================================================================

# Test SQL Server connection
try {
    Write-Info "Testing SQL Server connection..."
    $SqlConnection = New-Object System.Data.SqlClient.SqlConnection
    $SqlConnection.ConnectionString = $ConnectionString + "Initial Catalog=master;"
    $SqlConnection.Open()
    $SqlConnection.Close()
    Write-Success "Connected to SQL Server successfully."
}
catch {
    Write-Error-Custom "Cannot connect to SQL Server at '$SqlServer': $($_.Exception.Message)"
    exit 1
}

# Create database if requested
if ($CreateDatabase) {
    if (-not (Create-Database)) {
        exit 1
    }
}

# Define script execution order (IMPORTANT: Order matters!)
# Master_Installation_Script.sql handles all DDL (tables, FK, indexes)
# This ensures idempotency - safe to run multiple times on new or existing databases
$Scripts = @(
    "Master_Installation_Script.sql",     # Comprehensive idempotent DDL + initial indexes
    "Initial_dml_Scripts.sql",            # Stored procedures (uses CREATE OR ALTER)
    "V4_ddl_Scripts.sql",                 # V4 schema updates (idempotent)
    "V4_dml_Scripts.sql",                 # V4 procedures (uses CREATE OR ALTER)
    "Performance_Optimization.sql",       # Performance indexes + optimized procedures
    "PDF_Export_StoredProcedures.sql"     # PDF generation procedures
)

Write-Info ""
Write-Info "Executing database setup scripts in order..."
Write-Info ""

$FailedScripts = @()

foreach ($Script in $Scripts) {
    $ScriptPath = Join-Path -Path $ScriptsPath -ChildPath $Script

    if (Test-Path $ScriptPath) {
        if (-not (Execute-SqlScript -ScriptPath $ScriptPath -CreateDb $false)) {
            $FailedScripts += $Script
        }
    }
    else {
        Write-Warning-Custom "Script not found (skipping): $Script"
    }
}

Write-Info ""
Write-Info "=========================================="

if ($FailedScripts.Count -eq 0) {
    Write-Success "Database setup completed successfully!"
    Write-Success "Connection String: $ConnectionString`nInitial Catalog=$Database;"
    exit 0
}
else {
    Write-Error-Custom "Setup completed with errors. Failed scripts:"
    foreach ($Script in $FailedScripts) {
        Write-Host "  - $Script" -ForegroundColor Red
    }
    exit 1
}
