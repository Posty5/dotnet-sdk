namespace Posty5.Core.Configuration;

/// <summary>
/// Wire names of the link / QR analytics (contract C2) and statistics
/// endpoints, used by <see cref="Helpers.LinkAnalyticsQueryHelper"/>.
/// </summary>
internal static class LinkAnalyticsConst
{
    /// <summary>Path segment after <c>{basePath}/{id}</c>.</summary>
    public const string PathSegment = "analytics";

    /// <summary>Path segment of the account-wide statistics, after <c>{basePath}</c>.</summary>
    public const string StatisticsPathSegment = "statistics";

    /// <summary>Statistics preset-range key.</summary>
    public const string PeriodKey = "period";

    /// <summary>Format of <c>from</c> / <c>to</c>: an ISO date, no time.</summary>
    public const string DateFormat = "yyyy-MM-dd";

    /// <summary><c>breakdown</c> value asking for every breakdown the plan allows.</summary>
    public const string AllBreakdownsValue = "all";

    /// <summary>Separator of the <c>breakdown</c> comma list.</summary>
    public const string BreakdownSeparator = ",";

    /// <summary>Smallest <c>limit</c> the API accepts.</summary>
    public const int MinLimit = 1;

    /// <summary>Largest <c>limit</c> the API accepts.</summary>
    public const int MaxLimit = 50;

    public const string FromKey = "from";
    public const string ToKey = "to";
    public const string IntervalKey = "interval";
    public const string TzKey = "tz";
    public const string BreakdownKey = "breakdown";
    public const string LimitKey = "limit";
}
