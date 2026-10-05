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
    /// <c>createdFrom</c> on create and update. The API defaults an omitted
    /// <c>createdFrom</c> to <c>"api"</c> on update as well as on create.
    /// </summary>
    public const string CreatedFrom = "dotnetPackage";

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
}
