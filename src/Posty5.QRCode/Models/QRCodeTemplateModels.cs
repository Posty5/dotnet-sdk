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
