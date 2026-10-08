using System.Text.Json.Serialization;

namespace Posty5.ShortLink.Models;

/// <summary>Palette keys a campaign may carry.</summary>
public static class LinkCampaignColors
{
    /// <summary>slate</summary>
    public const string Slate = "slate";
    /// <summary>red</summary>
    public const string Red = "red";
    /// <summary>orange</summary>
    public const string Orange = "orange";
    /// <summary>amber</summary>
    public const string Amber = "amber";
    /// <summary>green</summary>
    public const string Green = "green";
    /// <summary>teal</summary>
    public const string Teal = "teal";
    /// <summary>blue</summary>
    public const string Blue = "blue";
    /// <summary>indigo</summary>
    public const string Indigo = "indigo";
    /// <summary>purple</summary>
    public const string Purple = "purple";
    /// <summary>pink</summary>
    public const string Pink = "pink";
}

/// <summary>A link campaign as the API returns it.</summary>
public class LinkCampaignModel : Posty5.Core.Models.IVersioned
{
    /// <summary>The document's version (<c>__v</c>); pass it to the next update or delete.</summary>
    [JsonPropertyName("__v")]
    public long Version { get; set; }

    /// <summary>Database ID</summary>
    [JsonPropertyName("_id")]
    public string? Id { get; set; }
    /// <summary>Owner</summary>
    public string? UserId { get; set; }
    /// <summary>API key that created it</summary>
    public string? ApiKeyId { get; set; }
    /// <summary>Name</summary>
    public string Name { get; set; } = string.Empty;
    /// <summary>Description</summary>
    public string? Description { get; set; }
    /// <summary>Value of <see cref="LinkCampaignColors"/></summary>
    public string? Color { get; set; }
    /// <summary>Default UTM copied into a link's empty UTM fields when it is saved with the campaign.</summary>
    public LinkUtmModel? Utm { get; set; }
    /// <summary>When archived</summary>
    public DateTimeOffset? ArchivedAt { get; set; }
    /// <summary>Archived (hidden from pickers; links keep working)</summary>
    public bool IsArchived { get; set; }
    /// <summary>Source</summary>
    public string? CreatedFrom { get; set; }
    /// <summary>Created</summary>
    public DateTimeOffset? CreatedAt { get; set; }
    /// <summary>Updated</summary>
    public DateTimeOffset? UpdatedAt { get; set; }
}

/// <summary><see cref="LinkCampaignClient.GetAsync"/>: the campaign with its link totals.</summary>
public class LinkCampaignDetailsModel : LinkCampaignModel
{
    /// <summary>Links in the campaign</summary>
    public long LinkCount { get; set; }
    /// <summary>Sum of the links' <c>numberOfVisitors</c>.</summary>
    public long TotalVisits { get; set; }
}

/// <summary><c>POST /api/link-campaign</c>. Feature key <c>urlShortener.campaigns</c> (plan-gated).</summary>
public class LinkCampaignCreateRequestModel
{
    /// <summary>Name, trimmed</summary>
    public required string Name { get; set; }
    /// <summary>Description</summary>
    public string? Description { get; set; }
    /// <summary>Value of <see cref="LinkCampaignColors"/></summary>
    public string? Color { get; set; }
    /// <summary>Default UTM</summary>
    public LinkUtmModel? Utm { get; set; }
    /// <summary><c>true</c> archives</summary>
    public bool? Archived { get; set; }
}

/// <summary><c>PUT /api/link-campaign/{id}</c>. A <c>null</c> property is not sent and keeps the stored value.</summary>
public class LinkCampaignUpdateRequestModel
{
    /// <summary>Name</summary>
    public string? Name { get; set; }
    /// <summary>Description</summary>
    public string? Description { get; set; }
    /// <summary>Value of <see cref="LinkCampaignColors"/></summary>
    public string? Color { get; set; }
    /// <summary>Default UTM</summary>
    public LinkUtmModel? Utm { get; set; }
    /// <summary>Sends <c>"utm": null</c> (removes the default UTM). Wins over <see cref="Utm"/>.</summary>
    public bool ClearUtm { get; set; }
    /// <summary><c>true</c> archives, <c>false</c> restores.</summary>
    public bool? Archived { get; set; }
}

/// <summary><c>GET /api/link-campaign</c> filters.</summary>
public class LinkCampaignListParamsModel
{
    /// <summary>Archived or active only</summary>
    public bool? Archived { get; set; }
    /// <summary>Name search</summary>
    public string? Term { get; set; }
}
