using System.Text.Json.Serialization;

namespace Posty5.Store.Models;

/// <summary>
/// Filters for a store's orders. Search, statistics, export and print all read
/// the same set, so an export can never disagree with the list on screen.
/// </summary>
public class OrderSearchParams
{
    /// <summary><c>pending</c>, <c>confirmed</c>, <c>processing</c>, <c>shipped</c>, <c>delivered</c>, <c>cancelled</c> or <c>refused</c>.</summary>
    public string? Status { get; set; }

    /// <summary>Business channel the order came from.</summary>
    public string? OrderSource { get; set; }

    /// <summary>Technical origin — <c>storefront</c>, <c>cpanel</c>, <c>dotnet</c>…</summary>
    public string? CreatedFrom { get; set; }

    /// <summary>Partial match on the order number.</summary>
    public string? OrderNumber { get; set; }

    /// <summary>Exact public tracking id.</summary>
    public string? PublicTrackingId { get; set; }

    /// <summary>One term matched across customer name, phone and email.</summary>
    public string? Customer { get; set; }

    /// <summary>Matches the item name snapshotted on the order, so it survives a rename.</summary>
    public string? ProductName { get; set; }

    /// <summary>Orders containing this product.</summary>
    public string? ProductId { get; set; }

    /// <summary>Orders containing any product carrying one of these tags.</summary>
    public List<string>? TagIds { get; set; }

    /// <summary>Created-at range start. Only applied together with <see cref="ToDate"/>.</summary>
    public string? FromDate { get; set; }

    /// <summary>Created-at range end.</summary>
    public string? ToDate { get; set; }
}

/// <summary>Order search filters plus the statistics window.</summary>
public class OrderStatisticsParams : OrderSearchParams
{
    /// <summary>Window length in days (1–365, default 30).</summary>
    public int? Days { get; set; }
}

/// <summary>Order summary row (search results).</summary>
public class StoreOrderSummary
{
    /// <summary>Order id.</summary>
    [JsonPropertyName("_id")]
    public string? Id { get; set; }

    /// <summary>Human-readable order number.</summary>
    public string? OrderNumber { get; set; }

    /// <summary>Customer's name.</summary>
    public string? CustomerName { get; set; }

    /// <summary>Customer's phone.</summary>
    public string? CustomerPhone { get; set; }

    /// <summary>Order total.</summary>
    public decimal Total { get; set; }

    /// <summary>Currency of <see cref="Total"/>.</summary>
    public string? Currency { get; set; }

    /// <summary>Current status.</summary>
    public string? Status { get; set; }

    /// <summary>Business channel.</summary>
    public string? OrderSource { get; set; }

    /// <summary>Technical origin.</summary>
    public string? CreatedFrom { get; set; }

    /// <summary>Creation timestamp.</summary>
    public string? CreatedAt { get; set; }
}

/// <summary>One line of a manual order.</summary>
public class OrderItemInput
{
    /// <summary>Product id.</summary>
    public string ProductId { get; set; } = string.Empty;

    /// <summary>Quantity (1–999).</summary>
    public int Qty { get; set; }

    /// <summary>Chosen variant values, keyed by group name.</summary>
    public Dictionary<string, string>? Options { get; set; }
}

/// <summary>Customer details for a manual order.</summary>
public class OrderCustomerInput
{
    /// <summary>Customer's name.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Customer's phone.</summary>
    public string Phone { get; set; } = string.Empty;

    /// <summary>Delivery address.</summary>
    public string Address { get; set; } = string.Empty;

    /// <summary>Customer's email.</summary>
    public string? Email { get; set; }

    /// <summary>Free-text notes from the customer.</summary>
    public string? Notes { get; set; }

    /// <summary>
    /// Shipping destination. Whether it is required depends on the store's open
    /// countries — the fee itself is always resolved server-side.
    /// </summary>
    public string? CountryIso { get; set; }

    /// <summary>Catalogue state code, upper-case.</summary>
    public string? GovernorateCode { get; set; }

    /// <summary>
    /// Normalized city key. City names repeat across governorates, so it is the
    /// pair that identifies a destination — send the governorate too.
    /// </summary>
    public string? CityKey { get; set; }
}

/// <summary>Request body to create an order manually.</summary>
public class CreateOrderInput
{
    /// <summary>The order's lines.</summary>
    public List<OrderItemInput> Items { get; set; } = new();

    /// <summary>Who the order is for.</summary>
    public OrderCustomerInput Customer { get; set; } = new();

    /// <summary>Business channel. <c>storefront</c> is reserved for real checkouts.</summary>
    public string? OrderSource { get; set; }

    /// <summary>Free-text detail about the channel.</summary>
    public string? OrderSourceNote { get; set; }

    /// <summary>Technical origin tag — set by the SDK.</summary>
    public string CreatedFrom { get; set; } = "dotnet";
}

/// <summary>Request body to change an order's status.</summary>
public class ChangeStatusRequest
{
    /// <summary>The new status.</summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>Customer-facing note, attached to the status event and the email.</summary>
    public string? Note { get; set; }
}

/// <summary>Request body to add a staff-only note.</summary>
public class InternalNoteRequest
{
    /// <summary>The note.</summary>
    public string Note { get; set; } = string.Empty;
}

/// <summary>Order totals.</summary>
public class StoreOrderTotals
{
    /// <summary>Items total before shipping.</summary>
    public decimal Subtotal { get; set; }

    /// <summary>Resolved shipping fee.</summary>
    public decimal Shipping { get; set; }

    /// <summary>What the customer pays.</summary>
    public decimal Total { get; set; }

    /// <summary>Currency of the amounts above.</summary>
    public string? Currency { get; set; }
}

/// <summary>
/// A store order. Loosely typed beyond the fields below: the API returns the
/// whole document, including snapshotted item rows and the status history.
/// </summary>
public class StoreOrder
{
    /// <summary>Order id.</summary>
    [JsonPropertyName("_id")]
    public string? Id { get; set; }

    /// <summary>Human-readable order number.</summary>
    public string? OrderNumber { get; set; }

    /// <summary>Opaque id the customer tracks the order with.</summary>
    public string? PublicTrackingId { get; set; }

    /// <summary>Current status.</summary>
    public string? Status { get; set; }

    /// <summary>Business channel.</summary>
    public string? OrderSource { get; set; }

    /// <summary>Technical origin.</summary>
    public string? CreatedFrom { get; set; }

    /// <summary>Money on the order.</summary>
    public StoreOrderTotals? Totals { get; set; }

    /// <summary>Every field the API returned, including those not typed above.</summary>
    [JsonExtensionData]
    public Dictionary<string, object>? Extra { get; set; }
}

/// <summary>Delivered revenue for a window.</summary>
public class OrderRevenue
{
    /// <summary>Revenue from delivered orders.</summary>
    public decimal Delivered { get; set; }

    /// <summary>Currency of <see cref="Delivered"/>.</summary>
    public string? Currency { get; set; }
}

/// <summary>One day of the order series.</summary>
public class OrderPerDay
{
    /// <summary>The day, as <c>YYYY-MM-DD</c>.</summary>
    public string? Date { get; set; }

    /// <summary>Orders created that day.</summary>
    public int Count { get; set; }
}

/// <summary>Order statistics over the requested window and filters.</summary>
public class OrderStatistics
{
    /// <summary>Matching orders.</summary>
    public int Total { get; set; }

    /// <summary>Counts keyed by status.</summary>
    public Dictionary<string, int> ByStatus { get; set; } = new();

    /// <summary>Delivered revenue.</summary>
    public OrderRevenue Revenue { get; set; } = new();

    /// <summary>Per-day series.</summary>
    public List<OrderPerDay> PerDay { get; set; } = new();
}
