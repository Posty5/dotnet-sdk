using System.Text.Json;
using System.Text.Json.Serialization;

namespace Posty5.SocialPublisherWorkspace.Models;

// ============================================================================
// CONNECTED SOCIAL ACCOUNTS (Posty5.SocialPublisherWorkspace 3.1.0+)
//
// Transcribed from the API's /swagger.json (`/api/social-publisher-account`).
// Read-only: connecting an account is an OAuth sign-in done in the dashboard.
// ============================================================================

/// <summary>
/// A connected social account, as the list returns it.
/// </summary>
public class SocialPublisherAccountSampleDetailsModel
{
    /// <summary>The account id — the <c>AccountId</c> the post methods' <c>...ToAccountAsync</c> variants take.</summary>
    [JsonPropertyName("_id")]
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// The platform: <c>youtube</c>, <c>tiktok</c>, <c>facebook</c>,
    /// <c>instagram</c>, <c>threads</c> or <c>twitter</c>.
    /// </summary>
    public string Platform { get; set; } = string.Empty;

    /// <summary>
    /// <c>active</c>, <c>inactive</c> or <c>authenticationExpired</c> — the last
    /// means it must be reconnected in the dashboard before it can publish.
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Instagram only: how the account was connected, <c>facebook_page</c> or
    /// <c>instagram_login</c>.
    /// </summary>
    public string? AuthSource { get; set; }

    /// <summary>The account's display name.</summary>
    public string? Name { get; set; }

    /// <summary>The account's picture URL.</summary>
    public string? Thumbnail { get; set; }

    /// <summary>A link to the account on its platform.</summary>
    public string? Link { get; set; }

    /// <summary>When the account was connected.</summary>
    public DateTime? CreatedAt { get; set; }
}

/// <summary>
/// A connected social account with its platform profile and its publishing
/// defaults.
/// </summary>
public class SocialPublisherAccountDetailsModel : SocialPublisherAccountSampleDetailsModel
{
    /// <summary>
    /// The account's default post settings, as the API stores them (per post
    /// kind — <c>video</c>, <c>image</c> — and per platform). Null when none
    /// were saved.
    /// </summary>
    public JsonElement? DefaultPostSettings { get; set; }

    /// <summary>The comments the composer pre-fills for this account, in posting order.</summary>
    public List<JsonElement>? DefaultComments { get; set; }

    /// <summary>
    /// Every other field the API returned — notably the platform's own profile
    /// block (e.g. <c>youtube_channelInfo</c>, <c>facebook_pageInfo</c>).
    /// </summary>
    [JsonExtensionData]
    public Dictionary<string, object>? Extra { get; set; }
}

/// <summary>Filters for listing connected accounts. Every field is optional.</summary>
public class SocialPublisherAccountListParamsModel
{
    /// <summary>
    /// Only this platform: <c>youtube</c>, <c>tiktok</c>, <c>facebook</c>,
    /// <c>instagram</c>, <c>threads</c> or <c>twitter</c>.
    /// </summary>
    public string? Platform { get; set; }

    /// <summary>Only this status, e.g. <c>active</c> or <c>authenticationExpired</c>.</summary>
    public string? Status { get; set; }

    /// <summary>Name contains this, case-insensitive.</summary>
    public string? Name { get; set; }
}

/// <summary>One match from the account lookup.</summary>
public class SocialPublisherAccountLookupItemModel
{
    /// <summary>The account id.</summary>
    [JsonPropertyName("_id")]
    public string Id { get; set; } = string.Empty;

    /// <summary>The account's display name.</summary>
    public string? Name { get; set; }

    /// <summary>Extra data for a picker.</summary>
    public SocialPublisherAccountLookupDataModel? Data { get; set; }
}

/// <summary>The extra data a lookup match carries.</summary>
public class SocialPublisherAccountLookupDataModel
{
    /// <summary>The account's picture URL.</summary>
    public string? Img { get; set; }
}
