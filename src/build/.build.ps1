<#
.Synopsis
	Build script (https://github.com/nightroman/Invoke-Build)
#>

[cmdletBinding()]
param(
	$appName = "Syrup",
	$serverDir = "C:\work\users\AntyPiracy\",
	$toolsDir = ($BL.ToolsPath),
	$BHDir = ($BL.BHPath),
	$artifactsPath = ($BL.ArtifactsPath),
	$scriptsPath = $BL.ScriptsPath,
	$nuget = (Join-Path $toolsDir  "nuget/nuget.exe"),
	$libz = (Join-Path $toolsDir  "LibZ.Tool/tools/libz.exe"),
	$7zip = (Join-Path $toolsDir  "7-Zip.CommandLine/tools/7za.exe"),
	$xunit = (Join-Path $toolsDir  "xunit.runner.console/tools/net462/xunit.console.exe"),

	$srcDir = (Join-Path $BL.RepoRoot "src"),
	$sln  = (Join-Path $BL.RepoRoot  "/src/Syrup.sln" ),
	$buildTmpDir  = (Join-Path $BL.TmpPath "build" ),

	$buildReadyDir  = (Join-Path $BL.TmpPath "ready" ),
	$Dirs = (@{"marge" = "marge"; "build" = "build"; "nuget" = "nuget"; "main" = "main"; "syrup" = "syrup"}),
	$buildWorkDir  = (Join-Path $buildTmpDir "build" ),
	$target  = "Release",
	$donotMarge =  @(),
	$projectSyrup = @{
			name = "Syrup";
			marge = $true;
			file = (Join-Path $BL.RepoRoot  "/src/Syrup/Syrup.csproj" );
			exe = "Syrup.exe";
			dir = "Syrup";
			dstExe = "Syrup.exe";
	},
	$projectSyrupSelf = @{
			name = "SyrupSelf";
			marge = $false;
			file = (Join-Path $BL.RepoRoot  "/src/Syrup.Self/Syrup.Self.csproj" );
			exe = "Syrup.Self.exe";
			dir = "Syrup.Self";
			dstExe = "SyrupSelf.exe";
	},
	$projectScriptExecutor = @{
			name = "SyrupSelf";
			marge = $false;
			file = (Join-Path $BL.RepoRoot  "/src/Syrup.ScriptExecutor/Syrup.ScriptExecutor.csproj" );
			exe = "Syrup.ScriptExecutor.exe";
			dir = "Syrup.ScriptExecutor";
			dstExe = "Syrup.ScriptExecutor.exe";
	},
	$projectTests = @{
			name = "SyrupTests";
			marge = $false;
			file = (Join-Path $BL.RepoRoot  "/src/Syrup.Tests/Syrup.Tests.csproj" );
			exe = "";
			dir = "Syrup.Tests";
			dstExe = "";
			testFile = "Syrup.Tests.dll";
			testHtml = "Syrup.Tests.html";
			testXml = "Syrup.Tests.xml";
	},
	$projects = @($projectSyrupSelf, $projectSyrup, $projectScriptExecutor, $projectTests )
    )




# Msbuild 
Set-Alias MSBuild (Resolve-MSBuild)

# inser tools
. (Join-Path $BHDir "ps\misc.ps1")
. (Join-Path $BHDir "ps\io.ps1")
. (Join-Path $BHDir "ps\syrup.ps1")
. (Join-Path $BHDir "ps\assembly-tools.ps1")



# Synopsis: Update-TeamCity
task Update-BuildServer -If (($env:AGENT_NAME).Length -ne 0) {
		$tvc = $env:BUILD_BUILDNUMBER
		$s = $buildVersion.SemVer
		Write-Host "Build server version: $tvc" 
		Write-Host "Script version: $s" 
			
		Write-Host "##vso[release.updatereleasename]$s"
		#Write-Host "##vso[build.updatebuildnumber]my-new-build-number"
}


# Synopsis: Download tools if needed
task Get-Tools {

	Write-Build Green "Check: Nuget"
	DownloadIfNotExists "https://dist.nuget.org/win-x86-commandline/latest/nuget.exe" $nuget 
	DownloadNugetIfNotExists $nuget "LibZ.Tool" $toolsDir $libz
	DownloadNugetIfNotExists $nuget "7-Zip.CommandLine" $toolsDir $7zip
	DownloadNugetIfNotExists $nuget "xunit.runner.console" $toolsDir $xunit
}

# Synopsis: Package-Restore
task RestorePackages {

	Push-Location  $srcDir
	Write-Host $slnFile
	exec {  &$nuget restore $slnFile  }
	Pop-Location

}


# Synopsis: Remove temp files.
task Clean {

	Write-Host $buildTmpDir
	EnsureDirExistsAndIsEmpty $buildTmpDir
	Push-Location $buildTmpDir
	Remove-Item out -Recurse -Force -ErrorAction 0
	Pop-Location
}



# Synopsis: Make nuget file
task Pack-Nuget  {

	 "*** Make-Nuget  ***"
	
	$dirStart = (Join-Path $buildTmpDir $Dirs.marge  )
	$margedDir = (Join-Path $dirStart   $projectSyrup.dir )
	$nugetDir = (Join-Path $buildTmpDir $Dirs.nuget  )
	$mainDir = (Join-Path $nugetDir $Dirs.main  )
	$syrupDir = Join-Path $nugetDir "/_syrup"
	$syruScriptspDir = Join-Path $syrupDir "scripts"

	EnsureDirExistsAndIsEmpty $nugetDir
	EnsureDirExistsAndIsEmpty $mainDir
	EnsureDirExistsAndIsEmpty $syrupDir
	EnsureDirExistsAndIsEmpty $syruScriptspDir
	
	$src =  "$margedDir/*"
	$dst = $mainDir
	"Copy main; Src: $src ; Dst: $dst"
	Copy-Item  $src  -Recurse  -Destination $dst

	$src = "$scriptsPath/syrup/scripts/*"
	$dst = $syruScriptspDir
	"Copy scripts; Src: $src ; Dst: $dst"
	Copy-Item  $src -Destination $dst -Recurse


	$spacFilePath = Join-Path $scriptsPath "nuget\nuget.nuspec"
	$specFileOutPath = Join-Path $nugetDir "$appName.nuspec"
	
    
    $spec = [xml](get-content $spacFilePath)
    $spec.package.metadata.version = ([string]$spec.package.metadata.version).Replace("{Version}", $BL.BuildVersion.SemVer)
    $spec.Save($specFileOutPath )

	$readyDir =  Join-Path $buildReadyDir  $Dirs.nuget
	EnsureDirExistsAndIsEmpty $readyDir
    exec { &$nuget pack $specFileOutPath -OutputDirectory $readyDir  -NoPackageAnalysis}
	

}

task Make-Zip {
	## $projectSyrup
	$p = $projectSyrup
	$margedDir = [System.IO.Path]::Combine( $buildTmpDir , $Dirs.marge,  $p.dir )
	$readyDirZip =  Join-Path $buildReadyDir  $Dirs.syrup
	EnsureDirExistsAndIsEmpty $readyDirZip
	Set-Location  $margedDir
	$ver = $BL.BuildVersion.SemVer
	exec {  &$7zip  a -r -tzip $readyDirZip/syrup-$ver.zip *.exe }
}


task Generate-SyrupFile {
	$currentDir =  Join-Path $buildReadyDir  $Dirs.syrup
	$file =  ([System.IO.Directory]::GetFiles($currentDir , "*.zip"))[0]
	SyrupGenerateInfoFile $file $projectSyrup.name  $BL.BuildVersion.SemVer $BL.BuildVersion.BranchName $BL.BuildDateTime

	$currentDir =  Join-Path $buildReadyDir  $Dirs.nuget
	$file =  ([System.IO.Directory]::GetFiles($currentDir , "*.nupkg"))[0]
	SyrupGenerateInfoFile $file $projectSyrup.name $BL.BuildVersion.SemVer $BL.BuildVersion.BranchName $BL.BuildDateTime

}

task Copy-Update-TeamCity -If (($env:TEAMCITY_VERSION).Length -ne 0) {
	$readyDirZip =  Join-Path $buildReadyDir  $Dirs.syrup
	$dst= "C:\work\users\AntyPiracy\Syrup\syrup"
	$src = $readyDirZip
	Ensure-DirExists $dst
	cp  "$src/*" -Destination $dst 

}

task Copy-ToSyrupDir -If ($buildEnv -eq 'local')   {
	$readyDirZip =  Join-Path $buildReadyDir  $Dirs.syrup
	$dst= "C:\work\users\AntyPiracy\Syrup\syrup"
	$src = $readyDirZip
	Ensure-DirExists $dst
	cp  "$src/*" -Destination $dst 
}



function DownloadIfNotExists($src , $dst){

	If (-not (Test-Path $dst)){
		$dir = [System.IO.Path]::GetDirectoryName($dst)
		If (-not (Test-Path $dir)){
			New-Item -ItemType directory -Path $dir
		}
	 	Invoke-WebRequest $src -OutFile $dst
	} 
}


function BuildFn ($currentProjects){


	$outMain = (Join-Path $buildTmpDir $Dirs.build  )
	EnsureDirExists $buildWorkDir

	foreach ($p in $currentProjects ) {

				Write-Build Green "*** Build $($p.Name) *** "
				$out = (Join-Path $outMain  $p.dir )
			
				try {

					EnsureDirExistsAndIsEmpty $out 
					$projectFile = $p.file

					"Build; Project file: $projectFile"
					"Build; out dir: $out"
					"Build; Target: $target"
				
					$bv = $BL.BuildVersion

					"AssemblyVersion: $($bv.AssemblyVersion)"
					"AssemblyVersion: $($bv.AssemblyFileVersion)"
					"AssemblyVersion: $($bv.AssemblyInformationalVersion)"

					$srcWorkDir = Join-Path $srcDir $p.dir
					BackupTemporaryFiles $srcWorkDir  "Properties\AssemblyInfo.cs"
					UpdateAssemblyInfo $srcWorkDir $bv.AssemblyVersion $bv.AssemblyFileVersion $bv.AssemblyInformationalVersion $p.name "DenebLab" "DenebLab"
					exec { MSBuild $projectFile /v:quiet  /p:Configuration=$target /p:OutDir=$out   } 
				}

				catch {
					RestoreTemporaryFiles $srcWorkDir
					throw $_.Exception
					exit 1
				}
				finally {
					RestoreTemporaryFiles $srcWorkDir
				}
		}
}

function MargFn ($currentProjects){

	foreach ($p in $currentProjects ) {

		$buildDir = [System.IO.Path]::Combine( $buildTmpDir , $Dirs.build,  $p.dir )
		$margedDir = [System.IO.Path]::Combine( $buildTmpDir , $Dirs.marge,  $p.dir )
	
		Set-Location  $buildDir
		EnsureDirExistsAndIsEmpty $margedDir 

		$dlls = [System.IO.Directory]::GetFiles($buildDir, "*.dll")
		$exclude = $donotMarge | Foreach-Object { "--exclude=$_" }

		foreach ($f in  $dlls ){
			Copy-Item $f -Destination $margedDir
		}

		$mainFile = [System.IO.Path]::Combine( $buildDir, $p.exe )
		CopyIfExistsFn  ($mainFile) $margedDir 
		$configFile = [System.IO.Path]::Combine( $buildDir,  "$($p.exe).config" )
		CopyIfExistsFn  ($configFile) $margedDir 


	
		
		
		$src = "$scriptsPath/nlog/$($p.name).NLog.config"
		$dst = "$margedDir/NLog.config"
		
		if([System.IO.File]::Exists("NLog.config"))
		{
			"Copy fixed version NLog.config; Src: $src ; Dst: $dst "
			Copy-Item  $src -Destination $dst -Force		
		} else {
			"Can not find fixed version NLog.config; Src: $src ; Dst: $dst "
		}


		
		if([System.IO.File]::Exists("NLog.config")){
			Copy-Item "NLog.config"  -Destination $margedDir
		}
		Set-Location  $margedDir
		"Marge in dir: $margedDir"
		$appConfigFile = $p.exe + ".config"
		If (-not (Test-Path $appConfigFile)){
			
			& $libz inject-dll --assembly $p.exe --include *.dll  $exclude --move 
		} else {
			"App config path exists: $appConfigFile"
			& $libz inject-dll --assembly $p.exe -b $appConfigFile --include *.dll  $exclude --move 
		}

		
		
	
	}

}

function TestdFn ($currentProjects){


	$outMain = (Join-Path $buildTmpDir $Dirs.build  )
	
	foreach ($p in $currentProjects ) {

				Write-Build Green "*** Test $($p.Name) *** "
				$out = (Join-Path $outMain  $p.dir )
				$testResultHtml = (Join-Path $out  $p.testHtml )
			    $testResultXml = (Join-Path $out  $p.testXml )
				$testFile = (Join-Path $out   $p.testFile )
				Write-Host $out 
				Write-Host $testFile
				Write-Host $testResultHtml 

			
				try {
			
					exec {
						& $xunit $testFile -html $testResultHtml -xml $testResultXml 
					}
				}

				catch {
					#throw $_.Exception
					#exit 1
				}
		}
}

function CopyIfExistsFn($src, $dst){

	if([System.IO.File]::Exists($src)){
		"Copy '$src' to '$dst'"
		Copy-Item $src  -Destination $dst
	} else {
		"Can't copy: $src - not found"
	}
	
}



task Build-SyrupSelf {
	$currentProjects  = @( $projectSyrupSelf  )
	BuildFn $currentProjects
}

task  Marge-SyrupSyrup  {
	$currentProjects  = @( $projectSyrupSelf  )
	MargFn $currentProjects
}


task Build-ScriptExecutor {
	$currentProjects  = @( $projectScriptExecutor  )
	BuildFn $currentProjects
}

task  Marge-ScriptExecutor  {
	$currentProjects  = @( $projectScriptExecutor   )
	MargFn $currentProjects
}

task Components-Copy {

	$dstMain = (Join-Path $BL.RepoRoot  "/src/Syrup/Embed/" )
	EnsureDirExists $dstMain 

	# self
	$src = [System.IO.Path]::Combine( $buildTmpDir , $Dirs.marge,  $projectSyrupSelf.dir, $projectSyrupSelf.exe  )
	$dst = (Join-Path $dstMain "syrup-self.bin")
	cp  "$src" -Destination $dst -Force


	# executor
	$src = [System.IO.Path]::Combine( $buildTmpDir , $Dirs.marge,  $projectScriptExecutor.dir, $projectScriptExecutor.exe  )
	$dst = (Join-Path $dstMain "syrup-executor.bin")
	cp  "$src" -Destination $dst -Force

}

task Build-Main {
	$currentProjects  = @( $projectSyrup  )
	BuildFn $currentProjects
}



task  Marge-Main  {
	$currentProjects  = @( $projectSyrup   )
	MargFn $currentProjects
}


task Build-Tests {
	$currentProjects  = @(  $projectTests  )
	BuildFn $currentProjects
}


task Run-Tests {
	$currentProjects  = @(  $projectTests  )
	TestdFn $currentProjects
}


task Copy-Artifacts {

	$buildDir = [System.IO.Path]::Combine( $buildTmpDir , $Dirs.build )
	EnsureDirExistsAndIsEmpty $artifactsPath
	
	$dst = "$artifactsPath/syrup/"
	EnsureDirExists $dst
	Copy-Item  "$buildReadyDir/nuget/*" -Destination $dst -Force

	
	$dst = "$artifactsPath/zip/"
	EnsureDirExists $dst
	Copy-Item  "$buildReadyDir/syrup/*.zip" -Destination $dst -Force

	$dst = "$artifactsPath/docs/"
	EnsureDirExists $dst
	Copy-Item  "$buildDir/Syrup.Tests/Syrup.Tests.html" -Destination $dst -Force
	Copy-Item  "$buildDir/Syrup.Tests/Syrup.Tests.xml" -Destination $dst -Force

}


# Synopsis: All the things
task Init Update-BuildServer, Clean, Get-Tools, RestorePackages
task Syrup-Self Build-SyrupSelf, Marge-SyrupSyrup 
task Syrup-ScriptExecutor  Build-ScriptExecutor, Marge-ScriptExecutor
task Components Syrup-Self, Syrup-ScriptExecutor, Components-Copy
task Syrup-Main Build-Main, Marge-Main
task Syrup-Tests Build-Tests, Run-Tests 
task Prepare-Release   Pack-Nuget, Make-Zip, Generate-SyrupFile, Copy-Update-TeamCity, Copy-ToSyrupDir, Copy-Artifacts
task . Init, Components, Syrup-Main, Syrup-Tests, Prepare-Release
