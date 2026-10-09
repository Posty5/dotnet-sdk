using Posty5.Webhooks;
using Posty5.Webhooks.Models;
using Xunit;

namespace Posty5.Tests;

/// <summary>Pure tests of <see cref="WebhookSignature.Verify(string, IDictionary{string, string}, string, TimeSpan?)"/>.</summary>
public class WebhookSignatureTests
{
    // The Standard Webhooks published test vector.
    private const string VectorSecret = "whsec_MfKQ9r8GKYqrTwjUPD8ILPZIo2LaLaSw";
    private const string VectorId = "msg_p5jXN8AQM9LWM0D4loKWxJek";
    private const string VectorTimestamp = "1614265330";
    private const string VectorSignature = "v1,g0hM9SsE+OTPJTGt/tmIKtSyZlE3uFJELVlNIOLJ1OE=";

    private static readonly DateTimeOffset VectorNow = DateTimeOffset.FromUnixTimeSeconds(1614265330);

    private const string Secret = "whsec_dGVzdC1zZWNyZXQtdGVzdC1zZWNyZXQtdGVzdC0xMg==";
    private const string OtherSecret = "whsec_b3RoZXItc2VjcmV0LW90aGVyLXNlY3JldC1vdGhlcg==";

    private const string VisitBody = """
        {"id":"evt_1","type":"short_link.visited","apiVersion":"2026-10-01","createdAt":"2026-10-04T12:00:00.000Z",
         "data":{"target":{"type":"shortLink","id":"66f0","code":"aB3xY9","name":"Spring sale"},
                 "visit":{"at":"2026-10-04T11:59:59.120Z","channel":"qr","country":"EG","isBot":false}}}
        """;

    private static Dictionary<string, string> Headers(string id, string timestamp, string signature) => new()
    {
        ["Webhook-Id"] = id,
        ["WEBHOOK-TIMESTAMP"] = timestamp,
        ["webhook-signature"] = signature
    };

    private static (Dictionary<string, string> Headers, DateTimeOffset Now) Signed(string body, params string[] secrets)
    {
        var now = DateTimeOffset.UtcNow;
        var ts = now.ToUnixTimeSeconds().ToString();
        var signature = string.Join(" ", secrets.Select(s => WebhookSignature.ComputeSignature(s, "msg_1", ts, body)));
        return (Headers("msg_1", ts, signature), now);
    }

    [Fact]
    public void ComputeSignature_MatchesThePublishedVector()
    {
        Assert.Equal(VectorSignature, WebhookSignature.ComputeSignature(VectorSecret, VectorId, VectorTimestamp, """{"test": 2432232314}"""));
    }

    [Fact]
    public void Verify_PublishedVector_PassesSignature_ThenReadsAnUnknownEvent()
    {
        var evt = WebhookSignature.Verify("""{"test": 2432232314}""", Headers(VectorId, VectorTimestamp, VectorSignature), VectorSecret, null, VectorNow);
        Assert.IsType<UnknownWebhookEvent>(evt);
    }

    [Fact]
    public void Verify_ReadsAVisitEvent_WithHeaderNamesInAnyCase()
    {
        var (headers, now) = Signed(VisitBody, Secret);

        var evt = Assert.IsType<ShortLinkVisitedEvent>(WebhookSignature.Verify(VisitBody, headers, Secret, null, now));
        Assert.Equal("evt_1", evt.Id);
        Assert.Equal("Spring sale", evt.Data.Target.Name);
        Assert.Equal("qr", evt.Data.Visit.Channel);
    }

    [Fact]
    public void Verify_AcceptsEitherSignatureDuringRotation()
    {
        var (headers, now) = Signed(VisitBody, OtherSecret, Secret);
        Assert.IsType<ShortLinkVisitedEvent>(WebhookSignature.Verify(VisitBody, headers, Secret, null, now));
        Assert.IsType<ShortLinkVisitedEvent>(WebhookSignature.Verify(VisitBody, headers, OtherSecret, null, now));
    }

    [Fact]
    public void Verify_RefusesATamperedBody()
    {
        var (headers, now) = Signed(VisitBody, Secret);
        var error = Assert.Throws<WebhookSignatureException>(() =>
            WebhookSignature.Verify(VisitBody.Replace("EG", "US"), headers, Secret, null, now));
        Assert.Equal(WebhookSignatureFailureReason.NoMatchingSignature, error.Reason);
    }

    [Fact]
    public void Verify_RefusesTheWrongSecret()
    {
        var (headers, now) = Signed(VisitBody, Secret);
        var error = Assert.Throws<WebhookSignatureException>(() => WebhookSignature.Verify(VisitBody, headers, OtherSecret, null, now));
        Assert.Equal(WebhookSignatureFailureReason.NoMatchingSignature, error.Reason);
    }

    [Fact]
    public void Verify_RefusesAStaleTimestamp()
    {
        var (headers, now) = Signed(VisitBody, Secret);
        var error = Assert.Throws<WebhookSignatureException>(() =>
            WebhookSignature.Verify(VisitBody, headers, Secret, null, now.AddMinutes(6)));
        Assert.Equal(WebhookSignatureFailureReason.TimestampOutOfRange, error.Reason);

        Assert.IsType<ShortLinkVisitedEvent>(WebhookSignature.Verify(VisitBody, headers, Secret, TimeSpan.FromMinutes(10), now.AddMinutes(6)));
    }

    [Fact]
    public void Verify_RefusesMissingHeaders()
    {
        var (headers, now) = Signed(VisitBody, Secret);
        headers.Remove("webhook-signature");
        var error = Assert.Throws<WebhookSignatureException>(() => WebhookSignature.Verify(VisitBody, headers, Secret, null, now));
        Assert.Equal(WebhookSignatureFailureReason.MissingHeaders, error.Reason);
    }

    [Fact]
    public void Verify_RefusesASignedBodyThatIsNotAnEvent()
    {
        var (headers, now) = Signed("not json", Secret);
        var error = Assert.Throws<WebhookSignatureException>(() => WebhookSignature.Verify("not json", headers, Secret, null, now));
        Assert.Equal(WebhookSignatureFailureReason.InvalidJson, error.Reason);
    }

    [Fact]
    public void Verify_ReadsABatchOfMixedEvents()
    {
        const string body = """
            {"id":"evt_b","type":"batch","apiVersion":"2026-10-01","createdAt":"2026-10-04T12:00:00Z","data":{"events":[
              {"id":"evt_1","type":"qr_code.scanned","createdAt":"2026-10-04T12:00:00Z","data":{"target":{"type":"qrCode","id":"q"},"visit":{"at":"2026-10-04T12:00:00Z"}}},
              {"id":"evt_2","type":"short_link.visits_milestone","createdAt":"2026-10-04T12:00:00Z","data":{"target":{"type":"shortLink","id":"s"},"milestone":{"metric":"visits","value":1000,"reachedAt":"2026-10-04T12:00:00Z"}}}
            ]}}
            """;
        var (headers, now) = Signed(body, Secret);

        var batch = Assert.IsType<WebhookBatchEvent>(WebhookSignature.Verify(body, headers, Secret, null, now));
        Assert.IsType<QrCodeScannedEvent>(batch.Data.Events[0]);
        Assert.Equal(1000, Assert.IsType<ShortLinkVisitsMilestoneEvent>(batch.Data.Events[1]).Data.Milestone.Value);
    }
}
