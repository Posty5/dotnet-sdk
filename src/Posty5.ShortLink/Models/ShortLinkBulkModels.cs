namespace Posty5.ShortLink.Models;

/// <summary>
/// One short link to create in bulk (<c>IShortLinkBulkRow</c>). The shared
/// answer, job and export types are in <c>Posty5.Core.Models</c>
/// (<c>BulkCreateResult</c>, <c>LinkBulkJob</c>, <c>ExportOptions</c>).
/// </summary>
public class ShortLinkBulkRow
{
    /// <summary>Destination URL (required).</summary>
    public string Url { get; set; } = string.Empty;

    /// <summary>Name; when empty the API uses the destination's host name.</summary>
    public string? Name { get; set; }

    /// <summary>Custom short code (needs the custom-id feature on your plan).</summary>
    public string? CustomId { get; set; }

    /// <summary>Tag.</summary>
    public string? Tag { get; set; }

    /// <summary>Your own reference.</summary>
    public string? RefId { get; set; }

    /// <summary>Template; overrides <c>BulkCreateDefaults.TemplateId</c>.</summary>
    public string? TemplateId { get; set; }
}
