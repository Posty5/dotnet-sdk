using System.Text.Json.Serialization;

namespace Posty5.Store.Models;

/// <summary>
/// A store the API key's owner can manage — one they own or are staff on — as
/// the store lookup returns it.
/// </summary>
public class StoreLookupItem
{
    /// <summary>The store id: the <c>storeId</c> every other store method takes.</summary>
    [JsonPropertyName("_id")]
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// The store's slug and name, as one label: <c>&lt;slug&gt; - &lt;name&gt;</c>.
    /// </summary>
    public string Name { get; set; } = string.Empty;
}
