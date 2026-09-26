using Posty5.Core.Configuration;
using Posty5.Core.Http;

namespace Posty5.Tests;

/// <summary>
/// Test configuration and shared resources
/// </summary>
public static class TestConfig
{
    public static Posty5Options GetOptions()
    {
        // User scope first (as before), then the process scope — the same lookup HasStoreFixture uses.
        var apiKey = Env(ApiKeyVar);
        var baseUrl = Env("POSTY5_BASE_URL") ?? "https://api.posty5.com";

        if (string.IsNullOrEmpty(apiKey))
        {
            Console.WriteLine("⚠️  WARNING: POSTY5_API_KEY is not set!");
            Console.WriteLine("Tests will fail without a valid API key.");
            Console.WriteLine("Set environment variable: POSTY5_API_KEY=your-key-here");
        }
        else
        {
            Console.WriteLine("✅ API Key loaded successfully");
            Console.WriteLine($"🌐 Base URL: {baseUrl}");
        }

        return new Posty5Options
        {
            ApiKey = apiKey ?? "test-key",
            BaseUrl = baseUrl,
            Debug = true
        };
    }

    public static Posty5HttpClient CreateHttpClient()
    {
        return new Posty5HttpClient(GetOptions());
    }

    // Template ID for testing
    public const string TemplateId = "698a268af42b052d15e8f93c";

    /// <summary>
    /// An environment variable from the User scope (like the API key), falling
    /// back to the process scope so CI and non-Windows runs can set it too.
    /// </summary>
    public static string? Env(string name) =>
        NullIfEmpty(Environment.GetEnvironmentVariable(name, EnvironmentVariableTarget.User))
        ?? NullIfEmpty(Environment.GetEnvironmentVariable(name));

    private static string? NullIfEmpty(string? value) => string.IsNullOrEmpty(value) ? null : value;

    // ─── Store dropshipping fixtures (StoreSuppliersClientTests) ─────────────
    // Names are constants so a [StoreFixtureFact] can name what it needs.

    public const string ApiKeyVar = "POSTY5_API_KEY";
    public const string StoreIdVar = "POSTY5_TEST_STORE_ID";
    /// <summary>A <b>test</b>-mode supplier connection on that store — no money can move through it.</summary>
    public const string SupplierIntegrationIdVar = "POSTY5_TEST_SUPPLIER_INTEGRATION_ID";
    public const string SupplierProductIdVar = "POSTY5_TEST_SUPPLIER_PRODUCT_ID";
    /// <summary>A merchant product the link tests may claim and release.</summary>
    public const string ProductIdVar = "POSTY5_TEST_PRODUCT_ID";
    /// <summary>An order with a part on the test-mode connection (group actions).</summary>
    public const string OrderIdVar = "POSTY5_TEST_ORDER_ID";
    /// <summary>That part's key, <c>supplier:&lt;integrationId&gt;</c>.</summary>
    public const string GroupKeyVar = "POSTY5_TEST_GROUP_KEY";
    /// <summary><c>true</c> lets the import fact run: it charges credits and creates (then deletes) a draft.</summary>
    public const string AllowChargesVar = "POSTY5_TEST_ALLOW_CHARGES";
    /// <summary><c>true</c> lets cancel and fulfil-manually run; they end the fixture part's supplier flow.</summary>
    public const string AllowPartTakeoverVar = "POSTY5_TEST_ALLOW_PART_TAKEOVER";

    public static string StoreId => Env(StoreIdVar) ?? string.Empty;
    public static string SupplierIntegrationId => Env(SupplierIntegrationIdVar) ?? string.Empty;
    public static string SupplierProductId => Env(SupplierProductIdVar) ?? string.Empty;
    public static string ProductId => Env(ProductIdVar) ?? string.Empty;
    public static string OrderId => Env(OrderIdVar) ?? string.Empty;
    public static string GroupKey => Env(GroupKeyVar) ?? string.Empty;
    public static bool AllowCharges => string.Equals(Env(AllowChargesVar), "true", StringComparison.OrdinalIgnoreCase);
    public static bool AllowPartTakeover => string.Equals(Env(AllowPartTakeoverVar), "true", StringComparison.OrdinalIgnoreCase);

    /// <summary>An API key and a fixture store: the minimum for any live store fact.</summary>
    public static bool HasStoreFixture => Env(ApiKeyVar) is not null && StoreId.Length > 0;

    // Store created resource IDs for cleanup
    public static class CreatedResources
    {
        public static List<string> QRCodes { get; } = new();
        public static List<string> ShortLinks { get; } = new();
        public static List<string> HtmlHostings { get; } = new();
        public static List<string> Workspaces { get; } = new();
        public static List<string> Posts { get; } = new();
        /// <summary>Supplier links a live store fact created and must delete.</summary>
        public static List<string> SupplierLinks { get; } = new();
        /// <summary>Store products a live store fact created (imports) and must delete.</summary>
        public static List<string> StoreProducts { get; } = new();
    }
}
