using Posty5.ShortLink.Models;

namespace Posty5.ShortLink.Helpers;

/// <summary>
/// Builds short-link request bodies as dictionaries. Posty5.Core serializes with
/// <c>WhenWritingNull</c>, which omits null properties of objects but keeps null
/// dictionary values - so a dictionary is how an explicit JSON <c>null</c>
/// (clear a section, remove a password) reaches the API. Keys are written camelCase
/// here because the naming policy does not apply to dictionary keys.
/// </summary>
internal static class ShortLinkRequestBodyHelper
{
    /// <summary>Adds <paramref name="value"/> under <paramref name="key"/> when it is not null.</summary>
    public static void AddIfSet(Dictionary<string, object?> body, string key, object? value)
    {
        if (value != null)
            body[key] = value;
    }

    /// <summary>Adds the rule sections of <paramref name="rules"/> (clear flags as JSON null).</summary>
    public static void AddRuleSections(Dictionary<string, object?> body, LinkRulesSectionsModel rules)
    {
        if (rules.ClearAccess)
            body["access"] = null;
        else if (rules.Access != null)
            body["access"] = ToAccessBody(rules.Access);

        AddIfSet(body, "routing", rules.Routing);
        AddIfSet(body, "variants", rules.Variants);

        if (rules.ClearUtm)
            body["utm"] = null;
        else
            AddIfSet(body, "utm", rules.Utm);

        AddIfSet(body, "pixels", rules.Pixels);
        AddIfSet(body, "pixelsConsentAcknowledged", rules.PixelsConsentAcknowledged);
    }

    /// <summary>Adds tags, campaign, health and the rule sections.</summary>
    public static void AddControls(Dictionary<string, object?> body, ShortLinkControlsRequestModel controls)
    {
        AddIfSet(body, "tags", controls.Tags);
        AddIfSet(body, "campaignId", controls.CampaignId);
        AddIfSet(body, "health", controls.Health);
        AddRuleSections(body, controls);
    }

    /// <summary><c>access</c> with explicit nulls for the clear flags and <c>RemovePassword</c>.</summary>
    public static Dictionary<string, object?> ToAccessBody(LinkAccessInputModel access)
    {
        var body = new Dictionary<string, object?>();
        if (access.ClearActiveFrom) body["activeFrom"] = null; else AddIfSet(body, "activeFrom", access.ActiveFrom);
        if (access.ClearExpiresAt) body["expiresAt"] = null; else AddIfSet(body, "expiresAt", access.ExpiresAt);
        if (access.ClearMaxVisits) body["maxVisits"] = null; else AddIfSet(body, "maxVisits", access.MaxVisits);
        AddIfSet(body, "fallbackUrl", access.FallbackUrl);
        if (access.RemovePassword) body["password"] = null; else AddIfSet(body, "password", access.Password);
        return body;
    }

    /// <summary>Joins list-filter tags with the API's separator; null when there are none.</summary>
    public static string? JoinTags(IReadOnlyList<string>? tags) =>
        tags is { Count: > 0 } ? string.Join(ShortLinkConst.TagsQuerySeparator, tags) : null;
}
