using System.Text.Json.Serialization;

namespace Posty5.Account.Models;

// ============================================================================
// CREDITS — /api/user/current/credits, /credit-usage, /credit-usage/summary
//
// Transcribed from the API's /swagger.json and checked against the handlers:
// where the two disagree (the ledger row's `reference` and `actorUserId` are
// objects, and the summary nests its totals under `range`), the shape here is
// the one the API actually sends, because a mismatched type would make
// deserialization throw.
// ============================================================================

/// <summary>A credit balance for the current period.</summary>
public class CreditBalance
{
    /// <summary>Credits granted by the plan for the current period.</summary>
    public decimal Granted { get; set; }

    /// <summary>Credits bought as top-ups; they survive renewal.</summary>
    public decimal Purchased { get; set; }

    /// <summary>Credits used in the current period.</summary>
    public decimal Used { get; set; }

    /// <summary>Credits owed by deferred charges; never spendable.</summary>
    public decimal Debt { get; set; }

    /// <summary><c>Granted + Purchased − Used</c>, floored at 0.</summary>
    public decimal Remaining { get; set; }

    /// <summary>What can actually be spent now: <c>Remaining − Debt</c>, floored at 0.</summary>
    public decimal Spendable { get; set; }

    /// <summary>When the current period started.</summary>
    public DateTime? PeriodStartedAt { get; set; }

    /// <summary>When the current period ends.</summary>
    public DateTime? PeriodEndsAt { get; set; }
}

/// <summary>The key owner's balance and how this period's credits were spent per feature.</summary>
public class AccountCredits
{
    /// <summary>The balance.</summary>
    public CreditBalance Credits { get; set; } = new();

    /// <summary>Per feature: how often it ran this period and what it cost.</summary>
    public List<CreditUsageStat> UsageStats { get; set; } = new();
}

/// <summary>One feature's usage in the current period.</summary>
public class CreditUsageStat
{
    /// <summary>The feature, e.g. <c>socialMediaPublisher.textPost</c>.</summary>
    public string? FeaturePath { get; set; }

    /// <summary>How many times it ran this period.</summary>
    public int Count { get; set; }

    /// <summary>Credits spent on it this period.</summary>
    public decimal CreditsSpent { get; set; }
}

/// <summary>
/// Filters for the credit history and its summary. Every field is optional.
/// </summary>
public class CreditUsageListParams
{
    /// <summary>The module an operation belongs to, e.g. <c>socialMediaPublisher</c>.</summary>
    public string? Module { get; set; }

    /// <summary>The operation within the module, e.g. <c>textPost</c>.</summary>
    public string? OperationType { get; set; }

    /// <summary>What produced the row — one of <see cref="CreditUsageKinds"/>.</summary>
    public string? Kind { get; set; }

    /// <summary><c>false</c> lists deferred charges not yet paid; <c>true</c> only paid ones.</summary>
    public bool? Settled { get; set; }

    /// <summary>
    /// Only operations charged for this store. Applied by the history; the
    /// summary does not filter by store.
    /// </summary>
    public string? StoreId { get; set; }

    /// <summary>Created-at range start, <c>yyyy-MM-dd</c>. Only applied together with <see cref="ToDate"/>.</summary>
    public string? FromDate { get; set; }

    /// <summary>Created-at range end, <c>yyyy-MM-dd</c>. Only applied together with <see cref="FromDate"/>.</summary>
    public string? ToDate { get; set; }
}

/// <summary>One row of the credit ledger: a charge, a grant, a refund…</summary>
public class CreditUsageEntry
{
    /// <summary>The row's id.</summary>
    [JsonPropertyName("_id")]
    public string Id { get; set; } = string.Empty;

    /// <summary>What produced the row — one of <see cref="CreditUsageKinds"/>.</summary>
    public string? Kind { get; set; }

    /// <summary>Which way the credits moved: <see cref="CreditDirections.In"/> or <see cref="CreditDirections.Out"/>.</summary>
    public string? Direction { get; set; }

    /// <summary>The module the operation belongs to.</summary>
    public string? Module { get; set; }

    /// <summary>The operation within the module.</summary>
    public string? OperationType { get; set; }

    /// <summary>A readable reason, as the usage tab shows it.</summary>
    public string? Reason { get; set; }

    /// <summary>Credits recorded — the full value, even for a deferred charge not yet paid.</summary>
    public decimal Credits { get; set; }

    /// <summary><see cref="Credits"/> with its sign, e.g. <c>-25</c> or <c>+1200</c>.</summary>
    public string? SignedCredits { get; set; }

    /// <summary>The spendable balance just before the row.</summary>
    public decimal? BalanceBefore { get; set; }

    /// <summary>The spendable balance just after the row (unchanged for a deferred charge).</summary>
    public decimal? BalanceAfter { get; set; }

    /// <summary><c>false</c> for a deferred charge still owed.</summary>
    public bool Settled { get; set; }

    /// <summary>When a deferred charge was paid.</summary>
    public DateTime? SettledAt { get; set; }

    /// <summary>The record the operation acted on, when there is one.</summary>
    public CreditReference? Reference { get; set; }

    /// <summary>
    /// Who performed the operation, when it was not the payer — e.g. a store's
    /// staff member. Null for system and visitor-triggered rows.
    /// </summary>
    [JsonPropertyName("actorUserId")]
    public CreditUsageActor? Actor { get; set; }

    /// <summary>Set on rows made by Posty5 support (adjustments, refunds).</summary>
    public string? AdminUserId { get; set; }

    /// <summary>When the row was written.</summary>
    public DateTime CreatedAt { get; set; }
}

/// <summary>The record a credit-ledger row links back to.</summary>
public class CreditReference
{
    /// <summary>What kind of record, e.g. <c>storeOrder</c>, <c>shortLink</c>, <c>socialPost</c>.</summary>
    public string? Type { get; set; }

    /// <summary>The record's id.</summary>
    public string? Id { get; set; }

    /// <summary>A label for it, e.g. an order number.</summary>
    public string? Label { get; set; }
}

/// <summary>The user who performed an operation on someone else's credits.</summary>
public class CreditUsageActor
{
    /// <summary>The user's id.</summary>
    [JsonPropertyName("_id")]
    public string Id { get; set; } = string.Empty;

    /// <summary>The user's login name.</summary>
    public string? UserName { get; set; }

    /// <summary>The user's full name.</summary>
    public string? FullName { get; set; }
}

/// <summary>
/// The balance plus totals over the filtered part of the credit history.
/// </summary>
public class CreditUsageSummary
{
    /// <summary>What can be spent now (the balance's <c>Spendable</c>).</summary>
    public decimal Balance { get; set; }

    /// <summary><c>Granted + Purchased − Used</c>, floored at 0.</summary>
    public decimal Remaining { get; set; }

    /// <summary>Credits owed by deferred charges.</summary>
    public decimal Debt { get; set; }

    /// <summary>Credits granted by the plan for the current period.</summary>
    public decimal Granted { get; set; }

    /// <summary>Credits bought as top-ups.</summary>
    public decimal Purchased { get; set; }

    /// <summary>Credits used in the current period.</summary>
    public decimal Used { get; set; }

    /// <summary>How <see cref="Remaining"/> divides between the plan's allowance and purchased credits.</summary>
    public CreditRemainingBreakdown? RemainingBreakdown { get; set; }

    /// <summary>When the current period started.</summary>
    public DateTime? PeriodStartedAt { get; set; }

    /// <summary>When the current period ends.</summary>
    public DateTime? PeriodEndsAt { get; set; }

    /// <summary>Totals over the rows matching the filters.</summary>
    public CreditRangeTotals Range { get; set; } = new();
}

/// <summary>How the remaining credits divide. The two always add up to the remaining balance.</summary>
public class CreditRemainingBreakdown
{
    /// <summary>Left of the plan's allowance. Reset at the next renewal.</summary>
    public decimal Monthly { get; set; }

    /// <summary>Left of what was bought. Survives renewal.</summary>
    public decimal Purchased { get; set; }
}

/// <summary>Totals over a filtered part of the credit history.</summary>
public class CreditRangeTotals
{
    /// <summary>How many charges.</summary>
    public int Operations { get; set; }

    /// <summary>Credits paid by those charges.</summary>
    public decimal CreditsSpent { get; set; }

    /// <summary>Credits those charges still owe (deferred, unpaid).</summary>
    public decimal CreditsOwed { get; set; }

    /// <summary>Credits added — grants, carry-overs and refunds.</summary>
    public decimal CreditsAdded { get; set; }
}
