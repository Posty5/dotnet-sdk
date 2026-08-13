using Posty5.Core.Http;
using Posty5.Core.Models;

namespace Posty5.Store.Clients;

/// <summary>
/// Shared plumbing for the store sub-clients.
/// </summary>
/// <remarks>
/// The only real work here is query building: the API reads comma-separated id
/// lists (<c>tagIds=a,b</c>) and lower-case string booleans, so a list or a
/// <c>bool</c> passed straight through would be serialized in a shape the
/// server ignores. Normalising in one place keeps the filter classes plain.
/// </remarks>
public abstract class StoreClientBase
{
    /// <summary>The HTTP client every call goes through.</summary>
    protected readonly Posty5HttpClient Http;

    /// <summary>Creates a sub-client over an existing HTTP client.</summary>
    /// <param name="httpClient">HTTP client instance from Posty5.Core.</param>
    protected StoreClientBase(Posty5HttpClient httpClient)
    {
        Http = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    }

    /// <summary>Start an empty query dictionary.</summary>
    protected static Dictionary<string, object?> Query() => new();

    /// <summary>Add a value unless it is null or empty.</summary>
    protected static void Add(Dictionary<string, object?> query, string key, string? value)
    {
        if (!string.IsNullOrEmpty(value)) query[key] = value;
    }

    /// <summary>Add a number unless it is null.</summary>
    protected static void Add(Dictionary<string, object?> query, string key, int? value)
    {
        if (value.HasValue) query[key] = value.Value;
    }

    /// <summary>Add a boolean as the lower-case string the API expects.</summary>
    protected static void Add(Dictionary<string, object?> query, string key, bool? value)
    {
        if (value.HasValue) query[key] = value.Value ? "true" : "false";
    }

    /// <summary>Add a list as a comma-separated value, unless it is empty.</summary>
    protected static void Add(Dictionary<string, object?> query, string key, List<string>? values)
    {
        if (values is { Count: > 0 }) query[key] = string.Join(",", values);
    }

    /// <summary>Append cursor pagination to a query.</summary>
    protected static void AddPagination(Dictionary<string, object?> query, PaginationParams? pagination)
    {
        if (pagination == null) return;
        Add(query, "cursor", pagination.Cursor);
        query["pageSize"] = pagination.PageSize;
    }
}
