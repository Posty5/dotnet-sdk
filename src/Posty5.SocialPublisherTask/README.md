# Posty5.SocialPublisherTask

Official Posty5 SDK for managing social media publishing tasks. Publish videos to YouTube Shorts, TikTok, Facebook Reels, and Instagram Reels with a unified, developer-friendly C# API.

---

## 🌟 What is Posty5?

**Posty5** is a comprehensive suite of free online tools designed to enhance your digital marketing and social media presence. With over 4+ powerful tools and counting, Posty5 provides everything you need to:

- 🔗 **Shorten URLs** - Create memorable, trackable short links
- 📱 **Generate QR Codes** - Transform URLs, WiFi credentials, contact cards, and more into scannable codes
- 🌐 **Host HTML Pages** - Deploy static HTML pages with dynamic variables and form submission handling
- 📢 **Automate Social Media** - Schedule and manage social media posts across multiple platforms
- 📊 **Track Performance** - Monitor and analyze your digital marketing efforts

Posty5 empowers businesses, marketers, and developers to streamline their online workflows—all from a unified control panel.

**Learn more:** [https://posty5.com](https://posty5.com)

---

## 📦 About This Package

`Posty5.SocialPublisherTask` is the **task management client** for the Posty5 Social Media Publisher. This package enables you to programmatically publish short-form videos to multiple social media platforms simultaneously from a single API call.

### Key Capabilities

- **Multi-Platform Publishing** - Publish to YouTube, TikTok, Facebook, and Instagram in one API call
- **Flexible Video Sources** - Upload files, provide URLs, or repost from other platforms (auto-detected)
- **Smart Thumbnail Handling** - Upload files or provide URLs for thumbnail images
- **Platform-Specific Configuration** - Customize titles, descriptions, captions, tags, and privacy settings per platform
- **Schedule Publishing** - Publish immediately or schedule for optimal engagement times
- **Repost Detection** - Automatically detect and repost from Facebook, TikTok, and YouTube Shorts URLs
- **Task Status Tracking** - Monitor publishing progress and platform-specific status
- **Tag & Reference System** - Organize tasks using custom tags and reference IDs
- **🔐 API Key Filtering** - Scope resources by API key for multi-tenant applications
- **Pagination & Filtering** - Search tasks by workspace, status, tag, or reference ID

---

## 📥 Installation

Install via NuGet Package Manager:

```bash
dotnet add package Posty5.SocialPublisherTask
```

Or via Package Manager Console:

```powershell
Install-Package Posty5.SocialPublisherTask
```

---

## 🚀 Quick Start

Here's a minimal example to get you started:

```csharp
using Posty5.Core.Configuration;
using Posty5.Core.Http;
using Posty5.SocialPublisherTask;
using Posty5.SocialPublisherTask.Models;

// Initialize the HTTP client with your API key
var options = new Posty5Options
{
    ApiKey = "your-api-key" // Get from studio.posty5.com/account/settings?tab=APIKeys
};
var httpClient = new Posty5HttpClient(options);

// Create task client
var client = new SocialPublisherTaskClient(httpClient);

// Read video file
using var videoStream = File.OpenRead("video.mp4");

// Publish video to YouTube Shorts
var taskId = await client.PublishShortVideoAsync(
    workspaceId: "workspace-123", // Your workspace ID
    video: videoStream,
    platforms: new List<string> { "youtube" },
    youtube: new YouTubeConfig
    {
        Title = "My First YouTube Short",
        Description = "Published using Posty5 SDK",
        Tags = new List<string> { "shorts", "video", "tutorial" }
    }
);

Console.WriteLine($"Task created: {taskId}");

// Check task status
var status = await client.GetStatusAsync(taskId);
Console.WriteLine($"Publishing status: {status.CurrentStatus}");
Console.WriteLine($"YouTube: {status.YouTube?.PostInfo?.CurrentStatus}");
```

---

## 📚 API Reference & Examples

### PublishShortVideoAsync

Publish a short video to one or more social media platforms. This is the main method for creating publishing tasks. It automatically detects video source type (Stream for file upload, string for URL or platform-specific repost URL) and handles all upload logic.

**Parameters:**

- `workspaceId` (string, **required**): Workspace ID containing connected social accounts
- `video` (object, **required**): Video source - `Stream` (file upload) or `string` (URL or repost URL)
- `thumbnail` (object?, optional): Thumbnail image - `Stream` or `string` URL
- `platforms` (List<string>?, optional): Target platforms ("youtube", "tiktok", "facebook", "instagram")
- `youtube` (YouTubeConfig?, optional): YouTube configuration
- `tiktok` (TikTokConfig?, optional): TikTok configuration
- `facebook` (FacebookPageConfig?, optional): Facebook configuration
- `instagram` (InstagramConfig?, optional): Instagram configuration
- `schedule` (object?, optional): "now" (string) or `DateTime` for scheduling
- `tag` (string?, optional): Custom tag for filtering
- `refId` (string?, optional): Your internal reference ID
- `videoContentType` (string?, optional): Content type for video file (default: "video/mp4")
- `thumbnailContentType` (string?, optional): Content type for thumbnail file

**Returns:** `Task<string>` - Created task ID

#### Example - Upload Video File

```csharp
using var videoStream = File.OpenRead("video.mp4");
using var thumbStream = File.OpenRead("thumb.jpg");

var taskId = await client.PublishShortVideoAsync(
    workspaceId: "workspace-123",
    video: videoStream,
    thumbnail: thumbStream,
    platforms: new List<string> { "youtube" },
    youtube: new YouTubeConfig
    {
        Title = "Product Launch Video",
        Description = "Introducing our new product line for 2024",
        Tags = new List<string> { "product", "launch", "2024" },
        MadeForKids = false
    },
    tag: "product-launch",
    refId: "PROD-LAUNCH-001"
);

Console.WriteLine($"Published to YouTube: {taskId}");
```

#### Example - Video URL with Thumbnail URL

```csharp
// Publish using URLs (no file upload needed)
var taskId = await client.PublishShortVideoAsync(
    workspaceId: "workspace-123",
    video: "https://cdn.example.com/videos/promo.mp4",
    thumbnail: "https://cdn.example.com/images/thumb.jpg",
    platforms: new List<string> { "youtube", "tiktok" },
    youtube: new YouTubeConfig
    {
        Title = "Summer Sale Announcement",
        Description = "Check out our summer collection",
        Tags = new List<string> { "sale", "summer", "fashion" }
    },
    tiktok: new TikTokConfig
    {
        Caption = "Summer sale is here! 🔥 #SummerSale #Fashion",
        PrivacyLevel = "public",
        DisableDuet = false,
        DisableStitch = false,
        DisableComment = false
    }
);

Console.WriteLine($"Published to YouTube and TikTok: {taskId}");
```

#### Example - Multi-Platform Publishing

```csharp
using var videoStream = File.OpenRead("video.mp4");

var taskId = await client.PublishShortVideoAsync(
    workspaceId: "workspace-123",
    video: videoStream,
    platforms: new List<string> { "youtube", "tiktok", "facebook", "instagram" },
    
    // YouTube configuration
    youtube: new YouTubeConfig
    {
        Title = "How to Use Our Product",
        Description = "Step-by-step tutorial",
        Tags = new List<string> { "tutorial" }
    },
    
    // TikTok configuration
    tiktok: new TikTokConfig
    {
        Caption = "Easy tutorial! 🎯 #Tutorial",
        PrivacyLevel = "public"
    },
    
    // Facebook configuration
    facebook: new FacebookPageConfig
    {
        Title = "Product Tutorial",
        Description = "Learn in 60s"
    },
    
    // Instagram configuration
    instagram: new InstagramConfig
    {
        Description = "Quick tutorial 📱 #Product",
        ShareToFeed = true
    }
);

Console.WriteLine($"Published to all platforms: {taskId}");
```

#### Example - Repost from TikTok/YouTube/Facebook

```csharp
// Automatically detect and repost from TikTok
var taskId = await client.PublishShortVideoAsync(
    workspaceId: "workspace-123",
    video: "https://www.tiktok.com/@username/video/1234567890", // TikTok URL
    platforms: new List<string> { "youtube" }, // Repost to YouTube
    youtube: new YouTubeConfig
    {
        Title = "Viral TikTok Repost",
        Description = "Sharing this viral moment from TikTok",
        Tags = new List<string> { "tiktok", "repost" }
    }
);

Console.WriteLine($"Reposted: {taskId}");
```

#### Example - Scheduled Publishing

```csharp
// Schedule video for future publication
var publishDate = new DateTime(2024, 12, 25, 12, 0, 0, DateTimeKind.Utc);

using var videoStream = File.OpenRead("video.mp4");

var taskId = await client.PublishShortVideoAsync(
    workspaceId: "workspace-123",
    video: videoStream,
    platforms: new List<string> { "youtube" },
    schedule: publishDate, // Schedule for specific date/time
    youtube: new YouTubeConfig
    {
        Title = "Merry Christmas! 🎄"
    }
);

Console.WriteLine($"Scheduled for {publishDate}: {taskId}");
```

---

### ListAsync

Search and retrieve publishing tasks with pagination and filtering options.

**Parameters:**

- `listParams` (ListTasksParams?, optional): Filter criteria
  - `WorkspaceId`, `CurrentStatus`, `Tag`, `RefId`, `Caption`, `Numbering`
- `pagination` (PaginationParams?, optional)

**Returns:** `Task<PaginationResponse<TaskModel>>`

**Example:**

```csharp
var tasks = await client.ListAsync(
    new ListTasksParams { WorkspaceId = "workspace-123" },
    new PaginationParams { PageNumber = 1, PageSize = 20 }
);

Console.WriteLine($"Total tasks: {tasks.Pagination.TotalCount}");
foreach (var task in tasks.Data)
{
    Console.WriteLine($"{task.Numbering}: {task.Caption} - {task.CurrentStatus}");
}
```

---

### GetStatusAsync

Retrieve detailed status information for a specific publishing task.

**Parameters:**

- `id` (string): Task ID

**Returns:** `Task<TaskStatusResponse>`

**Example:**

```csharp
var status = await client.GetStatusAsync("task-id");

Console.WriteLine($"Overall Status: {status.CurrentStatus}");

if (status.YouTube != null)
{
    Console.WriteLine($"YouTube Status: {status.YouTube.PostInfo.CurrentStatus}");
    if (!string.IsNullOrEmpty(status.YouTube.PostInfo.VideoURL))
        Console.WriteLine($"Video: {status.YouTube.PostInfo.VideoURL}");
}
```

---

### GetNextAndPreviousAsync

Get the IDs of the next and previous tasks for navigation purposes.

**Parameters:**

- `id` (string): Current task ID

**Returns:** `Task<NextPreviousResponse>`

**Example:**

```csharp
var nav = await client.GetNextAndPreviousAsync("current-task-id");

if (nav.NextId != null)
    Console.WriteLine($"Next task: {nav.NextId}");

if (nav.PreviousId != null)
    Console.WriteLine($"Previous task: {nav.PreviousId}");
```

---

### GetDefaultSettingsAsync

Retrieve default configuration settings for social media publishing.

**Returns:** `Task<DefaultSettingsResponse>`

**Example:**

```csharp
var settings = await client.GetDefaultSettingsAsync();
Console.WriteLine($"Max Video Size: {SocialPublisherTaskClient.MaxVideoUploadSizeBytes}");
```

---

## 🔒 Error Handling

Methods throw exceptions from `Posty5.Core.Exceptions`.

```csharp
try
{
    await client.GetStatusAsync("invalid-id");
}
catch (Posty5NotFoundException)
{
    Console.WriteLine("Task not found");
}
catch (Posty5Exception ex)
{
    Console.WriteLine($"API Error: {ex.Message}");
}
```

---

## 📖 Resources

- **Official Guides**: [https://guide.posty5.com](https://guide.posty5.com)
- **API Reference**: [https://docs.posty5.com](https://docs.posty5.com)
- **Source Code**: [https://github.com/Posty5/dotnet-sdk](https://github.com/Posty5/dotnet-sdk)

---

## 📦 Packages

This SDK ecosystem contains the following tool packages:

| Package | Description | Version | NuGet |
| --- | --- | --- | --- |
| [Posty5.Core](../Posty5.Core) | Core HTTP client and models | 1.0.0 | [📦 NuGet](https://www.nuget.org/packages/Posty5.Core) |
| [Posty5.ShortLink](../Posty5.ShortLink) | URL shortener client | 1.0.0 | [📦 NuGet](https://www.nuget.org/packages/Posty5.ShortLink) |
| [Posty5.QRCode](../Posty5.QRCode) | QR code generator client | 1.0.0 | [📦 NuGet](https://www.nuget.org/packages/Posty5.QRCode) |
| [Posty5.HtmlHosting](../Posty5.HtmlHosting) | HTML hosting client | 1.0.0 | [📦 NuGet](https://www.nuget.org/packages/Posty5.HtmlHosting) |
| [Posty5.HtmlHostingVariables](../Posty5.HtmlHostingVariables) | Variable management | 1.0.0 | [📦 NuGet](https://www.nuget.org/packages/Posty5.HtmlHostingVariables) |
| [Posty5.HtmlHostingFormSubmission](../Posty5.HtmlHostingFormSubmission) | Form submission management | 1.0.0 | [📦 NuGet](https://www.nuget.org/packages/Posty5.HtmlHostingFormSubmission) |
| [Posty5.SocialPublisherWorkspace](../Posty5.SocialPublisherWorkspace) | Social workspace management | 1.0.0 | [📦 NuGet](https://www.nuget.org/packages/Posty5.SocialPublisherWorkspace) |
| [Posty5.SocialPublisherTask](../Posty5.SocialPublisherTask) | Social publishing task client | 1.0.0 | [📦 NuGet](https://www.nuget.org/packages/Posty5.SocialPublisherTask) |

---

## 📄 License

MIT License - see [LICENSE](../../LICENSE) file for details.

---

Made with ❤️ by the Posty5 team
