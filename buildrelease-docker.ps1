#!/usr/bin/env powershell
$CURRENTPATH=$pwd.Path

docker run --rm -v ${CURRENTPATH}:c:/TownSuite.Web.SSV3Adapter mcr.microsoft.com/dotnet/sdk:8.0 'cd C:\TownSuite.Web.SSV3Adapter; ls; .\buildrelease.ps1; exit $LastExitCode'

exit 0
