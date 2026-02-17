# Posty5.HtmlHosting

Deploy and manage static HTML pages with the Posty5 .NET SDK. This package provides a complete C# client for creating, updating, and managing hosted HTML pages with features like GitHub integration, form submission tracking, monetization, and analytics.

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

`Posty5.HtmlHosting` is a **specialized tool package** for deploying and managing static HTML pages through the Posty5 platform. It enables developers to quickly host landing pages, product pages, portfolios, and any static HTML content with professional features.

### Key Capabilities

- **📁 File Upload Hosting** - Upload HTML files directly to high-performance cloud storage
- **🐙 GitHub Integration** - Deploy HTML files directly from GitHub repositories
- **📊 Analytics & Tracking** - Monitor page views, visitor locations, devices, and more
- **💰 Monetization** - Generate revenue through embedded advertisements
- **📝 Form Submission Tracking** - Capture and track form submissions automatically
- **🔄 Dynamic Variables** - Inject real-time data into your static pages
- **📱 Google Sheets Integration** - Auto-save form data to Google Sheets
- **🔐 API Key Filtering** - Scope resources by API key for multi-tenant applications
- **⚡ High Performance** - Fast global CDN delivery with caching
- **🔒 Secure Hosting** - HTTPS enabled with 24/7 security monitoring

### Role in the Posty5 Ecosystem

This package works seamlessly with other Posty5 SDK packages:

- Use `Posty5.HtmlHostingVariables` to manage dynamic variables
- Use `Posty5.HtmlHostingFormSubmission` to handle form submissions
- Use `Posty5.ShortLink` to create shortened URLs for your pages
- Use `Posty5.QRCode` to generate QR codes linking to your pages

Perfect for **developers**, **marketers**, **designers**, and **entrepreneurs** who need fast, reliable HTML hosting with built-in tracking and monetization capabilities.

---

## 📥 Installation

Install via NuGet Package Manager:

```bash
dotnet add package Posty5.HtmlHosting
```

Or via Package Manager Console:

```powershell
Install-Package Posty5.HtmlHosting
```

---

## 🚀 Quick Start

Here's a minimal example to get you started:

```csharp
using System.Text;
using Posty5.Core.Configuration;
using Posty5.Core.Http;
using Posty5.HtmlHosting;
using Posty5.HtmlHosting.Models;

// Initialize the HTTP client with your API key
var options = new Posty5Options
{
    ApiKey = "your-api-key" // Get from https://studio.posty5.com/account/settings?tab=APIKeys
};
var httpClient = new Posty5HttpClient(options);

// Create the HTML Hosting client
var htmlHosting = new HtmlHostingClient(httpClient);

// Create and deploy an HTML page
var htmlContent = @"
<!DOCTYPE html>
<html>
<head>
  <title>My Landing Page</title>
</head>
<body>
  <h1>Welcome to My Page!</h1>
  <p>This page is hosted on Posty5.</p>
</body>
</html>";

using var fileStream = new MemoryStream(Encoding.UTF8.GetBytes(htmlContent));

var page = await htmlHosting.CreateWithFileAsync(
    new CreateHtmlPageFileRequest
    {
        Name = "My First Landing Page", // Page name for your dashboard
        FileName = "landing.html" // File name
    },
    fileStream
);

Console.WriteLine($"Page URL: {page.ShorterLink}");
Console.WriteLine($"Page ID: {page.Id}");
```

---

## 📚 API Reference & Examples

### Creating HTML Pages

#### CreateWithFileAsync

Upload an HTML file to create a hosted page.

**Parameters:**

- `data` (CreateHtmlPageFileRequest): Configuration for the page
  - `Name` (string): Display name for the page in your dashboard
  - `FileName` (string): Name of the HTML file
  - `CustomLandingId` (string?, optional): Custom URL identifier
  - `IsEnableMonetization` (bool?, optional): Enable ad monetization
  - `AutoSaveInGoogleSheet` (bool?, optional): Auto-save form data to Google Sheets
  - `Tag` (string?, optional): Tag for categorization
  - `RefId` (string?, optional): Your custom reference ID
- `fileStream` (Stream): The HTML file content stream
- `contentType` (string, optional): Content type (default: "text/html")

**Returns:** `Task<HtmlPageFileResponse>`

- `Id` (string): Unique page ID
- `ShorterLink` (string): Public URL to access the page
- `FileUrl` (string): Direct URL to the uploaded file

**Example:**

```csharp
// Basic page creation
var htmlContent = "<html><body><h1>Hello World</h1></body></html>";
using var fileStream = new MemoryStream(Encoding.UTF8.GetBytes(htmlContent));

var page = await htmlHosting.CreateWithFileAsync(
    new CreateHtmlPageFileRequest
    {
        Name = "Simple Page",
        FileName = "index.html"
    },
    fileStream
);

Console.WriteLine($"Live URL: {page.ShorterLink}");
```

```csharp
// Page with custom URL and monetization
var page = await htmlHosting.CreateWithFileAsync(
    new CreateHtmlPageFileRequest
    {
        Name = "Product Launch Page",
        FileName = "product.html",
        CustomLandingId = "product-2024", // Custom URL: posty5.com/product-2024
        IsEnableMonetization = true, // Enable ads for revenue
        Tag = "marketing",
        RefId = "campaign-spring-2024"
    },
    fileStream
);
```

```csharp
// Page with form submission tracking
var page = await htmlHosting.CreateWithFileAsync(
    new CreateHtmlPageFileRequest
    {
        Name = "Contact Form",
        FileName = "contact.html",
        AutoSaveInGoogleSheet = true, // Auto-save form data to Google Sheets
        Tag = "forms"
    },
    fileStream
);
```

---

#### CreateWithGithubFileAsync

Deploy an HTML file directly from a GitHub repository.

**Parameters:**

- `data` (CreateHtmlPageGithubRequest): Configuration for the page
  - `Name` (string): Display name for the page
  - `GithubInfo` (GithubInfo): GitHub file information
    - `FileURL` (string): Full GitHub file URL (e.g., `https://github.com/user/repo/blob/main/index.html`)
  - `CustomLandingId` (string?, optional): Custom URL identifier
  - `IsEnableMonetization` (bool?, optional): Enable monetization
  - `AutoSaveInGoogleSheet` (bool?, optional): Auto-save form data
  - `Tag` (string?, optional): Tag for categorization
  - `RefId` (string?, optional): Your custom reference ID

**Returns:** `Task<HtmlPageGithubResponse>`

- `Id` (string): Unique page ID
- `ShorterLink` (string): Public URL to access the page
- `GithubInfo` (GithubInfo): GitHub file information

**Example:**

```csharp
// Deploy from GitHub
var page = await htmlHosting.CreateWithGithubFileAsync(new CreateHtmlPageGithubRequest
{
    Name = "Portfolio Site",
    GithubInfo = new GithubInfo
    {
        FileURL = "https://github.com/username/portfolio/blob/main/index.html"
    }
});

Console.WriteLine($"Deployed URL: {page.ShorterLink}");
```

```csharp
// GitHub deployment with all options
var page = await htmlHosting.CreateWithGithubFileAsync(new CreateHtmlPageGithubRequest
{
    Name = "Open Source Landing Page",
    GithubInfo = new GithubInfo
    {
        FileURL = "https://github.com/Netflix/netflix.github.com/blob/master/index.html"
    },
    CustomLandingId = "oss-project",
    IsEnableMonetization = false, // No ads on this page
    Tag = "open-source",
    RefId = "github-deploy-001"
});
```

---

### Retrieving HTML Pages

#### GetAsync

Retrieve details of a specific HTML page by ID.

**Parameters:**

- `id` (string): The unique page ID

**Returns:** `Task<HtmlPageModel>` - Complete page details

**Example:**

```csharp
var page = await htmlHosting.GetAsync("page-id-123");

Console.WriteLine($"Page Name: {page.Name}");
Console.WriteLine($"URL: {page.ShorterLink}");
Console.WriteLine($"Created: {page.CreatedAt}");

if (page.SourceType == "github")
{
    Console.WriteLine($"GitHub URL: {page.GithubInfo?.FileURL}");
}
```

---

#### ListAsync

Search and filter HTML pages with pagination.

**Parameters:**

- `listParams` (ListHtmlPagesParams, optional): Filter criteria
  - `Name` (string): Search by page name
  - `SourceType` (string): Filter by source type ('file' or 'github')
  - `Tag` (string): Filter by tag
  - `RefId` (string): Filter by reference ID
  - `IsEnableMonetization` (bool?): Filter by monetization status
  - `AutoSaveInGoogleSheet` (bool?): Filter by Google Sheets integration
  - `IsTemp` (bool?): Filter temporary pages
- `pagination` (PaginationParams, optional): Pagination options
  - `PageNumber` (int): Page number (default: 1) -- _Note: C# uses PageNumber vs TS 'page'_
  - `PageSize` (int): Items per page (default: 10)

**Returns:** `Task<PaginationResponse<HtmlPageModel>>`

**Example:**

```csharp
// Get all pages with pagination
var result = await htmlHosting.ListAsync(
    null,
    new PaginationParams { PageNumber = 1, PageSize = 20 }
);

Console.WriteLine($"Found {result.Pagination.TotalItems} pages");
foreach (var page in result.Data)
{
    Console.WriteLine($"- {page.Name}: {page.ShorterLink}");
}
```

```csharp
// Search by name
var result = await htmlHosting.ListAsync(
    new ListHtmlPagesParams { Name = "landing" },
    new PaginationParams { PageNumber = 1, PageSize = 10 }
);
```

```csharp
// Filter by multiple criteria
var filtered = await htmlHosting.ListAsync(new ListHtmlPagesParams
{
    SourceType = "file", // Only uploaded files
    IsEnableMonetization = true, // Only monetized pages
    Tag = "marketing" // Tagged as 'marketing'
});
```

```csharp
// Get pages by your reference ID
var myPages = await htmlHosting.ListAsync(new ListHtmlPagesParams
{
    RefId = "campaign-2024-q1"
});
```

---

#### LookupAsync

Get a simplified list of pages (ID and name only). Useful for dropdowns and selection lists.

**Returns:** `Task<List<HtmlPageLookupItem>>` - List of objects with Id and Name.

**Example:**

```csharp
var pages = await htmlHosting.LookupAsync();

// Use in a dropdown or list
foreach (var page in pages)
{
    Console.WriteLine($"ID: {page.Id}, Name: {page.Name}");
}
```

---

#### LookupFormsAsync

Get form IDs detected in an HTML page (useful for form submission tracking).

**Parameters:**

- `id` (string): HTML page ID

**Returns:** `Task<List<FormLookupItem>>` - List of form information

**Example:**

```csharp
var forms = await htmlHosting.LookupFormsAsync("page-id-123");

Console.WriteLine($"Found {forms.Count} forms on this page");
foreach (var form in forms)
{
    Console.WriteLine($"Form ID: {form.Id}, Name: {form.Name}");
}
```

---

### Updating HTML Pages

#### UpdateWithFileAsync

Update an existing page with a new HTML file.

**Parameters:**

- `id` (string): Page ID to update
- `data` (UpdateHtmlPageFileRequest): Update configuration
  - `Name` (string?): New page name
  - `FileName` (string): New file name
  - `CustomLandingId` (string?): New custom URL
  - `IsEnableMonetization` (bool?): Toggle monetization
  - `AutoSaveInGoogleSheet` (bool?): Toggle Google Sheets
  - `Tag` (string?): New tag
  - `RefId` (string?): New reference ID
- `fileStream` (Stream): New HTML file content stream

**Returns:** `Task<HtmlPageFileResponse>`

**Example:**

```csharp
var updatedContent = "<html><body><h1>Content has been updated!</h1></body></html>";
using var fileStream = new MemoryStream(Encoding.UTF8.GetBytes(updatedContent));

var updated = await htmlHosting.UpdateWithFileAsync(
    "page-id-123",
    new UpdateHtmlPageFileRequest
    {
        Name = "Updated Landing Page",
        FileName = "landing-v2.html"
    },
    fileStream
);

Console.WriteLine($"Updated URL: {updated.ShorterLink}");
```

---

#### UpdateWithGithubFileAsync

Update a page to use a different GitHub file or update GitHub settings.

**Parameters:**

- `id` (string): Page ID to update
- `data` (UpdateHtmlPageGithubRequest): Update configuration
  - `Name` (string?): New page name
  - `GithubInfo` (GithubInfo): New GitHub file information
  - ... other optional fields same as create

**Returns:** `Task<HtmlPageGithubResponse>`

**Example:**

```csharp
var updated = await htmlHosting.UpdateWithGithubFileAsync(
    "page-id-123",
    new UpdateHtmlPageGithubRequest
    {
        Name = "Updated from GitHub",
        GithubInfo = new GithubInfo
        {
            FileURL = "https://github.com/username/repo/blob/main/updated.html"
        },
        IsEnableMonetization = false
    }
);
```

---

### Page Operations

#### CleanCacheAsync

Clear the cache for a page to force fresh content delivery. Useful after updating content.

**Parameters:**

- `id` (string): HTML page ID

**Returns:** `Task`

**Example:**

```csharp
// Clear cache after updating external content
await htmlHosting.CleanCacheAsync("page-id-123");
Console.WriteLine("Cache cleared - fresh content will be served");
```

---

#### DeleteAsync

Permanently delete an HTML page.

**Parameters:**

- `id` (string): HTML page ID to delete

**Returns:** `Task`

**Example:**

```csharp
await htmlHosting.DeleteAsync("page-id-123");
Console.WriteLine("Page deleted successfully");
```

---

## 🔒 Error Handling

All methods may throw exceptions from `Posty5.Core.Exceptions`. Handle them appropriately:

```csharp
using Posty5.Core.Exceptions;

try
{
    var page = await htmlHosting.GetAsync("invalid-id");
}
catch (Posty5AuthenticationException ex)
{
    Console.WriteLine("Invalid API key");
}
catch (Posty5NotFoundException ex)
{
    Console.WriteLine("Page not found");
}
catch (Posty5ValidationException ex)
{
    Console.WriteLine($"Invalid data: {ex.Message}");
}
catch (Posty5RateLimitException ex)
{
    Console.WriteLine("Rate limit exceeded");
}
catch (Posty5Exception ex)
{
    Console.WriteLine($"Unexpected error: {ex.Message}");
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

| Package                                                                 | Description                   | Version | NuGet                                                                       |
| ----------------------------------------------------------------------- | ----------------------------- | ------- | --------------------------------------------------------------------------- |
| [Posty5.Core](../Posty5.Core)                                           | Core HTTP client and models   | 1.0.0   | [📦 NuGet](https://www.nuget.org/packages/Posty5.Core)                      |
| [Posty5.ShortLink](../Posty5.ShortLink)                                 | URL shortener client          | 1.0.0   | [📦 NuGet](https://www.nuget.org/packages/Posty5.ShortLink)                 |
| [Posty5.QRCode](../Posty5.QRCode)                                       | QR code generator client      | 1.0.0   | [📦 NuGet](https://www.nuget.org/packages/Posty5.QRCode)                    |
| [Posty5.HtmlHosting](../Posty5.HtmlHosting)                             | HTML hosting client           | 1.0.0   | [📦 NuGet](https://www.nuget.org/packages/Posty5.HtmlHosting)               |
| [Posty5.HtmlHostingVariables](../Posty5.HtmlHostingVariables)           | Variable management           | 1.0.0   | [📦 NuGet](https://www.nuget.org/packages/Posty5.HtmlHostingVariables)      |
| [Posty5.HtmlHostingFormSubmission](../Posty5.HtmlHostingFormSubmission) | Form submission management    | 1.0.0   | [📦 NuGet](https://www.nuget.org/packages/Posty5.HtmlHostingFormSubmission) |
| [Posty5.SocialPublisherWorkspace](../Posty5.SocialPublisherWorkspace)   | Social workspace management   | 1.0.0   | [📦 NuGet](https://www.nuget.org/packages/Posty5.SocialPublisherWorkspace)  |
| [Posty5.SocialPublisherPost](../Posty5.SocialPublisherPost)             | Social publishing post client | 1.0.0   | [📦 NuGet](https://www.nuget.org/packages/Posty5.SocialPublisherPost)       |

---

## 🆘 Support

MIT License - see [LICENSE](../../LICENSE) file for details.

---

Made with ❤️ by the Posty5 team
