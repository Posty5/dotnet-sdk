namespace Posty5.ShortLink.Models;

/// <summary>
/// Short link model
/// </summary>
public class ShortLinkModel
{
    public string? Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? ShortUrl { get; set; }
    public string TargetUrl { get; set; } = string.Empty;
    public string? TemplateId { get; set; }
    public string? CustomSlug { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public int? ClickCount { get; set; }
}

/// <summary>
/// Create short link request
/// </summary>
public class CreateShortLinkRequest
{
    public string Name { get; set; } = string.Empty;
    public string TargetUrl { get; set; } = string.Empty;
    public string? TemplateId { get; set; }
    public string? CustomSlug { get; set; }
}

/// <summary>
/// Update short link request
/// </summary>
public class UpdateShortLinkRequest
{
    public string? Name { get; set; }
    public string? TargetUrl { get; set; }
    public string? TemplateId { get; set; }
    public string? CustomSlug { get; set; }
}

/// <summary>
/// List parameters for short links
/// </summary>
public class ListShortLinksParams
{
    public string? Search { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
}
