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
    private const string BasePath = "/api/short-link";

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
        ListShortLinksParams? listParams = null,
        PaginationParams? pagination = null,
        CancellationToken cancellationToken = default)
    {
        var queryParams = new Dictionary<string, object?>();

        if (listParams != null)
        {
            if (!string.IsNullOrEmpty(listParams.Search))
                queryParams["search"] = listParams.Search;
            if (listParams.FromDate.HasValue)
                queryParams["fromDate"] = listParams.FromDate.Value.ToString("o");
            if (listParams.ToDate.HasValue)
                queryParams["toDate"] = listParams.ToDate.Value.ToString("o");
        }

        if (pagination != null)
        {
            queryParams["pageNumber"] = pagination.PageNumber;
            queryParams["pageSize"] = pagination.PageSize;
        }

        var response = await _http.GetAsync<PaginationResponse<ShortLinkModel>>(
            BasePath, 
            queryParams, 
            cancellationToken);
        
        return response.Result ?? new PaginationResponse<ShortLinkModel>();
    }

    /// <summary>
    /// Get a short link by ID
    /// </summary>
    /// <param name="id">Short link ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Short link full details</returns>
    public async Task<ShortLinkModel> GetAsync(string id, CancellationToken cancellationToken = default)
    {
        var response = await _http.GetAsync<ShortLinkModel>($"{BasePath}/{id}", cancellationToken: cancellationToken);
        return response.Result ?? throw new InvalidOperationException("Short link not found");
    }

    /// <summary>
    /// Create a new short link
    /// </summary>
    /// <param name="request">Create request data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created short link details</returns>
    public async Task<ShortLinkModel> CreateAsync(
        CreateShortLinkRequest request,
        CancellationToken cancellationToken = default)
    {
        var data = new
        {
            request.Name,
            request.TargetUrl,
            request.TemplateId,
            request.CustomSlug,
            TemplateType = "user",
            CreatedFrom = "dotnetPackage"
        };

        var response = await _http.PostAsync<ShortLinkModel>(BasePath, data, cancellationToken);
        return response.Result ?? throw new InvalidOperationException("Failed to create short link");
    }

    /// <summary>
    /// Update an existing short link
    /// </summary>
    /// <param name="id">Short link ID</param>
    /// <param name="request">Update request data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated short link details</returns>
    public async Task<ShortLinkModel> UpdateAsync(
        string id,
        UpdateShortLinkRequest request,
        CancellationToken cancellationToken = default)
    {
        var response = await _http.PutAsync<ShortLinkModel>($"{BasePath}/{id}", request, cancellationToken);
        return response.Result ?? throw new InvalidOperationException("Failed to update short link");
    }

    /// <summary>
    /// Delete a short link
    /// </summary>
    /// <param name="id">Short link ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if deleted successfully</returns>
    public async Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        var response = await _http.DeleteAsync<object>($"{BasePath}/{id}", cancellationToken);
        return response.IsSuccess;
    }
}
