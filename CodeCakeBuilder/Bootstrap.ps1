<#
.SYNOPSIS
Builds CodeCakeBuilder, and downloads tools like NuGet.exe.

.DESCRIPTION
This script builds CodeCakeBuilder with the help of nuget.exe (in Tools/, downloaded if missing).
Requires Visual Studio 2017 and/or MSBuild.

.NOTES
You may move this Bootstrap.ps1 to the solution directory, or let it in CodeCakeBuilder folder:
The $solutionDir and $builderDir variables are automatically set.
PowerShell (and WMF). 5.0 is required. See https://msdn.microsoft.com/en-us/powershell/wmf/readme for availability.

Note that the following PowerShell modules and scripts will be installed or updated:
- VSSetup - https://github.com/Microsoft/vssetup.powershell
- Resolve-MSBuild - https://github.com/nightroman/Invoke-Build/blob/master/Resolve-MSBuild.ps1

.PARAMETER Run
If specified, runs CodeCakeBuilder immediately after building it.
Omit to only build CodeCakeBuilder without running it.

.EXAMPLE
./Bootstrap.ps1 -Verbose -InformationAction Continue

.EXAMPLE
./Bootstrap.ps1 -Run

#>
#Requires -Version 5
[CmdletBinding()]
Param(
    [Parameter(Mandatory=$false)]
    [switch] $Run
)
$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$nugetDownloadUrl = 'https://dist.nuget.org/win-x86-commandline/latest/nuget.exe'

# Go back a level if ./CodeCakeBuilder isn't found
$solutionDir = $PSScriptRoot
$builderDir = Join-Path $solutionDir "CodeCakeBuilder"
if (!(Test-Path $builderDir -PathType Container)) {
    $builderDir = $PSScriptRoot
    $solutionDir = Join-Path $builderDir ".."
}
Write-Information "Using solution directory: $solutionDir"
Write-Information "Using builder directory: $builderDir"

# Ensure CodeCakeBuilder project exists
$builderProj = Join-Path $builderDir "CodeCakeBuilder.csproj"
if (!(Test-Path $builderProj)) {
    Throw "Could not find $builderProj"
}
Write-Information "Using builder project: $builderProj"

# Ensure packages.config file exists.
$builderPackageConfig = Join-Path $builderDir "packages.config"
if (!(Test-Path $builderPackageConfig)) {
    Throw "Could not find $builderPackageConfig"
}

# Resolve MSBuild executable
# Accept MSBuild from PATH
$msbuildExe = "msbuild"
if (Get-Command $msbuildExe -ErrorAction SilentlyContinue) {
    Write-Information "Using MSBuild from PATH."
} else {
    Write-Information "Using MSBuild from local Visual Studio installation:"
    # Install required PowerShell modules
    Write-Information "Installing PackageProvider"
    Install-PackageProvider -Name NuGet -Scope CurrentUser -Force | Out-Null
    Write-Information "Installing module: VSSetup"
    Install-Module -Name VSSetup -Scope CurrentUser -Force
    Write-Information "Installing script: Resolve-MSBuild"
    Install-Script -Name Resolve-MSBuild -Scope CurrentUser -Force
    Write-Information "Calling: Resolve-MSBuild"
    $msbuildExe = Resolve-MSBuild
    Write-Information "Resolved MSBuild at: $msbuildExe"
    if (!(Test-Path $msbuildExe)) {
        Throw "MSBuild executable does not exist: $msbuildExe"
    }
}

# Tools directory is for nuget.exe but it may be used to 
# contain other utilities.
$toolsDir = Join-Path $builderDir "Tools"
New-Item -ItemType Directory $toolsDir -Force | Out-Null

# Try download NuGet.exe if do not exist.
$nugetExe = Join-Path $toolsDir "nuget.exe"
if (Test-Path $nugetExe) {
    Write-Information "Using existing nuget.exe: $nugetExe"
} else {
    Write-Information "Downloading nuget.exe from $nugetDownloadUrl"
    Invoke-WebRequest -Uri $nugetDownloadUrl -OutFile $nugetExe
}

$nugetConfigFile = Join-Path $solutionDir "NuGet.config"
& $nugetExe restore $builderPackageConfig -SolutionDirectory $solutionDir -configfile $nugetConfigFile

& $msbuildExe $builderProj /p:Configuration=Release

if($Run) {
    & "$builderDir/bin/Release/CodeCakeBuilder.exe"
}
