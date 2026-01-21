# Build all NuGet packages
Write-Host "Building Posty5 .NET SDK NuGet Packages..." -ForegroundColor Green

# Clean previous builds
Write-Host "`nCleaning previous builds..." -ForegroundColor Yellow
Get-ChildItem -Path . -Include bin, obj -Recurse -Directory | Remove-Item -Recurse -Force
Get-ChildItem -Path . -Filter *.nupkg -Recurse | Remove-Item -Force

# Restore dependencies
Write-Host "`nRestoring dependencies..." -ForegroundColor Yellow
dotnet restore

# Build solution
Write-Host "`nBuilding solution..." -ForegroundColor Yellow
dotnet build --configuration Release

# Pack each project
Write-Host "`nCreating NuGet packages..." -ForegroundColor Yellow

$projects = @(
    "src/Posty5.Core/Posty5.Core.csproj",
    "src/Posty5.QRCode/Posty5.QRCode.csproj",
    "src/Posty5.ShortLink/Posty5.ShortLink.csproj",
    "src/Posty5.HtmlHosting/Posty5.HtmlHosting.csproj",
    "src/Posty5.SocialPublisher/Posty5.SocialPublisher.csproj"
)

foreach ($project in $projects) {
    Write-Host "`nPacking $project..." -ForegroundColor Cyan
    dotnet pack $project --configuration Release --output ./nupkg --no-build
}

Write-Host "`n✓ All packages created successfully!" -ForegroundColor Green
Write-Host "`nPackages are available in the ./nupkg directory" -ForegroundColor Yellow

# List created packages
Write-Host "`nCreated packages:" -ForegroundColor Green
Get-ChildItem -Path ./nupkg -Filter *.nupkg | ForEach-Object {
    Write-Host "  - $($_.Name)" -ForegroundColor Cyan
}

Write-Host "`nTo publish to NuGet.org, run:" -ForegroundColor Yellow
Write-Host "  dotnet nuget push ./nupkg/*.nupkg --api-key YOUR_API_KEY --source https://api.nuget.org/v3/index.json" -ForegroundColor White
