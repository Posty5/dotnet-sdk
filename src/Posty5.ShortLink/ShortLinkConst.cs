namespace Posty5.ShortLink;

/// <summary>
/// Fixed values <see cref="ShortLinkClient"/> sends to <c>/api/short-link</c>.
/// </summary>
internal static class ShortLinkConst
{
    /// <summary>The API path every short-link call is made under.</summary>
    public const string BasePath = "/api/short-link";

    /// <summary>
    /// <c>templateType</c> on create and update. The API stores it as sent, and
    /// an update that omits it clears the stored value, so update sends it too.
    /// </summary>
    public const string TemplateType = "user";

    /// <summary>
    /// List filter key for <see cref="Models.ShortLinkListParamsModel.PageInfoTitle"/>.
    /// SDK 3.0.0 sent <c>pageinfo.title</c>, which matches no field.
    /// </summary>
    public const string PageInfoTitleFilterKey = "pageInfo.title";

    /// <summary><c>[Obsolete]</c> text on every <c>IsEnableMonetization</c> (TP-D5).</summary>
    public const string MonetizationObsolete =
        "Never accepted by the API; ignored by this SDK and never sent. Removed in the next major.";

    /// <summary><c>[Obsolete]</c> text on list filters the API does not read.</summary>
    public const string IgnoredFilterObsolete =
        "The API ignores this filter, so this SDK no longer sends it. Removed in the next major.";

    /// <summary><c>[Obsolete]</c> text on request <c>Tag</c> properties.</summary>
    public const string TagObsolete = "Use Tags. The API treats Tag as Tags[0]; when both are sent, Tags wins.";

    /// <summary>Sub-path of the tag list route.</summary>
    public const string TagsPath = "/tags";

    /// <summary>Sub-path of the health check route under <c>/{id}</c>.</summary>
    public const string HealthCheckPath = "/health-check";

    /// <summary>Separator of <c>?tags=a,b</c>.</summary>
    public const string TagsQuerySeparator = ",";

    /// <summary>Base path of the link campaign routes.</summary>
    public const string LinkCampaignBasePath = "/api/link-campaign";
}
