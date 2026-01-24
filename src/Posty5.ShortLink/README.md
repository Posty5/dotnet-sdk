# Posty5.ShortLink

Create and manage branded short links with analytics tracking, custom slugs, and QR code generation using the .NET SDK. This package provides a client for building URL shortening solutions with editable destinations, comprehensive tracking, and monetization options.

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

`Posty5.ShortLink` is a **specialized tool package** for creating and managing URL shorteners on the Posty5 platform. It enables developers to build link management systems for marketing campaigns, social media, analytics tracking, and more.

### Key Capabilities

- **🔗 URL Shortening** - Transform long URLs into short, memorable links
- **🎨 Custom Slugs** - Create branded short links with custom aliases
- **🔄 Editable URLs** - Update destination URLs without changing the short link
- **📊 Analytics Tracking** - Monitor clicks, visitor counts, and last visitor dates
- **📱 Free QR Codes** - Automatic QR code generation for each short link
- **🏷️ Tag & Reference Support** - Organize links with custom tags and reference IDs
- **🎯 Landing Pages** - Create custom landing pages with titles and descriptions
- **💰 Monetization** - Enable partner earnings on short links
- **🔍 Advanced Filtering** - Search by name, URL, status, tag, or reference ID
- **🔐 API Key Filtering** - Scope resources by API key for multi-tenant applications
- **📝 CRUD Operations** - Complete create, read, update, delete operations

### Role in the Posty5 Ecosystem

This package works seamlessly with other Posty5 SDK packages:

- Combine with `Posty5.QRCode` for enhanced QR code customization
- Use with `Posty5.HtmlHosting` to create short links for hosted pages
- Build comprehensive marketing campaigns with full tracking and analytics

---

## 📥 Installation

Install via NuGet Package Manager:

```bash
dotnet add package Posty5.ShortLink
```

Or via Package Manager Console:

```powershell
Install-Package Posty5.ShortLink
```

---

## 🚀 Quick Start

Here's a minimal example to get you started:

```csharp
using Posty5.Core.Configuration;
using Posty5.Core.Http;
using Posty5.ShortLink;
using Posty5.ShortLink.Models;

// Initialize the HTTP client with your API key
var options = new Posty5Options
{
    ApiKey = "your-api-key" // Get from https://studio.posty5.com/account/settings?tab=APIKeys
};
var httpClient = new Posty5HttpClient(options);

// Create the Short Link client
var shortLinks = new ShortLinkClient(httpClient);

// Create a short link
var shortLink = await shortLinks.CreateAsync(new CreateShortLinkRequest
{
    Name = "Campaign Landing Page",
    BaseUrl = "https://example.com/long-url-to-campaign-page",
    CustomLandingId = "summer-sale", // Optional: Custom slug
    TemplateId = "template-123", // Optional: QR code template ID
    Tag = "marketing", // Optional: For organization
    RefId = "CAMPAIGN-001" // Optional: External reference
});

Console.WriteLine($"Short Link: {shortLink.ShorterLink}");
Console.WriteLine($"QR Code: {shortLink.QrCodeDownloadURL}");
Console.WriteLine($"Landing Page: {shortLink.QrCodeLandingPageURL}");

// List all short links
var allLinks = await shortLinks.ListAsync(
    null,
    new PaginationParams { PageNumber = 1, PageSize = 20 }
);

Console.WriteLine($"Total links: {allLinks.Pagination.TotalCount}");
foreach (var link in allLinks.Data)
{
    Console.WriteLine($"{link.Name}: {link.NumberOfVisitors} clicks");
}

// Update destination URL (short link stays the same!)
await shortLinks.UpdateAsync(shortLink.Id!, new UpdateShortLinkRequest
{
    BaseUrl = "https://example.com/updated-campaign-page",
    TemplateId = "template-123"
});

Console.WriteLine("✓ Destination updated - same short link, new target!");
```

---

## 📚 API Reference & Examples

### Creating Short Links

#### CreateAsync

Create a new short link with optional custom slug, landing page, and tracking parameters.

**Parameters:**

- `request` (CreateShortLinkRequest): Short link data
  - `BaseUrl` (string, **required**): Destination URL to redirect to
  - `Name` (string?, optional): Human-readable name for the link
  - `CustomLandingId` (string?, optional): Custom slug for branded short links
  - `TemplateId` (string?, optional): QR code template ID
  - `Tag` (string?, optional): Custom tag for grouping/filtering
  - `RefId` (string?, optional): External reference ID from your system
  - `IsEnableMonetization` (bool?, optional): Enable partner earnings
  - `PageInfo` (object?, optional): Landing page metadata

**Returns:** `Task<ShortLinkModel>` - Created short link details

**Example:**

```csharp
// Basic short link
var shortLink = await shortLinks.CreateAsync(new CreateShortLinkRequest
{
    BaseUrl = "https://example.com/product/awesome-widget",
    Name = "Product Page - Awesome Widget"
});

Console.WriteLine($"Share this: {shortLink.ShorterLink}");
```

```csharp
// Branded link
var brandedLink = await shortLinks.CreateAsync(new CreateShortLinkRequest
{
    BaseUrl = "https://example.com/summer-sale-2026",
    Name = "Summer Sale 2026",
    CustomLandingId = "summer-sale", // Creates: posty5.com/summer-sale
    Tag = "seasonal-campaigns",
    RefId = "SUMMER-2026"
});
```

---

### Retrieving Short Links

#### GetAsync

Retrieve complete details of a specific short link by ID.

**Parameters:**

- `id` (string): The unique short link ID

**Returns:** `Task<ShortLinkModel>` - Short link details

**Example:**

```csharp
var link = await shortLinks.GetAsync("short-link-id-123");

Console.WriteLine($"Short Link Details:");
Console.WriteLine($"  Name: {link.Name}");
Console.WriteLine($"  Short URL: {link.ShorterLink}");
Console.WriteLine($"  Destination: {link.BaseUrl}");
Console.WriteLine($"  Total Clicks: {link.NumberOfVisitors}");
```

---

#### ListAsync

Search and filter short links with advanced pagination and filtering options.

**Parameters:**

- `listParams` (ListShortLinksParams?, optional): Filter criteria
  - `Name` (string?): Filter by link name
  - `BaseUrl` (string?): Filter by destination URL
  - `Tag` (string?): Filter by tag
  - `RefId` (string?): Filter by reference ID
  - `Status` (string?): Filter by status
- `pagination` (PaginationParams?, optional): Pagination options

**Returns:** `Task<PaginationResponse<ShortLinkModel>>`

**Example:**

```csharp
var marketingLinks = await shortLinks.ListAsync(new ListShortLinksParams
{
    Tag = "marketing"
});

foreach (var link in marketingLinks.Data)
{
    Console.WriteLine($"{link.Name} - {link.ShorterLink}");
}
```

---

### Updating Short Links

#### UpdateAsync

Update an existing short link's destination URL or metadata. The short URL remains the same!

**Parameters:**

- `id` (string): Short link ID to update
- `request` (UpdateShortLinkRequest): Updated data
  - `BaseUrl` (string, **required**): New destination URL
  - `Name` (string?, optional): Updated link name
  - ... other optional fields

**Returns:** `Task<ShortLinkModel>`

**Example:**

```csharp
// Update destination URL (most common use case)
await shortLinks.UpdateAsync("link-id-123", new UpdateShortLinkRequest
{
    BaseUrl = "https://example.com/new-destination",
    TemplateId = "template-123"
});
```

---

### Managing Short Links

#### DeleteAsync

Permanently delete a short link. The short URL will no longer work.

**Parameters:**

- `id` (string): Short link ID to delete

**Returns:** `Task`

**Example:**

```csharp
await shortLinks.DeleteAsync("link-id-123");
```

---

## 🔒 Error Handling

Methods throw exceptions from `Posty5.Core.Exceptions`.

```csharp
using Posty5.Core.Exceptions;

try
{
    await shortLinks.CreateAsync(new CreateShortLinkRequest
    {
        BaseUrl = "invalid-url"
    });
}
catch (Posty5ValidationException ex)
{
    Console.WriteLine($"Invalid URL: {ex.Message}");
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
