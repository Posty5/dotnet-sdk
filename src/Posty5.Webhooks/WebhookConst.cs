namespace Posty5.Webhooks;

/// <summary>
/// Fixed values of the webhook API and of the Standard Webhooks signature scheme
/// Posty5 signs deliveries with.
/// </summary>
public static class WebhookConst
{
    /// <summary>The API path of the endpoint resource.</summary>
    public const string BasePath = "/api/webhook-endpoints";

    /// <summary>Delivery id header (<c>msg_&lt;deliveryId&gt;</c>, stable across retries — dedupe on it).</summary>
    public const string IdHeader = "webhook-id";

    /// <summary>Unix-seconds timestamp header.</summary>
    public const string TimestampHeader = "webhook-timestamp";

    /// <summary>Signature header: space-separated <c>v1,&lt;base64&gt;</c> entries.</summary>
    public const string SignatureHeader = "webhook-signature";

    /// <summary>Event type header Posty5 adds.</summary>
    public const string EventHeader = "Posty5-Event";

    /// <summary>Prefix of a signing secret.</summary>
    public const string SecretPrefix = "whsec_";

    /// <summary>Version prefix of a signature entry.</summary>
    public const string SignatureVersion = "v1";

    /// <summary>How far a delivery's timestamp may be from now (5 minutes).</summary>
    public static readonly TimeSpan DefaultTolerance = TimeSpan.FromMinutes(5);
}

/// <summary>Event type names (<c>GET /api/webhook-endpoints/event-types</c> is the live catalogue).</summary>
public static class WebhookEventTypes
{
    /// <summary>A short link was visited (also a scan of its QR image, <c>visit.channel = "qr"</c>).</summary>
    public const string ShortLinkVisited = "short_link.visited";
    /// <summary>A short link's visit counter reached a configured value.</summary>
    public const string ShortLinkVisitsMilestone = "short_link.visits_milestone";
    /// <summary>A QR code that points at a Posty5 URL was scanned. Static QR codes produce no events.</summary>
    public const string QrCodeScanned = "qr_code.scanned";
    /// <summary>A QR code's scan counter reached a configured value.</summary>
    public const string QrCodeScansMilestone = "qr_code.scans_milestone";
    /// <summary>Sent by the Send test action.</summary>
    public const string Test = "webhook.test";
    /// <summary>The wrapper of a batched delivery.</summary>
    public const string Batch = "batch";
}
