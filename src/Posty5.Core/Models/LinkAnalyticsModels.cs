using System.Text.Json.Serialization;
using Posty5.Core.Converts;

namespace Posty5.Core.Models;

/// <summary>
/// Query for <c>GET /api/short-link/{id}/analytics</c> and
/// <c>GET /api/qr-code/{id}/analytics</c>. Every property is optional; a
/// property left <c>null</c> is not sent and the API applies its default.
/// </summary>
/// <remarks>
/// The API validates the values: an unknown interval or an invalid
/// <see cref="Tz"/> answers 400 (<see cref="Exceptions.Posty5ValidationException"/>),
/// and so does a range the API refuses (e.g. a span over 400 days). A
/// <see cref="Limit"/> outside 1-50 is refused by the SDK before sending. Naming a breakdown your
/// plan does not include, or a <see cref="From"/> older than your plan's
/// history window, answers the API's feature-lock 403.
/// </remarks>
public class LinkAnalyticsQuery
{
    /// <summary>
    /// First day of the range, sent as <c>yyyy-MM-dd</c> (only the date part
    /// is used; no time-zone conversion). API default: 30 days before <see cref="To"/>.
    /// It may not be earlier than the link's creation day.
    /// </summary>
    public DateTime? From { get; set; }

    /// <summary>
    /// Last day of the range, sent as <c>yyyy-MM-dd</c>. API default: today.
    /// </summary>
    public DateTime? To { get; set; }

    /// <summary>Bucket size of <see cref="LinkAnalyticsModel.Series"/>. API default: day.</summary>
    public LinkAnalyticsInterval? Interval { get; set; }

    /// <summary>
    /// IANA time zone the days are counted in, e.g. <c>Africa/Cairo</c>. API
    /// default: the owner's account time zone, else UTC. Not validated by the SDK.
    /// </summary>
    public string? Tz { get; set; }

    /// <summary>
    /// The breakdowns to compute, sent as a comma list. <c>null</c> or empty:
    /// the parameter is omitted and the API returns <b>every breakdown your plan
    /// allows</b> (the rest listed in <see cref="LinkAnalyticsMeta.Locked"/>), the
    /// same as <see cref="AllBreakdowns"/>. Naming a breakdown your plan does not
    /// include answers 403. Cannot be combined with <see cref="AllBreakdowns"/>.
    /// </summary>
    public IReadOnlyList<LinkAnalyticsBreakdown>? Breakdown { get; set; }

    /// <summary>
    /// <c>true</c> sends <c>breakdown=all</c> explicitly: every breakdown your plan
    /// allows, the others listed in <see cref="LinkAnalyticsMeta.Locked"/>. Same
    /// answer as leaving <see cref="Breakdown"/> unset. Cannot be combined with
    /// <see cref="Breakdown"/>.
    /// </summary>
    public bool AllBreakdowns { get; set; }

    /// <summary>
    /// Rows per breakdown, 1-50 (API default 10); the overflow comes back as one
    /// row with key <c>other</c>. Outside 1-50 throws <see cref="ArgumentOutOfRangeException"/>
    /// before any request.
    /// </summary>
    public int? Limit { get; set; }
}

/// <summary>
/// Visit analytics of one short link or QR code (contract C2).
/// </summary>
/// <remarks>
/// Bots and link-preview crawlers are excluded from every <c>Visits</c> and
/// counted only in <see cref="LinkAnalyticsTotals.BotVisits"/>.
/// <c>UniqueVisitors</c> over more than one day is the <b>sum of daily
/// uniques</b>: the visitor hash rotates daily, so the same person on two days
/// counts twice. Data starts on <see cref="LinkAnalyticsMeta.AnalyticsStartedAt"/>;
/// no earlier visit was ever recorded. For a static QR code only visits to its
/// Posty5 page are counted, not scans that never reach Posty5.
/// </remarks>
public class LinkAnalyticsModel
{
    /// <summary>Totals over the whole range.</summary>
    public LinkAnalyticsTotals Totals { get; set; } = new();

    /// <summary>One point per <see cref="LinkAnalyticsQuery.Interval"/> bucket, oldest first.</summary>
    public List<LinkAnalyticsSeriesPoint> Series { get; set; } = new();

    /// <summary>
    /// Rows per breakdown, keyed by the breakdown's wire name (<c>country</c>,
    /// <c>device</c>, … - see <see cref="LinkAnalyticsBreakdown"/>). Each list is the
    /// top <see cref="LinkAnalyticsQuery.Limit"/> keys plus <c>other</c> for the
    /// overflow; a missing value (no referrer, unknown country, …) is key <c>unknown</c>.
    /// <c>variant</c> and <c>rule</c> are empty until smart-routing ships.
    /// </summary>
    public Dictionary<string, List<LinkAnalyticsBreakdownRow>> Breakdowns { get; set; } = new();

    /// <summary>How the answer was computed, and what your plan left out.</summary>
    public LinkAnalyticsMeta Meta { get; set; } = new();
}

/// <summary>Totals of a <see cref="LinkAnalyticsModel"/>.</summary>
public class LinkAnalyticsTotals
{
    /// <summary>Human visits (bots excluded).</summary>
    public long Visits { get; set; }

    /// <summary>Sum of daily unique visitors over the range.</summary>
    public long UniqueVisitors { get; set; }

    /// <summary>Visits from bots and link-preview crawlers.</summary>
    public long BotVisits { get; set; }
}

/// <summary>One bucket of <see cref="LinkAnalyticsModel.Series"/>.</summary>
public class LinkAnalyticsSeriesPoint
{
    /// <summary>
    /// The bucket's first day as the API sends it (<c>yyyy-MM-dd</c>, in
    /// <see cref="LinkAnalyticsMeta.Timezone"/>). Kept as a string so no
    /// time-zone conversion can shift the day.
    /// </summary>
    public string Date { get; set; } = string.Empty;

    /// <summary>Human visits in the bucket.</summary>
    public long Visits { get; set; }

    /// <summary>Sum of daily unique visitors in the bucket.</summary>
    public long UniqueVisitors { get; set; }
}

/// <summary>One row of a breakdown in <see cref="LinkAnalyticsModel.Breakdowns"/>.</summary>
public class LinkAnalyticsBreakdownRow
{
    /// <summary>The value counted: an ISO-2 country, a device class, a referrer host, <c>click</c>/<c>scan</c>, …; <c>other</c> for the overflow beyond <see cref="LinkAnalyticsQuery.Limit"/>, <c>unknown</c> for a missing value.</summary>
    public string? Key { get; set; }

    /// <summary>Human visits with this key.</summary>
    public long Visits { get; set; }

    /// <summary>Sum of daily unique visitors with this key.</summary>
    public long UniqueVisitors { get; set; }
}

/// <summary>
/// The <c>meta</c> block of a <see cref="LinkAnalyticsModel"/>. Values are kept
/// as the API sends them (strings) so a value added by a later API release does
/// not fail deserialization.
/// </summary>
public class LinkAnalyticsMeta
{
    /// <summary>First day actually answered (after defaults and clamping).</summary>
    public string? From { get; set; }

    /// <summary>Last day actually answered.</summary>
    public string? To { get; set; }

    /// <summary>The interval used: <c>day</c>, <c>week</c> or <c>month</c>.</summary>
    public string? Interval { get; set; }

    /// <summary>
    /// The time zone the days are counted in. <c>UTC</c> when part of the range
    /// is older than raw-event retention, whatever <see cref="LinkAnalyticsQuery.Tz"/> asked.
    /// </summary>
    public string? Timezone { get; set; }

    /// <summary><c>events</c>, <c>rollup</c> or <c>mixed</c>: where the numbers came from.</summary>
    public string? Source { get; set; }

    /// <summary>The day Posty5 started recording visits (ISO date). Nothing before it exists.</summary>
    public string? AnalyticsStartedAt { get; set; }

    /// <summary>Breakdowns your plan does not include, with the lowest plan that does.</summary>
    public List<LinkAnalyticsLockedBreakdown> Locked { get; set; } = new();

    /// <summary>How far back your plan may query, in days: <c>30</c> on Free, <c>null</c> (unlimited) on Starter and up.</summary>
    public int? MaxHistoryDays { get; set; }
}

/// <summary>A breakdown left out by the plan (<see cref="LinkAnalyticsMeta.Locked"/>).</summary>
public class LinkAnalyticsLockedBreakdown
{
    /// <summary>The breakdown's wire name, e.g. <c>country</c>.</summary>
    public string Breakdown { get; set; } = string.Empty;

    /// <summary>The lowest plan that includes it, as a plan key such as <c>basic</c> (Starter).</summary>
    public string RequiredPlan { get; set; } = string.Empty;
}

/// <summary>Series bucket size (<see cref="LinkAnalyticsQuery.Interval"/>).</summary>
[JsonConverter(typeof(StringValueObjectConverter<LinkAnalyticsInterval>))]
public readonly record struct LinkAnalyticsInterval(string Value)
{
    /// <summary>One point per day.</summary>
    public static readonly LinkAnalyticsInterval Day = new("day");
    /// <summary>One point per week.</summary>
    public static readonly LinkAnalyticsInterval Week = new("week");
    /// <summary>One point per month.</summary>
    public static readonly LinkAnalyticsInterval Month = new("month");

    /// <summary>The wire name.</summary>
    public override string ToString() => Value;
}

/// <summary>A breakdown dimension (<see cref="LinkAnalyticsQuery.Breakdown"/>), with its C2 wire name.</summary>
[JsonConverter(typeof(StringValueObjectConverter<LinkAnalyticsBreakdown>))]
public readonly record struct LinkAnalyticsBreakdown(string Value)
{
    /// <summary>Visitor country, ISO-2.</summary>
    public static readonly LinkAnalyticsBreakdown Country = new("country");
    /// <summary>Device class (mobile, desktop, tablet, ...).</summary>
    public static readonly LinkAnalyticsBreakdown Device = new("device");
    /// <summary>Operating system.</summary>
    public static readonly LinkAnalyticsBreakdown Os = new("os");
    /// <summary>Browser.</summary>
    public static readonly LinkAnalyticsBreakdown Browser = new("browser");
    /// <summary>Referring host.</summary>
    public static readonly LinkAnalyticsBreakdown Referrer = new("referrer");
    /// <summary><c>click</c> (short link opened) or <c>scan</c> (QR code scanned).</summary>
    public static readonly LinkAnalyticsBreakdown Channel = new("channel");
    /// <summary>Browser language.</summary>
    public static readonly LinkAnalyticsBreakdown Language = new("language");
    /// <summary>A/B variant; empty until smart-routing ships.</summary>
    public static readonly LinkAnalyticsBreakdown Variant = new("variant");
    /// <summary>Routing rule; empty until smart-routing ships.</summary>
    public static readonly LinkAnalyticsBreakdown Rule = new("rule");

    /// <summary>The wire name.</summary>
    public override string ToString() => Value;
}
