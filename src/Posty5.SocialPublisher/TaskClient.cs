using Posty5.Core.Http;
using Posty5.Core.Models;
using Posty5.SocialPublisher.Models;

namespace Posty5.SocialPublisher;

/// <summary>
/// Client for managing social publisher tasks via Posty5 API
/// </summary>
public class TaskClient
{
    private readonly Posty5HttpClient _http;
    private const string BasePath = "/api/social-publisher/task";

    /// <summary>
    /// Creates a new Task client
    /// </summary>
    /// <param name="httpClient">HTTP client instance from Posty5.Core</param>
    public TaskClient(Posty5HttpClient httpClient)
    {
        _http = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    }

    /// <summary>
    /// List tasks with pagination and filters
    /// </summary>
    /// <param name="listParams">Filter parameters</param>
    /// <param name="pagination">Pagination parameters</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paginated list of tasks</returns>
    public async Task<PaginationResponse<TaskModel>> ListAsync(
        ListTasksParams? listParams = null,
        PaginationParams? pagination = null,
        CancellationToken cancellationToken = default)
    {
        var queryParams = new Dictionary<string, object?>();

        if (listParams != null)
        {
            if (!string.IsNullOrEmpty(listParams.WorkspaceId))
                queryParams["workspaceId"] = listParams.WorkspaceId;
            if (listParams.Status.HasValue)
                queryParams["status"] = listParams.Status.Value.ToString();
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

        var response = await _http.GetAsync<PaginationResponse<TaskModel>>(
            BasePath, 
            queryParams, 
            cancellationToken);
        
        return response.Result ?? new PaginationResponse<TaskModel>();
    }

    /// <summary>
    /// Get a task by ID
    /// </summary>
    /// <param name="id">Task ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task details</returns>
    public async Task<TaskModel> GetAsync(string id, CancellationToken cancellationToken = default)
    {
        var response = await _http.GetAsync<TaskModel>($"{BasePath}/{id}", cancellationToken: cancellationToken);
        return response.Result ?? throw new InvalidOperationException("Task not found");
    }

    /// <summary>
    /// Create a new task
    /// </summary>
    /// <param name="request">Create request data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created task</returns>
    public async Task<TaskModel> CreateAsync(
        CreateTaskRequest request,
        CancellationToken cancellationToken = default)
    {
        var response = await _http.PostAsync<TaskModel>(BasePath, request, cancellationToken);
        return response.Result ?? throw new InvalidOperationException("Failed to create task");
    }

    /// <summary>
    /// Update an existing task
    /// </summary>
    /// <param name="id">Task ID</param>
    /// <param name="request">Update request data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated task</returns>
    public async Task<TaskModel> UpdateAsync(
        string id,
        UpdateTaskRequest request,
        CancellationToken cancellationToken = default)
    {
        var response = await _http.PutAsync<TaskModel>($"{BasePath}/{id}", request, cancellationToken);
        return response.Result ?? throw new InvalidOperationException("Failed to update task");
    }

    /// <summary>
    /// Delete a task
    /// </summary>
    /// <param name="id">Task ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if deleted successfully</returns>
    public async Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        var response = await _http.DeleteAsync<object>($"{BasePath}/{id}", cancellationToken);
        return response.IsSuccess;
    }

    /// <summary>
    /// Publish a task immediately
    /// </summary>
    /// <param name="id">Task ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated task with published status</returns>
    public async Task<TaskModel> PublishAsync(string id, CancellationToken cancellationToken = default)
    {
        var response = await _http.PostAsync<TaskModel>($"{BasePath}/{id}/publish", new { }, cancellationToken);
        return response.Result ?? throw new InvalidOperationException("Failed to publish task");
    }
}
