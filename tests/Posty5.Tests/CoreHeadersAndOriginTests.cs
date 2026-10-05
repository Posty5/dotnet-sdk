using System.Reflection;
using System.Text.Json;
using Posty5.Core.Configuration;
using Posty5.Core.Http;
using Posty5.ShortLink;
using Posty5.ShortLink.Models;
using Posty5.Store;
using Posty5.Store.Models;
using Xunit;

namespace Posty5.Tests.Unit;

/// <summary>
/// Offline: what every request carries (<c>X-Posty5-Client</c>, the caller's
/// default headers, the API key) and the configurable <c>createdFrom</c> label.
/// </summary>
public class CoreHeadersAndOriginTests : IDisposable
{
    private readonly RecordingServer _server = new();

    public void Dispose() => _server.Dispose();

    // ─── X-Posty5-Client ────────────────────────────────────────────────────

    [Fact]
    public void ClientIdentity_NamesTheSdkAndTheCoreVersion()
    {
        var coreVersion = typeof(Posty5HttpClient).Assembly.GetName().Version!.ToString(3);

        Assert.Equal(coreVersion, Posty5ClientIdentity.Version);
        Assert.Equal($"posty5-dotnet/{coreVersion}", Posty5ClientIdentity.HeaderValue);
        Assert.DoesNotContain("+", Posty5ClientIdentity.Version);
    }

    [Fact]
    public async Task EveryVerb_CarriesTheClientHeaderAndTheApiKey()
    {
        using var http = _server.Http();
        await http.GetAsync<object>("/api/a");
        await http.PostAsync<object>("/api/b", new { });
        await http.PutAsync<object>("/api/c", new { });
        await http.PatchAsync<object>("/api/d", new { });
        await http.DeleteAsync<object>("/api/e");

        Assert.Equal(5, _server.Headers.Count);
        Assert.All(_server.Headers, headers =>
        {
            Assert.Equal(Posty5ClientIdentity.HeaderValue, headers[Posty5HttpDefaults.ClientHeader]);
            Assert.Equal("test-key", headers[Posty5HttpDefaults.ApiKeyHeader]);
        });
    }

    [Fact]
    public async Task DefaultHeaders_AreSentOnEveryRequest()
    {
        using var http = _server.Http(new Posty5Options
        {
            ApiKey = "test-key",
            DefaultHeaders = new() { ["X-Correlation-Id"] = "abc-123" },
        });

        await http.GetAsync<object>("/api/a");
        await http.PostAsync<object>("/api/b", new { });

        Assert.All(_server.Headers, headers => Assert.Equal("abc-123", headers["X-Correlation-Id"]));
        Assert.All(_server.Headers, headers => Assert.Equal(Posty5ClientIdentity.HeaderValue, headers[Posty5HttpDefaults.ClientHeader]));
    }

    [Fact]
    public async Task DefaultHeaders_MayRelabelTheClient()
    {
        using var http = _server.Http(new Posty5Options
        {
            ApiKey = "test-key",
            DefaultHeaders = new() { ["x-posty5-client"] = "my-wrapper/2.0" },
        });

        await http.GetAsync<object>("/api/a");

        Assert.Equal("my-wrapper/2.0", _server.Headers.Single()[Posty5HttpDefaults.ClientHeader]);
    }

    [Theory]
    [InlineData("X-API-Key")]
    [InlineData("x-api-key")]
    [InlineData(" X-Api-Key ")]
    public void DefaultHeaders_CannotCarryTheApiKey(string name)
    {
        var options = new Posty5Options
        {
            ApiKey = "real-key",
            BaseUrl = _server.BaseUrl,
            DefaultHeaders = new() { [name] = "other-key" },
        };

        var error = Assert.Throws<ArgumentException>(() => new Posty5HttpClient(options));
        Assert.Contains("X-API-Key", error.Message);
    }

    [Fact]
    public void DefaultHeaders_RefuseAContentHeader()
    {
        var options = new Posty5Options
        {
            BaseUrl = _server.BaseUrl,
            DefaultHeaders = new() { ["Content-Type"] = "text/plain" },
        };

        Assert.Throws<ArgumentException>(() => new Posty5HttpClient(options));
    }

    [Fact]
    public async Task SetApiKey_StillReplacesTheKey()
    {
        using var http = _server.Http();
        http.SetApiKey("second-key");

        await http.GetAsync<object>("/api/a");

        Assert.Equal("second-key", _server.Headers.Single()[Posty5HttpDefaults.ApiKeyHeader]);
    }

    [Fact]
    public async Task AClientBuiltWithoutOptions_StillSendsTheClientHeader()
    {
        // The pre-3.1 construction path: no options at all, key set afterwards.
        using var http = new Posty5HttpClient(new Posty5Options { BaseUrl = _server.BaseUrl });
        http.SetApiKey("late-key");

        await http.GetAsync<object>("/api/a");

        var headers = _server.Headers.Single();
        Assert.Equal(Posty5ClientIdentity.HeaderValue, headers[Posty5HttpDefaults.ClientHeader]);
        Assert.Equal("late-key", headers[Posty5HttpDefaults.ApiKeyHeader]);
    }

    // ─── createdFrom ────────────────────────────────────────────────────────

    [Theory]
    [InlineData(null, "dotnetPackage")]
    [InlineData("", "dotnetPackage")]
    [InlineData("  ", "dotnetPackage")]
    [InlineData("my-crm", "my-crm")]
    public void ResolveCreatedFrom_PrefersTheOptionOverThePackageDefault(string? configured, string expected)
    {
        using var http = _server.Http(new Posty5Options { ApiKey = "k", CreatedFrom = configured });

        Assert.Equal(expected, http.ResolveCreatedFrom(CreatedFromDefaults.Package));
    }

    [Fact]
    public async Task ShortLinkCreate_StampsThePackageLabelByDefault()
    {
        _server.ResultJson = "{\"_id\":\"l1\"}";
        await new ShortLinkClient(_server.Http()).CreateAsync(new ShortLinkCreateRequestModel { Name = "n", BaseUrl = "https://example.com", TemplateId = "t1" });

        using var body = JsonDocument.Parse(_server.Requests.Single().Body);
        Assert.Equal("dotnetPackage", body.RootElement.GetProperty("createdFrom").GetString());
    }

    [Fact]
    public async Task ShortLinkCreate_StampsTheConfiguredLabel()
    {
        _server.ResultJson = "{\"_id\":\"l1\"}";
        var http = _server.Http(new Posty5Options { ApiKey = "k", CreatedFrom = "my-crm" });
        await new ShortLinkClient(http).CreateAsync(new ShortLinkCreateRequestModel { Name = "n", BaseUrl = "https://example.com", TemplateId = "t1" });

        using var body = JsonDocument.Parse(_server.Requests.Single().Body);
        Assert.Equal("my-crm", body.RootElement.GetProperty("createdFrom").GetString());
    }

    [Fact]
    public async Task StoreOrderCreate_KeepsItsOwnDefault_TakesTheOption_AndLetsTheRequestWin()
    {
        _server.ResultJson = "{\"_id\":\"o1\"}";

        await new StoreClient(_server.Http()).Orders.CreateAsync("s1", new CreateOrderInput());
        await new StoreClient(_server.Http(new Posty5Options { ApiKey = "k", CreatedFrom = "my-crm" })).Orders.CreateAsync("s1", new CreateOrderInput());
        await new StoreClient(_server.Http(new Posty5Options { ApiKey = "k", CreatedFrom = "my-crm" })).Orders.CreateAsync("s1", new CreateOrderInput { CreatedFrom = "pos" });

        var labels = _server.Requests.Select(r =>
        {
            using var body = JsonDocument.Parse(r.Body);
            return body.RootElement.GetProperty("createdFrom").GetString();
        });
        Assert.Equal(new[] { "dotnet", "my-crm", "pos" }, labels);
        Assert.All(_server.Requests, r => Assert.Equal("POST /api/store-orders/s1", $"{r.Method} {r.PathAndQuery}"));
    }
}
