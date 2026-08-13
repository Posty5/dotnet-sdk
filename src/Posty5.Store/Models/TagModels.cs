using System.Text.Json.Serialization;

namespace Posty5.Store.Models;

/// <summary>A catalogue tag.</summary>
public class CreateTagInput
{
    /// <summary>Display name.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>URL slug. Generated from the name when omitted.</summary>
    public string? Slug { get; set; }

    /// <summary>Free-text description.</summary>
    public string? Description { get; set; }

    /// <summary><c>active</c> or <c>hidden</c>.</summary>
    public string? Status { get; set; }

    /// <summary>
    /// Days before an assignment is removed automatically. <c>null</c> = never —
    /// a zero-day period would be indistinguishable from "no expiry".
    /// </summary>
    public int? AutoRemoveAfterDays { get; set; }
}

/// <summary>Any subset of a tag's fields.</summary>
public class UpdateTagInput : CreateTagInput
{
}

/// <summary>Filters for searching a store's tags.</summary>
public class TagSearchParams
{
    /// <summary>Partial, case-insensitive match on the tag name.</summary>
    public string? Name { get; set; }

    /// <summary>Partial match on the slug.</summary>
    public string? Slug { get; set; }

    /// <summary><c>active</c> or <c>hidden</c>.</summary>
    public string? Status { get; set; }

    /// <summary>Presence filter on the automatic-removal period, not a value one.</summary>
    public bool? HasAutoRemoval { get; set; }

    /// <summary>Created-at range start. Only applied together with <see cref="ToDate"/>.</summary>
    public string? FromDate { get; set; }

    /// <summary>Created-at range end.</summary>
    public string? ToDate { get; set; }
}

/// <summary>A catalogue tag as returned by the API.</summary>
public class StoreTag
{
    /// <summary>Tag id.</summary>
    [JsonPropertyName("_id")]
    public string? Id { get; set; }

    /// <summary>Display name.</summary>
    public string? Name { get; set; }

    /// <summary>URL slug.</summary>
    public string? Slug { get; set; }

    /// <summary>Free-text description.</summary>
    public string? Description { get; set; }

    /// <summary><c>active</c> or <c>hidden</c>.</summary>
    public string? Status { get; set; }

    /// <summary><c>null</c> = assignments never expire.</summary>
    public int? AutoRemoveAfterDays { get; set; }

    /// <summary>How many products carry this tag.</summary>
    public int? ProductsCount { get; set; }

    /// <summary>Creation timestamp.</summary>
    public string? CreatedAt { get; set; }
}

/// <summary>Wrapper for the assign-products request body.</summary>
public class AssignProductsRequest
{
    /// <summary>Up to 200 product ids.</summary>
    public List<string> ProductIds { get; set; } = new();
}

/// <summary>Wrapper for the set-product-tags request body.</summary>
public class SetProductTagsRequest
{
    /// <summary>Tag ids. An empty list unassigns everything.</summary>
    public List<string> TagIds { get; set; } = new();
}
