namespace Posty5.HtmlHosting.Models;

/// <summary>
/// HTML hosting page model
/// </summary>
public class HtmlHostingModel
{
    public string? Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? HtmlContent { get; set; }
    public string? PublicUrl { get; set; }
    public string? TemplateId { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// Create HTML hosting request
/// </summary>
public class CreateHtmlHostingRequest
{
    public string Name { get; set; } = string.Empty;
    public string HtmlContent { get; set; } = string.Empty;
    public string? TemplateId { get; set; }
}

/// <summary>
/// Update HTML hosting request
/// </summary>
public class UpdateHtmlHostingRequest
{
    public string? Name { get; set; }
    public string? HtmlContent { get; set; }
    public string? TemplateId { get; set; }
}

/// <summary>
/// List parameters for HTML hosting pages
/// </summary>
public class ListHtmlHostingParams
{
    public string? Search { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
}
