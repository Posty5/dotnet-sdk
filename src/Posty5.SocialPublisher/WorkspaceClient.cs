using Posty5.Core.Http;
using Posty5.Core.Models;
using Posty5.SocialPublisher.Models;

namespace Posty5.SocialPublisher;

/// <summary>
/// Client for managing social publisher workspaces via Posty5 API
/// </summary>
public class WorkspaceClient
{
    private readonly Posty5HttpClient _http;
    private const string BasePath = "/api/social-publisher/workspace";

    /// <summary>
    /// Creates a new Workspace client
    /// </summary>
    /// <param name="httpClient">HTTP client instance from Posty5.Core</param>
    public WorkspaceClient(Posty5HttpClient httpClient)
    {
        _http = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    }

    /// <summary>
    /// List workspaces with pagination and filters
    /// </summary>
    /// <param name="listParams">Filter parameters</param>
    /// <param name="pagination">Pagination parameters</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paginated list of workspaces</returns>
    public async Task<PaginationResponse<WorkspaceModel>> ListAsync(
        ListWorkspacesParams? listParams = null,
        PaginationParams? pagination = null,
        CancellationToken cancellationToken = default)
    {
        var queryParams = new Dictionary<string, object?>();

        if (listParams != null)
        {
            if (!string.IsNullOrEmpty(listParams.Search))
                queryParams["search"] = listParams.Search;
        }

        if (pagination != null)
        {
            queryParams["pageNumber"] = pagination.PageNumber;
            queryParams["pageSize"] = pagination.PageSize;
        }

        var response = await _http.GetAsync<PaginationResponse<WorkspaceModel>>(
            BasePath, 
            queryParams, 
            cancellationToken);
        
        return response.Result ?? new PaginationResponse<WorkspaceModel>();
    }

    /// <summary>
    /// Get a workspace by ID
    /// </summary>
    /// <param name="id">Workspace ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Workspace details</returns>
    public async Task<WorkspaceModel> GetAsync(string id, CancellationToken cancellationToken = default)
    {
        var response = await _http.GetAsync<WorkspaceModel>($"{BasePath}/{id}", cancellationToken: cancellationToken);
        return response.Result ?? throw new InvalidOperationException("Workspace not found");
    }

    /// <summary>
    /// Create a new workspace
    /// </summary>
    /// <param name="request">Create request data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created workspace</returns>
    public async Task<WorkspaceModel> CreateAsync(
        CreateWorkspaceRequest request,
        CancellationToken cancellationToken = default)
    {
        var response = await _http.PostAsync<WorkspaceModel>(BasePath, request, cancellationToken);
        return response.Result ?? throw new InvalidOperationException("Failed to create workspace");
    }

    /// <summary>
    /// Update an existing workspace
    /// </summary>
    /// <param name="id">Workspace ID</param>
    /// <param name="request">Update request data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated workspace</returns>
    public async Task<WorkspaceModel> UpdateAsync(
        string id,
        UpdateWorkspaceRequest request,
        CancellationToken cancellationToken = default)
    {
        var response = await _http.PutAsync<WorkspaceModel>($"{BasePath}/{id}", request, cancellationToken);
        return response.Result ?? throw new InvalidOperationException("Failed to update workspace");
    }

    /// <summary>
    /// Delete a workspace
    /// </summary>
    /// <param name="id">Workspace ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if deleted successfully</returns>
    public async Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        var response = await _http.DeleteAsync<object>($"{BasePath}/{id}", cancellationToken);
        return response.IsSuccess;
    }
}
