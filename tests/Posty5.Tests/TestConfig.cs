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
        var apiKey = Environment.GetEnvironmentVariable("POSTY5_API_KEY", EnvironmentVariableTarget.User);
        var baseUrl = Environment.GetEnvironmentVariable("POSTY5_BASE_URL",EnvironmentVariableTarget.User) ?? "https://api.posty5.com";

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
