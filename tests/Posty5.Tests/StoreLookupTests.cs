using Posty5.Store;
using Xunit;

namespace Posty5.Tests.Store;

/// <summary>
/// Offline: the two store-level calls that take no storeId, against a
/// <see cref="RecordingServer"/>.
/// </summary>
public class StoreLookupRouteTests : IDisposable
{
    private readonly RecordingServer _server = new();

    public void Dispose() => _server.Dispose();

    [Fact]
    public async Task ListStoresAsync_AndLookupStoresAsync_HitTheLookupRoute()
    {
        _server.ResultJson = "[{\"_id\":\"s1\",\"name\":\"my-shop - My Shop\"}]";
        var store = new StoreClient(_server.Http());

        var all = await store.ListStoresAsync();
        var some = await store.LookupStoresAsync("my shop");
        await store.LookupStoresAsync("  ");

        Assert.Equal(new[]
        {
            "GET /api/store/lookup",
            "GET /api/store/lookup?term=my%20shop",
            "GET /api/store/lookup",
        }, _server.Requests.Select(r => $"{r.Method} {r.PathAndQuery}"));

        var first = Assert.Single(all);
        Assert.Equal("s1", first.Id);
        Assert.Equal("my-shop - My Shop", first.Name);
        Assert.Single(some);
    }

    [Fact]
    public async Task ListStoresAsync_NoStores_IsEmpty()
    {
        _server.ResultJson = "[]";

        Assert.Empty(await new StoreClient(_server.Http()).ListStoresAsync());
    }
}

/// <summary>
/// Live and read-only: skipped unless <c>POSTY5_API_KEY</c> is set.
/// </summary>
[Collection("Sequential")]
public class StoreLookupLiveTests
{
    [ApiKeyFact]
    public async Task ListStoresAsync_ReturnsIdsAndSlugNameLabels()
    {
        var store = new StoreClient(TestConfig.CreateHttpClient());

        var stores = await store.ListStoresAsync();

        Assert.All(stores, s => Assert.False(string.IsNullOrEmpty(s.Id)));
        Assert.All(stores, s => Assert.Contains(" - ", s.Name));
    }
}
