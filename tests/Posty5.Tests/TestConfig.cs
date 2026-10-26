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
        // User scope first (as before), then the process scope — the same lookup IsSet uses.
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

    // ─── Local test server (RecordingServer) ─────────────────────────────────

    /// <summary>
    /// How many freshly probed ports <see cref="RecordingServer"/> tries before
    /// its constructor gives up. A probed port can be taken before the listener
    /// registers it; one retry almost always lands.
    /// </summary>
    public const int RecordingServerStartAttempts = 10;

    /// <summary>
    /// An environment variable from the User scope (like the API key), falling
    /// back to the process scope so CI and non-Windows runs can set it too.
    /// </summary>
    public static string? Env(string name) =>
        NullIfEmpty(Environment.GetEnvironmentVariable(name, EnvironmentVariableTarget.User))
        ?? NullIfEmpty(Environment.GetEnvironmentVariable(name));

    private static string? NullIfEmpty(string? value) => string.IsNullOrEmpty(value) ? null : value;

    /// <summary>
    /// Whether a fixture variable is set. A variable ending in <c>_ALLOW_…</c> is
    /// a switch and must equal <c>true</c>; any other only has to be present.
    /// </summary>
    public static bool IsSet(string name)
    {
        var value = Env(name);
        return name.Contains("_ALLOW_", StringComparison.Ordinal)
            ? string.Equals(value, "true", StringComparison.OrdinalIgnoreCase)
            : value is not null;
    }

    // ─── Live fixtures ([ApiKeyFact], [BulkChargeFact]) ──────────────────────
    // Names are constants so a fact attribute can name what it needs.

    public const string ApiKeyVar = "POSTY5_API_KEY";
    /// <summary><c>true</c> lets a fact that charges credits run (bulk create).</summary>
    public const string AllowChargesVar = "POSTY5_TEST_ALLOW_CHARGES";

    public static bool AllowCharges => string.Equals(Env(AllowChargesVar), "true", StringComparison.OrdinalIgnoreCase);

    // ─── Link + QR truth pass (LinkQrTruthPassFact) ──────────────────────────

    /// <summary><c>true</c> when the API under test carries the link-qr truth pass (S13 deep links, TP filters).</summary>
    public const string LinkQrTruthPassVar = "POSTY5_TEST_LINK_QR_TP";

    public static bool ApiHasLinkQrTruthPass => string.Equals(Env(LinkQrTruthPassVar), "true", StringComparison.OrdinalIgnoreCase);

    // ─── Link + QR visit analytics (LinkQrVisitAnalyticsFact) ────────────────

    /// <summary><c>true</c> when the API under test serves <c>GET /api/short-link|qr-code/{id}/analytics</c> (VA).</summary>
    public const string LinkQrVisitAnalyticsVar = "POSTY5_TEST_LINK_QR_VA";

    public static bool ApiHasLinkQrVisitAnalytics => string.Equals(Env(LinkQrVisitAnalyticsVar), "true", StringComparison.OrdinalIgnoreCase);

    // Store created resource IDs for cleanup
    public static class CreatedResources
    {
        public static List<string> QRCodes { get; } = new();
        public static List<string> ShortLinks { get; } = new();
        public static List<string> HtmlHostings { get; } = new();
        public static List<string> Workspaces { get; } = new();
        public static List<string> Posts { get; } = new();
    }
}
