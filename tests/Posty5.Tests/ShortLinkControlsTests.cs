using System.Text.Json;
using Posty5.ShortLink;
using Posty5.ShortLink.Models;
using Xunit;

namespace Posty5.Tests;

/// <summary>
/// Short link controls (tags, rules, health check) and link campaigns, recorded
/// offline: they pin routes, query strings and bodies, including the explicit
/// JSON nulls the serializer would otherwise drop.
/// </summary>
public class ShortLinkControlsTests : IDisposable
{
    private readonly RecordingServer _server = new();

    public void Dispose() => _server.Dispose();

    [Fact]
    public async Task SetRules_RemovePassword_SendsPasswordNull_AndKeepsOmittedSectionsOut()
    {
        _server.ResultJson = """{"_id":"l1"}""";
        using var http = _server.Http();
        var client = new ShortLinkClient(http);

        await client.SetRulesAsync("l1", new LinkRulesUpdateModel
        {
            BaseUrl = "https://example.com",
            TemplateId = "t1",
            Access = new LinkAccessInputModel { RemovePassword = true, MaxVisits = 10 },
            ClearUtm = true,
            Variants = Array.Empty<LinkVariantModel>()
        }, 0);

        var request = Assert.Single(_server.Requests);
        Assert.Equal("PUT", request.Method);
        Assert.Equal("/api/short-link/l1", request.PathAndQuery);
        using var body = JsonDocument.Parse(request.Body);
        var access = body.RootElement.GetProperty("access");
        Assert.Equal(JsonValueKind.Null, access.GetProperty("password").ValueKind);
        Assert.Equal(10, access.GetProperty("maxVisits").GetInt32());
        Assert.False(access.TryGetProperty("expiresAt", out _));
        Assert.Equal(JsonValueKind.Null, body.RootElement.GetProperty("utm").ValueKind);
        Assert.Equal(0, body.RootElement.GetProperty("variants").GetArrayLength());
        Assert.False(body.RootElement.TryGetProperty("routing", out _));
        Assert.False(body.RootElement.TryGetProperty("pixels", out _));
    }

    [Fact]
    public async Task SetRules_WithoutBaseUrl_ReadsTheLinkFirst()
    {
        _server.ResultJson = """{"_id":"l1","baseUrl":"https://stored.example","templateId":"t9"}""";
        using var http = _server.Http();
        var client = new ShortLinkClient(http);

        await client.SetRulesAsync("l1", new LinkRulesUpdateModel { ClearAccess = true }, 0);

        Assert.Equal(2, _server.Requests.Count);
        Assert.Equal("GET", _server.Requests[0].Method);
        using var body = JsonDocument.Parse(_server.Requests[1].Body);
        Assert.Equal("https://stored.example", body.RootElement.GetProperty("baseUrl").GetString());
        Assert.Equal("t9", body.RootElement.GetProperty("templateId").GetString());
        Assert.Equal(JsonValueKind.Null, body.RootElement.GetProperty("access").ValueKind);
    }

    [Fact]
    public async Task Create_SendsTagsCampaignAndRules()
    {
        _server.ResultJson = """{"_id":"l1"}""";
        using var http = _server.Http();
        var client = new ShortLinkClient(http);

        await client.CreateAsync(new ShortLinkCreateRequestModel
        {
            BaseUrl = "https://example.com",
            TemplateId = "t1",
            Tags = new[] { "a", "b" },
            CampaignId = "c1",
            Access = new LinkAccessInputModel { Password = "secret1", ExpiresAt = new DateTimeOffset(2026, 12, 1, 0, 0, 0, TimeSpan.Zero) },
            Pixels = new[] { new LinkPixelModel { Provider = LinkPixelProviders.Meta, Id = "123456789012345" } },
            PixelsConsentAcknowledged = true,
            Health = new LinkHealthInputModel { Enabled = true }
        });

        using var body = JsonDocument.Parse(Assert.Single(_server.Requests).Body);
        var root = body.RootElement;
        Assert.Equal(2, root.GetProperty("tags").GetArrayLength());
        Assert.Equal("c1", root.GetProperty("campaignId").GetString());
        Assert.Equal("secret1", root.GetProperty("access").GetProperty("password").GetString());
        Assert.Equal("meta", root.GetProperty("pixels")[0].GetProperty("provider").GetString());
        Assert.True(root.GetProperty("health").GetProperty("enabled").GetBoolean());
        Assert.False(root.TryGetProperty("tag", out _));
    }

    [Fact]
    public void AccessInput_ToString_NeverShowsThePassword()
    {
        var access = new LinkAccessInputModel { Password = "topsecret" };
        Assert.DoesNotContain("topsecret", access.ToString());
    }

    [Fact]
    public async Task ListTags_And_List_SendTheExpectedQuery()
    {
        _server.ResultJson = """["a","b"]""";
        using var http = _server.Http();
        var client = new ShortLinkClient(http);

        var tags = await client.ListTagsAsync("a");
        Assert.Equal(new[] { "a", "b" }, tags);
        Assert.Equal("/api/short-link/tags?term=a", _server.Requests[0].PathAndQuery);

        _server.ResultJson = """{"items":[]}""";
        await client.ListAsync(new ShortLinkListParamsModel { Tags = new[] { "a", "b" }, CampaignId = "c1" });
        Assert.Contains("tags=a%2Cb", _server.Requests[1].PathAndQuery.Replace(",", "%2C"));
        Assert.Contains("campaignId=c1", _server.Requests[1].PathAndQuery);
    }

    [Fact]
    public async Task CheckHealth_PostsToTheHealthCheckRoute()
    {
        using var http = _server.Http();
        var client = new ShortLinkClient(http);

        await client.CheckHealthAsync("l1");

        var request = Assert.Single(_server.Requests);
        Assert.Equal("POST", request.Method);
        Assert.Equal("/api/short-link/l1/health-check", request.PathAndQuery);
    }

    [Fact]
    public async Task LinkCampaign_Routes()
    {
        _server.ResultJson = """{"_id":"c1","name":"Spring"}""";
        using var http = _server.Http();
        var client = new LinkCampaignClient(http);

        await client.CreateAsync(new LinkCampaignCreateRequestModel { Name = "Spring", Color = LinkCampaignColors.Green });
        await client.UpdateAsync("c1", new LinkCampaignUpdateRequestModel { ClearUtm = true }, 0);
        await client.GetAsync("c1");
        await client.DeleteAsync("c1", 0, detach: true);

        Assert.Equal(("POST", "/api/link-campaign"), (_server.Requests[0].Method, _server.Requests[0].PathAndQuery));
        Assert.Equal(("PUT", "/api/link-campaign/c1"), (_server.Requests[1].Method, _server.Requests[1].PathAndQuery));
        using (var body = JsonDocument.Parse(_server.Requests[1].Body))
            Assert.Equal(JsonValueKind.Null, body.RootElement.GetProperty("utm").ValueKind);
        Assert.Equal(("GET", "/api/link-campaign/c1"), (_server.Requests[2].Method, _server.Requests[2].PathAndQuery));
        Assert.Equal(("DELETE", "/api/link-campaign/c1?detach=true"), (_server.Requests[3].Method, _server.Requests[3].PathAndQuery));
    }
}
