@echo off
rem "Version-switcher" batch file to allow upgrade of tooling to VS2013
set msbld="C:\Windows\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe"
rem set msbld="C:\Program Files (x86)\MSBuild\12.0\Bin\amd64\MSBuild.exe" -- for VS2013 tooling

%msbld% %~dpn0.proj %*
