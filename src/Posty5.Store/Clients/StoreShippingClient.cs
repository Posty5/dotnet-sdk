using Posty5.Core.Http;
using Posty5.Core.Models;
using Posty5.Store.Models;

namespace Posty5.Store.Clients;

/// <summary>
/// Where a store delivers and what it charges — <c>/api/store-shipping</c>.
/// Requires <c>settings.manage</c>.
/// </summary>
/// <remarks>
/// The model is country → governorate → city, and a fee falls through those in
/// order before landing on the store default. A store stores one document per
/// open country plus one route row per place it prices or blocks differently;
/// everywhere it said nothing simply has no row. That is why
/// <see cref="UpsertRouteAsync"/> with nothing to say removes the row, and why
/// <see cref="ApplyFeeAsync"/> with the inherited fee clears rows rather than
/// writing them.
/// </remarks>
public class StoreShippingClient : StoreClientBase
{
    private const string Base = "/api/store-shipping";

    /// <summary>Creates a shipping client.</summary>
    /// <param name="httpClient">HTTP client instance from Posty5.Core.</param>
    public StoreShippingClient(Posty5HttpClient httpClient) : base(httpClient) { }

    // ─── Countries ──────────────────────────────────────────────────────────

    /// <summary>The countries this store has opened for delivery.</summary>
    public async Task<PaginationResponse<ShippingCountryRow>?> ListCountriesAsync(
        string storeId,
        ShippingCountrySearchParams? filters = null,
        PaginationParams? pagination = null,
        CancellationToken cancellationToken = default)
    {
        var query = Query();
        if (filters != null)
        {
            Add(query, "text", filters.Text);
            Add(query, "isEnabled", filters.IsEnabled);
        }
        AddPagination(query, pagination);

        var response = await Http.GetAsync<PaginationResponse<ShippingCountryRow>>($"{Base}/{storeId}/countries", query, cancellationToken);
        return response.Result;
    }

    /// <summary>
    /// The world catalogue minus what this store already opened — what an "add a
    /// country" picker reads. Set IncludeAdded for the full list.
    /// </summary>
    public async Task<ShippingCatalogueResult?> ListCountryCatalogueAsync(
        string storeId,
        ShippingCatalogueSearchParams? filters = null,
        CancellationToken cancellationToken = default)
    {
        var query = Query();
        if (filters != null)
        {
            Add(query, "text", filters.Text);
            Add(query, "includeAdded", filters.IncludeAdded);
            Add(query, "page", filters.Page);
            Add(query, "pageSize", filters.PageSize);
        }

        var response = await Http.GetAsync<ShippingCatalogueResult>($"{Base}/{storeId}/countries/catalogue", query, cancellationToken);
        return response.Result;
    }

    /// <summary>One country's card, plus the store default fee, calculation mode and currency.</summary>
    public async Task<ShippingCountryDetails?> GetCountryAsync(string storeId, string iso, CancellationToken cancellationToken = default)
    {
        var response = await Http.GetAsync<ShippingCountryDetails>($"{Base}/{storeId}/countries/{iso}", null, cancellationToken);
        return response.Result;
    }

    /// <summary>
    /// Open a country for delivery. DefaultFee is required — a country never
    /// opens without a price. Every governorate and city inside it inherits that
    /// fee; no rows are written for them until one is priced differently.
    /// Refuses a country that has already been added.
    /// </summary>
    public async Task<ShippingZoneResult?> AddCountryAsync(string storeId, AddShippingCountryInput input, CancellationToken cancellationToken = default)
    {
        var response = await Http.PostAsync<ShippingZoneResult>($"{Base}/{storeId}/countries", input, cancellationToken);
        return response.Result;
    }

    /// <summary>Open or pause a country and set its fee.</summary>
    public async Task<ShippingZoneResult?> UpdateCountryAsync(
        string storeId,
        string iso,
        UpdateShippingCountryInput changes,
        CancellationToken cancellationToken = default)
    {
        var response = await Http.PutAsync<ShippingZoneResult>($"{Base}/{storeId}/countries/{iso}", changes, cancellationToken);
        return response.Result;
    }

    /// <summary>Soft-delete a country and every route under it.</summary>
    public async Task<RemoveShippingCountryResult?> DeleteCountryAsync(string storeId, string iso, CancellationToken cancellationToken = default)
    {
        var response = await Http.DeleteAsync<RemoveShippingCountryResult>($"{Base}/{storeId}/countries/{iso}", cancellationToken);
        return response.Result;
    }

    // ─── Routes ─────────────────────────────────────────────────────────────

    /// <summary>
    /// A country's governorates — catalogue reference data, not this store's
    /// rows. An empty list means the catalogue does not divide this country.
    /// </summary>
    public async Task<ShippingGovernoratesResult?> ListGovernoratesAsync(string storeId, string iso, CancellationToken cancellationToken = default)
    {
        var response = await Http.GetAsync<ShippingGovernoratesResult>($"{Base}/{storeId}/countries/{iso}/governorates", null, cancellationToken);
        return response.Result;
    }

    /// <summary>
    /// Every governorate of a country, or every city of one governorate, each
    /// with what it charges today and whether that price is its own (Fee) or
    /// inherited (InheritedFrom).
    /// </summary>
    public async Task<ShippingRoutesResult?> ListRoutesAsync(
        string storeId,
        string iso,
        ShippingRouteSearchParams? filters = null,
        CancellationToken cancellationToken = default)
    {
        var query = Query();
        if (filters != null)
        {
            Add(query, "level", filters.Level);
            Add(query, "governorateCode", filters.GovernorateCode);
            Add(query, "text", filters.Text);
            Add(query, "hasFee", filters.HasFee);
            Add(query, "isAllowed", filters.IsAllowed);
            Add(query, "sortField", filters.SortField);
            Add(query, "sortType", filters.SortType);
            Add(query, "page", filters.Page);
            Add(query, "pageSize", filters.PageSize);
        }

        var response = await Http.GetAsync<ShippingRoutesResult>($"{Base}/{storeId}/countries/{iso}/routes", query, cancellationToken);
        return response.Result;
    }

    /// <summary>
    /// Price or block one governorate or city. A route that ends up saying
    /// nothing — no fee AND delivery allowed — is removed rather than stored, so
    /// Rate comes back null with Cleared set.
    /// </summary>
    public async Task<UpsertShippingRouteResult?> UpsertRouteAsync(
        string storeId,
        string iso,
        UpsertShippingRouteInput input,
        CancellationToken cancellationToken = default)
    {
        var response = await Http.PostAsync<UpsertShippingRouteResult>($"{Base}/{storeId}/countries/{iso}/routes", input, cancellationToken);
        return response.Result;
    }

    /// <summary>
    /// Price or block up to 200 places in one call. Each row is applied and
    /// reported independently, so one bad row never discards the rest.
    /// </summary>
    public async Task<BulkShippingRoutesResult?> BulkUpsertRoutesAsync(
        string storeId,
        string iso,
        IEnumerable<UpsertShippingRouteInput> items,
        CancellationToken cancellationToken = default)
    {
        var body = new BulkShippingRoutesRequest { Items = items.ToList() };
        var response = await Http.PostAsync<BulkShippingRoutesResult>($"{Base}/{storeId}/countries/{iso}/routes/bulk", body, cancellationToken);
        return response.Result;
    }

    /// <summary>
    /// Give every governorate of a country — or every city of one governorate —
    /// the same fee. A fee equal to what those places already inherit clears
    /// their rows instead of writing them: uniformity is what was asked for, and
    /// the model expresses it by having no rows.
    /// </summary>
    public async Task<ApplyShippingFeeResult?> ApplyFeeAsync(
        string storeId,
        string iso,
        ApplyShippingFeeInput input,
        CancellationToken cancellationToken = default)
    {
        var response = await Http.PostAsync<ApplyShippingFeeResult>($"{Base}/{storeId}/countries/{iso}/routes/apply-fee", input, cancellationToken);
        return response.Result;
    }

    /// <summary>Remove one override, putting the place back on whatever it inherits.</summary>
    public async Task<ClearShippingRouteResult?> ClearRouteAsync(string storeId, string rateId, CancellationToken cancellationToken = default)
    {
        var response = await Http.DeleteAsync<ClearShippingRouteResult>($"{Base}/{storeId}/routes/{rateId}", cancellationToken);
        return response.Result;
    }

    // ─── Preview ────────────────────────────────────────────────────────────

    /// <summary>
    /// What a destination would be charged, resolved through the same city →
    /// governorate → country → store-default fallback checkout uses. Use it to
    /// show a fee before creating an order — a manual order never sends its own.
    /// </summary>
    public async Task<ShippingFeePreview?> PreviewFeeAsync(
        string storeId,
        string countryIso,
        string? governorateCode = null,
        string? cityKey = null,
        CancellationToken cancellationToken = default)
    {
        var query = Query();
        Add(query, "countryIso", countryIso);
        Add(query, "governorateCode", governorateCode);
        Add(query, "cityKey", cityKey);

        var response = await Http.GetAsync<ShippingFeePreview>($"{Base}/{storeId}/preview-fee", query, cancellationToken);
        return response.Result;
    }
}
