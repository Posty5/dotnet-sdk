# Posty5 .NET SDK

Official .NET SDK for [Posty5](https://posty5.com) - A comprehensive toolkit for C# developers to integrate Posty5 services into their applications.

## 📦 NuGet Packages

The SDK is split into multiple packages for modularity:

- **Posty5.Core** - Core HTTP client and base classes
- **Posty5.QRCode** - QR Code generation and management
- **Posty5.ShortLink** - URL shortener functionality
- **Posty5.HtmlHosting** - HTML page hosting
- **Posty5.SocialPublisher** - Social media publishing tools

## 🚀 Installation

Install packages via NuGet Package Manager:

```bash
dotnet add package Posty5.Core
dotnet add package Posty5.QRCode
dotnet add package Posty5.ShortLink
dotnet add package Posty5.HtmlHosting
dotnet add package Posty5.SocialPublisher
```

Or via Package Manager Console:

```powershell
Install-Package Posty5.Core
Install-Package Posty5.QRCode
Install-Package Posty5.ShortLink
Install-Package Posty5.HtmlHosting
Install-Package Posty5.SocialPublisher
```

## 📖 Quick Start

### 1. Initialize the HTTP Client

```csharp
using Posty5.Core.Configuration;
using Posty5.Core.Http;

var options = new Posty5Options
{
    ApiKey = "your-api-key-here",
    Debug = false // Set to true for debugging
};

var httpClient = new Posty5HttpClient(options);
```

### 2. QR Code Management

```csharp
using Posty5.QRCode;
using Posty5.QRCode.Models;

var qrCodeClient = new QRCodeClient(httpClient);

// Create a URL QR code
var qrCode = await qrCodeClient.CreateUrlAsync(new CreateUrlQRCodeRequest
{
    Name = "My Website",
    QrCodeTarget = new UrlQRTarget
    {
        Url = "https://example.com"
    }
});

Console.WriteLine($"QR Code URL: {qrCode.QrCodeLandingPageURL}");

// Create a WiFi QR code
var wifiQr = await qrCodeClient.CreateWifiAsync(new CreateWifiQRCodeRequest
{
    Name = "Office WiFi",
    QrCodeTarget = new WifiQRTarget
    {
        Ssid = "MyNetwork",
        Password = "mypassword123",
        SecurityType = "WPA"
    }
});

// List QR codes with pagination
var qrCodes = await qrCodeClient.ListAsync(
    new ListQRCodesParams { Search = "website" },
    new PaginationParams { PageNumber = 0, PageSize = 20 }
);

// Get a specific QR code
var existingQr = await qrCodeClient.GetAsync("qr-code-id");

// Delete a QR code
await qrCodeClient.DeleteAsync("qr-code-id");
```

### 3. Short Link Management

```csharp
using Posty5.ShortLink;
using Posty5.ShortLink.Models;

var shortLinkClient = new ShortLinkClient(httpClient);

// Create a short link
var shortLink = await shortLinkClient.CreateAsync(new CreateShortLinkRequest
{
    Name = "My Campaign Link",
    TargetUrl = "https://example.com/long-url",
    CustomSlug = "my-link" // Optional
});

Console.WriteLine($"Short URL: {shortLink.ShortUrl}");

// List short links
var shortLinks = await shortLinkClient.ListAsync(
    new ListShortLinksParams { Search = "campaign" },
    new PaginationParams { PageNumber = 0, PageSize = 20 }
);

// Update a short link
var updated = await shortLinkClient.UpdateAsync("link-id", new UpdateShortLinkRequest
{
    TargetUrl = "https://example.com/new-url"
});

// Delete a short link
await shortLinkClient.DeleteAsync("link-id");
```

### 4. HTML Hosting

```csharp
using Posty5.HtmlHosting;
using Posty5.HtmlHosting.Models;

var htmlHostingClient = new HtmlHostingClient(httpClient);

// Create an HTML page
var htmlPage = await htmlHostingClient.CreateAsync(new CreateHtmlHostingRequest
{
    Name = "Landing Page",
    HtmlContent = "<html><body><h1>Hello World!</h1></body></html>"
});

Console.WriteLine($"Page URL: {htmlPage.PublicUrl}");

// Update HTML content
var updatedPage = await htmlHostingClient.UpdateAsync("page-id", new UpdateHtmlHostingRequest
{
    HtmlContent = "<html><body><h1>Updated Content</h1></body></html>"
});

// List HTML pages
var pages = await htmlHostingClient.ListAsync();

// Delete a page
await htmlHostingClient.DeleteAsync("page-id");
```

### 5. Social Media Publishing

```csharp
using Posty5.SocialPublisher;
using Posty5.SocialPublisher.Models;

var workspaceClient = new WorkspaceClient(httpClient);
var taskClient = new TaskClient(httpClient);

// Create a workspace
var workspace = await workspaceClient.CreateAsync(new CreateWorkspaceRequest
{
    Name = "My Social Media Workspace",
    Description = "Marketing campaigns"
});

// Create a publishing task
var task = await taskClient.CreateAsync(new CreateTaskRequest
{
    WorkspaceId = workspace.Id!,
    Title = "New Product Launch",
    Content = "Check out our new product! 🚀",
    Platforms = new List<SocialPlatform>
    {
        SocialPlatform.Facebook,
        SocialPlatform.Twitter
    },
    ScheduledAt = DateTime.UtcNow.AddHours(2)
});

// Publish immediately
await taskClient.PublishAsync(task.Id!);

// List tasks
var tasks = await taskClient.ListAsync(
    new ListTasksParams
    {
        WorkspaceId = workspace.Id,
        Status = TaskStatus.Scheduled
    }
);
```

## 🔧 Advanced Configuration

### Custom Timeout and Retry Settings

```csharp
var options = new Posty5Options
{
    ApiKey = "your-api-key",
    Debug = true
};
```

### Error Handling

```csharp
using Posty5.Core.Exceptions;

try
{
    var qrCode = await qrCodeClient.GetAsync("invalid-id");
}
catch (Posty5NotFoundException ex)
{
    Console.WriteLine($"Resource not found: {ex.Message}");
}
catch (Posty5AuthenticationException ex)
{
    Console.WriteLine($"Authentication failed: {ex.Message}");
}
catch (Posty5ValidationException ex)
{
    Console.WriteLine($"Validation error: {ex.Message}");
}
catch (Posty5RateLimitException ex)
{
    Console.WriteLine($"Rate limit exceeded: {ex.Message}");
}
catch (Posty5Exception ex)
{
    Console.WriteLine($"API error: {ex.Message}, Status: {ex.StatusCode}");
}
```

### Using Dependency Injection (ASP.NET Core)

```csharp
// In Program.cs or Startup.cs
services.AddSingleton<Posty5Options>(new Posty5Options
{
    ApiKey = configuration["Posty5:ApiKey"]
});

services.AddSingleton<Posty5.Core.Http.HttpClient>();
services.AddScoped<QRCodeClient>();
services.AddScoped<ShortLinkClient>();
services.AddScoped<HtmlHostingClient>();
services.AddScoped<WorkspaceClient>();
services.AddScoped<TaskClient>();

// In your controller or service
public class MyService
{
    private readonly QRCodeClient _qrCodeClient;

    public MyService(QRCodeClient qrCodeClient)
    {
        _qrCodeClient = qrCodeClient;
    }

    public async Task<QRCodeModel> CreateQRCode()
    {
        return await _qrCodeClient.CreateUrlAsync(new CreateUrlQRCodeRequest
        {
            Name = "My QR Code",
            QrCodeTarget = new UrlQRTarget { Url = "https://example.com" }
        });
    }
}
```

## 📚 API Documentation

For detailed API documentation, visit [https://docs.posty5.com](https://docs.posty5.com)

## 🛠️ Building from Source

```bash
git clone https://github.com/posty5/dotnet-sdk.git
cd posty5-dotnet-sdk
dotnet restore
dotnet build
```

## 🧪 Running Tests

```bash
dotnet test
```

## 📋 Requirements

- .NET 8.0 or higher
- C# 12.0 or higher

## 🤝 Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## 📖 Resources

- **Official Guides**: [https://guide.posty5.com](https://guide.posty5.com)
- **API Reference**: [https://docs.posty5.com](https://docs.posty5.com)
- **Source Code**: [https://github.com/Posty5/dotnet-sdk](https://github.com/Posty5/dotnet-sdk)

---

## 📦 Packages

This SDK ecosystem contains the following tool packages:

| Package | Description | Version | NuGet |
| --- | --- | --- | --- |
| [Posty5.Core](./src/Posty5.Core) | Core HTTP client and models | 1.0.0 | [📦 NuGet](https://www.nuget.org/packages/Posty5.Core) |
| [Posty5.ShortLink](./src/Posty5.ShortLink) | URL shortener client | 1.0.0 | [📦 NuGet](https://www.nuget.org/packages/Posty5.ShortLink) |
| [Posty5.QRCode](./src/Posty5.QRCode) | QR code generator client | 1.0.0 | [📦 NuGet](https://www.nuget.org/packages/Posty5.QRCode) |
| [Posty5.HtmlHosting](./src/Posty5.HtmlHosting) | HTML hosting client | 1.0.0 | [📦 NuGet](https://www.nuget.org/packages/Posty5.HtmlHosting) |
| [Posty5.HtmlHostingVariables](./src/Posty5.HtmlHostingVariables) | Variable management | 1.0.0 | [📦 NuGet](https://www.nuget.org/packages/Posty5.HtmlHostingVariables) |
| [Posty5.HtmlHostingFormSubmission](./src/Posty5.HtmlHostingFormSubmission) | Form submission management | 1.0.0 | [📦 NuGet](https://www.nuget.org/packages/Posty5.HtmlHostingFormSubmission) |
| [Posty5.SocialPublisherWorkspace](./src/Posty5.SocialPublisherWorkspace) | Social workspace management | 1.0.0 | [📦 NuGet](https://www.nuget.org/packages/Posty5.SocialPublisherWorkspace) |
| [Posty5.SocialPublisherTask](./src/Posty5.SocialPublisherTask) | Social publishing task client | 1.0.0 | [📦 NuGet](https://www.nuget.org/packages/Posty5.SocialPublisherTask) |

---

## 🆘 Support

We're here to help you succeed with Posty5!

### Get Help

- **Documentation**: [https://guide.posty5.com](https://guide.posty5.com)
- **Contact Us**: [https://posty5.com/contact-us](https://posty5.com/contact-us)
- **GitHub Issues**: [Report bugs or request features](https://github.com/Posty5/dotnet-sdk/issues)
- **API Status**: Check API status and uptime at [https://status.posty5.com](https://status.posty5.com)

---

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

---

## 🔗 Useful Links

- **Website**: [https://posty5.com](https://posty5.com)
- **API Documentation**: [https://docs.posty5.com](https://docs.posty5.com)
- **GitHub Repository**: [https://github.com/posty5/dotnet-sdk](https://github.com/posty5/dotnet-sdk)
- **NuGet Gallery**: [https://www.nuget.org/packages/Posty5.Core](https://www.nuget.org/packages/Posty5.Core)

---

Made with ❤️ by the Posty5 team
