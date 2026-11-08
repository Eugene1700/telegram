[CmdletBinding()]
param(
    [ValidatePattern('^\d+\.\d+\.\d+(-[0-9A-Za-z]+([.-][0-9A-Za-z]+)*)?$')]
    [string] $Version = '1.0.0-beta.1',
    [switch] $NoRestore
)

$ErrorActionPreference = 'Stop'
$repo = Split-Path $PSScriptRoot -Parent
$source = Join-Path $repo 'Telegram.Commands/src'
$output = Join-Path $repo "artifacts/nuget/$Version"
$projects = @('Abstract', 'Core', 'DependencyInjection', 'UI', 'Telegram', 'Max')
$restoreOptions = @()
if ($NoRestore) { $restoreOptions += '--no-restore' }

Push-Location $repo
try {
    dotnet test "$source/Telegram.Command.Tests/Bot.Commands.Core.Tests/Bot.Commands.Core.Tests/Bot.Commands.Core.Tests.csproj" -c Release "-p:Version=$Version" --nologo @restoreOptions
    if ($LASTEXITCODE -ne 0) { throw 'Release tests failed.' }

    foreach ($project in $projects) {
        dotnet pack "$source/Bot.Commands.$project/Bot.Commands.$project.csproj" -c Release "-p:Version=$Version" --output $output --nologo @restoreOptions
        if ($LASTEXITCODE -ne 0) { throw "Packing Bot.Commands.$project failed." }

        $package = Join-Path $output "Bot.Commands.$project.$Version.nupkg"
        $archive = [System.IO.Compression.ZipFile]::OpenRead($package)
        try {
            foreach ($entry in @('README.md', 'LICENSE', "lib/net10.0/Bot.Commands.$project.dll")) {
                if (-not $archive.GetEntry($entry)) { throw "$package is missing $entry" }
            }
        }
        finally { $archive.Dispose() }
        if (-not (Test-Path (Join-Path $output "Bot.Commands.$project.$Version.snupkg"))) {
            throw "Symbol package missing for Bot.Commands.$project"
        }
    }
    Write-Host "Packages prepared in $output"
}
finally {
    Pop-Location
}
