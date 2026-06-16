; ============================================================
;  Fatima Church TBM - Complete Installer with Database Setup
;  Inno Setup 6.x (https://jrsoftware.org/isinfo.php)
;
;  Features:
;  - Packages application + all dependencies
;  - Custom database configuration dialog
;  - Automatic database creation and schema setup
;  - Updates connection string in App.config
;
;  To build: Open in Inno Setup Compiler and press F9
; ============================================================

#define AppName      "Fatima Church TBM"
#define AppVersion   "1.0.1"
#define AppPublisher "Fatima Church"
#define AppExeName   "FatimaChurch.exe"
#define BuildDir     "bin\Debug"

[Setup]
AppId={{A1B2C3D4-E5F6-7890-ABCD-EF1234567890}
AppName={#AppName}
AppVersion={#AppVersion}
AppVerName={#AppName} {#AppVersion}
AppPublisher={#AppPublisher}
DefaultDirName={autopf}\{#AppName}
DefaultGroupName={#AppName}
AllowNoIcons=no
OutputDir=Installer
OutputBaseFilename=FatimaChurchTBM_Setup_v{#AppVersion}
SetupIconFile=
Compression=lzma2/ultra64
SolidCompression=yes
PrivilegesRequired=admin
PrivilegesRequiredOverridesAllowed=dialog
MinVersion=6.1
WizardStyle=modern
UninstallDisplayName={#AppName}
; Important: Allow for custom pages
DisableWelcomePage=no

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon";    Description: "{cm:CreateDesktopIcon}";    GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked
Name: "quicklaunchicon"; Description: "{cm:CreateQuickLaunchIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked; OnlyBelowVersion: 6.1

[Files]
; Main executable and config
Source: "{#BuildDir}\{#AppExeName}";         DestDir: "{app}"; Flags: ignoreversion
Source: "App.config.template";                DestDir: "{app}"; DestName: "FatimaChurch.exe.config"; Flags: ignoreversion

; Syncfusion DLLs (required)
Source: "{#BuildDir}\Syncfusion.Chart.Base.dll";              DestDir: "{app}"; Flags: ignoreversion
Source: "{#BuildDir}\Syncfusion.Chart.Windows.dll";           DestDir: "{app}"; Flags: ignoreversion
Source: "{#BuildDir}\Syncfusion.Compression.Base.dll";        DestDir: "{app}"; Flags: ignoreversion
Source: "{#BuildDir}\Syncfusion.Core.WinForms.dll";           DestDir: "{app}"; Flags: ignoreversion
Source: "{#BuildDir}\Syncfusion.DocIO.Base.dll";              DestDir: "{app}"; Flags: ignoreversion
Source: "{#BuildDir}\Syncfusion.Grid.Base.dll";               DestDir: "{app}"; Flags: ignoreversion
Source: "{#BuildDir}\Syncfusion.Grid.Grouping.Windows.dll";   DestDir: "{app}"; Flags: ignoreversion
Source: "{#BuildDir}\Syncfusion.Grid.Windows.dll";            DestDir: "{app}"; Flags: ignoreversion
Source: "{#BuildDir}\Syncfusion.GridHelperClasses.Windows.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#BuildDir}\Syncfusion.Gridconverter.Windows.dll";   DestDir: "{app}"; Flags: ignoreversion
Source: "{#BuildDir}\Syncfusion.Grouping.Base.dll";           DestDir: "{app}"; Flags: ignoreversion
Source: "{#BuildDir}\Syncfusion.Licensing.dll";               DestDir: "{app}"; Flags: ignoreversion
Source: "{#BuildDir}\Syncfusion.Linq.Base.dll";               DestDir: "{app}"; Flags: ignoreversion
Source: "{#BuildDir}\Syncfusion.Markdown.dll";                DestDir: "{app}"; Flags: ignoreversion
Source: "{#BuildDir}\Syncfusion.OfficeChart.Base.dll";        DestDir: "{app}"; Flags: ignoreversion
Source: "{#BuildDir}\Syncfusion.Pdf.Base.dll";                DestDir: "{app}"; Flags: ignoreversion
Source: "{#BuildDir}\Syncfusion.PivotAnalysis.Base.dll";      DestDir: "{app}"; Flags: ignoreversion
Source: "{#BuildDir}\Syncfusion.PivotAnalysis.Windows.dll";   DestDir: "{app}"; Flags: ignoreversion
Source: "{#BuildDir}\Syncfusion.SfInput.WinForms.dll";        DestDir: "{app}"; Flags: ignoreversion
Source: "{#BuildDir}\Syncfusion.Shared.Base.dll";             DestDir: "{app}"; Flags: ignoreversion
Source: "{#BuildDir}\Syncfusion.Shared.Windows.dll";          DestDir: "{app}"; Flags: ignoreversion
Source: "{#BuildDir}\Syncfusion.SpellChecker.Base.dll";       DestDir: "{app}"; Flags: ignoreversion
Source: "{#BuildDir}\Syncfusion.Tools.Base.dll";              DestDir: "{app}"; Flags: ignoreversion
Source: "{#BuildDir}\Syncfusion.Tools.Windows.dll";           DestDir: "{app}"; Flags: ignoreversion
Source: "{#BuildDir}\Syncfusion.XlsIO.Base.dll";              DestDir: "{app}"; Flags: ignoreversion

; Resource files
Source: "{#BuildDir}\Resources\anbiyam_map.html"; DestDir: "{app}\Resources"; Flags: ignoreversion

; Database setup scripts - CRITICAL FOR INSTALLATION
Source: "Scripts\Initial_ddl_Scripts.sql";        DestDir: "{app}\Scripts"; Flags: ignoreversion
Source: "Scripts\Initial_dml_Scripts.sql";        DestDir: "{app}\Scripts"; Flags: ignoreversion
Source: "Scripts\V4_ddl_Scripts.sql";             DestDir: "{app}\Scripts"; Flags: ignoreversion
Source: "Scripts\V4_dml_Scripts.sql";             DestDir: "{app}\Scripts"; Flags: ignoreversion
Source: "Scripts\Performance_Optimization.sql";   DestDir: "{app}\Scripts"; Flags: ignoreversion
Source: "Scripts\PDF_Export_StoredProcedures.sql"; DestDir: "{app}\Scripts"; Flags: ignoreversion
Source: "Scripts\DatabaseSetup.ps1";              DestDir: "{app}\Scripts"; Flags: ignoreversion
Source: "Scripts\DatabaseSetup.bat";              DestDir: "{app}\Scripts"; Flags: ignoreversion

[Icons]
Name: "{group}\{#AppName}";          Filename: "{app}\{#AppExeName}"
Name: "{group}\Uninstall {#AppName}"; Filename: "{uninstallexe}"
Name: "{autodesktop}\{#AppName}";    Filename: "{app}\{#AppExeName}"; Tasks: desktopicon
Name: "{userappdata}\Microsoft\Internet Explorer\Quick Launch\{#AppName}"; Filename: "{app}\{#AppExeName}"; Tasks: quicklaunchicon

[Registry]
; Store install location and database configuration
Root: HKLM; Subkey: "Software\{#AppPublisher}\{#AppName}"; ValueType: string; ValueName: "InstallPath"; ValueData: "{app}"; Flags: uninsdeletekey
Root: HKLM; Subkey: "Software\{#AppPublisher}\{#AppName}"; ValueType: string; ValueName: "Version"; ValueData: "{#AppVersion}"
Root: HKLM; Subkey: "Software\{#AppPublisher}\{#AppName}"; ValueType: string; ValueName: "SqlServer"; ValueData: "{code:GetSqlServer}"; Flags: deletevalue
Root: HKLM; Subkey: "Software\{#AppPublisher}\{#AppName}"; ValueType: string; ValueName: "Database"; ValueData: "{code:GetDatabaseName}"; Flags: deletevalue

[Run]
; Run database setup before starting the application
Filename: "{app}\Scripts\DatabaseSetup.bat"; Parameters: """{code:GetSqlServer}"" ""{code:GetDatabaseName}"" ""sa"" ""Cts190588!1"" ""{app}\Scripts"""; Description: "Setting up database..."; Flags: runhidden waituntilterminated
; Launch application
Filename: "{app}\{#AppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(AppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent

[Code]
// Global variables for database configuration
var
  SqlServerPage: TInputQueryWizardPage;
  DatabaseConfigPage: TInputQueryWizardPage;
  SqlServerValue: String;
  DatabaseValue: String;
  UsernameValue: String;
  PasswordValue: String;
  TestConnectionOK: Boolean;

// Check that .NET Framework 4.8 is installed
function IsDotNetInstalled(): Boolean;
var
  Release: Cardinal;
begin
  Result := False;
  if RegQueryDWordValue(HKLM,
      'SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full',
      'Release', Release) then
  begin
    Result := (Release >= 528040);
  end;
end;

// Test SQL Server connection using sqlcmd
function TestSqlConnection(SqlServer, Database, Username, Password: String): Boolean;
var
  ResultCode: Integer;
  CmdLine: String;
begin
  Result := False;
  try
    // Build command to test connection
    CmdLine := 'sqlcmd -S "' + SqlServer + '" -U "' + Username + '" -P "' + Password +
               '" -Q "SELECT 1" -b -e';

    if Exec(ExpandConstant('{cmd}'), '/c ' + CmdLine, '', SW_HIDE, ewWaitUntilTerminated, ResultCode) then
    begin
      Result := (ResultCode = 0);
    end;
  except
    Result := False;
  end;
end;

// Create custom page for SQL Server configuration
procedure CreateSqlServerPage();
begin
  SqlServerPage := CreateInputQueryPage(wpSelectDir,
    'Database Configuration',
    'SQL Server Settings',
    'Please specify your SQL Server instance details:');

  SqlServerPage.Add('SQL Server Instance:', False);
  SqlServerPage.Add('Database Name:', False);

  SqlServerPage.Values[0] := '(localdb)\MSSQLLocalDB';
  SqlServerPage.Values[1] := 'fatimachurchtbm';
end;

// Create custom page for database credentials
procedure CreateDatabaseCredentialsPage();
begin
  DatabaseConfigPage := CreateInputQueryPage(wpSelectDir,
    'Database Credentials',
    'Database User Account',
    'Enter the SQL Server credentials for database setup:');

  DatabaseConfigPage.Add('Username (typically sa):', False);
  DatabaseConfigPage.Add('Password:', True);

  DatabaseConfigPage.Values[0] := 'sa';
  DatabaseConfigPage.Values[1] := 'Cts190588!1';
end;

// Get SQL Server value for use in [Run] section
function GetSqlServer(Param: String): String;
begin
  Result := SqlServerValue;
end;

// Get Database Name value for use in [Run] section
function GetDatabaseName(Param: String): String;
begin
  Result := DatabaseValue;
end;

// Initialize setup
function InitializeSetup(): Boolean;
begin
  if not IsDotNetInstalled() then
  begin
    MsgBox(
      'This application requires Microsoft .NET Framework 4.8 or later.' + #13#10 +
      'Please install it from:' + #13#10 +
      'https://dotnet.microsoft.com/download/dotnet-framework/net48' + #13#10#10 +
      'Setup will now exit.',
      mbCriticalError, MB_OK);
    Result := False;
  end
  else
    Result := True;
end;

// Called before showing page
procedure InitializeWizard();
begin
  CreateSqlServerPage();
  CreateDatabaseCredentialsPage();
end;

// Handle page transitions
function NextButtonClick(CurPageID: Integer): Boolean;
var
  ConnectResult: String;
begin
  Result := True;

  if CurPageID = SqlServerPage.ID then
  begin
    SqlServerValue := SqlServerPage.Values[0];
    DatabaseValue := SqlServerPage.Values[1];

    if SqlServerValue = '' then
    begin
      MsgBox('SQL Server instance cannot be empty.', mbError, MB_OK);
      Result := False;
      Exit;
    end;

    if DatabaseValue = '' then
    begin
      MsgBox('Database name cannot be empty.', mbError, MB_OK);
      Result := False;
      Exit;
    end;

    // Show next page for credentials
  end
  else if CurPageID = DatabaseConfigPage.ID then
  begin
    UsernameValue := DatabaseConfigPage.Values[0];
    PasswordValue := DatabaseConfigPage.Values[1];

    if UsernameValue = '' then
    begin
      MsgBox('Username cannot be empty.', mbError, MB_OK);
      Result := False;
      Exit;
    end;

    MsgBox('Database will be initialized after installation.' + #13#10 +
           'SQL Server: ' + SqlServerValue + #13#10 +
           'Database: ' + DatabaseValue, mbInformation, MB_OK);
  end;
end;

// Update App.config with actual connection string after installation
procedure CurStepChanged(CurStep: TSetupStep);
var
  ConfigPath: String;
  ConfigContent: String;
begin
  if CurStep = ssPostInstall then
  begin
    ConfigPath := ExpandConstant('{app}\FatimaChurch.exe.config');

    if FileExists(ConfigPath) then
    begin
      // Read template
      LoadStringFromFile(ConfigPath, ConfigContent);

      // Replace placeholders with actual values
      StringChangeEx(ConfigContent, '{SQL_SERVER}', SqlServerValue, True);
      StringChangeEx(ConfigContent, '{DATABASE_NAME}', DatabaseValue, True);
      StringChangeEx(ConfigContent, '{DB_USERNAME}', UsernameValue, True);
      StringChangeEx(ConfigContent, '{DB_PASSWORD}', PasswordValue, True);

      // Write updated config
      SaveStringToFile(ConfigPath, ConfigContent, False);
    end;
  end;
end;
