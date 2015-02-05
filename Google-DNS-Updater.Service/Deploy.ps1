# Installs a TopShelf service using the default HostFactory definition (generally in Program.cs).
# If the service with the same name already exists it will be stopped and reconfigured to use
# the service executable from this deployed octopus package.
#
# see http://docs.topshelf-project.com/en/latest/overview/commandline.html
#
# These variables should be set via the Octopus web portal:
#
#   ServiceName         - Name of the Windows service
#   ServiceExecutable   - Path to the .exe containing the TopShelf service

# defaults
if (! $ServiceName) { $ServiceName = "Google-DNS-Updater" }
if (! $ServiceExecutable) { $ServiceExecutable = "Google-DNS-Updater.Service.exe" }

$fullPath = Resolve-Path $ServiceExecutable

$service = Get-Service $ServiceName -ErrorAction SilentlyContinue

if ($service)
{
    Write-Host "The service ($ServiceName) will be stopped and reconfigured for the new version..."

	Stop-Service $ServiceName | Write-Host

	& "$fullPath" uninstall -servicename:$ServiceName | Write-Host
}

Write-Host "The service ($ServiceName) is being (re)installed..."

& "$fullPath" install -servicename:$ServiceName -displayname:$ServiceName | Write-Host

Write-Host "(Re)Starting the service ($ServiceName)"

Start-Service $ServiceName