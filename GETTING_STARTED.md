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
Console.WriteLine($"URL: {qrCode.QrCodeLandingPageURL}");
Console.WriteLine($"ID: {qrCode.Id}");
```

Run your application:

```bash
dotnet run
```

## Publishing a long video (up to 60 minutes)

Long video is priced by **duration** — 50 credits for every started 5 minutes —
so quote it before you commit:

```csharp
var quote = await client.GetLongVideoQuoteAsync(videoUrl);

if (!quote.WithinLimit)
    throw new InvalidOperationException(quote.Reason);

Console.WriteLine($"{quote.DurationSeconds}s costs {quote.Credits} credits");

// Which targets can actually take a video this long?
foreach (var p in quote.Platforms.Where(p => !p.Accepted))
    Console.WriteLine($"{p.Platform}: {p.Reason}");
```

Then publish. Pass a `Stream` to upload, or a URL string for a video you already
host:

```csharp
using var video = File.OpenRead("recording.mp4");

var progress = new Progress<UploadProgress>(p =>
    Console.WriteLine($"Uploaded {p.BytesTransferred} of {p.TotalBytes} bytes"));

var result = await client.PublishLongVideoToWorkspaceAsync(
    workspaceId: "workspace_123",
    video: video,
    youtube: new YouTubeConfig
    {
        Title = "Full workshop recording",
        Description = "The complete two-part session.",
        Tags = new List<string> { "workshop" }
    },
    videoContentType: "video/mp4",
    progress: progress,
    cancellationToken: cancellationToken);

Console.WriteLine($"Charged {result.Credits} credits for {result.DurationSeconds}s");

// Platforms disagree about "long" — Instagram Reels stop at 15 minutes.
foreach (var target in result.RefusedTargets)
    Console.WriteLine($"{target.Platform} skipped: {target.Reason}");
```

### What to know

- **The duration is measured server-side.** There is no duration parameter, and
  one would be ignored — a client-supplied duration would be a client-supplied
  price.
- **The 100-second `HttpClient` timeout does not apply.** Long uploads run with
  it disabled; use the `CancellationToken` to bound them instead.
- **Progress needs a seekable stream** to know the total. A non-seekable stream
  still uploads, but `UploadProgress.Percentage` will be null.
- **Cancelling mid-upload** aborts the transfer, and the post is never created,
  so nothing is charged.
- Requires the `socialMediaPublisher.longVideoPost` plan feature.

### Interrupted uploads resume

The upload goes up in 8MiB chunks, so a dropped connection costs one chunk
rather than the hour of footage before it. Persist the upload URL to continue
the same transfer later, even after the process restarts:

```csharp
var result = await client.PublishLongVideoToWorkspaceAsync(
    workspaceId: "workspace_123",
    video: video,
    youtube: youtubeConfig,
    videoContentType: "video/mp4",
    onUploadUrl: url => File.WriteAllText("upload-url.txt", url),
    resumeFrom: File.Exists("upload-url.txt") ? File.ReadAllText("upload-url.txt") : null,
    cancellationToken: cancellationToken);
```

Resuming needs a **seekable** stream — retrying and resuming both seek to a byte
offset. A non-seekable stream still uploads, through the single-PUT path.

### Cancelling for good

Cancelling leaves the uploaded bytes on the server, because in most interfaces
"pause" and "cancel" are the same button and a user who paused a 40-minute video
does not expect to start over. Where the cancellation really is final, say so and
the server stops holding megabytes nobody will claim:

```csharp
await client.PublishLongVideoToWorkspaceAsync(
    workspaceId: "workspace_123",
    video: video,
    youtube: youtubeConfig,
    videoContentType: "video/mp4",
    cancellationToken: cancellationToken,
    terminateOnCancel: true);
```

For an upload URL you persisted earlier and have decided not to resume, discard
it directly. It never throws — a cleanup that fails is not worth an exception,
since the server expires abandoned uploads after 24 hours anyway:

```csharp
if (await ResumableUpload.TerminateAsync(savedUploadUrl))
    File.Delete("upload-url.txt");
```

### Rescheduling

```csharp
await client.ReschedulePostAsync("post_123", DateTime.UtcNow.AddDays(1));
await client.ReschedulePostAsync("post_123", "now");
```

Free, and only valid while the post is still pending with a future publish time.

## Check the key, find the ids, then act

Before a paid call, an integration — or an AI agent driving one — usually needs
three things: is this key valid and what can it spend, what does the call cost,
and which id does the call take. Each has one method:

```csharp
using Posty5.Account;

var account = new AccountClient(httpClient);
var me = await account.GetCurrentAsync();          // 401 → Posty5AuthenticationException
var costs = await account.GetOperationCostsAsync(); // live prices, public

var storeId    = (await new StoreClient(httpClient).ListStoresAsync()).First().Id;
var templateId = (await new QRCodeTemplateClient(httpClient).ListPublicTemplatesAsync()).Items.First().Id;
var accountId  = (await new SocialPublisherAccountClient(httpClient).ListAsync()).Items.First().Id;
```

| Need | Method | Route |
| --- | --- | --- |
| Who am I: key, owner, plan, credits, MCP settings | `AccountClient.GetCurrentAsync()` | `GET /api/api-key/current` |
| Balance and per-feature counters | `AccountClient.GetCreditsAsync()` | `GET /api/user/current/credits` |
| Credit history (cursor-paged) | `AccountClient.GetCreditUsageAsync(filters?, pagination?)` | `GET /api/user/current/credit-usage` |
| Totals over that history | `AccountClient.GetCreditUsageSummaryAsync(filters?)` | `GET /api/user/current/credit-usage/summary` |
| Price of every operation | `AccountClient.GetOperationCostsAsync(activeOnly = true)` | `GET /api/plans/operation-costs` |
| Stores you can manage | `StoreClient.ListStoresAsync()` / `LookupStoresAsync(term?)` | `GET /api/store/lookup` |
| Connected social accounts | `SocialPublisherAccountClient.ListAsync(params?, pagination?)` / `LookupAsync(term?, platform?)` / `GetAsync(id)` | `GET /api/social-publisher-account[/lookup\|/{id}]` |
| QR code templates | `QRCodeTemplateClient.ListUserTemplatesAsync(term?, pagination?)` / `ListPublicTemplatesAsync(term?, schemeType?, pagination?)` | `GET /api/qr-code-template/user-lookup`, `/public-lookup` |

### Text posts and stories

```csharp
var posts = new SocialPublisherPostClient(httpClient);

var text = await posts.CreateTextPostToWorkspaceAsync(new CreateTextPostToWorkspaceRequest
{
    WorkspaceId = "workspace_123",
    Caption = "Our autumn menu is out.",
    Facebook = new TextPostFacebookConfig { Description = "Our autumn menu is out:", Link = "https://example.com/menu" }
});
// YouTube, Instagram and TikTok take no text: they come back in text.SkippedPlatforms.

var story = await posts.CreateStoryPostToAccountAsync(new CreateStoryPostToAccountRequest
{
    AccountId = "account_123",
    Kind = StoryKinds.Video,
    VideoURL = "https://example.com/story.mp4"   // media by URL in this release
});

var status = await posts.GetStatusAsync(story.Id);  // GET /{id}/status — fixed in 4.6.0
var removed = await posts.RemovePostAsync(text.Id); // take a PUBLISHED post down from the platforms
```

`DeletePostAsync` deletes a post that has **not** published yet; `RemovePostAsync`
deletes the media of one that **has**, platform by platform (Instagram and TikTok
cannot delete through their APIs and report `NotSupported`).

## Identifying your integration

Every request carries `X-Posty5-Client: posty5-dotnet/<Posty5.Core version>`, so
the API's logs can tell SDK traffic apart. Two options let you add to that:

```csharp
var options = new Posty5Options
{
    ApiKey = Environment.GetEnvironmentVariable("POSTY5_API_KEY"),

    // Sent on every request. X-API-Key is refused here (use ApiKey);
    // an X-Posty5-Client entry replaces the SDK's own label.
    DefaultHeaders = new() { ["X-Correlation-Id"] = correlationId },

    // Stamped as createdFrom on everything this client creates, for your own filtering.
    // Null keeps the package defaults: "dotnetPackage", and "dotnet" for store orders.
    CreatedFrom = "my-crm"
};
```

There are no automatic retries: a request that fails is reported, not repeated,
so a create is never sent twice.

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
