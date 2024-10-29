#!/usr/bin/env powershell
$ErrorActionPreference = "Stop"
$CURRENTPATH=$pwd.Path

function delete_files([string]$path)
{
	If (Test-Path $path){
		Write-Host "Deleting path $path" -ForegroundColor Green
		Remove-Item -recurse -force $path
	}
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
	nuget restore "TownSuite.Web.SSV3Adapter.sln" -NonInteractive
}

function build()
{
	Write-Host "build TownSuite.Web.SSV3Adapter.sln" -ForegroundColor Green
	& dotnet build -c Release TownSuite.Web.SSV3Adapter.sln
	if ($LastExitCode -ne 0) { throw "Building solution, TownSuite.Web.SSV3Adapter.sln, failed" }
}


function package_nugetpkg(){
	Write-Host "package_nugetpkg" -ForegroundColor Green

	Get-ChildItem -Recurse "$CURRENTPATH\*.nupkg" | Where-Object { $_.FullName -notmatch '\\packages\\' } | Copy-Item -Destination  "$CURRENTPATH/build"

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
