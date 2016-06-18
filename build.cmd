@echo off

pushd %~dp0

SETLOCAL
SET CACHED_NUGET=%LocalAppData%\NuGet\NuGet.exe

IF EXIST %CACHED_NUGET% goto copynuget
echo Downloading latest version of NuGet.exe...
IF NOT EXIST %LocalAppData%\NuGet md %LocalAppData%\NuGet
@powershell -NoProfile -ExecutionPolicy unrestricted -Command "$ProgressPreference = 'SilentlyContinue'; Invoke-WebRequest 'https://www.nuget.org/nuget.exe' -OutFile '%CACHED_NUGET%'"

:copynuget
IF EXIST source\.nuget\nuget.exe goto restore
md source\.nuget
copy %CACHED_NUGET% source\.nuget\nuget.exe > nul

:restore

source\.nuget\NuGet.exe update -self


pushd %~dp0

source\.nuget\NuGet.exe update -self

source\.nuget\NuGet.exe install FAKE -ConfigFile source\.nuget\Nuget.Config -OutputDirectory source\packages -ExcludeVersion -Version 4.16.1

source\.nuget\NuGet.exe install xunit.runner.console -ConfigFile source\.nuget\Nuget.Config -OutputDirectory source\packages\FAKE -ExcludeVersion -Version 2.0.0
source\.nuget\NuGet.exe install NBench.Runner -OutputDirectory source\packages -ExcludeVersion -Version 0.2.1
source\.nuget\NuGet.exe install Microsoft.SourceBrowser -OutputDirectory source\packages -ExcludeVersion

if not exist source\packages\SourceLink.Fake\tools\SourceLink.fsx (
  source\.nuget\nuget.exe install SourceLink.Fake -ConfigFile source\.nuget\Nuget.Config -OutputDirectory source\packages -ExcludeVersion
)
rem cls

set encoding=utf-8
rem source\packages\FAKE\tools\FAKE.exe build.fsx %*

popd
