using Posty5.Core.Http;
using Posty5.Core.Models;
using Posty5.Store.Models;

namespace Posty5.Store.Clients;

/// <summary>
/// The store catalogue — <c>/api/store-products</c>. Requires the
/// <c>products.manage</c> store permission.
/// </summary>
/// <remarks>
/// Two ways to write a product, on purpose. <c>CreateAsync</c>/<c>UpdateAsync</c>
/// take the whole document and suit an integration that owns the product
/// outright. The <c>Update…Async</c> section methods write only the part they
/// name, so a sync job that owns stock and a merchant editing the description in
/// the control panel never overwrite each other.
/// </remarks>
public class StoreProductsClient : StoreClientBase
{
    private const string Base = "/api/store-products";

    /// <summary>Creates a products client.</summary>
    /// <param name="httpClient">HTTP client instance from Posty5.Core.</param>
    public StoreProductsClient(Posty5HttpClient httpClient) : base(httpClient) { }

    // ─── Read ───────────────────────────────────────────────────────────────

    /// <summary>Search a store's products.</summary>
    public async Task<PaginationResponse<ProductSummary>?> SearchAsync(
        string storeId,
        ProductSearchParams? filters = null,
        PaginationParams? pagination = null,
        CancellationToken cancellationToken = default)
    {
        var query = Query();
        if (filters != null)
        {
            Add(query, "name", filters.Name);
            Add(query, "slug", filters.Slug);
            Add(query, "sku", filters.Sku);
            Add(query, "status", filters.Status);
            Add(query, "tagIds", filters.TagIds);
            Add(query, "excludeTagIds", filters.ExcludeTagIds);
        }
        AddPagination(query, pagination);

        var response = await Http.GetAsync<PaginationResponse<ProductSummary>>($"{Base}/{storeId}", query, cancellationToken);
        return response.Result;
    }

    /// <summary>Get one product in full — variants, landing sections and purchase config included.</summary>
    public async Task<StoreProduct?> GetAsync(string storeId, string productId, CancellationToken cancellationToken = default)
    {
        var response = await Http.GetAsync<StoreProduct>($"{Base}/{storeId}/{productId}", null, cancellationToken);
        return response.Result;
    }

    /// <summary>
    /// The landing-section vocabulary: every section key, its fields and their
    /// rules. Read this before calling <see cref="UpdateLandingAsync"/> or the AI
    /// methods — the section keys they accept come from here. Not store-scoped.
    /// </summary>
    public async Task<object?> GetLandingSectionsConfigAsync(CancellationToken cancellationToken = default)
    {
        var response = await Http.GetAsync<object>($"{Base}/config/landing-sections", null, cancellationToken);
        return response.Result;
    }

    // ─── Create ─────────────────────────────────────────────────────────────

    /// <summary>Create one product from a complete payload. Charges addProduct.</summary>
    public async Task<StoreProduct?> CreateAsync(string storeId, CreateProductInput product, CancellationToken cancellationToken = default)
    {
        var response = await Http.PostAsync<StoreProduct>($"{Base}/{storeId}", product, cancellationToken);
        return response.Result;
    }

    /// <summary>
    /// Create up to 200 products in one call. Charges addProduct per created row
    /// — the whole batch is checked for affordability first, so an unaffordable
    /// batch is refused before anything is written. A row that fails validation
    /// is reported and skipped; it does not abort the batch.
    /// </summary>
    public async Task<BulkProductsReport?> BulkCreateAsync(
        string storeId,
        IEnumerable<BulkProductInput> products,
        CancellationToken cancellationToken = default)
    {
        var body = new BulkProductsRequest { Products = products.ToList() };
        var response = await Http.PostAsync<BulkProductsReport>($"{Base}/{storeId}/bulk", body, cancellationToken);
        return response.Result;
    }

    /// <summary>
    /// Create a draft from just a SKU and a name, then fill it in through the
    /// section methods — so a client never has to hold one large unsaved form.
    /// </summary>
    public async Task<StoreProduct?> CreateDraftAsync(string storeId, CreateProductDraftInput draft, CancellationToken cancellationToken = default)
    {
        var response = await Http.PostAsync<StoreProduct>($"{Base}/{storeId}/draft", draft, cancellationToken);
        return response.Result;
    }

    /// <summary>
    /// Clone a product from a link on any storefront. The page is scraped and a
    /// draft is created with <c>purchase.mode: "external"</c> and one buy link
    /// back to the source. No SKU is accepted — that is the merchant's own
    /// article number.
    /// </summary>
    public async Task<StoreProduct?> CloneFromUrlAsync(string storeId, string url, CancellationToken cancellationToken = default)
    {
        var response = await Http.PostAsync<StoreProduct>($"{Base}/{storeId}/clone", new { url }, cancellationToken);
        return response.Result;
    }

    // ─── Excel ──────────────────────────────────────────────────────────────

    /// <summary>Download the .xlsx import template (header row plus one sample row).</summary>
    public Task<FileResponse> DownloadImportTemplateAsync(string storeId, CancellationToken cancellationToken = default)
        => Http.GetBytesAsync($"{Base}/{storeId}/import/template", null, cancellationToken);

    /// <summary>Bulk-create products from a filled-in template, base64-encoded in the body.</summary>
    public async Task<BulkImportReport?> ImportFromExcelAsync(string storeId, ExcelUploadInput file, CancellationToken cancellationToken = default)
    {
        var response = await Http.PostAsync<BulkImportReport>($"{Base}/{storeId}/import", file, cancellationToken);
        return response.Result;
    }

    // ─── Update / delete ────────────────────────────────────────────────────

    /// <summary>Replace any subset of the product's top-level fields.</summary>
    public async Task<StoreProduct?> UpdateAsync(string storeId, string productId, UpdateProductInput changes, CancellationToken cancellationToken = default)
    {
        var response = await Http.PutAsync<StoreProduct>($"{Base}/{storeId}/{productId}", changes, cancellationToken);
        return response.Result;
    }

    /// <summary>
    /// Soft-delete a product. It leaves the catalogue and the storefront; orders
    /// that reference it keep their snapshotted item rows.
    /// </summary>
    public async Task<object?> DeleteAsync(string storeId, string productId, CancellationToken cancellationToken = default)
    {
        var response = await Http.DeleteAsync<object>($"{Base}/{storeId}/{productId}", cancellationToken);
        return response.Result;
    }

    /// <summary>Set sortOrder on up to 500 products — the storefront display order.</summary>
    public async Task<object?> ReorderAsync(string storeId, IEnumerable<ProductSortOrderInput> items, CancellationToken cancellationToken = default)
    {
        var body = new ProductReorderRequest { Items = items.ToList() };
        var response = await Http.PutAsync<object>($"{Base}/{storeId}/reorder", body, cancellationToken);
        return response.Result;
    }

    // ─── Sections ───────────────────────────────────────────────────────────

    /// <summary>Name, SKU and description. The slug is on the SEO section, not here.</summary>
    public Task<StoreProduct?> UpdateBasicInformationAsync(string storeId, string productId, ProductBasicInformationInput input, CancellationToken cancellationToken = default)
        => PatchSectionAsync(storeId, productId, "basic-information", input, cancellationToken);

    /// <summary>The full ordered image list — index 0 is the primary image.</summary>
    public Task<StoreProduct?> UpdateMediaAsync(string storeId, string productId, ProductMediaInput input, CancellationToken cancellationToken = default)
        => PatchSectionAsync(storeId, productId, "media", input, cancellationToken);

    /// <summary>Selling price and the optional compare-at price.</summary>
    public Task<StoreProduct?> UpdatePriceAsync(string storeId, string productId, ProductPriceInput input, CancellationToken cancellationToken = default)
        => PatchSectionAsync(storeId, productId, "price", input, cancellationToken);

    /// <summary>Product-level stock. <c>null</c> means stock is not tracked.</summary>
    public Task<StoreProduct?> UpdateStockAsync(string storeId, string productId, ProductStockInput input, CancellationToken cancellationToken = default)
        => PatchSectionAsync(storeId, productId, "stock", input, cancellationToken);

    /// <summary>Variant groups and, optionally, the buyable stock combinations.</summary>
    public Task<StoreProduct?> UpdateVariantsAsync(string storeId, string productId, ProductVariantsInput input, CancellationToken cancellationToken = default)
        => PatchSectionAsync(storeId, productId, "variants", input, cancellationToken);

    /// <summary>Replace the product's whole tag list.</summary>
    public Task<StoreProduct?> UpdateTagsAsync(string storeId, string productId, ProductTagsInput input, CancellationToken cancellationToken = default)
        => PatchSectionAsync(storeId, productId, "tags", input, cancellationToken);

    /// <summary>Meta title, description, social image, index policy — and the slug.</summary>
    public Task<StoreProduct?> UpdateSeoAsync(string storeId, string productId, ProductSeoInput input, CancellationToken cancellationToken = default)
        => PatchSectionAsync(storeId, productId, "seo", input, cancellationToken);

    /// <summary>Publication status, featured flag, per-order limits and sort position.</summary>
    public Task<StoreProduct?> UpdateSettingsAsync(string storeId, string productId, ProductSettingsInput input, CancellationToken cancellationToken = default)
        => PatchSectionAsync(storeId, productId, "settings", input, cancellationToken);

    /// <summary>The landing sections: which are enabled, their order and each one's data.</summary>
    public Task<StoreProduct?> UpdateLandingAsync(string storeId, string productId, ProductLandingInput input, CancellationToken cancellationToken = default)
        => PatchSectionAsync(storeId, productId, "landing", input, cancellationToken);

    /// <summary>
    /// A delivery surcharge for this one product, always charged per unit —
    /// regardless of the store's shipping calculation mode, because a bulky item
    /// costs more to ship for every copy of it.
    /// </summary>
    public Task<StoreProduct?> UpdateShippingAsync(string storeId, string productId, ProductShippingInput input, CancellationToken cancellationToken = default)
        => PatchSectionAsync(storeId, productId, "shipping", input, cancellationToken);

    /// <summary>
    /// How the product is bought: through the cart, on another shop, or both.
    /// <c>external</c> and <c>both</c> need at least one link.
    /// </summary>
    public Task<StoreProduct?> UpdatePurchaseAsync(string storeId, string productId, ProductPurchaseInput input, CancellationToken cancellationToken = default)
        => PatchSectionAsync(storeId, productId, "purchase", input, cancellationToken);

    // ─── Images ─────────────────────────────────────────────────────────────

    /// <summary>
    /// A short-lived signed URL to PUT an image straight to storage, plus the
    /// public URL to save on the product. <c>purpose</c> keeps the slot families
    /// apart — a social or variant image can never overwrite a gallery slot.
    /// </summary>
    public async Task<ProductImageUploadUrl?> CreateImageUploadUrlAsync(
        string storeId,
        string productId,
        ProductImageUploadUrlInput input,
        CancellationToken cancellationToken = default)
    {
        var response = await Http.PostAsync<ProductImageUploadUrl>($"{Base}/{storeId}/{productId}/image-upload-url", input, cancellationToken);
        return response.Result;
    }

    // ─── AI content ─────────────────────────────────────────────────────────

    /// <summary>Price a landing-page generation before running it. Charges nothing.</summary>
    public async Task<object?> EstimateAiContentAsync(string storeId, string productId, ProductAiContentInput input, CancellationToken cancellationToken = default)
    {
        var response = await Http.PostAsync<object>($"{Base}/{storeId}/{productId}/ai/estimate", input, cancellationToken);
        return response.Result;
    }

    /// <summary>
    /// Write the requested landing sections from a brief. Credits are withdrawn
    /// on real token usage, capped at the estimate. The result is an UNSAVED
    /// draft — apply it with <see cref="UpdateLandingAsync"/> /
    /// <see cref="UpdateVariantsAsync"/>.
    /// </summary>
    public async Task<object?> GenerateAiContentAsync(string storeId, string productId, ProductAiContentInput input, CancellationToken cancellationToken = default)
    {
        var response = await Http.PostAsync<object>($"{Base}/{storeId}/{productId}/ai/generate", input, cancellationToken);
        return response.Result;
    }

    private async Task<StoreProduct?> PatchSectionAsync(string storeId, string productId, string section, object body, CancellationToken cancellationToken)
    {
        var response = await Http.PatchAsync<StoreProduct>($"{Base}/{storeId}/{productId}/{section}", body, cancellationToken);
        return response.Result;
    }
}
