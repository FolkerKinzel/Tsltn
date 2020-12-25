; Script generated by the Inno Script Studio Wizard.
; SEE THE DOCUMENTATION FOR DETAILS ON CREATING INNO SETUP SCRIPT FILES!

#define MyAppSetupName 'TsltnSetup'
#define MyAppVersion "2.1.0.0"

#define MyAppName "Tsltn"
#define MyAppPublisher "Folker Kinzel"
#define MyAppURL "https://github.com/FolkerKinzel/Tsltn"
#define MyAppExeName "Tsltn.exe"
#define MyAppNameExt ".tsltn"
#define MyDeveloperDeploymentDirectory "{userpf}\Folker_Kinzel"

#define MyAppProjectDirectory "C:\Users\fkinz\source\repos\Tsltn\Tsltn"

[Setup]
; NOTE: The value of AppId uniquely identifies this application.
; Do not use the same AppId value in installers for other applications.
; (To generate a new GUID, click Tools | Generate GUID inside the IDE.)
AppId={{1F9AD9CF-6AD6-4F70-A8D3-F96B8D21028B}
VersionInfoVersion={#MyAppVersion}
VersionInfoDescription=Tsltn Setup for Windows x64 (contains the runtime)
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppVerName={#MyAppName} {#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}
AppUpdatesURL={#MyAppURL}
DefaultDirName={#MyDeveloperDeploymentDirectory}\{#MyAppName}
UninstallDisplayIcon={app}\{#MyAppExeName}
SourceDir=.
DefaultGroupName={#MyAppName}

LicenseFile={#MyAppProjectDirectory}\Installer\LICENSE.rtf
OutputDir={#MyAppProjectDirectory}\Installer\bin\Standalone
OutputBaseFilename={#MyAppSetupName}
SetupIconFile={#MyAppProjectDirectory}\Installer\Tsltn.ico
Compression=lzma
SolidCompression=yes

;AllowNoIcons=yes
DisableDirPage = yes
DisableFinishedPage = yes
DisableProgramGroupPage = yes
AlwaysShowGroupOnReadyPage = true

;MinVersion=10.0.10240
PrivilegesRequired=lowest
AlwaysUsePersonalGroup=yes
;UsedUserAreasWarning=false
ChangesAssociations = yes
ArchitecturesAllowed=x64
ArchitecturesInstallIn64BitMode=x64


[Languages]
Name: "en"; MessagesFile: "compiler:Default.isl,CustomMessages.isl"
Name: "de"; MessagesFile: "compiler:Languages\German.isl,CustomMessages.de.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked
;Name: "quicklaunchicon"; Description: "{cm:CreateQuickLaunchIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked; OnlyBelowVersion: 0,6.1
Name: "fileassoc"; Description: "{cm:AssocFileExtension,{#MyAppName},{#MyAppNameExt}}"; GroupDescription: "{cm:AssocingFileExtension,{#MyAppName},{#MyAppNameExt}}"

[Files]
Source: "{#MyAppProjectDirectory}\Binaries\Standalone\x64\Tsltn.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#MyAppProjectDirectory}\Installer\TsltnFileIcon.ico"; DestDir: "{app}"; Flags: ignoreversion
; NOTE: Don't use "Flags: ignoreversion" on any shared system files

[Icons]
;HINWEIS: "{cm:StartMenuToolTip}" ist - sprachabh�ngig - in CustomMessages.isl und CustomMessages.de.isl im Ordner des iss-Scripts definiert! 
Name: "{userprograms}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Comment: "{cm:StartMenuToolTip}"
;Name: "{group}\{cm:ProgramOnTheWeb,{#MyAppName}}"; Filename: "{#MyAppURL}"
Name: "{commondesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon
;Name: "{userappdata}\Microsoft\Internet Explorer\Quick Launch\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: quicklaunchicon

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent


[Registry]
;Schl�ssel f�r die assoziierte Dateiendung:
Root: HKCU; Subkey: "Software\Classes\{#MyAppNameExt}"; ValueType: string; ValueName: ""; ValueData: "FolkerKinzel.TsltnFile"; Flags: uninsdeletevalue; Tasks: fileassoc

;Schl�ssel zum �ffnen der assoziierten Dateiendung:
Root: HKCU; Subkey: "Software\Classes\FolkerKinzel.TsltnFile"; ValueType: string; ValueName: ""; ValueData: "{cm:TsltnFileTypeDescription}"; Flags: uninsdeletekey; Tasks: fileassoc
Root: HKCU; Subkey: "Software\Classes\FolkerKinzel.TsltnFile\DefaultIcon"; ValueType: string; ValueName: ""; ValueData: "{app}\TsltnFileIcon.ico"; Tasks: fileassoc
Root: HKCU; Subkey: "Software\Classes\FolkerKinzel.TsltnFile\shell\open\command"; ValueType: string; ValueName: ""; ValueData: """{app}\{#MyAppExeName}"" ""%1"""; Tasks: fileassoc


[UninstallDelete]
Type: files; Name: "{app}\*.*.RF.txt"
Type: dirifempty; Name: "{app}"
Type: dirifempty; Name: "{#MyDeveloperDeploymentDirectory}"