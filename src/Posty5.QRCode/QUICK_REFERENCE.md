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

### 8. vCard / Event / WhatsApp / Review / Social (3.4.0)
```csharp
await qrCodeClient.CreateVCardAsync(new QRCodeCreateVCardRequestModel { Name = "Card", TemplateId = "template_id", VCard = new() { FirstName = "Ada" } });
await qrCodeClient.CreateEventAsync(new QRCodeCreateEventRequestModel { Name = "Event", TemplateId = "template_id", Event = new() { Title = "Launch", StartsAt = DateTimeOffset.UtcNow.AddDays(7) } });
await qrCodeClient.CreateWhatsAppAsync(new QRCodeCreateWhatsAppRequestModel { Name = "Chat", TemplateId = "template_id", WhatsApp = new() { PhoneNumber = "+201000000000" } });
await qrCodeClient.CreateReviewAsync(new QRCodeCreateReviewRequestModel { Name = "Review", TemplateId = "template_id", Review = new() { Platform = QRCodeReviewPlatforms.Google, PlaceId = "ChIJ..." } });
await qrCodeClient.CreateSocialAsync(new QRCodeCreateSocialRequestModel { Name = "Social", TemplateId = "template_id", Social = new() { Profiles = new() { new() { Platform = QRCodeSocialPlatforms.Instagram, Handle = "posty5" } } } });
// Update*Async(id, request) twins: UpdateVCardAsync, UpdateEventAsync, UpdateWhatsAppAsync, UpdateReviewAsync, UpdateSocialAsync
```

### 9. App store / File (3.4.0, dynamic-only)
```csharp
await qrCodeClient.CreateAppStoreAsync(new QRCodeCreateAppStoreRequestModel { Name = "App", TemplateId = "template_id", AppStore = new() { AndroidUrl = "https://play.google.com/...", IosUrl = "https://apps.apple.com/...", FallbackUrl = "https://example.com/app" } });

await using var pdf = File.OpenRead("menu.pdf");
var qr = await qrCodeClient.CreateFileAsync(new QRCodeCreateFileRequestModel { Name = "Menu", TemplateId = "template_id", File = new() { FileName = "menu.pdf" } }, pdf, QRCodeFileMimeTypes.Pdf);
// Rename only, keeps the stored file:
await qrCodeClient.UpdateFileAsync(qr.Id, new QRCodeUpdateFileRequestModel { Name = "Menu", TemplateId = "template_id", File = new() { FileName = "menu-2026.pdf" } });
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

### Visit Analytics
```csharp
using Posty5.Core.Models;

var analytics = await qrCodeClient.GetAnalyticsAsync("qr_code_id", new LinkAnalyticsQuery
{
    From = new DateTime(2026, 10, 1),
    Interval = LinkAnalyticsInterval.Day,
    Breakdown = new[] { LinkAnalyticsBreakdown.Channel, LinkAnalyticsBreakdown.Device }
    // leave Breakdown unset (or AllBreakdowns = true): every breakdown your plan allows, the rest in Meta.Locked
});
Console.WriteLine($"Visits: {analytics.Totals.Visits}, bots: {analytics.Totals.BotVisits}");
```
Bots are excluded from `Visits`; `UniqueVisitors` over several days is the sum of daily uniques; data starts on `Meta.AnalyticsStartedAt`. A breakdown your plan does not include, named explicitly, throws `Posty5Exception` with `StatusCode == 403`. `Limit` is 1-50 (default 10; overflow as `other`, missing values as `unknown`). A missing QR code answers 400 (`Posty5ValidationException`, "The QR Code Is Not Found").

### Account Statistics
```csharp
var stats = await qrCodeClient.GetStatisticsAsync(new LinkStatisticsQuery { Period = LinkStatisticsPeriod.Month });
Console.WriteLine($"{stats.Data.Totals.TotalQRCodes} codes, {stats.Data.Totals.VisitsInRange} visits this month");
```
`Daily` is in UTC days; `TopQRCodes` are the ten codes with the most visits in the range.

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

Every type: `GetAsync()`, `ListAsync()`, `DeleteAsync()`, `GetAnalyticsAsync()`. Account-wide: `GetStatisticsAsync()`.

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
