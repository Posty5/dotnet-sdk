# Publish NuGet packages to NuGet.org
# Usage: .\publish-packages.ps1 -ApiKey YOUR_NUGET_API_KEY

param(
    [Parameter(Mandatory = $true)]
    [string]$ApiKey
)

Write-Host "Publishing Posty5 .NET SDK to NuGet.org..." -ForegroundColor Green

if (-not (Test-Path "./nupkg")) {
    Write-Host "Error: ./nupkg directory not found. Run build-packages.ps1 first." -ForegroundColor Red
    exit 1
}

$packages = Get-ChildItem -Path ./nupkg -Filter *.nupkg

if ($packages.Count -eq 0) {
    Write-Host "Error: No packages found in ./nupkg directory." -ForegroundColor Red
    exit 1
}

Write-Host "`nFound $($packages.Count) package(s) to publish:" -ForegroundColor Yellow
$packages | ForEach-Object {
    Write-Host "  - $($_.Name)" -ForegroundColor Cyan
}

Write-Host "`nPublishing packages..." -ForegroundColor Yellow

foreach ($package in $packages) {
    Write-Host "`nPublishing $($package.Name)..." -ForegroundColor Cyan
    
    dotnet nuget push $package.FullName `
        --api-key $ApiKey `
        --source https://api.nuget.org/v3/index.json `
        --skip-duplicate
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✓ Successfully published $($package.Name)" -ForegroundColor Green
    }
    else {
        Write-Host "✗ Failed to publish $($package.Name)" -ForegroundColor Red
    }
}

Write-Host "`n✓ Publishing complete!" -ForegroundColor Green
Write-Host "Packages should be available on NuGet.org within a few minutes." -ForegroundColor Yellow
