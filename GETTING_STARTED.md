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

### Rescheduling

```csharp
await client.ReschedulePostAsync("post_123", DateTime.UtcNow.AddDays(1));
await client.ReschedulePostAsync("post_123", "now");
```

Free, and only valid while the post is still pending with a future publish time.

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
