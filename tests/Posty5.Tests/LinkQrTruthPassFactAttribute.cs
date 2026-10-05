using Xunit;

namespace Posty5.Tests;

/// <summary>
/// A live short-link / QR fact that needs the API's link-qr truth pass (TP):
/// S13 deep links, landing page kept on update, the <c>pageInfo.title</c> and
/// QR <c>refId</c> filters. Reported as <b>skipped</b>, with the reason, until
/// <c>POSTY5_API_KEY</c> is set and <c>POSTY5_TEST_LINK_QR_TP</c> is <c>true</c> -
/// against an older API these facts fail for a reason the SDK cannot fix.
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public sealed class LinkQrTruthPassFactAttribute : FactAttribute
{
    public LinkQrTruthPassFactAttribute()
    {
        if (TestConfig.Env(TestConfig.ApiKeyVar) is null)
        {
            Skip = $"Live fact: {TestConfig.ApiKeyVar} is not set.";
        }
        else if (!TestConfig.ApiHasLinkQrTruthPass)
        {
            Skip = $"Needs the API with the link-qr truth pass; set {TestConfig.LinkQrTruthPassVar}=true once the API under test has it.";
        }
    }
}
