$PSake.use_exit_on_error = $true

if(!$Configuration) { $Configuration = "Debug" }

$Here = "$(Split-Path -parent $MyInvocation.MyCommand.Definition)"

$SolutionRoot = (Split-Path -parent $Here)

Import-Module "$Here\Common" -DisableNameChecking

$SolutionFile = "$SolutionRoot\Google-DNS-Updater.sln"

## $ToolsDir = Join-Path $SolutionRoot "lib"

$NuGet = Join-Path $SolutionRoot ".nuget\nuget.exe"

$MSBuild ="${env:ProgramFiles(x86)}\MSBuild\12.0\Bin\msbuild.exe"

FormatTaskName (("-"*25) + "[{0}]" + ("-"*25))

Task default -depends Build

Task Build -depends Restore-Packages {
	exec { . $MSBuild $SolutionFile /t:Build /v:minimal /p:Configuration=$Configuration }
}

Task Clean {
	Remove-Item -Path "$SolutionRoot\packages\*" -Exclude repositories.config -Recurse -Force 
	Get-ChildItem .\ -include bin,obj -Recurse | foreach ($_) { Remove-Item $_.fullname -Force -Recurse }
	exec { . $MSBuild $SolutionFile /t:Clean /v:quiet }
}

Task Package -depends Build {
	## TODO
}

Task Configure {
	notepad "$SolutionRoot\Google-DNS-Updater.Service\bin\$Configuration\Google-DNS-Updater.Service.exe.config"

	Write-Host "Press any key to continue ... "

	$x = $host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
}

Task Install-Service -depends Build {
	exec { . "$SolutionRoot\Google-DNS-Updater.Service\bin\$Configuration\Google-DNS-Updater.Service.exe" install }
}

Task Start-Service {
	exec { . "$SolutionRoot\Google-DNS-Updater.Service\bin\$Configuration\Google-DNS-Updater.Service.exe" start }
}

Task Uninstall-Service -depends Build {
	exec { . "$SolutionRoot\Google-DNS-Updater.Service\bin\$Configuration\Google-DNS-Updater.Service.exe" uninstall }
}

Task Start -depends Install-Service, Configure, Start-Service

Task Restore-Packages -depends Install-BuildTools {
	exec { . $NuGet restore $SolutionFile }
}

Task Install-MSBuild {
    if(!(Test-Path "${env:ProgramFiles(x86)}\MSBuild\12.0\Bin\msbuild.exe")) 
	{ 
		cinst microsoft-build-tools
	}
}

Task Install-BuildTools -depends Install-MSBuild

# Borrowed from Luis Rocha's Blog (http://www.luisrocha.net/2009/11/setting-assembly-version-with-windows.html)
Task Update-AssemblyInfoFiles {
	$assemblyVersionPattern = 'AssemblyVersion\("[0-9]+(\.([0-9]+|\*)){1,3}"\)'
    $fileVersionPattern = 'AssemblyFileVersion\("[0-9]+(\.([0-9]+|\*)){1,3}"\)'
    $fileCommitPattern = 'AssemblyInformationalVersion\("(.*?)"\)'

    $assemblyVersion = 'AssemblyVersion("' + $Version + '")';
    $fileVersion = 'AssemblyFileVersion("' + $Version + '")';
    $commitVersion = 'AssemblyInformationalVersion("' + $InformationalVersion + '")';

    Get-ChildItem -path $SolutionRoot -r -filter GlobalAssemblyInfo.cs | ForEach-Object {
        $filename = $_.Directory.ToString() + '\' + $_.Name
        $filename + ' -> ' + $Version
    
        (Get-Content $filename) | ForEach-Object {
            % {$_ -replace $assemblyVersionPattern, $assemblyVersion } |
            % {$_ -replace $fileVersionPattern, $fileVersion } |
            % {$_ -replace $fileCommitPattern, $commitVersion }
        } | Set-Content $filename
    }
}