using Xunit;

namespace Posty5.Tests;

/// <summary>
/// A live short-link / QR fact that needs the API's visit-analytics endpoints
/// (VA, <c>GET /api/short-link|qr-code/{id}/analytics</c>). Reported as
/// <b>skipped</b>, with the reason, until <c>POSTY5_API_KEY</c> is set and
/// <c>POSTY5_TEST_LINK_QR_VA</c> is <c>true</c> - an API without the endpoint
/// answers 404 for a reason the SDK cannot fix.
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public sealed class LinkQrVisitAnalyticsFactAttribute : FactAttribute
{
    public LinkQrVisitAnalyticsFactAttribute()
    {
        if (TestConfig.Env(TestConfig.ApiKeyVar) is null)
        {
            Skip = $"Live fact: {TestConfig.ApiKeyVar} is not set.";
        }
        else if (!TestConfig.ApiHasLinkQrVisitAnalytics)
        {
            Skip = $"Needs the API with link + QR visit analytics; set {TestConfig.LinkQrVisitAnalyticsVar}=true once the API under test has it.";
        }
    }
}
