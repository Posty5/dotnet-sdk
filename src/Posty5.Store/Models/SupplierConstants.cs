namespace Posty5.Store.Models;

// Dropshipping vocabularies, as the api spells them (store-supplier/type.ts).
//
// Strings with named constants, never enums: a value added on the server — a
// new review reason, a new supplier status — must not make deserialization
// throw in an application that has not upgraded yet.

/// <summary>Who ships an order part or a product.</summary>
public static class FulfilmentKinds
{
    /// <summary>The store ships it itself.</summary>
    public const string Merchant = "merchant";
    /// <summary>A supplier ships it on the store's behalf.</summary>
    public const string ThirdParty = "thirdParty";
}

/// <summary>A supplier connection's mode.</summary>
public static class SupplierModes
{
    /// <summary>Orders go to the supplier's sandbox where it has one; otherwise none are placed.</summary>
    public const string Test = "test";
    /// <summary>Real orders.</summary>
    public const string Live = "live";
}

/// <summary>How much a supplier connection does on its own. Every connection starts on <see cref="Manual"/>.</summary>
public static class SupplierAutomationModes
{
    /// <summary>Nothing is sent until someone submits the part.</summary>
    public const string Manual = "manual";
    /// <summary>The supplier order is created when the store order is confirmed; payment is left to the merchant.</summary>
    public const string Submit = "submit";
    /// <summary>Created and paid from the merchant's supplier balance. Only for suppliers that can be paid that way.</summary>
    public const string SubmitAndPay = "submitAndPay";
}

/// <summary>When a dropshipped sale concludes.</summary>
public static class DropshippingContractModels
{
    /// <summary>At checkout.</summary>
    public const string Standard = "standard";
    /// <summary>The shopper's payment is authorised at checkout and captured when the supplier accepts.</summary>
    public const string PromiseToSell = "promiseToSell";
}

/// <summary>An order part's status. The order moves at the pace of its slowest part.</summary>
public static class FulfilmentGroupStatuses
{
    /// <summary>Not started.</summary>
    public const string Pending = "pending";
    /// <summary>Being prepared.</summary>
    public const string Processing = "processing";
    /// <summary>With a carrier.</summary>
    public const string Shipped = "shipped";
    /// <summary>Delivered. Final.</summary>
    public const string Delivered = "delivered";
    /// <summary>Cancelled. Final.</summary>
    public const string Cancelled = "cancelled";
}

/// <summary>A supplier order's status.</summary>
public static class SupplierOrderStatuses
{
    /// <summary>Waiting to be sent.</summary>
    public const string Queued = "queued";
    /// <summary>Paused with a reason; needs a person or a changed input.</summary>
    public const string NeedsReview = "needsReview";
    /// <summary>Created at the supplier, not yet accepted and paid.</summary>
    public const string Submitted = "submitted";
    /// <summary>Accepted and paid at the supplier.</summary>
    public const string Confirmed = "confirmed";
    /// <summary>Being prepared by the supplier.</summary>
    public const string Processing = "processing";
    /// <summary>Shipped by the supplier.</summary>
    public const string Shipped = "shipped";
    /// <summary>Delivered. Final.</summary>
    public const string Delivered = "delivered";
    /// <summary>Cancelled. Final.</summary>
    public const string Cancelled = "cancelled";
    /// <summary>A temporary error; retried automatically.</summary>
    public const string Failed = "failed";
}

/// <summary>Why a supplier order is paused.</summary>
public static class SupplierReviewReasons
{
    /// <summary>The balance at the supplier cannot cover the order.</summary>
    public const string InsufficientBalance = "insufficientBalance";
    /// <summary>The supplier no longer offers a variant.</summary>
    public const string VariantUnavailable = "variantUnavailable";
    /// <summary>The supplier does not deliver there, or the store excluded the country.</summary>
    public const string DestinationUnsupported = "destinationUnsupported";
    /// <summary>The cost is above a limit the store set.</summary>
    public const string CostAboveLimit = "costAboveLimit";
    /// <summary>The supplier's price moved since import.</summary>
    public const string CostChanged = "costChanged";
    /// <summary>The shopper has not paid online.</summary>
    public const string CustomerPaymentPending = "customerPaymentPending";
    /// <summary>The payment is authorised but not settled.</summary>
    public const string CustomerPaymentNotSettled = "customerPaymentNotSettled";
    /// <summary>The supplier did not answer, or the connection is paused.</summary>
    public const string ConnectionUnhealthy = "connectionUnhealthy";
    /// <summary>The supplier rejected the order.</summary>
    public const string SupplierRefused = "supplierRefused";
    /// <summary>The connection is off, removed, or the supplier is switched off by Posty5.</summary>
    public const string ConnectionMissing = "connectionMissing";
    /// <summary>A cancel came after the supplier shipped.</summary>
    public const string SupplierAlreadyShipped = "supplierAlreadyShipped";
    /// <summary>A test connection to a supplier without a sandbox; no order was placed.</summary>
    public const string TestMode = "testMode";
    /// <summary>Promise to sell: the supplier was paid, charging the shopper failed.</summary>
    public const string CustomerCaptureFailed = "customerCaptureFailed";
}

/// <summary>The merchant's payment to the supplier — never the shopper's.</summary>
public static class SupplierPaymentStatuses
{
    /// <summary>Nothing to pay.</summary>
    public const string NotRequired = "notRequired";
    /// <summary>Not paid yet.</summary>
    public const string Unpaid = "unpaid";
    /// <summary>Paid.</summary>
    public const string Paid = "paid";
    /// <summary>Refunded by the supplier.</summary>
    public const string Refunded = "refunded";
}

/// <summary>How a price is worked out from the supplier's cost.</summary>
public static class PriceRuleTypes
{
    /// <summary>A percentage added to the cost.</summary>
    public const string MarkupPercent = "markupPercent";
    /// <summary>A fixed amount added to the cost.</summary>
    public const string MarkupFixed = "markupFixed";
    /// <summary>A target margin (below 100).</summary>
    public const string TargetMargin = "targetMargin";
}

/// <summary>How prices are rounded.</summary>
public static class PriceRoundings
{
    /// <summary>No rounding (the default).</summary>
    public const string None = "none";
    /// <summary>To the nearest whole number.</summary>
    public const string Nearest = "nearest";
    /// <summary>Ending in .99.</summary>
    public const string EndsIn99 = "endsIn99";
    /// <summary>Ending in .95.</summary>
    public const string EndsIn95 = "endsIn95";
}

/// <summary>What the hourly sync may overwrite on a linked product.</summary>
public static class LinkSyncFields
{
    /// <summary>Stock counts (on by default).</summary>
    public const string Stock = "stock";
    /// <summary>The product's cost (on by default).</summary>
    public const string Cost = "cost";
    /// <summary>The price, recalculated from the rule (off by default).</summary>
    public const string Price = "price";
    /// <summary>Images (off by default).</summary>
    public const string Images = "images";
    /// <summary>Description (off by default).</summary>
    public const string Description = "description";
}
