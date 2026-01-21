# Quick Start - Running Tests

## Step 1: Set Your API Key

**Windows PowerShell:**

```powershell
$env:POSTY5_API_KEY = "your-api-key-here"
```

**Windows CMD:**

```cmd
set POSTY5_API_KEY=your-api-key-here
```

## Step 2: Navigate to the SDK Directory

```powershell
cd d:\posty5\Posty5-WEB\posty5-dotnet-sdk
```

## Step 3: Run Tests

**All tests:**

```powershell
dotnet test
```

**Specific test class:**

```powershell
# QR Code tests only
dotnet test --filter "QRCodeClientTests"

# Short Link tests only
dotnet test --filter "ShortLinkClientTests"

# HTML Hosting tests only
dotnet test --filter "HtmlHostingClientTests"

# Social Publisher tests only
dotnet test --filter "SocialPublisher"
```

**Single test:**

```powershell
dotnet test --filter "CreateUrlQRCode_ShouldReturnValidQRCode"
```

## Test Coverage

✅ **QRCodeClientTests** - 12 tests

- All 7 QR code types (URL, FreeText, Email, WiFi, Call, SMS, Geolocation)
- Get by ID
- List with pagination and search
- Update
- Delete

✅ **ShortLinkClientTests** - 8 tests

- Create with auto-generated slug
- Create with custom slug
- Get by ID
- List with pagination and search
- Update name and URL
- Delete

✅ **HtmlHostingClientTests** - 8 tests

- Create with HTML content
- Create with file upload
- Get by ID
- List with pagination and search
- Update content
- Delete

✅ **SocialPublisherWorkspaceTests** - 7 tests

- Create workspace
- Get by ID
- List with pagination and search
- Update
- Delete

✅ **SocialPublisherTaskTests** - 10 tests

- Create task
- Create with multiple platforms
- Get by ID
- List all tasks
- List by workspace
- List by status
- Update task
- Update platforms
- Delete

**Total: 45+ integration tests**

## Important Notes

1. **API Key Required**: Tests will fail without a valid API key
2. **Test Data**: Tests create real resources in your Posty5 account
3. **Cleanup**: Some tests delete resources automatically, but you may want to manually clean up test data
4. **Template ID**: Tests use template ID `68d190bc4b42b89ef76e1398` - update in `TestConfig.cs` if needed
5. **Rate Limiting**: Tests are marked sequential to avoid rate limits

## Troubleshooting

**401 Unauthorized Error:**

- Verify API key is set: `echo $env:POSTY5_API_KEY`
- Ensure the key is valid and has proper permissions

**File Not Found:**

- Assets folder should be copied to bin directory automatically
- Check: `tests\Posty5.Tests\bin\Debug\net8.0\Assets\`

**Tests Timing Out:**

- API might be slow or rate-limiting
- Run tests one class at a time
- Check network connection

## Next Steps

1. Review test results
2. Check created resources in your Posty5 dashboard
3. Modify `TestConfig.cs` for custom configuration
4. Add more tests as needed for your specific use cases
