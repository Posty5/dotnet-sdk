using System.Diagnostics;
using System.Text.Json.Serialization;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Posty5.ShortLink.Models;

/// <summary>
/// Page information for landing page customization
/// </summary>
public class ShortLinkPageInfoModel
{
    /// <summary>
    /// Landing page title
    /// </summary>
    public string? Title { get; set; }
    
    /// <summary>
    /// Landing page description
    /// </summary>
    public string? Description { get; set; }
    
}

public class ShortLinkModel
{
    /// <summary>
    /// MongoDB document ID
    /// </summary>
    [JsonPropertyName("_id")]
    public string? Id { get; set; }
    
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Shorter link URL - the actual short URL (primary field from API)
    /// 
    /// </summary>
    public string? ShorterLink { get; set; }
    
    /// <summary>
    /// Short link ID - unique short code identifier
    /// 
    /// </summary>
    public string? ShortLinkId { get; set; }
    
    /// <summary>
    /// Base URL - the target URL to redirect to
    /// 
    /// </summary>
    public string? BaseUrl { get; set; }
    
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
    /// Number of visitors/clicks
    /// 
    /// </summary>
    public int? NumberOfVisitors { get; set; }
    
    /// <summary>
    /// Last visitor date
    /// 
    /// </summary>
    public string? LastVisitorDate { get; set; }
    
    /// <summary>
    /// QR code landing page URL
    /// 
    /// </summary>
    public string? QrCodeLandingPageURL { get; set; }
    
    /// <summary>
    /// QR code download URL
    /// 
    /// </summary>
    public string? QrCodeDownloadURL { get; set; }
    
    /// <summary>
    /// Landing page information
    /// </summary>
    public ShortLinkPageInfoModel? PageInfo { get; set; }
    
    /// <summary>
    /// Created timestamp
    /// </summary>
    public DateTime? CreatedAt { get; set; }
    
    /// <summary>
    /// Updated timestamp
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    public ShortLinkStatusType? Status { get; set; }

}

/// <summary>
/// Create short link request
/// </summary>
public class ShortLinkCreateRequestModel
{
    public string Name { get; set; } = string.Empty;
    /// <summary>
    /// Base URL (the target URL to redirect to)
    /// </summary>
    public string BaseUrl { get; set; } = string.Empty;
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
    /// Custom landing page ID (max 32 characters, paid plans only)
    /// </summary>
    public string? CustomLandingId { get; set; }
    
    /// <summary>
    /// Enable monetization for this short link
    /// </summary>
    public bool? IsEnableMonetization { get; set; }
    
    /// <summary>
    /// Landing page information
    /// </summary>
    public ShortLinkPageInfoModel? PageInfo { get; set; }
}

/// <summary>
/// Update short link request
/// </summary>
public class ShortLinkUpdateRequestModel
{
    public string? Name { get; set; }
    /// <summary>
    /// Base URL (the target URL to redirect to)
    /// </summary>
    public string? BaseUrl { get; set; }
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
    /// Enable landing page
    /// </summary>
    public bool? IsEnableLandingPage { get; set; }
    
    /// <summary>
    /// Enable monetization for this short link
    /// </summary>
    public bool? IsEnableMonetization { get; set; }
    
    /// <summary>
    /// Landing page information
    /// </summary>
    public ShortLinkPageInfoModel? PageInfo { get; set; }
}

/// <summary>
/// List parameters for short links
/// </summary>
public class ShortLinkListParamsModel
{
    /// <summary>
    /// Search by full or partial target URL
    /// </summary>
    public string? BaseUrl { get; set; }
    
    /// <summary>
    /// Search by name
    /// </summary>
    public string? Name { get; set; }
    
    /// <summary>
    /// Search by page title
    /// </summary>
    public string? PageInfoTitle { get; set; }
    
    /// <summary>
    /// Filter by created from source
    /// </summary>
    public string? CreatedFrom { get; set; }
    
    /// <summary>
    /// Filter by short link ID
    /// </summary>
    public string? ShortLinkId { get; set; }
    
    /// <summary>
    /// Filter by external reference ID
    /// </summary>
    public string? RefId { get; set; }
    
    /// <summary>
    /// Filter by custom tag
    /// </summary>
    public string? Tag { get; set; }
    
    /// <summary>
    /// Filter by template ID
    /// </summary>
    public string? TemplateId { get; set; }
    
    /// <summary>
    /// Filter by status (new, pending, approved, rejected)
    /// </summary>
    public ShortLinkStatusType? Status { get; set; }
    
    /// <summary>
    /// Filter by deep link flag
    /// </summary>
    public bool? IsForDeepLink { get; set; }
    
    /// <summary>
    /// Filter by monetization flag
    /// </summary>
    public bool? IsEnableMonetization { get; set; }
    
    /// <summary>
    /// Generic search term
    /// </summary>
    public string? Search { get; set; }
    
    /// <summary>
    /// Filter from date
    /// </summary>
    public DateTime? FromDate { get; set; }
    
    /// <summary>
    /// Filter to date
    /// </summary>
    public DateTime? ToDate { get; set; }
}

public readonly record struct ShortLinkStatusType (string Value)
{
    public static readonly ShortLinkStatusType New = new("new");
    public static readonly ShortLinkStatusType Pending = new("pending");
    public static readonly ShortLinkStatusType Rejected = new("rejected");
    public static readonly ShortLinkStatusType Approved = new("approved");

    public override string ToString ( ) => Value;
}