using System.Text.Json.Serialization;

namespace Posty5.Core.Models;

/// <summary>
/// The outcome of a versioned write whose route answers without a full
/// document: the document's id and, for an update, its new version.
/// </summary>
public class VersionedWriteResult
{
    /// <summary>The written document's id.</summary>
    [JsonPropertyName("_id")]
    public string? Id { get; set; }

    /// <summary>
    /// The document's new version, to send with the next write. Null after a
    /// delete: no successor document exists.
    /// </summary>
    public long? Version { get; set; }

    /// <summary>The API's message, when it sent one.</summary>
    public string? Message { get; set; }
}

/// <summary>An item a versioned bulk write did not apply.</summary>
public class VersionedBulkSkippedItem
{
    /// <summary>The skipped document's id.</summary>
    [JsonPropertyName("_id")]
    public string Id { get; set; } = string.Empty;

    /// <summary>Why it was skipped: <c>VERSION_CONFLICT</c> or <c>NOT_FOUND</c>.</summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>The stored version, on a conflict.</summary>
    public long? CurrentVersion { get; set; }
}

/// <summary>The outcome of a versioned bulk write.</summary>
public class VersionedBulkResult
{
    /// <summary>The ids the write applied to.</summary>
    public List<string> Applied { get; set; } = new();

    /// <summary>The ids it skipped, with the reason.</summary>
    public List<VersionedBulkSkippedItem> Skipped { get; set; } = new();

    /// <summary>The new version of every applied document, by id.</summary>
    public IDictionary<string, long> Versions { get; set; } = new Dictionary<string, long>();
}
