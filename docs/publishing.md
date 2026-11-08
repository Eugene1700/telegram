# Publishing to NuGet.org

## Prepare and validate

Use the .NET 10 SDK and PowerShell. From the repository root:

```powershell
./scripts/pack.ps1 -Version 1.0.0-beta.1
```

This runs the Release tests and creates six `.nupkg` files and six `.snupkg`
symbol packages in `artifacts/nuget/1.0.0-beta.1`. Samples and tests are not
packable. Ordinary builds do not produce packages. The script never uploads.
Use `-NoRestore` only when dependencies have already been restored for the chosen version.

The shared metadata is in `Telegram.Commands/src/Directory.Build.props`.
Use one version for the complete package family. Keep Max releases prerelease
while they depend on the prerelease MaxMessenger.Bot SDK; NuGet does not accept
stable packages with prerelease dependencies.

Before uploading:

1. Check that your NuGet.org account can publish all six package IDs and that
   the chosen version does not already exist. Package ownership is not verified
   by a successful local pack.
2. Review the source changes, MIT license, dependency versions and warnings.
3. Inspect the generated archives: each must contain its DLL, README, LICENSE,
   and the correct dependency versions. Symbols are in the matching `.snupkg`.
4. Restore and run a clean consumer using the local package directory as an
   additional NuGet source. Test both adapters without project references.
5. Exercise live bot integrations as appropriate. Local tests use synthetic updates.
6. Commit the exact release sources and rebuild before uploading, so repository
   commit metadata and Source Link match the published source. Tag that commit
   with the release version.

## Upload explicitly

Create a NuGet.org API key scoped to these package IDs, and supply it via the
`NUGET_API_KEY` environment variable. Do not store it in the repository.
After validation, run:

```powershell
$version = '1.0.0-beta.1'
$packages = 'Abstract', 'Core', 'DependencyInjection', 'UI', 'Telegram', 'Max'
foreach ($package in $packages) {
    dotnet nuget push "artifacts/nuget/$version/Bot.Commands.$package.$version.nupkg" `
        --source https://api.nuget.org/v3/index.json --api-key $env:NUGET_API_KEY
    if ($LASTEXITCODE -ne 0) { throw "Publishing Bot.Commands.$package failed." }
}
```

The CLI also submits a matching symbol package when present. Publishing a
version is irreversible: a correction needs a new version. After NuGet indexing
finishes, verify installation from NuGet.org and update the getting-started guide
to use the published version.

Packaging properties follow the [NuGet MSBuild reference](https://learn.microsoft.com/en-us/nuget/reference/msbuild-targets).
