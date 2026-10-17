using System.Text.Json.Serialization;

namespace Posty5.QRCode.Models;

/// <summary>
/// A QR code template as the template lookups list it — enough to choose one
/// and pass its id as <c>TemplateId</c> when creating a QR code or a short link.
/// </summary>
public class QRCodeTemplateLookupItemModel
{
    /// <summary>The template id.</summary>
    [JsonPropertyName("_id")]
    public string Id { get; set; } = string.Empty;

    /// <summary>The template's name.</summary>
    public string? Name { get; set; }
}

/// <summary>
/// One of your templates with its design (<c>GET /api/qr-code-template/{id}</c>).
/// </summary>
public class QRCodeTemplateDetailsModel : QRCodeTemplateModel
{
    /// <summary>Whether the template is one of Posty5's public templates.</summary>
    public bool? IsPublic { get; set; }

    /// <summary>The colour scheme a public template belongs to.</summary>
    public int? SchemeType { get; set; }

    /// <summary>The design.</summary>
    public QRCodeOptionsModel? Options { get; set; }
}

/// <summary>
/// The body of <see cref="QRCodeTemplateClient.CreateAsync"/> and
/// <see cref="QRCodeTemplateClient.UpdateAsync"/>. An update replaces the name and the
/// design, except that a stored v2 design keeps its v2 fields unless
/// <see cref="QRCodeOptionsModel.DesignVersion"/> is sent.
/// </summary>
public class QRCodeTemplateRequestModel
{
    /// <summary>The template's name.</summary>
    public required string Name { get; set; }

    /// <summary>The design.</summary>
    public required QRCodeOptionsModel Options { get; set; }
}
