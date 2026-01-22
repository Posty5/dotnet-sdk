# Posty5 .NET SDK - Getting Started Guide

## Prerequisites

- .NET 8.0 SDK or higher
- Visual Studio 2022, Visual Studio Code, or Rider
- A Posty5 API key (get one at https://posty5.com)

## Installation

### Option 1: Using .NET CLI

```bash
dotnet new console -n MyPosty5App
cd MyPosty5App
dotnet add package Posty5.Core
dotnet add package Posty5.QRCode
dotnet add package Posty5.ShortLink
```

### Option 2: Using Visual Studio

1. Right-click on your project in Solution Explorer
2. Select "Manage NuGet Packages"
3. Search for "Posty5.Core" and click Install
4. Repeat for other packages as needed

### Option 3: Using Package Manager Console

```powershell
Install-Package Posty5.Core
Install-Package Posty5.QRCode
Install-Package Posty5.ShortLink
Install-Package Posty5.HtmlHosting
Install-Package Posty5.SocialPublisher
```

## Quick Start Example

Create a new file `Program.cs`:

```csharp
using Posty5.Core.Configuration;
using Posty5.Core.Http;
using Posty5.QRCode;
using Posty5.QRCode.Models;

// Configure the SDK
var options = new Posty5Options
{
    ApiKey = "your-api-key-here" // Replace with your actual API key
};

// Initialize the HTTP client
using var httpClient = new Posty5HttpClient(options);

// Create a QR code client
var qrCodeClient = new QRCodeClient(httpClient);

// Create a QR code
var qrCode = await qrCodeClient.CreateUrlAsync(new CreateUrlQRCodeRequest
{
    Name = "My First QR Code",
    QrCodeTarget = new UrlQRTarget
    {
        Url = "https://posty5.com"
    }
});

Console.WriteLine($"QR Code created successfully!");
Console.WriteLine($"URL: {qrCode.QrCodeLandingPage}");
Console.WriteLine($"ID: {qrCode.Id}");
```

Run your application:

```bash
dotnet run
```

## Environment Variables

Instead of hardcoding your API key, use environment variables:

```csharp
var options = new Posty5Options
{
    ApiKey = Environment.GetEnvironmentVariable("POSTY5_API_KEY")
};
```

Set the environment variable:

**Windows (PowerShell):**

```powershell
$env:POSTY5_API_KEY = "your-api-key"
```

**Windows (Command Prompt):**

```cmd
set POSTY5_API_KEY=your-api-key
```

**Linux/macOS:**

```bash
export POSTY5_API_KEY=your-api-key
```

## Using with ASP.NET Core

In your `Program.cs`:

```csharp
builder.Services.AddSingleton<Posty5Options>(new Posty5Options
{
    ApiKey = builder.Configuration["Posty5:ApiKey"]
});

builder.Services.AddSingleton<Posty5HttpClient>();
builder.Services.AddScoped<QRCodeClient>();
builder.Services.AddScoped<ShortLinkClient>();
```

In your `appsettings.json`:

```json
{
  "Posty5": {
    "ApiKey": "your-api-key"
  }
}
```

## Next Steps

- Check out the [examples](examples/) folder for more code samples
- Read the [API documentation](https://docs.posty5.com)
- Explore the [README.md](README.md) for detailed usage

## Support

- Email: support@posty5.com
- Documentation: https://docs.posty5.com
- GitHub Issues: https://github.com/posty5/dotnet-sdk/issues
