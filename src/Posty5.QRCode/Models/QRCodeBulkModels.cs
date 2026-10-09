namespace Posty5.QRCode.Models;

/// <summary>
/// The <c>type</c> values a bulk QR row can carry (the same names as the
/// single-create routes, <c>POST /api/qr-code/{type}</c>).
/// </summary>
public static class QRCodeBulkType
{
    /// <summary>Free text.</summary>
    public const string FreeText = "freeText";
    /// <summary>Email.</summary>
    public const string Email = "email";
    /// <summary>Wi-Fi.</summary>
    public const string Wifi = "wifi";
    /// <summary>Phone call.</summary>
    public const string Call = "call";
    /// <summary>SMS.</summary>
    public const string Sms = "sms";
    /// <summary>URL (the API's default when a CSV row has no type).</summary>
    public const string Url = "url";
    /// <summary>Geolocation.</summary>
    public const string Geolocation = "geolocation";
}

/// <summary>
/// One QR code to create in bulk (<c>IQrCodeBulkRow</c>): a <see cref="Type"/>
/// discriminator plus that type's target fields, sent as the API's <c>type</c>
/// and <c>target</c>. Build one with the typed factories
/// (<see cref="ForUrl"/>, <see cref="ForWifi"/>, …) so the target always matches its type.
/// </summary>
public class QRCodeBulkRow
{
    /// <summary>One of <see cref="QRCodeBulkType"/>.</summary>
    public string Type { get; set; } = QRCodeBulkType.Url;

    /// <summary>The type's target model (e.g. <see cref="QRCodeUrlTargetModel"/>).</summary>
    public object Target { get; set; } = new();

    /// <summary><c>static</c> (default) or <c>dynamic</c>. Wi-Fi rows cannot be dynamic.</summary>
    public string? Mode { get; set; }

    /// <summary>Name.</summary>
    public string? Name { get; set; }

    /// <summary>Custom landing id.</summary>
    public string? CustomId { get; set; }

    /// <summary>Tag.</summary>
    public string? Tag { get; set; }

    /// <summary>Your own reference.</summary>
    public string? RefId { get; set; }

    /// <summary>Template; overrides <c>BulkCreateDefaults.TemplateId</c>.</summary>
    public string? TemplateId { get; set; }

    /// <summary>Image name inside a job's ZIP (slugged and deduplicated by the API).</summary>
    public string? FileName { get; set; }

    /// <summary>A URL row.</summary>
    public static QRCodeBulkRow ForUrl(QRCodeUrlTargetModel target) => new() { Type = QRCodeBulkType.Url, Target = target };
    /// <summary>A free-text row.</summary>
    public static QRCodeBulkRow ForFreeText(QRCodeFreeTextTargetModel target) => new() { Type = QRCodeBulkType.FreeText, Target = target };
    /// <summary>An email row.</summary>
    public static QRCodeBulkRow ForEmail(QRCodeEmailTargetModel target) => new() { Type = QRCodeBulkType.Email, Target = target };
    /// <summary>A Wi-Fi row.</summary>
    public static QRCodeBulkRow ForWifi(QRCodeWifiTargetModel target) => new() { Type = QRCodeBulkType.Wifi, Target = target };
    /// <summary>A call row.</summary>
    public static QRCodeBulkRow ForCall(QRCodeCallTargetModel target) => new() { Type = QRCodeBulkType.Call, Target = target };
    /// <summary>An SMS row.</summary>
    public static QRCodeBulkRow ForSms(QRCodeSmsTargetModel target) => new() { Type = QRCodeBulkType.Sms, Target = target };
    /// <summary>A geolocation row.</summary>
    public static QRCodeBulkRow ForGeolocation(QRCodeGeolocationTargetModel target) => new() { Type = QRCodeBulkType.Geolocation, Target = target };
}
