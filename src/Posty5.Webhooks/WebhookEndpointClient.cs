using Posty5.Core.Configuration;
using Posty5.Core.Exceptions;
using Posty5.Core.Http;
using Posty5.Core.Models;
using Posty5.Webhooks.Models;

namespace Posty5.Webhooks;

/// <summary>
/// Manages webhook endpoints (<c>/api/webhook-endpoints</c>): register an HTTPS
/// URL, pick events, read deliveries, redeliver. Receiving events needs the
/// visit-webhooks feature on your plan (403 otherwise); <c>webhook.test</c> is free.
/// </summary>
public class WebhookEndpointClient
{
    private readonly Posty5HttpClient _http;

    /// <summary>Creates the client.</summary>
    /// <param name="httpClient">HTTP client instance from Posty5.Core</param>
    public WebhookEndpointClient(Posty5HttpClient httpClient)
    {
        _http = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    }

    /// <summary>Lists your endpoints.</summary>
    public async Task<PaginationResponse<WebhookEndpoint>> ListAsync(PaginationParams? pagination = null, CancellationToken cancellationToken = default)
    {
        var response = await _http.GetAsync<PaginationResponse<WebhookEndpoint>>(WebhookConst.BasePath, PageQuery(pagination), cancellationToken);
        return response.Result ?? new PaginationResponse<WebhookEndpoint>();
    }

    /// <summary>Reads one endpoint.</summary>
    public async Task<WebhookEndpoint> GetAsync(string id, CancellationToken cancellationToken = default)
    {
        var response = await _http.GetAsync<WebhookEndpoint>(EndpointPath(id), cancellationToken: cancellationToken);
        return response.Result ?? throw new Posty5Exception("The API answered no webhook endpoint.");
    }

    /// <summary>Registers an endpoint. The answer's <see cref="CreateWebhookEndpointResponse.Secret"/> is shown only now.</summary>
    public async Task<CreateWebhookEndpointResponse> CreateAsync(WebhookEndpointRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var body = new
        {
            request.Url,
            request.Description,
            request.Events,
            request.Targets,
            request.IncludeBots,
            request.Milestones,
            request.Delivery,
            createdFrom = _http.ResolveCreatedFrom(CreatedFromDefaults.Package)
        };
        var response = await _http.PostAsync<CreateWebhookEndpointResponse>(WebhookConst.BasePath, body, cancellationToken);
        return response.Result ?? throw new Posty5Exception("The API answered no webhook endpoint.");
    }

    /// <summary>Updates an endpoint; <see cref="WebhookEndpointRequest.Enabled"/> = true re-enables it.</summary>
    public async Task<WebhookEndpoint> UpdateAsync(string id, WebhookEndpointRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var response = await _http.PutAsync<WebhookEndpoint>(EndpointPath(id), request, cancellationToken);
        return response.Result ?? throw new Posty5Exception("The API answered no webhook endpoint.");
    }

    /// <summary>Deletes an endpoint.</summary>
    public async Task DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        await _http.DeleteAsync<object>(EndpointPath(id), cancellationToken);
    }

    /// <summary>Issues a new secret; the old one keeps signing alongside it for 24 h.</summary>
    public async Task<RotateWebhookSecretResponse> RotateSecretAsync(string id, CancellationToken cancellationToken = default)
    {
        var response = await _http.PostAsync<RotateWebhookSecretResponse>($"{EndpointPath(id)}/rotate-secret", new { }, cancellationToken);
        return response.Result ?? throw new Posty5Exception("The API answered no secret.");
    }

    /// <summary>Queues one <c>webhook.test</c> delivery to the endpoint.</summary>
    public async Task<WebhookDelivery> SendTestAsync(string id, CancellationToken cancellationToken = default)
    {
        var response = await _http.PostAsync<WebhookDelivery>($"{EndpointPath(id)}/test", new { }, cancellationToken);
        return response.Result ?? throw new Posty5Exception("The API answered no delivery.");
    }

    /// <summary>Lists an endpoint's deliveries of the last 30 days, newest first.</summary>
    public async Task<PaginationResponse<WebhookDelivery>> ListDeliveriesAsync(
        string id,
        ListWebhookDeliveriesParams? filter = null,
        PaginationParams? pagination = null,
        CancellationToken cancellationToken = default)
    {
        var query = PageQuery(pagination) ?? new Dictionary<string, object?>();
        if (filter?.Status is { } status) query["status"] = StatusName(status);
        if (!string.IsNullOrEmpty(filter?.EventType)) query["eventType"] = filter!.EventType;
        var response = await _http.GetAsync<PaginationResponse<WebhookDelivery>>(
            $"{EndpointPath(id)}/deliveries", query.Count > 0 ? query : null, cancellationToken);
        return response.Result ?? new PaginationResponse<WebhookDelivery>();
    }

    /// <summary>Sends a logged delivery again, with the same <c>webhook-id</c>.</summary>
    public async Task<WebhookDelivery> RedeliverAsync(string id, string deliveryId, CancellationToken cancellationToken = default)
    {
        RequireId(deliveryId, nameof(deliveryId));
        var response = await _http.PostAsync<WebhookDelivery>(
            $"{EndpointPath(id)}/deliveries/{Uri.EscapeDataString(deliveryId)}/redeliver", new { }, cancellationToken);
        return response.Result ?? throw new Posty5Exception("The API answered no delivery.");
    }

    /// <summary>The event catalogue.</summary>
    public async Task<List<WebhookEventTypeInfo>> ListEventTypesAsync(CancellationToken cancellationToken = default)
    {
        var response = await _http.GetAsync<List<WebhookEventTypeInfo>>($"{WebhookConst.BasePath}/event-types", cancellationToken: cancellationToken);
        return response.Result ?? new List<WebhookEventTypeInfo>();
    }

    private static string StatusName(WebhookDeliveryStatus status) => status switch
    {
        WebhookDeliveryStatus.Pending => "pending",
        WebhookDeliveryStatus.Succeeded => "succeeded",
        WebhookDeliveryStatus.Failed => "failed",
        _ => "abandoned"
    };

    private static string EndpointPath(string id)
    {
        RequireId(id, nameof(id));
        return $"{WebhookConst.BasePath}/{Uri.EscapeDataString(id)}";
    }

    private static void RequireId(string id, string name)
    {
        if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException("An id is required.", name);
    }

    private static Dictionary<string, object?>? PageQuery(PaginationParams? pagination)
    {
        if (pagination == null) return null;
        var query = new Dictionary<string, object?> { ["pageSize"] = pagination.PageSize };
        if (!string.IsNullOrEmpty(pagination.Cursor)) query["cursor"] = pagination.Cursor;
        return query;
    }
}
