using System.Net.Http.Headers;
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
    /// Create an HTML page by uploading a file to R2 storage
    /// Auto-publishes after file upload is complete
    /// </summary>
    /// <param name="data">Create request data with file information</param>
    /// <param name="fileStream">HTML file stream to upload</param>
    /// <param name="contentType">Content type of the file (default: text/html)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created page with ID, shorter link, and file URL</returns>
    public async Task<HtmlHostingPageFileResponseModel> CreateWithFileAsync(
        HtmlHostingCreatePageFileRequestModel data,
        Stream fileStream,
        string contentType = "text/html",
        CancellationToken cancellationToken = default)
    {
        // Step 1: Create the HTML page record and get upload configuration
        var payload = new
        {
            data.Name,
            data.FileName,
            data.CustomLandingId,
            data.IsEnableMonetization,
            data.AutoSaveInGoogleSheet,
            data.RefId,
            data.Tag,
            createdFrom = "dotnetPackage"
        };

        var response = await _http.PostAsync<HtmlHostingCreatePageResponseModel>($"{BasePath}/file", payload, cancellationToken);
        var result = response.Result?.Details ?? throw new InvalidOperationException("Failed to create HTML page");
        var uploadConfig = response.Result?.UploadFileConfig ?? throw new InvalidOperationException("Upload configuration not provided");

        // Step 2: Upload HTML file to R2 storage
        await UploadToR2Async(uploadConfig.UploadUrl, fileStream, contentType, cancellationToken);

        // Page is auto-published by backend after file upload

        return new HtmlHostingPageFileResponseModel
        {
            Id = result.Id,
            ShorterLink = result.ShorterLink,
            FileUrl = result.FileUrl!
        };
    }

    /// <summary>
    /// Create an HTML page from a GitHub repository
    /// The API will automatically fetch and deploy the file from GitHub and publish it
    /// </summary>
    /// <param name="data">Create request data with GitHub file URL</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created page with ID, shorter link, and GitHub info</returns>
    public async Task<HtmlHostingPageGithubResponseModel> CreateWithGithubFileAsync(
        HtmlHostingCreatePageGithubRequestModel data,
        CancellationToken cancellationToken = default)
    {
        var payload = new
        {
            data.Name,
            data.GithubInfo,
            data.CustomLandingId,
            data.IsEnableMonetization,
            data.AutoSaveInGoogleSheet,
            data.RefId,
            data.Tag,
            createdFrom = "dotnetPackage"
        };

        var response = await _http.PostAsync<HtmlHostingCreatePageResponseModel>($"{BasePath}/github", payload, cancellationToken);
        var result = response.Result?.Details ?? throw new InvalidOperationException("Failed to create HTML page");

        // Page is auto-published by backend

        return new HtmlHostingPageGithubResponseModel
        {
            Id = result.Id,
            ShorterLink = result.ShorterLink,
            GithubInfo = result.GithubInfo!
        };
    }

    /// <summary>
    /// Get an HTML page by ID
    /// </summary>
    /// <param name="id">HTML page ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>HTML page full details</returns>
    public async Task<HtmlHostingPageModel> GetAsync(string id, CancellationToken cancellationToken = default)
    {
        var response = await _http.GetAsync<HtmlHostingPageModel>($"{BasePath}/{id}", cancellationToken: cancellationToken);
        return response.Result ?? throw new InvalidOperationException("HTML page not found");
    }

    /// <summary>
    /// Search/List HTML pages with pagination and filters
    /// </summary>
    /// <param name="listParams">Filter parameters</param>
    /// <param name="pagination">Pagination parameters</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paginated list of HTML pages</returns>
    public async Task<PaginationResponse<HtmlHostingPageModel>> ListAsync(
        HtmlHostingListPagesParamsModel? listParams = null,
        PaginationParams? pagination = null,
        CancellationToken cancellationToken = default)
    {
        var queryParams = new Dictionary<string, object?>();

        if (listParams != null)
        {
            if (!string.IsNullOrEmpty(listParams.Name))
                queryParams["name"] = listParams.Name;
            if (!string.IsNullOrEmpty(listParams.HtmlHostingId))
                queryParams["htmlHostingId"] = listParams.HtmlHostingId;
            if (!string.IsNullOrEmpty(listParams.Tag))
                queryParams["tag"] = listParams.Tag;
            if (!string.IsNullOrEmpty(listParams.RefId))
                queryParams["refId"] = listParams.RefId;
            if (listParams.Status.HasValue)
                queryParams["status"] = listParams.Status.Value.ToString();
            if (!string.IsNullOrEmpty(listParams.SourceType))
                queryParams["sourceType"] = listParams.SourceType;
            if (listParams.IsEnableMonetization.HasValue)
                queryParams["isEnableMonetization"] = listParams.IsEnableMonetization.Value;
            if (listParams.AutoSaveInGoogleSheet.HasValue)
                queryParams["autoSaveInGoogleSheet"] = listParams.AutoSaveInGoogleSheet.Value;
            if (listParams.IsTemp.HasValue)
                queryParams["isTemp"] = listParams.IsTemp.Value;
            if (listParams.IsCachedInLocalStorage.HasValue)
                queryParams["isCachedInLocalStorage"] = listParams.IsCachedInLocalStorage.Value;
        }

        if (pagination != null)
        {
            if (!string.IsNullOrEmpty(pagination.Cursor))
                queryParams["cursor"] = pagination.Cursor;
            queryParams["pageSize"] = pagination.PageSize;
        }

        var response = await _http.GetAsync<PaginationResponse<HtmlHostingPageModel>>(
            BasePath,
            queryParams,
            cancellationToken);

        return response.Result ?? new PaginationResponse<HtmlHostingPageModel>();
    }

    /// <summary>
    /// Get a simplified lookup list of HTML pages (ID and name only)
    /// Useful for dropdowns and selection lists
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of HTML page lookup items</returns>
    public async Task<List<HtmlHostingPageLookupItemModel>> LookupAsync(CancellationToken cancellationToken = default)
    {
        var response = await _http.GetAsync<List<HtmlHostingPageLookupItemModel>>(
            $"{BasePath}/lookup",
            cancellationToken: cancellationToken);
        return response.Result ?? new List<HtmlHostingPageLookupItemModel>();
    }

    /// <summary>
    /// Get form IDs for an HTML page
    /// </summary>
    /// <param name="id">HTML page ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of form lookup items</returns>
    public async Task<List<HtmlHostingFormLookupItemModel>> LookupFormsAsync(string id, CancellationToken cancellationToken = default)
    {
        var response = await _http.GetAsync<List<HtmlHostingFormLookupItemModel>>(
            $"{BasePath}/lookup-froms/{id}",
            cancellationToken: cancellationToken);
        return response.Result ?? new List<HtmlHostingFormLookupItemModel>();
    }

    /// <summary>
    /// Update an HTML page with a new file upload to R2 storage
    /// Auto-publishes after file upload is complete
    /// </summary>
    /// <param name="id">HTML page ID to update</param>
    /// <param name="data">Update request data with file information</param>
    /// <param name="fileStream">New HTML file stream to upload</param>
    /// <param name="contentType">Content type of the file (default: text/html)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated page with ID, shorter link, and file URL</returns>
    public async Task<HtmlHostingPageFileResponseModel> UpdateWithFileAsync(
        string id,
        HtmlHostingUpdatePageFileRequestModel data,
        Stream fileStream,
        string contentType = "text/html",
        CancellationToken cancellationToken = default)
    {
        // Step 1: Update the HTML page record and get upload configuration
        var payload = new
        {
            data.Name,
            data.FileName,
            data.CustomLandingId,
            data.IsEnableMonetization,
            data.AutoSaveInGoogleSheet,
            isNewFile = true
        };

        var response = await _http.PutAsync<HtmlHostingCreatePageResponseModel>($"{BasePath}/{id}/file", payload, cancellationToken);
        var result = response.Result?.Details ?? throw new InvalidOperationException("Failed to update HTML page");
        var uploadConfig = response.Result?.UploadFileConfig;

        // Step 2: Upload HTML file to R2 if upload config is provided
        if (uploadConfig != null)
        {
            await UploadToR2Async(uploadConfig.UploadUrl, fileStream, contentType, cancellationToken);
        }

        // Page is auto-published by backend

        return new HtmlHostingPageFileResponseModel
        {
            Id = result.Id,
            ShorterLink = result.ShorterLink,
            FileUrl = result.FileUrl!
        };
    }

    /// <summary>
    /// Update an HTML page with a new GitHub repository file
    /// The API will automatically fetch, deploy, and publish the file from GitHub
    /// </summary>
    /// <param name="id">HTML page ID to update</param>
    /// <param name="data">Update request data with GitHub file URL</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated page with ID, shorter link, and GitHub info</returns>
    public async Task<HtmlHostingPageGithubResponseModel> UpdateWithGithubFileAsync(
        string id,
        HtmlHostingUpdatePageGithubRequestModel data,
        CancellationToken cancellationToken = default)
    {
        var payload = new
        {
            data.Name,
            data.GithubInfo,
            data.CustomLandingId,
            data.IsEnableMonetization,
            data.AutoSaveInGoogleSheet
        };

        var response = await _http.PutAsync<HtmlHostingCreatePageResponseModel>($"{BasePath}/{id}/github", payload, cancellationToken);
        var result = response.Result?.Details ?? throw new InvalidOperationException("Failed to update HTML page");

        return new HtmlHostingPageGithubResponseModel
        {
            Id = result.Id,
            ShorterLink = result.ShorterLink,
            GithubInfo = result.GithubInfo
        };
    }

    /// <summary>
    /// Delete an HTML page
    /// </summary>
    /// <param name="id">HTML page ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    public async Task DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        await _http.DeleteAsync<object>($"{BasePath}/{id}", cancellationToken);
    }

    /// <summary>
    /// Clear the cache for an HTML page to force fresh content delivery
    /// </summary>
    /// <param name="id">HTML page ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    public async Task CleanCacheAsync(string id, CancellationToken cancellationToken = default)
    {
        await _http.PutAsync<object>($"{BasePath}/{id}/clean-cache", new { }, cancellationToken);
    }

    /// <summary>
    /// Upload file to R2 storage using pre-signed URL
    /// </summary>
    /// <param name="uploadUrl">Pre-signed R2 upload URL</param>
    /// <param name="fileStream">File stream to upload</param>
    /// <param name="contentType">Content type of the file</param>
    /// <param name="cancellationToken">Cancellation token</param>
    private async Task UploadToR2Async(
        string uploadUrl,
        Stream fileStream,
        string contentType,
        CancellationToken cancellationToken)
    {
        using var client = new HttpClient();
        using var content = new StreamContent(fileStream);
        content.Headers.ContentType = new MediaTypeHeaderValue(contentType);

        var response = await client.PutAsync(uploadUrl, content, cancellationToken);
        response.EnsureSuccessStatusCode();
    }
}
