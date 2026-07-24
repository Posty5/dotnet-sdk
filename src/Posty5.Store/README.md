# Posty5.Store

Online Store management client for the [Posty5](https://posty5.com) .NET SDK —
manage a store's **products** and **orders** programmatically.

## Install

```bash
dotnet add package Posty5.Store
dotnet add package Posty5.Core
```

## Authenticate

Create an [API key](https://studio.posty5.com) and pass it to the core
`Posty5HttpClient` (sent as the `X-API-Key` header). The key's owner must hold
the matching store permission.

```csharp
using Posty5.Core.Http;
using Posty5.Store;
using Posty5.Store.Models;

var http = new Posty5HttpClient(new Posty5Options { ApiKey = Environment.GetEnvironmentVariable("POSTY5_API_KEY") });
var store = new StoreClient(http);
```

## Products

```csharp
// Bulk-create products (charges addProduct per created row).
var report = await store.BulkCreateProductsAsync(storeId, new[]
{
    new BulkProductInput { Name = "Classic Tee", Price = 20, Stock = 100, Sku = "TEE-001" },
    new BulkProductInput { Name = "Hoodie", Price = 45 },
});
Console.WriteLine($"{report!.Imported} created, {report.Failed} failed");
```

## Orders

```csharp
// Search with filters + cursor pagination.
var page = await store.SearchOrdersAsync(storeId,
    new OrderSearchParams { Status = "pending", OrderSource = "facebook" },
    new PaginationParams { PageSize = 50 });

// Create an order manually (tagged createdFrom: "dotnet").
var order = await store.CreateOrderAsync(storeId, new CreateOrderInput
{
    Items = new() { new OrderItemInput { ProductId = productId, Qty = 2 } },
    Customer = new OrderCustomerInput { Name = "Sara", Phone = "0100000000", Address = "Cairo" },
    OrderSource = "facebook",
});

// Change status (respects the store status workflow).
await store.UpdateOrderStatusAsync(storeId, order!.Id!, "confirmed", "Called the customer");
```

## API

| Method | Endpoint |
| --- | --- |
| `BulkCreateProductsAsync(storeId, products)` | `POST /api/store-products/{storeId}/bulk` |
| `SearchOrdersAsync(storeId, filters?, pagination?)` | `GET /api/store-orders/{storeId}` |
| `CreateOrderAsync(storeId, order)` | `POST /api/store-orders/{storeId}` |
| `UpdateOrderStatusAsync(storeId, id, status, note?)` | `POST /api/store-orders/{storeId}/{id}/status` |

## License

MIT
