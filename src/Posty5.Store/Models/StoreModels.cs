using System.Text.Json.Serialization;

namespace Posty5.Store.Models;

// ─── Products ───────────────────────────────────────────────────────────────

/// <summary>A product image (URL-based).</summary>
public class ProductImageInput
{
    public string Url { get; set; } = string.Empty;
    public string? Source { get; set; } = "url";
}

/// <summary>A product option / variant (e.g. Size: S, M, L).</summary>
public class ProductOptionInput
{
    public string Name { get; set; } = string.Empty;
    public List<string> Values { get; set; } = new();
}

/// <summary>One product payload for bulk create (mirrors the single-create schema).</summary>
public class BulkProductInput
{
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string? Slug { get; set; }
    public string? Description { get; set; }
    public decimal? CompareAtPrice { get; set; }
    public int? Stock { get; set; }
    public string? Sku { get; set; }
    public string? Status { get; set; }
    public int? SortOrder { get; set; }
    public List<ProductImageInput>? Images { get; set; }
    public List<ProductOptionInput>? Options { get; set; }
}

/// <summary>Per-row result of a bulk create.</summary>
public class BulkRowResult
{
    public int Row { get; set; }
    public string? Name { get; set; }
    public string Status { get; set; } = string.Empty;
    public List<string> Errors { get; set; } = new();
}

/// <summary>Report returned by bulk product create.</summary>
public class BulkProductsReport
{
    public int TotalRows { get; set; }
    public int Imported { get; set; }
    public int Failed { get; set; }
    public int CreditsCharged { get; set; }
    public List<BulkRowResult> Rows { get; set; } = new();
}

/// <summary>Wrapper for the bulk create request body.</summary>
public class BulkProductsRequest
{
    public List<BulkProductInput> Products { get; set; } = new();
}

// ─── Orders ─────────────────────────────────────────────────────────────────

/// <summary>Filters for searching a store's orders.</summary>
public class OrderSearchParams
{
    public string? Status { get; set; }
    public string? OrderSource { get; set; }
    public string? OrderNumber { get; set; }
    public string? FromDate { get; set; }
    public string? ToDate { get; set; }
}

/// <summary>Order summary row (search results).</summary>
public class StoreOrderSummary
{
    [JsonPropertyName("_id")]
    public string? Id { get; set; }
    public string? OrderNumber { get; set; }
    public string? CustomerName { get; set; }
    public string? CustomerPhone { get; set; }
    public decimal Total { get; set; }
    public string? Currency { get; set; }
    public string? Status { get; set; }
    public string? OrderSource { get; set; }
    public string? CreatedFrom { get; set; }
    public string? CreatedAt { get; set; }
}

/// <summary>One line of a manual order.</summary>
public class OrderItemInput
{
    public string ProductId { get; set; } = string.Empty;
    public int Qty { get; set; }
    public Dictionary<string, string>? Options { get; set; }
}

/// <summary>Customer details for a manual order.</summary>
public class OrderCustomerInput
{
    public string Name { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Notes { get; set; }
}

/// <summary>Request body to create an order manually.</summary>
public class CreateOrderInput
{
    public List<OrderItemInput> Items { get; set; } = new();
    public OrderCustomerInput Customer { get; set; } = new();
    public string? OrderSource { get; set; }
    public string? OrderSourceNote { get; set; }
    public string CreatedFrom { get; set; } = "dotnet";
}

/// <summary>Request body to change an order's status.</summary>
public class ChangeStatusRequest
{
    public string Status { get; set; } = string.Empty;
    public string? Note { get; set; }
}

/// <summary>Store order (returned by create / status change).</summary>
public class StoreOrder
{
    [JsonPropertyName("_id")]
    public string? Id { get; set; }
    public string? OrderNumber { get; set; }
    public string? PublicTrackingId { get; set; }
    public string? Status { get; set; }
    public string? OrderSource { get; set; }
    public string? CreatedFrom { get; set; }
}
