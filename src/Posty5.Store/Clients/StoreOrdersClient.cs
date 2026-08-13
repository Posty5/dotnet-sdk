using Posty5.Core.Http;
using Posty5.Core.Models;
using Posty5.Store.Models;

namespace Posty5.Store.Clients;

/// <summary>
/// Merchant order management — <c>/api/store-orders</c>.
/// </summary>
/// <remarks>
/// Reads need <c>orders.view</c>, <see cref="CreateAsync"/> needs
/// <c>orders.create</c> and <see cref="UpdateStatusAsync"/> needs
/// <c>orders.updateStatus</c>; the store owner holds all three.
/// </remarks>
public class StoreOrdersClient : StoreClientBase
{
    private const string Base = "/api/store-orders";

    /// <summary>Creates an orders client.</summary>
    /// <param name="httpClient">HTTP client instance from Posty5.Core.</param>
    public StoreOrdersClient(Posty5HttpClient httpClient) : base(httpClient) { }

    // ─── Read ───────────────────────────────────────────────────────────────

    /// <summary>Search a store's orders.</summary>
    public async Task<PaginationResponse<StoreOrderSummary>?> SearchAsync(
        string storeId,
        OrderSearchParams? filters = null,
        PaginationParams? pagination = null,
        CancellationToken cancellationToken = default)
    {
        var query = BuildFilterQuery(filters);
        AddPagination(query, pagination);

        var response = await Http.GetAsync<PaginationResponse<StoreOrderSummary>>($"{Base}/{storeId}", query, cancellationToken);
        return response.Result;
    }

    /// <summary>One order in full: items, customer, totals, status history and notes.</summary>
    public async Task<StoreOrder?> GetAsync(string storeId, string orderId, CancellationToken cancellationToken = default)
    {
        var response = await Http.GetAsync<StoreOrder>($"{Base}/{storeId}/{orderId}", null, cancellationToken);
        return response.Result;
    }

    /// <summary>Totals, per-status breakdown, delivered revenue and a per-day series.</summary>
    public async Task<OrderStatistics?> StatisticsAsync(
        string storeId,
        OrderStatisticsParams? filters = null,
        CancellationToken cancellationToken = default)
    {
        var query = BuildFilterQuery(filters);
        Add(query, "days", filters?.Days);

        var response = await Http.GetAsync<OrderStatistics>($"{Base}/{storeId}/statistics", query, cancellationToken);
        return response.Result;
    }

    /// <summary>
    /// Full details of every order matching the filters, in one payload — what a
    /// packing-slip or invoice run reads. Charges nothing.
    /// </summary>
    public async Task<object?> PrintDataAsync(string storeId, OrderSearchParams? filters = null, CancellationToken cancellationToken = default)
    {
        var response = await Http.GetAsync<object>($"{Base}/{storeId}/print", BuildFilterQuery(filters), cancellationToken);
        return response.Result;
    }

    /// <summary>
    /// Download the filtered order set as .xlsx. Unlike the deferred order
    /// operations this charges exportOrders BEFORE the file is built, so an
    /// unaffordable export is refused rather than delivered free.
    /// </summary>
    public Task<FileResponse> ExportToExcelAsync(string storeId, OrderSearchParams? filters = null, CancellationToken cancellationToken = default)
        => Http.GetBytesAsync($"{Base}/{storeId}/export", BuildFilterQuery(filters), cancellationToken);

    // ─── Write ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Record an order received off-store. Runs the same pricing, stock,
    /// numbering and tracking machinery as a real checkout, tagged
    /// <c>createdFrom: "dotnet"</c>. Charges the deferred manualOrder op.
    /// Shipping is resolved server-side from the destination — never send a fee.
    /// </summary>
    public async Task<StoreOrder?> CreateAsync(string storeId, CreateOrderInput order, CancellationToken cancellationToken = default)
    {
        var response = await Http.PostAsync<StoreOrder>($"{Base}/{storeId}", order, cancellationToken);
        return response.Result;
    }

    /// <summary>
    /// Move an order to a new status. The workflow is enforced: pending →
    /// confirmed → processing → shipped → delivered, with cancelled/refused
    /// reachable from any non-terminal state and the terminal states accepting
    /// nothing further. Charges the deferred orderStatusChange. The note is
    /// customer-facing — it reaches the status event and the notification email.
    /// </summary>
    public async Task<StoreOrder?> UpdateStatusAsync(
        string storeId,
        string orderId,
        string status,
        string? note = null,
        CancellationToken cancellationToken = default)
    {
        var body = new ChangeStatusRequest { Status = status, Note = note ?? string.Empty };
        var response = await Http.PostAsync<StoreOrder>($"{Base}/{storeId}/{orderId}/status", body, cancellationToken);
        return response.Result;
    }

    /// <summary>Attach a staff-only note. Charges nothing and sends no email.</summary>
    public async Task<StoreOrder?> AddInternalNoteAsync(string storeId, string orderId, string note, CancellationToken cancellationToken = default)
    {
        var body = new InternalNoteRequest { Note = note };
        var response = await Http.PostAsync<StoreOrder>($"{Base}/{storeId}/{orderId}/notes", body, cancellationToken);
        return response.Result;
    }

    private static Dictionary<string, object?> BuildFilterQuery(OrderSearchParams? filters)
    {
        var query = Query();
        if (filters == null) return query;

        Add(query, "status", filters.Status);
        Add(query, "orderSource", filters.OrderSource);
        Add(query, "createdFrom", filters.CreatedFrom);
        Add(query, "orderNumber", filters.OrderNumber);
        Add(query, "publicTrackingId", filters.PublicTrackingId);
        Add(query, "customer", filters.Customer);
        Add(query, "productName", filters.ProductName);
        Add(query, "productId", filters.ProductId);
        Add(query, "tagIds", filters.TagIds);
        Add(query, "fromDate", filters.FromDate);
        Add(query, "toDate", filters.ToDate);

        return query;
    }
}
