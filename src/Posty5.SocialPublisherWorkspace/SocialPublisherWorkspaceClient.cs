using System.Net.Http.Headers;
using Posty5.Core.Http;
using Posty5.Core.Models;
using Posty5.SocialPublisherWorkspace.Models;

namespace Posty5.SocialPublisherWorkspace;

/// <summary>
/// Client for managing social publisher workspaces via Posty5 API
/// </summary>
public class SocialPublisherWorkspaceClient
{
    private readonly Posty5HttpClient _http;
    private const string BasePath = "/api/social-publisher-workspace";

    /// <summary>
    /// Creates a new Social Publisher Workspace client
    /// </summary>
    /// <param name="httpClient">HTTP client instance from Posty5.Core</param>
    public SocialPublisherWorkspaceClient(Posty5HttpClient httpClient)
    {
        _http = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    }

    /// <summary>
    /// List/search workspaces with pagination and filters
    /// </summary>
    /// <param name="listParams">Filter parameters</param>
    /// <param name="pagination">Pagination parameters</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paginated list of workspaces</returns>
    public async Task<PaginationResponse<SocialPublisherWorkspaceSampleDetailsModel>> ListAsync(
        SocialPublisherWorkspaceListParamsModel? listParams = null,
        PaginationParams? pagination = null,
        CancellationToken cancellationToken = default)
    {
        var queryParams = new Dictionary<string, object?>();

        if (listParams != null)
        {
            if (!string.IsNullOrEmpty(listParams.Name))
                queryParams["name"] = listParams.Name;
            if (!string.IsNullOrEmpty(listParams.Description))
                queryParams["description"] = listParams.Description;
            if (!string.IsNullOrEmpty(listParams.Tag))
                queryParams["tag"] = listParams.Tag;
            if (!string.IsNullOrEmpty(listParams.RefId))
                queryParams["refId"] = listParams.RefId;
        }

        if (pagination != null)
        {
            if (!string.IsNullOrEmpty(pagination.Cursor))
                queryParams["cursor"] = pagination.Cursor;
            queryParams["pageSize"] = pagination.PageSize;
        }

        var response = await _http.GetAsync<PaginationResponse<SocialPublisherWorkspaceSampleDetailsModel>>(
            BasePath,
            queryParams,
            cancellationToken);

        return response.Result ?? new PaginationResponse<SocialPublisherWorkspaceSampleDetailsModel>();
    }

    /// <summary>
    /// Get a workspace by ID
    /// </summary>
    /// <param name="id">Workspace ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Workspace details with account information</returns>
    public async Task<SocialPublisherWorkspaceModel> GetAsync(
        string id,
        CancellationToken cancellationToken = default)
    {
        var response = await _http.GetAsync<SocialPublisherWorkspaceModel>(
            $"{BasePath}/{id}",
            cancellationToken: cancellationToken);

        return response.Result ?? throw new InvalidOperationException("Workspace not found");
    }

    /// <summary>
    /// Get workspace details for creating new post
    /// </summary>
    /// <param name="id">Workspace ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Workspace details with populated account information</returns>
    /// <example>
    /// <code>
    /// // Get workspace details for post creation
    /// var workspace = await client.GetForNewPostAsync("workspace-id");
    /// 
    /// // Access account details
    /// if (workspace.Account.Youtube != null)
    /// {
    ///     Console.WriteLine($"YouTube: {workspace.Account.Youtube.Name}");
    ///     Console.WriteLine($"Status: {workspace.Account.Youtube.Status}");
    /// }
    /// 
    /// if (workspace.Account.Facebook != null)
    /// {
    ///     Console.WriteLine($"Facebook Page: {workspace.Account.Facebook.Name}");
    /// }
    /// </code>
    /// </example>
    public async Task<SocialPublisherWorkspaceForNewPostModel> GetForNewPostAsync(
        string id,
        CancellationToken cancellationToken = default)
    {
        var response = await _http.GetAsync<SocialPublisherWorkspaceForNewPostModel>(
            $"{BasePath}/{id}/for-new-post",
            cancellationToken: cancellationToken);

        return response.Result ?? throw new InvalidOperationException("Workspace not found");
    }


    /// <summary>
    /// Create a new workspace with optional image upload
    /// </summary>
    /// <param name="data">Workspace data (name, description, etc.)</param>
    /// <param name="logoStream">Optional workspace logo/image stream</param>
    /// <param name="contentType">Image content type (default: image/png)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created workspace ID</returns>
    /// <example>
    /// <code>
    /// // Create without image
    /// var workspaceId = await client.CreateAsync(new CreateWorkspaceRequest
    /// {
    ///     Name = "My Workspace",
    ///     Description = "Workspace description"
    /// });
    /// 
    /// // Create with image
    /// using var logoStream = File.OpenRead("logo.png");
    /// var workspaceId = await client.CreateAsync(
    ///     new CreateWorkspaceRequest { Name = "My Workspace", Description = "..." },
    ///     logoStream,
    ///     "image/png"
    /// );
    /// </code>
    /// </example>
    public async Task<string> CreateAsync(
        SocialPublisherWorkspaceCreateRequestModel data,
        Stream? logoStream = null,
        string contentType = "image/png",
        CancellationToken cancellationToken = default)
    {
        // Step 1: Create workspace and get upload config
        var payload = new
        {
            data.Name,
            data.Description,
            data.Tag,
            data.RefId,
            hasImage = logoStream != null,
            createdFrom = "dotnetPackage"
        };

        var response = await _http.PostAsync<SocialPublisherWorkspaceCreateResponseModel>(BasePath, payload, cancellationToken);
        var result = response.Result ?? throw new InvalidOperationException("Failed to create workspace");

        // Step 2: Upload image if provided
        if (logoStream != null && result.UploadImageConfig != null)
        {
            await UploadImageAsync(result.UploadImageConfig.UploadUrl, logoStream, contentType, cancellationToken);
        }

        return result.WorkspaceId;
    }

    /// <summary>
    /// Update an existing workspace with optional new image upload
    /// </summary>
    /// <param name="id">Workspace ID to update</param>
    /// <param name="data">Updated workspace data</param>
    /// <param name="logoStream">Optional new workspace logo/image stream</param>
    /// <param name="contentType">Image content type (default: image/png)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <example>
    /// <code>
    /// // Update without changing image
    /// await client.UpdateAsync("workspace-id", new UpdateWorkspaceRequest
    /// {
    ///     Name = "Updated Name",
    ///     Description = "Updated description"
    /// });
    /// 
    /// // Update with new image
    /// using var newLogo = File.OpenRead("new-logo.png");
    /// await client.UpdateAsync("workspace-id", request, newLogo);
    /// </code>
    /// </example>
    public async Post UpdateAsync(
        string id,
        SocialPublisherWorkspaceUpdateRequestModel data,
        Stream? logoStream = null,
        string contentType = "image/png",
        CancellationToken cancellationToken = default)
    {
        // Step 1: Update workspace and get upload config
        var payload = new
        {
            data.Name,
            data.Description,
            data.Tag,
            data.RefId,
            hasImage = logoStream != null
        };

        var response = await _http.PutAsync<SocialPublisherWorkspaceCreateResponseModel>($"{BasePath}/{id}", payload, cancellationToken);
        var result = response.Result;

        // Step 2: Upload image if provided
        if (logoStream != null && result?.UploadImageConfig != null)
        {
            await UploadImageAsync(result.UploadImageConfig.UploadUrl, logoStream, contentType, cancellationToken);
        }
    }

    /// <summary>
    /// Delete a workspace
    /// </summary>
    /// <param name="id">Workspace ID to delete</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Deletion confirmation response</returns>
    public async Task<DeleteResponse> DeleteAsync(
        string id,
        CancellationToken cancellationToken = default)
    {
        var response = await _http.DeleteAsync<DeleteResponse>($"{BasePath}/{id}", cancellationToken);
        return response.Result ?? new DeleteResponse { Message = "Deleted" };
    }

    /// <summary>
    /// Upload image to R2 storage using pre-signed URL
    /// </summary>
    /// <param name="uploadUrl">Pre-signed R2 upload URL</param>
    /// <param name="imageStream">Image stream to upload</param>
    /// <param name="contentType">Content type of the image</param>
    /// <param name="cancellationToken">Cancellation token</param>
    private async Post UploadImageAsync(
        string uploadUrl,
        Stream imageStream,
        string contentType,
        CancellationToken cancellationToken)
    {
        using var client = new HttpClient();
        using var content = new StreamContent(imageStream);
        content.Headers.ContentType = new MediaTypeHeaderValue(contentType);

        var response = await client.PutAsync(uploadUrl, content, cancellationToken);
        response.EnsureSuccessStatusCode();
    }
}
