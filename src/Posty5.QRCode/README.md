# Posty5.QRCode

Generate and manage customizable QR codes for multiple use cases including URLs, WiFi credentials, email, SMS, phone calls, geolocation, and free text. This package provides a complete C# client for creating professional QR codes with template support, analytics tracking, and dynamic content management.

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

`Posty5.QRCode` is a **specialized tool package** for generating and managing QR codes on the Posty5 platform. It enables developers to build QR code solutions for marketing campaigns, contactless interactions, WiFi sharing, and more.

### Key Capabilities

- **📱 7 QR Code Types** - URL, Free Text, Email, WiFi, SMS, Phone Call, and Geolocation
- **🎨 Template Support** - Apply professional templates for branded QR codes
- **🔄 Dynamic QR Codes** - Update QR code content without changing the code itself
- **📊 Analytics Tracking** - Monitor scans, visitor counts, and last visitor dates
- **🎯 Landing Pages** - Each QR code gets a custom landing page URL
- **🔍 Advanced Filtering** - Search and filter by name, status, or date range
- **📝 CRUD Operations** - Complete create, read, update, delete operations
- **📈 Pagination Support** - Efficiently handle large QR code collections
- **🔒 Type Safety** - Full C# type safety with nullable reference types
- **⚡ Async/Await** - Modern async programming patterns

### Role in the Posty5 Ecosystem

This package works seamlessly with other Posty5 SDK modules:

- Generate QR codes that link to `Posty5.ShortLink` shortened URLs
- Create QR codes pointing to `Posty5.HtmlHosting` hosted pages
- Build comprehensive marketing campaigns with tracking and analytics

Perfect for **businesses**, **event organizers**, **restaurants**, **retail stores**, **marketers**, and **developers** who need contactless solutions, marketing campaigns, product packaging, digital menus, WiFi sharing, contact sharing, and location-based services.

---

## 📥 Installation

Install via NuGet Package Manager:

```bash
dotnet add package Posty5.QRCode
```

Or via Package Manager Console:

```powershell
Install-Package Posty5.QRCode
```

The `Posty5.Core` package will be automatically installed as a dependency.

---

## 🚀 Quick Start

Here's a minimal example to get you started:

```csharp
using Posty5.Core.Configuration;
using Posty5.Core.Http;
using Posty5.QRCode;
using Posty5.QRCode.Models;

// Initialize the HTTP client with your API key
var options = new Posty5Options
{
    ApiKey = "your-api-key", // Get from https://studio.posty5.com/account/settings?tab=APIKeys
};

var httpClient = new Posty5HttpClient(options);

// Create the QR Code client
var qrCodeClient = new QRCodeClient(httpClient);

// Create a URL QR code
var request = new CreateUrlQRCodeRequest
{
    Name = "Website QR Code",
    TemplateId = "template-123", // Optional: Use a template for branding
    QrCodeTarget = new UrlQRTarget
    {
        Url = "https://posty5.com"
    }
};

var qrCode = await qrCodeClient.CreateUrlAsync(request);

Console.WriteLine($"QR Code Landing Page: {qrCode.QrCodeLandingPage}");
Console.WriteLine($"QR Code ID: {qrCode.Id}");

// List all QR codes
var result = await qrCodeClient.ListAsync(
    pagination: new Core.Models.PaginationParams
    {
        PageNumber = 0,
        PageSize = 20
    }
);

Console.WriteLine($"Total QR Codes: {result.TotalCount}");
foreach (var code in result.Items)
{
    Console.WriteLine($"- {code.Name}");
}
```

---

## 📱 QR Code Types

### 1. URL QR Code

Redirect users to any website:

```csharp
var urlQR = await qrCodeClient.CreateUrlAsync(new CreateUrlQRCodeRequest
{
    Name = "Product Page",
    QrCodeTarget = new UrlQRTarget
    {
        Url = "https://example.com/products/item-123"
    }
});
```

### 2. Free Text QR Code

Display plain text when scanned:

```csharp
var textQR = await qrCodeClient.CreateFreeTextAsync(new CreateFreeTextQRCodeRequest
{
    Name = "Welcome Message",
    QrCodeTarget = new FreeTextQRTarget
    {
        Text = "Welcome to our event! Check in at the registration desk."
    }
});
```

### 3. Email QR Code

Pre-fill an email message:

```csharp
var emailQR = await qrCodeClient.CreateEmailAsync(new CreateEmailQRCodeRequest
{
    Name = "Contact Support",
    QrCodeTarget = new EmailQRTarget
    {
        Email = "support@example.com",
        Subject = "Support Request",
        Body = "I need help with..."
    }
});
```

### 4. WiFi QR Code

Share WiFi credentials easily:

```csharp
var wifiQR = await qrCodeClient.CreateWifiAsync(new CreateWifiQRCodeRequest
{
    Name = "Guest WiFi",
    QrCodeTarget = new WifiQRTarget
    {
        Ssid = "Guest-Network",
        Password = "SecurePassword123",
        SecurityType = "WPA",
        Hidden = false
    }
});
```

### 5. Phone Call QR Code

Initiate a phone call:

```csharp
var callQR = await qrCodeClient.CreateCallAsync(new CreateCallQRCodeRequest
{
    Name = "Call Support",
    QrCodeTarget = new CallQRTarget
    {
        PhoneNumber = "+1234567890"
    }
});
```

### 6. SMS QR Code

Send a pre-filled text message:

```csharp
var smsQR = await qrCodeClient.CreateSmsAsync(new CreateSmsQRCodeRequest
{
    Name = "Text to Win",
    QrCodeTarget = new SmsQRTarget
    {
        PhoneNumber = "+1234567890",
        Message = "ENTER to participate in our contest"
    }
});
```

### 7. Geolocation QR Code

Open a map location:

```csharp
var locationQR = await qrCodeClient.CreateGeolocationAsync(new CreateGeolocationQRCodeRequest
{
    Name = "Store Location",
    QrCodeTarget = new GeolocationQRTarget
    {
        Latitude = 40.7128,
        Longitude = -74.0060
    }
});
```

---

## 📖 API Reference

### QRCodeClient Methods

#### Create Operations

- **`CreateUrlAsync(CreateUrlQRCodeRequest request, CancellationToken cancellationToken = default)`**
  - Creates a URL QR code
  - Returns `QRCodeModel`

- **`CreateFreeTextAsync(CreateFreeTextQRCodeRequest request, CancellationToken cancellationToken = default)`**
  - Creates a free text QR code
  - Returns `QRCodeModel`

- **`CreateEmailAsync(CreateEmailQRCodeRequest request, CancellationToken cancellationToken = default)`**
  - Creates an email QR code
  - Returns `QRCodeModel`

- **`CreateWifiAsync(CreateWifiQRCodeRequest request, CancellationToken cancellationToken = default)`**
  - Creates a WiFi QR code
  - Returns `QRCodeModel`

- **`CreateCallAsync(CreateCallQRCodeRequest request, CancellationToken cancellationToken = default)`**
  - Creates a phone call QR code
  - Returns `QRCodeModel`

- **`CreateSmsAsync(CreateSmsQRCodeRequest request, CancellationToken cancellationToken = default)`**
  - Creates an SMS QR code
  - Returns `QRCodeModel`

- **`CreateGeolocationAsync(CreateGeolocationQRCodeRequest request, CancellationToken cancellationToken = default)`**
  - Creates a geolocation QR code
  - Returns `QRCodeModel`

#### Read Operations

- **`GetAsync(string id, CancellationToken cancellationToken = default)`**
  - Gets a QR code by ID
  - Returns `QRCodeModel`

- **`ListAsync(ListQRCodesParams? listParams = null, PaginationParams? pagination = null, CancellationToken cancellationToken = default)`**
  - Lists QR codes with filtering and pagination
  - Returns `PaginationResponse<QRCodeModel>`

#### Update Operations

- **`UpdateAsync(string id, CreateUrlQRCodeRequest request, CancellationToken cancellationToken = default)`**
  - Updates an existing QR code (works for all types)
  - Returns `QRCodeModel`

#### Delete Operations

- **`DeleteAsync(string id, CancellationToken cancellationToken = default)`**
  - Deletes a QR code
  - Returns `bool` indicating success

---

## 🔍 Filtering and Search

```csharp
// Search QR codes by name
var searchParams = new ListQRCodesParams
{
    Search = "product",
    FromDate = DateTime.UtcNow.AddMonths(-1),
    ToDate = DateTime.UtcNow
};

var results = await qrCodeClient.ListAsync(
    searchParams,
    new PaginationParams { PageNumber = 0, PageSize = 50 }
);
```

---

## 🎨 Using Templates

Templates allow you to apply consistent branding to your QR codes:

```csharp
var request = new CreateUrlQRCodeRequest
{
    Name = "Branded QR",
    TemplateId = "your-template-id", // Create templates in Posty5 Studio
    QrCodeTarget = new UrlQRTarget { Url = "https://posty5.com" }
};

var qrCode = await qrCodeClient.CreateUrlAsync(request);
```

---

## 🔄 Updating QR Codes

Update QR code content without changing the QR code image:

```csharp
// Get existing QR code
var qrCode = await qrCodeClient.GetAsync("qr-code-id");

// Update the URL
var updateRequest = new CreateUrlQRCodeRequest
{
    Name = qrCode.Name,
    QrCodeTarget = new UrlQRTarget
    {
        Url = "https://new-url.com"
    }
};

var updated = await qrCodeClient.UpdateAsync(qrCode.Id!, updateRequest);
```

---

## 🔒 Error Handling

```csharp
using Posty5.Core.Exceptions;

try
{
    var qrCode = await qrCodeClient.GetAsync("invalid-id");
}
catch (Posty5NotFoundException ex)
{
    Console.WriteLine("QR code not found");
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

## 🤝 Related Packages

- **[Posty5.Core](../Posty5.Core)** - Core HTTP client (required dependency)
- **[Posty5.ShortLink](../Posty5.ShortLink)** - URL shortening functionality
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
