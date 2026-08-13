using Posty5.Core.Http;
using Posty5.Core.Models;
using Posty5.Store.Models;

namespace Posty5.Store.Clients;

/// <summary>
/// Catalogue tags and product assignments — <c>/api/store-tags</c>. Requires
/// <c>products.manage</c>. Tag operations are free and unmetered.
/// </summary>
/// <remarks>
/// Assignments live in their own collection rather than as an array on the
/// product, which is what lets an assignment carry its own expiry and lets a tag
/// list its products without scanning the catalogue.
/// </remarks>
public class StoreTagsClient : StoreClientBase
{
    private const string Base = "/api/store-tags";

    /// <summary>Creates a tags client.</summary>
    /// <param name="httpClient">HTTP client instance from Posty5.Core.</param>
    public StoreTagsClient(Posty5HttpClient httpClient) : base(httpClient) { }

    // ─── Tags ───────────────────────────────────────────────────────────────

    /// <summary>Search a store's tags.</summary>
    public async Task<PaginationResponse<StoreTag>?> SearchAsync(
        string storeId,
        TagSearchParams? filters = null,
        PaginationParams? pagination = null,
        CancellationToken cancellationToken = default)
    {
        var query = BuildFilterQuery(filters);
        AddPagination(query, pagination);

        var response = await Http.GetAsync<PaginationResponse<StoreTag>>($"{Base}/{storeId}", query, cancellationToken);
        return response.Result;
    }

    /// <summary>Get one tag.</summary>
    public async Task<StoreTag?> GetAsync(string storeId, string tagId, CancellationToken cancellationToken = default)
    {
        var response = await Http.GetAsync<StoreTag>($"{Base}/{storeId}/{tagId}", null, cancellationToken);
        return response.Result;
    }

    /// <summary>Create a tag.</summary>
    public async Task<StoreTag?> CreateAsync(string storeId, CreateTagInput tag, CancellationToken cancellationToken = default)
    {
        var response = await Http.PostAsync<StoreTag>($"{Base}/{storeId}", tag, cancellationToken);
        return response.Result;
    }

    /// <summary>Update a tag.</summary>
    public async Task<StoreTag?> UpdateAsync(string storeId, string tagId, UpdateTagInput changes, CancellationToken cancellationToken = default)
    {
        var response = await Http.PutAsync<StoreTag>($"{Base}/{storeId}/{tagId}", changes, cancellationToken);
        return response.Result;
    }

    /// <summary>Soft-delete a tag and drop its assignments. The products are untouched.</summary>
    public async Task<object?> DeleteAsync(string storeId, string tagId, CancellationToken cancellationToken = default)
    {
        var response = await Http.DeleteAsync<object>($"{Base}/{storeId}/{tagId}", cancellationToken);
        return response.Result;
    }

    // ─── Assignments ────────────────────────────────────────────────────────

    /// <summary>
    /// The products carrying any of the given tags, de-duplicated — what a
    /// tag-driven storefront section reads.
    /// </summary>
    public async Task<object?> ResolveProductsAsync(
        string storeId,
        IEnumerable<string> tagIds,
        int? limit = null,
        CancellationToken cancellationToken = default)
    {
        var query = Query();
        Add(query, "tagIds", tagIds.ToList());
        Add(query, "limit", limit);

        var response = await Http.GetAsync<object>($"{Base}/{storeId}/resolve", query, cancellationToken);
        return response.Result;
    }

    /// <summary>The products assigned to one tag.</summary>
    public async Task<PaginationResponse<ProductSummary>?> ListProductsAsync(
        string storeId,
        string tagId,
        string? search = null,
        PaginationParams? pagination = null,
        CancellationToken cancellationToken = default)
    {
        var query = Query();
        Add(query, "search", search);
        AddPagination(query, pagination);

        var response = await Http.GetAsync<PaginationResponse<ProductSummary>>($"{Base}/{storeId}/{tagId}/products", query, cancellationToken);
        return response.Result;
    }

    /// <summary>Add up to 200 products to a tag. Products already carrying it are left alone.</summary>
    public async Task<object?> AssignProductsAsync(string storeId, string tagId, IEnumerable<string> productIds, CancellationToken cancellationToken = default)
    {
        var body = new AssignProductsRequest { ProductIds = productIds.ToList() };
        var response = await Http.PostAsync<object>($"{Base}/{storeId}/{tagId}/products", body, cancellationToken);
        return response.Result;
    }

    /// <summary>Remove one product from a tag.</summary>
    public async Task<object?> UnassignProductAsync(string storeId, string tagId, string productId, CancellationToken cancellationToken = default)
    {
        var response = await Http.DeleteAsync<object>($"{Base}/{storeId}/{tagId}/products/{productId}", cancellationToken);
        return response.Result;
    }

    /// <summary>The tags carried by one product.</summary>
    public async Task<List<StoreTag>?> GetProductTagsAsync(string storeId, string productId, CancellationToken cancellationToken = default)
    {
        var response = await Http.GetAsync<List<StoreTag>>($"{Base}/{storeId}/product/{productId}", null, cancellationToken);
        return response.Result;
    }

    /// <summary>Replace one product's whole tag list — tags left out are unassigned.</summary>
    public async Task<object?> SetProductTagsAsync(string storeId, string productId, IEnumerable<string> tagIds, CancellationToken cancellationToken = default)
    {
        var body = new SetProductTagsRequest { TagIds = tagIds.ToList() };
        var response = await Http.PutAsync<object>($"{Base}/{storeId}/product/{productId}", body, cancellationToken);
        return response.Result;
    }

    // ─── Excel ──────────────────────────────────────────────────────────────

    /// <summary>Download the .xlsx import template.</summary>
    public Task<FileResponse> DownloadImportTemplateAsync(string storeId, CancellationToken cancellationToken = default)
        => Http.GetBytesAsync($"{Base}/{storeId}/import/template", null, cancellationToken);

    /// <summary>Bulk-create tags from a filled-in template, base64-encoded in the body.</summary>
    public async Task<BulkImportReport?> ImportFromExcelAsync(string storeId, ExcelUploadInput file, CancellationToken cancellationToken = default)
    {
        var response = await Http.PostAsync<BulkImportReport>($"{Base}/{storeId}/import", file, cancellationToken);
        return response.Result;
    }

    /// <summary>Download the filtered tag set as .xlsx — the same filters as the search.</summary>
    public Task<FileResponse> ExportToExcelAsync(string storeId, TagSearchParams? filters = null, CancellationToken cancellationToken = default)
        => Http.GetBytesAsync($"{Base}/{storeId}/export", BuildFilterQuery(filters), cancellationToken);

    private static Dictionary<string, object?> BuildFilterQuery(TagSearchParams? filters)
    {
        var query = Query();
        if (filters == null) return query;

        Add(query, "name", filters.Name);
        Add(query, "slug", filters.Slug);
        Add(query, "status", filters.Status);
        Add(query, "hasAutoRemoval", filters.HasAutoRemoval);
        Add(query, "fromDate", filters.FromDate);
        Add(query, "toDate", filters.ToDate);

        return query;
    }
}
