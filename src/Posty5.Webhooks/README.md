# Posty5.Webhooks

Manage Posty5 webhook endpoints and verify the deliveries they receive.

```bash
dotnet add package Posty5.Webhooks
```

Posty5 signs every delivery with [Standard Webhooks](https://www.standardwebhooks.com/):
`webhook-id`, `webhook-timestamp` and `webhook-signature` headers, HMAC-SHA256 over
`{id}.{timestamp}.{rawBody}`. Any Standard Webhooks library can verify them; this
package does it with `WebhookSignature.Verify`.

## Register an endpoint

```csharp
using Posty5.Core.Configuration;
using Posty5.Core.Http;
using Posty5.Webhooks;
using Posty5.Webhooks.Models;

var http = new Posty5HttpClient(new Posty5Options { ApiKey = "your-api-key" });
var webhooks = new WebhookEndpointClient(http);

var created = await webhooks.CreateAsync(new WebhookEndpointRequest
{
    Url = "https://example.com/posty5/webhooks",
    Events = new() { WebhookEventTypes.ShortLinkVisited, WebhookEventTypes.QrCodeScanned },
});

// Store this now: it is never returned again.
Console.WriteLine(created.Secret);

await webhooks.SendTestAsync(created.Endpoint.Id);
```

Other calls: `ListAsync`, `GetAsync`, `UpdateAsync` (`Enabled = true` re-enables a
disabled endpoint), `DeleteAsync`, `RotateSecretAsync` (the old secret keeps signing
for 24 h), `ListDeliveriesAsync` (optional `ListWebhookDeliveriesParams { Status, EventType }`), `RedeliverAsync` (same `webhook-id`),
`ListEventTypesAsync`.

Endpoints must be HTTPS and must not resolve to a private address. Visit and scan
events need a plan with visit webhooks. A **static** QR code encodes its content
directly and never reaches Posty5, so it produces no scan events.

## Receive events in ASP.NET Core

Verify against the **raw body**, before any model binding — a re-serialised object
does not match the signature.

```csharp
using Posty5.Webhooks;
using Posty5.Webhooks.Models;

app.MapPost("/posty5/webhooks", async (HttpRequest request) =>
{
    request.EnableBuffering();
    using var reader = new StreamReader(request.Body, leaveOpen: true);
    var body = await reader.ReadToEndAsync();

    var headers = request.Headers.ToDictionary(h => h.Key, h => h.Value.ToString());

    WebhookEvent evt;
    try
    {
        evt = WebhookSignature.Verify(body, headers, builder.Configuration["Posty5:WebhookSecret"]!);
    }
    catch (WebhookSignatureException ex)
    {
        return Results.BadRequest(ex.Reason.ToString());
    }

    switch (evt)
    {
        case ShortLinkVisitedEvent visited:
            Console.WriteLine($"{visited.Data.Target.Name} visited from {visited.Data.Visit.Country}");
            break;
        case WebhookBatchEvent batch:
            foreach (var inner in batch.Data.Events) { /* … */ }
            break;
    }

    return Results.Ok();
});
```

Answer 2xx quickly and do slow work afterwards; the delivery times out after 10 s and
is retried (30 s, 2 min, 10 min, 1 h, 6 h, 24 h). The same `webhook-id` arrives on
every retry — dedupe on it. Answering **410** disables the endpoint.

`Verify` throws `WebhookSignatureException` with a `Reason`: `MissingHeaders`,
`TimestampOutOfRange` (default tolerance 5 minutes), `NoMatchingSignature` or
`InvalidJson`.
