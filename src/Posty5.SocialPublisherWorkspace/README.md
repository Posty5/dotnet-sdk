# Posty5.SocialPublisherWorkspace

Official Posty5 SDK for managing social media publishing workspaces. Create, manage, and organize workspaces to group your social media accounts and streamline multi-platform content distribution in .NET.

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

`Posty5.SocialPublisherWorkspace` is the **workspace management client** for the Posty5 Social Media Publisher. This package enables you to programmatically create and manage workspaces (organizations) that group your social media accounts for streamlined content distribution.

### Key Capabilities

- **Create Workspaces** - Programmatically create new workspaces with custom names, descriptions, and logos
- **List & Search** - Retrieve all workspaces with pagination, filtering, and search capabilities
- **Update Workspaces** - Modify workspace details including name, description, and logo image
- **Delete Workspaces** - Remove workspaces when no longer needed
- **Tag & Reference System** - Organize workspaces using custom tags and reference IDs
- **🔐 API Key Filtering** - Scope resources by API key for multi-tenant applications
- **Account Management** - View connected social media accounts (YouTube, Facebook, Instagram, TikTok)
- **Logo Upload** - Upload custom workspace logos with automatic image optimization

---

## 📥 Installation

Install via NuGet Package Manager:

```bash
dotnet add package Posty5.SocialPublisherWorkspace
```

Or via Package Manager Console:

```powershell
Install-Package Posty5.SocialPublisherWorkspace
```

---

## 🚀 Quick Start

Here's a minimal example to get you started:

```csharp
using Posty5.Core.Configuration;
using Posty5.Core.Http;
using Posty5.SocialPublisherWorkspace;
using Posty5.SocialPublisherWorkspace.Models;

// Initialize the HTTP client with your API key
var options = new Posty5Options
{
    ApiKey = "your-api-key" // Get from studio.posty5.com/account/settings?tab=APIKeys
};
var httpClient = new Posty5HttpClient(options);

// Create workspace client
var client = new SocialPublisherWorkspaceClient(httpClient);

// Create a new workspace
var workspaceId = await client.CreateAsync(new CreateWorkspaceRequest
{
    Name = "My Brand Workspace",
    Description = "Workspace for managing social media accounts",
    Tag = "brand-2024",
    RefId = "WORKSPACE-001"
});

Console.WriteLine($"Created workspace: {workspaceId}");

// Get workspace details
var workspace = await client.GetAsync(workspaceId);
Console.WriteLine($"Workspace: {workspace.Name}");

if (workspace.Account.YouTube != null)
    Console.WriteLine($"Connected YouTube: {workspace.Account.YouTube.Name}");

// List all workspaces
var workspaces = await client.ListAsync(
    null,
    new PaginationParams { PageNumber = 1, PageSize = 10 }
);

Console.WriteLine($"Found {workspaces.Pagination.TotalCount} workspaces");
```

---

## 📚 API Reference & Examples

### CreateAsync

Create a new social media workspace with optional logo image upload.

**Parameters:**

- `data` (CreateWorkspaceRequest): Workspace configuration
  - `Name` (string): Workspace name
  - `Description` (string): Workspace description
  - `Tag` (string?): Custom tag for filtering
  - `RefId` (string?): Your internal reference ID
- `logoStream` (Stream?, optional): Logo image stream
- `contentType` (string, optional): Image content type (default: "image/png")

**Returns:** `Task<string>` - Created workspace ID

**Example:**

```csharp
// Create workspace without logo
var workspaceId = await client.CreateAsync(new CreateWorkspaceRequest
{
    Name = "Client A - Social Media",
    Description = "Managing social accounts for Client A",
    Tag = "client-a"
});

Console.WriteLine($"Workspace created: {workspaceId}");

// Create workspace with logo upload
using var logoStream = File.OpenRead("logo.png");
var workspaceWithLogo = await client.CreateAsync(
    new CreateWorkspaceRequest
    {
        Name = "Brand Workspace",
        Description = "Workspace with custom branding"
    },
    logoStream,
    "image/png"
);
```

---

### GetAsync

Retrieve detailed information about a specific workspace by ID.

**Parameters:**

- `id` (string): Workspace ID

**Returns:** `Task<WorkspaceModel>` - Workspace details including connected accounts

**Example:**

```csharp
var workspace = await client.GetAsync("workspace-id-here");

Console.WriteLine($"Workspace: {workspace.Name}");

// Check connected accounts
if (workspace.Account.YouTube != null)
{
    Console.WriteLine($"YouTube channel: {workspace.Account.YouTube.Name}");
    Console.WriteLine($"Status: {workspace.Account.YouTube.Status}");
}

if (workspace.Account.TikTok != null)
{
    Console.WriteLine($"TikTok account: {workspace.Account.TikTok.Name}");
}
```

---

### ListAsync

Search and retrieve workspaces with pagination and filtering options.

**Parameters:**

- `listParams` (ListWorkspacesParams?, optional): Search and filter options
  - `Name` (string?), `Description` (string?), `Tag` (string?), `RefId` (string?)
- `pagination` (PaginationParams?, optional)

**Returns:** `Task<PaginationResponse<WorkspaceSampleDetails>>`

**Example:**

```csharp
// Get all workspaces
var allWorkspaces = await client.ListAsync(
    null,
    new PaginationParams { PageNumber = 1, PageSize = 20 }
);

foreach (var ws in allWorkspaces.Data)
{
    Console.WriteLine($"- {ws.Name}: {ws.Description}");
}

// Search by name
var searchResults = await client.ListAsync(new ListWorkspacesParams
{
    Name = "brand"
});
```

---

### UpdateAsync

Update workspace details including name, description, and optional logo image.

**Parameters:**

- `id` (string): Workspace ID to update
- `data` (UpdateWorkspaceRequest): Updated workspace data
  - `Name`, `Description`, `Tag`, `RefId`
- `logoStream` (Stream?, optional): New logo image stream
- `contentType` (string, optional)

**Returns:** `Task`

**Example:**

```csharp
// Update details
await client.UpdateAsync("workspace-id", new UpdateWorkspaceRequest
{
    Name = "Updated Workspace Name",
    Description = "New description"
});

// Update with new logo
using var newLogoStream = File.OpenRead("new-logo.png");
await client.UpdateAsync(
    "workspace-id",
    new UpdateWorkspaceRequest { Name = "Workspace", Description = "Desc" },
    newLogoStream
);
```

---

### DeleteAsync

Delete a workspace.

**Parameters:**

- `id` (string): Workspace ID to delete

**Returns:** `Task`

**Example:**

```csharp
await client.DeleteAsync("workspace-id-to-delete");
Console.WriteLine("Workspace deleted successfully");
```

---

## 🔒 Error Handling

Methods throw exceptions from `Posty5.Core.Exceptions`.

```csharp
try
{
    await client.GetAsync("invalid-id");
}
catch (Posty5NotFoundException)
{
    Console.WriteLine("Workspace not found");
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
