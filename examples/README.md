# Posty5 .NET SDK Examples

This folder contains comprehensive examples demonstrating how to use the Posty5 .NET SDK.

## Running the Examples

1. Set your API key as an environment variable:

   ```powershell
   $env:POSTY5_API_KEY = "your-api-key"
   ```

2. Create a console application:

   ```bash
   dotnet new console -n Posty5Examples
   cd Posty5Examples
   ```

3. Add the Posty5 packages:

   ```bash
   dotnet add package Posty5.Core
   dotnet add package Posty5.QRCode
   dotnet add package Posty5.ShortLink
   dotnet add package Posty5.HtmlHosting
   dotnet add package Posty5.SocialPublisher
   ```

4. Copy the contents of `Examples.cs` to your `Program.cs`

5. Run the application:
   ```bash
   dotnet run
   ```

## Examples Included

### QR Code Examples

- Create URL QR code
- Create WiFi QR code
- Create Email QR code
- List QR codes with pagination

### Short Link Examples

- Create short link with custom slug
- Get short link details and statistics
- Update short link
- List short links

### HTML Hosting Examples

- Create HTML page
- Update HTML content
- List all pages

### Social Publisher Examples

- Create workspace
- Create scheduled task
- Publish task immediately
- List tasks by workspace

## Example Code Structure

Each example follows this pattern:

```csharp
// 1. Initialize HTTP client
var options = new Posty5Options { ApiKey = "your-key" };
var httpClient = new Posty5HttpClient(options);

// 2. Create specific client
var qrCodeClient = new QRCodeClient(httpClient);

// 3. Use the client
var result = await qrCodeClient.CreateUrlAsync(new CreateUrlQRCodeRequest
{
    Name = "My QR Code",
    QrCodeTarget = new UrlQRTarget { Url = "https://example.com" }
});

// 4. Use the result
Console.WriteLine($"Created: {result.QrCodeLandingPage}");
```

## Additional Resources

- [Quick Reference](../QUICK_REFERENCE.md) - API cheat sheet
- [Getting Started Guide](../GETTING_STARTED.md) - Detailed setup instructions
- [README](../README.md) - Complete documentation
