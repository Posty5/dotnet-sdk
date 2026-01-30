using System.Text.Json.Serialization;

namespace Posty5.HtmlHosting.Models;

 

/// <summary>
/// Form submission statistics
/// </summary>
public class HtmlHostingFormSubmissionDataModel
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
public class HtmlHostingPreviewReasonModel
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
public class HtmlHostingUploadFileConfigModel
{
    /// <summary>
    /// Pre-signed URL for direct upload to R2
    /// </summary>
    public string UploadUrl { get; set; } = string.Empty;
}

/// <summary>
/// HTML page model with full details
/// </summary>
public class HtmlHostingPageModel
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
    public DateTime? LastVisitorDate { get; set; }

    /// <summary>
    /// Number of visitors/views
    /// </summary>
    public int? NumberOfVisitors { get; set; }

    /// <summary>
    /// Number of reports
    /// </summary>
    public int? NumberOfReports { get; set; }

    /// <summary>
    /// Owner user ID
    /// </summary>
    public string? UserId { get; set; }

    /// <summary>
    /// API key ID
    /// </summary>
    public string? ApiKeyId { get; set; }

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
    public bool? IsTemp { get; set; }

    /// <summary>
    /// External reference ID for filtering/tracking
    /// </summary>
    public string? RefId { get; set; }

    /// <summary>
    /// Custom tag for filtering/categorization
    /// </summary>
    public string? Tag { get; set; }

    /// <summary>
    /// Template ID (QR or page template)
    /// </summary>
    public string? TemplateId { get; set; }

    /// <summary>
    /// Page sub category
    /// </summary>
    public int? SubCategory { get; set; }

    /// <summary>
    /// Page category
    /// </summary>
    public int? Category { get; set; }

    /// <summary>
    /// Created from (dashboard, api, etc)
    /// </summary>
    public string? CreatedFrom { get; set; }

    /// <summary>
    /// GitHub information (when sourceType is "github")
    /// </summary>
    public HtmlHostingGithubInfoModel? GithubInfo { get; set; }

    /// <summary>
    /// Form submission data
    /// </summary>
    public HtmlHostingFormSubmissionModel? FormSubmission { get; set; }

    /// <summary>
    /// Is cached in local storage
    /// </summary>
    public bool? IsCachedInLocalStorage { get; set; }

    /// <summary>
    /// Shorter link URL
    /// </summary>
    public string? ShorterLink { get; set; }

    /// <summary>
    /// File URL (R2 storage)
    /// </summary>
    public string? FileUrl { get; set; }

    /// <summary>
    /// Source type: "file" or "github"
    /// </summary>
    public string? SourceType { get; set; }

    /// <summary>
    /// File name (when sourceType is "file")
    /// </summary>
    public string? FileName { get; set; }
}

public class HtmlHostingGithubInfoModel
{
    /// <summary>
    /// GitHub file public URL
    /// </summary>
    public string? FileURL { get; set; }
}

public class HtmlHostingFormSubmissionModel
{
    /// <summary>
    /// Number of form submissions
    /// </summary>
    public int? NumberOfFormSubmission { get; set; }

    /// <summary>
    /// Last form submission time
    /// </summary>
    public DateTime? LasFormSubmissionAt { get; set; }

    /// <summary>
    /// Last export time
    /// </summary>
    public DateTime? LasExportTime { get; set; }

    /// <summary>
    /// Forms configuration
    /// </summary>
    public List<HtmlHostingFormInfoModel>? Forms { get; set; }
}

public class HtmlHostingFormInfoModel
{
    /// <summary>
    /// Form identifier
    /// </summary>
    public string? FormId { get; set; }

    /// <summary>
    /// Form fields names
    /// </summary>
    public List<string>? FormFields { get; set; }

    /// <summary>
    /// Google spreadsheet ID
    /// </summary>
    public string? SpreadsheetId { get; set; }
}
/// <summary>
/// Base request for HTML page creation/update
/// </summary>
public class HtmlHostingPageRequestBaseModel
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
public class HtmlHostingCreatePageFileRequestModel : HtmlHostingPageRequestBaseModel
{
    /// <summary>
    /// File name for the HTML file
    /// </summary>
    public string FileName { get; set; } = string.Empty;
}

/// <summary>
/// Create HTML page from GitHub repository
/// </summary>
public class HtmlHostingCreatePageGithubRequestModel : HtmlHostingPageRequestBaseModel
{
    /// <summary>
    /// GitHub repository file information
    /// </summary>
    public HtmlHostingGithubInfoModel GithubInfo { get; set; } = new();
}

/// <summary>
/// Update HTML page with new file
/// </summary>
public class HtmlHostingUpdatePageFileRequestModel : HtmlHostingPageRequestBaseModel
{
    /// <summary>
    /// File name for the HTML file
    /// </summary>
    public string FileName { get; set; } = string.Empty;
}

/// <summary>
/// Update HTML page from GitHub repository
/// </summary>
public class HtmlHostingUpdatePageGithubRequestModel : HtmlHostingPageRequestBaseModel
{
    /// <summary>
    /// GitHub repository file information
    /// </summary>
    public HtmlHostingGithubInfoModel GithubInfo { get; set; } = new();
}

/// <summary>
/// Response from create/update operations with upload config
/// Note: API has typo "uplaodFileConfig" instead of "uploadFileConfig"
/// </summary>
public class HtmlHostingCreatePageResponseModel
{
    /// <summary>
    /// Upload configuration for R2 storage
    /// Note: Property name has intentional typo to match API
    /// </summary>
    [JsonPropertyName("uplaodFileConfig")]
    public HtmlHostingUploadFileConfigModel? UploadFileConfig { get; set; }
    
    /// <summary>
    /// HTML page details
    /// </summary>
    public HtmlHostingPageModel? Details { get; set; }
}

/// <summary>
/// Simplified response for file-based operations
/// </summary>
public class HtmlHostingPageFileResponseModel
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
public class HtmlHostingPageGithubResponseModel
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
    public HtmlHostingGithubInfoModel? GithubInfo { get; set; }
}

/// <summary>
/// Simplified HTML page for lookup/dropdown lists
/// </summary>
public class HtmlHostingPageLookupItemModel
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
public class HtmlHostingFormLookupItemModel
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
public class HtmlHostingListPagesParamsModel
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
    public HtmlHostingStatusType? Status { get; set; }
    
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


public readonly record struct HtmlHostingStatusType (string Value)
{
    public static readonly HtmlHostingStatusType New = new("new");
    public static readonly HtmlHostingStatusType Pending = new("pending");
    public static readonly HtmlHostingStatusType Rejected = new("rejected");
    public static readonly HtmlHostingStatusType Approved = new("approved");
    public static readonly HtmlHostingStatusType FileIsNotFound = new("fileIsNotFound");

    public override string ToString ( ) => Value;
}