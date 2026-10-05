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
