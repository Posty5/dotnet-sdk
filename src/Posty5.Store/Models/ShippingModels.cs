using System.Text.Json.Serialization;

namespace Posty5.Store.Models;

// ─── Countries ──────────────────────────────────────────────────────────────

/// <summary>Filters for the store's shipping-countries grid.</summary>
public class ShippingCountrySearchParams
{
    /// <summary>Match on country name or ISO code.</summary>
    public string? Text { get; set; }

    /// <summary>Only countries currently open for orders (or, with <c>false</c>, only paused ones).</summary>
    public bool? IsEnabled { get; set; }
}

/// <summary>One row of the store's shipping-countries grid.</summary>
public class ShippingCountryRow
{
    /// <summary>The zone id.</summary>
    [JsonPropertyName("_id")]
    public string? Id { get; set; }

    /// <summary>Two-letter ISO country code.</summary>
    public string? Iso { get; set; }

    /// <summary>Country name.</summary>
    public string? Name { get; set; }

    /// <summary>Flag emoji.</summary>
    public string? Flag { get; set; }

    /// <summary>Whether the country is open for orders.</summary>
    public bool IsEnabled { get; set; }

    /// <summary>
    /// <c>null</c> = no country fee set; the store default applies. <c>0</c> is
    /// a real fee (free delivery) and stops the fallback.
    /// </summary>
    public decimal? DefaultFee { get; set; }

    /// <summary>Catalogue size — how many governorates could be priced.</summary>
    public int GovernoratesCount { get; set; }

    /// <summary>Catalogue size — how many cities could be priced.</summary>
    public int CitiesCount { get; set; }

    /// <summary>Rows this store actually owns for the country.</summary>
    public int RoutesCount { get; set; }

    /// <summary>Of those, how many refuse delivery.</summary>
    public int BlockedRoutesCount { get; set; }

    /// <summary>Display order.</summary>
    public int Order { get; set; }
}

/// <summary>Filters for the "add a country" picker.</summary>
public class ShippingCatalogueSearchParams
{
    /// <summary>Match on country name or ISO code.</summary>
    public string? Text { get; set; }

    /// <summary>Include countries this store has already opened.</summary>
    public bool? IncludeAdded { get; set; }

    /// <summary>Page number.</summary>
    public int? Page { get; set; }

    /// <summary>Rows per page.</summary>
    public int? PageSize { get; set; }
}

/// <summary>A country in the world catalogue.</summary>
public class ShippingCatalogueCountry
{
    /// <summary>Two-letter ISO country code.</summary>
    public string? Iso { get; set; }

    /// <summary>Country name.</summary>
    public string? Name { get; set; }

    /// <summary>Flag emoji.</summary>
    public string? Flag { get; set; }
}

/// <summary>Page metadata for the offset-paged shipping endpoints.</summary>
public class ShippingPageMeta
{
    /// <summary>Total matching rows.</summary>
    public int TotalCount { get; set; }

    /// <summary>Current page number.</summary>
    public int Page { get; set; }

    /// <summary>Rows per page.</summary>
    public int PageSize { get; set; }
}

/// <summary>Countries available to add.</summary>
public class ShippingCatalogueResult
{
    /// <summary>The catalogue rows.</summary>
    public List<ShippingCatalogueCountry> Items { get; set; } = new();

    /// <summary>The store's currency.</summary>
    public string? Currency { get; set; }

    /// <summary>Page metadata.</summary>
    public ShippingPageMeta Pagination { get; set; } = new();
}

/// <summary>A store's shipping zone for one country.</summary>
public class ShippingZone
{
    /// <summary>Zone id.</summary>
    [JsonPropertyName("_id")]
    public string? Id { get; set; }

    /// <summary>Two-letter ISO country code.</summary>
    public string? CountryIso { get; set; }

    /// <summary>Country name.</summary>
    public string? CountryName { get; set; }

    /// <summary>Whether the country is open for orders.</summary>
    public bool IsEnabled { get; set; }

    /// <summary><c>null</c> = fall through to the store default.</summary>
    public decimal? DefaultFee { get; set; }

    /// <summary>Display order.</summary>
    public int Order { get; set; }
}

/// <summary>Open a country for delivery.</summary>
public class AddShippingCountryInput
{
    /// <summary>Two-letter ISO country code.</summary>
    public string Iso { get; set; } = string.Empty;

    /// <summary>
    /// Required here, unlike everywhere else a fee appears: a country never
    /// opens without a price. Everything inside it inherits this until priced
    /// separately.
    /// </summary>
    public decimal DefaultFee { get; set; }
}

/// <summary>Open, pause or reprice a country.</summary>
public class UpdateShippingCountryInput
{
    /// <summary>Whether the country accepts orders.</summary>
    public bool? IsEnabled { get; set; }

    /// <summary><c>null</c> = fall through to the store default.</summary>
    public decimal? DefaultFee { get; set; }

    /// <summary>Display order.</summary>
    public int? Order { get; set; }
}

/// <summary>The saved zone.</summary>
public class ShippingZoneResult
{
    /// <summary>The zone.</summary>
    public ShippingZone? Zone { get; set; }
}

/// <summary>Confirmation that a country was removed.</summary>
public class RemoveShippingCountryResult
{
    /// <summary>The country that was removed.</summary>
    public string? CountryIso { get; set; }
}

/// <summary>One country's card, with the fees and settings it sits above.</summary>
public class ShippingCountryDetails
{
    /// <summary>The country row.</summary>
    public ShippingCountryRow? Country { get; set; }

    /// <summary>The store-wide fallback, below the country.</summary>
    public decimal StoreDefaultFee { get; set; }

    /// <summary>The store's shipping calculation mode.</summary>
    public string? Calculation { get; set; }

    /// <summary>The store's currency.</summary>
    public string? Currency { get; set; }
}

// ─── Governorates (catalogue reference data) ────────────────────────────────

/// <summary>A governorate in the world catalogue.</summary>
public class ShippingGovernorate
{
    /// <summary>Catalogue state code, upper-case.</summary>
    public string? Code { get; set; }

    /// <summary>Governorate name.</summary>
    public string? Name { get; set; }
}

/// <summary>A country's governorates.</summary>
public class ShippingGovernoratesResult
{
    /// <summary>The governorates. Empty when the catalogue does not divide this country.</summary>
    public List<ShippingGovernorate> Items { get; set; } = new();
}

// ─── Routes ─────────────────────────────────────────────────────────────────

/// <summary>Filters for a country's routes list.</summary>
public class ShippingRouteSearchParams
{
    /// <summary>Which tier to list — <c>governorate</c> (default) or <c>city</c>.</summary>
    public string? Level { get; set; }

    /// <summary>Narrow a city list to one governorate.</summary>
    public string? GovernorateCode { get; set; }

    /// <summary>Match on place name.</summary>
    public string? Text { get; set; }

    /// <summary><c>true</c> = only places with their own fee; <c>false</c> = only inherited ones.</summary>
    public bool? HasFee { get; set; }

    /// <summary><c>true</c> = only deliverable places; <c>false</c> = only blocked ones.</summary>
    public bool? IsAllowed { get; set; }

    /// <summary><c>name</c> or <c>fee</c>.</summary>
    public string? SortField { get; set; }

    /// <summary><c>asc</c> or <c>desc</c>.</summary>
    public string? SortType { get; set; }

    /// <summary>Page number.</summary>
    public int? Page { get; set; }

    /// <summary>Rows per page.</summary>
    public int? PageSize { get; set; }
}

/// <summary>One place, with what it charges today and where that price came from.</summary>
public class ShippingRouteRow
{
    /// <summary><c>governorate</c> or <c>city</c>.</summary>
    public string? Level { get; set; }

    /// <summary>Catalogue state code.</summary>
    public string? GovernorateCode { get; set; }

    /// <summary>Governorate name.</summary>
    public string? GovernorateName { get; set; }

    /// <summary>Empty on a governorate row.</summary>
    public string? CityKey { get; set; }

    /// <summary>Empty on a governorate row.</summary>
    public string? CityName { get; set; }

    /// <summary>Present only when this store stored an override for the place.</summary>
    public string? RateId { get; set; }

    /// <summary>The stored override; <c>null</c> = the place inherits.</summary>
    public decimal? Fee { get; set; }

    /// <summary>What the place charges today, once inheritance is applied.</summary>
    public decimal EffectiveFee { get; set; }

    /// <summary><c>city</c>, <c>governorate</c>, <c>country</c>, <c>storeDefault</c> or <c>none</c>.</summary>
    public string? InheritedFrom { get; set; }

    /// <summary>Whether the place can be delivered to.</summary>
    public bool IsAllowed { get; set; }

    /// <summary>Governorate rows only — catalogue size.</summary>
    public int? CitiesCount { get; set; }
}

/// <summary>A country's routes, plus the fees they inherit from.</summary>
public class ShippingRoutesResult
{
    /// <summary>The route rows.</summary>
    public List<ShippingRouteRow> Items { get; set; } = new();

    /// <summary>What the country charges — what an unpriced governorate inherits.</summary>
    public decimal? CountryFee { get; set; }

    /// <summary>The store-wide fallback, below the country.</summary>
    public decimal StoreDefaultFee { get; set; }

    /// <summary>The store's currency.</summary>
    public string? Currency { get; set; }

    /// <summary>Page metadata.</summary>
    public ShippingPageMeta Pagination { get; set; } = new();
}

/// <summary>A saved override on one place.</summary>
public class ShippingRoute
{
    /// <summary>Route (rate) id.</summary>
    [JsonPropertyName("_id")]
    public string? Id { get; set; }

    /// <summary><c>governorate</c> or <c>city</c>.</summary>
    public string? Level { get; set; }

    /// <summary>Catalogue state code.</summary>
    public string? GovernorateCode { get; set; }

    /// <summary>Governorate name.</summary>
    public string? GovernorateName { get; set; }

    /// <summary>Empty on a governorate row.</summary>
    public string? CityKey { get; set; }

    /// <summary>Empty on a governorate row.</summary>
    public string? CityName { get; set; }

    /// <summary><c>null</c> = the place inherits.</summary>
    public decimal? Fee { get; set; }

    /// <summary>Whether the place can be delivered to.</summary>
    public bool IsAllowed { get; set; }
}

/// <summary>
/// One route's save. At least one of <see cref="Fee"/> / <see cref="IsAllowed"/>
/// must be set — a payload saying nothing has nothing to store.
/// </summary>
public class UpsertShippingRouteInput
{
    /// <summary><c>governorate</c> or <c>city</c>.</summary>
    public string Level { get; set; } = "governorate";

    /// <summary>Catalogue state code, upper-case.</summary>
    public string GovernorateCode { get; set; } = string.Empty;

    /// <summary>Required when <see cref="Level"/> is <c>city</c>.</summary>
    public string? CityKey { get; set; }

    /// <summary><c>null</c> = the place inherits.</summary>
    public decimal? Fee { get; set; }

    /// <summary>Whether the place can be delivered to.</summary>
    public bool? IsAllowed { get; set; }
}

/// <summary>Wrapper for the bulk route request body.</summary>
public class BulkShippingRoutesRequest
{
    /// <summary>Up to 200 rows, each applied and reported independently.</summary>
    public List<UpsertShippingRouteInput> Items { get; set; } = new();
}

/// <summary>Result of saving one route. <c>Rate</c> is null when the row was cleared.</summary>
public class UpsertShippingRouteResult
{
    /// <summary>Whether the save removed the row instead of storing it.</summary>
    public bool Cleared { get; set; }

    /// <summary>The stored route, or <c>null</c> when cleared.</summary>
    public ShippingRoute? Rate { get; set; }
}

/// <summary>One row's outcome in a bulk route save.</summary>
public class BulkShippingRouteSaved
{
    /// <summary><c>governorate</c> or <c>city</c>.</summary>
    public string? Level { get; set; }

    /// <summary>Catalogue state code.</summary>
    public string? GovernorateCode { get; set; }

    /// <summary>Empty on a governorate row.</summary>
    public string? CityKey { get; set; }

    /// <summary>Whether the row was removed instead of stored.</summary>
    public bool Cleared { get; set; }

    /// <summary>The stored route, or <c>null</c> when cleared.</summary>
    public ShippingRoute? Rate { get; set; }
}

/// <summary>One row that could not be saved.</summary>
public class BulkShippingRouteFailed
{
    /// <summary><c>governorate</c> or <c>city</c>.</summary>
    public string? Level { get; set; }

    /// <summary>Catalogue state code.</summary>
    public string? GovernorateCode { get; set; }

    /// <summary>Empty on a governorate row.</summary>
    public string? CityKey { get; set; }

    /// <summary>Why the row was rejected.</summary>
    public string? Message { get; set; }
}

/// <summary>Per-row result of a bulk route save.</summary>
public class BulkShippingRoutesResult
{
    /// <summary>Rows that were applied.</summary>
    public List<BulkShippingRouteSaved> Saved { get; set; } = new();

    /// <summary>Rows that were rejected.</summary>
    public List<BulkShippingRouteFailed> Failed { get; set; } = new();
}

/// <summary>"Apply one fee to everything in this scope."</summary>
public class ApplyShippingFeeInput
{
    /// <summary><c>governorate</c> or <c>city</c>.</summary>
    public string Level { get; set; } = "governorate";

    /// <summary>Narrow the scope to one governorate; omit to re-price the whole country.</summary>
    public string? GovernorateCode { get; set; }

    /// <summary>The fee to apply. <c>null</c> puts the scope back on inheritance.</summary>
    public decimal? Fee { get; set; }

    /// <summary>Whether the places can be delivered to.</summary>
    public bool? IsAllowed { get; set; }
}

/// <summary>
/// How many rows the apply produced and removed. A fee equal to what the scope
/// already inherits clears rows rather than writing them, so <c>Cleared</c> is
/// the success case for "make it all uniform".
/// </summary>
public class ApplyShippingFeeResult
{
    /// <summary>Rows written.</summary>
    public int Written { get; set; }

    /// <summary>Rows removed.</summary>
    public int Cleared { get; set; }
}

/// <summary>Confirmation that a route override was cleared.</summary>
public class ClearShippingRouteResult
{
    /// <summary>The route that was cleared.</summary>
    [JsonPropertyName("_id")]
    public string? Id { get; set; }
}

// ─── Preview ────────────────────────────────────────────────────────────────

/// <summary>The resolved fee for a destination, and how the fallback reached it.</summary>
public class ShippingFeePreview
{
    /// <summary>What this destination would be charged.</summary>
    public decimal? Fee { get; set; }

    /// <summary>Currency of <see cref="Fee"/>.</summary>
    public string? Currency { get; set; }

    /// <summary>Which level the fee came from.</summary>
    public string? InheritedFrom { get; set; }

    /// <summary>Whether the destination can be delivered to.</summary>
    public bool? IsAllowed { get; set; }

    /// <summary>Every field the API returned, including those not typed above.</summary>
    [JsonExtensionData]
    public Dictionary<string, object>? Extra { get; set; }
}

// â”€â”€â”€ Package profiles (task12) â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
//
// A profile is a set of BRACKETS â€” "up to 1 kg", "up to 5 kg" â€” and an
// assignment attaches one to a place with a fee per bracket. At checkout the
// cart's parcel is measured, the most specific tier holding profiles answers,
// and the first bracket the parcel fits sets the fee.
//
// Two things about this are easy to get backwards, and both cost money:
//
//   - A profile answers BEFORE the flat country/governorate/city chain. Where
//     no bracket matches, the flat chain still answers, so profiles add to a
//     store's existing setup rather than replacing it.
//   - The most specific tier with any profiles owns the answer OUTRIGHT â€” it is
//     never merged with the tiers above. A city with its own profiles ignores
//     the governorate's completely, even for a parcel none of its brackets fit.

/// <summary>
/// One bracket of a package profile.
/// </summary>
/// <remarks>
/// Every limit is nullable and <c>null</c> means "no cap on this measurement" â€”
/// the opposite of a <c>null</c> on the parcel, which means "not measured" and
/// fits nothing.
/// </remarks>
public class ShippingProfileCondition
{
    /// <summary>Client-owned identity a fee points at. Generated server-side when omitted.</summary>
    public string? Key { get; set; }

    /// <summary>Shown next to the fee input; generated from the limits when left empty.</summary>
    public string? Label { get; set; }

    /// <summary>Maximum weight in kg. <c>null</c> = no cap.</summary>
    public decimal? MaxWeight { get; set; }

    /// <summary>Maximum length in cm. <c>null</c> = no cap.</summary>
    public decimal? MaxLength { get; set; }

    /// <summary>Maximum width in cm. <c>null</c> = no cap.</summary>
    public decimal? MaxWidth { get; set; }

    /// <summary>Maximum height in cm. <c>null</c> = no cap.</summary>
    public decimal? MaxHeight { get; set; }

    /// <summary>Matching order. The first fitting bracket wins.</summary>
    public int? Order { get; set; }
}

/// <summary>A package profile and its brackets.</summary>
public class ShippingProfile
{
    /// <summary>Profile id.</summary>
    [JsonPropertyName("_id")]
    public string? Id { get; set; }

    /// <summary>Display name.</summary>
    public string? Name { get; set; }

    /// <summary><c>weight</c> or <c>dimension</c>. Immutable after creation.</summary>
    public string? Type { get; set; }

    /// <summary>Merchant-facing note.</summary>
    public string? Description { get; set; }

    /// <summary>Brackets, in matching order.</summary>
    public List<ShippingProfileCondition> Conditions { get; set; } = new();

    /// <summary>How many brackets it holds.</summary>
    public int ConditionsCount { get; set; }

    /// <summary>How many places use it. A profile in use cannot be deleted.</summary>
    public int AssignmentsCount { get; set; }

    /// <summary>Creation timestamp.</summary>
    public DateTime? CreatedAt { get; set; }

    /// <summary>Last update timestamp.</summary>
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>Filters for the profiles list.</summary>
public class ShippingProfileSearchParams
{
    /// <summary>Match on the profile name.</summary>
    public string? Text { get; set; }

    /// <summary><c>weight</c> or <c>dimension</c>.</summary>
    public string? Type { get; set; }
}

/// <summary>Create a package profile.</summary>
public class CreateShippingProfileInput
{
    /// <summary>Display name.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// <c>weight</c> or <c>dimension</c>. Immutable afterwards: it decides which
    /// limits a bracket may carry, so changing it would reinterpret every bracket
    /// already written and every fee already priced against them.
    /// </summary>
    public string Type { get; set; } = "weight";

    /// <summary>Merchant-facing note.</summary>
    public string? Description { get; set; }

    /// <summary>
    /// Optional â€” a profile with no brackets is a legal first state, since the
    /// name and type are saved before the brackets are known.
    /// </summary>
    public List<ShippingProfileCondition>? Conditions { get; set; }
}

/// <summary>Rename, re-describe, or replace the bracket list. Note the absence of Type.</summary>
public class UpdateShippingProfileInput
{
    /// <summary>New display name.</summary>
    public string? Name { get; set; }

    /// <summary>New merchant-facing note.</summary>
    public string? Description { get; set; }

    /// <summary>
    /// Replaces the bracket list wholesale. To append instead, use
    /// <c>AddProfileConditionsAsync</c>.
    /// </summary>
    public List<ShippingProfileCondition>? Conditions { get; set; }
}

/// <summary>Request body for appending brackets.</summary>
public class AddShippingProfileConditionsRequest
{
    /// <summary>Brackets to append. The existing ones are left alone.</summary>
    public List<ShippingProfileCondition> Conditions { get; set; } = new();
}

/// <summary>Request body for a bracket import.</summary>
public class ImportShippingProfileConditionsRequest
{
    /// <summary>The filled-in .xlsx, base64-encoded.</summary>
    public string File { get; set; } = string.Empty;
}

/// <summary>Which tier an assignment lives at.</summary>
public class ShippingAssignmentPlace
{
    /// <summary><c>country</c>, <c>governorate</c> or <c>city</c>.</summary>
    public string Level { get; set; } = "country";

    /// <summary>Required at governorate and city level.</summary>
    public string? GovernorateCode { get; set; }

    /// <summary>
    /// Required at city level. City names repeat across governorates, so the
    /// governorate/city pair is the identity everywhere in this module.
    /// </summary>
    public string? CityKey { get; set; }
}

/// <summary>One bracket's price at one place.</summary>
public class ShippingAssignmentFee
{
    /// <summary>Which bracket of the profile this prices.</summary>
    public string ConditionKey { get; set; } = string.Empty;

    /// <summary><c>null</c> = not priced here yet; a parcel landing in it falls through.</summary>
    public decimal? Fee { get; set; }
}

/// <summary>One priced bracket, with the limits it prices.</summary>
public class ShippingAssignmentFeeRow : ShippingAssignmentFee
{
    /// <summary>Bracket label.</summary>
    public string? Label { get; set; }

    /// <summary>Maximum weight in kg. <c>null</c> = no cap.</summary>
    public decimal? MaxWeight { get; set; }

    /// <summary>Maximum length in cm. <c>null</c> = no cap.</summary>
    public decimal? MaxLength { get; set; }

    /// <summary>Maximum width in cm. <c>null</c> = no cap.</summary>
    public decimal? MaxWidth { get; set; }

    /// <summary>Maximum height in cm. <c>null</c> = no cap.</summary>
    public decimal? MaxHeight { get; set; }

    /// <summary>Matching order.</summary>
    public int Order { get; set; }
}

/// <summary>One profile assigned to one place.</summary>
public class ShippingAssignment
{
    /// <summary>Assignment id.</summary>
    [JsonPropertyName("_id")]
    public string? Id { get; set; }

    /// <summary>The profile being priced.</summary>
    public string? ProfileId { get; set; }

    /// <summary>The profile's name, for display.</summary>
    public string? ProfileName { get; set; }

    /// <summary><c>weight</c> or <c>dimension</c>.</summary>
    public string? Type { get; set; }

    /// <summary><c>country</c>, <c>governorate</c> or <c>city</c>.</summary>
    public string? Level { get; set; }

    /// <summary>Empty on a country row.</summary>
    public string? GovernorateCode { get; set; }

    /// <summary>Governorate name, for display.</summary>
    public string? GovernorateName { get; set; }

    /// <summary>Empty on a country or governorate row.</summary>
    public string? CityKey { get; set; }

    /// <summary>City name, for display.</summary>
    public string? CityName { get; set; }

    /// <summary>Wins over a cheaper alternative at the same place.</summary>
    public bool IsDefault { get; set; }

    /// <summary>The profile's brackets, with this place's prices.</summary>
    public List<ShippingAssignmentFeeRow> Fees { get; set; } = new();

    /// <summary>Brackets still waiting for a price.</summary>
    public int UnpricedCount { get; set; }

    /// <summary>Display order.</summary>
    public int Order { get; set; }
}

/// <summary>What a place's profiles look like once inheritance is applied.</summary>
/// <remarks>
/// <see cref="InheritedFrom"/> is what lets a UI say "these are the country's,
/// and they stop applying the moment you add one here" instead of showing an
/// empty list that reads like "nothing ships here".
/// </remarks>
public class ShippingAssignmentsView
{
    /// <summary>The profiles that apply.</summary>
    public List<ShippingAssignment> Items { get; set; } = new();

    /// <summary>Which tier the listed rows actually come from, or <c>none</c>.</summary>
    public string? InheritedFrom { get; set; }

    /// <summary>True when the rows belong to a level ABOVE the one being read.</summary>
    public bool IsInherited { get; set; }
}

/// <summary>Assign a profile to a place, or re-price one already there.</summary>
public class AssignShippingProfileInput : ShippingAssignmentPlace
{
    /// <summary>The profile to attach.</summary>
    public string ProfileId { get; set; } = string.Empty;

    /// <summary>
    /// One entry per bracket. A missing or <c>null</c> fee is a bracket the
    /// merchant has not priced â€” safe to save, and the parcel falls through to
    /// the flat chain rather than shipping free.
    /// </summary>
    public List<ShippingAssignmentFee>? Fees { get; set; }

    /// <summary>Make this the place's default profile.</summary>
    public bool? IsDefault { get; set; }
}

/// <summary>Result of removing an assignment.</summary>
public class RemoveShippingAssignmentResult
{
    /// <summary>The assignment that was removed.</summary>
    [JsonPropertyName("_id")]
    public string? Id { get; set; }
}

/// <summary>Result of deleting a profile.</summary>
public class DeleteShippingProfileResult
{
    /// <summary>The profile that was deleted.</summary>
    [JsonPropertyName("_id")]
    public string? Id { get; set; }
}

