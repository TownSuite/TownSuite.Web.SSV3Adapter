#!/usr/bin/env powershell
$ErrorActionPreference = "Stop"
$CURRENTPATH=$pwd.Path

function get_cpu_count(){
	$cs = Get-WmiObject -class Win32_ComputerSystem
	$cs.numberoflogicalprocessors
}


function build_from_docker(){
	# give 50 percent cpu to this docker build
	$cpuCount = (get_cpu_count)/2
	

	try{		
		& "docker" run --memory 4gb --cpus $cpuCount -e PROGET_USER="$env:PROGET_USER" -e PROGET_PASSWORD="$env:PROGET_PASSWORD" --rm -v ${CURRENTPATH}:c:/TownSuite.Web.SSV3Adapter townsuite/dotnet_windows_builder:8.0.2 'cd C:\TownSuite.Web.SSV3Adapter; ls; .\buildrelease.ps1; exit $LastExitCode'
		Write-Host "Error code $LastExitCode"
		if ( $LastExitCode -eq 99999 ){
			Write-Host "Error in build scripts, exiting..."				
			exit $LastExitCode
		}
		if ($LastExitCode -ne 0){
			throw "docker failed"
		}  
	}

	catch{
		if ( $LastExitCode -eq 99999 ){
			Write-Host "Error in build scripts, exiting..."
			Write-Host "Error code $LastExitCode"
			exit $LastExitCode
		}   

		if ( $i -eq 10){
			Write-Host "Docker failed"
			Write-Host "Error code $LastExitCode"
			exit $LastExitCode
		}
	}

	 
}

build_from_docker
exit 0
