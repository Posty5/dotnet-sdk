# Posty5.QRCode - Quick Reference Guide

## Installation

```bash
dotnet add package Posty5.QRCode
```

## Basic Setup

```csharp
using Posty5.Core.Configuration;
using Posty5.Core.Http;
using Posty5.QRCode;
using Posty5.QRCode.Models;

var httpClient = new Posty5HttpClient(new Posty5Options
{
    BaseUrl = "https://api.posty5.com",
    ApiKey = "your-api-key"
});

var qrCodeClient = new QRCodeClient(httpClient);
```

## Quick Examples

### 1. Free Text QR Code
```csharp
var qrCode = await qrCodeClient.CreateFreeTextAsync(new QRCodeCreateFreeTextRequestModel
{
    Name = "My Text QR",
    TemplateId = "template_id",
    Text = "Hello World!"
});
```

### 2. Email QR Code
```csharp
var qrCode = await qrCodeClient.CreateEmailAsync(new QRCodeCreateEmailRequestModel
{
    Name = "Contact Email",
    TemplateId = "template_id",
    Email = new QRCodeEmailTargetModel
    {
        Email = "contact@example.com",
        Subject = "Hello",
        Body = "Your message here"
    }
});
```

### 3. WiFi QR Code
```csharp
var qrCode = await qrCodeClient.CreateWifiAsync(new QRCodeCreateWifiRequestModel
{
    Name = "Office WiFi",
    TemplateId = "template_id",
    Wifi = new QRCodeWifiTargetModel
    {
        Name = "NetworkName",
        AuthenticationType = "WPA",
        Password = "password123"
    }
});
```

### 4. Phone Call QR Code
```csharp
var qrCode = await qrCodeClient.CreateCallAsync(new QRCodeCreateCallRequestModel
{
    Name = "Call Support",
    TemplateId = "template_id",
    Call = new QRCodeCallTargetModel
    {
        PhoneNumber = "+1234567890"
    }
});
```

### 5. SMS QR Code
```csharp
var qrCode = await qrCodeClient.CreateSMSAsync(new QRCodeCreateSMSRequestModel
{
    Name = "Text Us",
    TemplateId = "template_id",
    Sms = new QRCodeSmsTargetModel
    {
        PhoneNumber = "+1234567890",
        Message = "Hello from QR!"
    }
});
```

### 6. URL QR Code
```csharp
var qrCode = await qrCodeClient.CreateURLAsync(new QRCodeCreateURLRequestModel
{
    Name = "Website",
    TemplateId = "template_id",
    Url = new QRCodeUrlTargetModel { Url = "https://example.com" }
});
```

### 7. Geolocation QR Code
```csharp
var qrCode = await qrCodeClient.CreateGeolocationAsync(new QRCodeCreateGeolocationRequestModel
{
    Name = "Our Location",
    TemplateId = "template_id",
    Geolocation = new QRCodeGeolocationTargetModel
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
var updated = await qrCodeClient.UpdateURLAsync("qr_code_id", new QRCodeUpdateURLRequestModel
{
    Name = "New Name",
    TemplateId = "template_id",
    Url = new QRCodeUrlTargetModel { Url = "https://newurl.com" }
});
```

### Delete QR Code
```csharp
await qrCodeClient.DeleteAsync("qr_code_id");
```

### List QR Codes
```csharp
var result = await qrCodeClient.ListAsync(
    new QRCodeListParamsModel { Tag = "marketing" },
    new PaginationParams { PageSize = 20 }
);

foreach (var qr in result.Items)
{
    Console.WriteLine($"{qr.Name}: {qr.QrCodeLandingPageURL}");
}
```

## Advanced Features

### Custom Landing Page ID
```csharp
var qrCode = await qrCodeClient.CreateURLAsync(new QRCodeCreateURLRequestModel
{
    Name = "Custom Slug",
    TemplateId = "template_id",
    CustomLandingId = "my-custom-slug",
    Url = new QRCodeUrlTargetModel { Url = "https://example.com" }
});
```

### Landing Page Title and Description
```csharp
var qrCode = await qrCodeClient.CreateURLAsync(new QRCodeCreateURLRequestModel
{
    Name = "Spring Menu",
    TemplateId = "template_id",
    IsEnableLandingPage = true,
    PageInfo = new QRCodePageInfoModel
    {
        Title = "Spring Menu",          // required when IsEnableLandingPage is true
        Description = "New dishes every week"
    },
    Url = new QRCodeUrlTargetModel { Url = "https://example.com" }
});
```

### Tracking with RefId and Tag
```csharp
var qrCode = await qrCodeClient.CreateURLAsync(new QRCodeCreateURLRequestModel
{
    Name = "Campaign QR",
    TemplateId = "template_id",
    RefId = "CAMPAIGN-2024",
    Tag = "marketing",
    Url = new QRCodeUrlTargetModel { Url = "https://example.com" }
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
- `TemplateId` - Template to use (`required`; the API refuses an API-key call without it)
- `RefId` - External reference ID
- `Tag` - Custom tag for filtering
- `CustomLandingId` - Custom slug for the code's page (Starter plan and above)
- `IsEnableLandingPage` - Show `PageInfo` on the code's Posty5 page
- `PageInfo` - Landing page title (required when `IsEnableLandingPage` is true) and description

`IsEnableMonetization` is obsolete: the API never accepted it, and the SDK does not send it.

The typed methods send `qrCodeTarget` only; the API builds the text the image encodes.

## Response Fields

All responses include:
- `Id` - Database ID
- `QrCodeId` - Unique QR code identifier
- `QrCodeLandingPageURL` - The code's Posty5 page
- `QrCodeDownloadURL` - The QR image
- `NumberOfVisitors` - Visits to the code's Posty5 page (scanning a downloaded image is not counted)
- `Status` - Approval status
- `CreatedAt` - Creation timestamp
- `UpdatedAt` - Last update timestamp
