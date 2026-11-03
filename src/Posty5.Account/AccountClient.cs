using Posty5.Core.Helpers;
using Posty5.Account.Models;
using Posty5.Core.Http;
using Posty5.Core.Models;

namespace Posty5.Account;

/// <summary>
/// Who the API key belongs to, what the owner can spend, where the credits went,
/// and what each operation costs.
/// </summary>
/// <remarks>
/// <para>
/// Read-only, and nothing here is charged. Every method except
/// <see cref="GetOperationCostsAsync"/> describes the owner of the key in
/// <c>X-API-Key</c>; the price list is public.
/// </para>
/// <para>
/// Call <see cref="GetCurrentAsync"/> first to check a key: an unknown or
/// revoked key answers 401 (<c>Posty5AuthenticationException</c>).
/// </para>
/// </remarks>
/// <example>
/// <code>
/// var http = new Posty5HttpClient(new Posty5Options { ApiKey = apiKey });
/// var account = new AccountClient(http);
///
/// var me = await account.GetCurrentAsync();
/// Console.WriteLine($"{me.User.UserName} on {me.Plan?.Name}: {me.Credits.Spendable} credits");
///
/// var costs = await account.GetOperationCostsAsync();
/// </code>
/// </example>
public class AccountClient
{
    private readonly Posty5HttpClient _http;

    /// <summary>
    /// Creates a new Account client.
    /// </summary>
    /// <param name="httpClient">HTTP client instance from Posty5.Core</param>
    public AccountClient(Posty5HttpClient httpClient)
    {
        _http = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    }

    /// <summary>
    /// Who am I: the calling API key, its owner, the owner's plan and credits,
    /// and the key's MCP settings (<c>GET /api/api-key/current</c>).
    /// </summary>
    /// <remarks>
    /// Use it to validate a key and to read the spendable balance before a paid
    /// call. <see cref="AccountApiKey.RecordScope"/> says which records the key's
    /// lists return. Carries no email, phone or address.
    /// </remarks>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The key and its owner.</returns>
    /// <exception cref="Posty5.Core.Exceptions.Posty5AuthenticationException">The key is missing, unknown or revoked.</exception>
    public async Task<AccountInfo> GetCurrentAsync(CancellationToken cancellationToken = default)
    {
        var response = await _http.GetAsync<AccountInfo>(AccountRoutes.CurrentApiKey, cancellationToken: cancellationToken);
        return response.Result ?? throw new InvalidOperationException("The API returned no account information");
    }

    /// <summary>
    /// The key owner's credit balance, and how this period's credits were spent
    /// per feature (<c>GET /api/user/current/credits</c>).
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The balance and the per-feature counters.</returns>
    public async Task<AccountCredits> GetCreditsAsync(CancellationToken cancellationToken = default)
    {
        var response = await _http.GetAsync<AccountCredits>(AccountRoutes.Credits, cancellationToken: cancellationToken);
        return response.Result ?? new AccountCredits();
    }

    /// <summary>
    /// The key owner's credit history, newest first, a page at a time
    /// (<c>GET /api/user/current/credit-usage</c>).
    /// </summary>
    /// <remarks>
    /// Lists every row the owner pays for; a row records who performed the
    /// operation in <see cref="CreditUsageEntry.Actor"/>.
    /// </remarks>
    /// <param name="filters">Filters; all optional.</param>
    /// <param name="pagination">Cursor and page size; the API's default page size applies when null.</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A page of ledger rows.</returns>
    public async Task<PaginationResponse<CreditUsageEntry>> GetCreditUsageAsync(
        CreditUsageListParams? filters = null,
        PaginationParams? pagination = null,
        CancellationToken cancellationToken = default)
    {
        var queryParams = BuildCreditUsageQuery(filters);

        if (pagination != null)
        {
            if (!string.IsNullOrEmpty(pagination.Cursor))
                queryParams["cursor"] = pagination.Cursor;
            queryParams["pageSize"] = pagination.PageSize;
        }

        var response = await _http.GetAsync<PaginationResponse<CreditUsageEntry>>(
            AccountRoutes.CreditUsage,
            queryParams,
            cancellationToken);

        return response.Result ?? new PaginationResponse<CreditUsageEntry>();
    }

    /// <summary>
    /// The balance plus totals over the credit history the filters match —
    /// charges, credits spent, credits still owed, credits added
    /// (<c>GET /api/user/current/credit-usage/summary</c>).
    /// </summary>
    /// <remarks>
    /// Takes the same filters as <see cref="GetCreditUsageAsync"/>.
    /// </remarks>
    /// <param name="filters">Filters; all optional.</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The balance and the totals.</returns>
    public async Task<CreditUsageSummary> GetCreditUsageSummaryAsync(
        CreditUsageListParams? filters = null,
        CancellationToken cancellationToken = default)
    {
        var response = await _http.GetAsync<CreditUsageSummary>(
            AccountRoutes.CreditUsageSummary,
            BuildCreditUsageQuery(filters),
            cancellationToken);

        return response.Result ?? new CreditUsageSummary();
    }

    /// <summary>
    /// The live price of every paid operation, in credits, grouped by module
    /// (<c>GET /api/plans/operation-costs</c>). Public — needs no API key.
    /// </summary>
    /// <remarks>
    /// Prices change; read them here rather than hard-coding them. The price is
    /// what the same API call charges, whether it comes from the SDK, the
    /// dashboard or an MCP assistant.
    /// </remarks>
    /// <param name="activeOnly">
    /// True (the default) lists enabled operations only; false lists disabled
    /// ones too.
    /// </param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The price list.</returns>
    public async Task<OperationCostCatalog> GetOperationCostsAsync(
        bool activeOnly = true,
        CancellationToken cancellationToken = default)
    {
        // The API's default is activeOnly=true, so only the opt-out travels.
        var queryParams = new Dictionary<string, object?>();
        if (!activeOnly)
            queryParams["activeOnly"] = "false";

        var response = await _http.GetAsync<OperationCostCatalog>(
            AccountRoutes.OperationCosts,
            queryParams,
            cancellationToken);

        return response.Result ?? new OperationCostCatalog();
    }

    /// <summary>
    /// Whether the key owner can send credits to another Posty5 account
    /// (Business plan only), how many, and whether they can still receive one
    /// (<c>GET /api/credit-transfers/eligibility</c>).
    /// </summary>
    /// <remarks>
    /// Read-only. Sending is done from the Posty5 dashboard by a signed-in
    /// person; an API key cannot send (the API answers 403
    /// <see cref="CreditTransferRefusalCodes.SessionRequired"/>). A Free account
    /// can receive one transfer.
    /// </remarks>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The send and receive status.</returns>
    public async Task<CreditTransferEligibility> GetCreditTransferEligibilityAsync(CancellationToken cancellationToken = default)
    {
        var response = await _http.GetAsync<CreditTransferEligibility>(AccountRoutes.CreditTransferEligibility, cancellationToken: cancellationToken);
        return response.Result ?? new CreditTransferEligibility();
    }

    /// <summary>
    /// Credits the key owner sent to or received from other Posty5 accounts,
    /// newest first (<c>GET /api/credit-transfers</c>). Counterparties are masked.
    /// </summary>
    /// <param name="filters">Filters; all optional.</param>
    /// <param name="pagination">Cursor and page size; the API's default page size applies when null.</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A page of transfers.</returns>
    public async Task<PaginationResponse<CreditTransfer>> ListCreditTransfersAsync(
        CreditTransferListParams? filters = null,
        PaginationParams? pagination = null,
        CancellationToken cancellationToken = default)
    {
        var queryParams = new Dictionary<string, object?>();
        if (!string.IsNullOrEmpty(filters?.Direction))
            queryParams["direction"] = filters.Direction;
        if (!string.IsNullOrEmpty(filters?.Status))
            queryParams["status"] = filters.Status;
        if (!string.IsNullOrEmpty(filters?.FromDate))
            queryParams["fromDate"] = filters.FromDate;
        if (!string.IsNullOrEmpty(filters?.ToDate))
            queryParams["toDate"] = filters.ToDate;
        if (pagination != null)
        {
            if (!string.IsNullOrEmpty(pagination.Cursor))
                queryParams["cursor"] = pagination.Cursor;
            queryParams["pageSize"] = pagination.PageSize;
        }

        var response = await _http.GetAsync<PaginationResponse<CreditTransfer>>(AccountRoutes.CreditTransfers, queryParams, cancellationToken);
        return response.Result ?? new PaginationResponse<CreditTransfer>();
    }

    /// <summary>
    /// One transfer the key owner sent or received (<c>GET /api/credit-transfers/{id}</c>).
    /// A transfer they are not a party to answers 404.
    /// </summary>
    /// <param name="id">The transfer id.</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The transfer.</returns>
    public async Task<CreditTransfer> GetCreditTransferAsync(string id, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException("A transfer id is required", nameof(id));
        var response = await _http.GetAsync<CreditTransfer>($"{AccountRoutes.CreditTransfers}/{PathSegmentHelper.Escape(id, nameof(id))}", cancellationToken: cancellationToken);
        return response.Result ?? throw new InvalidOperationException("The API returned no credit transfer");
    }

    /// <summary>
    /// The query string for the credit history and its summary. Booleans go as
    /// the lower-case strings the API reads.
    /// </summary>
    private static Dictionary<string, object?> BuildCreditUsageQuery(CreditUsageListParams? filters)
    {
        var queryParams = new Dictionary<string, object?>();
        if (filters == null) return queryParams;

        if (!string.IsNullOrEmpty(filters.Module))
            queryParams["module"] = filters.Module;
        if (!string.IsNullOrEmpty(filters.OperationType))
            queryParams["operationType"] = filters.OperationType;
        if (!string.IsNullOrEmpty(filters.Kind))
            queryParams["kind"] = filters.Kind;
        if (filters.Settled.HasValue)
            queryParams["settled"] = filters.Settled.Value ? "true" : "false";
        if (!string.IsNullOrEmpty(filters.FromDate))
            queryParams["fromDate"] = filters.FromDate;
        if (!string.IsNullOrEmpty(filters.ToDate))
            queryParams["toDate"] = filters.ToDate;

        return queryParams;
    }
}
