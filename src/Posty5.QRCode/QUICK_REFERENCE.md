# Posty5.QRCode - Quick Reference Guide

## Installation

```bash
dotnet add package Posty5.QRCode
```

## Basic Setup

```csharp
using Posty5.Core.Http;
using Posty5.QRCode;
using Posty5.QRCode.Models;

var httpClient = new Posty5HttpClient(new Posty5HttpClientOptions
{
    BaseUrl = "https://api.posty5.com",
    ApiKey = "your-api-key"
});

var qrCodeClient = new QRCodeClient(httpClient);
```

## Quick Examples

### 1. Free Text QR Code
```csharp
var qrCode = await qrCodeClient.CreateFreeTextAsync(new CreateFreeTextQRCodeRequest
{
    Name = "My Text QR",
    TemplateId = "template_id",
    Text = "Hello World!"
});
```

### 2. Email QR Code
```csharp
var qrCode = await qrCodeClient.CreateEmailAsync(new CreateEmailQRCodeRequest
{
    Name = "Contact Email",
    TemplateId = "template_id",
    Email = new QRCodeEmailTarget
    {
        Email = "contact@example.com",
        Subject = "Hello",
        Body = "Your message here"
    }
});
```

### 3. WiFi QR Code
```csharp
var qrCode = await qrCodeClient.CreateWifiAsync(new CreateWifiQRCodeRequest
{
    Name = "Office WiFi",
    TemplateId = "template_id",
    Wifi = new QRCodeWifiTarget
    {
        Name = "NetworkName",
        AuthenticationType = "WPA",
        Password = "password123"
    }
});
```

### 4. Phone Call QR Code
```csharp
var qrCode = await qrCodeClient.CreateCallAsync(new CreateCallQRCodeRequest
{
    Name = "Call Support",
    TemplateId = "template_id",
    Call = new QRCodeCallTarget
    {
        PhoneNumber = "+1234567890"
    }
});
```

### 5. SMS QR Code
```csharp
var qrCode = await qrCodeClient.CreateSMSAsync(new CreateSMSQRCodeRequest
{
    Name = "Text Us",
    TemplateId = "template_id",
    Sms = new QRCodeSmsTarget
    {
        PhoneNumber = "+1234567890",
        Message = "Hello from QR!"
    }
});
```

### 6. URL QR Code
```csharp
var qrCode = await qrCodeClient.CreateURLAsync(new CreateURLQRCodeRequest
{
    Name = "Website",
    TemplateId = "template_id",
    Url = new QRCodeUrlTarget { Url = "https://example.com" }
});
```

### 7. Geolocation QR Code
```csharp
var qrCode = await qrCodeClient.CreateGeolocationAsync(new CreateGeolocationQRCodeRequest
{
    Name = "Our Location",
    TemplateId = "template_id",
    Geolocation = new QRCodeGeolocationTarget
    {
        Latitude = "40.7128",
        Longitude = "-74.0060"
    }
});
```

## CRUD Operations

### Get QR Code
```csharp
var qrCode = await qrCodeClient.GetAsync("qr_code_id");
Console.WriteLine($"Visitors: {qrCode.NumberOfVisitors}");
```

### Update QR Code
```csharp
var updated = await qrCodeClient.UpdateURLAsync("qr_code_id", new UpdateURLQRCodeRequest
{
    Name = "New Name",
    TemplateId = "template_id",
    Url = new QRCodeUrlTarget { Url = "https://newurl.com" }
});
```

### Delete QR Code
```csharp
await qrCodeClient.DeleteAsync("qr_code_id");
```

### List QR Codes
```csharp
var result = await qrCodeClient.ListAsync(
    new ListQRCodesParams { Tag = "marketing" },
    new PaginationParams { Page = 0, PageSize = 20 }
);

foreach (var qr in result.Items)
{
    Console.WriteLine($"{qr.Name}: {qr.QrCodeLandingPage}");
}
```

## Advanced Features

### Custom Landing Page ID
```csharp
var qrCode = await qrCodeClient.CreateURLAsync(new CreateURLQRCodeRequest
{
    Name = "Custom Slug",
    TemplateId = "template_id",
    CustomLandingId = "my-custom-slug",
    Url = new QRCodeUrlTarget { Url = "https://example.com" }
});
```

### Monetization
```csharp
var qrCode = await qrCodeClient.CreateURLAsync(new CreateURLQRCodeRequest
{
    Name = "Monetized QR",
    TemplateId = "template_id",
    IsEnableMonetization = true,
    PageInfo = new QRCodePageInfo
    {
        Title = "Please Wait",
        Description = "Redirecting..."
    },
    Url = new QRCodeUrlTarget { Url = "https://example.com" }
});
```

### Tracking with RefId and Tag
```csharp
var qrCode = await qrCodeClient.CreateURLAsync(new CreateURLQRCodeRequest
{
    Name = "Campaign QR",
    TemplateId = "template_id",
    RefId = "CAMPAIGN-2024",
    Tag = "marketing",
    Url = new QRCodeUrlTarget { Url = "https://example.com" }
});
```

## Method Reference

| QR Type | Create Method | Update Method |
|---------|--------------|---------------|
| Free Text | `CreateFreeTextAsync()` | `UpdateFreeTextAsync()` |
| Email | `CreateEmailAsync()` | `UpdateEmailAsync()` |
| WiFi | `CreateWifiAsync()` | `UpdateWifiAsync()` |
| Phone Call | `CreateCallAsync()` | `UpdateCallAsync()` |
| SMS | `CreateSMSAsync()` | `UpdateSMSAsync()` |
| URL | `CreateURLAsync()` | `UpdateURLAsync()` |
| Geolocation | `CreateGeolocationAsync()` | `UpdateGeolocationAsync()` |

## Common Properties

All QR code requests support:
- `Name` - QR code name
- `TemplateId` - Template to use (required)
- `RefId` - External reference ID
- `Tag` - Custom tag for filtering
- `CustomLandingId` - Custom URL slug
- `IsEnableMonetization` - Enable ads
- `PageInfo` - Landing page configuration

## Response Fields

All responses include:
- `Id` - Database ID
- `QrCodeId` - Unique QR code identifier
- `QrCodeLandingPage` - Scannable URL
- `ShorterLink` - Short URL
- `NumberOfVisitors` - Scan count
- `Status` - Approval status
- `CreatedAt` - Creation timestamp
- `UpdatedAt` - Last update timestamp
