# Posty5.QRCode

Generate and manage QR codes for seven content types with the .NET SDK: free text, email, WiFi, phone call, SMS, URL and map location, styled by your QR code templates.

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

`Posty5.QRCode` is a **specialized tool package** for generating and managing QR codes on the Posty5 platform. It enables developers to build QR code solutions for marketing campaigns, contactless interactions, WiFi sharing, and more.

### Key Capabilities

- **📱 7 QR Code Types** - URL, Free Text, Email, WiFi, SMS, Phone Call, and Geolocation
- **🎨 Template Support** - Style every code with one of your QR code templates
- **📊 Visit Counts** - `NumberOfVisitors` and `LastVisitorDate` count visits to the code's Posty5 page (`QrCodeLandingPageURL`). A downloaded QR image encodes its content directly, so scanning it is **not** counted
- **📈 Visit Analytics** - `GetAnalyticsAsync`: visits, unique visitors, a series and breakdowns by channel (scan vs click), country, device, OS, browser, referrer and language, bots counted apart
- **🏷️ Tag & Reference Support** - Organize QR codes with custom tags and reference IDs
- **🎯 Landing Pages** - `IsEnableLandingPage` + `PageInfo` put your title and description on the code's Posty5 page
- **🔍 Filtering** - By name, status, tag, reference ID, template or source
- **📝 CRUD Operations** - Complete create, read, update, delete operations
- **📈 Cursor Pagination** - `PaginationParams { Cursor, PageSize }`

The text a QR image encodes is built by the API from the content you send (`qrCodeTarget`); this SDK does not build it. From the API's link-qr truth pass on, the server also escapes it, so an `&` in an email subject or a `;` in a WiFi password encodes correctly.

### Role in the Posty5 Ecosystem

- Short links from `Posty5.ShortLink` get their own QR code, styled by the same templates
- Create URL QR codes pointing to `Posty5.HtmlHosting` hosted pages

---

## ⬆️ Upgrading to 3.2.0

- New: `GetAnalyticsAsync(id, query?)` - visit analytics (totals, series, breakdowns) for one QR code.
- New: `GetStatisticsAsync(query?)` - account-wide counts over all your QR codes, with visit totals in the range and a UTC daily list.
- Needs `Posty5.Core` 3.2.0 (the shared `LinkAnalytics*` / `LinkStatistics*` models).
- `TemplateId` is now `required` on every create and update request. The API refuses an API-key call without one, so code that left it out never succeeded.
- `IsEnableMonetization` is `[Obsolete]` everywhere and is never sent (the API never accepted it and answered 400).
- The typed methods no longer send `options.text`; the API builds it from `qrCodeTarget`. Free text still sends it.
- New: `IsEnableLandingPage` on every request.

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
var qrCode = await qrCodes.CreateURLAsync(new QRCodeCreateURLRequestModel
{
    Name = "Website QR Code",
    TemplateId = "your-template-id", // Required for API-key calls
    Url = new QRCodeUrlTargetModel
    {
        Url = "https://posty5.com"
    },
    Tag = "marketing", // Optional: For organization
    RefId = "CAMPAIGN-001" // Optional: External reference
});

Console.WriteLine($"QR Code Page: {qrCode.QrCodeLandingPageURL}");
Console.WriteLine($"QR Code Image: {qrCode.QrCodeDownloadURL}");
Console.WriteLine($"QR Code ID: {qrCode.Id}");

// List QR codes (cursor pagination)
var page = await qrCodes.ListAsync(
    null,
    new PaginationParams { PageSize = 20 }
);

foreach (var qr in page.Items)
{
    Console.WriteLine($"{qr.Name}: {qr.NumberOfVisitors} page visits");
}
// Next page: new PaginationParams { Cursor = page.Pagination.NextCursor, PageSize = 20 }
```

---

## 📚 API Reference & Examples

### Creating QR Codes

The SDK supports 7 different QR code types. Each type has its own creation method with type-specific parameters.

---

#### CreateURLAsync

Create a URL QR code that redirects users to a website when scanned.

**Parameters:**

- `data` (QRCodeCreateURLRequestModel): QR code data
  - `TemplateId` (string, **required**): QR code template ID. The API answers "Template Id Is Required" to an API-key call without one; your template IDs are on the dashboard's QR code templates page
  - `Url` (QRCodeUrlTargetModel, **required**):
    - `Url` (string): Target website URL (`http://` or `https://` from the API's link-qr truth pass on)
  - `Name` (string?): Human-readable name
  - `Tag` (string?): Custom tag
  - `RefId` (string?): External reference ID
  - `CustomLandingId` (string?): Custom slug for the code's page, 4-32 lowercase letters, digits or hyphens (Starter plan and above)
  - `IsEnableLandingPage` (bool?): Show `PageInfo` on the code's Posty5 page
  - `PageInfo` (`QRCodePageInfoModel?`): `Title` (required when `IsEnableLandingPage` is `true`) and `Description`

Every create and update method takes these common properties; the type-specific ones are listed below.

**Returns:** `Task<QRCodeModel>` - Created QR code details

**Example:**

```csharp
var qrCode = await qrCodes.CreateURLAsync(new QRCodeCreateURLRequestModel
{
    Name = "Company Website",
    TemplateId = "template-123",
    Url = new QRCodeUrlTargetModel { Url = "https://example.com" }
});

Console.WriteLine($"Scan this: {qrCode.QrCodeLandingPageURL}");
```

---

#### CreateFreeTextAsync

Create a free text QR code with any custom text content.

**Parameters:**

- `data` (QRCodeCreateFreeTextRequestModel): QR code data
  - `Name`, `TemplateId`...
  - `Text` (string, **required**): Custom text to encode

**Returns:** `Task<QRCodeModel>`

**Example:**

```csharp
var textQR = await qrCodes.CreateFreeTextAsync(new QRCodeCreateFreeTextRequestModel
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

- `data` (QRCodeCreateEmailRequestModel): QR code data
  - `Email` (QRCodeEmailTargetModel):
    - `Email` (string): Recipient email
    - `Subject` (string): Subject line
    - `Body` (string): Email body

**Returns:** `Task<QRCodeModel>`

**Example:**

```csharp
var supportQR = await qrCodes.CreateEmailAsync(new QRCodeCreateEmailRequestModel
{
    Name = "Contact Support",
    TemplateId = "template-123",
    Email = new QRCodeEmailTargetModel
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

- `data` (QRCodeCreateWifiRequestModel): QR code data
  - `Wifi` (QRCodeWifiTargetModel):
    - `Name` (string): SSID
    - `AuthenticationType` (string): 'WPA', 'WEP', or 'nopass'
    - `Password` (string): Network password

**Returns:** `Task<QRCodeModel>`

**Example:**

```csharp
var wifiQR = await qrCodes.CreateWifiAsync(new QRCodeCreateWifiRequestModel
{
    Name = "Office WiFi",
    TemplateId = "template-123",
    Wifi = new QRCodeWifiTargetModel
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

- `data` (QRCodeCreateCallRequestModel): QR code data
  - `Call` (QRCodeCallTargetModel):
    - `PhoneNumber` (string): Phone number to call

**Returns:** `Task<QRCodeModel>`

**Example:**

```csharp
var hotlineQR = await qrCodes.CreateCallAsync(new QRCodeCreateCallRequestModel
{
    Name = "Customer Service",
    TemplateId = "template-123",
    Call = new QRCodeCallTargetModel
    {
        PhoneNumber = "+1-800-123-4567"
    }
});
```

---

#### CreateSMSAsync

Create an SMS QR code.

**Parameters:**

- `data` (QRCodeCreateSMSRequestModel): QR code data
  - `Sms` (QRCodeSmsTargetModel):
    - `PhoneNumber` (string): Recipient number
    - `Message` (string): Message text

**Returns:** `Task<QRCodeModel>`

**Example:**

```csharp
var smsQR = await qrCodes.CreateSMSAsync(new QRCodeCreateSMSRequestModel
{
    Name = "Join Contest",
    TemplateId = "template-123",
    Sms = new QRCodeSmsTargetModel
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

- `data` (QRCodeCreateGeolocationRequestModel): QR code data
  - `Geolocation` (QRCodeGeolocationTargetModel):
    - `Latitude` (string/double): Latitude
    - `Longitude` (string/double): Longitude

**Returns:** `Task<QRCodeModel>`

**Example:**

```csharp
var mapQR = await qrCodes.CreateGeolocationAsync(new QRCodeCreateGeolocationRequestModel
{
    Name = "Office Location",
    TemplateId = "template-123",
    Geolocation = new QRCodeGeolocationTargetModel
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
Console.WriteLine($"Page visits: {qrCode.NumberOfVisitors}");
```

---

#### ListAsync

Search and filter QR codes.

**Parameters:**

- `listParams` (QRCodeListParamsModel?, optional): Filter criteria
  - `Name`, `QrCodeId`, `TemplateId`, `Tag`, `RefId`, `CreatedFrom` (string?)
  - `Status` (`QRCodeStatusType?`): `New`, `Pending`, `Approved`, `Rejected`
  - `IsEnableMonetization` is obsolete and not sent
- `pagination` (PaginationParams?, optional): `Cursor` and `PageSize`

The `RefId` filter works from the API's link-qr truth pass on (an older API ignored it and returned every record). List items carry the SMS message, `IsEnableLandingPage` and `Status` from the same release.

**Example:**

```csharp
var marketingQRs = await qrCodes.ListAsync(new QRCodeListParamsModel
{
    Tag = "marketing",
    Status = QRCodeStatusType.Approved
});

foreach (var qr in marketingQRs.Items)
{
    Console.WriteLine($"{qr.Name} - {qr.QrCodeLandingPageURL}");
}
```

---

### Visit Analytics

#### GetStatisticsAsync

Account-wide counts over all your QR codes: `GET /api/qr-code/statistics`.

**Parameters:**

- `query` (`LinkStatisticsQuery?`): `Period` (`LinkStatisticsPeriod.Today`, `Last7Days`, `Last30Days`, `Month`, `Custom`; default last 30 days) or a custom `From` / `To` (sent as `yyyy-MM-dd`). Setting `From`/`To` with a non-custom `Period`, or `From` after `To`, throws `ArgumentException` before sending.

**Returns:** `Task<QRCodeStatisticsModel>` - `Range { From, To, Period }` and `Data`:

- `Totals`: `TotalQRCodes`, `TotalVisitors` (lifetime counter, includes visits from before analytics launched), `AvgVisitorsPerQRCode`, and from visit analytics `VisitsInRange`, `UniqueVisitorsInRange` (sum of daily uniques), `BotVisitsInRange`
- `Daily`: one row per **UTC day** - `Day` (`yyyy-MM-dd`), `CreatedCount` (QR codes created), `VisitorsSum` (visits by people, bots excluded)
- `TopQRCodes`: the ten QR codes with the most visits in the range, each with `VisitsInRange`

Visits are visits to the codes' Posty5 pages; a scan of a static code opens its content directly and is not counted.

**Example:**

```csharp
using Posty5.Core.Models;

var stats = await qrCodes.GetStatisticsAsync(new LinkStatisticsQuery { Period = LinkStatisticsPeriod.Last7Days });
Console.WriteLine($"{stats.Data.Totals.VisitsInRange} visits ({stats.Data.Totals.BotVisitsInRange} bots)");
foreach (var day in stats.Data.Daily)
    Console.WriteLine($"{day.Day}: {day.VisitorsSum} visits, {day.CreatedCount} created");
```

#### GetAnalyticsAsync

Visits, unique visitors, a series and breakdowns (country, device, OS, browser, referrer, channel, language) for one QR code: `GET /api/qr-code/{id}/analytics`. Needs an API with link + QR visit analytics.

**Parameters:**

- `id` (string): QR code ID
- `query` (`LinkAnalyticsQuery?`): Range, interval, time zone and breakdowns; `null` for the API defaults

**`LinkAnalyticsQuery`** (every property optional):

| Property | Sent as | API default |
| --- | --- | --- |
| `From`, `To` (`DateTime?`) | `from` / `to`, `yyyy-MM-dd` (date part only) | last 30 days, up to today |
| `Interval` (`LinkAnalyticsInterval.Day` / `Week` / `Month`) | `interval` | `day` |
| `Tz` (IANA name, e.g. `Africa/Cairo`) | `tz` | your account time zone, else UTC |
| `Breakdown` (`LinkAnalyticsBreakdown.Country`, `Device`, `Os`, `Browser`, `Referrer`, `Channel`, `Language`, `Variant`, `Rule`) | `breakdown`, comma list | every breakdown your plan allows |
| `AllBreakdowns` (`bool`) | `breakdown=all` (same answer as leaving `Breakdown` unset) - cannot be combined with `Breakdown` | `false` |
| `Limit` (`int?`, 1-50; outside that range throws `ArgumentOutOfRangeException` before sending) | `limit`: rows per breakdown; the overflow comes back as key `other`, a missing value as `unknown` | 10 |

**Returns:** `Task<LinkAnalyticsModel>` - `Totals { Visits, UniqueVisitors, BotVisits }`, `Series` (`{ Date, Visits, UniqueVisitors }` per bucket, `Date` as `yyyy-MM-dd`), `Breakdowns` (keyed by wire name: `"country"`, `"device"`, ...; rows `{ Key, Visits, UniqueVisitors }`) and `Meta { From, To, Interval, Timezone, Source, AnalyticsStartedAt, Locked, MaxHistoryDays }`. The models live in `Posty5.Core.Models`.

**Example:**

```csharp
using Posty5.Core.Models;

var analytics = await qrCodes.GetAnalyticsAsync("qr-code-id-123", new LinkAnalyticsQuery
{
    From = new DateTime(2026, 10, 1),
    To = new DateTime(2026, 10, 31),
    Interval = LinkAnalyticsInterval.Week,
    AllBreakdowns = true
});

Console.WriteLine($"{analytics.Totals.Visits} visits, {analytics.Totals.BotVisits} bot visits");
foreach (var point in analytics.Series)
    Console.WriteLine($"{point.Date}: {point.Visits}");
foreach (var row in analytics.Breakdowns.GetValueOrDefault("channel") ?? new())
    Console.WriteLine($"{row.Key}: {row.Visits}"); // click / scan
foreach (var locked in analytics.Meta.Locked)
    Console.WriteLine($"{locked.Breakdown} needs {locked.RequiredPlan}");
```

**What the numbers mean:**

- Bots and link-preview crawlers are excluded from every `Visits` and counted only in `Totals.BotVisits`.
- `UniqueVisitors` over more than one day is the **sum of daily uniques** (the visitor hash rotates daily, so one person on two days counts twice).
- Data starts on `Meta.AnalyticsStartedAt`, when Posty5 started recording visits; nothing earlier exists.
- Days are counted in `Tz` (default: your account time zone). When part of the range is older than raw-event retention, the answer uses UTC days and `Meta.Timezone` says `UTC`.
- Reading analytics costs no credits. Your plan decides which breakdowns and how much history you get: unless you name breakdowns you get every one your plan allows, and the others are listed in `Meta.Locked` with `RequiredPlan` as a plan key (e.g. `basic` = Starter). `Meta.MaxHistoryDays` is `30` on Free and `null` (unlimited) on Starter and up. Naming a locked breakdown in `Breakdown`, or a `From` older than your plan's history, throws `Posty5Exception` with `StatusCode == 403` and the API's message (`This feature is not available on your current plan.`) in `ResponseBody`; a QR code you may not read answers 403 `You Have Not Permission`.
- `Meta.Source` is `events`, `rollup` or `mixed`.
- A missing QR code answers **400**, not 404: `Posty5ValidationException` whose message contains `The QR Code Is Not Found`.
- A static QR code (text, Wi-Fi, ...) counts visits to its Posty5 page only; scanning a downloaded image that encodes its content directly never reaches Posty5 and is not counted. The `channel` breakdown tells scans from clicks.

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
await qrCodes.UpdateURLAsync("qr-code-id-123", new QRCodeUpdateURLRequestModel
{
    Name = "Summer Sale - Extended",
    TemplateId = "template-123",
    Url = new QRCodeUrlTargetModel { Url = "https://example.com/extended" },
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

### Templates — QRCodeTemplateClient (3.1.0+)

The template ids every create method takes as `TemplateId`. Read-only; a
template is designed in the dashboard.

```csharp
var templates = new QRCodeTemplateClient(httpClient);

var mine = await templates.ListUserTemplatesAsync();              // your own
var shared = await templates.ListPublicTemplatesAsync("classic"); // Posty5's public ones (no API key needed)
var templateId = mine.Items.FirstOrDefault()?.Id ?? shared.Items.First().Id;
```

| Method | Route |
| --- | --- |
| `ListUserTemplatesAsync(term?, pagination?)` | `GET /api/qr-code-template/user-lookup` |
| `ListPublicTemplatesAsync(term?, schemeType?, pagination?)` | `GET /api/qr-code-template/public-lookup` |

Both page by cursor (`PaginationParams`, `Pagination.NextCursor`).

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
