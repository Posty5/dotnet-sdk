using System.Text.Json.Serialization;

namespace Posty5.Store.Models;

/// <summary>Filters for searching a store's customers.</summary>
public class CustomerSearchParams
{
    /// <summary>One term matched across name, phone and email.</summary>
    public string? Text { get; set; }

    /// <summary><c>true</c> = only shoppers with a Posty5 account, <c>false</c> = only guests.</summary>
    public bool? HasAccount { get; set; }
}

/// <summary>Figures for one customer in one store, computed from that store's orders.</summary>
public class StoreCustomerStats
{
    /// <summary>Orders placed with this store.</summary>
    public int OrdersCount { get; set; }

    /// <summary>Total spent with this store.</summary>
    public decimal TotalSpent { get; set; }

    /// <summary>Currency of <see cref="TotalSpent"/>.</summary>
    public string? Currency { get; set; }

    /// <summary>When they first ordered here.</summary>
    public string? FirstOrderAt { get; set; }

    /// <summary>When they last ordered here.</summary>
    public string? LastOrderAt { get; set; }
}

/// <summary>Somebody who has ordered from the store.</summary>
public class StoreCustomer
{
    /// <summary>Customer id.</summary>
    [JsonPropertyName("_id")]
    public string? Id { get; set; }

    /// <summary>Their name.</summary>
    public string? Name { get; set; }

    /// <summary>Their phone.</summary>
    public string? Phone { get; set; }

    /// <summary>Their email.</summary>
    public string? Email { get; set; }

    /// <summary>
    /// Whether the shopper has a Posty5 account. Provider ids and tokens are
    /// never exposed.
    /// </summary>
    public bool HasAccount { get; set; }

    /// <summary>Their figures for this store.</summary>
    public StoreCustomerStats? Stats { get; set; }
}

/// <summary>An address this customer has used with this store.</summary>
public class StoreCustomerAddress
{
    /// <summary>Address id.</summary>
    [JsonPropertyName("_id")]
    public string? Id { get; set; }

    /// <summary>The shopper's own label — "Home", "Work".</summary>
    public string? Label { get; set; }

    /// <summary>Recipient name.</summary>
    public string? Name { get; set; }

    /// <summary>Recipient phone.</summary>
    public string? Phone { get; set; }

    /// <summary>Street address.</summary>
    public string? Address { get; set; }

    /// <summary>Two-letter ISO country code.</summary>
    public string? CountryIso { get; set; }

    /// <summary>Country name.</summary>
    public string? CountryName { get; set; }

    /// <summary>Catalogue state code, upper-case.</summary>
    public string? GovernorateCode { get; set; }

    /// <summary>Governorate name.</summary>
    public string? GovernorateName { get; set; }

    /// <summary>Normalized city key.</summary>
    public string? CityKey { get; set; }

    /// <summary>City name.</summary>
    public string? CityName { get; set; }

    /// <summary>LEGACY — still returned for addresses saved before the route model.</summary>
    public string? CityId { get; set; }

    /// <summary>Delivery notes.</summary>
    public string? Notes { get; set; }

    /// <summary>Whether this is the shopper's default address.</summary>
    public bool IsDefault { get; set; }
}
