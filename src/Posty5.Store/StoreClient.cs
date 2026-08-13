using Posty5.Core.Http;
using Posty5.Core.Models;
using Posty5.Store.Clients;
using Posty5.Store.Models;

namespace Posty5.Store;

/// <summary>
/// Client for managing an Online Store via the Posty5 API — catalogue, orders,
/// tags, customers and shipping.
/// </summary>
/// <remarks>
/// Authenticate with an API key on <see cref="Posty5HttpClient"/> (sent as the
/// <c>X-API-Key</c> header). Every call is scoped to a store id and authorized
/// by the key owner's store permission: <c>products.manage</c> for the catalogue
/// and tags, <c>orders.view</c> / <c>orders.create</c> /
/// <c>orders.updateStatus</c> for orders and customers, <c>settings.manage</c>
/// for shipping. A store's owner holds all of them.
/// <para>
/// An API key carries the full identity of the user who created it — it is not
/// scoped to one store. Treat it as you would a password.
/// </para>
/// <example>
/// <code>
/// var http = new Posty5HttpClient(new Posty5Options { ApiKey = apiKey });
/// var store = new StoreClient(http);
///
/// await store.Products.CreateAsync(storeId, new CreateProductInput { Name = "Tee", Price = 20 });
/// var orders = await store.Orders.SearchAsync(storeId, new OrderSearchParams { Status = "pending" });
/// await store.Shipping.AddCountryAsync(storeId, new AddShippingCountryInput { Iso = "eg", DefaultFee = 50 });
/// </code>
/// </example>
/// </remarks>
public class StoreClient
{
    /// <summary>The catalogue: products, variants, landing pages, Excel import.</summary>
    public StoreProductsClient Products { get; }

    /// <summary>Orders: search, statistics, manual entry, the status workflow, export.</summary>
    public StoreOrdersClient Orders { get; }

    /// <summary>Catalogue tags and product assignments.</summary>
    public StoreTagsClient Tags { get; }

    /// <summary>The people who have ordered from the store (read-only).</summary>
    public StoreCustomersClient Customers { get; }

    /// <summary>Shipping countries, cities and fees.</summary>
    public StoreShippingClient Shipping { get; }

    /// <summary>Creates a new Store client.</summary>
    /// <param name="httpClient">HTTP client instance from Posty5.Core.</param>
    public StoreClient(Posty5HttpClient httpClient)
    {
        if (httpClient == null) throw new ArgumentNullException(nameof(httpClient));

        Products = new StoreProductsClient(httpClient);
        Orders = new StoreOrdersClient(httpClient);
        Tags = new StoreTagsClient(httpClient);
        Customers = new StoreCustomersClient(httpClient);
        Shipping = new StoreShippingClient(httpClient);
    }

    // ─── Shorthands ─────────────────────────────────────────────────────────
    //
    // The four methods this client shipped with, before the sub-clients existed.
    // They stay because they are published API, and they delegate rather than
    // re-implement so there is one code path per endpoint.

    /// <summary>Shorthand for <c>Products.BulkCreateAsync</c>.</summary>
    public Task<BulkProductsReport?> BulkCreateProductsAsync(
        string storeId,
        IEnumerable<BulkProductInput> products,
        CancellationToken cancellationToken = default)
        => Products.BulkCreateAsync(storeId, products, cancellationToken);

    /// <summary>Shorthand for <c>Orders.SearchAsync</c>.</summary>
    public Task<PaginationResponse<StoreOrderSummary>?> SearchOrdersAsync(
        string storeId,
        OrderSearchParams? filters = null,
        PaginationParams? pagination = null,
        CancellationToken cancellationToken = default)
        => Orders.SearchAsync(storeId, filters, pagination, cancellationToken);

    /// <summary>Shorthand for <c>Orders.CreateAsync</c>.</summary>
    public Task<StoreOrder?> CreateOrderAsync(
        string storeId,
        CreateOrderInput order,
        CancellationToken cancellationToken = default)
        => Orders.CreateAsync(storeId, order, cancellationToken);

    /// <summary>Shorthand for <c>Orders.UpdateStatusAsync</c>.</summary>
    public Task<StoreOrder?> UpdateOrderStatusAsync(
        string storeId,
        string orderId,
        string status,
        string? note = null,
        CancellationToken cancellationToken = default)
        => Orders.UpdateStatusAsync(storeId, orderId, status, note, cancellationToken);
}
