#!/bin/pwsh
$ErrorActionPreference = "Stop"
$CURRENTPATH = $pwd.Path

#nuget sign nupkg\*.nupkg -CertificatePath ${{ steps.cert_file.outputs.filePath }} -CertificatePassword ${{ secrets.CERT_PWD }}  -Timestamper http://timestamp.digicert.com –NonInteractive

#  PowerShell: Get-FileHash -Algorithm SHA256 .\my-public-certificate.cer
# nuget.exe: nuget.exe verify -Signatures .\my-signed-package.1.0.0.nupkg 

dotnet nuget sign TownSuite.Web.SSV3Facade\bin\Release\*.nupkg --certificate-path "$CURRENTPATH\..\certfile.pfx" --certificate-password "$env:CERTPASSWORD" --timestamper http://timestamp.digicert.com

dotnet nuget verify TownSuite.Web.SSV3Facade\bin\Release\*.nupkg
