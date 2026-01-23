# Posty5.HtmlHosting

HTML Hosting management client for Posty5 .NET SDK. This package allows you to create, manage, and deploy static HTML pages through the Posty5 platform with R2 storage or GitHub integration.

## Features

- 📤 **File Upload**: Upload HTML files directly to R2 storage
- 🐙 **GitHub Integration**: Deploy HTML pages from GitHub repositories
- 🔍 **Search & Filter**: Advanced search and filtering capabilities
- 📊 **Analytics**: Track visitors and form submissions
- 💰 **Monetization**: Optional monetization for hosted pages
- 🔄 **Auto-sync**: Automatic Google Sheets integration for form data

## Installation

```bash
dotnet add package Posty5.HtmlHosting
```

## Quick Start

```csharp
using Posty5.Core;
using Posty5.HtmlHosting;

// Initialize the client
var config = new Posty5Config("your-api-key");
var httpClient = new Posty5HttpClient(config);
var client = new HtmlHostingClient(httpClient);

// Create a page from a file
using var fileStream = File.OpenRead("index.html");
var result = await client.CreateWithFileAsync(
    new CreateHtmlPageFileRequest 
    { 
        Name = "My Landing Page",
        FileName = "index.html"
    },
    fileStream
);

Console.WriteLine($"Page created: {result.ShorterLink}");
```

## Usage Examples

### Creating Pages

#### 1. Create from File (File Upload)

```csharp
// From a file on disk
using var fileStream = File.OpenRead("landing-page.html");
var result = await client.CreateWithFileAsync(
    new CreateHtmlPageFileRequest 
    { 
        Name = "Product Landing Page",
        FileName = "landing-page.html",
        IsEnableMonetization = true,
        Tag = "product-pages",
        RefId = "product-123"
    },
    fileStream,
    "text/html"
);

Console.WriteLine($"ID: {result.Id}");
Console.WriteLine($"Shorter Link: {result.ShorterLink}");
Console.WriteLine($"File URL: {result.FileUrl}");
```

#### 2. Create from String Content

```csharp
var htmlContent = @"
<!DOCTYPE html>
<html>
<head>
    <title>Welcome</title>
</head>
<body>
    <h1>Hello from Posty5!</h1>
</body>
</html>";

using var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(htmlContent));
var result = await client.CreateWithFileAsync(
    new CreateHtmlPageFileRequest 
    { 
        Name = "Simple Welcome Page",
        FileName = "index.html"
    },
    memoryStream,
    "text/html"
);
```

#### 3. Create from GitHub Repository

```csharp
var result = await client.CreateWithGithubFileAsync(
    new CreateHtmlPageGithubRequest
    {
        Name = "GitHub Hosted Page",
        GithubInfo = new GithubInfo
        {
            // Supports multiple GitHub URL formats
            FileURL = "https://raw.githubusercontent.com/owner/repo/main/index.html"
            // Also supports:
            // "https://github.com/owner/repo/blob/main/index.html"
            // "https://raw.githubusercontent.com/owner/repo/commit-sha/index.html"
        },
        AutoSaveInGoogleSheet = true,
        Tag = "github-pages"
    }
);

Console.WriteLine($"GitHub Page: {result.ShorterLink}");
```

### Updating Pages

#### 1. Update with New File

```csharp
var pageId = "existing-page-id";

using var fileStream = File.OpenRead("updated-page.html");
var result = await client.UpdateWithFileAsync(
    pageId,
    new UpdateHtmlPageFileRequest 
    { 
        Name = "Updated Landing Page",
        FileName = "updated-page.html"
    },
    fileStream
);

Console.WriteLine($"Updated: {result.ShorterLink}");
```

#### 2. Update from GitHub

```csharp
var pageId = "existing-page-id";

var result = await client.UpdateWithGithubFileAsync(
    pageId,
    new UpdateHtmlPageGithubRequest
    {
        Name = "Updated GitHub Page",
        GithubInfo = new GithubInfo
        {
            FileURL = "https://raw.githubusercontent.com/owner/repo/develop/new-page.html"
        }
    }
);
```

### Retrieving Pages

#### 1. Get Single Page

```csharp
var page = await client.GetAsync("page-id");

Console.WriteLine($"Name: {page.Name}");
Console.WriteLine($"Visitors: {page.NumberOfVisitors}");
Console.WriteLine($"Source Type: {page.SourceType}"); // "file" or "github"
Console.WriteLine($"Status: {page.Status}");

if (page.FormSubmission != null)
{
    Console.WriteLine($"Form Submissions: {page.FormSubmission.NumberOfFormSubmission}");
}
```

#### 2. List Pages with Filters

```csharp
var pages = await client.ListAsync(
    new ListHtmlPagesParams
    {
        Tag = "product-pages",
        SourceType = "file",
        IsEnableMonetization = true
    },
    new PaginationParams
    {
        Page = 1,
        PageSize = 20
    }
);

Console.WriteLine($"Total: {pages.Total}");
foreach (var page in pages.Data)
{
    Console.WriteLine($"- {page.Name}: {page.ShorterLink}");
}
```

#### 3. Search by Name

```csharp
var pages = await client.ListAsync(
    new ListHtmlPagesParams
    {
        Name = "Landing"
    }
);
```

#### 4. Get Lookup List (for Dropdowns)

```csharp
var lookupItems = await client.LookupAsync();

foreach (var item in lookupItems)
{
    Console.WriteLine($"{item.Id}: {item.Name}");
}
```

#### 5. Get Form IDs

```csharp
var forms = await client.LookupFormsAsync("page-id");

foreach (var form in forms)
{
    Console.WriteLine($"Form ID: {form.FormId}");
    Console.WriteLine($"Fields: {string.Join(", ", form.FormFields)}");
}
```

### Deleting and Cache Management

#### 1. Delete a Page

```csharp
await client.DeleteAsync("page-id");
Console.WriteLine("Page deleted successfully");
```

#### 2. Clear Cache

```csharp
await client.CleanCacheAsync("page-id");
Console.WriteLine("Cache cleared - fresh content will be served");
```

## Advanced Usage

### Custom Landing IDs (Branded URLs)

```csharp
var result = await client.CreateWithFileAsync(
    new CreateHtmlPageFileRequest 
    { 
        Name = "Premium Page",
        FileName = "index.html",
        CustomLandingId = "my-custom-id" // Max 32 characters, paid plans only
    },
    fileStream
);

// Results in URL like: https://posty5.com/my-custom-id
```

### Monetization Support

```csharp
var result = await client.CreateWithFileAsync(
    new CreateHtmlPageFileRequest 
    { 
        Name = "Monetized Page",
        FileName = "index.html",
        IsEnableMonetization = true // Enable revenue sharing
    },
    fileStream
);
```

### Google Sheets Auto-Save

```csharp
var result = await client.CreateWithFileAsync(
    new CreateHtmlPageFileRequest 
    { 
        Name = "Form Page",
        FileName = "contact-form.html",
        AutoSaveInGoogleSheet = true // Form submissions auto-saved to Google Sheets
    },
    fileStream
);
```

### Tracking with RefId and Tags

```csharp
// Use RefId to link to your internal system
// Use Tag for categorization and filtering
var result = await client.CreateWithFileAsync(
    new CreateHtmlPageFileRequest 
    { 
        Name = "Campaign Page",
        FileName = "index.html",
        RefId = "campaign-2024-Q1",
        Tag = "marketing-campaigns"
    },
    fileStream
);

// Later, filter by tag
var campaignPages = await client.ListAsync(
    new ListHtmlPagesParams { Tag = "marketing-campaigns" }
);
```

## API Reference

### Methods

| Method | Description | Returns |
|--------|-------------|---------|
| `CreateWithFileAsync()` | Create page with file upload | `HtmlPageFileResponse` |
| `CreateWithGithubFileAsync()` | Create page from GitHub | `HtmlPageGithubResponse` |
| `GetAsync()` | Get page by ID | `HtmlPageModel` |
| `ListAsync()` | List/search pages | `PaginationResponse<HtmlPageModel>` |
| `LookupAsync()` | Get simplified list | `List<HtmlPageLookupItem>` |
| `LookupFormsAsync()` | Get form IDs | `List<FormLookupItem>` |
| `UpdateWithFileAsync()` | Update with new file | `HtmlPageFileResponse` |
| `UpdateWithGithubFileAsync()` | Update from GitHub | `HtmlPageGithubResponse` |
| `DeleteAsync()` | Delete page | `Task` |
| `CleanCacheAsync()` | Clear page cache | `Task` |

### Models

- **`HtmlPageModel`** - Full page details with all fields
- **`CreateHtmlPageFileRequest`** - Request for file-based creation
- **`CreateHtmlPageGithubRequest`** - Request for GitHub-based creation
- **`UpdateHtmlPageFileRequest`** - Request for file-based update
- **`UpdateHtmlPageGithubRequest`** - Request for GitHub-based update
- **`ListHtmlPagesParams`** - Filter parameters for listing
- **`HtmlPageFileResponse`** - Simplified response for file operations
- **`HtmlPageGithubResponse`** - Simplified response for GitHub operations
- **`HtmlPageLookupItem`** - Lightweight model for dropdowns
- **`FormLookupItem`** - Form information

## Stream Handling

The SDK uses `Stream` for file uploads, giving you flexibility in how you provide content:

```csharp
// From file
using var fileStream = File.OpenRead("page.html");

// From string
var content = "<html>...</html>";
using var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(content));

// From byte array
var bytes = GetHtmlBytes();
using var byteStream = new MemoryStream(bytes);

// From HTTP response
var httpContent = await httpClient.GetAsync("https://example.com/page.html");
using var stream = await httpContent.Content.ReadAsStreamAsync();

// All work with CreateWithFileAsync
await client.CreateWithFileAsync(request, stream);
```

## Error Handling

```csharp
try
{
    var result = await client.CreateWithFileAsync(request, fileStream);
}
catch (InvalidOperationException ex)
{
    // Handle API errors (e.g., page not found, invalid upload config)
    Console.WriteLine($"Operation failed: {ex.Message}");
}
catch (HttpRequestException ex)
{
    // Handle network errors
    Console.WriteLine($"Network error: {ex.Message}");
}
```

## Source Types

The SDK supports two source types:

- **`file`** - HTML files uploaded to R2 storage
- **`github`** - HTML files fetched from GitHub repositories

Pages maintain their source type throughout their lifecycle.

## Status Values

Pages can have various status values:
- `new` - Newly created
- `pending` - Under review
- `approved` - Live and accessible
- `rejected` - Not approved for publication
- `fileIsNotFound` - Source file not found (GitHub only)

## Related Packages

- **Posty5.Core** - Core functionality and HTTP client
- **Posty5.ShortLink** - Short link management
- **Posty5.QRCode** - QR code generation and management

## Support

- **Documentation**: [https://posty5.com/docs](https://posty5.com/docs)
- **API Reference**: [https://posty5.com/api](https://posty5.com/api)
- **GitHub**: [https://github.com/posty5/dotnet-sdk](https://github.com/posty5/dotnet-sdk)

## License

MIT License - see LICENSE file for details
