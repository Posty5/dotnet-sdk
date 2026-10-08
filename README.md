# Posty5 .NET SDK

Official .NET SDK for [Posty5](https://posty5.com) - A comprehensive toolkit for C# developers to integrate Posty5 services into their applications.

## 📦 NuGet Packages

The SDK is split into multiple packages for modularity:

- **Posty5.Core** - Core HTTP client and base classes
- **Posty5.QRCode** - QR Code generation and management
- **Posty5.ShortLink** - URL shortener functionality
- **Posty5.HtmlHosting** - HTML page hosting
- **Posty5.SocialPublisher** - Social media publishing tools
- **Posty5.Store** - Online store: catalogue, orders, shipping, dropshipping
- **Posty5.Account** - Who the API key belongs to, credits, credit history, live operation prices

## 🚀 Installation

Install packages via NuGet Package Manager:

```bash
dotnet add package Posty5.Core
dotnet add package Posty5.QRCode
dotnet add package Posty5.ShortLink
dotnet add package Posty5.HtmlHosting
dotnet add package Posty5.SocialPublisher
dotnet add package Posty5.Account
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
using Posty5.Core.Models;
using Posty5.QRCode;
using Posty5.QRCode.Models;

var qrCodeClient = new QRCodeClient(httpClient);

// Create a URL QR code (TemplateId is required for API-key calls)
var qrCode = await qrCodeClient.CreateURLAsync(new QRCodeCreateURLRequestModel
{
    Name = "My Website",
    TemplateId = "your-template-id",
    Url = new QRCodeUrlTargetModel { Url = "https://example.com" }
});

Console.WriteLine($"QR Code page: {qrCode.QrCodeLandingPageURL}");

// Create a WiFi QR code (the API builds and escapes the encoded text)
var wifiQr = await qrCodeClient.CreateWifiAsync(new QRCodeCreateWifiRequestModel
{
    Name = "Office WiFi",
    TemplateId = "your-template-id",
    Wifi = new QRCodeWifiTargetModel
    {
        Name = "MyNetwork",
        AuthenticationType = "WPA",
        Password = "mypassword123"
    }
});

// List QR codes (cursor pagination)
var qrCodes = await qrCodeClient.ListAsync(
    new QRCodeListParamsModel { Name = "website" },
    new PaginationParams { PageSize = 20 }
);

// Get a specific QR code
var existingQr = await qrCodeClient.GetAsync("qr-code-id");

// Delete a QR code: pass the version you read (see "Versioned writes")
await qrCodeClient.DeleteAsync("qr-code-id", existingQr.Version);
```

### 3. Short Link Management

```csharp
using Posty5.Core.Models;
using Posty5.ShortLink;
using Posty5.ShortLink.Models;

var shortLinkClient = new ShortLinkClient(httpClient);

// Create a short link (BaseUrl and TemplateId are required)
var shortLink = await shortLinkClient.CreateAsync(new ShortLinkCreateRequestModel
{
    Name = "My Campaign Link",
    BaseUrl = "https://example.com/long-url",
    TemplateId = "your-template-id",
    CustomLandingId = "my-link" // Optional, Starter plan and above
});

Console.WriteLine($"Short URL: {shortLink.ShorterLink}");

// List short links
var shortLinks = await shortLinkClient.ListAsync(
    new ShortLinkListParamsModel { Name = "campaign" },
    new PaginationParams { PageSize = 20 }
);

// Update a short link (BaseUrl and TemplateId are required on every update).
// The version you read goes with the write; the answer carries the new one.
var link = await shortLinkClient.GetAsync("link-id");
var updated = await shortLinkClient.UpdateAsync("link-id", new ShortLinkUpdateRequestModel
{
    BaseUrl = "https://example.com/new-url",
    TemplateId = "your-template-id"
}, link.Version);

// Delete a short link
await shortLinkClient.DeleteAsync("link-id", updated.Version);
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

// Update the page from a new GitHub file, with the version you read
var page = await htmlHostingClient.GetAsync("page-id");
var updatedPage = await htmlHostingClient.UpdateWithGithubFileAsync("page-id", new HtmlHostingUpdatePageGithubRequestModel
{
    Name = "Landing Page",
    GithubInfo = new() { FileURL = "https://github.com/owner/repo/blob/main/index.html" }
}, page.Version);

// List HTML pages
var pages = await htmlHostingClient.ListAsync();

// Delete a page
await htmlHostingClient.DeleteAsync("page-id", updatedPage.Version);
```

### 5. Social Media Publishing

```csharp
using Posty5.SocialPublisher;
using Posty5.SocialPublisher.Models;

var workspaceClient = new WorkspaceClient(httpClient);
var postClient = new PostClient(httpClient);

// Create a workspace
var workspace = await workspaceClient.CreateAsync(new CreateWorkspaceRequest
{
    Name = "My Social Media Workspace",
    Description = "Marketing campaigns"
});

// Create a publishing post
var post = await postClient.CreateAsync(new CreatePostRequest
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
await postClient.PublishAsync(post.Id!);

// List posts
var posts = await postClient.ListAsync(
    new ListPostsParams
    {
        WorkspaceId = workspace.Id,
        Status = PostStatus.Scheduled
    }
);
```

## 🔧 Advanced Configuration

### Options

```csharp
var options = new Posty5Options
{
    ApiKey = "your-api-key",
    Debug = true,

    // Sent on every request. X-API-Key cannot be set here (the constructor
    // throws); an X-Posty5-Client entry replaces the SDK's own label.
    DefaultHeaders = new() { ["X-Correlation-Id"] = "abc-123" },

    // The createdFrom label on everything this client creates. Null keeps the
    // package defaults ("dotnetPackage"; "dotnet" for store orders).
    CreatedFrom = "my-crm"
};
```

Every request carries `X-Posty5-Client: posty5-dotnet/<Posty5.Core version>`.
The request timeout is 120 seconds, and there are **no automatic retries** — a
failed request is reported, never repeated, so a create is not sent twice.

### Agent essentials: who am I, what does it cost, which id

| Package | Method | Route |
| --- | --- | --- |
| `Posty5.Account` | `AccountClient.GetCurrentAsync()` | `GET /api/api-key/current` |
| `Posty5.Account` | `AccountClient.GetCreditsAsync()` | `GET /api/user/current/credits` |
| `Posty5.Account` | `AccountClient.GetCreditUsageAsync(filters?, pagination?)` | `GET /api/user/current/credit-usage` |
| `Posty5.Account` | `AccountClient.GetCreditUsageSummaryAsync(filters?)` | `GET /api/user/current/credit-usage/summary` |
| `Posty5.Account` | `AccountClient.GetOperationCostsAsync(activeOnly = true)` | `GET /api/plans/operation-costs` |
| `Posty5.Store` | `StoreClient.ListStoresAsync()`, `LookupStoresAsync(term?)` | `GET /api/store/lookup` |
| `Posty5.SocialPublisherWorkspace` | `SocialPublisherAccountClient.ListAsync / LookupAsync / GetAsync` | `GET /api/social-publisher-account`, `/lookup`, `/{id}` |
| `Posty5.QRCode` | `QRCodeTemplateClient.ListUserTemplatesAsync / ListPublicTemplatesAsync` | `GET /api/qr-code-template/user-lookup`, `/public-lookup` |
| `Posty5.SocialPublisherPost` | `CreateTextPostToWorkspaceAsync / ToAccountAsync` | `POST /api/social-publisher-post/text/workspace\|account[/{id}]` |
| `Posty5.SocialPublisherPost` | `CreateStoryPostToWorkspaceAsync / ToAccountAsync` (media by URL) | `POST /api/social-publisher-post/story/workspace\|account` |
| `Posty5.SocialPublisherPost` | `RemovePostAsync(id)` | `POST /api/social-publisher-post/{id}/remove` |
| `Posty5.SocialPublisherPost` | `GetStatusAsync(id)` (route fixed in 4.6.0) | `GET /api/social-publisher-post/{id}/status` |

See [GETTING_STARTED.md](GETTING_STARTED.md#check-the-key-find-the-ids-then-act) for a walk-through.

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
catch (Posty5ConflictException ex)
{
    Console.WriteLine($"Changed by someone else; stored version is {ex.CurrentVersion}");
}
catch (Posty5Exception ex)
{
    Console.WriteLine($"API error: {ex.Message}, Status: {ex.StatusCode}");
}
```

### Versioned writes (optimistic concurrency)

Every update, delete and state change takes the version of the document you
read, and the SDK sends it as `If-Match: "<version>"` on that request only. If
someone else changed the document in between, the API refuses the write with
`409 VERSION_CONFLICT` instead of silently overwriting their change.

```csharp
using Posty5.Core.Exceptions;

var link = await shortLinkClient.GetAsync(id);            // link.Version is the document's __v
try
{
    var saved = await shortLinkClient.UpdateAsync(id, request, link.Version);
    // saved.Version is the new version: pass it to the next write
}
catch (Posty5ConflictException ex)
{
    // Someone else saved first. ex.CurrentVersion is the stored version;
    // read the document again, reapply your change, and write again.
}
```

- Every model exposes `Version` (`[JsonPropertyName("__v")]`). A write answers
  with the new version (the envelope's `version`, also sent as `ETag`), and the
  SDK copies it onto the returned model.
- Bulk writes take `IDictionary<string, long> versions` covering every id; the
  result lists `Applied`, `Skipped` (with `CurrentVersion` on a conflict) and the
  new `Versions`.
- `Posty5VersionRequiredException` (428) means a write reached a versioned route
  without a version: a bug, since every SDK write method requires one.
- Versioned writes are **never retried** by the SDK: a retry with the same
  version would either succeed twice or conflict with itself.
- During the API's rollout, a write that should have carried a version is
  answered with `X-Posty5-Concurrency: missing-version`. Set
  `Posty5Options.Logger` to an `ILogger` and the SDK logs one warning the first
  time it sees it.
- Creates, reorders and job triggers (health checks, cache clears, tests,
  syncs) take no version.

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
services.AddScoped<PostClient>();

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

| Package                                                                    | Description                   | Version | NuGet                                                                       |
| -------------------------------------------------------------------------- | ----------------------------- | ------- | --------------------------------------------------------------------------- |
| [Posty5.Core](./src/Posty5.Core)                                           | Core HTTP client and models   | 4.0.0   | [📦 NuGet](https://www.nuget.org/packages/Posty5.Core)                      |
| [Posty5.Account](./src/Posty5.Account)                                     | Key owner, credits, prices    | 3.2.0   | [📦 NuGet](https://www.nuget.org/packages/Posty5.Account)                   |
| [Posty5.ShortLink](./src/Posty5.ShortLink)                                 | URL shortener client          | 4.0.0   | [📦 NuGet](https://www.nuget.org/packages/Posty5.ShortLink)                 |
| [Posty5.QRCode](./src/Posty5.QRCode)                                       | QR code generator client      | 4.0.0   | [📦 NuGet](https://www.nuget.org/packages/Posty5.QRCode)                    |
| [Posty5.HtmlHosting](./src/Posty5.HtmlHosting)                             | HTML hosting client           | 4.0.0   | [📦 NuGet](https://www.nuget.org/packages/Posty5.HtmlHosting)               |
| [Posty5.HtmlHostingVariables](./src/Posty5.HtmlHostingVariables)           | Variable management           | 4.0.0   | [📦 NuGet](https://www.nuget.org/packages/Posty5.HtmlHostingVariables)      |
| [Posty5.HtmlHostingFormSubmission](./src/Posty5.HtmlHostingFormSubmission) | Form submission management    | 4.0.0   | [📦 NuGet](https://www.nuget.org/packages/Posty5.HtmlHostingFormSubmission) |
| [Posty5.SocialPublisherWorkspace](./src/Posty5.SocialPublisherWorkspace)   | Social workspace management   | 4.0.0   | [📦 NuGet](https://www.nuget.org/packages/Posty5.SocialPublisherWorkspace)  |
| [Posty5.SocialPublisherPost](./src/Posty5.SocialPublisherPost)             | Social publishing post client | 5.0.0   | [📦 NuGet](https://www.nuget.org/packages/Posty5.SocialPublisherPost)       |
| [Posty5.Store](./src/Posty5.Store)                                         | Online store client           | 4.0.0   | [📦 NuGet](https://www.nuget.org/packages/Posty5.Store)                     |

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
