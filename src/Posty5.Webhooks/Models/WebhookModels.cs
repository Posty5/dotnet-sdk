using System.Text.Json;
using System.Text.Json.Serialization;
using Posty5.Core.Converts;

namespace Posty5.Webhooks.Models;

#region Endpoints

/// <summary>How an endpoint receives events.</summary>
[JsonConverter(typeof(CamelCaseEnumConverter<WebhookDeliveryMode>))]
public enum WebhookDeliveryMode
{
    /// <summary>One request per event.</summary>
    Each,
    /// <summary>One <c>batch</c> request per window.</summary>
    Batch
}

/// <summary>Why an endpoint is disabled.</summary>
[JsonConverter(typeof(CamelCaseEnumConverter<WebhookDisabledReason>))]
public enum WebhookDisabledReason
{
    /// <summary>20 consecutive failures over at least 24 h.</summary>
    ConsecutiveFailures,
    /// <summary>The endpoint answered 410 Gone.</summary>
    Gone,
    /// <summary>Disabled by its owner.</summary>
    User,
    /// <summary>The plan no longer includes a subscribed event.</summary>
    PlanDowngrade
}

/// <summary>Delivery settings.</summary>
public class WebhookDeliverySettings
{
    /// <summary>Each event or batched.</summary>
    public WebhookDeliveryMode Mode { get; set; } = WebhookDeliveryMode.Each;

    /// <summary>Batch window: 60, 300 or 3600 seconds.</summary>
    public int? WindowSeconds { get; set; }
}

/// <summary>Narrows an endpoint to specific records.</summary>
public class WebhookTargets
{
    /// <summary>Only these short links.</summary>
    public List<string>? ShortLinkIds { get; set; }

    /// <summary>Only these QR codes.</summary>
    public List<string>? QrCodeIds { get; set; }
}

/// <summary>A registered endpoint (<c>IWebhookEndpoint</c>). The secret is never returned after creation.</summary>
public class WebhookEndpoint
{
    /// <summary>Endpoint id.</summary>
    [JsonPropertyName("_id")]
    public string Id { get; set; } = string.Empty;
    /// <summary>Owner.</summary>
    public string? UserId { get; set; }
    /// <summary>API key that created it, if any.</summary>
    public string? ApiKeyId { get; set; }
    /// <summary>HTTPS URL.</summary>
    public string Url { get; set; } = string.Empty;
    /// <summary>Description.</summary>
    public string? Description { get; set; }
    /// <summary>Subscribed event types (<see cref="WebhookEventTypes"/>).</summary>
    public List<string> Events { get; set; } = new();
    /// <summary>Record filter.</summary>
    public WebhookTargets? Targets { get; set; }
    /// <summary>Whether bot visits are sent (default false).</summary>
    public bool IncludeBots { get; set; }
    /// <summary>Milestone values for <c>*_milestone</c> events.</summary>
    public List<long>? Milestones { get; set; }
    /// <summary>Delivery settings.</summary>
    public WebhookDeliverySettings Delivery { get; set; } = new();
    /// <summary>Whether deliveries are sent.</summary>
    public bool Enabled { get; set; }
    /// <summary>Why it is disabled.</summary>
    public WebhookDisabledReason? DisabledReason { get; set; }
    /// <summary>Consecutive failed deliveries.</summary>
    public int FailureCount { get; set; }
    /// <summary>Events dropped by the daily or pending caps.</summary>
    public int? DroppedEvents { get; set; }
    /// <summary>Last delivery attempt.</summary>
    public DateTime? LastDeliveryAt { get; set; }
    /// <summary>Last successful delivery.</summary>
    public DateTime? LastSuccessAt { get; set; }
    /// <summary>Masked secret, e.g. <c>whsec_…k9w</c>.</summary>
    public string? SecretHint { get; set; }
    /// <summary>Origin label.</summary>
    public string? CreatedFrom { get; set; }
    /// <summary>Created at.</summary>
    public DateTime CreatedAt { get; set; }
    /// <summary>Updated at.</summary>
    public DateTime UpdatedAt { get; set; }
}

/// <summary>The create answer <c>{ endpoint, secret }</c>: the endpoint plus its signing secret, shown this once.</summary>
public class CreateWebhookEndpointResponse
{
    /// <summary>The new endpoint.</summary>
    public WebhookEndpoint Endpoint { get; set; } = new();
    /// <summary><c>whsec_…</c> — store it; it is never returned again.</summary>
    public string Secret { get; set; } = string.Empty;
}

/// <summary>The rotate answer <c>{ secret }</c>.</summary>
public class RotateWebhookSecretResponse
{
    /// <summary>The new secret. The old one keeps signing for 24 h alongside it.</summary>
    public string Secret { get; set; } = string.Empty;
}

/// <summary>Fields to create or update an endpoint.</summary>
public class WebhookEndpointRequest
{
    /// <summary>HTTPS URL (http and private addresses are refused).</summary>
    public string? Url { get; set; }
    /// <summary>Description.</summary>
    public string? Description { get; set; }
    /// <summary>Event types (<see cref="WebhookEventTypes"/>).</summary>
    public List<string>? Events { get; set; }
    /// <summary>Record filter.</summary>
    public WebhookTargets? Targets { get; set; }
    /// <summary>Send bot visits too.</summary>
    public bool? IncludeBots { get; set; }
    /// <summary>Up to 10 milestone values.</summary>
    public List<long>? Milestones { get; set; }
    /// <summary>Delivery settings.</summary>
    public WebhookDeliverySettings? Delivery { get; set; }
    /// <summary>Update only: <c>true</c> re-enables a disabled endpoint, <c>false</c> disables it.</summary>
    public bool? Enabled { get; set; }
}

/// <summary>One event type of the catalogue.</summary>
public class WebhookEventTypeInfo
{
    /// <summary>Type name.</summary>
    public string Type { get; set; } = string.Empty;
    /// <summary>What fires it.</summary>
    public string? Description { get; set; }
    /// <summary>i18n key of <see cref="Description"/>.</summary>
    public string? DescriptionKey { get; set; }
    /// <summary>Plan feature it needs, if any (the API's <c>gate</c>).</summary>
    public string? Gate { get; set; }
    /// <summary>Whether your plan allows it.</summary>
    public bool? Allowed { get; set; }
    /// <summary>True for the <c>*_milestone</c> events (they need <c>Milestones</c> on the endpoint).</summary>
    public bool? IsMilestone { get; set; }
    /// <summary>A sample payload of the event.</summary>
    public JsonElement? SamplePayload { get; set; }
}

/// <summary>State of a delivery.</summary>
[JsonConverter(typeof(CamelCaseEnumConverter<WebhookDeliveryStatus>))]
public enum WebhookDeliveryStatus
{
    /// <summary>Waiting or retrying.</summary>
    Pending,
    /// <summary>Delivered (2xx).</summary>
    Succeeded,
    /// <summary>Last attempt failed; a retry is scheduled.</summary>
    Failed,
    /// <summary>Retries exhausted.</summary>
    Abandoned
}

/// <summary>Filters of <c>ListDeliveriesAsync</c>.</summary>
public class ListWebhookDeliveriesParams
{
    /// <summary>Only deliveries in this state.</summary>
    public WebhookDeliveryStatus? Status { get; set; }
    /// <summary>Only this event type (e.g. <see cref="WebhookEventTypes.ShortLinkVisited"/>).</summary>
    public string? EventType { get; set; }
}

/// <summary>One logged delivery (kept 30 days).</summary>
public class WebhookDelivery
{
    /// <summary>Delivery id.</summary>
    [JsonPropertyName("_id")]
    public string Id { get; set; } = string.Empty;
    /// <summary>Endpoint.</summary>
    public string EndpointId { get; set; } = string.Empty;
    /// <summary>Event id.</summary>
    public string? EventId { get; set; }
    /// <summary>Event type.</summary>
    public string? EventType { get; set; }
    /// <summary>The <c>webhook-id</c> header (<c>msg_&lt;first delivery id&gt;</c>), stable across retries and redeliveries.</summary>
    public string? MessageId { get; set; }
    /// <summary>Set on a redelivery: the delivery it repeats.</summary>
    public string? RedeliveryOf { get; set; }
    /// <summary>The body sent (omitted by the deliveries list).</summary>
    public JsonElement? Payload { get; set; }
    /// <summary>Attempts made.</summary>
    public int Attempt { get; set; }
    /// <summary>State.</summary>
    public WebhookDeliveryStatus Status { get; set; }
    /// <summary>Your endpoint's HTTP status.</summary>
    public int? ResponseStatus { get; set; }
    /// <summary>First 1 KB of your endpoint's answer.</summary>
    public string? ResponseSnippet { get; set; }
    /// <summary>Duration of the last attempt.</summary>
    public int? DurationMs { get; set; }
    /// <summary>Failure reason.</summary>
    public string? Error { get; set; }
    /// <summary>Next retry.</summary>
    public DateTime? NextAttemptAt { get; set; }
    /// <summary>Created at.</summary>
    public DateTime CreatedAt { get; set; }
}

#endregion

#region Events

/// <summary>
/// A received event. Concrete types: <see cref="ShortLinkVisitedEvent"/>,
/// <see cref="QrCodeScannedEvent"/>, <see cref="ShortLinkVisitsMilestoneEvent"/>,
/// <see cref="QrCodeScansMilestoneEvent"/>, <see cref="WebhookTestEvent"/>,
/// <see cref="WebhookBatchEvent"/>, and <see cref="UnknownWebhookEvent"/> for a
/// type this SDK version does not know yet.
/// </summary>
[JsonConverter(typeof(WebhookEventConverter))]
public abstract class WebhookEvent
{
    /// <summary><c>evt_…</c></summary>
    public string Id { get; set; } = string.Empty;
    /// <summary>Event type.</summary>
    public string Type { get; set; } = string.Empty;
    /// <summary>Payload version, e.g. <c>2026-10-01</c>.</summary>
    public string? ApiVersion { get; set; }
    /// <summary>When the event happened.</summary>
    public DateTime CreatedAt { get; set; }
}

/// <summary>The record an event is about.</summary>
public class WebhookEventTarget
{
    /// <summary><c>shortLink</c> or <c>qrCode</c>.</summary>
    public string Type { get; set; } = string.Empty;
    /// <summary>Record id.</summary>
    public string Id { get; set; } = string.Empty;
    /// <summary>Short code.</summary>
    public string? Code { get; set; }
    /// <summary>Posty5 URL.</summary>
    public string? Url { get; set; }
    /// <summary>Name.</summary>
    public string? Name { get; set; }
    /// <summary>Destination.</summary>
    public string? DestinationUrl { get; set; }
    /// <summary>Tag.</summary>
    public string? Tag { get; set; }
    /// <summary>Your reference.</summary>
    public string? RefId { get; set; }
}

/// <summary>One visit or scan. Never carries an IP, visitor hash, user agent or full referrer.</summary>
public class WebhookVisit
{
    /// <summary>When.</summary>
    public DateTime At { get; set; }
    /// <summary><c>link</c> or <c>qr</c>.</summary>
    public string? Channel { get; set; }
    /// <summary>ISO country code.</summary>
    public string? Country { get; set; }
    /// <summary>Device type.</summary>
    public string? DeviceType { get; set; }
    /// <summary>OS.</summary>
    public string? Os { get; set; }
    /// <summary>Browser.</summary>
    public string? Browser { get; set; }
    /// <summary>Referrer host only.</summary>
    public string? ReferrerHost { get; set; }
    /// <summary>Language.</summary>
    public string? Language { get; set; }
    /// <summary>Bot visit.</summary>
    public bool IsBot { get; set; }
    /// <summary>What the landing did.</summary>
    public string? Outcome { get; set; }
    /// <summary>Routing rule id.</summary>
    public string? RuleId { get; set; }
    /// <summary>A/B variant id.</summary>
    public string? VariantId { get; set; }
    /// <summary>Custom domain id.</summary>
    public string? DomainId { get; set; }
}

/// <summary>A reached milestone.</summary>
public class WebhookMilestone
{
    /// <summary><c>visits</c>.</summary>
    public string Metric { get; set; } = string.Empty;
    /// <summary>The value reached.</summary>
    public long Value { get; set; }
    /// <summary>When.</summary>
    public DateTime ReachedAt { get; set; }
}

/// <summary>Data of a visit or scan event.</summary>
public class WebhookVisitData
{
    /// <summary>The record.</summary>
    public WebhookEventTarget Target { get; set; } = new();
    /// <summary>The visit.</summary>
    public WebhookVisit Visit { get; set; } = new();
}

/// <summary>Data of a milestone event.</summary>
public class WebhookMilestoneData
{
    /// <summary>The record.</summary>
    public WebhookEventTarget Target { get; set; } = new();
    /// <summary>The milestone.</summary>
    public WebhookMilestone Milestone { get; set; } = new();
}

/// <summary>Data of a batch.</summary>
public class WebhookBatchData
{
    /// <summary>Up to 500 events.</summary>
    public List<WebhookEvent> Events { get; set; } = new();
}

/// <summary><c>short_link.visited</c></summary>
public sealed class ShortLinkVisitedEvent : WebhookEvent
{
    /// <summary>Payload.</summary>
    public WebhookVisitData Data { get; set; } = new();
}

/// <summary><c>qr_code.scanned</c></summary>
public sealed class QrCodeScannedEvent : WebhookEvent
{
    /// <summary>Payload.</summary>
    public WebhookVisitData Data { get; set; } = new();
}

/// <summary><c>short_link.visits_milestone</c></summary>
public sealed class ShortLinkVisitsMilestoneEvent : WebhookEvent
{
    /// <summary>Payload.</summary>
    public WebhookMilestoneData Data { get; set; } = new();
}

/// <summary><c>qr_code.scans_milestone</c></summary>
public sealed class QrCodeScansMilestoneEvent : WebhookEvent
{
    /// <summary>Payload.</summary>
    public WebhookMilestoneData Data { get; set; } = new();
}

/// <summary><c>webhook.test</c> — sample data.</summary>
public sealed class WebhookTestEvent : WebhookEvent
{
    /// <summary>Payload as sent.</summary>
    public JsonElement Data { get; set; }
}

/// <summary><c>batch</c> — the events of one window.</summary>
public sealed class WebhookBatchEvent : WebhookEvent
{
    /// <summary>Payload.</summary>
    public WebhookBatchData Data { get; set; } = new();
}

/// <summary>An event type this SDK version does not know.</summary>
public sealed class UnknownWebhookEvent : WebhookEvent
{
    /// <summary>Payload as sent.</summary>
    public JsonElement Data { get; set; }
}

/// <summary>Reads an event as its concrete type by its <c>type</c> field, wherever that field sits.</summary>
public sealed class WebhookEventConverter : JsonConverter<WebhookEvent>
{
    private static readonly Dictionary<string, Type> Types = new()
    {
        [WebhookEventTypes.ShortLinkVisited] = typeof(ShortLinkVisitedEvent),
        [WebhookEventTypes.QrCodeScanned] = typeof(QrCodeScannedEvent),
        [WebhookEventTypes.ShortLinkVisitsMilestone] = typeof(ShortLinkVisitsMilestoneEvent),
        [WebhookEventTypes.QrCodeScansMilestone] = typeof(QrCodeScansMilestoneEvent),
        [WebhookEventTypes.Test] = typeof(WebhookTestEvent),
        [WebhookEventTypes.Batch] = typeof(WebhookBatchEvent),
    };

    /// <inheritdoc />
    public override WebhookEvent? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using var doc = JsonDocument.ParseValue(ref reader);
        var root = doc.RootElement;
        if (root.ValueKind != JsonValueKind.Object)
            throw new JsonException("A webhook event must be a JSON object.");

        var type = root.TryGetProperty("type", out var t) && t.ValueKind == JsonValueKind.String ? t.GetString() : null;
        var concrete = type != null && Types.TryGetValue(type, out var known) ? known : typeof(UnknownWebhookEvent);
        return (WebhookEvent?)root.Deserialize(concrete, options);
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, WebhookEvent value, JsonSerializerOptions options)
        => JsonSerializer.Serialize(writer, value, value.GetType(), options);
}

#endregion
