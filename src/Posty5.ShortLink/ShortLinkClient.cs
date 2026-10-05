using Posty5.Core.Exceptions;
using Posty5.Core.Helpers;
using Posty5.Core.Http;
using Posty5.Core.Models;
using Posty5.ShortLink.Models;

namespace Posty5.ShortLink;

/// <summary>
/// Client for managing short links via Posty5 API
/// </summary>
public class ShortLinkClient
{
    private readonly Posty5HttpClient _http;

    /// <summary>
    /// Creates a new Short Link client
    /// </summary>
    /// <param name="httpClient">HTTP client instance from Posty5.Core</param>
    public ShortLinkClient(Posty5HttpClient httpClient)
    {
        _http = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    }

    /// <summary>
    /// Search/List short links with pagination and filters
    /// </summary>
    /// <param name="listParams">Filter parameters</param>
    /// <param name="pagination">Pagination parameters</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paginated list of short links</returns>
    public async Task<PaginationResponse<ShortLinkModel>> ListAsync(
        ShortLinkListParamsModel? listParams = null,
        PaginationParams? pagination = null,
        CancellationToken cancellationToken = default)
    {
        var queryParams = new Dictionary<string, object?>();

        if (listParams != null)
        {
            if (!string.IsNullOrEmpty(listParams.BaseUrl))
                queryParams["baseUrl"] = listParams.BaseUrl;
            if (!string.IsNullOrEmpty(listParams.Name))
                queryParams["name"] = listParams.Name;
            if (!string.IsNullOrEmpty(listParams.PageInfoTitle))
                queryParams[ShortLinkConst.PageInfoTitleFilterKey] = listParams.PageInfoTitle;
            if (!string.IsNullOrEmpty(listParams.CreatedFrom))
                queryParams["createdFrom"] = listParams.CreatedFrom;
            if (!string.IsNullOrEmpty(listParams.ShortLinkId))
                queryParams["shortLinkId"] = listParams.ShortLinkId;
            if (!string.IsNullOrEmpty(listParams.RefId))
                queryParams["refId"] = listParams.RefId;
            if (!string.IsNullOrEmpty(listParams.Tag))
                queryParams["tag"] = listParams.Tag;
            if (!string.IsNullOrEmpty(listParams.TemplateId))
                queryParams["templateId"] = listParams.TemplateId;
            if (listParams.Status.HasValue)
                queryParams["status"] = listParams.Status.Value.ToString();
            if (listParams.IsForDeepLink.HasValue)
                queryParams["isForDeepLink"] = listParams.IsForDeepLink.Value;
            // IsEnableMonetization, Search, FromDate and ToDate are obsolete: the
            // API has no such filters, so they are never sent.
        }

        if (pagination != null)
        {
            if (!string.IsNullOrEmpty(pagination.Cursor))
                queryParams["cursor"] = pagination.Cursor;
            queryParams["pageSize"] = pagination.PageSize;
        }

        var response = await _http.GetAsync<PaginationResponse<ShortLinkModel>>(
            ShortLinkConst.BasePath,
            queryParams,
            cancellationToken);

        return response.Result ?? new PaginationResponse<ShortLinkModel>();
    }

    /// <summary>
    /// Get a short link by ID with full details
    /// </summary>
    /// <param name="id">Short link ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Short link full details including populated template, user, API key, and metadata</returns>
    public async Task<ShortLinkFullDetailsModel> GetAsync(string id, CancellationToken cancellationToken = default)
    {
        var response = await _http.GetAsync<ShortLinkFullDetailsModel>($"{ShortLinkConst.BasePath}/{id}", cancellationToken: cancellationToken);
        return response.Result ?? throw new InvalidOperationException("Short link not found");
    }

    /// <summary>
    /// Visit analytics of one short link: totals, a series and breakdowns
    /// (<c>GET /api/short-link/{id}/analytics</c>). Reading analytics costs no credits.
    /// </summary>
    /// <remarks>
    /// <para>Bots and link-preview crawlers are excluded from every <c>Visits</c>
    /// and counted only in <see cref="LinkAnalyticsTotals.BotVisits"/>.</para>
    /// <para><c>UniqueVisitors</c> over more than one day is the sum of daily
    /// uniques (the visitor hash rotates daily).</para>
    /// <para>Data starts on <see cref="LinkAnalyticsMeta.AnalyticsStartedAt"/>,
    /// when Posty5 started recording visits; nothing earlier exists.</para>
    /// <para>Plan limits come from the API: unless you name breakdowns, you get
    /// every breakdown your plan allows and the rest are listed in
    /// <see cref="LinkAnalyticsMeta.Locked"/>; naming one
    /// in <see cref="LinkAnalyticsQuery.Breakdown"/>, or a
    /// <see cref="LinkAnalyticsQuery.From"/> older than your plan's history, throws
    /// <see cref="Posty5Exception"/> with <see cref="Posty5Exception.StatusCode"/>
    /// 403 and the API's message (<c>This feature is not available on your current plan.</c>,
    /// or <c>You Have Not Permission</c>) in <see cref="Posty5Exception.ResponseBody"/>.</para>
    /// </remarks>
    /// <param name="id">Short link ID</param>
    /// <param name="query">Range, interval, time zone and breakdowns; <c>null</c> for the API defaults (last 30 days, by day, every breakdown your plan allows).</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The analytics answer</returns>
    /// <exception cref="ArgumentOutOfRangeException"><see cref="LinkAnalyticsQuery.Limit"/> is outside 1-50.</exception>
    /// <exception cref="ArgumentException"><paramref name="id"/> is empty, or both <see cref="LinkAnalyticsQuery.AllBreakdowns"/> and <see cref="LinkAnalyticsQuery.Breakdown"/> are set.</exception>
    /// <exception cref="Posty5ValidationException">
    /// 400: the API refused the query (e.g. an unknown interval or time zone), or no
    /// short link with that ID is visible to this API key (<c>The Short Link Is Not Found</c>; the API answers
    /// 400, not 404, for a missing record).
    /// </exception>
    /// <example>
    /// <code>
    /// var analytics = await shortLinkClient.GetAnalyticsAsync("link123", new LinkAnalyticsQuery
    /// {
    ///     From = new DateTime(2026, 10, 1),
    ///     To = new DateTime(2026, 10, 31),
    ///     Interval = LinkAnalyticsInterval.Week,
    ///     AllBreakdowns = true
    /// });
    /// Console.WriteLine(analytics.Totals.Visits);
    /// foreach (var row in analytics.Breakdowns.GetValueOrDefault("device") ?? new())
    ///     Console.WriteLine($"{row.Key}: {row.Visits}");
    /// </code>
    /// </example>
    public async Task<LinkAnalyticsModel> GetAnalyticsAsync(
        string id,
        LinkAnalyticsQuery? query = null,
        CancellationToken cancellationToken = default)
    {
        var response = await _http.GetAsync<LinkAnalyticsModel>(
            LinkAnalyticsQueryHelper.BuildPath(ShortLinkConst.BasePath, id),
            LinkAnalyticsQueryHelper.ToQueryParams(query),
            cancellationToken);

        return response.Result ?? throw new InvalidOperationException("Short link analytics were not returned");
    }

    /// <summary>
    /// Account-wide statistics over all your short links (<c>GET /api/short-link/statistics</c>):
    /// lifetime totals, visit totals in the range, a <c>daily</c> list in UTC days
    /// and the top ten short links by visits in the range.
    /// </summary>
    /// <remarks>
    /// <para><c>VisitsInRange</c>, <c>UniqueVisitorsInRange</c>, <c>BotVisitsInRange</c>,
    /// <c>Daily[].VisitorsSum</c> and <c>TopLinks[].VisitsInRange</c> come from visit
    /// analytics: bots excluded, uniques summed per UTC day, nothing before
    /// analytics launched. <c>TotalVisitors</c> is the lifetime counter and still
    /// includes older visits.</para>
    /// <para>Days are UTC days whatever your account time zone; use
    /// <see cref="GetAnalyticsAsync"/> for one short link in your time zone.</para>
    /// </remarks>
    /// <param name="query">Preset period or custom <c>From</c>/<c>To</c>; <c>null</c> for the last 30 days.</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The resolved range and the statistics</returns>
    /// <exception cref="ArgumentException"><c>From</c>/<c>To</c> set with a non-custom <c>Period</c>, or <c>From</c> after <c>To</c>.</exception>
    /// <example>
    /// <code>
    /// var stats = await shortLinkClient.GetStatisticsAsync(new LinkStatisticsQuery { Period = LinkStatisticsPeriod.Last7Days });
    /// Console.WriteLine($"{stats.Data.Totals.VisitsInRange} visits since {stats.Range.From:d}");
    /// foreach (var day in stats.Data.Daily)
    ///     Console.WriteLine($"{day.Day}: {day.VisitorsSum}");
    /// </code>
    /// </example>
    public async Task<ShortLinkStatisticsModel> GetStatisticsAsync(
        LinkStatisticsQuery? query = null,
        CancellationToken cancellationToken = default)
    {
        var response = await _http.GetAsync<ShortLinkStatisticsModel>(
            LinkAnalyticsQueryHelper.BuildStatisticsPath(ShortLinkConst.BasePath),
            LinkAnalyticsQueryHelper.ToStatisticsQueryParams(query),
            cancellationToken);

        return response.Result ?? throw new InvalidOperationException("Short link statistics were not returned");
    }

    /// <summary>
    /// Create a new short link
    /// </summary>
    /// <param name="request">Create request data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created short link details</returns>
    public async Task<ShortLinkModel> CreateAsync(
        ShortLinkCreateRequestModel request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        // An explicit body: only fields the API accepts, so an obsolete model
        // property can never reach it. Null values are omitted on the wire.
        var data = new
        {
            request.Name,
            request.BaseUrl,
            request.TemplateId,
            request.CustomLandingId,
            request.RefId,
            request.Tag,
            request.IsEnableLandingPage,
            request.PageInfo,
            request.AndroidUrl,
            request.IosUrl,
            TemplateType = ShortLinkConst.TemplateType,
            CreatedFrom = ShortLinkConst.CreatedFrom
        };

        var response = await _http.PostAsync<ShortLinkModel>(ShortLinkConst.BasePath, data, cancellationToken);
        return response.Result ?? throw new InvalidOperationException("Failed to create short link");
    }

    /// <summary>
    /// Update an existing short link
    /// </summary>
    /// <remarks>
    /// The API needs <see cref="ShortLinkUpdateRequestModel.BaseUrl"/> and
    /// <see cref="ShortLinkUpdateRequestModel.TemplateId"/> on every update.
    /// A property left <c>null</c> is not sent; see each property for what the
    /// API does then.
    /// </remarks>
    /// <param name="id">Short link ID</param>
    /// <param name="request">Update request data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated short link details</returns>
    /// <exception cref="ArgumentException"><see cref="ShortLinkUpdateRequestModel.BaseUrl"/> is empty.</exception>
    public async Task<ShortLinkModel> UpdateAsync(
        string id,
        ShortLinkUpdateRequestModel request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.BaseUrl, $"{nameof(request)}.{nameof(request.BaseUrl)}");

        // Explicit body: no IsEnableMonetization, and no CustomLandingId (the
        // update schema rejects it; a link's id cannot change).
        var data = new
        {
            request.Name,
            request.BaseUrl,
            request.TemplateId,
            request.RefId,
            request.Tag,
            request.IsEnableLandingPage,
            request.PageInfo,
            request.AndroidUrl,
            request.IosUrl,
            TemplateType = ShortLinkConst.TemplateType,
            CreatedFrom = ShortLinkConst.CreatedFrom
        };

        var response = await _http.PutAsync<ShortLinkModel>($"{ShortLinkConst.BasePath}/{id}", data, cancellationToken);
        return response.Result ?? throw new InvalidOperationException("Failed to update short link");
    }

    /// <summary>
    /// Delete a short link
    /// </summary>
    /// <param name="id">Short link ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Deletion confirmation response</returns>
    public async Task<DeleteResponse> DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        var response = await _http.DeleteAsync<DeleteResponse>($"{ShortLinkConst.BasePath}/{id}", cancellationToken);
        return response.Result ?? new DeleteResponse { Message = "Deleted" };
    }
}
