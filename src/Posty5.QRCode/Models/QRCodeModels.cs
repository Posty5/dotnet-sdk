using Posty5.Core.Converts;
using Posty5.Core.Models;
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

    /// <summary>vCard configuration (when type is 'vcard')</summary>
    [JsonPropertyName("vcard")] public QRCodeVCardTargetModel? VCard { get; set; }

    /// <summary>Event configuration (when type is 'event')</summary>
    public QRCodeEventTargetModel? Event { get; set; }

    /// <summary>WhatsApp configuration (when type is 'whatsapp')</summary>
    [JsonPropertyName("whatsapp")] public QRCodeWhatsAppTargetModel? WhatsApp { get; set; }

    /// <summary>Review configuration (when type is 'review')</summary>
    public QRCodeReviewTargetModel? Review { get; set; }

    /// <summary>Social profiles configuration (when type is 'social')</summary>
    public QRCodeSocialTargetModel? Social { get; set; }

    /// <summary>App store configuration (when type is 'appStore')</summary>
    public QRCodeAppStoreTargetModel? AppStore { get; set; }

    /// <summary>File configuration (when type is 'file')</summary>
    public QRCodeFileTargetModel? File { get; set; }
    
    /// <summary>
    /// Free text content (when type is 'freeText')
    /// </summary>
    public string? Text { get; set; }
}
 
/// <summary>
/// QR Code template information
/// </summary>
public class QRCodeTemplateModel : Posty5.Core.Models.IVersioned
{
    /// <summary>The document's version (<c>__v</c>); pass it to the next update or delete.</summary>
    [JsonPropertyName("__v")]
    public long Version { get; set; }

    /// <summary>
    /// Template ID
    /// </summary>
    [JsonPropertyName("_id")]
    public string? Id { get; set; }
    
    /// <summary>
    /// Template name
    /// </summary>
    public string? Name { get; set; }
    
    /// <summary>
    /// Number of QR codes using this template
    /// </summary>
    public int? NumberOfSubQrCodes { get; set; }
    
    /// <summary>
    /// Number of short links using this template
    /// </summary>
    public int? NumberOfSubShortLinks { get; set; }
    
    /// <summary>
    /// QR code download URL
    /// </summary>
    public string? QrCodeDownloadURL { get; set; }

    /// <summary>Creation time.</summary>
    public DateTime? CreatedAt { get; set; }

    /// <summary>Last update time.</summary>
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// A QR design: the legacy EasyQRCode fields plus the v2 design fields
/// (shapes, gradient fill, frame, logo source). Any v2 field makes the design
/// v2 (<see cref="DesignVersion"/> 2, set by the API); legacy designs keep
/// rendering exactly as before.
/// </summary>
/// <remarks>
/// <para>Shape, frame, fill-type and logo-source values are plain strings with
/// the known values in <see cref="QrDotShapes"/>, <see cref="QrEyeFrameShapes"/>,
/// <see cref="QrEyeBallShapes"/>, <see cref="QrFillTypes"/>, <see cref="QrFrameIds"/>
/// and <see cref="QrLogoSources"/>, so a value added by a newer API reads back
/// unchanged instead of failing.</para>
/// <para>Keys this class does not name are kept in <see cref="AdditionalProperties"/>
/// and sent back on update, so a design round-trips intact.</para>
/// <para>Updating a v2 design without <see cref="DesignVersion"/> keeps its stored
/// v2 fields; send <c>DesignVersion = 1</c> to remove them.</para>
/// </remarks>
public class QRCodeOptionsModel
{
    /// <summary>The content the image encodes. Written by the API; a value sent by a client is ignored.</summary>
    public string? Text { get; set; }
    /// <summary>Stored width in px (10-600). The render size is a separate render parameter.</summary>
    public int? Width { get; set; }
    /// <summary>Stored height in px.</summary>
    public int? Height { get; set; }
    /// <summary>Error correction, EasyQRCode numbering: L = 1, M = 0, Q = 3, H = 2.</summary>
    public int? CorrectLevel { get; set; }
    /// <summary>Data module scale, 0-1.</summary>
    public double? DotScale { get; set; }
    /// <summary>Horizontal timing pattern scale.</summary>
    public double? DotScaleTiming_H { get; set; }
    /// <summary>Vertical timing pattern scale.</summary>
    public double? DotScaleTiming_V { get; set; }
    /// <summary>Alignment pattern outer scale.</summary>
    public double? DotScaleAO { get; set; }
    /// <summary>Alignment pattern inner scale.</summary>
    public double? DotScaleAI { get; set; }
    /// <summary>Quiet zone (margin) in px.</summary>
    public int? QuietZone { get; set; }
    /// <summary>Quiet zone colour.</summary>
    public string? QuietZoneColor { get; set; }
    /// <summary>Foreground colour (v2: used when <see cref="Fill"/> is absent).</summary>
    public string? ColorDark { get; set; }
    /// <summary>Background colour; ignored when <see cref="BackgroundTransparent"/> is true.</summary>
    public string? ColorLight { get; set; }
    /// <summary>Top-left eye frame colour.</summary>
    [JsonPropertyName("PO_TL")] public string? PO_TL { get; set; }
    /// <summary>Top-right eye frame colour.</summary>
    [JsonPropertyName("PO_TR")] public string? PO_TR { get; set; }
    /// <summary>Bottom-left eye frame colour.</summary>
    [JsonPropertyName("PO_BL")] public string? PO_BL { get; set; }
    /// <summary>Top-left eye ball colour.</summary>
    [JsonPropertyName("PI_TL")] public string? PI_TL { get; set; }
    /// <summary>Top-right eye ball colour.</summary>
    [JsonPropertyName("PI_TR")] public string? PI_TR { get; set; }
    /// <summary>Bottom-left eye ball colour.</summary>
    [JsonPropertyName("PI_BL")] public string? PI_BL { get; set; }
    /// <summary>Alignment pattern inner colour.</summary>
    [JsonPropertyName("AI")] public string? AI { get; set; }
    /// <summary>Alignment pattern outer colour.</summary>
    [JsonPropertyName("AO")] public string? AO { get; set; }
    /// <summary>Vertical timing pattern colour.</summary>
    [JsonPropertyName("timing_V")] public string? Timing_V { get; set; }
    /// <summary>Horizontal timing pattern colour.</summary>
    [JsonPropertyName("timing_H")] public string? Timing_H { get; set; }
    /// <summary>Legacy title strip text. A v2 design may not carry both a title and a <see cref="Frame"/>.</summary>
    public string? Title { get; set; }
    /// <summary>Legacy title font.</summary>
    public string? TitleFont { get; set; }
    /// <summary>Legacy title colour.</summary>
    public string? TitleColor { get; set; }
    /// <summary>Legacy title background colour.</summary>
    public string? TitleBackgroundColor { get; set; }
    /// <summary>Legacy title strip height.</summary>
    public int? TitleHeight { get; set; }
    /// <summary>Legacy title offset.</summary>
    public int? TitleTop { get; set; }
    /// <summary>
    /// Logo URL: a library logo, your own upload (<see cref="QRCodeTemplateClient.UploadLogoAsync"/>)
    /// or any image URL, which the API re-hosts on the Posty5 CDN when the design is saved.
    /// </summary>
    public string? Logo { get; set; }
    /// <summary>Logo width.</summary>
    public int? LogoWidth { get; set; }
    /// <summary>Logo height.</summary>
    public int? LogoHeight { get; set; }
    /// <summary>Logo background colour.</summary>
    public string? LogoBackgroundColor { get; set; }
    /// <summary>Whether the logo background is transparent.</summary>
    public bool? LogoBackgroundTransparent { get; set; }

    /// <summary>A library logo stored by the template editor; renderers read <c>Logo ?? Posty5Logo</c>.</summary>
    public string? Posty5Logo { get; set; }

    /// <summary>1 (legacy engine) or 2 (Posty5 design engine). Absent reads as 1; the API sets 2 when any v2 field is present.</summary>
    public int? DesignVersion { get; set; }
    /// <summary>Data module shape (v2), see <see cref="QrDotShapes"/>.</summary>
    public string? DotShape { get; set; }
    /// <summary>Eye (position pattern) outer ring shape (v2), see <see cref="QrEyeFrameShapes"/>.</summary>
    public string? EyeFrameShape { get; set; }
    /// <summary>Eye centre shape (v2), see <see cref="QrEyeBallShapes"/>.</summary>
    public string? EyeBallShape { get; set; }
    /// <summary>Solid or gradient fill of the data modules (v2). Eyes keep their <c>PO_*</c>/<c>PI_*</c> colours and fall back to the first stop.</summary>
    public QrFillModel? Fill { get; set; }
    /// <summary>Transparent background (v2); <see cref="ColorLight"/> is ignored when true.</summary>
    public bool? BackgroundTransparent { get; set; }
    /// <summary>A frame with a call to action (v2).</summary>
    public QrFrameModel? Frame { get; set; }
    /// <summary>How <see cref="Logo"/> was chosen (v2), see <see cref="QrLogoSources"/>.</summary>
    public string? LogoSource { get; set; }
    /// <summary>Clear the modules under the logo (v2; the API's default is true).</summary>
    public bool? LogoExcavate { get; set; }

    /// <summary>Design keys this class does not name, kept so a design read from the API is sent back intact.</summary>
    [JsonExtensionData]
    public Dictionary<string, System.Text.Json.JsonElement>? AdditionalProperties { get; set; }
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
public class QRCodeModel : Posty5.Core.Models.IVersioned
{
    /// <summary>The document's version (<c>__v</c>); pass it to the next update or delete.</summary>
    [JsonPropertyName("__v")]
    public long Version { get; set; }

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
    /// Number of visits to the code's Posty5 page (<see cref="QrCodeLandingPageURL"/>).
    /// A downloaded QR image encodes its content directly, so scanning it is
    /// not counted here.
    /// </summary>
    public int? NumberOfVisitors { get; set; }


    /// <summary>
    /// Whether the code's Posty5 page shows <see cref="PageInfo"/>. Also
    /// returned in API-key list results from the API's link-qr truth pass on.
    /// </summary>
    public bool? IsEnableLandingPage { get; set; }
    
    /// <summary>
    /// Template name
    /// </summary>
    public string? TemplateName { get; set; }
 
    /// <summary>
    /// Last visitor date
    /// </summary>
    public string? LastVisitorDate { get; set; }
    
    /// <summary>
    /// Never returned by the API; always <c>null</c>.
    /// </summary>
    [Obsolete(QRCodeConst.MonetizationObsolete)]
    [JsonIgnore]
    public bool? IsEnableMonetization { get; set; }

    /// <summary>
    /// QR code status (new, pending, approved, rejected). Also returned in
    /// API-key list results from the API's link-qr truth pass on.
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
    /// <c>static</c>: the image encodes the content itself. <c>dynamic</c>: the image encodes
    /// <see cref="QrCodeLandingPageURL"/>, which redirects to <see cref="QrCodeTarget"/>, so the
    /// target can change later without reprinting. Codes stored before dynamic QR codes existed
    /// come back as <c>static</c>.
    /// </summary>
    public QRCodeMode? Mode { get; set; }

    /// <summary>When the code last became dynamic; <c>null</c> for a static code.</summary>
    public DateTime? DynamicSince { get; set; }

    /// <summary>The code's scan rules; <c>null</c> when it has none.</summary>
    public QRCodeAccessModel? Access { get; set; }
    
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
/// QR Code full details model with all populated fields
/// </summary>
public class QRCodeFullDetailsModel : QRCodeModel
{
      
    /// <summary>
    /// Template object (populated)
    /// </summary>
    public QRCodeTemplateModel? Template { get; set; }
    
    /// <summary>
    /// Template type
    /// </summary>
    public string? TemplateType { get; set; }
     
 
    /// <summary>
    /// QR code styling options
    /// </summary>
    public QRCodeOptionsModel? Options { get; set; }
    
    /// <summary>
    /// Number of likes
    /// </summary>
    public int? NumberOfLikes { get; set; }
    
    /// <summary>
    /// Number of comments
    /// </summary>
    public int? NumberOfComments { get; set; }
    
    /// <summary>
    /// Source platform where QR code was created
    /// </summary>
    public string? CreatedFrom { get; set; }
    
       
   
    
 
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
    /// QR code template ID. Required on create and update for API-key callers,
    /// which every SDK call is: the API answers "Template Id Is Required"
    /// without it. Your template IDs are on the dashboard's QR code templates page.
    /// </summary>
    public required string TemplateId { get; set; }
    
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
    /// Whether the code's Posty5 page shows <see cref="PageInfo"/>. Omitted
    /// (<c>null</c>): the API stores <c>false</c> on create and keeps the stored
    /// value on update.
    /// </summary>
    public bool? IsEnableLandingPage { get; set; }

    /// <summary>
    /// Page information. <see cref="QRCodePageInfoModel.Title"/> is required
    /// when <see cref="IsEnableLandingPage"/> is <c>true</c>.
    /// </summary>
    public QRCodePageInfoModel? PageInfo { get; set; }

    /// <summary>
    /// Never accepted by the API; ignored and never sent.
    /// </summary>
    [Obsolete(QRCodeConst.MonetizationObsolete)]
    [JsonIgnore]
    public bool? IsEnableMonetization { get; set; }

    /// <summary>
    /// <see cref="QRCodeMode.Static"/> or <see cref="QRCodeMode.Dynamic"/>. Omitted (<c>null</c>):
    /// the API creates a static code, and an update keeps the stored mode. Wi-Fi codes cannot be
    /// dynamic: the Wi-Fi methods throw <see cref="ArgumentException"/> before any call.
    /// </summary>
    public QRCodeMode? Mode { get; set; }

    /// <summary>
    /// Scan rules (dynamic codes only, Starter plan and above). Omitted (<c>null</c>): an update
    /// keeps the stored rules. A sent object replaces the stored rules as a whole, so a property
    /// left <c>null</c> in it is cleared. The API answers 400 for a static code and 403 below Starter.
    /// </summary>
    public QRCodeAccessModel? Access { get; set; }

    /// <summary>
    /// <c>true</c> sends <c>access: null</c>, removing every scan rule. Needed because a
    /// <c>null</c> <see cref="Access"/> means "keep". Takes precedence over <see cref="Access"/>.
    /// </summary>
    [JsonIgnore]
    public bool ClearAccess { get; set; }
}

/// <summary>
/// Scan rules of a dynamic QR code. Every property is optional; with none set the code is never gated.
/// </summary>
public class QRCodeAccessModel
{
    /// <summary>Scans before this moment (UTC) go to <see cref="FallbackUrl"/> or are refused.</summary>
    public DateTime? ActiveFrom { get; set; }

    /// <summary>Scans from this moment (UTC) go to <see cref="FallbackUrl"/> or are refused. Must be after <see cref="ActiveFrom"/>.</summary>
    public DateTime? ExpiresAt { get; set; }

    /// <summary>Number of scans allowed (1 or more); later scans go to <see cref="FallbackUrl"/> or are refused.</summary>
    public int? MaxVisits { get; set; }

    /// <summary>An http(s) URL (up to 2048 characters) that gated scans are sent to.</summary>
    public string? FallbackUrl { get; set; }
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
    /// No such filter exists; ignored and never sent.
    /// </summary>
    [Obsolete(QRCodeConst.MonetizationObsolete)]
    [JsonIgnore]
    public bool? IsEnableMonetization { get; set; }
    
    /// <summary>
    /// Filter by status (new, pending, approved, rejected)
    /// </summary>
    public QRCodeStatusType? Status { get; set; }

    /// <summary>Filter by mode (static or dynamic)</summary>
    public QRCodeMode? Mode { get; set; }
    
    /// <summary>
    /// Filter by created from source
    /// </summary>
    public string? CreatedFrom { get; set; }

    /// <summary>
    /// Created on or after this day, sent as <c>fromDate</c> (<c>yyyy-MM-dd</c>,
    /// date part only, invariant culture). Applied only together with
    /// <see cref="ToDate"/>: the API ignores a lone bound, and the SDK does not
    /// check the pair. Matched on the creation date, in whole UTC days.
    /// </summary>
    public DateTime? FromDate { get; set; }

    /// <summary>
    /// Created on or before this day, sent as <c>toDate</c> (<c>yyyy-MM-dd</c>).
    /// Applied only together with <see cref="FromDate"/>; the API rounds it to
    /// the end of its UTC day.
    /// </summary>
    public DateTime? ToDate { get; set; }
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
    public static readonly QRCodeTargetType VCard = new("vcard");
    public static readonly QRCodeTargetType Event = new("event");
    public static readonly QRCodeTargetType WhatsApp = new("whatsapp");
    public static readonly QRCodeTargetType Review = new("review");
    public static readonly QRCodeTargetType Social = new("social");
    public static readonly QRCodeTargetType AppStore = new("appStore");
    public static readonly QRCodeTargetType File = new("file");

    public override string ToString ( ) => Value;
}




/// <summary>
/// QR code mode. Serialised as <c>"static"</c> / <c>"dynamic"</c>; an unknown value from a newer
/// API reads back as-is instead of failing.
/// </summary>
[JsonConverter(typeof(QRCodeModeConverter))]
public readonly record struct QRCodeMode (string Value)
{
    /// <summary>The image encodes the content itself (the default).</summary>
    public static readonly QRCodeMode Static = new("static");
    /// <summary>The image encodes a Posty5 link that redirects to the target.</summary>
    public static readonly QRCodeMode Dynamic = new("dynamic");

    /// <summary>The wire value, <c>"static"</c> or <c>"dynamic"</c>.</summary>
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

/// <summary>
/// Delete response model
/// </summary>
public class DeleteResponse
{
    /// <summary>
    /// Success message
    /// </summary>
    public string Message { get; set; } = string.Empty;
}

/// <summary>
/// Account-wide QR-code statistics (<c>GET /api/qr-code/statistics</c>):
/// the resolved <see cref="LinkStatisticsResponse{TData}.Range"/> plus
/// <see cref="QRCodeStatisticsDataModel"/>.
/// </summary>
public class QRCodeStatisticsModel : LinkStatisticsResponse<QRCodeStatisticsDataModel>
{
}

/// <summary>The <c>data</c> of <see cref="QRCodeStatisticsModel"/>.</summary>
public class QRCodeStatisticsDataModel
{
    /// <summary>Lifetime QR code count and counters, plus the visit totals in the range.</summary>
    public QRCodeStatisticsTotalsModel Totals { get; set; } = new();

    /// <summary>One row per UTC day that had a QR code created or a visit to a code's Posty5 page, oldest first.</summary>
    public List<LinkStatisticsDailyRow> Daily { get; set; } = new();

    /// <summary>The ten QR codes with the most visits in the range, most first; codes with no visit in the range are left out.</summary>
    public List<QRCodeStatisticsTopQRCodeModel> TopQRCodes { get; set; } = new();
}

/// <summary>Totals of <see cref="QRCodeStatisticsDataModel"/>.</summary>
public class QRCodeStatisticsTotalsModel : LinkStatisticsVisitTotals
{
    /// <summary>Your QR codes (lifetime, deleted ones excluded).</summary>
    public long TotalQRCodes { get; set; }

    /// <summary><see cref="LinkStatisticsVisitTotals.TotalVisitors"/> / <see cref="TotalQRCodes"/> (0 with no links).</summary>
    public double AvgVisitorsPerQRCode { get; set; }
}

/// <summary>One of <see cref="QRCodeStatisticsDataModel.TopQRCodes"/>.</summary>
public class QRCodeStatisticsTopQRCodeModel
{
    /// <summary>Database ID</summary>
    [JsonPropertyName("_id")]
    public string? Id { get; set; }

    /// <summary>QR code name</summary>
    public string? Name { get; set; }

    /// <summary>Lifetime visit counter</summary>
    public long? NumberOfVisitors { get; set; }

    /// <summary>Creation time</summary>
    public DateTime? CreatedAt { get; set; }

    /// <summary>Visits by people in the range (bots excluded).</summary>
    public long VisitsInRange { get; set; }
}

/// <summary>
/// Reads and writes <see cref="QRCodeMode"/> as its string value. Unlike the shared
/// <c>StringValueObjectConverter</c>, an unknown value is kept as-is instead of failing, so a
/// mode added by a newer API does not break deserialising a whole response.
/// </summary>
public sealed class QRCodeModeConverter : JsonConverter<QRCodeMode>
{
    /// <inheritdoc />
    public override QRCodeMode Read(ref System.Text.Json.Utf8JsonReader reader, Type typeToConvert, System.Text.Json.JsonSerializerOptions options)
    {
        var value = reader.GetString() ?? string.Empty;
        if (string.Equals(value, QRCodeMode.Static.Value, StringComparison.OrdinalIgnoreCase)) return QRCodeMode.Static;
        if (string.Equals(value, QRCodeMode.Dynamic.Value, StringComparison.OrdinalIgnoreCase)) return QRCodeMode.Dynamic;
        return new QRCodeMode(value);
    }

    /// <inheritdoc />
    public override void Write(System.Text.Json.Utf8JsonWriter writer, QRCodeMode value, System.Text.Json.JsonSerializerOptions options)
        => writer.WriteStringValue(value.Value);
}
