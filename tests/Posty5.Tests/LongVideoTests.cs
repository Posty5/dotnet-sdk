using Xunit;
using Posty5.SocialPublisherPost;
using Posty5.SocialPublisherPost.Models;
using Posty5.Core.Http;
using Posty5.Core.Configuration;

namespace Posty5.Tests.Unit;

/// <summary>
/// Long video pricing, limits and argument validation.
///
/// Unlike <c>SocialPublisherPostClientTests</c>, which talks to the live API and
/// needs credentials, these run offline: they cover the pure calculations and
/// the guards that reject bad input before any request is made.
/// </summary>
public class LongVideoTests
{
    // ========================================================================
    // Pricing
    // ========================================================================

    [Fact]
    public void Constants_MatchTheApiContract()
    {
        Assert.Equal(3600, LongVideo.MaxDurationSeconds);
        Assert.Equal(300, LongVideo.CreditUnitSeconds);
    }

    [Theory]
    [InlineData(1, 1)]       // one second still costs a full unit
    [InlineData(299, 1)]
    [InlineData(300, 1)]     // exactly 5 minutes is still one unit
    [InlineData(301, 2)]     // one second over rolls into the next
    [InlineData(600, 2)]
    [InlineData(720, 3)]     // 12 minutes -> 3 units -> 150 credits
    [InlineData(900, 3)]     // 15 minutes is the last 3-unit duration
    [InlineData(901, 4)]
    [InlineData(3600, 12)]   // the ceiling -> 600 credits
    public void CreditUnits_ChargesAWholeUnitPerStartedFiveMinutes(int seconds, int expectedUnits)
    {
        Assert.Equal(expectedUnits, LongVideo.CreditUnits(seconds));
    }

    [Fact]
    public void CreditUnits_PricesTheWorkedExampleAt150Credits()
    {
        Assert.Equal(150, LongVideo.CreditUnits(12 * 60) * 50);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void CreditUnits_RefusesAnUnmeasuredDurationRatherThanQuotingFree(int seconds)
    {
        // A non-positive duration means the video was never probed. Throwing
        // makes that a loud bug instead of a silent free publish.
        Assert.Throws<ArgumentOutOfRangeException>(() => LongVideo.CreditUnits(seconds));
    }

    [Theory]
    [InlineData(1, true)]
    [InlineData(3599, true)]
    [InlineData(3600, true)]   // exactly 60 minutes is accepted
    [InlineData(3601, false)]  // 60m00s01 is refused at the boundary
    [InlineData(0, false)]
    [InlineData(-1, false)]
    public void IsDurationAllowed_HoldsTheBoundary(int seconds, bool allowed)
    {
        Assert.Equal(allowed, LongVideo.IsDurationAllowed(seconds));
    }

    // ========================================================================
    // Argument validation — must fail before any network call
    // ========================================================================

    private static SocialPublisherPostClient MakeClient() =>
        new(new Posty5HttpClient(new Posty5Options { ApiKey = "test-key", BaseUrl = "https://api.invalid" }));

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task GetLongVideoQuote_RequiresAVideoUrl(string videoUrl)
    {
        await Assert.ThrowsAsync<ArgumentException>(() => MakeClient().GetLongVideoQuoteAsync(videoUrl));
    }

    [Fact]
    public async Task PublishLongVideoToWorkspace_RequiresAWorkspaceId()
    {
        var ex = await Assert.ThrowsAsync<ArgumentException>(() =>
            MakeClient().PublishLongVideoToWorkspaceAsync("", "https://example.com/v.mp4",
                youtube: new YouTubeConfig { Title = "t", Description = "d" }));
        Assert.Contains("workspaceId", ex.Message);
    }

    [Fact]
    public async Task PublishLongVideoToWorkspace_RequiresAtLeastOnePlatformConfig()
    {
        var ex = await Assert.ThrowsAsync<ArgumentException>(() =>
            MakeClient().PublishLongVideoToWorkspaceAsync("ws_1", "https://example.com/v.mp4"));
        Assert.Contains("platform configuration", ex.Message);
    }

    [Fact]
    public async Task PublishLongVideoToAccount_RequiresAnAccountId()
    {
        var ex = await Assert.ThrowsAsync<ArgumentException>(() =>
            MakeClient().PublishLongVideoToAccountAsync("", "https://example.com/v.mp4",
                youtube: new YouTubeConfig { Title = "t", Description = "d" }));
        Assert.Contains("accountId", ex.Message);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task ReschedulePost_RequiresAnId(string id)
    {
        await Assert.ThrowsAsync<ArgumentException>(() => MakeClient().ReschedulePostAsync(id, "now"));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task DeletePost_RequiresAnId(string id)
    {
        await Assert.ThrowsAsync<ArgumentException>(() => MakeClient().DeletePostAsync(id));
    }

    [Fact]
    public async Task ReschedulePost_RejectsAScheduleItCannotInterpret()
    {
        // Anything that is not a DateTime or the string "now" would otherwise
        // be silently dropped, leaving the post at its original time.
        var ex = await Assert.ThrowsAsync<ArgumentException>(() => MakeClient().ReschedulePostAsync("post_1", 42));
        Assert.Contains("schedule", ex.Message);
    }

    // ========================================================================
    // Response models
    // ========================================================================

    [Fact]
    public void PublishLongVideoResult_DefaultsToAnEmptyRefusalList()
    {
        // Callers iterate RefusedTargets directly; a null would NRE on the
        // happy path, which is the common one.
        Assert.Empty(new PublishLongVideoResult().RefusedTargets);
    }

    [Fact]
    public void UploadProgress_ReportsAPercentageOnlyWhenTheTotalIsKnown()
    {
        Assert.Equal(50.0, new UploadProgress { BytesTransferred = 50, TotalBytes = 100 }.Percentage);
        // A non-seekable stream cannot report its length — no total, no percentage.
        Assert.Null(new UploadProgress { BytesTransferred = 50, TotalBytes = null }.Percentage);
        Assert.Null(new UploadProgress { BytesTransferred = 0, TotalBytes = 0 }.Percentage);
    }
}
