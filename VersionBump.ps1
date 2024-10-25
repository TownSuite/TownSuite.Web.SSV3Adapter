#!/usr/bin/env powershell

$CURRENTPATH=$pwd.Path
$branch_name =  $args[0]
$GITHUB_TOKEN =  $args[1]

Write-Host 'Automatic Version Bump processing .....' -ForegroundColor Green
New-Item -Path $CURRENTPATH -Force -Name 'SourceCode' -ItemType 'directory' 				
Set-Location SourceCode
try
{ 
    git init
    if ($branch_name -ne 'master') { git checkout -b $branch_name }
    $repoUrl = "https://${GITHUB_TOKEN}@github.com/TownSuite/TownSuite.Web.SSV3Adapter.git"
    git pull $repoUrl $branch_name --allow-unrelated-histories
    git status
    Write-Host 'Execute Web Version Bump script .....' -ForegroundColor Green
    Set-Location "$CURRENTPATH/SourceCode"
    ./PrepareVersionNumbers.ps1
    git add .
    git status 
    git commit --author="TownSuite CI <ci@townsuite.com>" -am "version bump"
    git push git@github.com:TownSuite/TownSuite.Tunnels.git $branch_name
    
}
catch
{
    throw "Automatic Version Bump failed !!!!!" 
}

if ($LASTEXITCODE  -ne 0) { throw "Automatic Version Bump failed !!!!!" }
Write-Host 'Automatic Version bump happened successfully.........................................................' -ForegroundColor Green
Set-Location $CURRENTPATH