using System.Globalization;
using Posty5.Core.Configuration;
using Posty5.Core.Models;

namespace Posty5.Core.Helpers;

/// <summary>
/// Builds the path and query of <c>GET {basePath}/{id}/analytics</c> and
/// <c>GET {basePath}/statistics</c>, shared by <c>ShortLinkClient</c> and
/// <c>QRCodeClient</c> so both send exactly the same query for the same
/// <see cref="LinkAnalyticsQuery"/> / <see cref="LinkStatisticsQuery"/>.
/// </summary>
public static class LinkAnalyticsQueryHelper
{
    /// <summary>
    /// <c>{basePath}/{id}/analytics</c>, with <paramref name="id"/> escaped.
    /// </summary>
    /// <param name="basePath">The resource's API path, e.g. <c>/api/short-link</c>.</param>
    /// <param name="id">The short link or QR code ID.</param>
    /// <exception cref="ArgumentException"><paramref name="id"/> is empty.</exception>
    public static string BuildPath(string basePath, string id)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        return $"{basePath}/{Uri.EscapeDataString(id)}/{LinkAnalyticsConst.PathSegment}";
    }

    /// <summary>
    /// The query parameters for <paramref name="query"/>; an unset property is
    /// left out so the API applies its default.
    /// </summary>
    /// <param name="query">The caller's query, or <c>null</c> for every API default.</param>
    /// <returns>Parameters for <c>Posty5HttpClient.GetAsync</c>.</returns>
    /// <exception cref="ArgumentException">
    /// Both <see cref="LinkAnalyticsQuery.AllBreakdowns"/> and a non-empty
    /// <see cref="LinkAnalyticsQuery.Breakdown"/> are set.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <see cref="LinkAnalyticsQuery.Limit"/> is outside 1-50.
    /// </exception>
    public static Dictionary<string, object?> ToQueryParams(LinkAnalyticsQuery? query)
    {
        var queryParams = new Dictionary<string, object?>();
        if (query == null) return queryParams;

        var hasBreakdownList = query.Breakdown is { Count: > 0 };
        if (query.AllBreakdowns && hasBreakdownList)
        {
            throw new ArgumentException(
                $"Set either {nameof(LinkAnalyticsQuery.AllBreakdowns)} or {nameof(LinkAnalyticsQuery.Breakdown)}, not both.",
                nameof(query));
        }

        if (query.Limit is < LinkAnalyticsConst.MinLimit or > LinkAnalyticsConst.MaxLimit)
        {
            throw new ArgumentOutOfRangeException(
                nameof(query),
                query.Limit,
                $"{nameof(LinkAnalyticsQuery.Limit)} must be between {LinkAnalyticsConst.MinLimit} and {LinkAnalyticsConst.MaxLimit}.");
        }

        if (query.From.HasValue)
            queryParams[LinkAnalyticsConst.FromKey] = FormatDate(query.From.Value);
        if (query.To.HasValue)
            queryParams[LinkAnalyticsConst.ToKey] = FormatDate(query.To.Value);
        if (query.Interval.HasValue)
            queryParams[LinkAnalyticsConst.IntervalKey] = query.Interval.Value.Value;
        if (!string.IsNullOrWhiteSpace(query.Tz))
            queryParams[LinkAnalyticsConst.TzKey] = query.Tz;

        if (query.AllBreakdowns)
            queryParams[LinkAnalyticsConst.BreakdownKey] = LinkAnalyticsConst.AllBreakdownsValue;
        else if (hasBreakdownList)
            queryParams[LinkAnalyticsConst.BreakdownKey] = string.Join(
                LinkAnalyticsConst.BreakdownSeparator,
                query.Breakdown!.Select(breakdown => breakdown.Value));

        if (query.Limit.HasValue)
            queryParams[LinkAnalyticsConst.LimitKey] = query.Limit.Value.ToString(CultureInfo.InvariantCulture);

        return queryParams;
    }

    /// <summary><c>{basePath}/statistics</c>.</summary>
    /// <param name="basePath">The resource's API path, e.g. <c>/api/short-link</c>.</param>
    public static string BuildStatisticsPath(string basePath) =>
        $"{basePath}/{LinkAnalyticsConst.StatisticsPathSegment}";

    /// <summary>
    /// The query parameters for <paramref name="query"/>; an unset property is
    /// left out so the API applies its default (the last 30 days).
    /// </summary>
    /// <param name="query">The caller's query, or <c>null</c> for the API defaults.</param>
    /// <returns>Parameters for <c>Posty5HttpClient.GetAsync</c>.</returns>
    /// <exception cref="ArgumentException">
    /// <see cref="LinkStatisticsQuery.From"/> or <see cref="LinkStatisticsQuery.To"/> is
    /// set with a <see cref="LinkStatisticsQuery.Period"/> other than custom (the API
    /// would ignore the period), or <c>From</c> is after <c>To</c>.
    /// </exception>
    public static Dictionary<string, object?> ToStatisticsQueryParams(LinkStatisticsQuery? query)
    {
        var queryParams = new Dictionary<string, object?>();
        if (query == null) return queryParams;

        var hasDates = query.From.HasValue || query.To.HasValue;
        if (hasDates && query.Period.HasValue && query.Period.Value != LinkStatisticsPeriod.Custom)
        {
            throw new ArgumentException(
                $"{nameof(LinkStatisticsQuery.From)}/{nameof(LinkStatisticsQuery.To)} make the range custom; " +
                $"leave {nameof(LinkStatisticsQuery.Period)} unset or use {nameof(LinkStatisticsPeriod)}.{nameof(LinkStatisticsPeriod.Custom)}.",
                nameof(query));
        }
        if (query.From.HasValue && query.To.HasValue && query.From.Value.Date > query.To.Value.Date)
        {
            throw new ArgumentException(
                $"{nameof(LinkStatisticsQuery.From)} must not be after {nameof(LinkStatisticsQuery.To)}.",
                nameof(query));
        }

        if (query.Period.HasValue)
            queryParams[LinkAnalyticsConst.PeriodKey] = query.Period.Value.Value;
        if (query.From.HasValue)
            queryParams[LinkAnalyticsConst.FromKey] = FormatDate(query.From.Value);
        if (query.To.HasValue)
            queryParams[LinkAnalyticsConst.ToKey] = FormatDate(query.To.Value);

        return queryParams;
    }

    /// <summary>The date part only, culture-independent: no time-zone conversion.</summary>
    private static string FormatDate(DateTime value) =>
        value.ToString(LinkAnalyticsConst.DateFormat, CultureInfo.InvariantCulture);
}
