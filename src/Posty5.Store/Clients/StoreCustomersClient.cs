using Posty5.Core.Http;
using Posty5.Core.Models;
using Posty5.Store.Models;

namespace Posty5.Store.Clients;

/// <summary>
/// The merchant's view of the people who have ordered from their store —
/// <c>/api/store-customers</c>. Requires <c>orders.view</c>.
/// </summary>
/// <remarks>
/// Read-only by design: a customer record is derived from orders and, when the
/// shopper has a Posty5 account, owned by them.
/// <para>
/// Every method is scoped to the store. A customer who has never ordered here
/// answers 404, so a guessed id cannot be told apart from a customer of another
/// store.
/// </para>
/// </remarks>
public class StoreCustomersClient : StoreClientBase
{
    private const string Base = "/api/store-customers";

    /// <summary>Creates a customers client.</summary>
    /// <param name="httpClient">HTTP client instance from Posty5.Core.</param>
    public StoreCustomersClient(Posty5HttpClient httpClient) : base(httpClient) { }

    /// <summary>Search the store's customers, with per-store order counts and spend.</summary>
    public async Task<PaginationResponse<StoreCustomer>?> SearchAsync(
        string storeId,
        CustomerSearchParams? filters = null,
        PaginationParams? pagination = null,
        CancellationToken cancellationToken = default)
    {
        var query = Query();
        if (filters != null)
        {
            Add(query, "text", filters.Text);
            Add(query, "hasAccount", filters.HasAccount);
        }
        AddPagination(query, pagination);

        var response = await Http.GetAsync<PaginationResponse<StoreCustomer>>($"{Base}/{storeId}", query, cancellationToken);
        return response.Result;
    }

    /// <summary>One customer's profile and their figures for this store.</summary>
    public async Task<StoreCustomer?> GetAsync(string storeId, string customerId, CancellationToken cancellationToken = default)
    {
        var response = await Http.GetAsync<StoreCustomer>($"{Base}/{storeId}/{customerId}", null, cancellationToken);
        return response.Result;
    }

    /// <summary>Only the addresses this customer has used with this store.</summary>
    public async Task<List<StoreCustomerAddress>?> AddressesAsync(string storeId, string customerId, CancellationToken cancellationToken = default)
    {
        var response = await Http.GetAsync<List<StoreCustomerAddress>>($"{Base}/{storeId}/{customerId}/addresses", null, cancellationToken);
        return response.Result;
    }

    /// <summary>This store's orders for one customer — never orders placed elsewhere.</summary>
    public async Task<PaginationResponse<StoreOrderSummary>?> OrdersAsync(
        string storeId,
        string customerId,
        PaginationParams? pagination = null,
        CancellationToken cancellationToken = default)
    {
        var query = Query();
        AddPagination(query, pagination);

        var response = await Http.GetAsync<PaginationResponse<StoreOrderSummary>>($"{Base}/{storeId}/{customerId}/orders", query, cancellationToken);
        return response.Result;
    }
}
