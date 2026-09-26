using System.Text.Json.Serialization;

namespace Posty5.Store.Models;

// Dropshipping: supplier orders and the parts of a store order.

/// <summary>One line of a supplier order.</summary>
public class StoreSupplierOrderLine
{
    /// <summary>The store order line.</summary>
    public string? LineKey { get; set; }
    /// <summary>The store product.</summary>
    public string? ProductId { get; set; }
    /// <summary>Name.</summary>
    public string? Name { get; set; }
    /// <summary>The supplier's variant id.</summary>
    public string? SupplierVariantId { get; set; }
    /// <summary>Quantity.</summary>
    public int Qty { get; set; }
    /// <summary>Cost per unit when sent.</summary>
    public decimal? UnitCost { get; set; }
    /// <summary>Cost per unit when imported.</summary>
    public decimal? SnapshotCost { get; set; }
}

/// <summary>What the supplier charges.</summary>
public class StoreSupplierOrderCosts
{
    /// <summary>Products.</summary>
    public decimal Products { get; set; }
    /// <summary>Freight.</summary>
    public decimal Freight { get; set; }
    /// <summary>Total.</summary>
    public decimal Total { get; set; }
    /// <summary>Currency.</summary>
    public string? Currency { get; set; }
}

/// <summary>The merchant's payment to the supplier — never the shopper's.</summary>
public class StoreSupplierOrderPayment
{
    /// <summary>See <see cref="SupplierPaymentStatuses"/>.</summary>
    public string? Status { get; set; }
    /// <summary>Amount paid.</summary>
    public decimal? Amount { get; set; }
    /// <summary>Currency.</summary>
    public string? Currency { get; set; }
    /// <summary>When it was paid.</summary>
    public string? PaidAt { get; set; }
    /// <summary>The supplier's payment reference.</summary>
    public string? Reference { get; set; }
    /// <summary>Where to finish payment on the supplier's site, when it cannot be paid through the api.</summary>
    public string? PaymentUrl { get; set; }
}

/// <summary>How the supplier ships it.</summary>
public class StoreSupplierOrderShipping
{
    /// <summary>Shipping line.</summary>
    public string? Method { get; set; }
    /// <summary>Carrier.</summary>
    public string? CarrierName { get; set; }
    /// <summary>Tracking number.</summary>
    public string? TrackingNumber { get; set; }
    /// <summary>Tracking link.</summary>
    public string? TrackingUrl { get; set; }
}

/// <summary>One entry of a supplier order's history.</summary>
public class StoreSupplierOrderEvent
{
    /// <summary>See <see cref="SupplierOrderStatuses"/>.</summary>
    public string? Status { get; set; }
    /// <summary>See <see cref="SupplierReviewReasons"/>.</summary>
    public string? Reason { get; set; }
    /// <summary>Detail.</summary>
    public string? Message { get; set; }
    /// <summary>Who.</summary>
    public string? ByUserId { get; set; }
    /// <summary><c>owner</c>, <c>staff</c>, <c>system</c> or <c>admin</c>.</summary>
    public string? ByRole { get; set; }
    /// <summary>When.</summary>
    public string? At { get; set; }
}

/// <summary>One attempt at sending one order part to its supplier.</summary>
public class StoreSupplierOrder
{
    /// <summary>Supplier order id.</summary>
    [JsonPropertyName("_id")]
    public string? Id { get; set; }
    /// <summary>Store.</summary>
    public string? StoreId { get; set; }
    /// <summary>The store order.</summary>
    public string? OrderId { get; set; }
    /// <summary>The store order's number.</summary>
    public string? OrderNumber { get; set; }
    /// <summary>The order part this belongs to (<c>supplier:{integrationId}</c>).</summary>
    public string? FulfilmentGroupKey { get; set; }
    /// <summary>The connection.</summary>
    public string? IntegrationId { get; set; }
    /// <summary>Catalogue key.</summary>
    public string? SupplierKey { get; set; }
    /// <summary>Attempt number.</summary>
    public int Attempt { get; set; }
    /// <summary>See <see cref="SupplierOrderStatuses"/>.</summary>
    public string? Status { get; set; }
    /// <summary>See <see cref="SupplierReviewReasons"/>.</summary>
    public string? ReviewReason { get; set; }
    /// <summary>The supplier's words, redacted.</summary>
    public string? ReviewMessage { get; set; }
    /// <summary>Whether trying again with nothing changed may succeed.</summary>
    public bool Retryable { get; set; }
    /// <summary>The supplier's own id.</summary>
    public string? SupplierOrderId { get; set; }
    /// <summary>Our number, sent to the supplier as its order number.</summary>
    public string? SupplierOrderNumber { get; set; }
    /// <summary>The supplier's own status, verbatim.</summary>
    public string? SupplierStatus { get; set; }
    /// <summary>See <see cref="DropshippingContractModels"/>.</summary>
    public string? ContractModel { get; set; }
    /// <summary>Lines.</summary>
    public List<StoreSupplierOrderLine>? Lines { get; set; }
    /// <summary>What the supplier charges.</summary>
    public StoreSupplierOrderCosts? Costs { get; set; }
    /// <summary>The merchant's payment to the supplier.</summary>
    public StoreSupplierOrderPayment? Payment { get; set; }
    /// <summary>Shipping.</summary>
    public StoreSupplierOrderShipping? Shipping { get; set; }
    /// <summary>The shipment it produced.</summary>
    public string? ShipmentId { get; set; }
    /// <summary>The delivery address — null without <c>orders.customerData.view</c>.</summary>
    public Dictionary<string, object>? Destination { get; set; }
    /// <summary>True when the address was withheld from this caller.</summary>
    public bool DestinationRedacted { get; set; }
    /// <summary>History.</summary>
    public List<StoreSupplierOrderEvent>? Events { get; set; }
    /// <summary>When it was sent.</summary>
    public string? SubmittedAt { get; set; }
    /// <summary>When the supplier accepted it.</summary>
    public string? AcceptedAt { get; set; }
    /// <summary>Created at.</summary>
    public string? CreatedAt { get; set; }
    /// <summary>Updated at.</summary>
    public string? UpdatedAt { get; set; }

    /// <summary>Every field the API returned, including those not typed above.</summary>
    [JsonExtensionData]
    public Dictionary<string, object>? Extra { get; set; }
}

/// <summary>
/// Filters for the supplier-order list. Paging travels separately, as
/// <c>Posty5.Core.Models.PaginationParams</c> (cursor + page size).
/// </summary>
public class SupplierOrderSearchParams
{
    /// <summary>See <see cref="SupplierOrderStatuses"/>.</summary>
    public string? Status { get; set; }
    /// <summary>Only paused supplier orders.</summary>
    public bool? NeedsReview { get; set; }
    /// <summary>One connection.</summary>
    public string? IntegrationId { get; set; }
    /// <summary>One store order.</summary>
    public string? OrderId { get; set; }
    /// <summary>See <see cref="DropshippingContractModels"/>.</summary>
    public string? ContractModel { get; set; }
}

internal class SubmitGroupRequest
{
    public bool? PayNow { get; set; }
}

internal class RetrySupplierOrderRequest
{
    public bool? AcceptCost { get; set; }
}

/// <summary>
/// What sending, retrying, paying or cancelling concluded. A pause is thrown as
/// a <c>Posty5ValidationException</c> instead.
/// </summary>
public class SupplierOrderActionResult
{
    /// <summary>See <see cref="SupplierOrderStatuses"/>.</summary>
    public string? Status { get; set; }
    /// <summary>Whether trying again with nothing changed may succeed.</summary>
    public bool Retryable { get; set; }
    /// <summary>See <see cref="SupplierReviewReasons"/>.</summary>
    public string? Reason { get; set; }
    /// <summary>Detail.</summary>
    public string? Message { get; set; }
    /// <summary>The supplier order row.</summary>
    public string? SupplierOrderRowId { get; set; }
    /// <summary>The supplier's own id.</summary>
    public string? SupplierOrderId { get; set; }

    /// <summary>Every field the API returned, including those not typed above.</summary>
    [JsonExtensionData]
    public Dictionary<string, object>? Extra { get; set; }
}

/// <summary>The part taken over.</summary>
public class FulfilGroupManuallyResult
{
    /// <summary>The store order.</summary>
    public string? OrderId { get; set; }
}

// ─── On a store order ──────────────────────────────────────────────────────

/// <summary>A part's shipment.</summary>
public class OrderFulfilmentGroupShipment
{
    /// <summary>Carrier.</summary>
    public string? CarrierName { get; set; }
    /// <summary>Tracking number.</summary>
    public string? TrackingNumber { get; set; }
    /// <summary>Tracking link.</summary>
    public string? TrackingUrl { get; set; }
    /// <summary>Shipment status.</summary>
    public string? Status { get; set; }

    /// <summary>Every field the API returned, including those not typed above.</summary>
    [JsonExtensionData]
    public Dictionary<string, object>? Extra { get; set; }
}

/// <summary>One part of an order: the store's own items, or one supplier connection's.</summary>
public class OrderFulfilmentGroup
{
    /// <summary><c>merchant</c> or <c>supplier:{integrationId}</c>.</summary>
    public string? Key { get; set; }
    /// <summary>See <see cref="FulfilmentKinds"/>.</summary>
    public string? Kind { get; set; }
    /// <summary>Human wording, e.g. "Shipped by the store".</summary>
    public string? Label { get; set; }
    /// <summary>The connection.</summary>
    public string? IntegrationId { get; set; }
    /// <summary>Catalogue key.</summary>
    public string? SupplierKey { get; set; }
    /// <summary>Supplier name.</summary>
    public string? SupplierName { get; set; }
    /// <summary>The order lines in this part.</summary>
    public List<string>? LineKeys { get; set; }
    /// <summary>See <see cref="FulfilmentGroupStatuses"/>.</summary>
    public string? Status { get; set; }
    /// <summary>When the status last changed.</summary>
    public string? StatusAt { get; set; }
    /// <summary>A supplier order of this part is paused.</summary>
    public bool NeedsAttention { get; set; }
    /// <summary>The current supplier order.</summary>
    public string? SupplierOrderId { get; set; }
    /// <summary>Shipment.</summary>
    public OrderFulfilmentGroupShipment? Shipment { get; set; }
    /// <summary>Delivery estimate.</summary>
    public DeliveryEstimate? DeliveryEstimate { get; set; }
    /// <summary>Warnings.</summary>
    public List<string>? Warnings { get; set; }
    /// <summary>The store took this part over.</summary>
    public bool? FulfilledManually { get; set; }
}

/// <summary>Progress across an order's parts, on each row of the order list.</summary>
public class OrderFulfilmentSummary
{
    /// <summary>Number of parts.</summary>
    public int Parts { get; set; }
    /// <summary>Parts shipped (delivered included).</summary>
    public int Shipped { get; set; }
    /// <summary>Parts delivered.</summary>
    public int Delivered { get; set; }
    /// <summary>Some part needs attention.</summary>
    public bool NeedsAttention { get; set; }
}
