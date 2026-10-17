using System.Text.Json;
using Posty5.Store.Models;
using Xunit;

namespace Posty5.Tests.Store;

/// <summary>
/// Offline: store-test-mode's additive fields deserialise from payloads with and
/// without them (older API responses carry neither). Written 2026-10-17, not run.
/// </summary>
public class StoreTestModeFieldsTests
{
    [Fact]
    public void OrderSummary_ReadsIsTestAndIsDemo()
    {
        var order = JsonSerializer.Deserialize<StoreOrderSummary>("{\"_id\":\"o1\",\"__v\":2,\"isTest\":true,\"isDemo\":false}")!;
        Assert.True(order.IsTest);
        Assert.False(order.IsDemo);
    }

    [Fact]
    public void OlderPayloads_LeaveTheFlagsNull()
    {
        var order = JsonSerializer.Deserialize<StoreOrder>("{\"_id\":\"o1\",\"__v\":0}")!;
        Assert.Null(order.IsTest);
        Assert.Null(order.IsDemo);
        Assert.False(order.IsTest ?? false);
    }

    [Fact]
    public void ProductsCustomersAndTags_ReadIsDemo()
    {
        Assert.True(JsonSerializer.Deserialize<ProductSummary>("{\"_id\":\"p1\",\"__v\":0,\"isDemo\":true}")!.IsDemo);
        Assert.True(JsonSerializer.Deserialize<StoreCustomer>("{\"_id\":\"c1\",\"__v\":0,\"isDemo\":true}")!.IsDemo);
        Assert.False(JsonSerializer.Deserialize<StoreTag>("{\"_id\":\"t1\",\"__v\":0,\"isDemo\":false}")!.IsDemo);
    }
}
