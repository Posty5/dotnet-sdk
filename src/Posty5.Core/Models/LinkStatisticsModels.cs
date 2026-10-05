using System.Text.Json.Serialization;
using Posty5.Core.Converts;

namespace Posty5.Core.Models;

/// <summary>
/// Query for <c>GET /api/short-link/statistics</c> and
/// <c>GET /api/qr-code/statistics</c>: account-wide counts over all your short
/// links or QR codes. Every property is optional; with none set the API answers
/// the last 30 days.
/// </summary>
/// <remarks>
/// Sending <see cref="From"/> or <see cref="To"/> makes the range custom, so
/// <see cref="Period"/> may then only be <c>null</c> or
/// <see cref="LinkStatisticsPeriod.Custom"/>; anything else throws
/// <see cref="ArgumentException"/> before sending.
/// </remarks>
public class LinkStatisticsQuery
{
    /// <summary>Preset range. API default: <see cref="LinkStatisticsPeriod.Last30Days"/>.</summary>
    public LinkStatisticsPeriod? Period { get; set; }

    /// <summary>
    /// Range start (custom), sent as <c>yyyy-MM-dd</c> (date part only). API
    /// default: 29 days before today.
    /// </summary>
    public DateTime? From { get; set; }

    /// <summary>Range end (custom), sent as <c>yyyy-MM-dd</c>. API default: today.</summary>
    public DateTime? To { get; set; }
}

/// <summary>Preset range of <see cref="LinkStatisticsQuery.Period"/>, with its wire name.</summary>
[JsonConverter(typeof(StringValueObjectConverter<LinkStatisticsPeriod>))]
public readonly record struct LinkStatisticsPeriod(string Value)
{
    /// <summary>Today only.</summary>
    public static readonly LinkStatisticsPeriod Today = new("today");
    /// <summary>The last 7 days, today included.</summary>
    public static readonly LinkStatisticsPeriod Last7Days = new("7d");
    /// <summary>The last 30 days, today included (API default).</summary>
    public static readonly LinkStatisticsPeriod Last30Days = new("30d");
    /// <summary>The current calendar month.</summary>
    public static readonly LinkStatisticsPeriod Month = new("month");
    /// <summary><see cref="LinkStatisticsQuery.From"/> to <see cref="LinkStatisticsQuery.To"/>.</summary>
    public static readonly LinkStatisticsPeriod Custom = new("custom");

    /// <summary>The wire name.</summary>
    public override string ToString() => Value;
}

/// <summary>
/// The envelope every <c>/statistics</c> endpoint answers: the resolved range
/// plus the resource's data.
/// </summary>
/// <typeparam name="TData">The resource's statistics.</typeparam>
public class LinkStatisticsResponse<TData> where TData : new()
{
    /// <summary>The range the API resolved from the query.</summary>
    public LinkStatisticsRange Range { get; set; } = new();

    /// <summary>The statistics over that range.</summary>
    public TData Data { get; set; } = new();
}

/// <summary>The resolved range of a statistics answer.</summary>
public class LinkStatisticsRange
{
    /// <summary>Inclusive start (start of day), as the API sends it (ISO date-time).</summary>
    public DateTime? From { get; set; }

    /// <summary>Inclusive end (end of day), as the API sends it (ISO date-time).</summary>
    public DateTime? To { get; set; }

    /// <summary>The period used: <c>today</c>, <c>7d</c>, <c>30d</c>, <c>month</c> or <c>custom</c>.</summary>
    public string? Period { get; set; }
}

/// <summary>
/// The visit totals shared by short-link and QR-code statistics. The
/// <c>…InRange</c> values come from visit analytics: bots excluded from
/// <see cref="VisitsInRange"/>, uniques summed per UTC day.
/// </summary>
public class LinkStatisticsVisitTotals
{
    /// <summary>
    /// Lifetime visit counter summed over every record. Includes visits (and
    /// bots) from before visit analytics launched, so it does not equal the sum
    /// of <see cref="LinkStatisticsDailyRow.VisitorsSum"/>.
    /// </summary>
    public long TotalVisitors { get; set; }

    /// <summary>Visits by people in the range (bots excluded).</summary>
    public long VisitsInRange { get; set; }

    /// <summary>Sum of daily unique visitors in the range.</summary>
    public long UniqueVisitorsInRange { get; set; }

    /// <summary>Bot and link-preview visits in the range.</summary>
    public long BotVisitsInRange { get; set; }
}

/// <summary>One UTC day of a statistics answer's <c>daily</c> list.</summary>
public class LinkStatisticsDailyRow
{
    /// <summary>The UTC day, <c>yyyy-MM-dd</c> (the API's <c>_id</c>).</summary>
    [JsonPropertyName("_id")]
    public string Day { get; set; } = string.Empty;

    /// <summary>Records created that day.</summary>
    public long CreatedCount { get; set; }

    /// <summary>Visits by people that day (bots excluded).</summary>
    public long VisitorsSum { get; set; }
}
