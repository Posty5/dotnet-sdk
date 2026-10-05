using Xunit;

namespace Posty5.Tests;

/// <summary>
/// A live, read-only fact that needs only an API key. Reported as
/// <b>skipped</b>, naming the variable, when <c>POSTY5_API_KEY</c> is not set —
/// the same mechanism as <see cref="StoreFixtureFactAttribute"/>, without the
/// store fixture.
/// </summary>
/// <remarks>
/// It calls the API at <c>POSTY5_BASE_URL</c> (default production). The routes
/// these facts cover exist only on an API that has the mcp-server wave-1
/// endpoints deployed; against an older API they fail with 404.
/// </remarks>
[AttributeUsage(AttributeTargets.Method)]
public sealed class ApiKeyFactAttribute : FactAttribute
{
    public ApiKeyFactAttribute()
    {
        if (!TestConfig.IsSet(TestConfig.ApiKeyVar))
        {
            Skip = $"Live API key not set: {TestConfig.ApiKeyVar}.";
        }
    }
}
