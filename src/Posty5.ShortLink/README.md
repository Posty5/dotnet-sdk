# Posty5.ShortLink

Create and manage short links with custom slugs, visit counts, optional landing pages, app deep links and a QR code for each link, using the .NET SDK.

---

## 🌟 What is Posty5?

**Posty5** is a comprehensive suite of free online tools designed to enhance your digital marketing and social media presence. With over 4+ powerful tools and counting, Posty5 provides everything you need to:

- 🔗 **Shorten URLs** - Create memorable, trackable short links
- 📱 **Generate QR Codes** - Turn URLs, text, email, WiFi, phone, SMS and map locations into scannable codes
- 🌐 **Host HTML Pages** - Deploy static HTML pages with dynamic variables and form submission handling
- 📢 **Automate Social Media** - Schedule and manage social media posts across multiple platforms
- 📊 **Track Performance** - Monitor and analyze your digital marketing efforts

Posty5 empowers businesses, marketers, and developers to streamline their online workflows—all from a unified control panel.

**Learn more:** [https://posty5.com](https://posty5.com)

---

## 📦 About This Package

`Posty5.ShortLink` is a **specialized tool package** for creating and managing short links on the Posty5 platform.

### Key Capabilities

- **🔗 URL Shortening** - Turn long `http://` / `https://` URLs into short links
- **🎨 Custom Slugs** - `CustomLandingId`: 4-32 lowercase letters, digits or hyphens (Starter plan and above)
- **🔄 Editable URLs** - Update the destination without changing the short link
- **📊 Visit Counts** - `NumberOfVisitors` and `LastVisitorDate` per link
- **📱 QR Code per Link** - Every short link gets a QR code in the template you choose
- **🏷️ Tag & Reference Support** - Organize links with custom tags and reference IDs
- **🎯 Landing Pages** - Optionally show visitors a page with your title and description before they continue
- **📲 App Deep Links** - Android and iOS URLs, set by you or read from the target page's `al:*` meta tags
- **🔍 Filtering** - By name, URL, landing page title, status, tag, reference ID or template
- **📝 CRUD Operations** - Complete create, read, update, delete operations

### Role in the Posty5 Ecosystem

- `Posty5.QRCode` creates standalone QR codes; templates are shared with short links
- Use with `Posty5.HtmlHosting` to create short links for hosted pages

---

## ⬆️ Upgrading to 3.1.0

- `TemplateId` and `BaseUrl` are now `required` on `ShortLinkCreateRequestModel` and `ShortLinkUpdateRequestModel`. The API refuses an API-key call without a template ID, so code that left it out never succeeded.
- `IsEnableMonetization` is `[Obsolete]` everywhere and is never sent (the API never accepted it and answered 400).
- `ShortLinkListParamsModel.Search`, `FromDate` and `ToDate` are `[Obsolete]` and no longer sent: the API ignores them.
- `PageInfoTitle` is now sent as `pageInfo.title` (3.0.0 sent `pageinfo.title`, which matched nothing).
- New: `IsEnableLandingPage` on create; `AndroidUrl` / `IosUrl` on create and update (need the API's link-qr truth pass).

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
var shortLink = await shortLinks.CreateAsync(new ShortLinkCreateRequestModel
{
    Name = "Campaign Landing Page",
    BaseUrl = "https://example.com/long-url-to-campaign-page",
    TemplateId = "your-template-id",  // Required for API-key calls
    CustomLandingId = "summer-sale",  // Optional: custom slug (Starter plan and above)
    Tag = "marketing",                // Optional: for organization
    RefId = "CAMPAIGN-001"            // Optional: external reference
});

Console.WriteLine($"Short Link: {shortLink.ShorterLink}");
Console.WriteLine($"QR Code: {shortLink.QrCodeDownloadURL}");

// List short links (cursor pagination)
var page = await shortLinks.ListAsync(
    new ShortLinkListParamsModel { Tag = "marketing" },
    new PaginationParams { PageSize = 20 }
);

foreach (var link in page.Items)
{
    Console.WriteLine($"{link.Name}: {link.NumberOfVisitors} visits");
}

// Update the destination (the short link stays the same)
await shortLinks.UpdateAsync(shortLink.Id!, new ShortLinkUpdateRequestModel
{
    BaseUrl = "https://example.com/updated-campaign-page",
    TemplateId = "your-template-id"
});
```

---

## 📚 API Reference & Examples

### Creating Short Links

#### CreateAsync

Create a new short link.

**Parameters:**

- `request` (`ShortLinkCreateRequestModel`): Short link data
  - `BaseUrl` (string, **required**): Destination URL; must start with `http://` or `https://`
  - `TemplateId` (string, **required**): QR code template ID. The API answers "Template Id Is Required" to an API-key call without one; your template IDs are on the dashboard's QR code templates page
  - `Name` (string, optional): Name for the link; empty means the API names it from the target page's title
  - `CustomLandingId` (string?, optional): Custom slug, 4-32 lowercase letters, digits or hyphens (Starter plan and above)
  - `Tag` (string?, optional): Custom tag for grouping/filtering
  - `RefId` (string?, optional): External reference ID from your system
  - `IsEnableLandingPage` (bool?, optional): `true` shows visitors a page with your `PageInfo` title and description and a Continue button instead of redirecting straight away
  - `PageInfo` (`ShortLinkPageInfoModel?`): `Title` and `Description`; both required when `IsEnableLandingPage` is `true`
  - `AndroidUrl`, `IosUrl` (string?, optional): app deep links (see below)

**Returns:** `Task<ShortLinkModel>` - Created short link details

**Example:**

```csharp
// Basic short link
var shortLink = await shortLinks.CreateAsync(new ShortLinkCreateRequestModel
{
    BaseUrl = "https://example.com/product/awesome-widget",
    TemplateId = "your-template-id",
    Name = "Product Page - Awesome Widget"
});

Console.WriteLine($"Share this: {shortLink.ShorterLink}");
```

```csharp
// Landing page: visitors read your title and description, then continue
var withLanding = await shortLinks.CreateAsync(new ShortLinkCreateRequestModel
{
    BaseUrl = "https://example.com/summer-sale-2026",
    TemplateId = "your-template-id",
    IsEnableLandingPage = true,
    PageInfo = new ShortLinkPageInfoModel
    {
        Title = "Summer Sale 2026",
        Description = "Up to 40% off until August 31."
    }
});
```

#### App deep links (`AndroidUrl`, `IosUrl`)

```csharp
var appLink = await shortLinks.CreateAsync(new ShortLinkCreateRequestModel
{
    BaseUrl = "https://example.com/item/1",
    TemplateId = "your-template-id",
    AndroidUrl = "myapp://item/1",
    IosUrl = "myapp://item/1"
});
```

- Allowed schemes: `https:`, `http:` or an app scheme matching `^[a-z][a-z0-9+.-]*:` - never `javascript:`, `data:`, `vbscript:`, `file:`, `about:` or `blob:` (the API answers "The deep link URL is not allowed").
- **Create:** a supplied value wins; an empty or absent value falls back to the target page's `al:android:url` / `al:ios:url` meta tags.
- **Update:** a value is sent and wins, and `""` clears the stored deep link. Left `null`, the key is not sent: if `BaseUrl` changed, the API re-derives the deep link from the new target page's meta tags; if not, the stored value is kept.
- `IsSupportAndroidDeepUrl` / `IsSupportIOSDeepUrl` are true exactly when the link has that URL.
- These fields need the API's link-qr truth pass (S13); an older API answers 400 to a request that sets them.

---

### Retrieving Short Links

#### GetAsync

Retrieve complete details of a specific short link by ID.

**Parameters:**

- `id` (string): The short link's database ID (`Id`)

**Returns:** `Task<ShortLinkFullDetailsModel>` - Short link details, including `AndroidUrl` / `IosUrl` for the owner

**Example:**

```csharp
var link = await shortLinks.GetAsync("short-link-id-123");

Console.WriteLine($"Short Link Details:");
Console.WriteLine($"  Name: {link.Name}");
Console.WriteLine($"  Short URL: {link.ShorterLink}");
Console.WriteLine($"  Destination: {link.BaseUrl}");
Console.WriteLine($"  Visits: {link.NumberOfVisitors}");
```

---

#### ListAsync

Search and filter short links with cursor pagination.

**Parameters:**

- `listParams` (`ShortLinkListParamsModel?`, optional): Filter criteria
  - `Name` (string?): Name contains
  - `BaseUrl` (string?): Destination URL contains
  - `PageInfoTitle` (string?): Landing page title contains (sent as `pageInfo.title`)
  - `Tag`, `RefId`, `TemplateId`, `ShortLinkId`, `CreatedFrom` (string?): Exact match
  - `Status` (`ShortLinkStatusType?`): `New`, `Pending`, `Approved`, `Rejected`
  - `IsForDeepLink` (bool?)
- `pagination` (`PaginationParams?`, optional): `Cursor` and `PageSize`

`Search`, `FromDate`, `ToDate` and `IsEnableMonetization` are obsolete: the API has no such filters, so they are not sent.

**Returns:** `Task<PaginationResponse<ShortLinkModel>>` - `Items` plus `Pagination.NextCursor` / `HasMore`. List items carry `NumberOfVisitors`, `IsEnableLandingPage` and `Status` from the API's link-qr truth pass on; deep-link URLs are only returned by `GetAsync`.

**Example:**

```csharp
var marketingLinks = await shortLinks.ListAsync(new ShortLinkListParamsModel
{
    Tag = "marketing"
});

foreach (var link in marketingLinks.Items)
{
    Console.WriteLine($"{link.Name} - {link.ShorterLink}");
}
```

---

### Updating Short Links

#### UpdateAsync

Update an existing short link's destination URL or metadata. The short URL remains the same.

**Parameters:**

- `id` (string): Short link ID to update
- `request` (`ShortLinkUpdateRequestModel`): Updated data. A property left `null` is not sent.
  - `BaseUrl` (string, **required**): Destination URL - send the current one to keep it
  - `TemplateId` (string, **required**): QR code template ID
  - `Name` (string?): `null` or empty means the API names the link from the target page's title
  - `Tag`, `RefId` (string?): `null` keeps the stored value
  - `IsEnableLandingPage` (bool?): `null` keeps the stored value (from the API's link-qr truth pass on; an older API turned the landing page off)
  - `PageInfo` (`ShortLinkPageInfoModel?`): title and description, both required when the landing page is on
  - `AndroidUrl`, `IosUrl` (string?): see *App deep links* above

A custom slug cannot be changed after creation.

**Returns:** `Task<ShortLinkModel>`

**Example:**

```csharp
// Update destination URL (most common use case)
await shortLinks.UpdateAsync("link-id-123", new ShortLinkUpdateRequestModel
{
    BaseUrl = "https://example.com/new-destination",
    TemplateId = "your-template-id"
});
```

---

### Managing Short Links

#### DeleteAsync

Permanently delete a short link. The short URL will no longer work.

**Parameters:**

- `id` (string): Short link ID to delete

**Returns:** `Task<DeleteResponse>`

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
    await shortLinks.CreateAsync(new ShortLinkCreateRequestModel
    {
        BaseUrl = "https://example.com",
        TemplateId = "" // missing template
    });
}
catch (Posty5ValidationException ex)
{
    Console.WriteLine($"Refused: {ex.Message}");
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

## 📄 License

MIT License - see [LICENSE](../../LICENSE) file for details.

---

Made with ❤️ by the Posty5 team
