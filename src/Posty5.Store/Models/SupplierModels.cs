using System.Text.Json.Serialization;

namespace Posty5.Store.Models;

// Dropshipping: the supplier catalogue, connections, supplier products,
// imports and product links. Transcribed from the api's DTOs. No model here
// has a credential property: credentials are write-only and never returned.

// ─── Catalogue ─────────────────────────────────────────────────────────────

/// <summary>What a supplier can do, as the catalogue declares it.</summary>
public class StoreSupplierCapabilities
{
    /// <summary>Its catalogue can be searched.</summary>
    public bool BrowseCatalogue { get; set; }
    /// <summary>A pasted product link can be resolved.</summary>
    public bool ProductByUrl { get; set; }
    /// <summary>A product can be fetched by id.</summary>
    public bool ProductById { get; set; }
    /// <summary>Stock can be read.</summary>
    public bool Stock { get; set; }
    /// <summary>Freight can be quoted.</summary>
    public bool FreightQuote { get; set; }
    /// <summary>Orders can be created.</summary>
    public bool CreateOrder { get; set; }
    /// <summary>Orders can be paid from the merchant's supplier balance.</summary>
    public bool PayFromBalance { get; set; }
    /// <summary>Creating the order pays for it: there is no separate pay step.</summary>
    public bool PayOnCreate { get; set; }
    /// <summary>Payment is finished on the supplier's own site through a link.</summary>
    public bool ManualPaymentUrl { get; set; }
    /// <summary>The balance can be read.</summary>
    public bool Balance { get; set; }
    /// <summary>Orders can be withdrawn.</summary>
    public bool CancelOrder { get; set; }
    /// <summary>Tracking can be read.</summary>
    public bool Tracking { get; set; }
    /// <summary>The supplier sends notifications.</summary>
    public bool Webhooks { get; set; }
    /// <summary>Print-on-demand artwork.</summary>
    public bool Artwork { get; set; }
}

/// <summary>A field the supplier asks for when connecting. Read these before <c>ConnectAsync</c>.</summary>
public class StoreSupplierCredentialField
{
    /// <summary>The key to send in <see cref="ConnectSupplierInput.Credentials"/>.</summary>
    public string? Key { get; set; }
    /// <summary>Human label.</summary>
    public string? Label { get; set; }
    /// <summary>Whether the value is secret.</summary>
    public bool Secret { get; set; }
    /// <summary>Whether it may be left out.</summary>
    public bool? Optional { get; set; }
}

/// <summary>A non-secret, supplier-specific setting.</summary>
public class StoreSupplierSettingsField
{
    /// <summary>The key to send in the settings dictionary.</summary>
    public string? Key { get; set; }
    /// <summary>Human label.</summary>
    public string? Label { get; set; }
    /// <summary><c>text</c>, <c>select</c>, <c>country</c>…</summary>
    public string? Type { get; set; }
    /// <summary>Whether it must be set.</summary>
    public bool? Required { get; set; }
    /// <summary>The allowed values of a select.</summary>
    public List<string>? Options { get; set; }
    /// <summary>Help text.</summary>
    public string? Hint { get; set; }
}

/// <summary>One supplier this build can connect, with this store's verdict on it.</summary>
public class StoreSupplierCatalogueEntry
{
    /// <summary>Catalogue key, e.g. <c>cjdropshipping</c>.</summary>
    public string? Key { get; set; }
    /// <summary>Display name.</summary>
    public string? Name { get; set; }
    /// <summary><c>general</c>, <c>printOnDemand</c> or <c>regional</c>.</summary>
    public string? Category { get; set; }
    /// <summary>Warehouse countries.</summary>
    public List<string>? Countries { get; set; }
    /// <summary>Countries it delivers to; empty means not restricted.</summary>
    public List<string>? DestinationCountries { get; set; }
    /// <summary><c>manual</c> (a key) and/or <c>oauth</c> (sign-in).</summary>
    public List<string>? ConnectionMethods { get; set; }
    /// <summary>What to send as credentials.</summary>
    public List<StoreSupplierCredentialField>? CredentialFields { get; set; }
    /// <summary>What may be sent as settings.</summary>
    public List<StoreSupplierSettingsField>? SettingsFields { get; set; }
    /// <summary>What it can do.</summary>
    public StoreSupplierCapabilities? Capabilities { get; set; }
    /// <summary>A <c>test</c> connection can place orders that are never charged or shipped.</summary>
    public bool Sandbox { get; set; }
    /// <summary>False until a real order has moved through it end to end.</summary>
    public bool Verified { get; set; }
    /// <summary>Logo key.</summary>
    public string? Logo { get; set; }
    /// <summary>The supplier's page in the Posty5 guide.</summary>
    public string? DocsUrl { get; set; }
    /// <summary>False when Posty5 switched it off or this build lacks it.</summary>
    public bool Available { get; set; }
    /// <summary>Why it is unavailable.</summary>
    public string? UnavailableReason { get; set; }
    /// <summary>A hint, never a filter: whether it delivers to any country the store ships to.</summary>
    public bool ServesStoreCountries { get; set; }
}

/// <summary><c>GET /api/store-suppliers/{storeId}/catalogue</c>.</summary>
public class StoreSupplierCatalogue
{
    /// <summary>The suppliers.</summary>
    public List<StoreSupplierCatalogueEntry>? Items { get; set; }
    /// <summary>False when the server cannot encrypt credentials yet — connecting would be refused.</summary>
    public bool CredentialsStorageReady { get; set; }
}

// ─── Connections ───────────────────────────────────────────────────────────

/// <summary>What a connection may do on its own.</summary>
public class StoreSupplierAutomation
{
    /// <summary>See <see cref="SupplierAutomationModes"/>.</summary>
    public string? Mode { get; set; }
    /// <summary>Send cash-on-delivery and manual orders too. Off by default.</summary>
    public bool AllowUnpaidOrders { get; set; }
    /// <summary>The most one order may cost at the supplier.</summary>
    public decimal? MaxCostPerOrder { get; set; }
    /// <summary>The most the supplier cost may be, as a percentage of what the shopper paid.</summary>
    public decimal? MaxCostRatio { get; set; }
    /// <summary>ISO-2 countries orders may be sent to; empty means any.</summary>
    public List<string>? AllowedCountries { get; set; }
}

/// <summary>The result of the last connection check.</summary>
public class StoreSupplierHealth
{
    /// <summary>When it was checked.</summary>
    public string? CheckedAt { get; set; }
    /// <summary>Whether it passed.</summary>
    public bool Ok { get; set; }
    /// <summary>The supplier's words, redacted.</summary>
    public string? Message { get; set; }
    /// <summary>The last time a check passed.</summary>
    public string? LastSuccessAt { get; set; }
}

/// <summary>The supplier account a connection signs in as.</summary>
public class StoreSupplierAccount
{
    /// <summary>Account name at the supplier.</summary>
    public string? Name { get; set; }
    /// <summary>The account's currency.</summary>
    public string? Currency { get; set; }
}

/// <summary>One entry of a connection's audit trail.</summary>
public class StoreSupplierAuditEntry
{
    /// <summary><c>connected</c>, <c>credentialsReplaced</c>, <c>tested</c>…</summary>
    public string? Action { get; set; }
    /// <summary>Who did it.</summary>
    public string? ByUserId { get; set; }
    /// <summary>When.</summary>
    public string? At { get; set; }
    /// <summary>Detail.</summary>
    public string? Note { get; set; }
}

/// <summary>A supplier connection. Credentials are never returned — only <see cref="HasCredentials"/>.</summary>
public class StoreSupplierIntegration
{
    /// <summary>Connection id.</summary>
    [JsonPropertyName("_id")]
    public string? Id { get; set; }
    /// <summary>Catalogue key.</summary>
    public string? SupplierKey { get; set; }
    /// <summary><c>manual</c> (a key) or <c>oauth</c> (sign-in).</summary>
    public string? ConnectionMethod { get; set; }
    /// <summary>See <see cref="SupplierModes"/>.</summary>
    public string? Mode { get; set; }
    /// <summary>Whether the connection is switched on.</summary>
    public bool Enabled { get; set; }
    /// <summary>Supplier-specific settings.</summary>
    public Dictionary<string, object>? Settings { get; set; }
    /// <summary>What it may do on its own.</summary>
    public StoreSupplierAutomation? Automation { get; set; }
    /// <summary>The supplier account.</summary>
    public StoreSupplierAccount? Account { get; set; }
    /// <summary>The last check.</summary>
    public StoreSupplierHealth? Health { get; set; }
    /// <summary>Whether credentials are stored.</summary>
    public bool HasCredentials { get; set; }
    /// <summary>Paused after repeated errors, until <see cref="CircuitOpenUntil"/>.</summary>
    public bool IsCircuitOpen { get; set; }
    /// <summary>When the pause ends.</summary>
    public string? CircuitOpenUntil { get; set; }
    /// <summary>When the supplier last sent a notification.</summary>
    public string? LastWebhookAt { get; set; }
    /// <summary>Audit trail.</summary>
    public List<StoreSupplierAuditEntry>? Audit { get; set; }
    /// <summary>Where the supplier's notifications are sent. Empty when no public origin is configured.</summary>
    public string? WebhookUrl { get; set; }
    /// <summary>Created at.</summary>
    public string? CreatedAt { get; set; }
    /// <summary>Updated at.</summary>
    public string? UpdatedAt { get; set; }
}

/// <summary>
/// Connecting a supplier. The <see cref="Credentials"/> keys come from the
/// catalogue entry's <see cref="StoreSupplierCatalogueEntry.CredentialFields"/>
/// (for example <c>apiKey</c>) — they differ per supplier and are sent exactly as given.
/// </summary>
public class ConnectSupplierInput
{
    /// <summary>Catalogue key.</summary>
    public string SupplierKey { get; set; } = string.Empty;
    /// <summary>Defaults to <c>test</c>.</summary>
    public string? Mode { get; set; }
    /// <summary>The supplier's credential fields.</summary>
    public Dictionary<string, string> Credentials { get; set; } = new();
    /// <summary>Supplier-specific settings.</summary>
    public Dictionary<string, object>? Settings { get; set; }
}

/// <summary>Replacing a connection's credentials.</summary>
public class ReplaceSupplierCredentialsInput
{
    /// <summary>The new credential fields.</summary>
    public Dictionary<string, string> Credentials { get; set; } = new();
    /// <summary>Optionally switch the mode at the same time.</summary>
    public string? Mode { get; set; }
}

/// <summary>Changing a connection's settings.</summary>
public class UpdateSupplierSettingsInput
{
    /// <summary>Supplier-specific settings: short strings, numbers or booleans.</summary>
    public Dictionary<string, object> Settings { get; set; } = new();
    /// <summary>Optionally switch the mode at the same time.</summary>
    public string? Mode { get; set; }
}

/// <summary>Changing what a connection may do on its own. Send at least one field.</summary>
public class SupplierAutomationInput
{
    /// <summary>See <see cref="SupplierAutomationModes"/>.</summary>
    public string? Mode { get; set; }
    /// <summary>Send cash-on-delivery and manual orders too.</summary>
    public bool? AllowUnpaidOrders { get; set; }
    /// <summary>The most one order may cost at the supplier.</summary>
    public decimal? MaxCostPerOrder { get; set; }
    /// <summary>The most the cost may be, as a percentage of what the shopper paid.</summary>
    public decimal? MaxCostRatio { get; set; }
    /// <summary>ISO-2 countries orders may be sent to.</summary>
    public List<string>? AllowedCountries { get; set; }
}

internal class SetSupplierEnabledRequest
{
    public bool Enabled { get; set; }
}

/// <summary>What a connection check concluded.</summary>
public class SupplierTestResult
{
    /// <summary>Whether it passed.</summary>
    public bool Ok { get; set; }
    /// <summary>The supplier's words, redacted.</summary>
    public string? Message { get; set; }
    /// <summary>The supplier account.</summary>
    public StoreSupplierAccount? Account { get; set; }
}

/// <summary>The balance at the supplier, in the supplier's currency — never converted.</summary>
public class SupplierBalance
{
    /// <summary>Amount.</summary>
    public decimal Amount { get; set; }
    /// <summary>Currency.</summary>
    public string? Currency { get; set; }
}

/// <summary>What disconnecting would touch.</summary>
public class SupplierDisconnectImpact
{
    /// <summary>Products linked to this connection.</summary>
    public int LinkedProducts { get; set; }
    /// <summary>Supplier orders not yet finished.</summary>
    public int OpenSupplierOrders { get; set; }
}

/// <summary>What a disconnect did.</summary>
public class DisconnectSupplierResult : SupplierDisconnectImpact
{
    /// <summary>Connection id.</summary>
    [JsonPropertyName("_id")]
    public string? Id { get; set; }
}

/// <summary>Starting a sign-in connection (AliExpress).</summary>
public class StartSupplierOAuthInput
{
    /// <summary>Catalogue key.</summary>
    public string SupplierKey { get; set; } = string.Empty;
    /// <summary>Defaults to <c>live</c>.</summary>
    public string? Mode { get; set; }
    /// <summary>Where the merchant lands afterwards; one of the platform's own origins.</summary>
    public string? ReturnUrl { get; set; }
}

/// <summary>Where to send the merchant to sign in.</summary>
public class SupplierOAuthStartResult
{
    /// <summary>The supplier's sign-in page.</summary>
    public string? AuthorizeUrl { get; set; }
    /// <summary>When the handshake expires.</summary>
    public string? ExpiresAt { get; set; }
}

// ─── Supplier products ─────────────────────────────────────────────────────

/// <summary>A delivery estimate in days.</summary>
public class DeliveryEstimate
{
    /// <summary>Fewest days.</summary>
    public int MinDays { get; set; }
    /// <summary>Most days.</summary>
    public int MaxDays { get; set; }
    /// <summary>The country it was quoted for.</summary>
    public string? CountryIso { get; set; }
}

/// <summary>One buyable variant of a supplier product.</summary>
public class SupplierVariant
{
    /// <summary>The supplier's variant id.</summary>
    public string? SupplierVariantId { get; set; }
    /// <summary>SKU.</summary>
    public string? Sku { get; set; }
    /// <summary>Name.</summary>
    public string? Name { get; set; }
    /// <summary>Option name → value, e.g. Color → Red.</summary>
    public Dictionary<string, string>? Options { get; set; }
    /// <summary>What one unit costs at the supplier, in <see cref="Currency"/>.</summary>
    public decimal Cost { get; set; }
    /// <summary>Currency of <see cref="Cost"/>.</summary>
    public string? Currency { get; set; }
    /// <summary>Stock, where known.</summary>
    public int? Stock { get; set; }
    /// <summary>Weight in grams.</summary>
    public int? WeightGrams { get; set; }
    /// <summary>Variant image.</summary>
    public string? ImageUrl { get; set; }
}

/// <summary>One product as the supplier describes it. <see cref="Description"/> is untrusted HTML.</summary>
public class SupplierProduct
{
    /// <summary>The supplier's product id.</summary>
    public string? SupplierProductId { get; set; }
    /// <summary>Name.</summary>
    public string? Name { get; set; }
    /// <summary>Untrusted HTML.</summary>
    public string? Description { get; set; }
    /// <summary>Image URLs.</summary>
    public List<string>? Images { get; set; }
    /// <summary>Category path.</summary>
    public List<string>? CategoryPath { get; set; }
    /// <summary>Variants.</summary>
    public List<SupplierVariant>? Variants { get; set; }
    /// <summary>Warehouse countries with stock.</summary>
    public List<string>? ShipsFromCountries { get; set; }
    /// <summary>Delivery estimate, where the supplier gives one.</summary>
    public DeliveryEstimate? DeliveryEstimate { get; set; }
    /// <summary>The product's page at the supplier.</summary>
    public string? Url { get; set; }
    /// <summary>The store product already imported from it, if any.</summary>
    public string? AlreadyImported { get; set; }
}

/// <summary>A row of the supplier catalogue.</summary>
public class SupplierProductSummary
{
    /// <summary>The supplier's product id.</summary>
    public string? SupplierProductId { get; set; }
    /// <summary>Name.</summary>
    public string? Name { get; set; }
    /// <summary>Main image.</summary>
    public string? Image { get; set; }
    /// <summary>Cheapest variant cost.</summary>
    public decimal? CostMin { get; set; }
    /// <summary>Dearest variant cost.</summary>
    public decimal? CostMax { get; set; }
    /// <summary>Currency of the costs.</summary>
    public string? Currency { get; set; }
    /// <summary>Number of variants.</summary>
    public int? VariantCount { get; set; }
    /// <summary>Category path.</summary>
    public List<string>? CategoryPath { get; set; }
    /// <summary>The product's page at the supplier.</summary>
    public string? Url { get; set; }
    /// <summary>The store product already imported from it, if any.</summary>
    public string? AlreadyImported { get; set; }
}

/// <summary>Filters for browsing a supplier catalogue.</summary>
public class BrowseSupplierProductsParams
{
    /// <summary>Search text.</summary>
    public string? Q { get; set; }
    /// <summary>Supplier category id.</summary>
    public string? CategoryId { get; set; }
}

/// <summary>A page of a supplier catalogue — paged by number, not by cursor.</summary>
public class SupplierProductPage
{
    /// <summary>The rows.</summary>
    public List<SupplierProductSummary>? Items { get; set; }
    /// <summary>Page number.</summary>
    public int Page { get; set; }
    /// <summary>Rows per page.</summary>
    public int PageSize { get; set; }
    /// <summary>Total rows, where the supplier reports it.</summary>
    public int? Total { get; set; }
    /// <summary><c>warming</c> or <c>stale</c> for a mirrored catalogue (BigBuy).</summary>
    public string? CatalogueState { get; set; }
}

internal class ResolveSupplierUrlRequest
{
    public string Url { get; set; } = string.Empty;
}

// ─── Importing ─────────────────────────────────────────────────────────────

/// <summary>How a price is worked out from the supplier's cost.</summary>
public class SupplierPriceRule
{
    /// <summary>See <see cref="PriceRuleTypes"/>.</summary>
    public string Type { get; set; } = PriceRuleTypes.MarkupPercent;
    /// <summary>The percentage, amount or margin.</summary>
    public decimal Value { get; set; }
    /// <summary>See <see cref="PriceRoundings"/>.</summary>
    public string? Rounding { get; set; }
    /// <summary>Add the supplier's freight estimate to the product's extra delivery fee.</summary>
    public bool? IncludeFreightEstimate { get; set; }
}

/// <summary>One product to import. Leave <see cref="SupplierVariantIds"/> empty to take every variant.</summary>
public class ImportSupplierProductItem
{
    /// <summary>The supplier's product id.</summary>
    public string SupplierProductId { get; set; } = string.Empty;
    /// <summary>The variants to sell.</summary>
    public List<string>? SupplierVariantIds { get; set; }
}

/// <summary>Defaults for imported products.</summary>
public class ImportSupplierProductDefaults
{
    /// <summary><c>draft</c>, <c>active</c> or <c>hidden</c>.</summary>
    public string? Status { get; set; }
    /// <summary>Existing tags to add.</summary>
    public List<string>? TagIds { get; set; }
    /// <summary>Tags to add by name.</summary>
    public List<string>? TagNames { get; set; }
}

/// <summary>Previewing or importing supplier products.</summary>
public class ImportSupplierProductsInput
{
    /// <summary>Up to 50 products, each once.</summary>
    public List<ImportSupplierProductItem> Items { get; set; } = new();
    /// <summary>Defaults to the store's rule.</summary>
    public SupplierPriceRule? PriceRule { get; set; }
    /// <summary>Defaults for the new products.</summary>
    public ImportSupplierProductDefaults? Defaults { get; set; }
    /// <summary>Import a supplier product the store already has, as a second copy.</summary>
    public bool? AllowDuplicate { get; set; }
}

/// <summary>An existing store product a preview row duplicates.</summary>
public class SupplierImportDuplicate
{
    /// <summary>The store product.</summary>
    public string? ProductId { get; set; }
    /// <summary>Its name.</summary>
    public string? Name { get; set; }
}

/// <summary>One row of an import preview.</summary>
public class SupplierImportPreviewRow
{
    /// <summary>The supplier's product id.</summary>
    public string? SupplierProductId { get; set; }
    /// <summary>Name.</summary>
    public string? Name { get; set; }
    /// <summary>Image.</summary>
    public string? Image { get; set; }
    /// <summary>Variants selected.</summary>
    public int VariantCount { get; set; }
    /// <summary>Cheapest cost.</summary>
    public decimal? CostMin { get; set; }
    /// <summary>Dearest cost.</summary>
    public decimal? CostMax { get; set; }
    /// <summary>Cheapest price after the rule.</summary>
    public decimal? PriceMin { get; set; }
    /// <summary>Dearest price after the rule.</summary>
    public decimal? PriceMax { get; set; }
    /// <summary>Currency.</summary>
    public string? Currency { get; set; }
    /// <summary>Set when this supplier product is already imported.</summary>
    public SupplierImportDuplicate? DuplicateOf { get; set; }
    /// <summary>Warnings.</summary>
    public List<string>? Warnings { get; set; }
    /// <summary>Why the row cannot be imported.</summary>
    public string? Error { get; set; }
}

/// <summary>An import preview's totals.</summary>
public class SupplierImportPreviewTotals
{
    /// <summary>Products that would be created.</summary>
    public int Products { get; set; }
    /// <summary>Credits per product.</summary>
    public decimal CreditsPerProduct { get; set; }
    /// <summary>Credits in all.</summary>
    public decimal Credits { get; set; }
}

/// <summary>What an import would do. Nothing is created or charged.</summary>
public class SupplierImportPreview
{
    /// <summary>One row per product.</summary>
    public List<SupplierImportPreviewRow>? Rows { get; set; }
    /// <summary>Totals.</summary>
    public SupplierImportPreviewTotals? Totals { get; set; }
}

/// <summary>One imported row's outcome.</summary>
public class SupplierImportRow
{
    /// <summary>The supplier's product id.</summary>
    public string? SupplierProductId { get; set; }
    /// <summary><c>added</c>, <c>failed</c>, <c>skipped</c> or <c>duplicate</c>.</summary>
    public string? State { get; set; }
    /// <summary>The store product created.</summary>
    public string? ProductId { get; set; }
    /// <summary>Name.</summary>
    public string? Name { get; set; }
    /// <summary>Warnings.</summary>
    public List<string>? Warnings { get; set; }
    /// <summary>Why it failed.</summary>
    public string? Message { get; set; }
}

/// <summary>
/// An import's answer: the rows (small imports) or a job id to poll (large
/// ones). <see cref="IsQueued"/> tells them apart.
/// </summary>
public class ImportSupplierProductsResult
{
    /// <summary>Set when the import was queued.</summary>
    public string? JobId { get; set; }
    /// <summary>Set when the import ran inline.</summary>
    public List<SupplierImportRow>? Rows { get; set; }

    /// <summary>True when the import was queued — poll <c>GetImportStatusAsync</c> with <see cref="JobId"/>.</summary>
    [JsonIgnore]
    public bool IsQueued => !string.IsNullOrEmpty(JobId);
}

/// <summary>A background import's progress.</summary>
public class SupplierImportJobStatus
{
    /// <summary>Job id.</summary>
    public string? JobId { get; set; }
    /// <summary><c>waiting</c>, <c>active</c>, <c>completed</c>, <c>failed</c>…</summary>
    public string? State { get; set; }
    /// <summary>0–100.</summary>
    public int Progress { get; set; }
    /// <summary>The rows, once completed.</summary>
    public List<SupplierImportRow>? Rows { get; set; }
}

// ─── Product links ─────────────────────────────────────────────────────────

/// <summary>A store variant mapped to a supplier variant.</summary>
public class StoreProductSupplierLinkVariant
{
    /// <summary>The store variant's combination key (null for a product without variants).</summary>
    public string? CombinationKey { get; set; }
    /// <summary>The store option value keys.</summary>
    public List<string>? ValueKeys { get; set; }
    /// <summary>The supplier's variant id.</summary>
    public string? SupplierVariantId { get; set; }
    /// <summary>The supplier's SKU.</summary>
    public string? SupplierSku { get; set; }
    /// <summary>Cost at the supplier.</summary>
    public decimal Cost { get; set; }
    /// <summary>Currency of the cost.</summary>
    public string? Currency { get; set; }
    /// <summary>Stock at the supplier.</summary>
    public int? Stock { get; set; }
    /// <summary>The supplier stopped offering it; stock is held at zero, never deleted.</summary>
    public bool? Unavailable { get; set; }
    /// <summary>Last synced.</summary>
    public string? LastSyncedAt { get; set; }
}

/// <summary>A store product and the supplier product it is fulfilled from.</summary>
public class StoreProductSupplierLink
{
    /// <summary>Link id.</summary>
    [JsonPropertyName("_id")]
    public string? Id { get; set; }
    /// <summary>The store product.</summary>
    public string? ProductId { get; set; }
    /// <summary>The connection.</summary>
    public string? IntegrationId { get; set; }
    /// <summary>Catalogue key.</summary>
    public string? SupplierKey { get; set; }
    /// <summary>The supplier's product id.</summary>
    public string? SupplierProductId { get; set; }
    /// <summary>The product's page at the supplier.</summary>
    public string? SupplierProductUrl { get; set; }
    /// <summary>Variant mapping.</summary>
    public List<StoreProductSupplierLinkVariant>? Variants { get; set; }
    /// <summary>The product's price rule.</summary>
    public SupplierPriceRule? PriceRule { get; set; }
    /// <summary>What the hourly sync may overwrite; keys from <see cref="LinkSyncFields"/>.</summary>
    public Dictionary<string, bool>? Sync { get; set; }
    /// <summary>Delivery estimate.</summary>
    public DeliveryEstimate? DeliveryEstimate { get; set; }
    /// <summary>Last sync.</summary>
    public string? LastSyncAt { get; set; }
    /// <summary>Why the last sync failed.</summary>
    public string? LastSyncError { get; set; }
    /// <summary>Warnings.</summary>
    public List<string>? Warnings { get; set; }
    /// <summary>Created at.</summary>
    public string? CreatedAt { get; set; }
    /// <summary>Updated at.</summary>
    public string? UpdatedAt { get; set; }
}

internal class SupplierLinkList
{
    public List<StoreProductSupplierLink>? Items { get; set; }
}

internal class SupplierIntegrationList
{
    public List<StoreSupplierIntegration>? Items { get; set; }
}

/// <summary>A store variant mapped to a supplier variant, when linking.</summary>
public class SupplierLinkVariantInput
{
    /// <summary>The store variant's combination key; null for a product without variants.</summary>
    public string? CombinationKey { get; set; }
    /// <summary>The supplier's variant id.</summary>
    public string SupplierVariantId { get; set; } = string.Empty;
}

/// <summary>Linking a product the store already sells to a supplier product.</summary>
public class CreateSupplierLinkInput
{
    /// <summary>The store product.</summary>
    public string ProductId { get; set; } = string.Empty;
    /// <summary>The connection.</summary>
    public string IntegrationId { get; set; } = string.Empty;
    /// <summary>The supplier's product id.</summary>
    public string SupplierProductId { get; set; } = string.Empty;
    /// <summary>Variant mapping.</summary>
    public List<SupplierLinkVariantInput> Variants { get; set; } = new();
    /// <summary>Pull cost and stock straight away. Defaults to true.</summary>
    public bool? SyncNow { get; set; }
}

/// <summary>A product's partner disclosure.</summary>
public class SupplierLinkDisclosureInput
{
    /// <summary>Show the partner badge for this product.</summary>
    public bool? Enabled { get; set; }
    /// <summary>The product's own label; null for the store's.</summary>
    public string? Label { get; set; }
}

/// <summary>Changing a link. Send at least one field.</summary>
public class UpdateSupplierLinkInput
{
    /// <summary>The product's price rule.</summary>
    public SupplierPriceRule? PriceRule { get; set; }
    /// <summary>What sync may overwrite; keys from <see cref="LinkSyncFields"/>.</summary>
    public Dictionary<string, bool>? Sync { get; set; }
    /// <summary>Delivery estimate.</summary>
    public DeliveryEstimate? DeliveryEstimate { get; set; }
    /// <summary>Partner disclosure.</summary>
    public SupplierLinkDisclosureInput? Disclosure { get; set; }
    /// <summary>Reprice the product with the new rule now. A rule change alone never reprices.</summary>
    public bool? ApplyPriceRuleNow { get; set; }
}

/// <summary>What a manual sync did.</summary>
public class SupplierLinkSyncResult
{
    /// <summary>The link after the sync.</summary>
    public StoreProductSupplierLink? Link { get; set; }
    /// <summary>The fields that changed; empty when already up to date.</summary>
    public List<string>? Changed { get; set; }
}

/// <summary>A deleted link.</summary>
public class DeletedSupplierLink
{
    /// <summary>Link id.</summary>
    [JsonPropertyName("_id")]
    public string? Id { get; set; }
}
