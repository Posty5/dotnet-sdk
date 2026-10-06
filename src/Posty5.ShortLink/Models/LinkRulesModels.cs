using System.Text.Json.Serialization;

namespace Posty5.ShortLink.Models;

// Short link controls (access, routing, A/B variants, UTM, pixels, health) as the
// API's Joi schemas accept them (api: @posty5/shared/shared-area/link-rules/joi.ts,
// short-link/core/services/campaign-tools/schema.ts). Mirrors npm @posty5/short-link.

/// <summary>Device types a routing rule matches (tablet, then mobile, then desktop, then other).</summary>
public static class LinkDeviceTypes
{
    /// <summary>Tablet</summary>
    public const string Tablet = "tablet";
    /// <summary>Phone</summary>
    public const string Mobile = "mobile";
    /// <summary>Desktop</summary>
    public const string Desktop = "desktop";
    /// <summary>Anything else</summary>
    public const string Other = "other";
}

/// <summary>OS families a routing rule matches.</summary>
public static class LinkOsFamilies
{
    /// <summary>Android</summary>
    public const string Android = "android";
    /// <summary>iOS / iPadOS</summary>
    public const string Ios = "ios";
    /// <summary>Windows</summary>
    public const string Windows = "windows";
    /// <summary>macOS</summary>
    public const string MacOs = "macos";
    /// <summary>Linux</summary>
    public const string Linux = "linux";
}

/// <summary>Retargeting pixel providers.</summary>
public static class LinkPixelProviders
{
    /// <summary>Meta (Facebook) pixel</summary>
    public const string Meta = "meta";
    /// <summary>Google Ads tag</summary>
    public const string GoogleAds = "googleAds";
    /// <summary>TikTok pixel</summary>
    public const string TikTok = "tiktok";
    /// <summary>LinkedIn Insight tag</summary>
    public const string LinkedIn = "linkedin";
    /// <summary>X (Twitter) pixel</summary>
    public const string X = "x";
    /// <summary>Pinterest tag</summary>
    public const string Pinterest = "pinterest";
}

/// <summary>Destination health states.</summary>
public static class LinkHealthStatuses
{
    /// <summary>Not checked yet</summary>
    public const string Unknown = "unknown";
    /// <summary>Last checks succeeded</summary>
    public const string Healthy = "healthy";
    /// <summary>Destination is failing</summary>
    public const string Unhealthy = "unhealthy";
}

/// <summary>
/// Start / stop / limit / password of a link, as sent. On update the API merges
/// <c>access</c> field by field: a <c>null</c> property is not sent and keeps the
/// stored value. Use the <c>Clear…</c> flags and <see cref="RemovePassword"/> to
/// send an explicit JSON <c>null</c> for one field.
/// </summary>
/// <remarks>
/// A class, not a record: <see cref="ToString"/> never includes <see cref="Password"/>.
/// </remarks>
public class LinkAccessInputModel
{
    /// <summary>The link answers "not yet active" before this moment (ISO 8601 with offset on the wire).</summary>
    public DateTimeOffset? ActiveFrom { get; set; }

    /// <summary>Sends <c>"activeFrom": null</c> (clears it).</summary>
    public bool ClearActiveFrom { get; set; }

    /// <summary>After <see cref="ActiveFrom"/>; the link answers "expired" after it.</summary>
    public DateTimeOffset? ExpiresAt { get; set; }

    /// <summary>Sends <c>"expiresAt": null</c> (clears it).</summary>
    public bool ClearExpiresAt { get; set; }

    /// <summary>1 - 10,000,000 visits (people, never bots).</summary>
    public int? MaxVisits { get; set; }

    /// <summary>Sends <c>"maxVisits": null</c> (clears it).</summary>
    public bool ClearMaxVisits { get; set; }

    /// <summary>http(s) URL a stopped visit goes to instead of the unavailable page. <c>""</c> clears it.</summary>
    public string? FallbackUrl { get; set; }

    /// <summary>
    /// Write-only, 4 - 128 characters; never returned (responses carry
    /// <see cref="LinkAccessModel.HasPassword"/>). <c>null</c> keeps the stored
    /// password. Feature key <c>urlShortener.passwordProtection</c> (plan-gated).
    /// </summary>
    public string? Password { get; set; }

    /// <summary>
    /// Sends <c>"password": null</c>, which removes the password. Wins over
    /// <see cref="Password"/>. Needed because the SDK's serializer omits null properties.
    /// </summary>
    public bool RemovePassword { get; set; }

    /// <summary>Never includes the password.</summary>
    public override string ToString() =>
        $"LinkAccessInputModel {{ ActiveFrom = {ActiveFrom}, ExpiresAt = {ExpiresAt}, MaxVisits = {MaxVisits}, FallbackUrl = {FallbackUrl}, Password = {(Password != null ? "***" : "<unchanged>")}, RemovePassword = {RemovePassword} }}";
}

/// <summary><c>access</c> as responses carry it: never the password, only <see cref="HasPassword"/>.</summary>
public class LinkAccessModel
{
    /// <summary>Start of the active window</summary>
    public DateTimeOffset? ActiveFrom { get; set; }
    /// <summary>End of the active window</summary>
    public DateTimeOffset? ExpiresAt { get; set; }
    /// <summary>Visit limit</summary>
    public int? MaxVisits { get; set; }
    /// <summary>Where a stopped visit goes</summary>
    public string? FallbackUrl { get; set; }
    /// <summary>Whether a password is set</summary>
    public bool? HasPassword { get; set; }
}

/// <summary>A weekly window in an IANA time zone; <c>To &lt; From</c> is overnight.</summary>
public class LinkTimeWindowModel
{
    /// <summary>1 - 7 unique days, 0 (Sunday) - 6.</summary>
    public List<int> Days { get; set; } = new();
    /// <summary><c>HH:mm</c></summary>
    public string From { get; set; } = string.Empty;
    /// <summary><c>HH:mm</c>, different from <see cref="From"/></summary>
    public string To { get; set; } = string.Empty;
    /// <summary>IANA time zone, e.g. <c>Europe/Berlin</c></summary>
    public string Tz { get; set; } = string.Empty;
}

/// <summary>AND across kinds, OR within a list; at least one kind is required.</summary>
public class LinkRoutingConditionsModel
{
    /// <summary>ISO 3166-1 alpha-2 codes (max 250).</summary>
    public List<string>? Countries { get; set; }
    /// <summary>Values of <see cref="LinkDeviceTypes"/>.</summary>
    public List<string>? Devices { get; set; }
    /// <summary>Values of <see cref="LinkOsFamilies"/>.</summary>
    public List<string>? Os { get; set; }
    /// <summary>2-3 letter lower-case language codes (max 50).</summary>
    public List<string>? Languages { get; set; }
    /// <summary>Weekly time window</summary>
    public LinkTimeWindowModel? TimeWindow { get; set; }
}

/// <summary>One ordered routing rule; the first match wins.</summary>
public class LinkRoutingRuleModel
{
    /// <summary>8 lower-case letters/digits; omit on create (the API assigns one), keep it on update.</summary>
    public string? Id { get; set; }
    /// <summary>Max 60 characters</summary>
    public string? Name { get; set; }
    /// <summary>Match conditions</summary>
    public LinkRoutingConditionsModel Conditions { get; set; } = new();
    /// <summary>http(s) URL</summary>
    public string TargetUrl { get; set; } = string.Empty;
}

/// <summary>One A/B variant; weights are relative (1 - 100).</summary>
public class LinkVariantModel
{
    /// <summary>8 lower-case letters/digits; omit on create, keep it on update.</summary>
    public string? Id { get; set; }
    /// <summary>Max 40 characters</summary>
    public string? Name { get; set; }
    /// <summary>http(s) URL</summary>
    public string Url { get; set; } = string.Empty;
    /// <summary>1 - 100</summary>
    public int Weight { get; set; }
}

/// <summary>UTM parameters appended to the destination; each max 100 chars, no spaces, quotes or <c>&lt;&gt;</c>.</summary>
public class LinkUtmModel
{
    /// <summary>utm_source</summary>
    public string? Source { get; set; }
    /// <summary>utm_medium</summary>
    public string? Medium { get; set; }
    /// <summary>utm_campaign</summary>
    public string? Campaign { get; set; }
    /// <summary>utm_term</summary>
    public string? Term { get; set; }
    /// <summary>utm_content</summary>
    public string? Content { get; set; }
}

/// <summary>One retargeting pixel; one per provider, max 5.</summary>
public class LinkPixelModel
{
    /// <summary>Value of <see cref="LinkPixelProviders"/>.</summary>
    public string Provider { get; set; } = string.Empty;
    /// <summary>Pixel id, checked against the provider's pattern.</summary>
    public string Id { get; set; } = string.Empty;
}

/// <summary>Destination health monitoring switch (the rest of <c>health</c> is system-written).</summary>
public class LinkHealthInputModel
{
    /// <summary>Turn monitoring on or off. Feature key <c>healthMonitor</c>.</summary>
    public bool Enabled { get; set; }
}

/// <summary>Last health check result.</summary>
public class LinkHealthLastResultModel
{
    /// <summary>Checked URL</summary>
    public string? Url { get; set; }
    /// <summary>HTTP status</summary>
    public int? HttpStatus { get; set; }
    /// <summary>Network error code</summary>
    public string? ErrorCode { get; set; }
}

/// <summary>Destination health monitor state of a link (list rows carry <see cref="Status"/> only).</summary>
public class LinkHealthModel
{
    /// <summary>Monitoring on</summary>
    public bool? Enabled { get; set; }
    /// <summary>Value of <see cref="LinkHealthStatuses"/>.</summary>
    public string? Status { get; set; }
    /// <summary>Last check</summary>
    public DateTimeOffset? CheckedAt { get; set; }
    /// <summary>Last successful check</summary>
    public DateTimeOffset? LastOkAt { get; set; }
    /// <summary>Failing since</summary>
    public DateTimeOffset? FailingSince { get; set; }
    /// <summary>Consecutive failures</summary>
    public int? ConsecutiveFailures { get; set; }
    /// <summary>Last result</summary>
    public LinkHealthLastResultModel? LastResult { get; set; }
}

/// <summary>
/// Rule sections shared by create, update and <see cref="ShortLinkClient.SetRulesAsync"/>.
/// A <c>null</c> property is not sent and leaves the section untouched; an empty
/// list or a <c>Clear…</c> flag clears it.
/// </summary>
public class LinkRulesSectionsModel
{
    /// <summary>Access rules, merged field by field on update.</summary>
    public LinkAccessInputModel? Access { get; set; }
    /// <summary>Sends <c>"access": null</c>: clears every access rule. Wins over <see cref="Access"/>.</summary>
    public bool ClearAccess { get; set; }
    /// <summary>Max 20 ordered routing rules; empty clears.</summary>
    public IReadOnlyList<LinkRoutingRuleModel>? Routing { get; set; }
    /// <summary>0 or 2 - 5 A/B variants, at least one URL different; empty clears.</summary>
    public IReadOnlyList<LinkVariantModel>? Variants { get; set; }
    /// <summary>UTM parameters. Feature key <c>urlShortener.utmBuilder</c>.</summary>
    public LinkUtmModel? Utm { get; set; }
    /// <summary>Sends <c>"utm": null</c>. Wins over <see cref="Utm"/>.</summary>
    public bool ClearUtm { get; set; }
    /// <summary>Max 5 pixels, one per provider; empty clears. Feature key <c>urlShortener.retargetingPixels</c>.</summary>
    public IReadOnlyList<LinkPixelModel>? Pixels { get; set; }
    /// <summary>Must be <c>true</c> the first time pixels are set (lawful-basis attestation).</summary>
    public bool? PixelsConsentAcknowledged { get; set; }
}

/// <summary>
/// Input of <see cref="ShortLinkClient.SetRulesAsync"/>. Partial: omitted sections are untouched.
/// </summary>
public class LinkRulesUpdateModel : LinkRulesSectionsModel
{
    /// <summary>The link's destination; read from the link when omitted (update requires it).</summary>
    public string? BaseUrl { get; set; }
    /// <summary>The link's QR template; read from the link when omitted (update requires it).</summary>
    public string? TemplateId { get; set; }
}

/// <summary>Short link controls accepted by create and update on top of the rule sections.</summary>
public class ShortLinkControlsRequestModel : LinkRulesSectionsModel
{
    /// <summary>Max 10 tags, each 1 - 40 chars, unique case-insensitively. Empty removes them.</summary>
    public IReadOnlyList<string>? Tags { get; set; }
    /// <summary>Campaign (24-hex id) the link belongs to; <c>""</c> detaches. Feature key <c>urlShortener.campaigns</c>.</summary>
    public string? CampaignId { get; set; }
    /// <summary>Destination health monitoring. Feature key <c>healthMonitor</c>.</summary>
    public LinkHealthInputModel? Health { get; set; }
}
