using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using Posty5.Core.Configuration;
using Posty5.Core.Exceptions;
using Posty5.Core.Http;
using Posty5.Store;
using Posty5.Store.Clients;
using Posty5.Store.Models;
using Xunit;

namespace Posty5.Tests.Store;

/// <summary>
/// A local HTTP server the real <see cref="Posty5HttpClient"/> is pointed at. It
/// records each request and answers with the API's envelope, so routes, verbs,
/// query strings and bodies are pinned without the network.
/// </summary>
internal sealed class RecordingServer : IDisposable
{
    private readonly HttpListener _listener = new();
    private readonly CancellationTokenSource _stop = new();

    public List<(string Method, string PathAndQuery, string Body)> Requests { get; } = new();
    public string ResultJson { get; set; } = "{}";
    public HttpStatusCode Status { get; set; } = HttpStatusCode.OK;
    public string BaseUrl { get; }

    public RecordingServer()
    {
        var probe = new TcpListener(IPAddress.Loopback, 0);
        probe.Start();
        var port = ((IPEndPoint)probe.LocalEndpoint).Port;
        probe.Stop();

        BaseUrl = $"http://127.0.0.1:{port}";
        _listener.Prefixes.Add($"{BaseUrl}/");
        _listener.Start();
        _ = Task.Run(LoopAsync);
    }

    private async Task LoopAsync()
    {
        while (!_stop.IsCancellationRequested)
        {
            HttpListenerContext context;
            try { context = await _listener.GetContextAsync(); }
            catch { return; }

            using var reader = new StreamReader(context.Request.InputStream, Encoding.UTF8);
            var body = await reader.ReadToEndAsync();
            lock (Requests) Requests.Add((context.Request.HttpMethod, context.Request.RawUrl ?? "", body));

            var payload = Encoding.UTF8.GetBytes($"{{\"message\":\"ok\",\"result\":{ResultJson}}}");
            context.Response.StatusCode = (int)Status;
            context.Response.ContentType = "application/json";
            await context.Response.OutputStream.WriteAsync(payload);
            context.Response.Close();
        }
    }

    public StoreSuppliersClient Client() =>
        new(new Posty5HttpClient(new Posty5Options { ApiKey = "test-key", BaseUrl = BaseUrl }));

    public void Dispose()
    {
        _stop.Cancel();
        _listener.Close();
    }
}

public class StoreSuppliersRouteTests : IDisposable
{
    private const string Base = "/api/store-suppliers/s1";
    private readonly RecordingServer _server = new();

    public void Dispose() => _server.Dispose();

    [Fact]
    public void StoreClient_ExposesSuppliers()
    {
        var store = new StoreClient(new Posty5HttpClient(new Posty5Options { ApiKey = "k", BaseUrl = _server.BaseUrl }));
        Assert.NotNull(store.Suppliers);
    }

    [Fact]
    public async Task ConnectAsync_SendsCredentialKeysExactly_InTheBody()
    {
        await _server.Client().ConnectAsync("s1", new ConnectSupplierInput
        {
            SupplierKey = "cjdropshipping",
            Credentials = new Dictionary<string, string> { ["apiKey"] = "secret", ["AppSecret"] = "x" },
        });

        var (method, path, body) = _server.Requests.Single();
        Assert.Equal("POST", method);
        Assert.Equal(Base, path);
        Assert.DoesNotContain("secret", path);
        using var json = JsonDocument.Parse(body);
        var credentials = json.RootElement.GetProperty("credentials");
        Assert.Equal("secret", credentials.GetProperty("apiKey").GetString());
        // Dictionary keys are not camel-cased: a key reaches the api as the catalogue spells it.
        Assert.Equal("x", credentials.GetProperty("AppSecret").GetString());
        Assert.False(json.RootElement.TryGetProperty("mode", out _));
    }

    [Fact]
    public async Task ListAsync_UnwrapsItems()
    {
        _server.ResultJson = "{\"items\":[{\"_id\":\"i1\",\"supplierKey\":\"cjdropshipping\",\"hasCredentials\":true}]}";
        var list = await _server.Client().ListAsync("s1");
        Assert.Equal("i1", Assert.Single(list).Id);
    }

    [Fact]
    public async Task ConnectionRoutes_MapToTheApi()
    {
        var client = _server.Client();
        await client.GetCatalogueAsync("s1");
        await client.ReplaceCredentialsAsync("s1", "i1", new ReplaceSupplierCredentialsInput { Credentials = new() { ["apiKey"] = "k" } });
        await client.UpdateSettingsAsync("s1", "i1", new UpdateSupplierSettingsInput { Settings = new() { ["fromCountryCode"] = "US" } });
        await client.UpdateAutomationAsync("s1", "i1", new SupplierAutomationInput { Mode = SupplierAutomationModes.Submit });
        await client.SetEnabledAsync("s1", "i1", false);
        await client.TestAsync("s1", "i1");
        await client.GetBalanceAsync("s1", "i1");
        await client.GetDisconnectImpactAsync("s1", "i1");
        await client.DisconnectAsync("s1", "i1", force: true);
        await client.StartOAuthAsync("s1", new StartSupplierOAuthInput { SupplierKey = "aliexpress" });

        Assert.Equal(new[]
        {
            $"GET {Base}/catalogue",
            $"PUT {Base}/i1",
            $"PUT {Base}/i1/settings",
            $"PUT {Base}/i1/automation",
            $"PUT {Base}/i1/enabled",
            $"POST {Base}/i1/test",
            $"GET {Base}/i1/balance",
            $"GET {Base}/i1/impact",
            $"DELETE {Base}/i1?force=true",
            $"POST {Base}/oauth/start",
        }, _server.Requests.Select(r => $"{r.Method} {r.PathAndQuery}"));

        // A false value is still sent.
        Assert.Contains("\"enabled\":false", _server.Requests[4].Body);
    }

    [Fact]
    public async Task ProductAndLinkRoutes_MapToTheApi()
    {
        _server.ResultJson = "{\"items\":[]}";
        var client = _server.Client();
        await client.BrowseProductsAsync("s1", "i1", new BrowseSupplierProductsParams { Q = "mug" }, page: 2);
        await client.GetProductAsync("s1", "i1", "p/1");
        await client.ResolveUrlAsync("s1", "i1", "https://cjdropshipping.com/product/x");
        await client.PreviewImportAsync("s1", "i1", new ImportSupplierProductsInput { Items = { new ImportSupplierProductItem { SupplierProductId = "p1" } } });
        await client.ImportProductsAsync("s1", "i1", new ImportSupplierProductsInput { Items = { new ImportSupplierProductItem { SupplierProductId = "p1" } } });
        await client.GetImportStatusAsync("s1", "job1");
        await client.ListLinksAsync("s1", "prod1");
        await client.CreateLinkAsync("s1", new CreateSupplierLinkInput { ProductId = "prod1", IntegrationId = "i1", SupplierProductId = "p1", Variants = { new SupplierLinkVariantInput { SupplierVariantId = "v1" } } });
        await client.UpdateLinkAsync("s1", "l1", new UpdateSupplierLinkInput { Sync = new() { [LinkSyncFields.Price] = true } });
        await client.SyncLinkAsync("s1", "l1");
        await client.DeleteLinkAsync("s1", "l1");

        Assert.Equal(new[]
        {
            $"GET {Base}/i1/products?q=mug&page=2",
            $"GET {Base}/i1/products/p%2F1",
            $"POST {Base}/i1/products/resolve-url",
            $"POST {Base}/i1/import/preview",
            $"POST {Base}/i1/import",
            $"GET {Base}/imports/job1",
            $"GET {Base}/links?productId=prod1",
            $"POST {Base}/links",
            $"PUT {Base}/links/l1",
            $"POST {Base}/links/l1/sync",
            $"DELETE {Base}/links/l1",
        }, _server.Requests.Select(r => $"{r.Method} {r.PathAndQuery}"));
    }

    [Fact]
    public async Task SupplierOrderRoutes_EncodeThePartKey()
    {
        var client = _server.Client();
        await client.ListSupplierOrdersAsync("s1", new SupplierOrderSearchParams { NeedsReview = true }, page: 1);
        await client.GetSupplierOrderAsync("s1", "so1");
        await client.SubmitGroupAsync("s1", "o1", "supplier:i1", payNow: true);
        await client.RetryAsync("s1", "so1", acceptCost: true);
        await client.PayAsync("s1", "so1");
        await client.CancelAsync("s1", "so1");
        await client.FulfilGroupManuallyAsync("s1", "o1", "supplier:i1");

        Assert.Equal(new[]
        {
            $"GET {Base}/orders?needsReview=true&page=1",
            $"GET {Base}/orders/so1",
            $"POST {Base}/orders/o1/groups/supplier%3Ai1/submit",
            $"POST {Base}/orders/so1/retry",
            $"POST {Base}/orders/so1/pay",
            $"POST {Base}/orders/so1/cancel",
            $"POST {Base}/orders/o1/groups/supplier%3Ai1/fulfil-manually",
        }, _server.Requests.Select(r => $"{r.Method} {r.PathAndQuery}"));
        Assert.Contains("\"payNow\":true", _server.Requests[2].Body);
        Assert.Contains("\"acceptCost\":true", _server.Requests[3].Body);
    }

    [Fact]
    public async Task ImportResult_TellsQueuedFromInline()
    {
        _server.ResultJson = "{\"jobId\":\"j1\",\"rows\":null}";
        var queued = await _server.Client().ImportProductsAsync("s1", "i1", new ImportSupplierProductsInput());
        Assert.True(queued!.IsQueued);

        _server.ResultJson = "{\"jobId\":null,\"rows\":[{\"supplierProductId\":\"p1\",\"state\":\"added\"}]}";
        var inline = await _server.Client().ImportProductsAsync("s1", "i1", new ImportSupplierProductsInput());
        Assert.False(inline!.IsQueued);
        Assert.Equal("added", Assert.Single(inline.Rows!).State);
    }

    [Fact]
    public async Task PausedOutcome_Throws()
    {
        _server.Status = HttpStatusCode.BadRequest;
        await Assert.ThrowsAsync<Posty5ValidationException>(() => _server.Client().RetryAsync("s1", "so1"));
    }
}

/// <summary>
/// Live, like the rest of this suite, and only when the store fixtures are set
/// (<c>POSTY5_TEST_STORE_ID</c>). Reads only — never connects, imports, sends or pays.
/// </summary>
public class StoreSuppliersLiveTests
{
    private static readonly string? StoreId = Environment.GetEnvironmentVariable("POSTY5_TEST_STORE_ID", EnvironmentVariableTarget.User)
        ?? Environment.GetEnvironmentVariable("POSTY5_TEST_STORE_ID");

    [Fact]
    public async Task ListsConnections_WithoutCredentials()
    {
        if (string.IsNullOrEmpty(StoreId)) return; // no fixture — nothing to read

        var store = new StoreClient(TestConfig.CreateHttpClient());
        var catalogue = await store.Suppliers.GetCatalogueAsync(StoreId);
        Assert.NotNull(catalogue?.Items);

        foreach (var connection in await store.Suppliers.ListAsync(StoreId))
        {
            Assert.NotNull(connection.Id);
        }

        var queue = await store.Suppliers.ListSupplierOrdersAsync(StoreId, new SupplierOrderSearchParams { NeedsReview = true }, pageSize: 5);
        Assert.NotNull(queue?.Items);
    }
}
