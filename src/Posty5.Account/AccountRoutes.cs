namespace Posty5.Account;

/// <summary>
/// The API routes <see cref="AccountClient"/> calls, in one place rather than
/// inline in the client. They sit under three services' base paths — the key,
/// the user and the plans — and this package is the one place an SDK user
/// reads all three from.
/// </summary>
internal static class AccountRoutes
{
    /// <summary>Who the calling API key is: key, owner, plan, credits, MCP settings.</summary>
    public const string CurrentApiKey = "/api/api-key/current";

    /// <summary>The key owner's credit balance and per-feature counters.</summary>
    public const string Credits = "/api/user/current/credits";

    /// <summary>The key owner's credit history, newest first.</summary>
    public const string CreditUsage = "/api/user/current/credit-usage";

    /// <summary>Totals over the key owner's credit history.</summary>
    public const string CreditUsageSummary = "/api/user/current/credit-usage/summary";

    /// <summary>The live price of every operation, grouped by module. Public.</summary>
    public const string OperationCosts = "/api/plans/operation-costs";
}
