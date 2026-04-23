; ============================================================
;  Fatima Church TBM - Inno Setup Installer Script
;  Packages the WinForms application and all dependencies
;  into a single self-extracting .exe installer.
;
;  Requirements: Inno Setup 6.x  (https://jrsoftware.org/isinfo.php)
;  To build: open this file in Inno Setup Compiler and press F9
; ============================================================

#define AppName      "Fatima Church TBM"
#define AppVersion   "1.0.0"
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
; Output installer to the project root
OutputDir=Installer
OutputBaseFilename=FatimaChurchTBM_Setup_v{#AppVersion}
SetupIconFile=
Compression=lzma2/ultra64
SolidCompression=yes
; Require administrator rights for install
PrivilegesRequired=admin
PrivilegesRequiredOverridesAllowed=dialog
; Minimum Windows version: Windows 7 (6.1)
MinVersion=6.1
WizardStyle=modern
; Uninstall display name and icon
UninstallDisplayName={#AppName}

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon";    Description: "{cm:CreateDesktopIcon}";    GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked
Name: "quicklaunchicon"; Description: "{cm:CreateQuickLaunchIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked; OnlyBelowVersion: 6.1

[Files]
; Main executable and config
Source: "{#BuildDir}\{#AppExeName}";         DestDir: "{app}"; Flags: ignoreversion
Source: "{#BuildDir}\{#AppExeName}.config";  DestDir: "{app}"; Flags: ignoreversion

; Syncfusion DLLs (required, no XML docs at runtime)
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

; Database setup scripts (for DBA reference, not auto-run)
Source: "Scripts\Initial_ddl_Scripts.sql";  DestDir: "{app}\Scripts"; Flags: ignoreversion
Source: "Scripts\Initial_dml_Scripts.sql";  DestDir: "{app}\Scripts"; Flags: ignoreversion
Source: "Scripts\V4_ddl_Scripts.sql";       DestDir: "{app}\Scripts"; Flags: ignoreversion
Source: "Scripts\V4_dml_Scripts.sql";       DestDir: "{app}\Scripts"; Flags: ignoreversion

[Icons]
Name: "{group}\{#AppName}";          Filename: "{app}\{#AppExeName}"
Name: "{group}\Uninstall {#AppName}"; Filename: "{uninstallexe}"
Name: "{autodesktop}\{#AppName}";    Filename: "{app}\{#AppExeName}"; Tasks: desktopicon
Name: "{userappdata}\Microsoft\Internet Explorer\Quick Launch\{#AppName}"; Filename: "{app}\{#AppExeName}"; Tasks: quicklaunchicon

[Run]
Filename: "{app}\{#AppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(AppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent

[Registry]
; Store install location for auto-update / diagnostics
Root: HKLM; Subkey: "Software\{#AppPublisher}\{#AppName}"; ValueType: string; ValueName: "InstallPath"; ValueData: "{app}"; Flags: uninsdeletekey
Root: HKLM; Subkey: "Software\{#AppPublisher}\{#AppName}"; ValueType: string; ValueName: "Version";     ValueData: "{#AppVersion}"

[Code]
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
    // 528040 = .NET 4.8 on Windows 10 and later
    Result := (Release >= 528040);
  end;
end;

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
