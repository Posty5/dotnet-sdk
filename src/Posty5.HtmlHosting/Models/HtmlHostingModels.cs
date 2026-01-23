using System.Text.Json.Serialization;

namespace Posty5.HtmlHosting.Models;

/// <summary>
/// GitHub repository information for HTML hosting
/// </summary>
public class GithubInfo
{
    /// <summary>
    /// GitHub file URL (supports multiple formats)
    /// </summary>
    public string FileURL { get; set; } = string.Empty;
    
    /// <summary>
    /// Final computed raw file URL
    /// </summary>
    public string? FinalFileRawURL { get; set; }
}

/// <summary>
/// Form submission statistics
/// </summary>
public class FormSubmissionData
{
    /// <summary>
    /// Last form submission date
    /// </summary>
    public string? LasFormSubmissionAt { get; set; }
    
    /// <summary>
    /// Number of form submissions
    /// </summary>
    public int NumberOfFormSubmission { get; set; }
}

/// <summary>
/// Preview/moderation reason with score
/// </summary>
public class PreviewReason
{
    /// <summary>
    /// Reason key/category
    /// </summary>
    public string Key { get; set; } = string.Empty;
    
    /// <summary>
    /// Reason value/score
    /// </summary>
    public double Value { get; set; }
}

/// <summary>
/// Upload configuration for R2 storage
/// </summary>
public class UploadFileConfig
{
    /// <summary>
    /// Pre-signed URL for direct upload to R2
    /// </summary>
    public string UploadUrl { get; set; } = string.Empty;
}

/// <summary>
/// HTML page model with full details
/// </summary>
public class HtmlPageModel
{
    /// <summary>
    /// MongoDB document ID
    /// </summary>
    [JsonPropertyName("_id")]
    public string? Id { get; set; }
    
    /// <summary>
    /// HTML hosting ID (short code identifier)
    /// </summary>
    public string? HtmlHostingId { get; set; }
    
    /// <summary>
    /// Page name
    /// </summary>
    public string? Name { get; set; }
    
    /// <summary>
    /// SEO-friendly page path
    /// </summary>
    public string? PagePath { get; set; }
    
    /// <summary>
    /// Last visitor date
    /// </summary>
    public string? LastVisitorDate { get; set; }
    
    /// <summary>
    /// Number of visitors/views
    /// </summary>
    public int NumberOfVisitors { get; set; }
    
    /// <summary>
    /// Created timestamp
    /// </summary>
    public DateTime CreatedAt { get; set; }
    
    /// <summary>
    /// Updated timestamp
    /// </summary>
    public DateTime? UpdatedAt { get; set; }
    
    /// <summary>
    /// Owner user ID
    /// </summary>
    public string User { get; set; } = string.Empty;
    
    /// <summary>
    /// Enable monetization flag
    /// </summary>
    public bool? IsEnableMonetization { get; set; }
    
    /// <summary>
    /// Auto-save form submissions in Google Sheet
    /// </summary>
    public bool? AutoSaveInGoogleSheet { get; set; }
    
    /// <summary>
    /// Is temporary page
    /// </summary>
    public bool IsTemp { get; set; }
    
    /// <summary>
    /// Page status (approved, pending, rejected, etc.)
    /// </summary>
    public string? Status { get; set; }
    
    /// <summary>
    /// Preview/moderation reasons
    /// </summary>
    public List<PreviewReason>? PreviewReasons { get; set; }
    
    /// <summary>
    /// Is cached in local storage
    /// </summary>
    public bool IsCachedInLocalStorage { get; set; }
    
    /// <summary>
    /// Source type: "file" or "github"
    /// </summary>
    public string? SourceType { get; set; }
    
    /// <summary>
    /// Form submission data
    /// </summary>
    public FormSubmissionData? FormSubmission { get; set; }
    
    /// <summary>
    /// Shorter link URL
    /// </summary>
    public string? ShorterLink { get; set; }
    
    /// <summary>
    /// File URL (R2 storage)
    /// </summary>
    public string? FileUrl { get; set; }
    
    /// <summary>
    /// GitHub information (when sourceType is "github")
    /// </summary>
    public GithubInfo? GithubInfo { get; set; }
    
    /// <summary>
    /// File name (when sourceType is "file")
    /// </summary>
    public string? FileName { get; set; }
    
    /// <summary>
    /// External reference ID for filtering/tracking
    /// </summary>
    public string? RefId { get; set; }
    
    /// <summary>
    /// Custom tag for filtering/categorization
    /// </summary>
    public string? Tag { get; set; }
}

/// <summary>
/// Base request for HTML page creation/update
/// </summary>
public class HtmlPageRequestBase
{
    /// <summary>
    /// Page name
    /// </summary>
    public string? Name { get; set; }
    
    /// <summary>
    /// Custom landing ID (max 32 characters, paid plans only)
    /// </summary>
    public string? CustomLandingId { get; set; }
    
    /// <summary>
    /// Enable monetization for this page
    /// </summary>
    public bool? IsEnableMonetization { get; set; }
    
    /// <summary>
    /// Auto-save form submissions in Google Sheet
    /// </summary>
    public bool? AutoSaveInGoogleSheet { get; set; }
    
    /// <summary>
    /// Custom tag for filtering/categorization
    /// </summary>
    public string? Tag { get; set; }
    
    /// <summary>
    /// External reference ID for filtering/tracking
    /// </summary>
    public string? RefId { get; set; }
}

/// <summary>
/// Create HTML page with file upload
/// </summary>
public class CreateHtmlPageFileRequest : HtmlPageRequestBase
{
    /// <summary>
    /// File name for the HTML file
    /// </summary>
    public string FileName { get; set; } = string.Empty;
}

/// <summary>
/// Create HTML page from GitHub repository
/// </summary>
public class CreateHtmlPageGithubRequest : HtmlPageRequestBase
{
    /// <summary>
    /// GitHub repository file information
    /// </summary>
    public GithubInfo GithubInfo { get; set; } = new();
}

/// <summary>
/// Update HTML page with new file
/// </summary>
public class UpdateHtmlPageFileRequest : HtmlPageRequestBase
{
    /// <summary>
    /// File name for the HTML file
    /// </summary>
    public string FileName { get; set; } = string.Empty;
}

/// <summary>
/// Update HTML page from GitHub repository
/// </summary>
public class UpdateHtmlPageGithubRequest : HtmlPageRequestBase
{
    /// <summary>
    /// GitHub repository file information
    /// </summary>
    public GithubInfo GithubInfo { get; set; } = new();
}

/// <summary>
/// Response from create/update operations with upload config
/// Note: API has typo "uplaodFileConfig" instead of "uploadFileConfig"
/// </summary>
public class CreateHtmlPageResponse
{
    /// <summary>
    /// Upload configuration for R2 storage
    /// Note: Property name has intentional typo to match API
    /// </summary>
    [JsonPropertyName("uplaodFileConfig")]
    public UploadFileConfig? UploadFileConfig { get; set; }
    
    /// <summary>
    /// HTML page details
    /// </summary>
    public HtmlPageModel? Details { get; set; }
}

/// <summary>
/// Simplified response for file-based operations
/// </summary>
public class HtmlPageFileResponse
{
    /// <summary>
    /// Page ID
    /// </summary>
    public string? Id { get; set; }
    
    /// <summary>
    /// Shorter link URL
    /// </summary>
    public string? ShorterLink { get; set; }
    
    /// <summary>
    /// File URL (R2 storage)
    /// </summary>
    public string? FileUrl { get; set; }
}

/// <summary>
/// Simplified response for GitHub-based operations
/// </summary>
public class HtmlPageGithubResponse
{
    /// <summary>
    /// Page ID
    /// </summary>
    public string? Id { get; set; }
    
    /// <summary>
    /// Shorter link URL
    /// </summary>
    public string? ShorterLink { get; set; }
    
    /// <summary>
    /// GitHub repository information
    /// </summary>
    public GithubInfo? GithubInfo { get; set; }
}

/// <summary>
/// Simplified HTML page for lookup/dropdown lists
/// </summary>
public class HtmlPageLookupItem
{
    /// <summary>
    /// Page ID
    /// </summary>
    [JsonPropertyName("_id")]
    public string Id { get; set; } = string.Empty;
    
    /// <summary>
    /// Page name
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// HTML hosting ID
    /// </summary>
    public string HtmlHostingId { get; set; } = string.Empty;
}

/// <summary>
/// Form lookup item
/// </summary>
public class FormLookupItem
{
    /// <summary>
    /// Form internal ID
    /// </summary>
    [JsonPropertyName("_id")]
    public string Id { get; set; } = string.Empty;
    
    /// <summary>
    /// Form ID
    /// </summary>
    public string FormId { get; set; } = string.Empty;
    
    /// <summary>
    /// Form fields
    /// </summary>
    public List<string> FormFields { get; set; } = new();
}

/// <summary>
/// Parameters for listing/filtering HTML pages
/// </summary>
public class ListHtmlPagesParams
{
    /// <summary>
    /// Search by name
    /// </summary>
    public string? Name { get; set; }
    
    /// <summary>
    /// Filter by HTML hosting ID
    /// </summary>
    public string? HtmlHostingId { get; set; }
    
    /// <summary>
    /// Filter by custom tag
    /// </summary>
    public string? Tag { get; set; }
    
    /// <summary>
    /// Filter by external reference ID
    /// </summary>
    public string? RefId { get; set; }
    
    /// <summary>
    /// Filter by status
    /// </summary>
    public string? Status { get; set; }
    
    /// <summary>
    /// Filter by source type ("file" or "github")
    /// </summary>
    public string? SourceType { get; set; }
    
    /// <summary>
    /// Filter by monetization flag
    /// </summary>
    public bool? IsEnableMonetization { get; set; }
    
    /// <summary>
    /// Filter by auto-save in Google Sheet flag
    /// </summary>
    public bool? AutoSaveInGoogleSheet { get; set; }
    
    /// <summary>
    /// Filter by temporary page flag
    /// </summary>
    public bool? IsTemp { get; set; }
    
    /// <summary>
    /// Filter by cached in local storage flag
    /// </summary>
    public bool? IsCachedInLocalStorage { get; set; }
}
