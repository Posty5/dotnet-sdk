using Posty5.Core.Models;
using Posty5.QRCode;
using Xunit;

namespace Posty5.Tests.Unit;

/// <summary>
/// Offline: the two <see cref="QRCodeTemplateClient"/> routes, their queries and
/// the page they return, against a <see cref="RecordingServer"/>.
/// </summary>
public class QRCodeTemplateClientRouteTests : IDisposable
{
    private readonly RecordingServer _server = new();

    public void Dispose() => _server.Dispose();

    private QRCodeTemplateClient Client() => new(_server.Http());

    [Fact]
    public async Task ListUserTemplatesAsync_SendsTermAndCursor_AndReadsThePage()
    {
        _server.ResultJson = "{\"items\":[{\"_id\":\"t1\",\"name\":\"Brand\"}],\"pagination\":{\"nextCursor\":\"c2\",\"hasMore\":true,\"pageSize\":1}}";

        var page = await Client().ListUserTemplatesAsync("bra", new PaginationParams { Cursor = "c1", PageSize = 1 });

        var request = _server.Requests.Single();
        Assert.Equal("GET /api/qr-code-template/user-lookup?term=bra&cursor=c1&pageSize=1", $"{request.Method} {request.PathAndQuery}");
        var template = Assert.Single(page.Items);
        Assert.Equal("t1", template.Id);
        Assert.Equal("Brand", template.Name);
        Assert.Equal("c2", page.Pagination.NextCursor);
    }

    [Fact]
    public async Task ListPublicTemplatesAsync_SendsTheSchemeType()
    {
        _server.ResultJson = "{\"items\":[],\"pagination\":{}}";

        await Client().ListPublicTemplatesAsync(schemeType: "dots");
        await Client().ListPublicTemplatesAsync();

        Assert.Equal(new[]
        {
            "GET /api/qr-code-template/public-lookup?schemeType=dots",
            "GET /api/qr-code-template/public-lookup",
        }, _server.Requests.Select(r => $"{r.Method} {r.PathAndQuery}"));
    }
}

/// <summary>
/// Live and read-only: skipped unless <c>POSTY5_API_KEY</c> is set.
/// </summary>
[Collection("Sequential")]
public class QRCodeTemplateClientLiveTests
{
    private readonly QRCodeTemplateClient _templates = new(TestConfig.CreateHttpClient());

    [ApiKeyFact]
    public async Task BothLists_Read()
    {
        var mine = await _templates.ListUserTemplatesAsync(pagination: new PaginationParams { PageSize = 5 });
        Assert.True(mine.Items.Count <= 5);

        var shared = await _templates.ListPublicTemplatesAsync(pagination: new PaginationParams { PageSize = 5 });
        Assert.NotEmpty(shared.Items);
        Assert.All(shared.Items, t => Assert.False(string.IsNullOrEmpty(t.Id)));
    }
}
