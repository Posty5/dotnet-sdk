using System.Text.Json.Serialization;

namespace Posty5.QRCode.Models;

/// <summary>vCard phone kinds (<c>TEL;TYPE=CELL</c> / <c>WORK,VOICE</c> / <c>HOME,VOICE</c>). Default <c>"mobile"</c>.</summary>
public static class QRCodeVCardPhoneKinds
{
    /// <summary>Mobile.</summary>
    public const string Mobile = "mobile";
    /// <summary>Work.</summary>
    public const string Work = "work";
    /// <summary>Home.</summary>
    public const string Home = "home";
}

/// <summary>Review platforms of a <c>review</c> code. Plain strings, so a newer server value still reads back.</summary>
public static class QRCodeReviewPlatforms
{
    /// <summary>Google.</summary>
    public const string Google = "google";
    /// <summary>Tripadvisor.</summary>
    public const string Tripadvisor = "tripadvisor";
    /// <summary>Trustpilot.</summary>
    public const string Trustpilot = "trustpilot";
    /// <summary>Yelp.</summary>
    public const string Yelp = "yelp";
    /// <summary>Facebook.</summary>
    public const string Facebook = "facebook";
    /// <summary>Other.</summary>
    public const string Other = "other";
}

/// <summary>Social profile platforms of a <c>social</c> code. Plain strings, so a newer server value still reads back.</summary>
public static class QRCodeSocialPlatforms
{
    /// <summary>Instagram.</summary>
    public const string Instagram = "instagram";
    /// <summary>Facebook.</summary>
    public const string Facebook = "facebook";
    /// <summary>TikTok.</summary>
    public const string TikTok = "tiktok";
    /// <summary>X.</summary>
    public const string X = "x";
    /// <summary>YouTube.</summary>
    public const string YouTube = "youtube";
    /// <summary>LinkedIn.</summary>
    public const string LinkedIn = "linkedin";
    /// <summary>Snapchat.</summary>
    public const string Snapchat = "snapchat";
    /// <summary>Telegram.</summary>
    public const string Telegram = "telegram";
    /// <summary>Threads.</summary>
    public const string Threads = "threads";
    /// <summary>Pinterest.</summary>
    public const string Pinterest = "pinterest";
    /// <summary>Other.</summary>
    public const string Other = "other";
}

/// <summary>One vCard phone.</summary>
public class QRCodeVCardPhoneModel
{
    /// <summary>Phone kind (<see cref="QRCodeVCardPhoneKinds"/>); the API defaults to <c>"mobile"</c>.</summary>
    [JsonPropertyName("kind")] public string? Kind { get; set; }
    /// <summary>Phone number (required).</summary>
    [JsonPropertyName("number")] public string Number { get; set; } = string.Empty;
}

/// <summary>A vCard's work address (<c>ADR;TYPE=WORK</c>).</summary>
public class QRCodeVCardAddressModel
{
    /// <summary>Street.</summary>
    [JsonPropertyName("street")] public string? Street { get; set; }
    /// <summary>City.</summary>
    [JsonPropertyName("city")] public string? City { get; set; }
    /// <summary>Region.</summary>
    [JsonPropertyName("region")] public string? Region { get; set; }
    /// <summary>PostalCode.</summary>
    [JsonPropertyName("postalCode")] public string? PostalCode { get; set; }
    /// <summary>Country.</summary>
    [JsonPropertyName("country")] public string? Country { get; set; }
}

/// <summary>vCard (contact card) target, encoded by the API as vCard 3.0. <c>FirstName</c> or <c>Organization</c> is required.</summary>
public class QRCodeVCardTargetModel
{
    /// <summary>FirstName.</summary>
    [JsonPropertyName("firstName")] public string? FirstName { get; set; }
    /// <summary>LastName.</summary>
    [JsonPropertyName("lastName")] public string? LastName { get; set; }
    /// <summary>Organization.</summary>
    [JsonPropertyName("organization")] public string? Organization { get; set; }
    /// <summary>JobTitle.</summary>
    [JsonPropertyName("jobTitle")] public string? JobTitle { get; set; }
    /// <summary>Phones.</summary>
    [JsonPropertyName("phones")] public List<QRCodeVCardPhoneModel>? Phones { get; set; }
    /// <summary>Emails.</summary>
    [JsonPropertyName("emails")] public List<string>? Emails { get; set; }
    /// <summary>http(s) URL.</summary>
    [JsonPropertyName("website")] public string? Website { get; set; }
    /// <summary>Address.</summary>
    [JsonPropertyName("address")] public QRCodeVCardAddressModel? Address { get; set; }
    /// <summary>Note.</summary>
    [JsonPropertyName("note")] public string? Note { get; set; }
}

/// <summary>Calendar event target, encoded by the API as a VEVENT. Times are sent as ISO 8601.</summary>
public class QRCodeEventTargetModel
{
    /// <summary>Event title (required).</summary>
    [JsonPropertyName("title")] public string Title { get; set; } = string.Empty;
    /// <summary>Location.</summary>
    [JsonPropertyName("location")] public string? Location { get; set; }
    /// <summary>Description.</summary>
    [JsonPropertyName("description")] public string? Description { get; set; }
    /// <summary>Start (required), serialized as ISO 8601.</summary>
    [JsonPropertyName("startsAt")] public DateTimeOffset StartsAt { get; set; }
    /// <summary>End; must be after <see cref="StartsAt"/>.</summary>
    [JsonPropertyName("endsAt")] public DateTimeOffset? EndsAt { get; set; }
    /// <summary>All-day event; the API defaults to <c>false</c>.</summary>
    [JsonPropertyName("allDay")] public bool? AllDay { get; set; }
    /// <summary>IANA time zone, e.g. <c>"Africa/Cairo"</c>.</summary>
    [JsonPropertyName("timezone")] public string? Timezone { get; set; }
    /// <summary>http(s) URL.</summary>
    [JsonPropertyName("url")] public string? Url { get; set; }
}

/// <summary>WhatsApp chat target: encoded as <c>https://wa.me/&lt;digits&gt;[?text=…]</c>.</summary>
public class QRCodeWhatsAppTargetModel
{
    /// <summary>Phone number in international format (required).</summary>
    [JsonPropertyName("phoneNumber")] public string PhoneNumber { get; set; } = string.Empty;
    /// <summary>Pre-filled message.</summary>
    [JsonPropertyName("message")] public string? Message { get; set; }
}

/// <summary>Review target. Google takes <c>PlaceId</c> or <c>Url</c>; the other platforms a <c>Url</c> on their host.</summary>
public class QRCodeReviewTargetModel
{
    /// <summary>Platform (required), see <see cref="QRCodeReviewPlatforms"/>.</summary>
    [JsonPropertyName("platform")] public string Platform { get; set; } = string.Empty;
    /// <summary>Google place ID (Google only).</summary>
    [JsonPropertyName("placeId")] public string? PlaceId { get; set; }
    /// <summary>Review page URL.</summary>
    [JsonPropertyName("url")] public string? Url { get; set; }
}

/// <summary>One social profile: <c>Handle</c> or <c>Url</c>.</summary>
public class QRCodeSocialProfileModel
{
    /// <summary>Platform (required), see <see cref="QRCodeSocialPlatforms"/>.</summary>
    [JsonPropertyName("platform")] public string Platform { get; set; } = string.Empty;
    /// <summary>Handle.</summary>
    [JsonPropertyName("handle")] public string? Handle { get; set; }
    /// <summary>Url.</summary>
    [JsonPropertyName("url")] public string? Url { get; set; }
}

/// <summary>Social profiles target: 1 to 12 profiles. A static code takes one profile (its URL is encoded); more than one requires <see cref="QRCodeMode.Dynamic"/>.</summary>
public class QRCodeSocialTargetModel
{
    /// <summary>1 to 12 profiles (more than one requires a dynamic code).</summary>
    [JsonPropertyName("profiles")] public List<QRCodeSocialProfileModel> Profiles { get; set; } = new();
    /// <summary>Title.</summary>
    [JsonPropertyName("title")] public string? Title { get; set; }
}

/// <summary>Create vCard QR code request.</summary>
public class QRCodeCreateVCardRequestModel : QRCodeRequestBaseModel
{
    /// <summary>VCard.</summary>
    public QRCodeVCardTargetModel VCard { get; set; } = new();
}

/// <summary>Create event QR code request.</summary>
public class QRCodeCreateEventRequestModel : QRCodeRequestBaseModel
{
    /// <summary>Event.</summary>
    public QRCodeEventTargetModel Event { get; set; } = new();
}

/// <summary>Create WhatsApp QR code request.</summary>
public class QRCodeCreateWhatsAppRequestModel : QRCodeRequestBaseModel
{
    /// <summary>WhatsApp.</summary>
    public QRCodeWhatsAppTargetModel WhatsApp { get; set; } = new();
}

/// <summary>Create review QR code request.</summary>
public class QRCodeCreateReviewRequestModel : QRCodeRequestBaseModel
{
    /// <summary>Review.</summary>
    public QRCodeReviewTargetModel Review { get; set; } = new();
}

/// <summary>Create social profiles QR code request.</summary>
public class QRCodeCreateSocialRequestModel : QRCodeRequestBaseModel
{
    /// <summary>Social.</summary>
    public QRCodeSocialTargetModel Social { get; set; } = new();
}

/// <summary>Update vCard QR code request.</summary>
public class QRCodeUpdateVCardRequestModel : QRCodeCreateVCardRequestModel
{
    /// <summary>QR code name (required for updates).</summary>
    public new string Name { get; set; } = string.Empty;
}

/// <summary>Update event QR code request.</summary>
public class QRCodeUpdateEventRequestModel : QRCodeCreateEventRequestModel
{
    /// <summary>QR code name (required for updates).</summary>
    public new string Name { get; set; } = string.Empty;
}

/// <summary>Update WhatsApp QR code request.</summary>
public class QRCodeUpdateWhatsAppRequestModel : QRCodeCreateWhatsAppRequestModel
{
    /// <summary>QR code name (required for updates).</summary>
    public new string Name { get; set; } = string.Empty;
}

/// <summary>Update review QR code request.</summary>
public class QRCodeUpdateReviewRequestModel : QRCodeCreateReviewRequestModel
{
    /// <summary>QR code name (required for updates).</summary>
    public new string Name { get; set; } = string.Empty;
}

/// <summary>Update social profiles QR code request.</summary>
public class QRCodeUpdateSocialRequestModel : QRCodeCreateSocialRequestModel
{
    /// <summary>QR code name (required for updates).</summary>
    public new string Name { get; set; } = string.Empty;
}

/// <summary>MIME types a <c>file</c> code accepts.</summary>
public static class QRCodeFileMimeTypes
{
    /// <summary>PDF.</summary>
    public const string Pdf = "application/pdf";
    /// <summary>JPEG.</summary>
    public const string Jpeg = "image/jpeg";
    /// <summary>PNG.</summary>
    public const string Png = "image/png";
    /// <summary>WebP.</summary>
    public const string Webp = "image/webp";
}

/// <summary>App store target (dynamic-only): a scan goes to the store of the scanning device, else to <c>FallbackUrl</c>.</summary>
public class QRCodeAppStoreTargetModel
{
    /// <summary>Google Play URL.</summary>
    [JsonPropertyName("androidUrl")] public string? AndroidUrl { get; set; }
    /// <summary>App Store URL.</summary>
    [JsonPropertyName("iosUrl")] public string? IosUrl { get; set; }
    /// <summary>Where any other device goes (required).</summary>
    [JsonPropertyName("fallbackUrl")] public string FallbackUrl { get; set; } = string.Empty;
}

/// <summary>
/// File target (dynamic-only) as returned by the API: a hosted PDF or image. <c>FileURL</c>,
/// <c>MimeType</c> and <c>SizeBytes</c> are server-set after the upload is verified.
/// </summary>
public class QRCodeFileTargetModel
{
    /// <summary>Display name of the file.</summary>
    [JsonPropertyName("fileName")] public string? FileName { get; set; }
    /// <summary>Public URL of the hosted file (server-set).</summary>
    [JsonPropertyName("fileURL")] public string? FileURL { get; set; }
    /// <summary>MIME type (server-set), see <see cref="QRCodeFileMimeTypes"/>.</summary>
    [JsonPropertyName("mimeType")] public string? MimeType { get; set; }
    /// <summary>Size in bytes (server-set).</summary>
    [JsonPropertyName("sizeBytes")] public long? SizeBytes { get; set; }
}

/// <summary>What <c>CreateFileAsync</c> / <c>UpdateFileAsync</c> take under <c>File</c>.</summary>
public class QRCodeFileInputModel
{
    /// <summary>Display name; also the name sent to the upload-url route. Default <c>"file"</c>.</summary>
    public string? FileName { get; set; }
    /// <summary>
    /// Size in bytes. Read from <c>Stream.Length</c> when the stream is seekable;
    /// required for a non-seekable stream.
    /// </summary>
    public long? SizeBytes { get; set; }
}

/// <summary><c>POST /api/qr-code/file/upload-url</c> response.</summary>
public class QRCodeFileUploadTicketModel
{
    /// <summary>Signed PUT URL, valid for <see cref="ExpiresInSeconds"/> (60 s).</summary>
    [JsonPropertyName("uploadFileURL")] public string UploadFileURL { get; set; } = string.Empty;
    /// <summary>Path sent back on create / update.</summary>
    [JsonPropertyName("bucketFilePath")] public string BucketFilePath { get; set; } = string.Empty;
    /// <summary>Lifetime of <see cref="UploadFileURL"/>.</summary>
    [JsonPropertyName("expiresInSeconds")] public int ExpiresInSeconds { get; set; }
}

/// <summary>Create app store QR code request. Dynamic-only: <c>Mode</c> may be omitted or <see cref="QRCodeMode.Dynamic"/>.</summary>
public class QRCodeCreateAppStoreRequestModel : QRCodeRequestBaseModel
{
    /// <summary>App store target.</summary>
    public QRCodeAppStoreTargetModel AppStore { get; set; } = new();
}

/// <summary>Create file QR code request. Dynamic-only. The content is a separate argument of <c>CreateFileAsync</c>.</summary>
public class QRCodeCreateFileRequestModel : QRCodeRequestBaseModel
{
    /// <summary>File name and size.</summary>
    public QRCodeFileInputModel? File { get; set; }
}

/// <summary>Update app store QR code request.</summary>
public class QRCodeUpdateAppStoreRequestModel : QRCodeCreateAppStoreRequestModel
{
    /// <summary>QR code name (required for updates).</summary>
    public new string Name { get; set; } = string.Empty;
}

/// <summary>Update file QR code request. Without new content only <c>File.FileName</c> may change; the stored file is kept.</summary>
public class QRCodeUpdateFileRequestModel : QRCodeCreateFileRequestModel
{
    /// <summary>QR code name (required for updates).</summary>
    public new string Name { get; set; } = string.Empty;
}
