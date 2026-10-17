using System.Text.Json.Serialization;

namespace Posty5.Account.Models;

// ============================================================================
// CREDIT TRANSFERS — /api/credit-transfers (read-only for an API key)
//
// A Business user sends credits to another Posty5 account from the dashboard;
// an API key cannot send (the API answers 403 SESSION_REQUIRED). These are the
// read shapes: eligibility, the list and one transfer.
// ============================================================================

/// <summary>A credit transfer the key owner sent or received.</summary>
public class CreditTransfer
{
    /// <summary>The transfer's id.</summary>
    [JsonPropertyName("_id")]
    public string Id { get; set; } = string.Empty;

    /// <summary>The document version.</summary>
    [JsonPropertyName("__v")]
    public int Version { get; set; }

    /// <summary><see cref="CreditTransferDirections.Sent"/> or <see cref="CreditTransferDirections.Received"/>.</summary>
    public string? Direction { get; set; }

    /// <summary>Whole credits.</summary>
    public decimal Amount { get; set; }

    /// <summary>The sender's optional note (plain text, at most 140 characters).</summary>
    public string? Note { get; set; }

    /// <summary>One of <see cref="CreditTransferStatuses"/>.</summary>
    public string? Status { get; set; }

    /// <summary>The other party, masked by the API.</summary>
    public CreditTransferCounterparty Counterparty { get; set; } = new();

    /// <summary>When the transfer was created.</summary>
    public DateTime? CreatedAt { get; set; }

    /// <summary>When the recipient was credited.</summary>
    public DateTime? CompletedAt { get; set; }

    /// <summary>When Posty5 support reversed it, if it did.</summary>
    public DateTime? ReversedAt { get; set; }
}

/// <summary>The other party of a transfer — never their id or full email.</summary>
public class CreditTransferCounterparty
{
    /// <summary>First name plus last initial, e.g. <c>Ahmed N.</c>; may be empty.</summary>
    public string? DisplayName { get; set; }

    /// <summary>e.g. <c>a***d@g***.com</c>.</summary>
    public string? EmailMasked { get; set; }
}

/// <summary>Whether the key owner can send or receive credit transfers (<c>GET /api/credit-transfers/eligibility</c>).</summary>
public class CreditTransferEligibility
{
    /// <summary>True on an active Business plan. Sending itself is done in the dashboard.</summary>
    public bool CanSend { get; set; }

    /// <summary>Why the owner cannot send, when <see cref="CanSend"/> is false.</summary>
    public CreditTransferRefusal? SendRefusal { get; set; }

    /// <summary>What the owner could spend now.</summary>
    public decimal Spendable { get; set; }

    /// <summary>What may be sent now.</summary>
    public decimal Transferable { get; set; }

    /// <summary>The smallest transfer.</summary>
    public decimal MinAmount { get; set; }

    /// <summary>The largest transfer (the transferable balance).</summary>
    public decimal MaxAmount { get; set; }

    /// <summary>Present only when a daily cap is configured.</summary>
    public decimal? DailyRemaining { get; set; }

    /// <summary>Whether the owner can still receive a transfer.</summary>
    public bool CanReceive { get; set; }

    /// <summary>1 on Free; null when unlimited.</summary>
    public int? FreeReceiveLimit { get; set; }

    /// <summary>0 or 1.</summary>
    public int FreeReceivesUsed { get; set; }
}

/// <summary>A refusal: a machine-readable code (<see cref="CreditTransferRefusalCodes"/>) and the API's message.</summary>
public class CreditTransferRefusal
{
    /// <summary>One of <see cref="CreditTransferRefusalCodes"/>.</summary>
    public string? Code { get; set; }

    /// <summary>The API's message.</summary>
    public string? Message { get; set; }

    /// <summary>On <see cref="CreditTransferRefusalCodes.PlanRequired"/>: the lowest plan that can send (<c>enterprise</c>, shown as Business).</summary>
    public string? RequiredPlan { get; set; }
}

/// <summary>Filters for <see cref="AccountClient.ListCreditTransfersAsync"/>; all optional.</summary>
public class CreditTransferListParams
{
    /// <summary><c>sent</c>, <c>received</c> or <c>all</c> (the default).</summary>
    public string? Direction { get; set; }

    /// <summary>One of <see cref="CreditTransferStatuses"/>.</summary>
    public string? Status { get; set; }

    /// <summary><c>YYYY-MM-DD</c>, with <see cref="ToDate"/>.</summary>
    public string? FromDate { get; set; }

    /// <summary><c>YYYY-MM-DD</c>, with <see cref="FromDate"/>.</summary>
    public string? ToDate { get; set; }
}

/// <summary>Which side of a transfer the key owner is on.</summary>
public static class CreditTransferDirections
{
    /// <summary>The owner sent the credits.</summary>
    public const string Sent = "sent";

    /// <summary>The owner received the credits.</summary>
    public const string Received = "received";

    /// <summary>Both (a list filter only).</summary>
    public const string All = "all";
}

/// <summary>A transfer's lifecycle, as the API spells it.</summary>
public static class CreditTransferStatuses
{
    /// <summary>Created; the sender is not debited yet.</summary>
    public const string Pending = "pending";

    /// <summary>The sender is debited; the recipient is not credited yet.</summary>
    public const string Debited = "debited";

    /// <summary>Both sides moved.</summary>
    public const string Completed = "completed";

    /// <summary>Nothing moved, or the sender was refunded.</summary>
    public const string Failed = "failed";

    /// <summary>Posty5 support reversed it.</summary>
    public const string Reversed = "reversed";
}

/// <summary>The refusal codes of <c>/api/credit-transfers</c>.</summary>
public static class CreditTransferRefusalCodes
{
    /// <summary>The plan does not include sending (Business only).</summary>
    public const string PlanRequired = "PLAN_REQUIRED";

    /// <summary>The Business plan has ended.</summary>
    public const string PlanExpired = "PLAN_EXPIRED";

    /// <summary>The call used an API key; sending needs a signed-in session.</summary>
    public const string SessionRequired = "SESSION_REQUIRED";

    /// <summary>Not a whole number, or outside the allowed range.</summary>
    public const string AmountInvalid = "AMOUNT_INVALID";

    /// <summary>More than the transferable balance.</summary>
    public const string Insufficient = "INSUFFICIENT";

    /// <summary>The daily cap was reached.</summary>
    public const string DailyLimit = "DAILY_LIMIT";

    /// <summary>The recipient is the caller.</summary>
    public const string SelfTransfer = "SELF_TRANSFER";

    /// <summary>The account cannot receive (one generic code, by design).</summary>
    public const string RecipientUnavailable = "RECIPIENT_UNAVAILABLE";
}
