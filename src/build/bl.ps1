#requires -Version 3.0
<#
.Synopsis
	Build luncher for (https://github.com/nightroman/Invoke-Build)
	This script create spacial variable $BL
#>

param(
	$scriptFile = (Join-Path $PSScriptRoot ".build.ps1"),
	$major = 0,
	$minor = 0,
	$patch = 0,
	$buildCounter = 0,
	$psGitVersionStrategy = "combinateMajorAndBuildSlice3"
)



# 
$BL = @{}
$BL.RepoRoot = (Resolve-Path ( & git rev-parse --show-toplevel))
$BL.BuildDateTime = ((Get-Date).ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ"))
$BL.ScriptsPath = (Split-Path $MyInvocation.MyCommand.Path -Parent)
$BL.TmpPath = (Join-Path $BL.RepoRoot "_tmp" )
$BL.ArtifactsPath = (Join-Path $BL.RepoRoot "_artifacts" )
$BL.ToolsPath = (Join-Path $BL.RepoRoot "tools" )
$BL.BHPath = (Join-Path $BL.ToolsPath  "dev-helpers" )
$BL.BuildScriptPath = $scriptFile
$BL.PsAutoHelpers = (Join-Path $BL.BHPath "ps") 
$BL.ib = (Join-Path $BL.ToolsPath  "Invoke-Build\tools\Invoke-Build.ps1")

# import tools
. (Join-Path $BL.PsAutoHelpers "psgitversion.ps1")



$BL.BuildVersion = Get-GitVersion $psGitVersionStrategy $major $minor $patch $buildCounter

Write-Output "`$BL values"
$BL.GetEnumerator()| Sort-Object -Property name | Format-Table Name, Value -AutoSize

try {

	# Invoke the build and keep results in the variable Result
	& $BL.ib -File $BL.BuildScriptPath -Result Result  @args
}
catch {
	Write-Output $Result.Error
	Write-Output $_
	exit 1 # Failure
}

$Result.Tasks | Format-Table Elapsed, Name -AutoSize
exit 0
