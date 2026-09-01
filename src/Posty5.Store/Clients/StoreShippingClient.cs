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

    // ─── Package profiles (task12) ──────────────────────────────────────────
    //
    // A profile prices by the size of the parcel rather than by where it is
    // going, and answers BEFORE the flat chain above. Where no bracket matches,
    // the flat chain still answers — so profiles add to a store's setup rather
    // than replacing it.

    /// <summary>The store's package profiles.</summary>
    public async Task<PaginationResponse<ShippingProfile>?> ListProfilesAsync(
        string storeId,
        ShippingProfileSearchParams? filters = null,
        PaginationParams? pagination = null,
        CancellationToken cancellationToken = default)
    {
        var query = Query();
        if (filters != null)
        {
            Add(query, "text", filters.Text);
            Add(query, "type", filters.Type);
        }
        AddPagination(query, pagination);

        var response = await Http.GetAsync<PaginationResponse<ShippingProfile>>($"{Base}/{storeId}/profiles", query, cancellationToken);
        return response.Result;
    }

    /// <summary>One profile with its brackets.</summary>
    public async Task<ShippingProfile?> GetProfileAsync(string storeId, string profileId, CancellationToken cancellationToken = default)
    {
        var response = await Http.GetAsync<ShippingProfile>($"{Base}/{storeId}/profiles/{profileId}", null, cancellationToken);
        return response.Result;
    }

    /// <summary>
    /// Create a profile. A profile with no brackets is a legal first state — the
    /// name and type are saved before the brackets are known.
    /// </summary>
    public async Task<ShippingProfile?> CreateProfileAsync(
        string storeId,
        CreateShippingProfileInput input,
        CancellationToken cancellationToken = default)
    {
        var response = await Http.PostAsync<ShippingProfile>($"{Base}/{storeId}/profiles", input, cancellationToken);
        return response.Result;
    }

    /// <summary>Rename, re-describe, or replace the bracket list. Type cannot change.</summary>
    public async Task<ShippingProfile?> UpdateProfileAsync(
        string storeId,
        string profileId,
        UpdateShippingProfileInput changes,
        CancellationToken cancellationToken = default)
    {
        var response = await Http.PutAsync<ShippingProfile>($"{Base}/{storeId}/profiles/{profileId}", changes, cancellationToken);
        return response.Result;
    }

    /// <summary>
    /// Delete a profile. Refused while any place still assigns it — the fees a
    /// merchant typed against its brackets would go with it.
    /// </summary>
    public async Task<DeleteShippingProfileResult?> DeleteProfileAsync(
        string storeId,
        string profileId,
        CancellationToken cancellationToken = default)
    {
        var response = await Http.DeleteAsync<DeleteShippingProfileResult>($"{Base}/{storeId}/profiles/{profileId}", cancellationToken);
        return response.Result;
    }

    /// <summary>
    /// Append brackets, leaving the existing ones alone.
    /// </summary>
    /// <remarks>
    /// A bracket added here shows up on every assignment immediately, unpriced —
    /// the profile owns the bracket list, an assignment only prices it.
    /// </remarks>
    public async Task<ShippingProfile?> AddProfileConditionsAsync(
        string storeId,
        string profileId,
        IEnumerable<ShippingProfileCondition> conditions,
        CancellationToken cancellationToken = default)
    {
        var body = new AddShippingProfileConditionsRequest { Conditions = conditions.ToList() };
        var response = await Http.PostAsync<ShippingProfile>($"{Base}/{storeId}/profiles/{profileId}/conditions", body, cancellationToken);
        return response.Result;
    }

    /// <summary>Remove one bracket, and with it the fees pointing at it.</summary>
    public async Task<ShippingProfile?> RemoveProfileConditionAsync(
        string storeId,
        string profileId,
        string conditionKey,
        CancellationToken cancellationToken = default)
    {
        var response = await Http.DeleteAsync<ShippingProfile>(
            $"{Base}/{storeId}/profiles/{profileId}/conditions/{Uri.EscapeDataString(conditionKey)}",
            cancellationToken);
        return response.Result;
    }

    /// <summary>
    /// The spreadsheet a merchant fills brackets into.
    /// </summary>
    /// <remarks>
    /// The columns differ by type — a weight profile gets two, a dimension
    /// profile five — so pass the type you are importing into. A weight store
    /// handed three columns it must leave empty cannot tell "no limit" from
    /// "I forgot".
    /// </remarks>
    public Task<FileResponse> DownloadProfileTemplateAsync(
        string storeId,
        string type = "weight",
        CancellationToken cancellationToken = default)
    {
        var query = Query();
        Add(query, "type", type);
        return Http.GetBytesAsync($"{Base}/{storeId}/profiles/template", query, cancellationToken);
    }

    /// <summary>Bulk brackets from a filled-in template, base64-encoded in the body.</summary>
    public async Task<BulkImportReport?> ImportProfileConditionsAsync(
        string storeId,
        string profileId,
        string fileBase64,
        CancellationToken cancellationToken = default)
    {
        var body = new ImportShippingProfileConditionsRequest { File = fileBase64 };
        var response = await Http.PostAsync<BulkImportReport>(
            $"{Base}/{storeId}/profiles/{profileId}/conditions/import",
            body,
            cancellationToken);
        return response.Result;
    }

    // ─── Profile assignments ────────────────────────────────────────────────

    /// <summary>
    /// The profiles that apply at one place, and which tier they came from.
    /// </summary>
    /// <remarks>
    /// Walks upward, so an unpriced city reports what it is currently inheriting
    /// rather than an empty list. Read <see cref="ShippingAssignmentsView.IsInherited"/>
    /// before showing the rows as the place's own.
    /// </remarks>
    public async Task<ShippingAssignmentsView?> ListAssignmentsAsync(
        string storeId,
        string iso,
        ShippingAssignmentPlace place,
        CancellationToken cancellationToken = default)
    {
        var query = Query();
        Add(query, "level", place.Level);
        Add(query, "governorateCode", place.GovernorateCode);
        Add(query, "cityKey", place.CityKey);

        var response = await Http.GetAsync<ShippingAssignmentsView>($"{Base}/{storeId}/countries/{iso}/assignments", query, cancellationToken);
        return response.Result;
    }

    /// <summary>
    /// Assign a profile to a place, or re-price one already there.
    /// </summary>
    /// <remarks>
    /// The moment a place has one of these, it stops inheriting from above
    /// entirely — including for parcels none of its own brackets fit. That is the
    /// rule the whole feature turns on: a merchant who prices a city separately
    /// means "this is what this city costs", not "add these to what the country
    /// already said".
    /// </remarks>
    public async Task<ShippingAssignment?> AssignProfileAsync(
        string storeId,
        string iso,
        AssignShippingProfileInput input,
        CancellationToken cancellationToken = default)
    {
        var response = await Http.PostAsync<ShippingAssignment>($"{Base}/{storeId}/countries/{iso}/assignments", input, cancellationToken);
        return response.Result;
    }

    /// <summary>
    /// Make one assignment the place's default.
    /// </summary>
    /// <remarks>
    /// Where a place offers alternatives ("by weight" or "by size"), the default
    /// is quoted even when the other is cheaper — the merchant named it, which is
    /// a decision rather than a tie-break. Without one, the cheaper match wins.
    /// </remarks>
    public async Task<ShippingAssignment?> SetDefaultAssignmentAsync(
        string storeId,
        string assignmentId,
        CancellationToken cancellationToken = default)
    {
        var response = await Http.PutAsync<ShippingAssignment>($"{Base}/{storeId}/assignments/{assignmentId}/default", new { }, cancellationToken);
        return response.Result;
    }

    /// <summary>Remove an assignment, putting the place back on whatever it inherits.</summary>
    public async Task<RemoveShippingAssignmentResult?> RemoveAssignmentAsync(
        string storeId,
        string assignmentId,
        CancellationToken cancellationToken = default)
    {
        var response = await Http.DeleteAsync<RemoveShippingAssignmentResult>($"{Base}/{storeId}/assignments/{assignmentId}", cancellationToken);
        return response.Result;
    }
}
