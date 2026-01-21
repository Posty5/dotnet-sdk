# Publishing to NuGet - Step by Step Guide

This guide walks you through publishing the Posty5 .NET SDK packages to NuGet.org.

## Prerequisites

1. **NuGet Account**: Create an account at [nuget.org](https://www.nuget.org/)
2. **API Key**: Generate an API key from your NuGet account
   - Go to https://www.nuget.org/account/apikeys
   - Click "Create"
   - Give it a name like "Posty5 SDK Publishing"
   - Select "Push new packages and package versions"
   - Choose "All Packages"
   - Copy the generated API key

## Step 1: Build the Packages

Run the build script:

```powershell
.\build-packages.ps1
```

This will:

- Clean previous builds
- Restore NuGet dependencies
- Build all projects in Release mode
- Create NuGet packages in `./nupkg` folder

Expected output:

```
Posty5.Core.1.0.0.nupkg
Posty5.QRCode.1.0.0.nupkg
Posty5.ShortLink.1.0.0.nupkg
Posty5.HtmlHosting.1.0.0.nupkg
Posty5.SocialPublisher.1.0.0.nupkg
```

## Step 2: Test Packages Locally (Optional)

Before publishing, test packages locally:

```powershell
# Create a test project
dotnet new console -n TestPosty5
cd TestPosty5

# Add local package source
dotnet nuget add source D:\posty5\Posty5-WEB\posty5-dotnet-sdk\nupkg -n LocalPosty5

# Install packages
dotnet add package Posty5.Core --version 1.0.0
dotnet add package Posty5.QRCode --version 1.0.0

# Test the code
# ... write test code ...

dotnet run
```

## Step 3: Validate Package Metadata

Check each `.nupkg` file contains:

- Correct version number (1.0.0)
- License information (MIT)
- Project URL
- Repository URL
- Proper dependencies

You can inspect using:

```powershell
# Extract and examine
Expand-Archive ./nupkg/Posty5.Core.1.0.0.nupkg -DestinationPath ./temp
cat ./temp/Posty5.Core.nuspec
```

## Step 4: Publish to NuGet.org

### Option A: Using the Publish Script (Recommended)

```powershell
.\publish-packages.ps1 -ApiKey YOUR_NUGET_API_KEY
```

### Option B: Manual Publishing

Publish each package individually:

```powershell
dotnet nuget push ./nupkg/Posty5.Core.1.0.0.nupkg `
    --api-key YOUR_API_KEY `
    --source https://api.nuget.org/v3/index.json

dotnet nuget push ./nupkg/Posty5.QRCode.1.0.0.nupkg `
    --api-key YOUR_API_KEY `
    --source https://api.nuget.org/v3/index.json

dotnet nuget push ./nupkg/Posty5.ShortLink.1.0.0.nupkg `
    --api-key YOUR_API_KEY `
    --source https://api.nuget.org/v3/index.json

dotnet nuget push ./nupkg/Posty5.HtmlHosting.1.0.0.nupkg `
    --api-key YOUR_API_KEY `
    --source https://api.nuget.org/v3/index.json

dotnet nuget push ./nupkg/Posty5.SocialPublisher.1.0.0.nupkg `
    --api-key YOUR_API_KEY `
    --source https://api.nuget.org/v3/index.json
```

## Step 5: Verify Publication

1. Check your NuGet profile: https://www.nuget.org/profiles/YourUsername
2. Packages will be listed under "Published Packages"
3. Initial validation takes 5-15 minutes
4. Once validated, packages become searchable

## Step 6: Update Documentation

After publishing:

1. Update README.md installation instructions
2. Add badges to README.md:

   ```markdown
   [![NuGet](https://img.shields.io/nuget/v/Posty5.Core.svg)](https://www.nuget.org/packages/Posty5.Core/) [![Downloads](https://img.shields.io/nuget/dt/Posty5.Core.svg)](https://www.nuget.org/packages/Posty5.Core/)
   ```

3. Create a GitHub release
4. Update CHANGELOG.md

## Publishing Updates

For version updates:

1. Update version in `.csproj` files:

   ```xml
   <Version>1.0.1</Version>
   ```

2. Update CHANGELOG.md

3. Build new packages:

   ```powershell
   .\build-packages.ps1
   ```

4. Publish:
   ```powershell
   .\publish-packages.ps1 -ApiKey YOUR_API_KEY
   ```

## Package Dependencies

The packages have the following dependency chain:

```
Posty5.Core (base)
  ↓
  ├── Posty5.QRCode
  ├── Posty5.ShortLink
  ├── Posty5.HtmlHosting
  └── Posty5.SocialPublisher
```

Always publish `Posty5.Core` first, as other packages depend on it.

## Troubleshooting

### "Package already exists"

- Version already published to NuGet
- Increment version number and rebuild

### "Invalid API key"

- API key expired or incorrect
- Generate new key from NuGet account

### "Package validation failed"

- Check package metadata
- Ensure all required fields are filled
- Review NuGet validation rules

### "Missing dependencies"

- Ensure Posty5.Core is published first
- Check dependency versions in .csproj files

## Security Best Practices

1. **Never commit API keys** to version control
2. **Use environment variables** for API keys
3. **Rotate API keys** periodically
4. **Limit API key scopes** to only what's needed
5. **Store API keys securely** (e.g., Azure Key Vault, password manager)

## Resources

- [NuGet Documentation](https://docs.microsoft.com/en-us/nuget/)
- [Creating NuGet Packages](https://docs.microsoft.com/en-us/nuget/create-packages/overview-and-workflow)
- [Publishing Packages](https://docs.microsoft.com/en-us/nuget/nuget-org/publish-a-package)
- [Package Metadata](https://docs.microsoft.com/en-us/nuget/reference/nuspec)
