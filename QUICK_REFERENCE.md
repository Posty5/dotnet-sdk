# Posty5 .NET SDK - Quick Reference

## Installation

```bash
dotnet add package Posty5.Core
dotnet add package Posty5.QRCode
dotnet add package Posty5.ShortLink
dotnet add package Posty5.HtmlHosting
dotnet add package Posty5.SocialPublisher
```

## Initialize Client

```csharp
using Posty5.Core.Configuration;
using Posty5.Core.Http;

var options = new Posty5Options
{
    ApiKey = "your-api-key",
    BaseUrl = "https://api.posty5.com"
};
var httpClient = new Posty5HttpClient(options);
```

## QR Codes

```csharp
using Posty5.QRCode;
using Posty5.QRCode.Models;

var qrClient = new QRCodeClient(httpClient);

// URL QR Code
var qr = await qrClient.CreateUrlAsync(new CreateUrlQRCodeRequest
{
    Name = "My QR",
    QrCodeTarget = new UrlQRTarget { Url = "https://example.com" }
});

// WiFi QR Code
var wifi = await qrClient.CreateWifiAsync(new CreateWifiQRCodeRequest
{
    Name = "WiFi",
    QrCodeTarget = new WifiQRTarget { Ssid = "Network", Password = "pass123" }
});

// List QR Codes
var list = await qrClient.ListAsync();
```

## Short Links

```csharp
using Posty5.ShortLink;
using Posty5.ShortLink.Models;

var linkClient = new ShortLinkClient(httpClient);

// Create
var link = await linkClient.CreateAsync(new CreateShortLinkRequest
{
    Name = "Campaign",
    TargetUrl = "https://example.com/long-url",
    CustomSlug = "my-link"
});

// Get
var details = await linkClient.GetAsync(link.Id);

// Update
await linkClient.UpdateAsync(link.Id, new UpdateShortLinkRequest
{
    TargetUrl = "https://example.com/new-url"
});

// Delete
await linkClient.DeleteAsync(link.Id);
```

## HTML Hosting

```csharp
using Posty5.HtmlHosting;
using Posty5.HtmlHosting.Models;

var htmlClient = new HtmlHostingClient(httpClient);

// Create
var page = await htmlClient.CreateAsync(new CreateHtmlHostingRequest
{
    Name = "Landing Page",
    HtmlContent = "<html><body><h1>Hello</h1></body></html>"
});

// Update
await htmlClient.UpdateAsync(page.Id, new UpdateHtmlHostingRequest
{
    HtmlContent = "<html><body><h1>Updated</h1></body></html>"
});
```

## Social Publisher

```csharp
using Posty5.SocialPublisher;
using Posty5.SocialPublisher.Models;

var workspaceClient = new WorkspaceClient(httpClient);
var taskClient = new TaskClient(httpClient);

// Create Workspace
var workspace = await workspaceClient.CreateAsync(new CreateWorkspaceRequest
{
    Name = "Marketing",
    Description = "Social campaigns"
});

// Create Task
var task = await taskClient.CreateAsync(new CreateTaskRequest
{
    WorkspaceId = workspace.Id,
    Title = "Product Launch",
    Content = "Check out our new product!",
    Platforms = new List<SocialPlatform> { SocialPlatform.Facebook },
    ScheduledAt = DateTime.UtcNow.AddHours(2)
});

// Publish
await taskClient.PublishAsync(task.Id);
```

## Error Handling

```csharp
using Posty5.Core.Exceptions;

try
{
    var qr = await qrClient.GetAsync("invalid-id");
}
catch (Posty5NotFoundException ex)
{
    Console.WriteLine("Not found: " + ex.Message);
}
catch (Posty5AuthenticationException ex)
{
    Console.WriteLine("Auth failed: " + ex.Message);
}
catch (Posty5Exception ex)
{
    Console.WriteLine($"Error: {ex.Message} (Status: {ex.StatusCode})");
}
```

## Pagination

```csharp
using Posty5.Core.Models;

var result = await qrClient.ListAsync(
    pagination: new PaginationParams
    {
        PageNumber = 0,
        PageSize = 20
    }
);

Console.WriteLine($"Total: {result.TotalCount}");
Console.WriteLine($"Has more: {result.HasNextPage}");
```

## Dependency Injection (ASP.NET Core)

```csharp
// Program.cs
builder.Services.AddSingleton<Posty5Options>(new Posty5Options
{
    ApiKey = builder.Configuration["Posty5:ApiKey"]
});
builder.Services.AddSingleton<Posty5HttpClient>();
builder.Services.AddScoped<QRCodeClient>();
builder.Services.AddScoped<ShortLinkClient>();
```
