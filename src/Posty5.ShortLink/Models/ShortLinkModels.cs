using Posty5.Core.Converts;
using Posty5.Core.Models;
using System.Diagnostics;
using System.Text.Json.Serialization;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Posty5.ShortLink.Models;
 
/// <summary>
/// QR Code template information
/// </summary>
public class QRCodeTemplateModel
{
    /// <summary>
    /// Template ID
    /// </summary>
    [JsonPropertyName("_id")]
    public string? Id { get; set; }
    
    /// <summary>
    /// Template name
    /// </summary>
    public string? Name { get; set; }
    
    /// <summary>
    /// Number of QR codes using this template
    /// </summary>
    public int? NumberOfSubQrCodes { get; set; }
    
    /// <summary>
    /// Number of short links using this template
    /// </summary>
    public int? NumberOfSubShortLinks { get; set; }
    
    /// <summary>
    /// QR code download URL
    /// </summary>
    public string? QrCodeDownloadURL { get; set; }
}

/// <summary>
/// Short link metadata information
/// </summary>
public class ShortLinkMetaDataModel
{
    /// <summary>
    /// Meta image URL
    /// </summary>
    public string? Image { get; set; }
    
    /// <summary>
    /// Meta title
    /// </summary>
    public string? Title { get; set; }
    
    /// <summary>
    /// Meta description
    /// </summary>
    public string? Description { get; set; }
}

/// <summary>
/// Preview reason (moderation score)
/// </summary>
public class ShortLinkPreviewReasonModel
{
    /// <summary>
    /// Category name
    /// </summary>
    public string Category { get; set; } = string.Empty;
    
    /// <summary>
    /// Score value
    /// </summary>
    public double Score { get; set; }
}

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
    /// Number of visits to the short link. Also returned in API-key list
    /// results from the API's link-qr truth pass on (earlier: get one link at a time).
    /// </summary>
    public int? NumberOfVisitors { get; set; }
    
    /// <summary>
    /// Number of reports/flags
    /// </summary>
    public int? NumberOfReports { get; set; }
    
    /// <summary>
    /// iOS deep link URL support
    /// </summary>
    public bool? IsSupportIOSDeepUrl { get; set; }
    
    /// <summary>
    /// Android deep link URL support
    /// </summary>
    public bool? IsSupportAndroidDeepUrl { get; set; }
    
   
    
    /// <summary>
    /// QR code template name
    /// </summary>
    public string? QrCodeTemplateName { get; set; }
    
    /// <summary>
    /// Whether visitors see an interstitial page with <see cref="PageInfo"/>'s
    /// title and description instead of a direct redirect. Also returned in
    /// API-key list results from the API's link-qr truth pass on.
    /// </summary>
    public bool? IsEnableLandingPage { get; set; }

    /// <summary>
    /// Never returned by the API; always <c>null</c>.
    /// </summary>
    [Obsolete(ShortLinkConst.MonetizationObsolete)]
    [JsonIgnore]
    public bool? IsEnableMonetization { get; set; }

 
 
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

    /// <summary>
    /// Review status. Also returned in API-key list results from the API's
    /// link-qr truth pass on.
    /// </summary>
    public ShortLinkStatusType? Status { get; set; }
}

/// <summary>
/// Short link full details model with all populated fields
/// </summary>
public class ShortLinkFullDetailsModel : ShortLinkModel
{
    /// <summary>
    /// Android deep link URL. Returned to the owner by <c>GetAsync</c> from the
    /// API's link-qr truth pass on (earlier only the create response carried it).
    /// </summary>
    public string? AndroidUrl { get; set; }

    /// <summary>
    /// iOS deep link URL. Returned to the owner by <c>GetAsync</c> from the
    /// API's link-qr truth pass on (earlier only the create response carried it).
    /// </summary>
    public string? IosUrl { get; set; }
    
    
    /// <summary>
    /// Template type
    /// </summary>
    public string? TemplateType { get; set; }
    
    /// <summary>
    /// Template object (populated)
    /// </summary>
    public QRCodeTemplateModel? Template { get; set; }
     
    /// <summary>
    /// Link metadata for social sharing
    /// </summary>
    public ShortLinkMetaDataModel? LinkMetaData { get; set; }
       
}

/// <summary>
/// Create short link request
/// </summary>
public class ShortLinkCreateRequestModel
{
    /// <summary>
    /// Link name. Empty: the API names the link from the target page's title.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Base URL (the target URL to redirect to). Required; must start with
    /// <c>http://</c> or <c>https://</c> from the API's link-qr truth pass on.
    /// </summary>
    public required string BaseUrl { get; set; }

    /// <summary>
    /// QR code template ID. Required for API-key callers, which every SDK call
    /// is: the API answers "Template Id Is Required" without it. Your template
    /// IDs are on the dashboard's QR code templates page.
    /// </summary>
    public required string TemplateId { get; set; }

    /// <summary>
    /// External reference ID for filtering/tracking
    /// </summary>
    public string? RefId { get; set; }

    /// <summary>
    /// Custom tag for filtering/categorization
    /// </summary>
    public string? Tag { get; set; }

    /// <summary>
    /// Custom landing page ID: 4-32 lowercase letters, digits or hyphens.
    /// Starter plan and above; a lower plan is refused by the API.
    /// </summary>
    public string? CustomLandingId { get; set; }

    /// <summary>
    /// <c>true</c>: visitors see an interstitial page with
    /// <see cref="PageInfo"/>'s title and description and a Continue button,
    /// instead of being redirected straight away. Omitted (<c>null</c>): the
    /// API stores <c>false</c>.
    /// </summary>
    public bool? IsEnableLandingPage { get; set; }

    /// <summary>
    /// Landing page title and description. Both are required when
    /// <see cref="IsEnableLandingPage"/> is <c>true</c>.
    /// </summary>
    public ShortLinkPageInfoModel? PageInfo { get; set; }

    /// <summary>
    /// Android deep link, opened instead of <see cref="BaseUrl"/> on Android.
    /// </summary>
    /// <remarks>
    /// <para>Allowed schemes: <c>https:</c>, <c>http:</c> or an app scheme matching
    /// <c>^[a-z][a-z0-9+.-]*:</c> (for example <c>myapp://item/1</c>) - never
    /// <c>javascript:</c>, <c>data:</c>, <c>vbscript:</c>, <c>file:</c>,
    /// <c>about:</c> or <c>blob:</c>; the API refuses those with
    /// "The deep link URL is not allowed".</para>
    /// <para>Create: a supplied value wins; an empty or absent value falls back
    /// to the target page's <c>al:android:url</c> meta tag.
    /// <see cref="ShortLinkModel.IsSupportAndroidDeepUrl"/> is true exactly when
    /// the link has an Android URL.</para>
    /// <para>Accepted from the API's link-qr truth pass (S13) on; an older API
    /// answers 400 to a request that sets it.</para>
    /// </remarks>
    public string? AndroidUrl { get; set; }

    /// <summary>
    /// iOS deep link, opened instead of <see cref="BaseUrl"/> on iOS.
    /// </summary>
    /// <remarks>
    /// Same scheme rule and create precedence as <see cref="AndroidUrl"/>; the
    /// fallback is the target page's <c>al:ios:url</c> meta tag, and
    /// <see cref="ShortLinkModel.IsSupportIOSDeepUrl"/> follows it.
    /// </remarks>
    public string? IosUrl { get; set; }

    /// <summary>
    /// Never accepted by the API; ignored and never sent.
    /// </summary>
    [Obsolete(ShortLinkConst.MonetizationObsolete)]
    [JsonIgnore]
    public bool? IsEnableMonetization { get; set; }
}

/// <summary>
/// Update short link request
/// </summary>
/// <remarks>
/// A property left <c>null</c> is not sent. <see cref="BaseUrl"/> and
/// <see cref="TemplateId"/> are required on every update.
/// </remarks>
public class ShortLinkUpdateRequestModel
{
    /// <summary>
    /// Link name. <c>null</c> or empty: the API names the link from the target
    /// page's title.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Base URL (the target URL to redirect to). Required on every update - send
    /// the current one to keep it. Must start with <c>http://</c> or
    /// <c>https://</c> from the API's link-qr truth pass on.
    /// </summary>
    public required string BaseUrl { get; set; }

    /// <summary>
    /// QR code template ID. Required for API-key callers, which every SDK call
    /// is: the API answers "Template Id Is Required" without it.
    /// </summary>
    public required string TemplateId { get; set; }

    /// <summary>
    /// External reference ID for filtering/tracking. <c>null</c> keeps the stored value.
    /// </summary>
    public string? RefId { get; set; }

    /// <summary>
    /// Custom tag for filtering/categorization. <c>null</c> keeps the stored value.
    /// </summary>
    public string? Tag { get; set; }

    /// <summary>
    /// Turn the landing page on or off. <c>null</c> (the default) is not sent,
    /// and the API keeps the stored value - from the API's link-qr truth pass
    /// on; an older API turned the landing page off when the key was omitted.
    /// </summary>
    public bool? IsEnableLandingPage { get; set; }

    /// <summary>
    /// Landing page title and description. Both are required when
    /// <see cref="IsEnableLandingPage"/> is <c>true</c>.
    /// </summary>
    public ShortLinkPageInfoModel? PageInfo { get; set; }

    /// <summary>
    /// Android deep link. Scheme rule as on
    /// <see cref="ShortLinkCreateRequestModel.AndroidUrl"/>.
    /// </summary>
    /// <remarks>
    /// Precedence on update: a value is sent and wins, and <c>""</c> clears the
    /// stored deep link. <c>null</c> (the default) omits the key: if
    /// <see cref="BaseUrl"/> changed, the API re-derives the deep link from the
    /// new target page's <c>al:android:url</c> meta tag; if it did not, the
    /// stored value is kept. Accepted from the API's link-qr truth pass (S13) on.
    /// </remarks>
    public string? AndroidUrl { get; set; }

    /// <summary>
    /// iOS deep link. Same scheme rule and update precedence as
    /// <see cref="AndroidUrl"/>, re-derived from <c>al:ios:url</c>.
    /// </summary>
    public string? IosUrl { get; set; }

    /// <summary>
    /// Never accepted by the API; ignored and never sent.
    /// </summary>
    [Obsolete(ShortLinkConst.MonetizationObsolete)]
    [JsonIgnore]
    public bool? IsEnableMonetization { get; set; }
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
    /// Search by landing page title (sent as <c>pageInfo.title</c>)
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
    /// No such filter exists; ignored and never sent.
    /// </summary>
    [Obsolete(ShortLinkConst.MonetizationObsolete)]
    [JsonIgnore]
    public bool? IsEnableMonetization { get; set; }

    /// <summary>
    /// The API's short-link search has no generic search term; never sent.
    /// Use <see cref="Name"/>, <see cref="BaseUrl"/> or <see cref="PageInfoTitle"/>.
    /// </summary>
    [Obsolete(ShortLinkConst.IgnoredFilterObsolete)]
    [JsonIgnore]
    public string? Search { get; set; }

    /// <summary>
    /// The API's short-link search has no date range; never sent.
    /// </summary>
    [Obsolete(ShortLinkConst.IgnoredFilterObsolete)]
    [JsonIgnore]
    public DateTime? FromDate { get; set; }

    /// <summary>
    /// The API's short-link search has no date range; never sent.
    /// </summary>
    [Obsolete(ShortLinkConst.IgnoredFilterObsolete)]
    [JsonIgnore]
    public DateTime? ToDate { get; set; }
}

[JsonConverter(typeof(StringValueObjectConverter<ShortLinkStatusType>))]
public readonly record struct ShortLinkStatusType (string Value)
{
    public static readonly ShortLinkStatusType New = new("new");
    public static readonly ShortLinkStatusType Pending = new("pending");
    public static readonly ShortLinkStatusType Rejected = new("rejected");
    public static readonly ShortLinkStatusType Approved = new("approved");

    public override string ToString ( ) => Value;
}

/// <summary>
/// Account-wide short-link statistics (<c>GET /api/short-link/statistics</c>):
/// the resolved <see cref="LinkStatisticsResponse{TData}.Range"/> plus
/// <see cref="ShortLinkStatisticsDataModel"/>.
/// </summary>
public class ShortLinkStatisticsModel : LinkStatisticsResponse<ShortLinkStatisticsDataModel>
{
}

/// <summary>The <c>data</c> of <see cref="ShortLinkStatisticsModel"/>.</summary>
public class ShortLinkStatisticsDataModel
{
    /// <summary>Lifetime link count and counters, plus the visit totals in the range.</summary>
    public ShortLinkStatisticsTotalsModel Totals { get; set; } = new();

    /// <summary>One row per UTC day that had a link created or a visit, oldest first.</summary>
    public List<LinkStatisticsDailyRow> Daily { get; set; } = new();

    /// <summary>The ten links with the most visits in the range, most first; links with no visit in the range are left out.</summary>
    public List<ShortLinkStatisticsTopLinkModel> TopLinks { get; set; } = new();
}

/// <summary>Totals of <see cref="ShortLinkStatisticsDataModel"/>.</summary>
public class ShortLinkStatisticsTotalsModel : LinkStatisticsVisitTotals
{
    /// <summary>Your short links (lifetime, deleted ones excluded).</summary>
    public long TotalLinks { get; set; }

    /// <summary><see cref="LinkStatisticsVisitTotals.TotalVisitors"/> / <see cref="TotalLinks"/> (0 with no links).</summary>
    public double AvgVisitorsPerLink { get; set; }
}

/// <summary>One of <see cref="ShortLinkStatisticsDataModel.TopLinks"/>.</summary>
public class ShortLinkStatisticsTopLinkModel
{
    /// <summary>Database ID</summary>
    [JsonPropertyName("_id")]
    public string? Id { get; set; }

    /// <summary>Link name</summary>
    public string? Name { get; set; }

    /// <summary>Destination URL</summary>
    public string? BaseUrl { get; set; }

    /// <summary>The short code</summary>
    public string? ShortLinkId { get; set; }

    /// <summary>Lifetime visit counter</summary>
    public long? NumberOfVisitors { get; set; }

    /// <summary>Creation time</summary>
    public DateTime? CreatedAt { get; set; }

    /// <summary>Visits by people in the range (bots excluded).</summary>
    public long VisitsInRange { get; set; }
}

/// <summary>
/// Delete response model
/// </summary>
public class DeleteResponse
{
    /// <summary>
    /// Success message
    /// </summary>
    public string Message { get; set; } = string.Empty;
}