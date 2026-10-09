namespace Posty5.Account.Models;

// Account vocabularies, as the API spells them.
//
// Strings with named constants, never enums: a value added on the server must
// not make deserialization throw in an application that has not upgraded yet.

/// <summary>Which records an API key sees and edits (<see cref="AccountApiKey.RecordScope"/>).</summary>
public static class ApiKeyRecordScopes
{
    /// <summary>Only the records this key created. The default for every key.</summary>
    public const string Key = "key";

    /// <summary>Every record of the key's owner, whoever created it.</summary>
    public const string Account = "account";
}

/// <summary>What an MCP connection on the key may do (<see cref="AccountMcpConnection.Access"/>).</summary>
public static class McpAccessLevels
{
    /// <summary>Read-only tools.</summary>
    public const string Read = "read";

    /// <summary>Plus create, update, publish, reschedule and status changes. The default.</summary>
    public const string Write = "write";

    /// <summary>Plus deletes and the irreversible or money-moving tools.</summary>
    public const string Full = "full";
}

/// <summary>What produced a credit-ledger row (<see cref="CreditUsageEntry.Kind"/>, <see cref="CreditUsageListParams.Kind"/>).</summary>
public static class CreditUsageKinds
{
    /// <summary>An operation consumed (or owes) credits.</summary>
    public const string Charge = "charge";

    /// <summary>Credits added at renewal, signup or purchase.</summary>
    public const string Grant = "grant";

    /// <summary>A settlement run paid off unsettled charges.</summary>
    public const string Settlement = "settlement";

    /// <summary>Remaining balance carried into a new billing period.</summary>
    public const string CarryOver = "carryOver";

    /// <summary>A manual balance change made by Posty5 support.</summary>
    public const string AdminAdjustment = "adminAdjustment";

    /// <summary>Credits returned to the payer.</summary>
    public const string Refund = "refund";
}

/// <summary>Which way credits moved on a ledger row (<see cref="CreditUsageEntry.Direction"/>).</summary>
public static class CreditDirections
{
    /// <summary>Credits were added to the balance.</summary>
    public const string In = "in";

    /// <summary>Credits left the balance.</summary>
    public const string Out = "out";
}
