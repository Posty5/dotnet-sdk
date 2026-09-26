using Posty5.Core.Http;
using Posty5.Core.Models;
using Posty5.Store.Models;

namespace Posty5.Store.Clients;

/// <summary>
/// Dropshipping — <c>/api/store-suppliers</c>: connect suppliers, import and link
/// their products, and act on the orders sent to them.
/// </summary>
/// <remarks>
/// <para>
/// Permissions: <c>suppliers.view</c> to read, <c>suppliers.manage</c> to connect
/// and configure, <c>suppliers.import</c> to browse, import and link,
/// <c>suppliers.orders.manage</c> to send, pay, retry, cancel or take over a part.
/// Paying a supplier spends the merchant's own supplier balance — treat a key
/// holding <c>suppliers.orders.manage</c> accordingly.
/// </para>
/// <para>
/// Connecting, importing, sending and paying are refused when the store owner's
/// plan does not include dropshipping (Pro and above); reads, cancel and
/// fulfil-manually are not. Importing a product costs the same credits as adding
/// one; nothing else here is charged.
/// </para>
/// <para>
/// <b>Paused outcomes throw.</b> <see cref="SubmitGroupAsync"/>,
/// <see cref="RetryAsync"/> and <see cref="PayAsync"/> answer a pause
/// (<c>needsReview</c>, <c>failed</c>, <c>skipped</c>, or a part already being
/// sent) as an HTTP 400 naming the reason, so they throw
/// <c>Posty5ValidationException</c>. Read the supplier order again to see where it stands.
/// </para>
/// </remarks>
public class StoreSuppliersClient : StoreClientBase
{
    private const string Base = "/api/store-suppliers";

    /// <summary>Creates a suppliers client.</summary>
    /// <param name="httpClient">HTTP client instance from Posty5.Core.</param>
    public StoreSuppliersClient(Posty5HttpClient httpClient) : base(httpClient) { }

    // ─── Catalogue and connections ──────────────────────────────────────────

    /// <summary>The suppliers this store can connect, with what each can do. <c>suppliers.view</c>.</summary>
    public async Task<StoreSupplierCatalogue?> GetCatalogueAsync(string storeId, CancellationToken cancellationToken = default)
    {
        var response = await Http.GetAsync<StoreSupplierCatalogue>($"{Base}/{storeId}/catalogue", null, cancellationToken);
        return response.Result;
    }

    /// <summary>The store's supplier connections. Credentials are never returned. <c>suppliers.view</c>.</summary>
    public async Task<List<StoreSupplierIntegration>> ListAsync(string storeId, CancellationToken cancellationToken = default)
    {
        var response = await Http.GetAsync<SupplierIntegrationList>($"{Base}/{storeId}", null, cancellationToken);
        return response.Result?.Items ?? new List<StoreSupplierIntegration>();
    }

    /// <summary>
    /// Connect a supplier. Read <see cref="GetCatalogueAsync"/> first: the credential
    /// keys come from the entry's <c>CredentialFields</c>. The key is checked with
    /// the supplier before it is stored, encrypted. Starts in <c>test</c> mode unless
    /// <c>Mode</c> says otherwise. <c>suppliers.manage</c>, plan gate.
    /// </summary>
    public async Task<StoreSupplierIntegration?> ConnectAsync(string storeId, ConnectSupplierInput input, CancellationToken cancellationToken = default)
    {
        var response = await Http.PostAsync<StoreSupplierIntegration>($"{Base}/{storeId}", input, cancellationToken);
        return response.Result;
    }

    /// <summary>
    /// Start connecting a supplier that uses sign-in instead of a key (AliExpress).
    /// Send the merchant to <c>AuthorizeUrl</c>. <c>suppliers.manage</c>, plan gate.
    /// </summary>
    public async Task<SupplierOAuthStartResult?> StartOAuthAsync(string storeId, StartSupplierOAuthInput input, CancellationToken cancellationToken = default)
    {
        var response = await Http.PostAsync<SupplierOAuthStartResult>($"{Base}/{storeId}/oauth/start", input, cancellationToken);
        return response.Result;
    }

    /// <summary>Replace a connection's credentials. <c>suppliers.manage</c>, plan gate.</summary>
    public async Task<StoreSupplierIntegration?> ReplaceCredentialsAsync(string storeId, string id, ReplaceSupplierCredentialsInput input, CancellationToken cancellationToken = default)
    {
        var response = await Http.PutAsync<StoreSupplierIntegration>($"{Base}/{storeId}/{id}", input, cancellationToken);
        return response.Result;
    }

    /// <summary>Change the supplier-specific settings (and optionally the mode). <c>suppliers.manage</c>.</summary>
    public async Task<StoreSupplierIntegration?> UpdateSettingsAsync(string storeId, string id, UpdateSupplierSettingsInput input, CancellationToken cancellationToken = default)
    {
        var response = await Http.PutAsync<StoreSupplierIntegration>($"{Base}/{storeId}/{id}/settings", input, cancellationToken);
        return response.Result;
    }

    /// <summary>
    /// Change what the connection may do on its own. <c>submitAndPay</c> is refused
    /// for a supplier that cannot be paid from a balance. <c>suppliers.manage</c>, plan gate.
    /// </summary>
    public async Task<StoreSupplierIntegration?> UpdateAutomationAsync(string storeId, string id, SupplierAutomationInput automation, CancellationToken cancellationToken = default)
    {
        var response = await Http.PutAsync<StoreSupplierIntegration>($"{Base}/{storeId}/{id}/automation", automation, cancellationToken);
        return response.Result;
    }

    /// <summary>Switch the connection on or off. <c>suppliers.manage</c>, plan gate.</summary>
    public async Task<StoreSupplierIntegration?> SetEnabledAsync(string storeId, string id, bool enabled, CancellationToken cancellationToken = default)
    {
        var response = await Http.PutAsync<StoreSupplierIntegration>($"{Base}/{storeId}/{id}/enabled", new SetSupplierEnabledRequest { Enabled = enabled }, cancellationToken);
        return response.Result;
    }

    /// <summary>Check the connection now and record its health. <c>suppliers.manage</c>.</summary>
    public async Task<SupplierTestResult?> TestAsync(string storeId, string id, CancellationToken cancellationToken = default)
    {
        var response = await Http.PostAsync<SupplierTestResult>($"{Base}/{storeId}/{id}/test", new { }, cancellationToken);
        return response.Result;
    }

    /// <summary>The balance at the supplier, where the supplier reports one. <c>suppliers.view</c>.</summary>
    public async Task<SupplierBalance?> GetBalanceAsync(string storeId, string id, CancellationToken cancellationToken = default)
    {
        var response = await Http.GetAsync<SupplierBalance>($"{Base}/{storeId}/{id}/balance", null, cancellationToken);
        return response.Result;
    }

    /// <summary>What disconnecting would affect. <c>suppliers.view</c>.</summary>
    public async Task<SupplierDisconnectImpact?> GetDisconnectImpactAsync(string storeId, string id, CancellationToken cancellationToken = default)
    {
        var response = await Http.GetAsync<SupplierDisconnectImpact>($"{Base}/{storeId}/{id}/impact", null, cancellationToken);
        return response.Result;
    }

    /// <summary>
    /// Disconnect. Credentials are deleted; imported products stay as the store's
    /// own. Refused while supplier orders are open unless <paramref name="force"/>.
    /// <c>suppliers.manage</c>.
    /// </summary>
    public async Task<DisconnectSupplierResult?> DisconnectAsync(string storeId, string id, bool force = false, CancellationToken cancellationToken = default)
    {
        // DeleteAsync takes no query dictionary; the one flag rides on the path.
        var path = force ? $"{Base}/{storeId}/{id}?force=true" : $"{Base}/{storeId}/{id}";
        var response = await Http.DeleteAsync<DisconnectSupplierResult>(path, cancellationToken);
        return response.Result;
    }

    // ─── Supplier products and imports ──────────────────────────────────────

    /// <summary>Browse or search the supplier's catalogue. Paged by number. <c>suppliers.import</c>, plan gate.</summary>
    public async Task<SupplierProductPage?> BrowseProductsAsync(
        string storeId,
        string id,
        BrowseSupplierProductsParams? filters = null,
        int? page = null,
        int? pageSize = null,
        CancellationToken cancellationToken = default)
    {
        var query = Query();
        Add(query, "q", filters?.Q);
        Add(query, "categoryId", filters?.CategoryId);
        Add(query, "page", page);
        Add(query, "pageSize", pageSize);

        var response = await Http.GetAsync<SupplierProductPage>($"{Base}/{storeId}/{id}/products", query, cancellationToken);
        return response.Result;
    }

    /// <summary>One supplier product with its variants, costs and stock. <c>suppliers.import</c>, plan gate.</summary>
    public async Task<SupplierProduct?> GetProductAsync(string storeId, string id, string supplierProductId, CancellationToken cancellationToken = default)
    {
        var response = await Http.GetAsync<SupplierProduct>($"{Base}/{storeId}/{id}/products/{Uri.EscapeDataString(supplierProductId)}", null, cancellationToken);
        return response.Result;
    }

    /// <summary>Resolve a pasted product link to the supplier product. Short links are refused. <c>suppliers.import</c>, plan gate.</summary>
    public async Task<SupplierProduct?> ResolveUrlAsync(string storeId, string id, string url, CancellationToken cancellationToken = default)
    {
        var response = await Http.PostAsync<SupplierProduct>($"{Base}/{storeId}/{id}/products/resolve-url", new ResolveSupplierUrlRequest { Url = url }, cancellationToken);
        return response.Result;
    }

    /// <summary>Price the chosen products and name duplicates. Nothing is created or charged. <c>suppliers.import</c>, plan gate.</summary>
    public async Task<SupplierImportPreview?> PreviewImportAsync(string storeId, string id, ImportSupplierProductsInput input, CancellationToken cancellationToken = default)
    {
        var response = await Http.PostAsync<SupplierImportPreview>($"{Base}/{storeId}/{id}/import/preview", input, cancellationToken);
        return response.Result;
    }

    /// <summary>
    /// Import up to 50 products. Small imports answer the rows; large ones answer a
    /// job id — check <c>IsQueued</c> and poll <see cref="GetImportStatusAsync"/>.
    /// Charged like adding products; the whole batch is refused if the credits
    /// cannot cover it. <c>suppliers.import</c>, plan gate.
    /// </summary>
    public async Task<ImportSupplierProductsResult?> ImportProductsAsync(string storeId, string id, ImportSupplierProductsInput input, CancellationToken cancellationToken = default)
    {
        var response = await Http.PostAsync<ImportSupplierProductsResult>($"{Base}/{storeId}/{id}/import", input, cancellationToken);
        return response.Result;
    }

    /// <summary>A background import's progress, and its rows once completed. <c>suppliers.import</c>.</summary>
    public async Task<SupplierImportJobStatus?> GetImportStatusAsync(string storeId, string jobId, CancellationToken cancellationToken = default)
    {
        var response = await Http.GetAsync<SupplierImportJobStatus>($"{Base}/{storeId}/imports/{Uri.EscapeDataString(jobId)}", null, cancellationToken);
        return response.Result;
    }

    // ─── Product links ──────────────────────────────────────────────────────

    /// <summary>The store's product links, optionally for one product. <c>suppliers.view</c>.</summary>
    public async Task<List<StoreProductSupplierLink>> ListLinksAsync(string storeId, string? productId = null, CancellationToken cancellationToken = default)
    {
        var query = Query();
        Add(query, "productId", productId);

        var response = await Http.GetAsync<SupplierLinkList>($"{Base}/{storeId}/links", query, cancellationToken);
        return response.Result?.Items ?? new List<StoreProductSupplierLink>();
    }

    /// <summary>
    /// Link a product the store already sells to a supplier product. Changes who
    /// ships it; never its price, images or description. <c>suppliers.import</c>, plan gate.
    /// </summary>
    public async Task<StoreProductSupplierLink?> CreateLinkAsync(string storeId, CreateSupplierLinkInput input, CancellationToken cancellationToken = default)
    {
        var response = await Http.PostAsync<StoreProductSupplierLink>($"{Base}/{storeId}/links", input, cancellationToken);
        return response.Result;
    }

    /// <summary>Change a link's price rule, sync switches, estimate or disclosure. <c>suppliers.import</c>.</summary>
    public async Task<StoreProductSupplierLink?> UpdateLinkAsync(string storeId, string linkId, UpdateSupplierLinkInput changes, CancellationToken cancellationToken = default)
    {
        var response = await Http.PutAsync<StoreProductSupplierLink>($"{Base}/{storeId}/links/{linkId}", changes, cancellationToken);
        return response.Result;
    }

    /// <summary>Unlink. The product stays and becomes the store's own. <c>suppliers.import</c>.</summary>
    public async Task<DeletedSupplierLink?> DeleteLinkAsync(string storeId, string linkId, CancellationToken cancellationToken = default)
    {
        var response = await Http.DeleteAsync<DeletedSupplierLink>($"{Base}/{storeId}/links/{linkId}", cancellationToken);
        return response.Result;
    }

    /// <summary>Sync one link now. Refused within a minute of the last sync. <c>suppliers.import</c>.</summary>
    public async Task<SupplierLinkSyncResult?> SyncLinkAsync(string storeId, string linkId, CancellationToken cancellationToken = default)
    {
        var response = await Http.PostAsync<SupplierLinkSyncResult>($"{Base}/{storeId}/links/{linkId}/sync", new { }, cancellationToken);
        return response.Result;
    }

    // ─── Supplier orders ────────────────────────────────────────────────────

    /// <summary>
    /// Supplier orders, newest first, paged by cursor like every other store list.
    /// <c>suppliers.view</c>.
    /// </summary>
    /// <param name="storeId">The store.</param>
    /// <param name="filters">Status, needs-review, connection, store order and contract-model filters.</param>
    /// <param name="pagination">
    /// <c>Cursor</c> is the previous page's <c>Pagination.NextCursor</c>; <c>PageSize</c> is capped
    /// at 100 by the API. Omit it for the first page at the API's default of 25 rows.
    /// </param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>
    /// The list envelope: <c>Items</c> and <c>Pagination</c>. Keep passing
    /// <c>Pagination.NextCursor</c> back while <c>Pagination.HasMore</c> is true.
    /// </returns>
    public async Task<PaginationResponse<StoreSupplierOrder>?> ListSupplierOrdersAsync(
        string storeId,
        SupplierOrderSearchParams? filters = null,
        PaginationParams? pagination = null,
        CancellationToken cancellationToken = default)
    {
        var query = Query();
        Add(query, "status", filters?.Status);
        Add(query, "needsReview", filters?.NeedsReview);
        Add(query, "integrationId", filters?.IntegrationId);
        Add(query, "orderId", filters?.OrderId);
        Add(query, "contractModel", filters?.ContractModel);
        AddPagination(query, pagination);

        var response = await Http.GetAsync<PaginationResponse<StoreSupplierOrder>>($"{Base}/{storeId}/orders", query, cancellationToken);
        return response.Result;
    }

    /// <summary>
    /// One supplier order with its history. <c>Destination</c> is present only for a
    /// caller holding <c>orders.customerData.view</c>. <c>suppliers.view</c>.
    /// </summary>
    public async Task<StoreSupplierOrder?> GetSupplierOrderAsync(string storeId, string supplierOrderId, CancellationToken cancellationToken = default)
    {
        var response = await Http.GetAsync<StoreSupplierOrder>($"{Base}/{storeId}/orders/{supplierOrderId}", null, cancellationToken);
        return response.Result;
    }

    /// <summary>
    /// Send one part of an order to its supplier now. Calling it twice finds the
    /// first supplier order rather than creating a second. <paramref name="payNow"/>
    /// pays the supplier even when the shopper has not paid — recorded with the
    /// caller. <paramref name="groupKey"/> is the part's key (<c>supplier:{integrationId}</c>).
    /// <c>suppliers.orders.manage</c>, plan gate. Throws on a pause.
    /// </summary>
    public async Task<SupplierOrderActionResult?> SubmitGroupAsync(
        string storeId,
        string orderId,
        string groupKey,
        bool? payNow = null,
        CancellationToken cancellationToken = default)
    {
        var response = await Http.PostAsync<SupplierOrderActionResult>(
            $"{Base}/{storeId}/orders/{orderId}/groups/{Uri.EscapeDataString(groupKey)}/submit",
            new SubmitGroupRequest { PayNow = payNow },
            cancellationToken);
        return response.Result;
    }

    /// <summary>
    /// Try a queued, paused or failed supplier order again. The pre-flight runs
    /// again; a <c>needsReview</c> order usually needs something changed first.
    /// <paramref name="acceptCost"/> accepts the supplier's new price — recorded with
    /// the caller. <c>suppliers.orders.manage</c>, plan gate. Throws on a pause.
    /// </summary>
    public async Task<SupplierOrderActionResult?> RetryAsync(
        string storeId,
        string supplierOrderId,
        bool? acceptCost = null,
        CancellationToken cancellationToken = default)
    {
        var response = await Http.PostAsync<SupplierOrderActionResult>(
            $"{Base}/{storeId}/orders/{supplierOrderId}/retry",
            new RetrySupplierOrderRequest { AcceptCost = acceptCost },
            cancellationToken);
        return response.Result;
    }

    /// <summary>
    /// Pay a supplier order that was created but not paid. The supplier's own
    /// status is read first, so an order already paid there is recorded, not paid
    /// again. <c>suppliers.orders.manage</c>, plan gate. Throws on a pause.
    /// </summary>
    public async Task<SupplierOrderActionResult?> PayAsync(string storeId, string supplierOrderId, CancellationToken cancellationToken = default)
    {
        var response = await Http.PostAsync<SupplierOrderActionResult>($"{Base}/{storeId}/orders/{supplierOrderId}/pay", new { }, cancellationToken);
        return response.Result;
    }

    /// <summary>
    /// Withdraw the order at the supplier, where the supplier still allows it. Once
    /// shipped it cannot be withdrawn and the call throws. Open below Pro.
    /// <c>suppliers.orders.manage</c>.
    /// </summary>
    public async Task<SupplierOrderActionResult?> CancelAsync(string storeId, string supplierOrderId, CancellationToken cancellationToken = default)
    {
        var response = await Http.PostAsync<SupplierOrderActionResult>($"{Base}/{storeId}/orders/{supplierOrderId}/cancel", new { }, cancellationToken);
        return response.Result;
    }

    /// <summary>Take a part over: the store ships it itself. Open below Pro. <c>suppliers.orders.manage</c>.</summary>
    public async Task<FulfilGroupManuallyResult?> FulfilGroupManuallyAsync(string storeId, string orderId, string groupKey, CancellationToken cancellationToken = default)
    {
        var response = await Http.PostAsync<FulfilGroupManuallyResult>(
            $"{Base}/{storeId}/orders/{orderId}/groups/{Uri.EscapeDataString(groupKey)}/fulfil-manually",
            new { },
            cancellationToken);
        return response.Result;
    }
}
