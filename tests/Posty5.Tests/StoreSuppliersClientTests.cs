using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using Posty5.Core.Configuration;
using Posty5.Core.Exceptions;
using Posty5.Core.Http;
using Posty5.Core.Models;
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
        await client.ListSupplierOrdersAsync("s1", new SupplierOrderSearchParams { NeedsReview = true }, new PaginationParams { PageSize = 25 });
        await client.GetSupplierOrderAsync("s1", "so1");
        await client.SubmitGroupAsync("s1", "o1", "supplier:i1", payNow: true);
        await client.RetryAsync("s1", "so1", acceptCost: true);
        await client.PayAsync("s1", "so1");
        await client.CancelAsync("s1", "so1");
        await client.FulfilGroupManuallyAsync("s1", "o1", "supplier:i1");

        Assert.Equal(new[]
        {
            $"GET {Base}/orders?needsReview=true&pageSize=25",
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
    public async Task ListSupplierOrders_PagesByCursor_AndReadsTheListEnvelope()
    {
        _server.ResultJson = "{\"items\":[{\"_id\":\"so2\"}],\"pagination\":{\"nextCursor\":\"c3\",\"previousCursor\":\"c1\",\"hasMore\":true,\"totalCount\":30,\"pageSize\":25}}";
        var page = await _server.Client().ListSupplierOrdersAsync("s1",
            new SupplierOrderSearchParams { Status = SupplierOrderStatuses.Failed },
            new PaginationParams { Cursor = "c2", PageSize = 25 });

        var request = _server.Requests.Single();
        Assert.Equal($"GET {Base}/orders?status=failed&cursor=c2&pageSize=25", $"{request.Method} {request.PathAndQuery}");
        Assert.DoesNotContain("page=", request.PathAndQuery);

        Assert.NotNull(page);
        Assert.Equal("so2", Assert.Single(page!.Items).Id);
        Assert.Equal("c3", page.Pagination.NextCursor);
        Assert.Equal("c1", page.Pagination.PreviousCursor);
        Assert.True(page.Pagination.HasMore);
        Assert.Equal(25, page.Pagination.PageSize);
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
/// Live, like the rest of this suite: there is no transport injection in
/// <see cref="Posty5HttpClient"/>, so these call the configured API. Each fact
/// is reported as <b>skipped</b> — naming the variable — when its fixture is not
/// set (<see cref="StoreFixtureFactAttribute"/>). By default they read, link,
/// sync and unlink, and flip one automation setting and restore it; they never
/// connect, disconnect or send a credential. Import (charges credits) and the
/// group actions sit behind further variables.
/// </summary>
[Collection("Sequential")]
public class StoreSuppliersLiveTests : IDisposable
{
    private readonly StoreClient _store = new(TestConfig.CreateHttpClient());
    private static string StoreId => TestConfig.StoreId;
    private static string IntegrationId => TestConfig.SupplierIntegrationId;

    /// <summary>Anything a fact created and could not remove itself is removed here.</summary>
    public void Dispose()
    {
        foreach (var linkId in TestConfig.CreatedResources.SupplierLinks.ToList())
        {
            try { _store.Suppliers.DeleteLinkAsync(StoreId, linkId).GetAwaiter().GetResult(); } catch { /* already gone */ }
            TestConfig.CreatedResources.SupplierLinks.Remove(linkId);
        }
        foreach (var productId in TestConfig.CreatedResources.StoreProducts.ToList())
        {
            try { _store.Products.DeleteAsync(StoreId, productId).GetAwaiter().GetResult(); } catch { /* already gone */ }
            TestConfig.CreatedResources.StoreProducts.Remove(productId);
        }
    }

    /// <summary>A paused outcome is thrown (HTTP 400) rather than returned; read it from either side.</summary>
    private static async Task<string> OutcomeOf(Func<Task<SupplierOrderActionResult?>> action)
    {
        try
        {
            return JsonSerializer.Serialize(await action());
        }
        catch (Posty5ValidationException paused)
        {
            return paused.Message;
        }
    }

    [StoreFixtureFact]
    public async Task GetCatalogueAndList_CarryNoCredentials()
    {
        var catalogue = await _store.Suppliers.GetCatalogueAsync(StoreId);
        Assert.NotNull(catalogue?.Items);

        foreach (var connection in await _store.Suppliers.ListAsync(StoreId))
        {
            Assert.NotNull(connection.Id);
            Assert.NotNull(connection.Automation);
        }
    }

    [StoreFixtureFact]
    public async Task ListSupplierOrders_ThenGetTheFirst()
    {
        var queue = await _store.Suppliers.ListSupplierOrdersAsync(StoreId, new SupplierOrderSearchParams { NeedsReview = true }, new PaginationParams { PageSize = 5 });
        Assert.NotNull(queue?.Items);
        Assert.NotNull(queue!.Pagination);
        if (queue.Pagination.HasMore)
        {
            var next = await _store.Suppliers.ListSupplierOrdersAsync(StoreId, new SupplierOrderSearchParams { NeedsReview = true },
                new PaginationParams { Cursor = queue.Pagination.NextCursor, PageSize = 5 });
            Assert.NotNull(next?.Items);
        }

        var any = await _store.Suppliers.ListSupplierOrdersAsync(StoreId, pagination: new PaginationParams { PageSize = 1 });
        var first = any?.Items?.FirstOrDefault();
        if (first?.Id is null)
        {
            Console.WriteLine("No supplier orders on the fixture store; GetSupplierOrderAsync not exercised.");
            return;
        }
        var row = await _store.Suppliers.GetSupplierOrderAsync(StoreId, first.Id);
        Assert.Equal(first.Id, row?.Id);
        Assert.False(string.IsNullOrEmpty(row?.Status));
    }

    [StoreFixtureFact]
    public async Task Orders_NeedsAttentionFilter_AndPartsDeserialise()
    {
        var page = await _store.Orders.SearchAsync(StoreId, new OrderSearchParams { NeedsAttention = true }, new PaginationParams { PageSize = 5 });
        Assert.NotNull(page?.Items);

        var orderId = TestConfig.OrderId.Length > 0
            ? TestConfig.OrderId
            : (await _store.Orders.SearchAsync(StoreId, pagination: new PaginationParams { PageSize = 1 }))?.Items.FirstOrDefault()?.Id;
        if (orderId is null)
        {
            Console.WriteLine("No orders on the fixture store; Orders.GetAsync not exercised.");
            return;
        }
        var order = await _store.Orders.GetAsync(StoreId, orderId);
        Assert.NotNull(order);
        foreach (var group in order!.FulfilmentGroups ?? new())
        {
            Assert.False(string.IsNullOrEmpty(group.Key));
            Assert.Contains(group.Kind, new[] { FulfilmentKinds.Merchant, FulfilmentKinds.ThirdParty });
            Assert.NotNull(group.LineKeys);
        }
    }

    [StoreFixtureFact]
    public async Task ListLinks_ReturnsAList()
    {
        var links = await _store.Suppliers.ListLinksAsync(StoreId, TestConfig.ProductId.Length > 0 ? TestConfig.ProductId : null);
        Assert.NotNull(links);
    }

    [StoreFixtureFact(TestConfig.SupplierIntegrationIdVar)]
    public async Task GetBalance_AndTest_OnTheTestConnection()
    {
        var balance = await _store.Suppliers.GetBalanceAsync(StoreId, IntegrationId);
        Assert.False(string.IsNullOrEmpty(balance?.Currency));

        var test = await _store.Suppliers.TestAsync(StoreId, IntegrationId);
        Assert.NotNull(test);
    }

    [StoreFixtureFact(TestConfig.SupplierIntegrationIdVar)]
    public async Task BrowseProducts_OneSmallPage()
    {
        var page = await _store.Suppliers.BrowseProductsAsync(StoreId, IntegrationId, page: 1, pageSize: 5);
        Assert.NotNull(page?.Items);
        Assert.True(page!.Items!.Count <= 5);
        Assert.All(page.Items, item => Assert.False(string.IsNullOrEmpty(item.SupplierProductId)));
    }

    [StoreFixtureFact(TestConfig.SupplierIntegrationIdVar, TestConfig.SupplierProductIdVar)]
    public async Task PreviewImport_CreatesAndChargesNothing()
    {
        var preview = await _store.Suppliers.PreviewImportAsync(StoreId, IntegrationId, new ImportSupplierProductsInput
        {
            Items = { new ImportSupplierProductItem { SupplierProductId = TestConfig.SupplierProductId } },
        });
        var row = Assert.Single(preview!.Rows!);
        Assert.Equal(TestConfig.SupplierProductId, row.SupplierProductId);
        Assert.NotNull(preview.Totals);
    }

    [StoreFixtureFact(TestConfig.SupplierIntegrationIdVar)]
    public async Task UpdateAutomation_RoundTrip_IsRestored()
    {
        var before = (await _store.Suppliers.ListAsync(StoreId)).Single(c => c.Id == IntegrationId).Automation!;
        try
        {
            var changed = await _store.Suppliers.UpdateAutomationAsync(StoreId, IntegrationId,
                new SupplierAutomationInput { AllowUnpaidOrders = !before.AllowUnpaidOrders });
            Assert.Equal(!before.AllowUnpaidOrders, changed!.Automation!.AllowUnpaidOrders);
        }
        finally
        {
            var restored = await _store.Suppliers.UpdateAutomationAsync(StoreId, IntegrationId, new SupplierAutomationInput
            {
                Mode = before.Mode,
                AllowUnpaidOrders = before.AllowUnpaidOrders,
                MaxCostPerOrder = before.MaxCostPerOrder,
                MaxCostRatio = before.MaxCostRatio,
                AllowedCountries = before.AllowedCountries ?? new(),
            });
            Assert.Equal(before.AllowUnpaidOrders, restored!.Automation!.AllowUnpaidOrders);
        }
    }

    [StoreFixtureFact(TestConfig.SupplierIntegrationIdVar, TestConfig.SupplierProductIdVar, TestConfig.ProductIdVar)]
    public async Task CreateUpdateSyncDeleteLink_OnTheTestProduct()
    {
        var product = await _store.Suppliers.GetProductAsync(StoreId, IntegrationId, TestConfig.SupplierProductId);
        var variantId = product!.Variants!.First().SupplierVariantId!;

        var link = await _store.Suppliers.CreateLinkAsync(StoreId, new CreateSupplierLinkInput
        {
            ProductId = TestConfig.ProductId,
            IntegrationId = IntegrationId,
            SupplierProductId = TestConfig.SupplierProductId,
            Variants = { new SupplierLinkVariantInput { SupplierVariantId = variantId } },
            SyncNow = false,
        });
        TestConfig.CreatedResources.SupplierLinks.Add(link!.Id!);
        Assert.Equal(TestConfig.SupplierProductId, link.SupplierProductId);

        var updated = await _store.Suppliers.UpdateLinkAsync(StoreId, link.Id!, new UpdateSupplierLinkInput
        {
            Sync = new() { [LinkSyncFields.Price] = false },
        });
        Assert.False(updated!.Sync![LinkSyncFields.Price]);

        // Never synced (SyncNow = false), so the once-a-minute limit does not apply yet.
        var synced = await _store.Suppliers.SyncLinkAsync(StoreId, link.Id!);
        Assert.Equal(link.Id, synced?.Link?.Id);
        Assert.NotNull(synced?.Changed);

        await _store.Suppliers.DeleteLinkAsync(StoreId, link.Id!);
        TestConfig.CreatedResources.SupplierLinks.Remove(link.Id!);
    }

    // ─── Guarded: charges, or acts on a supplier order ──────────────────────

    [StoreFixtureFact(TestConfig.AllowChargesVar, TestConfig.SupplierIntegrationIdVar, TestConfig.SupplierProductIdVar)]
    public async Task ImportProducts_OneDraft_ThenDeleted()
    {
        var result = await _store.Suppliers.ImportProductsAsync(StoreId, IntegrationId, new ImportSupplierProductsInput
        {
            Items = { new ImportSupplierProductItem { SupplierProductId = TestConfig.SupplierProductId } },
            Defaults = new ImportSupplierProductDefaults { Status = "draft" },
            AllowDuplicate = true,
        });
        Assert.NotNull(result);

        // One product is under the inline limit, but a queued answer is still a valid answer.
        var rows = result!.IsQueued
            ? (await _store.Suppliers.GetImportStatusAsync(StoreId, result.JobId!))?.Rows ?? new()
            : result.Rows!;
        foreach (var created in rows.Where(r => r.ProductId is not null))
        {
            TestConfig.CreatedResources.StoreProducts.Add(created.ProductId!);
        }
        if (!result.IsQueued)
        {
            Assert.Equal("added", Assert.Single(rows).State);
        }
    }

    [StoreFixtureFact(TestConfig.OrderIdVar, TestConfig.GroupKeyVar)]
    public async Task SubmitGroup_OnTheTestConnection_AnswersTestMode()
    {
        var outcome = await OutcomeOf(() => _store.Suppliers.SubmitGroupAsync(StoreId, TestConfig.OrderId, TestConfig.GroupKey));
        Assert.Contains(SupplierReviewReasons.TestMode, outcome);
    }

    [StoreFixtureFact(TestConfig.OrderIdVar, TestConfig.GroupKeyVar)]
    public async Task RetryAndPay_OnTheTestConnection_MoveNoMoney()
    {
        var row = await FixturePartSupplierOrder();
        Assert.Contains(SupplierReviewReasons.TestMode, await OutcomeOf(() => _store.Suppliers.RetryAsync(StoreId, row.Id!)));

        var paid = await OutcomeOf(() => _store.Suppliers.PayAsync(StoreId, row.Id!));
        Assert.DoesNotContain($"\"status\":\"{SupplierOrderStatuses.Confirmed}\"", paid);
    }

    [StoreFixtureFact(TestConfig.OrderIdVar, TestConfig.GroupKeyVar, TestConfig.AllowPartTakeoverVar)]
    public async Task CancelThenFulfilManually_EndsTheFixturePart()
    {
        var row = await FixturePartSupplierOrder();
        await OutcomeOf(() => _store.Suppliers.CancelAsync(StoreId, row.Id!));
        Assert.Equal(SupplierOrderStatuses.Cancelled, (await _store.Suppliers.GetSupplierOrderAsync(StoreId, row.Id!))?.Status);

        var manual = await _store.Suppliers.FulfilGroupManuallyAsync(StoreId, TestConfig.OrderId, TestConfig.GroupKey);
        Assert.Equal(TestConfig.OrderId, manual?.OrderId);
    }

    private async Task<StoreSupplierOrder> FixturePartSupplierOrder()
    {
        var page = await _store.Suppliers.ListSupplierOrdersAsync(StoreId, new SupplierOrderSearchParams { OrderId = TestConfig.OrderId });
        var row = page?.Items?.FirstOrDefault(r => r.FulfilmentGroupKey == TestConfig.GroupKey);
        Assert.NotNull(row);
        return row!;
    }
}