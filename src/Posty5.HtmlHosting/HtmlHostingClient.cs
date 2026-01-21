using Posty5.Core.Http;
using Posty5.Core.Models;
using Posty5.HtmlHosting.Models;

namespace Posty5.HtmlHosting;

/// <summary>
/// Client for managing HTML hosting pages via Posty5 API
/// </summary>
public class HtmlHostingClient
{
    private readonly Posty5HttpClient _http;
    private const string BasePath = "/api/html-hosting";

    /// <summary>
    /// Creates a new HTML Hosting client
    /// </summary>
    /// <param name="httpClient">HTTP client instance from Posty5.Core</param>
    public HtmlHostingClient(Posty5HttpClient httpClient)
    {
        _http = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    }

    /// <summary>
    /// List HTML hosting pages with pagination and filters
    /// </summary>
    /// <param name="listParams">Filter parameters</param>
    /// <param name="pagination">Pagination parameters</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paginated list of HTML hosting pages</returns>
    public async Task<PaginationResponse<HtmlHostingModel>> ListAsync(
        ListHtmlHostingParams? listParams = null,
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

        var response = await _http.GetAsync<PaginationResponse<HtmlHostingModel>>(
            BasePath, 
            queryParams, 
            cancellationToken);
        
        return response.Result ?? new PaginationResponse<HtmlHostingModel>();
    }

    /// <summary>
    /// Get an HTML hosting page by ID
    /// </summary>
    /// <param name="id">Page ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>HTML hosting page details</returns>
    public async Task<HtmlHostingModel> GetAsync(string id, CancellationToken cancellationToken = default)
    {
        var response = await _http.GetAsync<HtmlHostingModel>($"{BasePath}/{id}", cancellationToken: cancellationToken);
        return response.Result ?? throw new InvalidOperationException("HTML hosting page not found");
    }

    /// <summary>
    /// Create a new HTML hosting page
    /// </summary>
    /// <param name="request">Create request data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created HTML hosting page</returns>
    public async Task<HtmlHostingModel> CreateAsync(
        CreateHtmlHostingRequest request,
        CancellationToken cancellationToken = default)
    {
        var response = await _http.PostAsync<HtmlHostingModel>(BasePath, request, cancellationToken);
        return response.Result ?? throw new InvalidOperationException("Failed to create HTML hosting page");
    }

    /// <summary>
    /// Update an existing HTML hosting page
    /// </summary>
    /// <param name="id">Page ID</param>
    /// <param name="request">Update request data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated HTML hosting page</returns>
    public async Task<HtmlHostingModel> UpdateAsync(
        string id,
        UpdateHtmlHostingRequest request,
        CancellationToken cancellationToken = default)
    {
        var response = await _http.PutAsync<HtmlHostingModel>($"{BasePath}/{id}", request, cancellationToken);
        return response.Result ?? throw new InvalidOperationException("Failed to update HTML hosting page");
    }

    /// <summary>
    /// Delete an HTML hosting page
    /// </summary>
    /// <param name="id">Page ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if deleted successfully</returns>
    public async Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        var response = await _http.DeleteAsync<object>($"{BasePath}/{id}", cancellationToken);
        return response.IsSuccess;
    }
}
