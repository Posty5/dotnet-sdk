using System.Text.Json;
using Posty5.Webhooks;
using Posty5.Webhooks.Models;
using Xunit;

namespace Posty5.Tests;

/// <summary>
/// <see cref="WebhookEndpointClient"/> routes pinned offline; one live read
/// needs POSTY5_API_KEY and an API with the webhook routes deployed.
/// </summary>
public class WebhookEndpointClientTests : IDisposable
{
    private readonly RecordingServer _server = new();

    public void Dispose() => _server.Dispose();

    [Fact]
    public async Task Create_SendsTheEndpoint_AndReadsTheSecretOnce()
    {
        _server.ResultJson = """{"endpoint":{"_id":"w1","url":"https://example.com/h","events":["short_link.visited"],"delivery":{"mode":"batch","windowSeconds":60},"enabled":true,"failureCount":0,"secretHint":"whsec_…abc","createdAt":"2026-10-06T00:00:00Z","updatedAt":"2026-10-06T00:00:00Z"},"secret":"whsec_abc"}""";
        using var http = _server.Http();
        var client = new WebhookEndpointClient(http);

        var created = await client.CreateAsync(new WebhookEndpointRequest
        {
            Url = "https://example.com/h",
            Events = new() { WebhookEventTypes.ShortLinkVisited },
            Delivery = new WebhookDeliverySettings { Mode = WebhookDeliveryMode.Batch, WindowSeconds = 60 }
        });

        Assert.Equal("whsec_abc", created.Secret);
        Assert.Equal("w1", created.Endpoint.Id);
        Assert.Equal(WebhookDeliveryMode.Batch, created.Endpoint.Delivery.Mode);
        var request = _server.Requests.Single();
        Assert.Equal(("POST", "/api/webhook-endpoints"), (request.Method, request.PathAndQuery));
        using var body = JsonDocument.Parse(request.Body);
        Assert.Equal("batch", body.RootElement.GetProperty("delivery").GetProperty("mode").GetString());
        Assert.Equal("short_link.visited", body.RootElement.GetProperty("events")[0].GetString());
    }

    [Fact]
    public async Task EveryRoute()
    {
        _server.ResultJson = """{"_id":"x","endpointId":"w1","status":"pending","attempt":1,"secret":"whsec_new","items":[],"pagination":{}}""";
        using var http = _server.Http();
        var client = new WebhookEndpointClient(http);

        await client.GetAsync("w1");
        await client.UpdateAsync("w1", new WebhookEndpointRequest { Enabled = true }, 0);
        await client.RotateSecretAsync("w1", 0);
        await client.SendTestAsync("w1");
        await client.ListDeliveriesAsync("w1");
        await client.RedeliverAsync("w1", "d1");
        await client.DeleteAsync("w1", 0);
        await client.ListAsync();

        Assert.Equal(new[]
        {
            ("GET", "/api/webhook-endpoints/w1"),
            ("PUT", "/api/webhook-endpoints/w1"),
            ("POST", "/api/webhook-endpoints/w1/rotate-secret"),
            ("POST", "/api/webhook-endpoints/w1/test"),
            ("GET", "/api/webhook-endpoints/w1/deliveries"),
            ("POST", "/api/webhook-endpoints/w1/deliveries/d1/redeliver"),
            ("DELETE", "/api/webhook-endpoints/w1"),
            ("GET", "/api/webhook-endpoints"),
        }, _server.Requests.Select(r => (r.Method, r.PathAndQuery)));
    }

    [Fact]
    public async Task ListDeliveries_SendsTheFilters()
    {
        _server.ResultJson = """{"items":[],"pagination":{}}""";
        using var http = _server.Http();

        await new WebhookEndpointClient(http).ListDeliveriesAsync("w1",
            new ListWebhookDeliveriesParams { Status = WebhookDeliveryStatus.Failed, EventType = WebhookEventTypes.ShortLinkVisited });

        var path = _server.Requests.Single().PathAndQuery;
        Assert.StartsWith("/api/webhook-endpoints/w1/deliveries?", path);
        Assert.Contains("status=failed", path);
        Assert.Contains("eventType=short_link.visited", path);
    }

    [Fact]
    public async Task EventTypes_Route()
    {
        _server.ResultJson = """[{"type":"short_link.visited"},{"type":"webhook.test"}]""";
        using var http = _server.Http();

        var types = await new WebhookEndpointClient(http).ListEventTypesAsync();

        Assert.Equal("/api/webhook-endpoints/event-types", _server.Requests.Single().PathAndQuery);
        Assert.Equal(2, types.Count);
    }

    [ApiKeyFact]
    public async Task Live_ListEventTypes()
    {
        using var http = TestConfig.CreateHttpClient();
        var types = await new WebhookEndpointClient(http).ListEventTypesAsync();
        Assert.Contains(types, t => t.Type == WebhookEventTypes.Test);
    }
}
