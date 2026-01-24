# Posty5.QRCode

Generate and manage customizable QR codes for multiple use cases with the .NET SDK. This package provides a complete C# client for creating professional QR codes with template support, analytics tracking, and dynamic content management.

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
- **🏷️ Tag & Reference Support** - Organize QR codes with custom tags and reference IDs
- **🎯 Landing Pages** - Each QR code gets a custom landing page URL
- **🔗 Short Links** - Automatic short URL generation for easy sharing
- **🔍 Advanced Filtering** - Search and filter by name, status, tag, or reference ID
- **📝 CRUD Operations** - Complete create, read, update, delete operations
- **🔐 API Key Filtering** - Scope resources by API key for multi-tenant applications
- **📈 Pagination Support** - Efficiently handle large QR code collections

### Role in the Posty5 Ecosystem

This package works seamlessly with other Posty5 SDK packages:

- Generate QR codes that link to `Posty5.ShortLink` shortened URLs
- Create QR codes pointing to `Posty5.HtmlHosting` hosted pages
- Build comprehensive marketing campaigns with tracking and analytics

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
    ApiKey = "your-api-key" // Get from https://studio.posty5.com/account/settings?tab=APIKeys
};
var httpClient = new Posty5HttpClient(options);

// Create the QR Code client
var qrCodes = new QRCodeClient(httpClient);

// Create a URL QR code
var qrCode = await qrCodes.CreateURLAsync(new CreateURLQRCodeRequest
{
    Name = "Website QR Code",
    TemplateId = "template-123", // Optional: Use a template for branding
    Url = new QRCodeUrlTarget
    {
        Url = "https://posty5.com"
    },
    Tag = "marketing", // Optional: For organization
    RefId = "CAMPAIGN-001" // Optional: External reference
});

Console.WriteLine($"QR Code Landing Page: {qrCode.QrCodeLandingPageURL}");
Console.WriteLine($"Short Link: {qrCode.ShorterLink}");
Console.WriteLine($"QR Code ID: {qrCode.Id}");

// List all QR codes
var allQRCodes = await qrCodes.ListAsync(
    null,
    new PaginationParams { PageNumber = 1, PageSize = 20 }
);

Console.WriteLine($"Total QR codes: {allQRCodes.Pagination.TotalCount}");
foreach (var qr in allQRCodes.Data)
{
    Console.WriteLine($"{qr.Name}: {qr.NumberOfVisitors} scans");
}
```

---

## 📚 API Reference & Examples

### Creating QR Codes

The SDK supports 7 different QR code types. Each type has its own creation method with type-specific parameters.

---

#### CreateURLAsync

Create a URL QR code that redirects users to a website when scanned.

**Parameters:**

- `data` (CreateURLQRCodeRequest): QR code data
  - `Name` (string, **required**): Human-readable name
  - `TemplateId` (string, **required**): Template ID for styling
  - `Url` (QRCodeUrlTarget, **required**):
    - `Url` (string): Target website URL
  - `Tag` (string?): Custom tag
  - `RefId` (string?): External reference ID

**Returns:** `Task<QRCodeModel>` - Created QR code details

**Example:**

```csharp
var qrCode = await qrCodes.CreateURLAsync(new CreateURLQRCodeRequest
{
    Name = "Company Website",
    TemplateId = "template-123",
    Url = new QRCodeUrlTarget { Url = "https://example.com" }
});

Console.WriteLine($"Scan this: {qrCode.QrCodeLandingPageURL}");
```

---

#### CreateFreeTextAsync

Create a free text QR code with any custom text content.

**Parameters:**

- `data` (CreateFreeTextQRCodeRequest): QR code data
  - `Name`, `TemplateId`...
  - `Text` (string, **required**): Custom text to encode

**Returns:** `Task<QRCodeModel>`

**Example:**

```csharp
var textQR = await qrCodes.CreateFreeTextAsync(new CreateFreeTextQRCodeRequest
{
    Name = "Product Serial #12345",
    TemplateId = "template-123",
    Text = "SN:12345-ABCDE-67890",
    Tag = "inventory"
});
```

---

#### CreateEmailAsync

Create an email QR code that opens the default email client.

**Parameters:**

- `data` (CreateEmailQRCodeRequest): QR code data
  - `Email` (QRCodeEmailTarget):
    - `Email` (string): Recipient email
    - `Subject` (string): Subject line
    - `Body` (string): Email body

**Returns:** `Task<QRCodeModel>`

**Example:**

```csharp
var supportQR = await qrCodes.CreateEmailAsync(new CreateEmailQRCodeRequest
{
    Name = "Contact Support",
    TemplateId = "template-123",
    Email = new QRCodeEmailTarget
    {
        Email = "support@example.com",
        Subject = "Support Request",
        Body = "I need help with..."
    }
});
```

---

#### CreateWifiAsync

Create a WiFi QR code for network connection.

**Parameters:**

- `data` (CreateWifiQRCodeRequest): QR code data
  - `Wifi` (QRCodeWifiTarget):
    - `Name` (string): SSID
    - `AuthenticationType` (string): 'WPA', 'WEP', or 'nopass'
    - `Password` (string): Network password

**Returns:** `Task<QRCodeModel>`

**Example:**

```csharp
var wifiQR = await qrCodes.CreateWifiAsync(new CreateWifiQRCodeRequest
{
    Name = "Office WiFi",
    TemplateId = "template-123",
    Wifi = new QRCodeWifiTarget
    {
        Name = "OfficeNetwork-5G",
        AuthenticationType = "WPA",
        Password = "SecurePassword123!"
    }
});
```

---

#### CreateCallAsync

Create a phone call QR code.

**Parameters:**

- `data` (CreateCallQRCodeRequest): QR code data
  - `Call` (QRCodeCallTarget):
    - `PhoneNumber` (string): Phone number to call

**Returns:** `Task<QRCodeModel>`

**Example:**

```csharp
var hotlineQR = await qrCodes.CreateCallAsync(new CreateCallQRCodeRequest
{
    Name = "Customer Service",
    TemplateId = "template-123",
    Call = new QRCodeCallTarget
    {
        PhoneNumber = "+1-800-123-4567"
    }
});
```

---

#### CreateSMSAsync

Create an SMS QR code.

**Parameters:**

- `data` (CreateSMSQRCodeRequest): QR code data
  - `Sms` (QRCodeSmsTarget):
    - `PhoneNumber` (string): Recipient number
    - `Message` (string): Message text

**Returns:** `Task<QRCodeModel>`

**Example:**

```csharp
var smsQR = await qrCodes.CreateSMSAsync(new CreateSMSQRCodeRequest
{
    Name = "Join Contest",
    TemplateId = "template-123",
    Sms = new QRCodeSmsTarget
    {
        PhoneNumber = "+1-555-CONTEST",
        Message = "ENTER 2026"
    }
});
```

---

#### CreateGeolocationAsync

Create a map location QR code.

**Parameters:**

- `data` (CreateGeolocationQRCodeRequest): QR code data
  - `Geolocation` (QRCodeGeolocationTarget):
    - `Latitude` (string/double): Latitude
    - `Longitude` (string/double): Longitude

**Returns:** `Task<QRCodeModel>`

**Example:**

```csharp
var mapQR = await qrCodes.CreateGeolocationAsync(new CreateGeolocationQRCodeRequest
{
    Name = "Office Location",
    TemplateId = "template-123",
    Geolocation = new QRCodeGeolocationTarget
    {
        Latitude = "40.7128",
        Longitude = "-74.0060"
    }
});
```

---

### Retrieving QR Codes

#### GetAsync

Retrieve complete details of a specific QR code by ID.

**Example:**

```csharp
var qrCode = await qrCodes.GetAsync("qr-code-id-123");
Console.WriteLine($"Scans: {qrCode.NumberOfVisitors}");
```

---

#### ListAsync

Search and filter QR codes.

**Parameters:**

- `listParams` (ListQRCodesParams?, optional): Filter criteria
  - `Name` (string?), `Status` (string?), `Tag` (string?), `RefId` (string?)
- `pagination` (PaginationParams?, optional)

**Example:**

```csharp
var marketingQRs = await qrCodes.ListAsync(new ListQRCodesParams
{
    Tag = "marketing",
    Status = "approved"
});

foreach (var qr in marketingQRs.Data)
{
    Console.WriteLine($"{qr.Name} - {qr.ShorterLink}");
}
```

---

### Updating QR Codes

Each type has a corresponding Update method.

- `UpdateURLAsync(id, request)`
- `UpdateFreeTextAsync(id, request)`
- `UpdateEmailAsync(id, request)`
- `UpdateWifiAsync(id, request)`
- `UpdateCallAsync(id, request)`
- `UpdateSMSAsync(id, request)`
- `UpdateGeolocationAsync(id, request)`

**Example (Update URL):**

```csharp
await qrCodes.UpdateURLAsync("qr-code-id-123", new UpdateURLQRCodeRequest
{
    Name = "Summer Sale - Extended",
    TemplateId = "template-123",
    Url = new QRCodeUrlTarget { Url = "https://example.com/extended" },
    Tag = "summer-sale"
});
```

---

### Deleting QR Codes

#### DeleteAsync

**Example:**

```csharp
await qrCodes.DeleteAsync("qr-code-id-123");
```

---

## 🔒 Error Handling

Methods throw exceptions from `Posty5.Core.Exceptions`.

```csharp
try
{
    await qrCodes.GetAsync("invalid-id");
}
catch (Posty5NotFoundException)
{
    Console.WriteLine("QR Code not found");
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
