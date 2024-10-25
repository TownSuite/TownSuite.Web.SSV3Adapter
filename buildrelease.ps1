#!/usr/bin/env powershell
$ErrorActionPreference = "Stop"
$CURRENTPATH=$pwd.Path
$MSBUILD = "C:\BuildTools\MSBuild\Current\Bin\MSBuild.exe"
If (Test-Path "C:\BuildTools\MSBuild\Current\Bin\MSBuild.exe") {
	$MSBUILD = "C:\BuildTools\MSBuild\Current\Bin\MSBuild.exe"
}
ElseIf (Test-Path "C:\BuildTools\MSBuild\17.0\Bin\MSBuild.exe") {
	$MSBUILD = "C:\BuildTools\MSBuild\17.0\Bin\MSBuild.exe"
}
Else {
	$MSBUILD = "C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe"
}

function delete_files([string]$path)
{
	If (Test-Path $path){
		Write-Host "Deleting path $path" -ForegroundColor Green
		Remove-Item -recurse -force $path
	}
}

function AddInternalNugetSource
{
	NuGet.exe sources add -Name "CI Server" -Source "https://gander.dev.townsuite.com/nuget/Procom" -User "$env:PROGET_USER" -pass "$env:PROGET_PASSWORD"
}

function clean_bin_obj()
{
	cd "$CURRENTPATH"
	# DELETE ALL "BIN" and "OBJ" FOLDERS
	get-childitem -Include bin -Recurse -force | Remove-Item -Force -Recurse
	get-childitem -Include obj -Recurse -force | Remove-Item -Force -Recurse
}

function clean_build()
{
	# DELETE ALL ITEMS IN "BUILD" OUTPUT FOLDER
	Write-Host "Begin clean" -ForegroundColor Green
	if(!(Test-Path "build")){
		mkdir build
	}
	cd build
	Remove-Item * -Recurse -Force
	cd ..

	clean_bin_obj
}

function nuget_restore()
{
	AddInternalNugetSource
	nuget restore "TownSuite.Web.SSV3Adapter.sln" -NonInteractive
}

function build()
{
	Write-Host "build TownSuite.Web.SSV3Adapter.sln" -ForegroundColor Green
	& "$MSBUILD" "TownSuite.Web.SSV3Adapter.sln" /verbosity:minimal /property:Configuration="Release" /property:Platform="Any CPU"
	if ($LastExitCode -ne 0) { throw "Building solution, TownSuite.Web.SSV3Adapter.sln, failed" }
}


function package_nugetpkg(){
	Write-Host "package_nugetpkg" -ForegroundColor Green

	Copy-Item "$CURRENTPATH\TownSuite.Web.SSV3Adapter\bin\Release\*.nupkg" -Destination "$CURRENTPATH/build" -Force
	Copy-Item "$CURRENTPATH\TownSuite.Web.SSV3Adapter.Interfaces\bin\Release\*.nupkg" -Destination "$CURRENTPATH/build" -Force
	Copy-Item "$CURRENTPATH\TownSuite.Web.SSV3Adapter.Prometheus\bin\Release\*.nupkg" -Destination "$CURRENTPATH/build" -Force

	cd "$CURRENTPATH"
	$pwd.Path
}


function package_nunit()
{
	Write-Host "package_nunit" -ForegroundColor Green
	cd "$CURRENTPATH/TownSuite.Web.Tests/bin"

	7za a -t7z "$CURRENTPATH/build/NUnitTests.7z" Release -x!"*.pdb" -x!"*.xml" -bd
	if ($LastExitCode -ne 0) { throw "failed, NUnitTests.7z" }

	cd "$CURRENTPATH"
	$pwd.Path
}


if ( ($args.Count -ne 1) -or ($args[0] -eq 'build') )
{
	try{
		clean_build
		nuget_restore
		build
		package_nugetpkg
		package_nunit
			try{
				clean_bin_obj
			}
			catch{
				Write-Host "Failed to clean up bin/obj folders after bulid."
			}
	}
	catch{
			try{
				clean_bin_obj
			}
			catch{
				Write-Host "Failed to clean up bin/obj folders after bulid."
			}
		exit 99999
	}
}


exit 0
