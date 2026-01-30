using Posty5.Core.Converts;
using System.Text.Json.Serialization;

namespace Posty5.QRCode.Models;

/// <summary>
/// QR Code page information for landing page customization
/// </summary>
public class QRCodePageInfoModel
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

public class QRCodeFreeTextTargetModel
{
       /// <summary>
    /// Free text content
    /// </summary>
    public string? Text { get; set; }
}
/// <summary>
/// Email QR code target configuration
/// </summary>
public class QRCodeEmailTargetModel
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
public class QRCodeWifiTargetModel
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
public class QRCodeCallTargetModel
{
    /// <summary>
    /// Phone number
    /// </summary>
    public string? PhoneNumber { get; set; }
}

/// <summary>
/// SMS QR code target configuration
/// </summary>
public class QRCodeSmsTargetModel
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
public class QRCodeUrlTargetModel
{
    /// <summary>
    /// Target URL
    /// </summary>
    public string? Url { get; set; }
}

/// <summary>
/// Geolocation QR code target configuration
/// </summary>
public class QRCodeGeolocationTargetModel
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
public class QRCodeTargetModel
{
    /// <summary>
    /// Target type (email, wifi, call, sms, url, geolocation, freeText)
    /// </summary>
    public QRCodeTargetType  Type { get; set; } = QRCodeTargetType.FreeText;

    /// <summary>
    /// Free Text configuration (when type is 'freeText')
    /// </summary>
    public QRCodeFreeTextTargetModel? FreeText { get; set; }

    /// <summary>
    /// Email configuration (when type is 'email')
    /// </summary>
    public QRCodeEmailTargetModel? Email { get; set; }
    
    /// <summary>
    /// WiFi configuration (when type is 'wifi')
    /// </summary>
    public QRCodeWifiTargetModel? Wifi { get; set; }
    
    /// <summary>
    /// Call configuration (when type is 'call')
    /// </summary>
    public QRCodeCallTargetModel? Call { get; set; }
    
    /// <summary>
    /// SMS configuration (when type is 'sms')
    /// </summary>
    public QRCodeSmsTargetModel? Sms { get; set; }
    
    /// <summary>
    /// URL configuration (when type is 'url')
    /// </summary>
    public QRCodeUrlTargetModel? Url { get; set; }
    
    /// <summary>
    /// Geolocation configuration (when type is 'geolocation')
    /// </summary>
    public QRCodeGeolocationTargetModel? Geolocation { get; set; }
    
    /// <summary>
    /// Free text content (when type is 'freeText')
    /// </summary>
    public string? Text { get; set; }
}

/// <summary>
/// Preview reason (moderation score)
/// </summary>
public class QRCodePreviewReasonModel
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
    public QRCodeStatusType Status { get; set; }
    
    /// <summary>
    /// QR code landing page URL
    /// </summary>
    public string? QrCodeLandingPageURL { get; set; }
    public string? QrCodeDownloadURL { get; set; }

    
    /// <summary>
    /// Page information for landing page customization
    /// </summary>
    public QRCodePageInfoModel? PageInfo { get; set; }
    
    /// <summary>
    /// QR code target configuration
    /// </summary>
    public QRCodeTargetModel? QrCodeTarget { get; set; }
    
    /// <summary>
    /// Preview reasons (moderation scores)
    /// </summary>
    public List<QRCodePreviewReasonModel>? PreviewReasons { get; set; }
    
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
public class QRCodeRequestBaseModel
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
    public QRCodePageInfoModel? PageInfo { get; set; }
}

/// <summary>
/// Create free text QR code request
/// </summary>
public class QRCodeCreateFreeTextRequestModel : QRCodeRequestBaseModel
{
    /// <summary>
    /// Free text content
    /// </summary>
    public string Text { get; set; } = string.Empty;
}

/// <summary>
/// Create email QR code request
/// </summary>
public class QRCodeCreateEmailRequestModel : QRCodeRequestBaseModel
{
    /// <summary>
    /// Email configuration
    /// </summary>
    public QRCodeEmailTargetModel Email { get; set; } = new();
}

/// <summary>
/// Create WiFi QR code request
/// </summary>
public class QRCodeCreateWifiRequestModel : QRCodeRequestBaseModel
{
    /// <summary>
    /// WiFi configuration
    /// </summary>
    public QRCodeWifiTargetModel Wifi { get; set; } = new();
}

/// <summary>
/// Create phone call QR code request
/// </summary>
public class QRCodeCreateCallRequestModel : QRCodeRequestBaseModel
{
    /// <summary>
    /// Call configuration
    /// </summary>
    public QRCodeCallTargetModel Call { get; set; } = new();
}

/// <summary>
/// Create SMS QR code request
/// </summary>
public class QRCodeCreateSMSRequestModel : QRCodeRequestBaseModel
{
    /// <summary>
    /// SMS configuration
    /// </summary>
    public QRCodeSmsTargetModel Sms { get; set; } = new();
}

/// <summary>
/// Create URL QR code request
/// </summary>
public class QRCodeCreateURLRequestModel : QRCodeRequestBaseModel
{
    /// <summary>
    /// URL configuration
    /// </summary>
    public QRCodeUrlTargetModel Url { get; set; } = new();
}

/// <summary>
/// Create geolocation QR code request
/// </summary>
public class QRCodeCreateGeolocationRequestModel : QRCodeRequestBaseModel
{
    /// <summary>
    /// Geolocation configuration
    /// </summary>
    public QRCodeGeolocationTargetModel Geolocation { get; set; } = new();
}

/// <summary>
/// Update free text QR code request
/// </summary>
public class QRCodeUpdateFreeTextRequestModel : QRCodeRequestBaseModel
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
public class QRCodeUpdateEmailRequestModel : QRCodeRequestBaseModel
{
    /// <summary>
    /// QR code name (required for updates)
    /// </summary>
    public new string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Email configuration
    /// </summary>
    public QRCodeEmailTargetModel Email { get; set; } = new();
}

/// <summary>
/// Update WiFi QR code request
/// </summary>
public class QRCodeUpdateWifiRequestModel : QRCodeRequestBaseModel
{
    /// <summary>
    /// QR code name (required for updates)
    /// </summary>
    public new string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// WiFi configuration
    /// </summary>
    public QRCodeWifiTargetModel Wifi { get; set; } = new();
}

/// <summary>
/// Update phone call QR code request
/// </summary>
public class QRCodeUpdateCallRequestModel : QRCodeRequestBaseModel
{
    /// <summary>
    /// QR code name (required for updates)
    /// </summary>
    public new string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Call configuration
    /// </summary>
    public QRCodeCallTargetModel Call { get; set; } = new();
}

/// <summary>
/// Update SMS QR code request
/// </summary>
public class QRCodeUpdateSMSRequestModel : QRCodeRequestBaseModel
{
    /// <summary>
    /// QR code name (required for updates)
    /// </summary>
    public new string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// SMS configuration
    /// </summary>
    public QRCodeSmsTargetModel Sms { get; set; } = new();
}

/// <summary>
/// Update URL QR code request
/// </summary>
public class QRCodeUpdateURLRequestModel : QRCodeRequestBaseModel
{
    /// <summary>
    /// QR code name (required for updates)
    /// </summary>
    public new string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// URL configuration
    /// </summary>
    public QRCodeUrlTargetModel Url { get; set; } = new();
}

/// <summary>
/// Update geolocation QR code request
/// </summary>
public class QRCodeUpdateGeolocationRequestModel : QRCodeRequestBaseModel
{
    /// <summary>
    /// QR code name (required for updates)
    /// </summary>
    public new string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Geolocation configuration
    /// </summary>
    public QRCodeGeolocationTargetModel Geolocation { get; set; } = new();
}

/// <summary>
/// List parameters for searching QR codes
/// </summary>
public class QRCodeListParamsModel
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
    public QRCodeStatusType? Status { get; set; }
    
    /// <summary>
    /// Filter by created from source
    /// </summary>
    public string? CreatedFrom { get; set; }
}




[JsonConverter(typeof(StringValueObjectConverter<QRCodeTargetType>))]
public readonly record struct QRCodeTargetType (string Value)
{
    public static readonly QRCodeTargetType FreeText = new("freeText");
    public static readonly QRCodeTargetType Email = new("email");
    public static readonly QRCodeTargetType Wifi = new("wifi");
    public static readonly QRCodeTargetType Call = new("call");
    public static readonly QRCodeTargetType Sms = new("sms");
    public static readonly QRCodeTargetType Url = new("url");
    public static readonly QRCodeTargetType Geolocation = new("geolocation");

    public override string ToString ( ) => Value;
}




[JsonConverter(typeof(StringValueObjectConverter<QRCodeStatusType>))]
public readonly record struct QRCodeStatusType (string Value)
{
    public static readonly QRCodeStatusType New = new("new");
    public static readonly QRCodeStatusType Pending = new("pending");
    public static readonly QRCodeStatusType Rejected = new("rejected");
    public static readonly QRCodeStatusType Approved = new("approved");

    public override string ToString ( ) => Value;
}