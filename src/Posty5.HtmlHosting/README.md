# Posty5.HtmlHosting

Create and manage hosted HTML pages with dynamic content, form submissions, and custom variables. This package provides a complete C# client for deploying and managing static HTML pages on the Posty5 platform.

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

`Posty5.HtmlHosting` is a **specialized tool package** for creating and managing hosted HTML pages on the Posty5 platform. It enables developers to quickly deploy static HTML content with professional hosting.

### Key Capabilities

- **🌐 HTML Page Hosting** - Deploy static HTML pages instantly
- **📝 Dynamic Content** - Update HTML content programmatically
- **🎨 Template Support** - Apply professional templates for consistent branding
- **🔗 Public URLs** - Get instant public URLs for hosted pages
- **🔍 Advanced Filtering** - Search and filter by name or date range
- **📝 CRUD Operations** - Complete create, read, update, delete operations
- **📈 Pagination Support** - Efficiently handle large page collections
- **🔒 Type Safety** - Full C# type safety with nullable reference types
- **⚡ Async/Await** - Modern async programming patterns

### Role in the Posty5 Ecosystem

This package works seamlessly with other Posty5 SDK modules:

- Create landing pages and link them with `Posty5.ShortLink`
- Generate QR codes with `Posty5.QRCode` that point to hosted pages
- Build complete marketing funnels with integrated tools

Perfect for **marketers**, **businesses**, **developers**, and **designers** who need landing pages, marketing campaigns, form collection, event pages, product showcases, and quick deployments.

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

The `Posty5.Core` package will be automatically installed as a dependency.

---

## 🚀 Quick Start

Here's a minimal example to get you started:

```csharp
using Posty5.Core.Configuration;
using Posty5.Core.Http;
using Posty5.HtmlHosting;
using Posty5.HtmlHosting.Models;

// Initialize the HTTP client with your API key
var options = new Posty5Options
{
    ApiKey = "your-api-key", // Get from https://studio.posty5.com/account/settings?tab=APIKeys
};

var httpClient = new Posty5HttpClient(options);

// Create the HTML Hosting client
var htmlHostingClient = new HtmlHostingClient(httpClient);

// Create a hosted HTML page
var htmlContent = @"
<!DOCTYPE html>
<html>
<head>
    <title>Welcome Page</title>
    <style>
        body { font-family: Arial; text-align: center; padding: 50px; }
        h1 { color: #333; }
    </style>
</head>
<body>
    <h1>Welcome to My Landing Page</h1>
    <p>This page is hosted on Posty5!</p>
</body>
</html>";

var request = new CreateHtmlHostingRequest
{
    Name = "Welcome Landing Page",
    HtmlContent = htmlContent,
    TemplateId = "template-123" // Optional
};

var hostedPage = await htmlHostingClient.CreateAsync(request);

Console.WriteLine($"Public URL: {hostedPage.PublicUrl}");
Console.WriteLine($"Page ID: {hostedPage.Id}");

// List all hosted pages
var result = await htmlHostingClient.ListAsync(
    pagination: new Core.Models.PaginationParams
    {
        PageNumber = 0,
        PageSize = 20
    }
);

Console.WriteLine($"Total Pages: {result.TotalCount}");
foreach (var page in result.Items)
{
    Console.WriteLine($"- {page.Name}: {page.PublicUrl}");
}
```

---

## 📖 API Reference

### HtmlHostingClient Methods

#### Create Operations

**`CreateAsync(CreateHtmlHostingRequest request, CancellationToken cancellationToken = default)`**

- Creates a new hosted HTML page
- Returns `HtmlHostingModel`

#### Read Operations

**`GetAsync(string id, CancellationToken cancellationToken = default)`**

- Gets a hosted page by ID
- Returns `HtmlHostingModel`

**`ListAsync(ListHtmlHostingParams? listParams = null, PaginationParams? pagination = null, CancellationToken cancellationToken = default)`**

- Lists hosted pages with filtering and pagination
- Returns `PaginationResponse<HtmlHostingModel>`

#### Update Operations

**`UpdateAsync(string id, UpdateHtmlHostingRequest request, CancellationToken cancellationToken = default)`**

- Updates an existing hosted page
- Returns `HtmlHostingModel`

#### Delete Operations

**`DeleteAsync(string id, CancellationToken cancellationToken = default)`**

- Deletes a hosted page
- Returns `bool` indicating success

---

## 🔄 Updating HTML Content

Update the HTML content of a hosted page without changing the URL:

```csharp
// Get existing page
var page = await htmlHostingClient.GetAsync("page-id");

// Update the content
var newHtml = @"
<!DOCTYPE html>
<html>
<head>
    <title>Updated Page</title>
</head>
<body>
    <h1>Content Updated!</h1>
    <p>This is the new version of the page.</p>
</body>
</html>";

var updateRequest = new UpdateHtmlHostingRequest
{
    HtmlContent = newHtml,
    Name = "Updated Landing Page" // Optional
};

var updated = await htmlHostingClient.UpdateAsync(page.Id!, updateRequest);
```

---

## 💡 Common Use Cases

### Landing Page

```csharp
var landingHtml = @"
<!DOCTYPE html>
<html>
<head>
    <title>Product Launch</title>
    <meta name='viewport' content='width=device-width, initial-scale=1'>
    <style>
        * { margin: 0; padding: 0; box-sizing: border-box; }
        body {
            font-family: 'Arial', sans-serif;
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            min-height: 100vh;
            display: flex;
            align-items: center;
            justify-content: center;
            color: white;
        }
        .container {
            text-align: center;
            padding: 2rem;
        }
        h1 { font-size: 3rem; margin-bottom: 1rem; }
        p { font-size: 1.5rem; margin-bottom: 2rem; }
        .cta {
            background: white;
            color: #667eea;
            padding: 1rem 2rem;
            border-radius: 50px;
            text-decoration: none;
            font-weight: bold;
            font-size: 1.2rem;
        }
    </style>
</head>
<body>
    <div class='container'>
        <h1>🚀 New Product Launch</h1>
        <p>Join thousands of satisfied customers</p>
        <a href='#signup' class='cta'>Get Started Free</a>
    </div>
</body>
</html>";

var page = await htmlHostingClient.CreateAsync(new CreateHtmlHostingRequest
{
    Name = "Product Launch Landing",
    HtmlContent = landingHtml
});
```

### Contact Form

```csharp
var formHtml = File.ReadAllText("contact_form.html");

var contactPage = await htmlHostingClient.CreateAsync(new CreateHtmlHostingRequest
{
    Name = "Contact Form",
    HtmlContent = formHtml
});

Console.WriteLine($"Contact form available at: {contactPage.PublicUrl}");
```

### Event Page

```csharp
var eventHtml = @"
<!DOCTYPE html>
<html>
<head>
    <title>Annual Conference 2026</title>
    <style>
        body { font-family: Arial; max-width: 800px; margin: 0 auto; padding: 20px; }
        .header { background: #2c3e50; color: white; padding: 2rem; text-align: center; }
        .details { padding: 2rem; }
        .date { font-size: 1.5rem; color: #e74c3c; font-weight: bold; }
    </style>
</head>
<body>
    <div class='header'>
        <h1>Annual Tech Conference 2026</h1>
        <p class='date'>June 15-17, 2026</p>
    </div>
    <div class='details'>
        <h2>About the Event</h2>
        <p>Join us for three days of innovation, networking, and learning.</p>
        <h3>Schedule</h3>
        <ul>
            <li>Day 1: Keynote Speakers</li>
            <li>Day 2: Technical Workshops</li>
            <li>Day 3: Networking Sessions</li>
        </ul>
    </div>
</body>
</html>";

var eventPage = await htmlHostingClient.CreateAsync(new CreateHtmlHostingRequest
{
    Name = "Tech Conference 2026",
    HtmlContent = eventHtml
});
```

---

## 🔍 Filtering and Search

### Search by Name

```csharp
var searchParams = new ListHtmlHostingParams
{
    Search = "landing"
};

var results = await htmlHostingClient.ListAsync(
    searchParams,
    new PaginationParams { PageNumber = 0, PageSize = 50 }
);
```

### Filter by Date Range

```csharp
var searchParams = new ListHtmlHostingParams
{
    FromDate = DateTime.UtcNow.AddMonths(-1),
    ToDate = DateTime.UtcNow
};

var results = await htmlHostingClient.ListAsync(searchParams);
```

---

## 🎨 Using Templates

Templates allow you to apply consistent branding:

```csharp
var request = new CreateHtmlHostingRequest
{
    Name = "Branded Page",
    HtmlContent = myHtml,
    TemplateId = "your-template-id" // Create templates in Posty5 Studio
};

var page = await htmlHostingClient.CreateAsync(request);
```

---

## 🔒 Error Handling

```csharp
using Posty5.Core.Exceptions;

try
{
    var page = await htmlHostingClient.GetAsync("invalid-id");
}
catch (Posty5NotFoundException ex)
{
    Console.WriteLine("Page not found");
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

1. **Responsive Design**
   - Always include viewport meta tag
   - Use mobile-friendly CSS
   - Test on multiple devices

2. **Optimize HTML**
   - Minify HTML for production
   - Keep file sizes reasonable
   - Use external CSS for large stylesheets

3. **SEO Considerations**
   - Include proper title and meta tags
   - Use semantic HTML
   - Add alt text to images

4. **Update Rather Than Delete**
   - Update existing pages to preserve URLs
   - Useful for maintaining SEO and links

5. **Naming Convention**
   - Use descriptive names for easy management
   - Include campaign or project identifiers

---

## 🤝 Related Packages

- **[Posty5.Core](../Posty5.Core)** - Core HTTP client (required dependency)
- **[Posty5.QRCode](../Posty5.QRCode)** - QR code generation and management
- **[Posty5.ShortLink](../Posty5.ShortLink)** - URL shortening functionality
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
