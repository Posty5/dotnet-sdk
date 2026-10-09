using System.Text.Json.Serialization;

namespace Posty5.Account.Models;

// ============================================================================
// WHO AM I — GET /api/api-key/current
// Transcribed from the API's /swagger.json.
// ============================================================================

/// <summary>
/// The calling API key, its owner, the owner's plan and credits, and the key's
/// MCP settings. Carries no email, phone or address.
/// </summary>
public class AccountInfo
{
    /// <summary>The key that made the call.</summary>
    public AccountApiKey ApiKey { get; set; } = new();

    /// <summary>The key's owner.</summary>
    public AccountUser User { get; set; } = new();

    /// <summary>The owner's current plan; null when none is assigned.</summary>
    public AccountPlan? Plan { get; set; }

    /// <summary>The owner's credit balance for the current period.</summary>
    public CreditBalance Credits { get; set; } = new();

    /// <summary>The key's MCP connection, when it has one; otherwise null.</summary>
    public AccountMcpConnection? Mcp { get; set; }
}

/// <summary>An API key, as <see cref="AccountInfo"/> describes it. Never carries the key itself.</summary>
public class AccountApiKey
{
    /// <summary>The key's id.</summary>
    [JsonPropertyName("_id")]
    public string Id { get; set; } = string.Empty;

    /// <summary>The name the owner gave the key.</summary>
    public string? Name { get; set; }

    /// <summary>
    /// Which records the key sees: <see cref="ApiKeyRecordScopes.Key"/> (only
    /// the records it created — the default) or
    /// <see cref="ApiKeyRecordScopes.Account"/> (every record of its owner).
    /// </summary>
    public string RecordScope { get; set; } = ApiKeyRecordScopes.Key;

    /// <summary>When the key was created.</summary>
    public DateTime? CreatedAt { get; set; }

    /// <summary>When the key was last used, to within a few minutes; null if never.</summary>
    public DateTime? LastUsedAt { get; set; }
}

/// <summary>The owner of an API key.</summary>
public class AccountUser
{
    /// <summary>The user's id.</summary>
    [JsonPropertyName("_id")]
    public string Id { get; set; } = string.Empty;

    /// <summary>The user's login name.</summary>
    public string? UserName { get; set; }

    /// <summary>The user's full name.</summary>
    public string? FullName { get; set; }

    /// <summary>Whether the user holds the developer role (needed to create an MCP connection).</summary>
    public bool IsDeveloper { get; set; }
}

/// <summary>A user's current plan.</summary>
public class AccountPlan
{
    /// <summary>
    /// The plan's key: <c>free</c>, <c>basic</c>, <c>pro</c>, <c>business</c> or
    /// <c>enterprise</c>.
    /// </summary>
    public string? Key { get; set; }

    /// <summary>The plan's display name.</summary>
    public string? Name { get; set; }

    /// <summary>When the plan started.</summary>
    public DateTime? StartedAt { get; set; }

    /// <summary>When the plan ends, for a paid plan.</summary>
    public DateTime? EndedAt { get; set; }
}

/// <summary>The MCP connection on an API key.</summary>
public class AccountMcpConnection
{
    /// <summary>
    /// What the connection may do: <see cref="McpAccessLevels.Read"/>,
    /// <see cref="McpAccessLevels.Write"/> or <see cref="McpAccessLevels.Full"/>.
    /// </summary>
    public string? Access { get; set; }

    /// <summary>The toolsets the connection exposes.</summary>
    public List<string> Toolsets { get; set; } = new();
}
