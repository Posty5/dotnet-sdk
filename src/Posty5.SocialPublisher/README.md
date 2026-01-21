# Posty5.SocialPublisher

Official Posty5 SDK for managing social media publishing workspaces and tasks. Create, manage, and schedule social media posts across multiple platforms including YouTube, Facebook, Instagram, and TikTok.

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

`Posty5.SocialPublisher` is the **social media management client** for the Posty5 platform. This package enables you to programmatically create and manage workspaces and publishing tasks for multi-platform social media distribution.

### What is Social Publisher?

Social Publisher allows you to manage social media content distribution across multiple platforms from a single interface. It includes:

- **Workspaces** - Containers that group your connected social media accounts (YouTube, Facebook, Instagram, TikTok)
- **Tasks** - Scheduled posts with content, media, and platform-specific settings

### Key Capabilities

#### Workspace Management

- **Create Workspaces** - Organize accounts by brand, client, or campaign
- **List & Search** - Retrieve workspaces with pagination and filtering
- **Update Workspaces** - Modify names, descriptions, and settings
- **Delete Workspaces** - Remove workspaces when no longer needed

#### Task Management

- **Create Tasks** - Schedule posts with title, content, and media
- **Multi-Platform Publishing** - Publish to YouTube, Facebook, Instagram, TikTok simultaneously
- **Schedule Posts** - Set specific publish dates and times
- **Update Tasks** - Modify content, schedule, or platforms
- **Task Status Tracking** - Monitor Draft, Scheduled, Published, Failed, and Cancelled statuses
- **List & Filter Tasks** - Search by workspace, status, or date range

### Role in the Posty5 Ecosystem

This package works seamlessly with other Posty5 SDK modules:

- Link social posts to `Posty5.ShortLink` shortened URLs
- Include `Posty5.QRCode` QR codes in visual content
- Drive traffic to `Posty5.HtmlHosting` landing pages

Perfect for **social media managers**, **marketing teams**, **agencies**, **content creators**, and **businesses** who need automated posting, campaign management, multi-platform distribution, scheduling, and brand management.

---

## 📥 Installation

Install via NuGet Package Manager:

```bash
dotnet add package Posty5.SocialPublisher
```

Or via Package Manager Console:

```powershell
Install-Package Posty5.SocialPublisher
```

The `Posty5.Core` package will be automatically installed as a dependency.

---

## 🚀 Quick Start

### Workspace Management

```csharp
using Posty5.Core.Configuration;
using Posty5.Core.Http;
using Posty5.SocialPublisher;
using Posty5.SocialPublisher.Models;

// Initialize the HTTP client with your API key
var options = new Posty5Options
{
    ApiKey = "your-api-key", // Get from https://studio.posty5.com/account/settings?tab=APIKeys
    BaseUrl = "https://api.posty5.com"
};

var httpClient = new Posty5HttpClient(options);

// Create workspace client
var workspaceClient = new WorkspaceClient(httpClient);

// Create a new workspace
var workspaceRequest = new CreateWorkspaceRequest
{
    Name = "My Brand Workspace",
    Description = "Workspace for managing brand social media accounts"
};

var workspace = await workspaceClient.CreateAsync(workspaceRequest);
Console.WriteLine($"Created workspace: {workspace.Name} (ID: {workspace.Id})");

// List all workspaces
var workspaces = await workspaceClient.ListAsync(
    pagination: new Core.Models.PaginationParams
    {
        PageNumber = 0,
        PageSize = 10
    }
);

Console.WriteLine($"Total workspaces: {workspaces.TotalCount}");
```

### Task Management

```csharp
// Create task client
var taskClient = new TaskClient(httpClient);

// Create a scheduled post
var taskRequest = new CreateTaskRequest
{
    WorkspaceId = workspace.Id!,
    Title = "Product Launch Announcement",
    Content = "🚀 Exciting news! Our new product is now available. Check it out!",
    Platforms = new List<SocialPlatform>
    {
        SocialPlatform.Facebook,
        SocialPlatform.Instagram,
        SocialPlatform.Twitter
    },
    ScheduledAt = DateTime.UtcNow.AddHours(2)
};

var task = await taskClient.CreateAsync(taskRequest);
Console.WriteLine($"Created task: {task.Title} (ID: {task.Id})");
Console.WriteLine($"Scheduled for: {task.ScheduledAt}");

// List tasks in workspace
var tasks = await taskClient.ListAsync(
    new ListTasksParams { WorkspaceId = workspace.Id },
    new Core.Models.PaginationParams { PageNumber = 0, PageSize = 20 }
);

Console.WriteLine($"Total tasks: {tasks.TotalCount}");
```

---

## 📖 API Reference

### WorkspaceClient Methods

#### Create Operations

**`CreateAsync(CreateWorkspaceRequest request, CancellationToken cancellationToken = default)`**

- Creates a new workspace
- Returns `WorkspaceModel`

#### Read Operations

**`GetAsync(string id, CancellationToken cancellationToken = default)`**

- Gets a workspace by ID
- Returns `WorkspaceModel`

**`ListAsync(ListWorkspacesParams? listParams = null, PaginationParams? pagination = null, CancellationToken cancellationToken = default)`**

- Lists workspaces with filtering and pagination
- Returns `PaginationResponse<WorkspaceModel>`

#### Update Operations

**`UpdateAsync(string id, UpdateWorkspaceRequest request, CancellationToken cancellationToken = default)`**

- Updates an existing workspace
- Returns `WorkspaceModel`

#### Delete Operations

**`DeleteAsync(string id, CancellationToken cancellationToken = default)`**

- Deletes a workspace
- Returns `bool` indicating success

---

### TaskClient Methods

#### Create Operations

**`CreateAsync(CreateTaskRequest request, CancellationToken cancellationToken = default)`**

- Creates a new publishing task
- Returns `TaskModel`

#### Read Operations

**`GetAsync(string id, CancellationToken cancellationToken = default)`**

- Gets a task by ID
- Returns `TaskModel`

**`ListAsync(ListTasksParams? listParams = null, PaginationParams? pagination = null, CancellationToken cancellationToken = default)`**

- Lists tasks with filtering and pagination
- Returns `PaginationResponse<TaskModel>`

#### Update Operations

**`UpdateAsync(string id, UpdateTaskRequest request, CancellationToken cancellationToken = default)`**

- Updates an existing task
- Returns `TaskModel`

#### Delete Operations

**`DeleteAsync(string id, CancellationToken cancellationToken = default)`**

- Deletes a task
- Returns `bool` indicating success

---

## 🎯 Working with Workspaces

### Create a Workspace

```csharp
var request = new CreateWorkspaceRequest
{
    Name = "Client ABC - Social Media",
    Description = "Social media accounts for Client ABC marketing campaigns"
};

var workspace = await workspaceClient.CreateAsync(request);
```

### Update Workspace Details

```csharp
var updateRequest = new UpdateWorkspaceRequest
{
    Name = "Client ABC - Updated",
    Description = "Updated description for workspace"
};

var updated = await workspaceClient.UpdateAsync(workspace.Id!, updateRequest);
```

### Search Workspaces

```csharp
var searchParams = new ListWorkspacesParams
{
    Search = "client"
};

var results = await workspaceClient.ListAsync(searchParams);
```

---

## 📱 Working with Tasks

### Create a Multi-Platform Post

```csharp
var taskRequest = new CreateTaskRequest
{
    WorkspaceId = "workspace-id",
    Title = "Summer Sale Announcement",
    Content = "🌞 Summer Sale! Get 50% off all items this weekend only! #SummerSale",
    Platforms = new List<SocialPlatform>
    {
        SocialPlatform.Facebook,
        SocialPlatform.Instagram,
        SocialPlatform.Twitter,
        SocialPlatform.LinkedIn
    },
    ScheduledAt = new DateTime(2026, 6, 1, 10, 0, 0, DateTimeKind.Utc)
};

var task = await taskClient.CreateAsync(taskRequest);
```

### Schedule a Post for Later

```csharp
var taskRequest = new CreateTaskRequest
{
    WorkspaceId = "workspace-id",
    Title = "Weekly Tip",
    Content = "💡 Pro tip: Always test your content before publishing!",
    Platforms = new List<SocialPlatform> { SocialPlatform.Twitter },
    ScheduledAt = DateTime.UtcNow.AddDays(1).Date.AddHours(9) // Tomorrow at 9 AM
};

var task = await taskClient.CreateAsync(taskRequest);
```

### Update Task Content

```csharp
var updateRequest = new UpdateTaskRequest
{
    Content = "Updated content with new information and hashtags #Updated #New"
};

var updated = await taskClient.UpdateAsync(task.Id!, updateRequest);
```

### Change Platforms

```csharp
var updateRequest = new UpdateTaskRequest
{
    Platforms = new List<SocialPlatform>
    {
        SocialPlatform.Facebook,
        SocialPlatform.Instagram
        // Removed Twitter and LinkedIn
    }
};

var updated = await taskClient.UpdateAsync(task.Id!, updateRequest);
```

---

## 🔍 Filtering and Search

### List Tasks by Workspace

```csharp
var taskParams = new ListTasksParams
{
    WorkspaceId = "workspace-id"
};

var tasks = await taskClient.ListAsync(taskParams);
```

### Filter by Status

```csharp
var taskParams = new ListTasksParams
{
    Status = Posty5.SocialPublisher.Models.TaskStatus.Scheduled
};

var scheduledTasks = await taskClient.ListAsync(taskParams);
```

### Filter by Date Range

```csharp
var taskParams = new ListTasksParams
{
    FromDate = DateTime.UtcNow,
    ToDate = DateTime.UtcNow.AddDays(7)
};

var upcomingTasks = await taskClient.ListAsync(taskParams);
```

---

## 📊 Task Statuses

Tasks can have the following statuses:

- **`Draft`** - Task created but not scheduled
- **`Scheduled`** - Task scheduled for future publishing
- **`Published`** - Task successfully published to platforms
- **`Failed`** - Task failed to publish
- **`Cancelled`** - Task was cancelled before publishing

```csharp
var task = await taskClient.GetAsync("task-id");

switch (task.Status)
{
    case Posty5.SocialPublisher.Models.TaskStatus.Draft:
        Console.WriteLine("Task is in draft mode");
        break;
    case Posty5.SocialPublisher.Models.TaskStatus.Scheduled:
        Console.WriteLine($"Scheduled for: {task.ScheduledAt}");
        break;
    case Posty5.SocialPublisher.Models.TaskStatus.Published:
        Console.WriteLine($"Published at: {task.PublishedAt}");
        break;
    case Posty5.SocialPublisher.Models.TaskStatus.Failed:
        Console.WriteLine("Publishing failed");
        break;
    case Posty5.SocialPublisher.Models.TaskStatus.Cancelled:
        Console.WriteLine("Task was cancelled");
        break;
}
```

---

## 🎨 Supported Platforms

The SDK supports the following social media platforms:

```csharp
public enum SocialPlatform
{
    Facebook,
    Instagram,
    Twitter,
    LinkedIn,
    YouTube,
    TikTok
}
```

**Note**: You must connect your social media accounts in the Posty5 Studio before you can publish to them.

---

## 💡 Common Use Cases

### Content Calendar Management

```csharp
// Schedule a week's worth of content
var dates = Enumerable.Range(1, 7)
    .Select(day => DateTime.UtcNow.AddDays(day).Date.AddHours(10));

foreach (var date in dates)
{
    await taskClient.CreateAsync(new CreateTaskRequest
    {
        WorkspaceId = workspaceId,
        Title = $"Daily Post - {date:MMM dd}",
        Content = "Daily content...",
        Platforms = new List<SocialPlatform> { SocialPlatform.Twitter },
        ScheduledAt = date
    });
}
```

### Campaign Management

```csharp
// Create workspace for campaign
var campaignWorkspace = await workspaceClient.CreateAsync(new CreateWorkspaceRequest
{
    Name = "Holiday Campaign 2026",
    Description = "Social media campaign for holiday season"
});

// Schedule campaign posts
var posts = new[]
{
    ("Teaser", DateTime.UtcNow.AddDays(7)),
    ("Launch", DateTime.UtcNow.AddDays(14)),
    ("Reminder", DateTime.UtcNow.AddDays(21))
};

foreach (var (title, date) in posts)
{
    await taskClient.CreateAsync(new CreateTaskRequest
    {
        WorkspaceId = campaignWorkspace.Id!,
        Title = title,
        Content = $"Content for {title}",
        Platforms = new List<SocialPlatform>
        {
            SocialPlatform.Facebook,
            SocialPlatform.Instagram
        },
        ScheduledAt = date
    });
}
```

---

## 🔒 Error Handling

```csharp
using Posty5.Core.Exceptions;

try
{
    var task = await taskClient.CreateAsync(new CreateTaskRequest
    {
        WorkspaceId = "invalid-workspace-id",
        Title = "Test Post",
        Content = "Content",
        Platforms = new List<SocialPlatform> { SocialPlatform.Twitter }
    });
}
catch (Posty5NotFoundException ex)
{
    Console.WriteLine("Workspace not found");
}
catch (Posty5ValidationException ex)
{
    Console.WriteLine($"Validation error: {ex.Message}");
}
catch (Posty5Exception ex)
{
    Console.WriteLine($"API error: {ex.Message}");
}
```

---

## 🎯 Best Practices

1. **Organize with Workspaces**
   - Use separate workspaces for different brands or clients
   - Group related accounts together

2. **Schedule in Advance**
   - Plan content calendar ahead of time
   - Use consistent posting times

3. **Handle Time Zones**
   - Always use UTC for scheduled times
   - Convert to local time in your UI

4. **Monitor Task Status**
   - Check task status after scheduling
   - Handle failed tasks appropriately

5. **Bulk Operations**
   - Use pagination for large lists
   - Implement batch processing for multiple tasks

---

## 🤝 Related Packages

- **[Posty5.Core](../Posty5.Core)** - Core HTTP client (required dependency)
- **[Posty5.QRCode](../Posty5.QRCode)** - QR code generation and management
- **[Posty5.ShortLink](../Posty5.ShortLink)** - URL shortening functionality
- **[Posty5.HtmlHosting](../Posty5.HtmlHosting)** - HTML page hosting

---

## 📄 License

MIT License - See [LICENSE](../../LICENSE) file in the root directory.

---

## 🔗 Resources

- **Documentation**: [https://posty5.com/docs](https://posty5.com/docs)
- **API Reference**: [https://posty5.com/api](https://posty5.com/api)
- **Get API Key**: [https://studio.posty5.com/account/settings?tab=APIKeys](https://studio.posty5.com/account/settings?tab=APIKeys)
- **Connect Social Accounts**: [https://studio.posty5.com/social-publisher](https://studio.posty5.com/social-publisher)
- **Support**: [https://posty5.com/contact-us](https://posty5.com/contact-us)

---

Made with ❤️ by the Posty5 Team
