using System.Text.Json.Serialization;

namespace Posty5.Store.Models;

// ─── Shared pieces ──────────────────────────────────────────────────────────

/// <summary>A product image.</summary>
public class ProductImageInput
{
    /// <summary>Kept when editing an existing image, so variant values referencing it stay valid.</summary>
    [JsonPropertyName("_id")]
    public string? Id { get; set; }

    /// <summary>Public URL of the image.</summary>
    public string Url { get; set; } = string.Empty;

    /// <summary><c>url</c> or <c>upload</c>.</summary>
    public string? Source { get; set; } = "url";

    /// <summary>Storage path, when the image was uploaded through a signed URL.</summary>
    public string? BucketFilePath { get; set; }
}

/// <summary>A product option / variant (e.g. Size: S, M, L).</summary>
public class ProductOptionInput
{
    /// <summary>Option name.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Allowed values.</summary>
    public List<string> Values { get; set; } = new();
}

// ─── Create / update ────────────────────────────────────────────────────────

/// <summary>A complete product payload.</summary>
public class CreateProductInput
{
    /// <summary>Display name.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Selling price.</summary>
    public decimal Price { get; set; }

    /// <summary>URL slug. Generated from the name when omitted.</summary>
    public string? Slug { get; set; }

    /// <summary>Long description.</summary>
    public string? Description { get; set; }

    /// <summary>Struck-through "was" price.</summary>
    public decimal? CompareAtPrice { get; set; }

    /// <summary><c>null</c> = stock not tracked.</summary>
    public int? Stock { get; set; }

    /// <summary>The merchant's own article number.</summary>
    public string? Sku { get; set; }

    /// <summary><c>active</c> or <c>hidden</c>.</summary>
    public string? Status { get; set; }

    /// <summary>Position on the storefront.</summary>
    public int? SortOrder { get; set; }

    /// <summary>Gallery images, index 0 is the primary.</summary>
    public List<ProductImageInput>? Images { get; set; }

    /// <summary>Simple options. Use the variants section for priced combinations.</summary>
    public List<ProductOptionInput>? Options { get; set; }
}

/// <summary>One product payload for bulk create (mirrors the single-create schema).</summary>
public class BulkProductInput : CreateProductInput
{
}

/// <summary>Wrapper for the bulk create request body.</summary>
public class BulkProductsRequest
{
    /// <summary>Up to 200 products.</summary>
    public List<BulkProductInput> Products { get; set; } = new();
}

/// <summary>Any subset of a product's top-level fields.</summary>
public class UpdateProductInput
{
    /// <summary>Display name.</summary>
    public string? Name { get; set; }

    /// <summary>Selling price.</summary>
    public decimal? Price { get; set; }

    /// <summary>URL slug.</summary>
    public string? Slug { get; set; }

    /// <summary>Long description.</summary>
    public string? Description { get; set; }

    /// <summary>Struck-through "was" price. <c>null</c> clears it.</summary>
    public decimal? CompareAtPrice { get; set; }

    /// <summary><c>null</c> = stock not tracked.</summary>
    public int? Stock { get; set; }

    /// <summary>The merchant's own article number.</summary>
    public string? Sku { get; set; }

    /// <summary><c>draft</c>, <c>active</c> or <c>hidden</c>.</summary>
    public string? Status { get; set; }

    /// <summary>Position on the storefront.</summary>
    public int? SortOrder { get; set; }

    /// <summary>Gallery images.</summary>
    public List<ProductImageInput>? Images { get; set; }

    /// <summary>Simple options.</summary>
    public List<ProductOptionInput>? Options { get; set; }
}

/// <summary>The minimum a product record needs to exist.</summary>
public class CreateProductDraftInput
{
    /// <summary>The merchant's own article number.</summary>
    public string Sku { get; set; } = string.Empty;

    /// <summary>Display name.</summary>
    public string Name { get; set; } = string.Empty;
}

// ─── Search ─────────────────────────────────────────────────────────────────

/// <summary>Filters for searching a store's products.</summary>
public class ProductSearchParams
{
    /// <summary>Partial, case-insensitive match on the product name.</summary>
    public string? Name { get; set; }

    /// <summary>Partial match on the slug.</summary>
    public string? Slug { get; set; }

    /// <summary>Partial match on the SKU.</summary>
    public string? Sku { get; set; }

    /// <summary><c>draft</c>, <c>active</c> or <c>hidden</c>.</summary>
    public string? Status { get; set; }

    /// <summary>Only products carrying one of these tags.</summary>
    public List<string>? TagIds { get; set; }

    /// <summary>Drop products carrying one of these tags.</summary>
    public List<string>? ExcludeTagIds { get; set; }
}

/// <summary>Product summary row (search results).</summary>
public class ProductSummary
{
    /// <summary>Product id.</summary>
    [JsonPropertyName("_id")]
    public string? Id { get; set; }

    /// <summary>Per-store sequence number.</summary>
    public int? Numbering { get; set; }

    /// <summary>Display name.</summary>
    public string? Name { get; set; }

    /// <summary>URL slug.</summary>
    public string? Slug { get; set; }

    /// <summary>Selling price.</summary>
    public decimal Price { get; set; }

    /// <summary>Struck-through "was" price.</summary>
    public decimal? CompareAtPrice { get; set; }

    /// <summary><c>null</c> = stock not tracked.</summary>
    public int? Stock { get; set; }

    /// <summary>The merchant's own article number.</summary>
    public string? Sku { get; set; }

    /// <summary><c>draft</c>, <c>active</c> or <c>hidden</c>.</summary>
    public string? Status { get; set; }

    /// <summary>Position on the storefront.</summary>
    public int SortOrder { get; set; }

    /// <summary>Creation timestamp.</summary>
    public string? CreatedAt { get; set; }

    /// <summary>Last-modified timestamp.</summary>
    public string? UpdatedAt { get; set; }
}

/// <summary>
/// The full product document. Loosely typed on purpose: the API returns every
/// section (variants, landing, purchase), and pinning them all here would make
/// the SDK a second copy of the schema.
/// </summary>
public class StoreProduct : ProductSummary
{
    /// <summary>Every field the API returned, including the sections not typed above.</summary>
    [JsonExtensionData]
    public Dictionary<string, object>? Extra { get; set; }
}

// ─── Section payloads ───────────────────────────────────────────────────────

/// <summary>Name, SKU and description. The slug lives on the SEO section.</summary>
public class ProductBasicInformationInput
{
    /// <summary>Display name.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>The merchant's own article number.</summary>
    public string Sku { get; set; } = string.Empty;

    /// <summary>Long description.</summary>
    public string? Description { get; set; }
}

/// <summary>The full ordered image list — index 0 is the primary image.</summary>
public class ProductMediaInput
{
    /// <summary>Up to 10 images, in display order.</summary>
    public List<ProductImageInput> Images { get; set; } = new();
}

/// <summary>Selling price and the optional compare-at price.</summary>
public class ProductPriceInput
{
    /// <summary>Selling price. <c>null</c> clears it.</summary>
    public decimal? Price { get; set; }

    /// <summary>Struck-through "was" price.</summary>
    public decimal? CompareAtPrice { get; set; }
}

/// <summary>Product-level stock.</summary>
public class ProductStockInput
{
    /// <summary><c>null</c> = stock not tracked; the product never sells out.</summary>
    public int? Stock { get; set; }
}

/// <summary>One value inside a variant group.</summary>
public class VariantValueInput
{
    /// <summary>Existing value id, when editing.</summary>
    [JsonPropertyName("_id")]
    public string? Id { get; set; }

    /// <summary>Client-owned identity that stock combinations reference.</summary>
    public string? Key { get; set; }

    /// <summary>Display name.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Required on a <c>color</c> group.</summary>
    public string? ColorHex { get; set; }

    /// <summary>Image URLs entered for this value.</summary>
    public List<string>? Images { get; set; }

    /// <summary>Surcharge for choosing this value.</summary>
    public decimal? ExtraPrice { get; set; }

    /// <summary><c>null</c> = not tracked at value level.</summary>
    public int? Stock { get; set; }

    /// <summary>Display order.</summary>
    public int? Order { get; set; }

    /// <summary>Whether the value can be chosen.</summary>
    public bool? IsActive { get; set; }
}

/// <summary>A variant group — "Size", "Colour".</summary>
public class VariantGroupInput
{
    /// <summary>Existing group id, when editing.</summary>
    [JsonPropertyName("_id")]
    public string? Id { get; set; }

    /// <summary>Client-owned identity that stock combinations reference.</summary>
    public string? Key { get; set; }

    /// <summary>Display name.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary><c>color</c>, <c>size</c>, <c>material</c>, <c>storage</c> or <c>custom</c>.</summary>
    public string? Type { get; set; }

    /// <summary>Whether the shopper must choose a value.</summary>
    public bool? IsRequired { get; set; }

    /// <summary>Display order.</summary>
    public int? Order { get; set; }

    /// <summary>The group's values.</summary>
    public List<VariantValueInput> Values { get; set; } = new();
}

/// <summary>One buyable combination — "3XL in Black".</summary>
public class VariantStockGroupInput
{
    /// <summary>Existing combination id, when editing.</summary>
    [JsonPropertyName("_id")]
    public string? Id { get; set; }

    /// <summary>Client-owned identity.</summary>
    public string? Key { get; set; }

    /// <summary>Value keys, at most one per group.</summary>
    public List<string> ValueKeys { get; set; } = new();

    /// <summary>Combination-specific article number.</summary>
    public string? Sku { get; set; }

    /// <summary><c>null</c> = this combination is not stock-tracked.</summary>
    public int? Stock { get; set; }

    /// <summary>Whether the combination can be ordered.</summary>
    public bool? IsAvailable { get; set; }

    /// <summary>Display order.</summary>
    public int? Order { get; set; }
}

/// <summary>Variant groups and, optionally, the buyable combinations.</summary>
public class ProductVariantsInput
{
    /// <summary>The variant groups.</summary>
    public List<VariantGroupInput> VariantGroups { get; set; } = new();

    /// <summary>
    /// Leave <c>null</c> to keep the saved combinations. Sending an empty list
    /// wipes them.
    /// </summary>
    public List<VariantStockGroupInput>? VariantStockGroups { get; set; }
}

/// <summary>Replace the product's whole tag list.</summary>
public class ProductTagsInput
{
    /// <summary>Tag ids. An empty list unassigns everything.</summary>
    public List<string> TagIds { get; set; } = new();
}

/// <summary>Search-engine and social metadata.</summary>
public class ProductSeoFields
{
    /// <summary>Meta title.</summary>
    public string? Title { get; set; }

    /// <summary>Meta description.</summary>
    public string? MetaDescription { get; set; }

    /// <summary>Social share image.</summary>
    public string? OgImage { get; set; }

    /// <summary>Ask search engines not to index the page.</summary>
    public bool? NoIndex { get; set; }
}

/// <summary>The SEO section — metadata plus the product's slug.</summary>
public class ProductSeoInput
{
    /// <summary>Metadata fields.</summary>
    public ProductSeoFields Seo { get; set; } = new();

    /// <summary>The product's slug lives on this section.</summary>
    public string? Slug { get; set; }
}

/// <summary>Publication status, featured flag and per-order limits.</summary>
public class ProductSettingsInput
{
    /// <summary><c>draft</c>, <c>active</c> or <c>hidden</c>.</summary>
    public string? Status { get; set; }

    /// <summary>Whether the product is highlighted on the storefront.</summary>
    public bool? IsFeatured { get; set; }

    /// <summary>Minimum quantity per order.</summary>
    public int? MinPerOrder { get; set; }

    /// <summary>Maximum quantity per order.</summary>
    public int? MaxPerOrder { get; set; }

    /// <summary>Position on the storefront.</summary>
    public int? SortOrder { get; set; }
}

/// <summary>One landing-page section.</summary>
public class ProductLandingSection
{
    /// <summary>Whether the section is rendered.</summary>
    public bool? IsEnabled { get; set; }

    /// <summary>The section's own fields, per its definition.</summary>
    public Dictionary<string, object>? Data { get; set; }
}

/// <summary>The product's landing page.</summary>
public class ProductLandingInput
{
    /// <summary>Section keys in render order.</summary>
    public List<string>? SectionOrder { get; set; }

    /// <summary>Sections keyed by section key.</summary>
    public Dictionary<string, ProductLandingSection>? Sections { get; set; }
}

/// <summary>
/// A surcharge this one product adds to delivery. Always charged PER UNIT —
/// five of a product with a <c>5</c> fee add <c>25</c> — regardless of the
/// store's own shipping calculation mode, because the reason is physical: a
/// bulky item costs more to ship for every copy of it.
/// </summary>
public class ProductShippingInput
{
    /// <summary><c>null</c> clears the surcharge.</summary>
    public decimal? ExtraFeePerUnit { get; set; }

    /// <summary>Merchant-facing reason; snapshotted onto the order, never shown to buyers.</summary>
    public string? Note { get; set; }
}

/// <summary>A buy link on a shop we do not control.</summary>
public class ExternalBuyLinkInput
{
    /// <summary>Existing link id, when editing.</summary>
    [JsonPropertyName("_id")]
    public string? Id { get; set; }

    /// <summary>Name of the external shop.</summary>
    public string? StoreName { get; set; }

    /// <summary>The product page. <c>http</c>/<c>https</c> only.</summary>
    public string Url { get; set; } = string.Empty;

    /// <summary>Informational — the price on a store we do not control.</summary>
    public decimal? Price { get; set; }

    /// <summary>Currency of <see cref="Price"/>.</summary>
    public string? Currency { get; set; }

    /// <summary>Shop logo.</summary>
    public string? LogoUrl { get; set; }

    /// <summary><c>amazon</c>, <c>aliexpress</c>, <c>noon</c>, <c>ebay</c>, <c>etsy</c> or <c>custom</c>.</summary>
    public string? Platform { get; set; }

    /// <summary>Display order.</summary>
    public int? Order { get; set; }

    /// <summary>Whether the link is shown.</summary>
    public bool? IsActive { get; set; }
}

/// <summary>How the product is bought.</summary>
public class ProductPurchaseInput
{
    /// <summary>
    /// <c>store</c> sells through the cart, <c>external</c> sends the buyer
    /// elsewhere, <c>both</c> offers each. <c>external</c> and <c>both</c>
    /// require at least one link.
    /// </summary>
    public string? Mode { get; set; }

    /// <summary>The external buy links.</summary>
    public List<ExternalBuyLinkInput>? ExternalLinks { get; set; }
}

// ─── Reorder / images / AI ──────────────────────────────────────────────────

/// <summary>One product's new sort position.</summary>
public class ProductSortOrderInput
{
    /// <summary>Product id.</summary>
    [JsonPropertyName("_id")]
    public string Id { get; set; } = string.Empty;

    /// <summary>New position.</summary>
    public int SortOrder { get; set; }
}

/// <summary>Wrapper for the reorder request body.</summary>
public class ProductReorderRequest
{
    /// <summary>Up to 500 products.</summary>
    public List<ProductSortOrderInput> Items { get; set; } = new();
}

/// <summary>Ask for a signed image upload URL.</summary>
public class ProductImageUploadUrlInput
{
    /// <summary>MIME type of the file being uploaded.</summary>
    public string FileType { get; set; } = string.Empty;

    /// <summary>Gallery slot index (0–9).</summary>
    public int? Index { get; set; }

    /// <summary>
    /// <c>gallery</c>, <c>seo</c>, <c>variant</c> or <c>landing</c> — keeps the
    /// slot families apart so a social image can never overwrite a gallery slot.
    /// </summary>
    public string? Purpose { get; set; }
}

/// <summary>A signed upload URL and the public URL to save on the product.</summary>
public class ProductImageUploadUrl
{
    /// <summary>PUT the file here.</summary>
    public string? UploadUrl { get; set; }

    /// <summary>Save this on the product once the upload succeeds.</summary>
    public string? PublicUrl { get; set; }

    /// <summary>Storage path behind the public URL.</summary>
    public string? BucketFilePath { get; set; }
}

/// <summary>A brief plus the landing sections to write from it.</summary>
public class ProductAiContentInput
{
    /// <summary>What the product is, in the merchant's own words.</summary>
    public string Brief { get; set; } = string.Empty;

    /// <summary>Section keys — from the landing-sections config.</summary>
    public List<string> SectionKeys { get; set; } = new();

    /// <summary>Also suggest variant groups.</summary>
    public bool? GenerateVariants { get; set; }

    /// <summary>Writing tone.</summary>
    public string? Tone { get; set; }

    /// <summary>Output language.</summary>
    public string? Language { get; set; }
}
