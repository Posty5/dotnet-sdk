using Posty5.Core.Http;
using Posty5.Core.Models;
using Posty5.SocialPublisherWorkspace.Models;

namespace Posty5.SocialPublisherWorkspace;

/// <summary>
/// Read the social accounts connected to Posty5 —
/// <c>/api/social-publisher-account</c>.
/// </summary>
/// <remarks>
/// <para>
/// Read-only. Connecting an account is an OAuth sign-in on the platform and is
/// done in the dashboard; there is no API-key route for it.
/// </para>
/// <para>
/// Which accounts a key sees depends on the key's record scope: a key scoped to
/// <c>key</c> (the default) sees the accounts connected with that key; a key
/// scoped to <c>account</c> sees every account of its owner. Read the scope with
/// <c>Posty5.Account</c>'s <c>GetCurrentAsync</c>.
/// </para>
/// </remarks>
public class SocialPublisherAccountClient
{
    private readonly Posty5HttpClient _http;

    /// <summary>
    /// Creates a new connected-accounts client.
    /// </summary>
    /// <param name="httpClient">HTTP client instance from Posty5.Core</param>
    public SocialPublisherAccountClient(Posty5HttpClient httpClient)
    {
        _http = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    }

    /// <summary>
    /// List connected accounts, newest first, a page at a time.
    /// </summary>
    /// <param name="listParams">Filters; all optional.</param>
    /// <param name="pagination">Cursor and page size; the API's default page size applies when null.</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A page of accounts.</returns>
    public async Task<PaginationResponse<SocialPublisherAccountSampleDetailsModel>> ListAsync(
        SocialPublisherAccountListParamsModel? listParams = null,
        PaginationParams? pagination = null,
        CancellationToken cancellationToken = default)
    {
        var queryParams = new Dictionary<string, object?>();

        if (listParams != null)
        {
            if (!string.IsNullOrEmpty(listParams.Platform))
                queryParams["platform"] = listParams.Platform;
            if (!string.IsNullOrEmpty(listParams.Status))
                queryParams["status"] = listParams.Status;
            if (!string.IsNullOrEmpty(listParams.Name))
                queryParams["name"] = listParams.Name;
        }

        if (pagination != null)
        {
            if (!string.IsNullOrEmpty(pagination.Cursor))
                queryParams["cursor"] = pagination.Cursor;
            queryParams["pageSize"] = pagination.PageSize;
        }

        var response = await _http.GetAsync<PaginationResponse<SocialPublisherAccountSampleDetailsModel>>(
            SocialPublisherAccountRoutes.Base,
            queryParams,
            cancellationToken);

        return response.Result ?? new PaginationResponse<SocialPublisherAccountSampleDetailsModel>();
    }

    /// <summary>
    /// Look accounts up by name — a short list of id, name and picture, for a
    /// picker. Returns one page (the API's default size).
    /// </summary>
    /// <param name="term">Name contains this, case-insensitive; null for any.</param>
    /// <param name="platform">Only this platform, e.g. <c>facebook</c>; null for any.</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The matching accounts.</returns>
    public async Task<List<SocialPublisherAccountLookupItemModel>> LookupAsync(
        string? term = null,
        string? platform = null,
        CancellationToken cancellationToken = default)
    {
        var queryParams = new Dictionary<string, object?>();
        if (!string.IsNullOrEmpty(term))
            queryParams["term"] = term;
        if (!string.IsNullOrEmpty(platform))
            queryParams["platform"] = platform;

        var response = await _http.GetAsync<List<SocialPublisherAccountLookupItemModel>>(
            SocialPublisherAccountRoutes.Lookup,
            queryParams,
            cancellationToken);

        return response.Result ?? new List<SocialPublisherAccountLookupItemModel>();
    }

    /// <summary>
    /// One connected account, with its platform profile and its default post
    /// settings and comments.
    /// </summary>
    /// <param name="id">The account id.</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The account.</returns>
    /// <exception cref="Posty5.Core.Exceptions.Posty5ValidationException">
    /// The account does not exist or is not yours (the API answers 400 for both).
    /// </exception>
    public async Task<SocialPublisherAccountDetailsModel> GetAsync(
        string id,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("id is required", nameof(id));

        var response = await _http.GetAsync<SocialPublisherAccountDetailsModel>(
            $"{SocialPublisherAccountRoutes.Base}/{id}",
            cancellationToken: cancellationToken);

        return response.Result ?? throw new InvalidOperationException("Account not found");
    }
}
