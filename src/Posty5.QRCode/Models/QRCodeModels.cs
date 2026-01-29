using System.Text.Json.Serialization;

namespace Posty5.QRCode.Models;

/// <summary>
/// QR Code page information for landing page customization
/// </summary>
public class QRCodePageInfo
{
    /// <summary>
    /// Page title
    /// </summary>
    public string? Title { get; set; }
    
    /// <summary>
    /// Page description
    /// </summary>
    public string? Description { get; set; }
    
}

public class QRCodeFreeTextTarget
{
       /// <summary>
    /// Free text content
    /// </summary>
    public string? Text { get; set; }
}
/// <summary>
/// Email QR code target configuration
/// </summary>
public class QRCodeEmailTarget
{
    /// <summary>
    /// Email address
    /// </summary>
    public string? Email { get; set; }
    
    /// <summary>
    /// Email subject
    /// </summary>
    public string? Subject { get; set; }
    
    /// <summary>
    /// Email body
    /// </summary>
    public string? Body { get; set; }
}

/// <summary>
/// WiFi QR code target configuration
/// </summary>
public class QRCodeWifiTarget
{
    /// <summary>
    /// WiFi network name (SSID)
    /// </summary>
    public string? Name { get; set; }
    
    /// <summary>
    /// Authentication type (WPA, WEP, etc.)
    /// </summary>
    public string? AuthenticationType { get; set; }
    
    /// <summary>
    /// WiFi password
    /// </summary>
    public string? Password { get; set; }
}

/// <summary>
/// Phone call QR code target configuration
/// </summary>
public class QRCodeCallTarget
{
    /// <summary>
    /// Phone number
    /// </summary>
    public string? PhoneNumber { get; set; }
}

/// <summary>
/// SMS QR code target configuration
/// </summary>
public class QRCodeSmsTarget
{
    /// <summary>
    /// Phone number
    /// </summary>
    public string? PhoneNumber { get; set; }
    
    /// <summary>
    /// SMS message
    /// </summary>
    public string? Message { get; set; }
}

/// <summary>
/// URL QR code target configuration
/// </summary>
public class QRCodeUrlTarget
{
    /// <summary>
    /// Target URL
    /// </summary>
    public string? Url { get; set; }
}

/// <summary>
/// Geolocation QR code target configuration
/// </summary>
public class QRCodeGeolocationTarget
{
    /// <summary>
    /// Latitude coordinate
    /// </summary>
    public string Latitude { get; set; } = string.Empty;
    
    /// <summary>
    /// Longitude coordinate
    /// </summary>
    public string Longitude { get; set; } = string.Empty;
}

/// <summary>
/// QR Code target configuration
/// </summary>
public class QRCodeTarget
{
    /// <summary>
    /// Target type (email, wifi, call, sms, url, geolocation, freeText)
    /// </summary>
    public QRCodeTargetType  Type { get; set; } = QRCodeTargetType.FreeText;

    /// <summary>
    /// Free Text configuration (when type is 'freeText')
    /// </summary>
    public QRCodeFreeTextTarget? FreeText { get; set; }

    /// <summary>
    /// Email configuration (when type is 'email')
    /// </summary>
    public QRCodeEmailTarget? Email { get; set; }
    
    /// <summary>
    /// WiFi configuration (when type is 'wifi')
    /// </summary>
    public QRCodeWifiTarget? Wifi { get; set; }
    
    /// <summary>
    /// Call configuration (when type is 'call')
    /// </summary>
    public QRCodeCallTarget? Call { get; set; }
    
    /// <summary>
    /// SMS configuration (when type is 'sms')
    /// </summary>
    public QRCodeSmsTarget? Sms { get; set; }
    
    /// <summary>
    /// URL configuration (when type is 'url')
    /// </summary>
    public QRCodeUrlTarget? Url { get; set; }
    
    /// <summary>
    /// Geolocation configuration (when type is 'geolocation')
    /// </summary>
    public QRCodeGeolocationTarget? Geolocation { get; set; }
    
    /// <summary>
    /// Free text content (when type is 'freeText')
    /// </summary>
    public string? Text { get; set; }
}

/// <summary>
/// Preview reason (moderation score)
/// </summary>
public class PreviewReason
{
    /// <summary>
    /// Category name
    /// </summary>
    public string Category { get; set; } = string.Empty;
    
    /// <summary>
    /// Score value
    /// </summary>
    public double Score { get; set; }
}

/// <summary>
/// QR Code model
/// </summary>
public class QRCodeModel
{
    /// <summary>
    /// MongoDB document ID
    /// </summary>
    [JsonPropertyName("_id")]
    public string? Id { get; set; }
    
    /// <summary>
    /// QR code unique identifier
    /// </summary>
    public string? QrCodeId { get; set; }
    
    /// <summary>
    /// QR code name
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Template ID used
    /// </summary>
    public string? TemplateId { get; set; }
    
    /// <summary>
    /// External reference ID for filtering/tracking
    /// </summary>
    public string? RefId { get; set; }
    
    /// <summary>
    /// Custom tag for filtering/categorization
    /// </summary>
    public string? Tag { get; set; }
    
    
    /// <summary>
    /// Number of visitors/scans
    /// </summary>
    public int? NumberOfVisitors { get; set; }
    
    /// <summary>
    /// Last visitor date
    /// </summary>
    public string? LastVisitorDate { get; set; }
    
    /// <summary>
    /// Whether monetization is enabled
    /// </summary>
    public bool? IsEnableMonetization { get; set; }
    
    /// <summary>
    /// QR code status (new, pending, approved, rejected)
    /// </summary>
    public string? Status { get; set; }
    
    /// <summary>
    /// QR code landing page URL
    /// </summary>
    public string? qrCodeLandingPageURL { get; set; }
    public string? QrCodeDownloadURL { get; set; }

    
    /// <summary>
    /// Page information for landing page customization
    /// </summary>
    public QRCodePageInfo? PageInfo { get; set; }
    
    /// <summary>
    /// QR code target configuration
    /// </summary>
    public QRCodeTarget? QrCodeTarget { get; set; }
    
    /// <summary>
    /// Preview reasons (moderation scores)
    /// </summary>
    public List<PreviewReason>? PreviewReasons { get; set; }
    
    /// <summary>
    /// Created timestamp
    /// </summary>
    public DateTime? CreatedAt { get; set; }
    
    /// <summary>
    /// Updated timestamp
    /// </summary>
    public DateTime? UpdatedAt { get; set; }


 
}

/// <summary>
/// Base request for QR code operations
/// </summary>
public class QRCodeRequestBase
{
    /// <summary>
    /// QR code name (optional)
    /// </summary>
    public string? Name { get; set; }
    
    /// <summary>
    /// Template ID
    /// </summary>
    public string TemplateId { get; set; } = string.Empty;
    
    /// <summary>
    /// External reference ID for filtering/tracking
    /// </summary>
    public string? RefId { get; set; }
    
    /// <summary>
    /// Custom tag for filtering/categorization
    /// </summary>
    public string? Tag { get; set; }
    
    /// <summary>
    /// Custom landing page ID (optional, max 32 chars)
    /// </summary>
    public string? CustomLandingId { get; set; }
    
    /// <summary>
    /// Enable monetization (default: false)
    /// </summary>
    public bool? IsEnableMonetization { get; set; }
    
    /// <summary>
    /// Page information (required when monetization is enabled)
    /// </summary>
    public QRCodePageInfo? PageInfo { get; set; }
}

/// <summary>
/// Create free text QR code request
/// </summary>
public class CreateFreeTextQRCodeRequest : QRCodeRequestBase
{
    /// <summary>
    /// Free text content
    /// </summary>
    public string Text { get; set; } = string.Empty;
}

/// <summary>
/// Create email QR code request
/// </summary>
public class CreateEmailQRCodeRequest : QRCodeRequestBase
{
    /// <summary>
    /// Email configuration
    /// </summary>
    public QRCodeEmailTarget Email { get; set; } = new();
}

/// <summary>
/// Create WiFi QR code request
/// </summary>
public class CreateWifiQRCodeRequest : QRCodeRequestBase
{
    /// <summary>
    /// WiFi configuration
    /// </summary>
    public QRCodeWifiTarget Wifi { get; set; } = new();
}

/// <summary>
/// Create phone call QR code request
/// </summary>
public class CreateCallQRCodeRequest : QRCodeRequestBase
{
    /// <summary>
    /// Call configuration
    /// </summary>
    public QRCodeCallTarget Call { get; set; } = new();
}

/// <summary>
/// Create SMS QR code request
/// </summary>
public class CreateSMSQRCodeRequest : QRCodeRequestBase
{
    /// <summary>
    /// SMS configuration
    /// </summary>
    public QRCodeSmsTarget Sms { get; set; } = new();
}

/// <summary>
/// Create URL QR code request
/// </summary>
public class CreateURLQRCodeRequest : QRCodeRequestBase
{
    /// <summary>
    /// URL configuration
    /// </summary>
    public QRCodeUrlTarget Url { get; set; } = new();
}

/// <summary>
/// Create geolocation QR code request
/// </summary>
public class CreateGeolocationQRCodeRequest : QRCodeRequestBase
{
    /// <summary>
    /// Geolocation configuration
    /// </summary>
    public QRCodeGeolocationTarget Geolocation { get; set; } = new();
}

/// <summary>
/// Update free text QR code request
/// </summary>
public class UpdateFreeTextQRCodeRequest : QRCodeRequestBase
{
    /// <summary>
    /// QR code name (required for updates)
    /// </summary>
    public new string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Free text content
    /// </summary>
    public string Text { get; set; } = string.Empty;
}

/// <summary>
/// Update email QR code request
/// </summary>
public class UpdateEmailQRCodeRequest : QRCodeRequestBase
{
    /// <summary>
    /// QR code name (required for updates)
    /// </summary>
    public new string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Email configuration
    /// </summary>
    public QRCodeEmailTarget Email { get; set; } = new();
}

/// <summary>
/// Update WiFi QR code request
/// </summary>
public class UpdateWifiQRCodeRequest : QRCodeRequestBase
{
    /// <summary>
    /// QR code name (required for updates)
    /// </summary>
    public new string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// WiFi configuration
    /// </summary>
    public QRCodeWifiTarget Wifi { get; set; } = new();
}

/// <summary>
/// Update phone call QR code request
/// </summary>
public class UpdateCallQRCodeRequest : QRCodeRequestBase
{
    /// <summary>
    /// QR code name (required for updates)
    /// </summary>
    public new string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Call configuration
    /// </summary>
    public QRCodeCallTarget Call { get; set; } = new();
}

/// <summary>
/// Update SMS QR code request
/// </summary>
public class UpdateSMSQRCodeRequest : QRCodeRequestBase
{
    /// <summary>
    /// QR code name (required for updates)
    /// </summary>
    public new string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// SMS configuration
    /// </summary>
    public QRCodeSmsTarget Sms { get; set; } = new();
}

/// <summary>
/// Update URL QR code request
/// </summary>
public class UpdateURLQRCodeRequest : QRCodeRequestBase
{
    /// <summary>
    /// QR code name (required for updates)
    /// </summary>
    public new string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// URL configuration
    /// </summary>
    public QRCodeUrlTarget Url { get; set; } = new();
}

/// <summary>
/// Update geolocation QR code request
/// </summary>
public class UpdateGeolocationQRCodeRequest : QRCodeRequestBase
{
    /// <summary>
    /// QR code name (required for updates)
    /// </summary>
    public new string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Geolocation configuration
    /// </summary>
    public QRCodeGeolocationTarget Geolocation { get; set; } = new();
}

/// <summary>
/// List parameters for searching QR codes
/// </summary>
public class ListQRCodesParams
{
    /// <summary>
    /// Filter by QR code name
    /// </summary>
    public string? Name { get; set; }
    
    /// <summary>
    /// Filter by QR code ID
    /// </summary>
    public string? QrCodeId { get; set; }
    
    /// <summary>
    /// Filter by template ID
    /// </summary>
    public string? TemplateId { get; set; }
    
    /// <summary>
    /// Filter by custom tag
    /// </summary>
    public string? Tag { get; set; }
    
    /// <summary>
    /// Filter by external reference ID
    /// </summary>
    public string? RefId { get; set; }
    
    /// <summary>
    /// Filter by monetization enabled
    /// </summary>
    public bool? IsEnableMonetization { get; set; }
    
    /// <summary>
    /// Filter by status (new, pending, approved, rejected)
    /// </summary>
    public string? Status { get; set; }
    
    /// <summary>
    /// Filter by created from source
    /// </summary>
    public string? CreatedFrom { get; set; }
}


public enum QRCodeTargetType
{
    FreeText=1,
    Email,
    Wifi,
    Call,
    Sms,
    Url,
    Geolocation
}