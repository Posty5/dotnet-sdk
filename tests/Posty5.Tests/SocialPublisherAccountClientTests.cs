using Posty5.Core.Exceptions;
using Posty5.Core.Models;
using Posty5.SocialPublisherWorkspace;
using Posty5.SocialPublisherWorkspace.Models;
using Xunit;

namespace Posty5.Tests.Unit;

/// <summary>
/// Offline: every <see cref="SocialPublisherAccountClient"/> route, verb and
/// query, and the response shapes, against a <see cref="RecordingServer"/>.
/// </summary>
public class SocialPublisherAccountClientRouteTests : IDisposable
{
    private const string Base = "/api/social-publisher-account";
    private readonly RecordingServer _server = new();

    public void Dispose() => _server.Dispose();

    private SocialPublisherAccountClient Client() => new(_server.Http());

    [Fact]
    public async Task ListAsync_SendsFiltersAndCursor_AndReadsThePage()
    {
        _server.ResultJson = """
        { "items": [ { "_id": "a1", "platform": "instagram", "status": "authenticationExpired", "authSource": "instagram_login",
                       "name": "Shop", "thumbnail": "https://cdn/x.jpg", "link": "https://instagram.com/shop", "createdAt": "2026-09-01T00:00:00Z" } ],
          "pagination": { "nextCursor": null, "previousCursor": null, "hasMore": false, "pageSize": 10 } }
        """;

        var page = await Client().ListAsync(
            new SocialPublisherAccountListParamsModel { Platform = "instagram", Status = "active", Name = "sh op" },
            new PaginationParams { Cursor = "c1", PageSize = 10 });

        var request = _server.Requests.Single();
        Assert.Equal($"GET {Base}?platform=instagram&status=active&name=sh%20op&cursor=c1&pageSize=10", $"{request.Method} {request.PathAndQuery}");
        var account = Assert.Single(page.Items);
        Assert.Equal("a1", account.Id);
        Assert.Equal("authenticationExpired", account.Status);
        Assert.Equal("instagram_login", account.AuthSource);
    }

    [Fact]
    public async Task ListAsync_WithNoArguments_SendsNoQuery()
    {
        _server.ResultJson = "{\"items\":[],\"pagination\":{}}";

        await Client().ListAsync();

        var request = _server.Requests.Single();
        Assert.Equal($"GET {Base}", $"{request.Method} {request.PathAndQuery}");
    }

    [Fact]
    public async Task LookupAsync_SendsTermAndPlatform_AndReadsTheArray()
    {
        _server.ResultJson = "[{\"_id\":\"a1\",\"name\":\"Shop page\",\"data\":{\"img\":\"https://cdn/x.jpg\"}}]";

        var matches = await Client().LookupAsync("shop", "facebook");

        var request = _server.Requests.Single();
        Assert.Equal($"GET {Base}/lookup?term=shop&platform=facebook", $"{request.Method} {request.PathAndQuery}");
        var match = Assert.Single(matches);
        Assert.Equal("a1", match.Id);
        Assert.Equal("https://cdn/x.jpg", match.Data!.Img);
    }

    [Fact]
    public async Task GetAsync_ReadsTheDetailsAndKeepsThePlatformProfile()
    {
        _server.ResultJson = """
        { "_id": "a1", "platform": "youtube", "status": "active", "name": "Channel",
          "youtube_channelInfo": { "id": "UC1", "title": "Channel" },
          "defaultPostSettings": { "video": { "youtube": { "madeForKids": false } } },
          "defaultComments": [ { "text": "Thanks for watching", "order": 0 } ] }
        """;

        var account = await Client().GetAsync("a1");

        var request = _server.Requests.Single();
        Assert.Equal($"GET {Base}/a1", $"{request.Method} {request.PathAndQuery}");
        Assert.Equal("youtube", account.Platform);
        Assert.True(account.DefaultPostSettings.HasValue);
        Assert.Single(account.DefaultComments!);
        Assert.True(account.Extra!.ContainsKey("youtube_channelInfo"));
    }

    [Fact]
    public async Task GetAsync_SomeoneElsesAccount_Throws()
    {
        _server.Status = System.Net.HttpStatusCode.BadRequest;
        await Assert.ThrowsAsync<Posty5ValidationException>(() => Client().GetAsync("a1"));
    }

    [Fact]
    public async Task GetAsync_RefusesAnEmptyId_BeforeAnyRequest()
    {
        await Assert.ThrowsAsync<ArgumentException>(() => Client().GetAsync(" "));
        Assert.Empty(_server.Requests);
    }
}

/// <summary>
/// Live and read-only: skipped unless <c>POSTY5_API_KEY</c> is set.
/// </summary>
[Collection("Sequential")]
public class SocialPublisherAccountClientLiveTests
{
    private readonly SocialPublisherAccountClient _accounts = new(TestConfig.CreateHttpClient());

    [ApiKeyFact]
    public async Task ListAndLookup_Read()
    {
        var page = await _accounts.ListAsync(pagination: new PaginationParams { PageSize = 5 });
        Assert.True(page.Items.Count <= 5);
        Assert.All(page.Items, a => Assert.False(string.IsNullOrEmpty(a.Platform)));

        var matches = await _accounts.LookupAsync();
        Assert.NotNull(matches);

        var first = page.Items.FirstOrDefault();
        if (first == null)
        {
            Console.WriteLine("No connected accounts visible to this key; GetAsync not exercised.");
            return;
        }
        var details = await _accounts.GetAsync(first.Id);
        Assert.Equal(first.Id, details.Id);
    }
}
