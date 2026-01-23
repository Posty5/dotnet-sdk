# Posty5.QRCode

QR Code management client for the Posty5 .NET SDK. Create and manage dynamic QR codes for email, WiFi, phone calls, SMS, URLs, geolocation, and free text.

## Features

- ✅ Create and manage QR codes for multiple content types
- ✅ Full async/await support with cancellation tokens
- ✅ Strongly typed request and response models
- ✅ Comprehensive XML documentation
- ✅ Support for custom templates and landing pages
- ✅ Built-in monetization support
- ✅ Filtering and pagination for QR code listings

## Installation

```bash
dotnet add package Posty5.QRCode
```

## Quick Start

```csharp
using Posty5.Core.Http;
using Posty5.QRCode;
using Posty5.QRCode.Models;

// Initialize HTTP client
var httpClient = new Posty5HttpClient(new Posty5HttpClientOptions
{
    BaseUrl = "https://api.posty5.com",
    ApiKey = "your-api-key"
});

// Create QR Code client
var qrCodeClient = new QRCodeClient(httpClient);

// Create a URL QR code
var qrCode = await qrCodeClient.CreateURLAsync(new CreateURLQRCodeRequest
{
    Name = "My Website",
    TemplateId = "template_123",
    Url = new QRCodeUrlTarget { Url = "https://example.com" }
});

Console.WriteLine($"QR Code Created: {qrCode.QrCodeLandingPage}");
Console.WriteLine($"Short Link: {qrCode.ShorterLink}");
```

## Supported QR Code Types

### 1. Free Text QR Code

Encode any custom text content.

```csharp
var qrCode = await qrCodeClient.CreateFreeTextAsync(new CreateFreeTextQRCodeRequest
{
    Name = "Custom Text QR",
    TemplateId = "template_123",
    Text = "Any custom text you want to encode"
});
```

### 2. Email QR Code

Opens the default email client with pre-filled data.

```csharp
var qrCode = await qrCodeClient.CreateEmailAsync(new CreateEmailQRCodeRequest
{
    Name = "Contact Us",
    TemplateId = "template_123",
    Email = new QRCodeEmailTarget
    {
        Email = "contact@example.com",
        Subject = "Inquiry from QR Code",
        Body = "Hello, I would like to know more about..."
    }
});
```

### 3. WiFi QR Code

Connect to WiFi networks instantly.

```csharp
var qrCode = await qrCodeClient.CreateWifiAsync(new CreateWifiQRCodeRequest
{
    Name = "Office WiFi",
    TemplateId = "template_123",
    Wifi = new QRCodeWifiTarget
    {
        Name = "OfficeNetwork",
        AuthenticationType = "WPA",
        Password = "secret123"
    }
});
```

### 4. Phone Call QR Code

Initiates a phone call when scanned.

```csharp
var qrCode = await qrCodeClient.CreateCallAsync(new CreateCallQRCodeRequest
{
    Name = "Call Support",
    TemplateId = "template_123",
    Call = new QRCodeCallTarget
    {
        PhoneNumber = "+1234567890"
    }
});
```

### 5. SMS QR Code

Opens messaging app with pre-filled text.

```csharp
var qrCode = await qrCodeClient.CreateSMSAsync(new CreateSMSQRCodeRequest
{
    Name = "Text Us",
    TemplateId = "template_123",
    Sms = new QRCodeSmsTarget
    {
        PhoneNumber = "+1234567890",
        Message = "I scanned your QR code!"
    }
});
```

### 6. URL QR Code

Redirects to any website.

```csharp
var qrCode = await qrCodeClient.CreateURLAsync(new CreateURLQRCodeRequest
{
    Name = "Website Link",
    TemplateId = "template_123",
    Url = new QRCodeUrlTarget { Url = "https://example.com" },
    Tag = "marketing",
    RefId = "CAMPAIGN-001"
});
```

### 7. Geolocation QR Code

Opens map application with specific coordinates.

```csharp
var qrCode = await qrCodeClient.CreateGeolocationAsync(new CreateGeolocationQRCodeRequest
{
    Name = "Our Office Location",
    TemplateId = "template_123",
    Geolocation = new QRCodeGeolocationTarget
    {
        Latitude = "40.7128",
        Longitude = "-74.0060"
    }
});
```

## Update QR Codes

All QR code types can be updated using their respective update methods:

```csharp
var updatedQrCode = await qrCodeClient.UpdateURLAsync("qr_code_id", new UpdateURLQRCodeRequest
{
    Name = "Updated Website Link",
    TemplateId = "template_123",
    Url = new QRCodeUrlTarget { Url = "https://newexample.com" }
});
```

## Get QR Code Details

```csharp
var qrCode = await qrCodeClient.GetAsync("qr_code_id");
Console.WriteLine($"Name: {qrCode.Name}");
Console.WriteLine($"Visitors: {qrCode.NumberOfVisitors}");
Console.WriteLine($"Status: {qrCode.Status}");
```

## List QR Codes

List all QR codes with optional filtering and pagination:

```csharp
// List all QR codes
var allQrCodes = await qrCodeClient.ListAsync();

// List with filters
var filteredQrCodes = await qrCodeClient.ListAsync(
    new ListQRCodesParams
    {
        Status = "approved",
        Tag = "marketing",
        IsEnableMonetization = true
    },
    new PaginationParams
    {
        Page = 1,
        PageSize = 20
    }
);

foreach (var qr in filteredQrCodes.Items)
{
    Console.WriteLine($"{qr.Name} - Visitors: {qr.NumberOfVisitors}");
}
```

## Delete QR Code

```csharp
await qrCodeClient.DeleteAsync("qr_code_id");
```

## Advanced Features

### Custom Landing Pages

```csharp
var qrCode = await qrCodeClient.CreateURLAsync(new CreateURLQRCodeRequest
{
    Name = "Custom Landing Page",
    TemplateId = "template_123",
    CustomLandingId = "my-custom-id", // Max 32 characters
    Url = new QRCodeUrlTarget { Url = "https://example.com" }
});
```

### Monetization

Enable monetization to show ads before redirecting:

```csharp
var qrCode = await qrCodeClient.CreateURLAsync(new CreateURLQRCodeRequest
{
    Name = "Monetized Link",
    TemplateId = "template_123",
    IsEnableMonetization = true,
    PageInfo = new QRCodePageInfo
    {
        Title = "Please Wait",
        Description = "You will be redirected shortly..."
    },
    Url = new QRCodeUrlTarget { Url = "https://example.com" }
});
```

### Reference ID and Tags

Track QR codes with custom identifiers:

```csharp
var qrCode = await qrCodeClient.CreateURLAsync(new CreateURLQRCodeRequest
{
    Name = "Campaign QR Code",
    TemplateId = "template_123",
    RefId = "CAMPAIGN-2024-Q1",
    Tag = "marketing",
    Url = new QRCodeUrlTarget { Url = "https://example.com" }
});

// Later, filter by reference ID or tag
var campaignQrCodes = await qrCodeClient.ListAsync(
    new ListQRCodesParams { RefId = "CAMPAIGN-2024-Q1" }
);
```

## Error Handling

```csharp
try
{
    var qrCode = await qrCodeClient.CreateURLAsync(new CreateURLQRCodeRequest
    {
        Name = "My QR Code",
        TemplateId = "template_123",
        Url = new QRCodeUrlTarget { Url = "https://example.com" }
    });
}
catch (InvalidOperationException ex)
{
    Console.WriteLine($"Failed to create QR code: {ex.Message}");
}
catch (HttpRequestException ex)
{
    Console.WriteLine($"Network error: {ex.Message}");
}
```

## Cancellation Token Support

All methods support cancellation tokens for better async control:

```csharp
using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));

try
{
    var qrCode = await qrCodeClient.CreateURLAsync(
        new CreateURLQRCodeRequest
        {
            Name = "My QR Code",
            TemplateId = "template_123",
            Url = new QRCodeUrlTarget { Url = "https://example.com" }
        },
        cts.Token
    );
}
catch (OperationCanceledException)
{
    Console.WriteLine("Request was cancelled");
}
```

## API Reference

### QRCodeClient Methods

#### Create Methods
- `CreateFreeTextAsync(CreateFreeTextQRCodeRequest, CancellationToken)` - Create free text QR code
- `CreateEmailAsync(CreateEmailQRCodeRequest, CancellationToken)` - Create email QR code
- `CreateWifiAsync(CreateWifiQRCodeRequest, CancellationToken)` - Create WiFi QR code
- `CreateCallAsync(CreateCallQRCodeRequest, CancellationToken)` - Create phone call QR code
- `CreateSMSAsync(CreateSMSQRCodeRequest, CancellationToken)` - Create SMS QR code
- `CreateURLAsync(CreateURLQRCodeRequest, CancellationToken)` - Create URL QR code
- `CreateGeolocationAsync(CreateGeolocationQRCodeRequest, CancellationToken)` - Create geolocation QR code

#### Update Methods
- `UpdateFreeTextAsync(string, UpdateFreeTextQRCodeRequest, CancellationToken)` - Update free text QR code
- `UpdateEmailAsync(string, UpdateEmailQRCodeRequest, CancellationToken)` - Update email QR code
- `UpdateWifiAsync(string, UpdateWifiQRCodeRequest, CancellationToken)` - Update WiFi QR code
- `UpdateCallAsync(string, UpdateCallQRCodeRequest, CancellationToken)` - Update phone call QR code
- `UpdateSMSAsync(string, UpdateSMSQRCodeRequest, CancellationToken)` - Update SMS QR code
- `UpdateURLAsync(string, UpdateURLQRCodeRequest, CancellationToken)` - Update URL QR code
- `UpdateGeolocationAsync(string, UpdateGeolocationQRCodeRequest, CancellationToken)` - Update geolocation QR code

#### CRUD Methods
- `GetAsync(string, CancellationToken)` - Get QR code by ID
- `DeleteAsync(string, CancellationToken)` - Delete QR code
- `ListAsync(ListQRCodesParams?, PaginationParams?, CancellationToken)` - List QR codes with filters

## Dependencies

- **Posty5.Core** - Core HTTP client and models

## License

MIT License

## Support

For issues, questions, or contributions, please visit:
- Website: https://posty5.com
- GitHub: https://github.com/posty5/dotnet-sdk
