# Posty5.Store

Online Store management client for the [Posty5](https://posty5.com) .NET SDK —
run a store's **catalogue, orders, tags, customers, shipping and dropshipping suppliers** from anywhere.

## Install

```bash
dotnet add package Posty5.Store
dotnet add package Posty5.Core
```

## Authenticate

Create an [API key](https://studio.posty5.com) and pass it to the core
`Posty5HttpClient` (sent as the `X-API-Key` header). Every call is scoped to a
store id and authorized by the key owner's store permission — `products.manage`
for the catalogue and tags, `orders.*` for orders and customers,
`settings.manage` for shipping, and `suppliers.view` / `suppliers.manage` /
`suppliers.import` / `suppliers.orders.manage` for dropshipping. A store's
owner holds all of them.

> An API key carries the full identity of the user who created it; it is not
> scoped to a single store. Treat it as you would a password.

```csharp
using Posty5.Core.Http;
using Posty5.Core.Models;
using Posty5.Store;
using Posty5.Store.Models;

var http = new Posty5HttpClient(new Posty5Options { ApiKey = Environment.GetEnvironmentVariable("POSTY5_API_KEY") });
var store = new StoreClient(http);
```

The client is split into six areas: `store.Products`, `store.Orders`,
`store.Tags`, `store.Customers`, `store.Shipping` and `store.Suppliers`.

## Products

```csharp
// Whole-document create.
var product = await store.Products.CreateAsync(storeId, new CreateProductInput
{
    Name = "Classic Tee",
    Price = 20,
    Stock = 100,
    Sku = "TEE-001",
});

// Or up to 200 at a time — a row that fails validation is reported, not fatal.
var report = await store.Products.BulkCreateAsync(storeId, new[]
{
    new BulkProductInput { Name = "Hoodie", Price = 45 },
});
Console.WriteLine($"{report!.Imported} created, {report.Failed} failed");

// Search, with tag filters and cursor pagination.
var page = await store.Products.SearchAsync(storeId,
    new ProductSearchParams { Status = "active", TagIds = new() { tagId } },
    new PaginationParams { PageSize = 50 });
```

### Section updates

`CreateAsync`/`UpdateAsync` take the whole document. The section methods write
only the part they name, so a stock-sync job and a merchant editing the
description never overwrite each other:

```csharp
await store.Products.UpdateStockAsync(storeId, productId, new ProductStockInput { Stock = 42 });
await store.Products.UpdatePriceAsync(storeId, productId, new ProductPriceInput { Price = 18, CompareAtPrice = 25 });
await store.Products.UpdateSeoAsync(storeId, productId, new ProductSeoInput
{
    Seo = new ProductSeoFields { Title = "Classic Tee", MetaDescription = "100% cotton" },
    Slug = "classic-tee",
});
```

Sections: `UpdateBasicInformationAsync`, `UpdateMediaAsync`, `UpdatePriceAsync`,
`UpdateStockAsync`, `UpdateVariantsAsync`, `UpdateTagsAsync`, `UpdateSeoAsync`,
`UpdateSettingsAsync`, `UpdateLandingAsync`, `UpdateShippingAsync`,
`UpdatePurchaseAsync`.

### Clone, import, AI

```csharp
// Paste a link from any storefront → a draft with an external buy link.
var draft = await store.Products.CloneFromUrlAsync(storeId, "https://example.com/p/123");

// Excel round-trip.
var template = await store.Products.DownloadImportTemplateAsync(storeId);
await File.WriteAllBytesAsync(template.FileName ?? "template.xlsx", template.Data);

await store.Products.ImportFromExcelAsync(storeId, new ExcelUploadInput
{
    FileBase64 = Convert.ToBase64String(await File.ReadAllBytesAsync("products.xlsx")),
    FileName = "products.xlsx",
});

// Price a landing-page generation, then run it. The result is an UNSAVED draft.
var ask = new ProductAiContentInput
{
    Brief = "A soft cotton tee for summer",
    SectionKeys = new() { "hero", "features" },
};
var estimate = await store.Products.EstimateAiContentAsync(storeId, productId, ask);
var generated = await store.Products.GenerateAiContentAsync(storeId, productId, ask);
```

## Orders

```csharp
var page = await store.Orders.SearchAsync(storeId,
    new OrderSearchParams { Status = "pending", OrderSource = "facebook", FromDate = "2026-07-01" },
    new PaginationParams { PageSize = 50 });

// Record an order received off-store (tagged createdFrom: "dotnet").
var order = await store.Orders.CreateAsync(storeId, new CreateOrderInput
{
    Items = new() { new OrderItemInput { ProductId = productId, Qty = 2 } },
    Customer = new OrderCustomerInput
    {
        Name = "Sara",
        Phone = "0100000000",
        Address = "12 Nile St",
        // City names repeat across governorates, so send the pair.
        CountryIso = "eg",
        GovernorateCode = "C",
        CityKey = "cairo",
    },
    OrderSource = "facebook",
});

await store.Orders.UpdateStatusAsync(storeId, order!.Id!, "confirmed", "Called the customer");
await store.Orders.AddInternalNoteAsync(storeId, order.Id!, "Wants evening delivery");

var stats = await store.Orders.StatisticsAsync(storeId, new OrderStatisticsParams { Days = 30 });
var workbook = await store.Orders.ExportToExcelAsync(storeId, new OrderSearchParams { Status = "delivered" });
await File.WriteAllBytesAsync(workbook.FileName ?? "orders.xlsx", workbook.Data);
```

The status workflow is enforced server-side: `pending → confirmed → processing →
shipped → delivered`, with `cancelled`/`refused` reachable from any non-terminal
state and the three terminal states accepting nothing further.

An order with dropshipped items is split into parts: `order.FulfilmentGroups`
holds one for the store's own items and one per supplier connection, each with
its own `Status` and `Shipment`. The order moves at the pace of its slowest
part, so on a multi-part order `shipped` and `delivered` are reached by the
parts rather than set by hand. List rows carry `FulfilmentSummary`, and
`new OrderSearchParams { NeedsAttention = true }` finds orders with a paused part.

## Tags

```csharp
var tag = await store.Tags.CreateAsync(storeId, new CreateTagInput { Name = "Summer", AutoRemoveAfterDays = 90 });
await store.Tags.AssignProductsAsync(storeId, tag!.Id!, new[] { productId });
await store.Tags.SetProductTagsAsync(storeId, productId, new[] { tag.Id! });
var summer = await store.Tags.ResolveProductsAsync(storeId, new[] { tag.Id! }, limit: 12);
```

## Customers

Read-only — a customer record is derived from orders and, when the shopper has a
Posty5 account, owned by them.

```csharp
var customers = await store.Customers.SearchAsync(storeId, new CustomerSearchParams { Text = "sara", HasAccount = true });
var profile = await store.Customers.GetAsync(storeId, customerId);
var theirOrders = await store.Customers.OrdersAsync(storeId, customerId);
```

## Shipping

The model is **country → governorate → city**, and a fee falls through those in
order before landing on the store default. `null` at any level means "not set
here — inherit"; an explicit `0` is free delivery and stops the fallback.

The world's countries, governorates and cities are **reference data** and are
never stored on a store. A store owns one document per open country, plus one
**route** row per place it prices or blocks differently — everywhere it said
nothing simply has no row.

```csharp
// Opening a country prices everything inside it. No rows are written yet.
await store.Shipping.AddCountryAsync(storeId, new AddShippingCountryInput { Iso = "eg", DefaultFee = 50 });

// The picker behind "add a country" — the world minus what you already opened.
var catalogue = await store.Shipping.ListCountryCatalogueAsync(storeId, new ShippingCatalogueSearchParams { Text = "eg" });

// Governorates are catalogue reference data, not your rows.
var govs = await store.Shipping.ListGovernoratesAsync(storeId, "eg");

// Every place with what it charges today, and where that price came from.
var routes = await store.Shipping.ListRoutesAsync(storeId, "eg",
    new ShippingRouteSearchParams { Level = "city", GovernorateCode = "C" });

foreach (var r in routes!.Items)
    Console.WriteLine($"{r.CityName} {r.EffectiveFee} ({r.InheritedFrom})");

// Price or block one place, or up to 200 at once.
await store.Shipping.UpsertRouteAsync(storeId, "eg", new UpsertShippingRouteInput
{
    Level = "city",
    GovernorateCode = "C",
    CityKey = "cairo",
    Fee = 30,
});

await store.Shipping.BulkUpsertRoutesAsync(storeId, "eg", new[]
{
    new UpsertShippingRouteInput { Level = "governorate", GovernorateCode = "ALX", IsAllowed = false },
});

// One fee for a whole scope. Sending the fee they already inherit CLEARS the
// rows instead of writing them — uniformity is "no rows" in this model.
var applied = await store.Shipping.ApplyFeeAsync(storeId, "eg",
    new ApplyShippingFeeInput { Level = "city", Fee = 40 });

// Put one place back on what it inherits.
await store.Shipping.ClearRouteAsync(storeId, rateId);

// What would this destination be charged? Same resolution checkout uses.
var quote = await store.Shipping.PreviewFeeAsync(storeId, "eg", "C", "cairo");
```

A route that ends up saying nothing — no fee **and** delivery allowed — is
removed rather than stored, so `UpsertRouteAsync` answers with `Cleared = true`
and `Rate = null`. That is not an error: it is the state "no row" already
represents.

Per-product surcharges are separate, and always charged **per unit**:

```csharp
await store.Products.UpdateShippingAsync(storeId, productId,
    new ProductShippingInput { ExtraFeePerUnit = 5, Note = "Bulky" });
```

## Suppliers

Dropshipping: connect a supplier account, import its products, and follow the
orders sent to it. Connecting, importing, sending and paying need the store
owner's plan to include dropshipping (Pro and above).

```csharp
// 1. Connect — the credential keys come from the catalogue entry.
var catalogue = await store.Suppliers.GetCatalogueAsync(storeId);
var cj = catalogue!.Items!.First(s => s.Key == "cjdropshipping");
// cj.CredentialFields → [{ Key = "apiKey", Label = "API key", Secret = true }]
var connection = await store.Suppliers.ConnectAsync(storeId, new ConnectSupplierInput
{
    SupplierKey = "cjdropshipping",
    Mode = SupplierModes.Test, // CJdropshipping has a sandbox: test orders are never charged or shipped
    Credentials = new() { ["apiKey"] = Environment.GetEnvironmentVariable("CJ_API_KEY")! },
});

// 2. Decide what it may do on its own — every connection starts on "manual".
await store.Suppliers.UpdateAutomationAsync(storeId, connection!.Id!,
    new SupplierAutomationInput { Mode = SupplierAutomationModes.Submit, MaxCostPerOrder = 50 });

// 3. Browse, preview, import.
var page = await store.Suppliers.BrowseProductsAsync(storeId, connection.Id!, new BrowseSupplierProductsParams { Q = "mug" });
var request = new ImportSupplierProductsInput
{
    Items = { new ImportSupplierProductItem { SupplierProductId = page!.Items![0].SupplierProductId! } },
};
var preview = await store.Suppliers.PreviewImportAsync(storeId, connection.Id!, request);
if (preview!.Rows![0].DuplicateOf == null)
{
    var result = await store.Suppliers.ImportProductsAsync(storeId, connection.Id!, request);
    if (result!.IsQueued)
    {
        var status = await store.Suppliers.GetImportStatusAsync(storeId, result.JobId!);
    }
}

// 4. Watch the queue and act on a paused part. The queue pages by cursor; read
//    every page first, because a retry changes the list the cursor walks.
var paused = new List<StoreSupplierOrder>();
string? cursor = null;
do
{
    var queue = await store.Suppliers.ListSupplierOrdersAsync(storeId,
        new SupplierOrderSearchParams { NeedsReview = true },
        new PaginationParams { Cursor = cursor, PageSize = 100 });
    paused.AddRange(queue!.Items);
    cursor = queue.Pagination.HasMore ? queue.Pagination.NextCursor : null;
} while (cursor != null);

foreach (var supplierOrder in paused.Where(o => o.ReviewReason == SupplierReviewReasons.CostChanged))
{
    await store.Suppliers.RetryAsync(storeId, supplierOrder.Id!, acceptCost: true);
}
```

- **Credentials are write-only.** No response carries them; a connection says
  only `HasCredentials`. Debug mode logs the method and URL, never a body.
- **Paused outcomes throw.** `SubmitGroupAsync`, `RetryAsync` and `PayAsync`
  answer a pause (`needsReview`, `failed`, a part already being sent) as a 400,
  so they throw `Posty5ValidationException`; read the supplier order again to see why.
- **No duplicates.** A second submit finds the first supplier order, and
  `PayAsync` reads the supplier's status first — an order already paid there is
  recorded, not paid again.
- **Money.** A key holding `suppliers.orders.manage` can spend the merchant's
  balance at the supplier. Treat it accordingly.
- **Vocabularies are strings**, with names in `SupplierOrderStatuses`,
  `SupplierReviewReasons`, `SupplierAutomationModes` and friends — a value added
  on the server never breaks deserialization.
- The supplier-order queue (`ListSupplierOrdersAsync`) pages by cursor like
  every other list — `PaginationParams` (`Cursor`, `PageSize`: max 100; the
  API uses 25 when no `PaginationParams` is passed), answered as
  `PaginationResponse<StoreSupplierOrder>`. Only
  the supplier's own catalogue (`BrowseProductsAsync`) pages by number
  (`page`, `pageSize`).

## API

### `store.Products` — `/api/store-products`

| Method | Endpoint |
| --- | --- |
| `SearchAsync(storeId, filters?, pagination?)` | `GET /{storeId}` |
| `GetAsync(storeId, productId)` | `GET /{storeId}/{id}` |
| `GetLandingSectionsConfigAsync()` | `GET /config/landing-sections` |
| `CreateAsync(storeId, product)` | `POST /{storeId}` |
| `BulkCreateAsync(storeId, products)` | `POST /{storeId}/bulk` |
| `CreateDraftAsync(storeId, draft)` | `POST /{storeId}/draft` |
| `CloneFromUrlAsync(storeId, url)` | `POST /{storeId}/clone` |
| `DownloadImportTemplateAsync(storeId)` | `GET /{storeId}/import/template` |
| `ImportFromExcelAsync(storeId, file)` | `POST /{storeId}/import` |
| `UpdateAsync(storeId, productId, changes)` | `PUT /{storeId}/{id}` |
| `DeleteAsync(storeId, productId)` | `DELETE /{storeId}/{id}` |
| `ReorderAsync(storeId, items)` | `PUT /{storeId}/reorder` |
| `UpdateBasicInformationAsync(…)` | `PATCH /{storeId}/{id}/basic-information` |
| `UpdateMediaAsync(…)` | `PATCH /{storeId}/{id}/media` |
| `UpdatePriceAsync(…)` | `PATCH /{storeId}/{id}/price` |
| `UpdateStockAsync(…)` | `PATCH /{storeId}/{id}/stock` |
| `UpdateVariantsAsync(…)` | `PATCH /{storeId}/{id}/variants` |
| `UpdateTagsAsync(…)` | `PATCH /{storeId}/{id}/tags` |
| `UpdateSeoAsync(…)` | `PATCH /{storeId}/{id}/seo` |
| `UpdateSettingsAsync(…)` | `PATCH /{storeId}/{id}/settings` |
| `UpdateLandingAsync(…)` | `PATCH /{storeId}/{id}/landing` |
| `UpdateShippingAsync(…)` | `PATCH /{storeId}/{id}/shipping` |
| `UpdatePurchaseAsync(…)` | `PATCH /{storeId}/{id}/purchase` |
| `CreateImageUploadUrlAsync(…)` | `POST /{storeId}/{id}/image-upload-url` |
| `EstimateAiContentAsync(…)` | `POST /{storeId}/{id}/ai/estimate` |
| `GenerateAiContentAsync(…)` | `POST /{storeId}/{id}/ai/generate` |

### `store.Orders` — `/api/store-orders`

| Method | Endpoint |
| --- | --- |
| `SearchAsync(storeId, filters?, pagination?)` | `GET /{storeId}` |
| `GetAsync(storeId, orderId)` | `GET /{storeId}/{id}` |
| `StatisticsAsync(storeId, filters?)` | `GET /{storeId}/statistics` |
| `PrintDataAsync(storeId, filters?)` | `GET /{storeId}/print` |
| `ExportToExcelAsync(storeId, filters?)` | `GET /{storeId}/export` |
| `CreateAsync(storeId, order)` | `POST /{storeId}` |
| `UpdateStatusAsync(storeId, id, status, note?)` | `POST /{storeId}/{id}/status` |
| `AddInternalNoteAsync(storeId, id, note)` | `POST /{storeId}/{id}/notes` |

### `store.Tags` — `/api/store-tags`

| Method | Endpoint |
| --- | --- |
| `SearchAsync(storeId, filters?, pagination?)` | `GET /{storeId}` |
| `GetAsync(storeId, tagId)` | `GET /{storeId}/{id}` |
| `CreateAsync(storeId, tag)` | `POST /{storeId}` |
| `UpdateAsync(storeId, tagId, changes)` | `PUT /{storeId}/{id}` |
| `DeleteAsync(storeId, tagId)` | `DELETE /{storeId}/{id}` |
| `ResolveProductsAsync(storeId, tagIds, limit?)` | `GET /{storeId}/resolve` |
| `ListProductsAsync(storeId, tagId, search?, pagination?)` | `GET /{storeId}/{id}/products` |
| `AssignProductsAsync(storeId, tagId, productIds)` | `POST /{storeId}/{id}/products` |
| `UnassignProductAsync(storeId, tagId, productId)` | `DELETE /{storeId}/{id}/products/{productId}` |
| `GetProductTagsAsync(storeId, productId)` | `GET /{storeId}/product/{productId}` |
| `SetProductTagsAsync(storeId, productId, tagIds)` | `PUT /{storeId}/product/{productId}` |
| `DownloadImportTemplateAsync(storeId)` | `GET /{storeId}/import/template` |
| `ImportFromExcelAsync(storeId, file)` | `POST /{storeId}/import` |
| `ExportToExcelAsync(storeId, filters?)` | `GET /{storeId}/export` |

### `store.Customers` — `/api/store-customers`

| Method | Endpoint |
| --- | --- |
| `SearchAsync(storeId, filters?, pagination?)` | `GET /{storeId}` |
| `GetAsync(storeId, customerId)` | `GET /{storeId}/{customerId}` |
| `AddressesAsync(storeId, customerId)` | `GET /{storeId}/{customerId}/addresses` |
| `OrdersAsync(storeId, customerId, pagination?)` | `GET /{storeId}/{customerId}/orders` |

### `store.Shipping` — `/api/store-shipping`

| Method | Endpoint |
| --- | --- |
| `ListCountriesAsync(storeId, filters?, pagination?)` | `GET /{storeId}/countries` |
| `ListCountryCatalogueAsync(storeId, filters?)` | `GET /{storeId}/countries/catalogue` |
| `GetCountryAsync(storeId, iso)` | `GET /{storeId}/countries/{iso}` |
| `AddCountryAsync(storeId, input)` | `POST /{storeId}/countries` |
| `UpdateCountryAsync(storeId, iso, changes)` | `PUT /{storeId}/countries/{iso}` |
| `DeleteCountryAsync(storeId, iso)` | `DELETE /{storeId}/countries/{iso}` |
| `ListGovernoratesAsync(storeId, iso)` | `GET /{storeId}/countries/{iso}/governorates` |
| `ListRoutesAsync(storeId, iso, filters?)` | `GET /{storeId}/countries/{iso}/routes` |
| `UpsertRouteAsync(storeId, iso, input)` | `POST /{storeId}/countries/{iso}/routes` |
| `BulkUpsertRoutesAsync(storeId, iso, items)` | `POST /{storeId}/countries/{iso}/routes/bulk` |
| `ApplyFeeAsync(storeId, iso, input)` | `POST /{storeId}/countries/{iso}/routes/apply-fee` |
| `ClearRouteAsync(storeId, rateId)` | `DELETE /{storeId}/routes/{rateId}` |
| `PreviewFeeAsync(storeId, countryIso, governorateCode?, cityKey?)` | `GET /{storeId}/preview-fee` |

### `store.Suppliers` — `/api/store-suppliers`

| Method | Endpoint | Permission |
| --- | --- | --- |
| `GetCatalogueAsync(storeId)` | `GET /{storeId}/catalogue` | `suppliers.view` |
| `ListAsync(storeId)` | `GET /{storeId}` | `suppliers.view` |
| `ConnectAsync(storeId, input)` | `POST /{storeId}` | `suppliers.manage` |
| `StartOAuthAsync(storeId, input)` | `POST /{storeId}/oauth/start` | `suppliers.manage` |
| `ReplaceCredentialsAsync(storeId, id, input)` | `PUT /{storeId}/{id}` | `suppliers.manage` |
| `UpdateSettingsAsync(storeId, id, input)` | `PUT /{storeId}/{id}/settings` | `suppliers.manage` |
| `UpdateAutomationAsync(storeId, id, automation)` | `PUT /{storeId}/{id}/automation` | `suppliers.manage` |
| `SetEnabledAsync(storeId, id, enabled)` | `PUT /{storeId}/{id}/enabled` | `suppliers.manage` |
| `TestAsync(storeId, id)` | `POST /{storeId}/{id}/test` | `suppliers.manage` |
| `GetBalanceAsync(storeId, id)` | `GET /{storeId}/{id}/balance` | `suppliers.view` |
| `GetDisconnectImpactAsync(storeId, id)` | `GET /{storeId}/{id}/impact` | `suppliers.view` |
| `DisconnectAsync(storeId, id, force?)` | `DELETE /{storeId}/{id}` | `suppliers.manage` |
| `BrowseProductsAsync(storeId, id, filters?, page?, pageSize?)` | `GET /{storeId}/{id}/products` | `suppliers.import` |
| `GetProductAsync(storeId, id, supplierProductId)` | `GET /{storeId}/{id}/products/{supplierProductId}` | `suppliers.import` |
| `ResolveUrlAsync(storeId, id, url)` | `POST /{storeId}/{id}/products/resolve-url` | `suppliers.import` |
| `PreviewImportAsync(storeId, id, input)` | `POST /{storeId}/{id}/import/preview` | `suppliers.import` |
| `ImportProductsAsync(storeId, id, input)` | `POST /{storeId}/{id}/import` | `suppliers.import` |
| `GetImportStatusAsync(storeId, jobId)` | `GET /{storeId}/imports/{jobId}` | `suppliers.import` |
| `ListLinksAsync(storeId, productId?)` | `GET /{storeId}/links` | `suppliers.view` |
| `CreateLinkAsync(storeId, input)` | `POST /{storeId}/links` | `suppliers.import` |
| `UpdateLinkAsync(storeId, linkId, changes)` | `PUT /{storeId}/links/{linkId}` | `suppliers.import` |
| `DeleteLinkAsync(storeId, linkId)` | `DELETE /{storeId}/links/{linkId}` | `suppliers.import` |
| `SyncLinkAsync(storeId, linkId)` | `POST /{storeId}/links/{linkId}/sync` | `suppliers.import` |
| `ListSupplierOrdersAsync(storeId, filters?, pagination?)` | `GET /{storeId}/orders` | `suppliers.view` |
| `GetSupplierOrderAsync(storeId, supplierOrderId)` | `GET /{storeId}/orders/{supplierOrderId}` | `suppliers.view` |
| `SubmitGroupAsync(storeId, orderId, groupKey, payNow?)` | `POST /{storeId}/orders/{orderId}/groups/{groupKey}/submit` | `suppliers.orders.manage` |
| `RetryAsync(storeId, supplierOrderId, acceptCost?)` | `POST /{storeId}/orders/{supplierOrderId}/retry` | `suppliers.orders.manage` |
| `PayAsync(storeId, supplierOrderId)` | `POST /{storeId}/orders/{supplierOrderId}/pay` | `suppliers.orders.manage` |
| `CancelAsync(storeId, supplierOrderId)` | `POST /{storeId}/orders/{supplierOrderId}/cancel` | `suppliers.orders.manage` |
| `FulfilGroupManuallyAsync(storeId, orderId, groupKey)` | `POST /{storeId}/orders/{orderId}/groups/{groupKey}/fulfil-manually` | `suppliers.orders.manage` |

### Shorthands

`BulkCreateProductsAsync`, `SearchOrdersAsync`, `CreateOrderAsync` and
`UpdateOrderStatusAsync` remain on `StoreClient` itself and delegate to the
sub-clients.

## Pagination

Lists page by opaque cursor, not by page number, so a list stays stable while
rows are written underneath it. This includes the supplier-order queue
(`store.Suppliers.ListSupplierOrdersAsync`); the one exception is a supplier's
own catalogue (`store.Suppliers.BrowseProductsAsync`), which pages by `page`:

```csharp
string? cursor = null;
do
{
    var page = await store.Orders.SearchAsync(storeId, null, new PaginationParams { Cursor = cursor, PageSize = 100 });
    Handle(page!.Items);
    cursor = page.Pagination.NextCursor;
} while (cursor != null);
```

## File downloads

`DownloadImportTemplateAsync`, `ExportToExcelAsync` and friends answer with the
file itself rather than the JSON envelope, so they return a `FileResponse`
(`Data`, `ContentType`, `FileName`).

## Credits

Product, order and store operations (`addProduct`, `manualOrder`,
`orderStatusChange`, `exportOrders`, AI generation) are charged to the store
owner per the account's plan. Tag operations are free and unmetered.
Importing a supplier product is charged like adding a product; connecting,
linking, syncing and every supplier-order action are free.

## License

MIT
