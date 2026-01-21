# Posty5.ShortLink

Create and manage branded short links with analytics tracking, custom slugs, and QR code generation. This package provides a complete C# client for building URL shortening solutions with editable destinations, comprehensive tracking, and easy management.

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
- **📊 Analytics Tracking** - Monitor clicks, visitor counts, and engagement
- **📱 Automatic QR Codes** - Get a QR code for each short link
- **🔍 Advanced Filtering** - Search by name, URL, or date range
- **📝 CRUD Operations** - Complete create, read, update, delete operations
- **📈 Pagination Support** - Efficiently handle large link collections
- **🔒 Type Safety** - Full C# type safety with nullable reference types
- **⚡ Async/Await** - Modern async programming patterns
- **🎯 Template Support** - Apply templates for QR code customization

### Role in the Posty5 Ecosystem

This package works seamlessly with other Posty5 SDK modules:

- Combine with `Posty5.QRCode` for enhanced QR code customization
- Use with `Posty5.HtmlHosting` to create short links for hosted pages
- Build comprehensive marketing campaigns with full tracking and analytics

Perfect for **marketers**, **social media managers**, **content creators**, **businesses**, and **developers** who need URL shortening, link tracking, campaign management, social media optimization, and branded links.

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

The `Posty5.Core` package will be automatically installed as a dependency.

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
    ApiKey = "your-api-key", // Get from https://studio.posty5.com/account/settings?tab=APIKeys
    BaseUrl = "https://api.posty5.com"
};

var httpClient = new Posty5HttpClient(options);

// Create the Short Link client
var shortLinkClient = new ShortLinkClient(httpClient);

// Create a short link with auto-generated slug
var request = new CreateShortLinkRequest
{
    Name = "Campaign Landing Page",
    TargetUrl = "https://example.com/long-url-to-campaign-page",
    TemplateId = "template-123" // Optional: QR code template ID
};

var shortLink = await shortLinkClient.CreateAsync(request);

Console.WriteLine($"Short Link: {shortLink.ShortUrl}");
Console.WriteLine($"Short Link ID: {shortLink.Id}");

// List all short links
var result = await shortLinkClient.ListAsync(
    pagination: new Core.Models.PaginationParams
    {
        PageNumber = 0,
        PageSize = 20
    }
);

Console.WriteLine($"Total Short Links: {result.TotalCount}");
foreach (var link in result.Items)
{
    Console.WriteLine($"- {link.Name}: {link.ShortUrl}");
}
```

---

## 🔗 Creating Short Links

### Auto-Generated Slug

Let Posty5 generate a random short slug:

```csharp
var request = new CreateShortLinkRequest
{
    Name = "Product Launch",
    TargetUrl = "https://example.com/products/new-item"
};

var shortLink = await shortLinkClient.CreateAsync(request);
// Result: https://posty5.com/abc123
```

### Custom Slug

Create a branded, memorable short link:

```csharp
var request = new CreateShortLinkRequest
{
    Name = "Summer Sale",
    TargetUrl = "https://example.com/summer-sale-2026",
    CustomSlug = "summer-sale"
};

var shortLink = await shortLinkClient.CreateAsync(request);
// Result: https://posty5.com/summer-sale
```

### With QR Code Template

Apply a custom template to the auto-generated QR code:

```csharp
var request = new CreateShortLinkRequest
{
    Name = "Event Registration",
    TargetUrl = "https://example.com/events/register",
    CustomSlug = "event-2026",
    TemplateId = "your-template-id" // Create templates in Posty5 Studio
};

var shortLink = await shortLinkClient.CreateAsync(request);
```

---

## 📖 API Reference

### ShortLinkClient Methods

#### Create Operations

**`CreateAsync(CreateShortLinkRequest request, CancellationToken cancellationToken = default)`**

- Creates a new short link
- Returns `ShortLinkModel`

#### Read Operations

**`GetAsync(string id, CancellationToken cancellationToken = default)`**

- Gets a short link by ID
- Returns `ShortLinkModel`

**`ListAsync(ListShortLinksParams? listParams = null, PaginationParams? pagination = null, CancellationToken cancellationToken = default)`**

- Lists short links with filtering and pagination
- Returns `PaginationResponse<ShortLinkModel>`

#### Update Operations

**`UpdateAsync(string id, UpdateShortLinkRequest request, CancellationToken cancellationToken = default)`**

- Updates an existing short link
- Returns `ShortLinkModel`

#### Delete Operations

**`DeleteAsync(string id, CancellationToken cancellationToken = default)`**

- Deletes a short link
- Returns `bool` indicating success

---

## 🔄 Updating Short Links

### Change Destination URL

Update where the short link redirects without changing the short URL:

```csharp
// Get existing short link
var shortLink = await shortLinkClient.GetAsync("link-id");

// Update the target URL
var updateRequest = new UpdateShortLinkRequest
{
    TargetUrl = "https://new-destination.com"
};

var updated = await shortLinkClient.UpdateAsync(shortLink.Id!, updateRequest);
```

### Update Name

```csharp
var updateRequest = new UpdateShortLinkRequest
{
    Name = "Updated Campaign Name"
};

var updated = await shortLinkClient.UpdateAsync("link-id", updateRequest);
```

### Update Custom Slug

```csharp
var updateRequest = new UpdateShortLinkRequest
{
    CustomSlug = "new-slug"
};

var updated = await shortLinkClient.UpdateAsync("link-id", updateRequest);
```

---

## 🔍 Filtering and Search

### Search by Name

```csharp
var searchParams = new ListShortLinksParams
{
    Search = "campaign"
};

var results = await shortLinkClient.ListAsync(
    searchParams,
    new PaginationParams { PageNumber = 0, PageSize = 50 }
);
```

### Filter by Date Range

```csharp
var searchParams = new ListShortLinksParams
{
    FromDate = DateTime.UtcNow.AddMonths(-1),
    ToDate = DateTime.UtcNow
};

var results = await shortLinkClient.ListAsync(searchParams);
```

---

## 📊 Analytics and Tracking

Each short link includes tracking information:

```csharp
var shortLink = await shortLinkClient.GetAsync("link-id");

Console.WriteLine($"Total Clicks: {shortLink.ClickCount}");
Console.WriteLine($"Created: {shortLink.CreatedAt}");
Console.WriteLine($"Last Updated: {shortLink.UpdatedAt}");
```

---

## 💡 Common Use Cases

### Marketing Campaigns

```csharp
var campaignLink = await shortLinkClient.CreateAsync(new CreateShortLinkRequest
{
    Name = "Black Friday 2026",
    TargetUrl = "https://store.com/black-friday-deals",
    CustomSlug = "bf2026"
});

// Share: https://posty5.com/bf2026
```

### Social Media Posts

```csharp
var socialLink = await shortLinkClient.CreateAsync(new CreateShortLinkRequest
{
    Name = "Instagram Bio Link",
    TargetUrl = "https://example.com/links",
    CustomSlug = "mylinks"
});

// Use in Instagram bio: https://posty5.com/mylinks
```

### Event Registration

```csharp
var eventLink = await shortLinkClient.CreateAsync(new CreateShortLinkRequest
{
    Name = "Conference 2026",
    TargetUrl = "https://events.com/conference-2026/register",
    CustomSlug = "conf2026"
});

// Print on materials: https://posty5.com/conf2026
```

### Product Launches

```csharp
var productLink = await shortLinkClient.CreateAsync(new CreateShortLinkRequest
{
    Name = "New Product Launch",
    TargetUrl = "https://shop.com/products/new-item",
    CustomSlug = "newproduct"
});

// Update later when product page changes
await shortLinkClient.UpdateAsync(productLink.Id!, new UpdateShortLinkRequest
{
    TargetUrl = "https://shop.com/products/new-item-v2"
});
```

---

## 🔒 Error Handling

```csharp
using Posty5.Core.Exceptions;

try
{
    var shortLink = await shortLinkClient.CreateAsync(new CreateShortLinkRequest
    {
        Name = "Test Link",
        TargetUrl = "https://example.com",
        CustomSlug = "existing-slug" // Slug already taken
    });
}
catch (Posty5ValidationException ex)
{
    Console.WriteLine($"Validation error: {ex.Message}");
    // Handle duplicate slug
}
catch (Posty5NotFoundException ex)
{
    Console.WriteLine("Resource not found");
}
catch (Posty5Exception ex)
{
    Console.WriteLine($"API error: {ex.Message}");
}
```

---

## 🎯 Best Practices

1. **Use Descriptive Names**
   - Name your links clearly for easy management
   - Example: "Holiday Sale 2026" instead of "Link1"

2. **Custom Slugs for Branding**
   - Use custom slugs for important campaigns
   - Keep them short and memorable
   - Use hyphens for multi-word slugs

3. **Update Instead of Delete**
   - Update target URLs rather than deleting and recreating
   - Preserves analytics and QR codes

4. **Handle Slug Conflicts**
   - Implement error handling for duplicate slugs
   - Have a fallback strategy (e.g., add numbers)

5. **Pagination for Large Lists**
   - Use appropriate page sizes for performance
   - Implement pagination in your UI

---

## 🤝 Related Packages

- **[Posty5.Core](../Posty5.Core)** - Core HTTP client (required dependency)
- **[Posty5.QRCode](../Posty5.QRCode)** - QR code generation and management
- **[Posty5.HtmlHosting](../Posty5.HtmlHosting)** - HTML page hosting
- **[Posty5.SocialPublisher](../Posty5.SocialPublisher)** - Social media publishing

---

## 📄 License

MIT License - See [LICENSE](../../LICENSE) file in the root directory.

---

## 🔗 Resources

- **Documentation**: [https://posty5.com/docs](https://posty5.com/docs)
- **API Reference**: [https://posty5.com/api](https://posty5.com/api)
- **Get API Key**: [https://studio.posty5.com/account/settings?tab=APIKeys](https://studio.posty5.com/account/settings?tab=APIKeys)
- **Support**: [https://posty5.com/contact-us](https://posty5.com/contact-us)

---

Made with ❤️ by the Posty5 Team
