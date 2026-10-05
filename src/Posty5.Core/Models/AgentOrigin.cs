namespace Posty5.Core.Models;

/// <summary>
/// Which AI assistant created a record, as the API stamps it on records created
/// through an MCP connection (<c>agentOrigin</c> on a detail response).
/// </summary>
/// <remarks>
/// Everything here is reported by the assistant itself and cannot be verified;
/// <see cref="Trust"/> says only whether the call came through Posty5's own
/// hosted MCP server. Absent on records created any other way.
/// </remarks>
public class AgentOrigin
{
    /// <summary>Always <c>mcp</c> today.</summary>
    public string? Channel { get; set; }

    /// <summary>
    /// <c>hosted</c>: the call came through Posty5's hosted MCP server.
    /// <c>declared</c>: an unsigned header, e.g. a self-run MCP package.
    /// </summary>
    public string? Trust { get; set; }

    /// <summary>The MCP client that made the call.</summary>
    public AgentOriginClient? Client { get; set; }

    /// <summary>A display name for the client, e.g. <c>Claude Code</c>.</summary>
    public string? ClientLabel { get; set; }

    /// <summary>The model's provider, e.g. <c>anthropic</c>; absent when it could not be inferred.</summary>
    public string? Provider { get; set; }

    /// <summary>The model id the assistant reported, <c>unknown</c>, or <c>invalid</c>.</summary>
    public string? Model { get; set; }

    /// <summary>Where the model id came from: <c>agent</c>, <c>connection</c> or <c>unknown</c>.</summary>
    public string? ModelSource { get; set; }

    /// <summary>The MCP tool that created the record, e.g. <c>short_link_create</c>.</summary>
    public string? Tool { get; set; }

    /// <summary>The id of the logged MCP call — hosted calls only.</summary>
    public string? CallId { get; set; }

    /// <summary>When the origin was recorded.</summary>
    public DateTime? RecordedAt { get; set; }
}

/// <summary>The MCP client named in an <see cref="AgentOrigin"/>.</summary>
public class AgentOriginClient
{
    /// <summary>The client's name, normalised.</summary>
    public string? Name { get; set; }

    /// <summary>The client's version, when it sent one.</summary>
    public string? Version { get; set; }
}
