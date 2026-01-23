# Posty5.SocialPublisherWorkspace

Social Publisher Workspace management client for Posty5 .NET SDK. This package allows you to create and manage workspaces for social media publishing with multi-platform account integration (YouTube, Facebook, Instagram, TikTok).

## Features

- 🏢 **Workspace Management**: Create, read, update, and delete workspaces
- 🖼️ **Image Upload**: Optional workspace logo/image upload to R2 storage
- 📱 **Multi-Platform**: Integration with YouTube, Facebook, Instagram, and TikTok
- 🔍 **Advanced Filtering**: Search and filter workspaces by multiple criteria
- 📊 **Pagination**: Efficient pagination for large workspace lists
- 🏷️ **Tagging Support**: Organize workspaces with custom tags

## Installation

```bash
dotnet add package Posty5.SocialPublisherWorkspace
```

## Quick Start

```csharp
using Posty5.Core;
using Posty5.SocialPublisherWorkspace;

// Initialize the client
var config = new Posty5Config("your-api-key");
var httpClient = new Posty5HttpClient(config);
var client = new SocialPublisherWorkspaceClient(httpClient);

// Create a workspace
var workspaceId = await client.CreateAsync(new CreateWorkspaceRequest
{
    Name = "My Workspace",
    Description = "Workspace for social media publishing"
});
```

## Usage Examples

### Creating Workspaces

#### Create Without Image

```csharp
var workspaceId = await client.CreateAsync(new CreateWorkspaceRequest
{
    Name = "My Workspace",
    Description = "Central hub for social media publishing",
    Tag = "production"
});

Console.WriteLine($"Workspace created: {workspaceId}");
```

#### Create With Image (Logo)

```csharp
using var logoStream = File.OpenRead("workspace-logo.png");

var workspaceId = await client.CreateAsync(
    new CreateWorkspaceRequest
    {
        Name = "Branded Workspace",
        Description = "Workspace with custom branding",
        Tag = "production",
        RefId = "workspace-001"
    },
    logoStream,
    "image/png"
);

Console.WriteLine($"Workspace created with logo: {workspaceId}");
```

#### Create With Image from Memory

```csharp
byte[] imageBytes = await DownloadImageFromUrl("https://example.com/logo.png");
using var memoryStream = new MemoryStream(imageBytes);

var workspaceId = await client.CreateAsync(
    new CreateWorkspaceRequest
    {
        Name = "Downloaded Logo Workspace",
        Description = "Using downloaded image"
    },
    memoryStream,
    "image/png"
);
```

### Retrieving Workspaces

#### Get Single Workspace

```csharp
var workspace = await client.GetAsync("workspace-id");

Console.WriteLine($"Name: {workspace.Name}");
Console.WriteLine($"ID: {workspace.Id}");

// Check connected social media accounts
if (workspace.Account.Youtube != null)
{
    Console.WriteLine($"YouTube: {workspace.Account.Youtube.Name}");
    Console.WriteLine($"Status: {workspace.Account.Youtube.Status}");
}

if (workspace.Account.Facebook != null)
{
    Console.WriteLine($"Facebook: {workspace.Account.Facebook.Name}");
}

if (workspace.Account.Instagram != null)
{
    Console.WriteLine($"Instagram: {workspace.Account.Instagram.Name}");
}

if (workspace.Account.Tiktok != null)
{
    Console.WriteLine($"TikTok: {workspace.Account.Tiktok.Name}");
}
```

#### List All Workspaces

```csharp
var workspaces = await client.ListAsync(
    pagination: new PaginationParams
    {
        Page = 1,
        PageSize = 20
    }
);

Console.WriteLine($"Total Workspaces: {workspaces.Total}");
foreach (var workspace in workspaces.Items)
{
    Console.WriteLine($"- {workspace.Name}: {workspace.Description}");
    if (!string.IsNullOrEmpty(workspace.ImageUrl))
    {
        Console.WriteLine($"  Logo: {workspace.ImageUrl}");
    }
}
```

### Filtering Workspaces

#### Filter by Name

```csharp
var workspaces = await client.ListAsync(
    new ListWorkspacesParams
    {
        Name = "Marketing"
    }
);
```

#### Filter by Tag

```csharp
var productionWorkspaces = await client.ListAsync(
    new ListWorkspacesParams
    {
        Tag = "production"
    }
);
```

#### Multiple Filters

```csharp
var workspaces = await client.ListAsync(
    new ListWorkspacesParams
    {
        Tag = "client-work",
        Name = "Agency"
    },
    new PaginationParams { Page = 1, PageSize = 10 }
);
```

### Updating Workspaces

#### Update Without Changing Image

```csharp
await client.UpdateAsync("workspace-id", new UpdateWorkspaceRequest
{
    Name = "Updated Workspace Name",
    Description = "Updated description",
    Tag = "updated-tag"
});
```

#### Update With New Image

```csharp
using var newLogo = File.OpenRead("new-logo.png");

await client.UpdateAsync(
    "workspace-id",
    new UpdateWorkspaceRequest
    {
        Name = "Rebranded Workspace",
        Description = "With new branding"
    },
    newLogo,
    "image/png"
);
```

### Deleting Workspaces

```csharp
await client.DeleteAsync("workspace-id");
Console.WriteLine("Workspace deleted successfully");
```

## Common Use Cases

### Agency Management

Organize client workspaces:

```csharp
// Create workspace for each client
var clientWorkspaces = new[]
{
    new { Name = "Client A - Social Media", Tag = "client-a" },
    new { Name = "Client B - Marketing", Tag = "client-b" },
    new { Name = "Client C - Brand Management", Tag = "client-c" }
};

foreach (var client in clientWorkspaces)
{
    var id = await workspaceClient.CreateAsync(new CreateWorkspaceRequest
    {
        Name = client.Name,
        Description = $"Workspace for {client.Tag}",
        Tag = client.Tag
    });
    
    Console.WriteLine($"Created workspace for {client.Name}: {id}");
}
```

### Multi-Brand Management

Manage multiple brands from one account:

```csharp
var brands = new[]
{
    new { Name = "Brand X", Logo = "brand-x-logo.png", RefId = "brand-x-001" },
    new { Name = "Brand Y", Logo = "brand-y-logo.png", RefId = "brand-y-001" }
};

foreach (var brand in brands)
{
    using var logoStream = File.OpenRead(brand.Logo);
    
    var workspaceId = await client.CreateAsync(
        new CreateWorkspaceRequest
        {
            Name = brand.Name,
            Description = $"Official {brand.Name} workspace",
            RefId = brand.RefId
        },
        logoStream
    );
}
```

## API Reference

### Methods

| Method | Description | Returns |
|--------|-------------|---------|
| `ListAsync()` | List/search workspaces | `PaginationResponse<WorkspaceSampleDetails>` |
| `GetAsync()` | Get workspace by ID | `WorkspaceModel` |
| `CreateAsync()` | Create with optional image | `string` (workspace ID) |
| `UpdateAsync()` | Update with optional image | `Task` |
| `DeleteAsync()` | Delete workspace | `Task` |

### Models

**`WorkspaceModel`** - Full workspace details
- `Id` - Workspace ID
- `Name` - Workspace name
- `Account` - Social media accounts (YouTube, Facebook, Instagram, TikTok)

**`WorkspaceSampleDetails`** - Simplified for lists
- `Id`, `Name`, `Description` - Basic info
- `ImageUrl` - Workspace logo URL
- `CreatedAt` - Creation timestamp

**`AccountDetails`** - Social platform account
- `Link`, `Name`, `Thumbnail` - Account info
- `PlatformAccountId` - Platform-specific ID
- `Status` - Account status (active, inactive, authenticationExpired)

**`WorkspaceAccount`** - Social media accounts
- `Youtube`, `Facebook`, `Instagram`, `Tiktok` - Platform accounts
- `FacebookPlatformPageId`, `InstagramPlatformAccountId` - Platform IDs

**`CreateWorkspaceRequest` / `UpdateWorkspaceRequest`**
- `Name`, `Description` - Required
- `Tag`, `RefId` - Optional

## Image Upload

### Supported Image Formats

- PNG (`image/png`) - Default
- JPEG (`image/jpeg`)
- WebP (`image/webp`)
- GIF (`image/gif`)

### Image Upload Examples

```csharp
// PNG
using var png = File.OpenRead("logo.png");
await client.CreateAsync(request, png, "image/png");

// JPEG
using var jpg = File.OpenRead("logo.jpg");
await client.CreateAsync(request, jpg, "image/jpeg");

// WebP
using var webp = File.OpenRead("logo.webp");
await client.CreateAsync(request, webp, "image/webp");
```

### Upload Process

1. **Client calls Create/Update** with `hasImage: true`
2. **API returns** workspace ID + pre-signed R2 upload URL
3. **Client uploads** image directly to R2 using the pre-signed URL
4. **API updates** workspace with image URL

## Error Handling

```csharp
try
{
    var workspaceId = await client.CreateAsync(request);
}
catch (InvalidOperationException ex)
{
    Console.WriteLine($"Operation failed: {ex.Message}");
}
catch (HttpRequestException ex)
{
    Console.WriteLine($"Network error: {ex.Message}");
}
```

## Pagination

Handle large workspace lists:

```csharp
int currentPage = 1;
int pageSize = 50;

while (true)
{
    var result = await client.ListAsync(
        pagination: new PaginationParams
        {
            Page = currentPage,
            PageSize = pageSize
        }
    );

    foreach (var workspace in result.Items)
    {
        Console.WriteLine($"Processing: {workspace.Name}");
    }

    if (result.Items.Count < pageSize || currentPage * pageSize >= result.Total)
    {
        break;
    }

    currentPage++;
}
```

## Best Practices

### 1. Use Descriptive Names

```csharp
// ✅ Good
Name = "Marketing Team - Q1 2024"
Name = "Client ABC - Social Media Management"

// ❌ Avoid
Name = "Workspace 1"
Name = "Test"
```

### 2. Organize with Tags

```csharp
Tag = "client-work"
Tag = "internal"
Tag = "production"
Tag = "staging"
```

### 3. Link to External Systems

```csharp
RefId = "crm-account-12345"
RefId = "project-mgmt-789"
```

### 4. Optimize Image Sizes

Compress images before uploading to improve load times and reduce storage costs.

## Related Packages

- **Posty5.Core** - Core functionality and HTTP client
- **Posty5.HtmlHosting** - HTML page hosting
- **Posty5.ShortLink** - Short link management
- **Posty5.QRCode** - QR code generation

## Support

- **Documentation**: [https://posty5.com/docs](https://posty5.com/docs)
- **API Reference**: [https://posty5.com/api](https://posty5.com/api)
- **GitHub**: [https://github.com/posty5/dotnet-sdk](https://github.com/posty5/dotnet-sdk)

## License

MIT License - see LICENSE file for details
