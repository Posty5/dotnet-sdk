using Posty5.Core.Http;
using Posty5.Core.Models;
using Posty5.Store.Models;

namespace Posty5.Store;

/// <summary>
/// Client for managing an Online Store (products and orders) via the Posty5 API.
/// Authenticate with an API key on <see cref="Posty5HttpClient"/> (sent as the
/// <c>X-API-Key</c> header); the key owner must hold the matching store
/// permission (products.manage, orders.view / orders.create / orders.updateStatus).
/// </summary>
public class StoreClient
{
    private readonly Posty5HttpClient _http;
    private const string ProductsBase = "/api/store-products";
    private const string OrdersBase = "/api/store-orders";

    /// <summary>Creates a new Store client.</summary>
    /// <param name="httpClient">HTTP client instance from Posty5.Core.</param>
    public StoreClient(Posty5HttpClient httpClient)
    {
        _http = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    }

    // ─── Products ─────────────────────────────────────────────────────────────

    /// <summary>Create many products in one call (charges addProduct per created row).</summary>
    public async Task<BulkProductsReport?> BulkCreateProductsAsync(
        string storeId,
        IEnumerable<BulkProductInput> products,
        CancellationToken cancellationToken = default)
    {
        var body = new BulkProductsRequest { Products = products.ToList() };
        var response = await _http.PostAsync<BulkProductsReport>($"{ProductsBase}/{storeId}/bulk", body, cancellationToken);
        return response.Result;
    }

    // ─── Orders ───────────────────────────────────────────────────────────────

    /// <summary>Search a store's orders with optional filters and cursor pagination.</summary>
    public async Task<PaginationResponse<StoreOrderSummary>?> SearchOrdersAsync(
        string storeId,
        OrderSearchParams? filters = null,
        PaginationParams? pagination = null,
        CancellationToken cancellationToken = default)
    {
        var query = new Dictionary<string, object?>();
        if (filters != null)
        {
            if (!string.IsNullOrEmpty(filters.Status)) query["status"] = filters.Status;
            if (!string.IsNullOrEmpty(filters.OrderSource)) query["orderSource"] = filters.OrderSource;
            if (!string.IsNullOrEmpty(filters.OrderNumber)) query["orderNumber"] = filters.OrderNumber;
            if (!string.IsNullOrEmpty(filters.FromDate)) query["fromDate"] = filters.FromDate;
            if (!string.IsNullOrEmpty(filters.ToDate)) query["toDate"] = filters.ToDate;
        }
        if (pagination != null)
        {
            if (!string.IsNullOrEmpty(pagination.Cursor)) query["cursor"] = pagination.Cursor;
            query["pageSize"] = pagination.PageSize;
        }

        var response = await _http.GetAsync<PaginationResponse<StoreOrderSummary>>($"{OrdersBase}/{storeId}", query, cancellationToken);
        return response.Result;
    }

    /// <summary>Create an order manually (tagged createdFrom: "dotnet"). Charges the deferred manualOrder op.</summary>
    public async Task<StoreOrder?> CreateOrderAsync(
        string storeId,
        CreateOrderInput order,
        CancellationToken cancellationToken = default)
    {
        var response = await _http.PostAsync<StoreOrder>($"{OrdersBase}/{storeId}", order, cancellationToken);
        return response.Result;
    }

    /// <summary>Change an order's status (respects the store status workflow; charges orderStatusChange).</summary>
    public async Task<StoreOrder?> UpdateOrderStatusAsync(
        string storeId,
        string orderId,
        string status,
        string? note = null,
        CancellationToken cancellationToken = default)
    {
        var body = new ChangeStatusRequest { Status = status, Note = note ?? string.Empty };
        var response = await _http.PostAsync<StoreOrder>($"{OrdersBase}/{storeId}/{orderId}/status", body, cancellationToken);
        return response.Result;
    }
}
