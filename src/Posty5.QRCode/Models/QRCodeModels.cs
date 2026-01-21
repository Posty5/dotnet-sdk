namespace Posty5.QRCode.Models;

/// <summary>
/// QR code type
/// </summary>
public enum QRCodeType
{
    FreeText,
    Email,
    Wifi,
    Call,
    SMS,
    URL,
    Geolocation
}

/// <summary>
/// Base QR code model
/// </summary>
public class QRCodeModel
{
    public string? Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? TemplateId { get; set; }
    public string? QrCodeLandingPage { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// Free text QR code target
/// </summary>
public class FreeTextQRTarget
{
    public string Text { get; set; } = string.Empty;
}

/// <summary>
/// Email QR code target
/// </summary>
public class EmailQRTarget
{
    public string Email { get; set; } = string.Empty;
    public string? Subject { get; set; }
    public string? Body { get; set; }
}

/// <summary>
/// WiFi QR code target
/// </summary>
public class WifiQRTarget
{
    public string Ssid { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string SecurityType { get; set; } = "WPA"; // WPA, WEP, or nopass
    public bool Hidden { get; set; }
}

/// <summary>
/// Phone call QR code target
/// </summary>
public class CallQRTarget
{
    public string PhoneNumber { get; set; } = string.Empty;
}

/// <summary>
/// SMS QR code target
/// </summary>
public class SmsQRTarget
{
    public string PhoneNumber { get; set; } = string.Empty;
    public string? Message { get; set; }
}

/// <summary>
/// URL QR code target
/// </summary>
public class UrlQRTarget
{
    public string Url { get; set; } = string.Empty;
}

/// <summary>
/// Geolocation QR code target
/// </summary>
public class GeolocationQRTarget
{
    public double Latitude { get; set; }
    public double Longitude { get; set; }
}

/// <summary>
/// Create free text QR code request
/// </summary>
public class CreateFreeTextQRCodeRequest
{
    public string Name { get; set; } = string.Empty;
    public string? TemplateId { get; set; }
    public FreeTextQRTarget QrCodeTarget { get; set; } = new();
}

/// <summary>
/// Create email QR code request
/// </summary>
public class CreateEmailQRCodeRequest
{
    public string Name { get; set; } = string.Empty;
    public string? TemplateId { get; set; }
    public EmailQRTarget QrCodeTarget { get; set; } = new();
}

/// <summary>
/// Create WiFi QR code request
/// </summary>
public class CreateWifiQRCodeRequest
{
    public string Name { get; set; } = string.Empty;
    public string? TemplateId { get; set; }
    public WifiQRTarget QrCodeTarget { get; set; } = new();
}

/// <summary>
/// Create phone call QR code request
/// </summary>
public class CreateCallQRCodeRequest
{
    public string Name { get; set; } = string.Empty;
    public string? TemplateId { get; set; }
    public CallQRTarget QrCodeTarget { get; set; } = new();
}

/// <summary>
/// Create SMS QR code request
/// </summary>
public class CreateSmsQRCodeRequest
{
    public string Name { get; set; } = string.Empty;
    public string? TemplateId { get; set; }
    public SmsQRTarget QrCodeTarget { get; set; } = new();
}

/// <summary>
/// Create URL QR code request
/// </summary>
public class CreateUrlQRCodeRequest
{
    public string Name { get; set; } = string.Empty;
    public string? TemplateId { get; set; }
    public UrlQRTarget QrCodeTarget { get; set; } = new();
}

/// <summary>
/// Create geolocation QR code request
/// </summary>
public class CreateGeolocationQRCodeRequest
{
    public string Name { get; set; } = string.Empty;
    public string? TemplateId { get; set; }
    public GeolocationQRTarget QrCodeTarget { get; set; } = new();
}

/// <summary>
/// List parameters for QR codes
/// </summary>
public class ListQRCodesParams
{
    public string? Search { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
}
