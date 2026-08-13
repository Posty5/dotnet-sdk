# Posty5.Store

Online Store management client for the [Posty5](https://posty5.com) .NET SDK —
run a store's **catalogue, orders, tags, customers and shipping** from anywhere.

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
`settings.manage` for shipping. A store's owner holds all of them.

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

The client is split into five areas: `store.Products`, `store.Orders`,
`store.Tags`, `store.Customers` and `store.Shipping`.

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

### Shorthands

`BulkCreateProductsAsync`, `SearchOrdersAsync`, `CreateOrderAsync` and
`UpdateOrderStatusAsync` remain on `StoreClient` itself and delegate to the
sub-clients.

## Pagination

Lists page by opaque cursor, not by page number, so a list stays stable while
rows are written underneath it:

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

## License

MIT
