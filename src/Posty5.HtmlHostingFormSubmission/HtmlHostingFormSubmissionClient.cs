using Posty5.Core.Http;
using Posty5.Core.Models;
using Posty5.HtmlHostingFormSubmission.Models;

namespace Posty5.HtmlHostingFormSubmission;

/// <summary>
/// Client for managing HTML hosting form submissions via Posty5 API
/// </summary>
public class HtmlHostingFormSubmissionClient
{
    private readonly Posty5HttpClient _http;
    private const string BasePath = "/api/html-hosting-form-submission";

    /// <summary>
    /// Creates a new HTML Hosting Form Submission client
    /// </summary>
    /// <param name="httpClient">HTTP client instance from Posty5.Core</param>
    public HtmlHostingFormSubmissionClient(Posty5HttpClient httpClient)
    {
        _http = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    }

    /// <summary>
    /// Get a form submission by ID
    /// </summary>
    /// <param name="id">Submission ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Form submission details with form data</returns>
    public async Task<HtmlHostingFormSubmissionModel> GetAsync(
        string id,
        CancellationToken cancellationToken = default)
    {
        var response = await _http.GetAsync<HtmlHostingFormSubmissionModel>(
            $"{BasePath}/{id}",
            cancellationToken: cancellationToken);

        return response.Result ?? throw new InvalidOperationException("Form submission not found");
    }

    /// <summary>
    /// Get next and previous form submissions for navigation
    /// </summary>
    /// <param name="id">Current submission ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Next and previous submission references (if they exist)</returns>
    public async Task<HtmlHostingFormSubmissionNextPreviousResponseModel> GetNextPreviousAsync(
        string id,
        CancellationToken cancellationToken = default)
    {
        var response = await _http.GetAsync<HtmlHostingFormSubmissionNextPreviousResponseModel>(
            $"{BasePath}/{id}/next-previous",
            cancellationToken: cancellationToken);

        return response.Result ?? new HtmlHostingFormSubmissionNextPreviousResponseModel();
    }

    /// <summary>
    /// List form submissions with pagination and optional filters
    /// </summary>
    /// <param name="listParams">Filter parameters (htmlHostingId is required)</param>
    /// <param name="pagination">Pagination parameters</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paginated list of form submissions</returns>
    public async Task<PaginationResponse<HtmlHostingFormSubmissionModel>> ListAsync(
        HtmlHostingFormSubmissionListParamsModel listParams,
        PaginationParams? pagination = null,
        CancellationToken cancellationToken = default)
    {
        var queryParams = new Dictionary<string, object?>
        {
            ["htmlHostingId"] = listParams.HtmlHostingId
        };

        if (!string.IsNullOrEmpty(listParams.FormId))
            queryParams["formId"] = listParams.FormId;
        if (!string.IsNullOrEmpty(listParams.Numbering))
            queryParams["numbering"] = listParams.Numbering;
        if (!string.IsNullOrEmpty(listParams.Status?.ToString()))
            queryParams["status"] = listParams.Status.ToString();
        if (!string.IsNullOrEmpty(listParams.FilteredFields))
            queryParams["filteredFields"] = listParams.FilteredFields;

        if (pagination != null)
        {
            queryParams["page"] = pagination.Page;
            queryParams["pageSize"] = pagination.PageSize;
        }

        var response = await _http.GetAsync<PaginationResponse<HtmlHostingFormSubmissionModel>>(
            BasePath,
            queryParams,
            cancellationToken);

        return response.Result ?? new PaginationResponse<HtmlHostingFormSubmissionModel>();
    }

    /// <summary>
    /// Change the status of a form submission
    /// </summary>
    /// <param name="id">Submission ID</param>
    /// <param name="request">Status change request containing new status, rejection reason, and notes</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Response with updated status history</returns>
    /// <exception cref="ArgumentNullException">Thrown when id or request is null</exception>
    /// <exception cref="InvalidOperationException">Thrown when submission not found or user doesn't have permission</exception>
    public async Task<HtmlHostingFormSubmissionChangeStatusResponseModel> ChangeStatusAsync(
        string id,
        HtmlHostingFormSubmissionChangeStatusRequestModel request,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(id))
            throw new ArgumentNullException(nameof(id));
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        var response = await _http.PutAsync<HtmlHostingFormSubmissionChangeStatusResponseModel>(
            $"{BasePath}/{id}/status",
            request,
            cancellationToken: cancellationToken);

        return response.Result ?? throw new InvalidOperationException("Failed to change status");
    }

    /// <summary>
    /// Delete a form submission
    /// </summary>
    /// <param name="id">Submission ID to delete</param>
    /// <param name="cancellationToken">Cancellation token</param>
    public async Task DeleteAsync(
        string id,
        CancellationToken cancellationToken = default)
    {
        await _http.DeleteAsync<object>($"{BasePath}/{id}", cancellationToken);
    }
}
