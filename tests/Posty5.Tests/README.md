# Posty5 .NET SDK Tests

This directory contains comprehensive integration tests for all Posty5 .NET SDK clients.

## Test Structure

The test suite includes the following test classes:

- **QRCodeClientTests.cs** - Tests for all 7 QR code types (URL, FreeText, Email, WiFi, Call, SMS, Geolocation)
- **ShortLinkClientTests.cs** - Tests for URL shortening with custom slugs and pagination
- **HtmlHostingClientTests.cs** - Tests for HTML page hosting including file uploads
- **SocialPublisherWorkspaceTests.cs** - Tests for social media workspace management
- **SocialPublisherPostTests.cs** - Tests for social media post scheduling and publishing

Each test class covers:

- ✅ Create operations
- ✅ Get by ID
- ✅ List with pagination
- ✅ Update operations
- ✅ Delete operations
- ✅ Error handling

## Prerequisites

### 1. Set Environment Variables

The tests require a valid Posty5 API key. Set the following environment variable:

**Windows (PowerShell):**

```powershell
$env:POSTY5_API_KEY = "your-api-key-here"
```

**Windows (Command Prompt):**

```cmd
set POSTY5_API_KEY=your-api-key-here
```

**Linux/macOS:**

```bash
export POSTY5_API_KEY="your-api-key-here"
```

### 2. Optional: Custom Base URL

If you're using a different API endpoint:

```powershell
$env:POSTY5_BASE_URL = "https://your-custom-api.com"
```

## Running Tests

### Run All Tests

```powershell
dotnet test
```

### Run Specific Test Class

```powershell
# Run only QR Code tests
dotnet test --filter "FullyQualifiedName~QRCodeClientTests"

# Run only Short Link tests
dotnet test --filter "FullyQualifiedName~ShortLinkClientTests"

# Run only HTML Hosting tests
dotnet test --filter "FullyQualifiedName~HtmlHostingClientTests"

# Run only Social Publisher tests
dotnet test --filter "FullyQualifiedName~SocialPublisher"
```

### Run Specific Test

```powershell
dotnet test --filter "FullyQualifiedName~CreateUrlQRCode_ShouldReturnValidQRCode"
```

### Run with Verbose Output

```powershell
dotnet test --logger "console;verbosity=detailed"
```

### Generate Code Coverage

```powershell
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover
```

## Test Configuration

The **TestConfig.cs** file provides:

- API key and base URL configuration from environment variables
- Helper methods to create HTTP clients
- Template ID for testing (default: `68d190bc4b42b89ef76e1398`)
- Resource tracking for cleanup

## Test Assets

The `Assets/` folder contains test files:

- **contact_form.html** - Sample HTML form for HTML hosting tests
- **logo.png** - Sample image for upload tests (if needed)

These files are copied to the output directory during build.

## Important Notes

### Resource Cleanup

Tests track created resources in `TestConfig.CreatedResources` for potential cleanup:

```csharp
TestConfig.CreatedResources.QRCodes.Add(result.Id);
TestConfig.CreatedResources.ShortLinks.Add(result.Id);
TestConfig.CreatedResources.HtmlHostings.Add(result.Id);
TestConfig.CreatedResources.Workspaces.Add(result.Id);
TestConfig.CreatedResources.Posts.Add(result.Id);
```

Some tests automatically delete resources as part of their test flow, but you may want to manually clean up test data periodically.

### Sequential Execution

Tests are marked with `[Collection("Sequential")]` to avoid rate limiting and race conditions when creating/deleting resources.

### API Key Warning

If the API key is not set, tests will show a warning message:

```
⚠️  WARNING: POSTY5_API_KEY is not set!
Tests will fail without a valid API key.
Set environment variable: POSTY5_API_KEY=your-key-here
```

## Continuous Integration

For CI/CD pipelines, set the API key as a secret environment variable:

**GitHub Actions:**

```yaml
env:
  POSTY5_API_KEY: ${{ secrets.POSTY5_API_KEY }}
```

**Azure DevOps:**

```yaml
variables:
  POSTY5_API_KEY: $(POSTY5_API_KEY)
```

## Test Statistics

Total test count: **40+ integration tests**

Coverage by client:

- QRCode: 12 tests (7 creation types + CRUD operations)
- ShortLink: 8 tests
- HtmlHosting: 8 tests
- Social Publisher Workspace: 7 tests
- Social Publisher Post: 10 tests

## Troubleshooting

### Tests are failing with 401 Unauthorized

Check that your API key is correctly set and valid:

```powershell
echo $env:POSTY5_API_KEY
```

### Tests are timing out

The Posty5 API may be slow or rate-limiting your requests. Try:

- Running tests one class at a time
- Adding delays between tests if needed
- Checking your network connection

### File not found errors

Ensure the Assets folder is being copied to the output directory:

```powershell
ls tests\Posty5.Tests\bin\Debug\net8.0\Assets
```

## Contributing

When adding new tests:

1. Follow the existing naming conventions (`MethodName_Scenario_ExpectedResult`)
2. Add descriptive comments for arrange/act/assert sections
3. Track created resources for cleanup
4. Test both success and error scenarios
5. Use unique names with timestamps to avoid conflicts

## License

MIT License - See LICENSE file in the root directory.
