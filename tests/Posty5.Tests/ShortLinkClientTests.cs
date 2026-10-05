using System.Collections.Specialized;
using System.Globalization;
using System.Net;
using System.Text.Json;
using System.Web;
using Xunit;
using Posty5.ShortLink;
using Posty5.ShortLink.Models;
using Posty5.Core.Configuration;
using Posty5.Core.Exceptions;
using Posty5.Core.Http;
using Posty5.Core.Models;
using Posty5.Tests.Store;

namespace Posty5.Tests.Integration;

[Collection("Sequential")]
public class ShortLinkClientTests : IDisposable
{
    private readonly ShortLinkClient _client;
    private string? _createdId;

    public ShortLinkClientTests()
    {
        var httpClient = TestConfig.CreateHttpClient();
        _client = new ShortLinkClient(httpClient);
    }

    [Fact]
    public async Task CreateShortLink_ShouldReturnValidShortLink()
    {
        // Arrange
        var request = new ShortLinkCreateRequestModel
        {
            Name = $"Test Short Link - {DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}",
            BaseUrl = "https://posty5.com",
            TemplateId = TestConfig.TemplateId
        };

        // Act
        var result = await _client.CreateAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Id);
        Assert.NotNull(result.ShorterLink);
        Assert.Equal("https://posty5.com", result.BaseUrl);

        _createdId = result.Id;
        TestConfig.CreatedResources.ShortLinks.Add(_createdId);
    }

    [Fact]
    public async Task CreateShortLink_WithCustomLandingId_ShouldContainSlug()
    {
        // Arrange
        var customSlug = $"test-{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}";
        var request = new ShortLinkCreateRequestModel
        {
            Name = "Custom Slug Link",
            BaseUrl = "https://example.com",
            CustomLandingId = customSlug,
            TemplateId = TestConfig.TemplateId
        };

        // Act
        var result = await _client.CreateAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Id);
        Assert.Contains(customSlug, result.ShorterLink);

        TestConfig.CreatedResources.ShortLinks.Add(result.Id!);
    }

    [Fact]
    public async Task GetShortLinkById_WithValidId_ShouldReturnShortLink()
    {
        // Arrange - Create a short link first
        var createRequest = new ShortLinkCreateRequestModel
        {
            Name = "Test Link for Get",
            BaseUrl = "https://posty5.com",
            TemplateId = TestConfig.TemplateId
        };
        var created = await _client.CreateAsync(createRequest);
        TestConfig.CreatedResources.ShortLinks.Add(created.Id!);

        // Act
        var result = await _client.GetAsync(created.Id!);
        TestConfig.CreatedResources.ShortLinks.Add(created.Id!);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(created.Id, result.Id);
        Assert.NotNull(result.ShorterLink);
        Assert.NotNull(result.BaseUrl);
    }



    [Fact]
    public async Task ListShortLinks_ShouldReturnPaginatedResults()
    {
        // Act
        var result = await _client.ListAsync(
            pagination: new Core.Models.PaginationParams { PageSize = 10 }
        );

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Items);
    }

    [Fact]
    public async Task ListShortLinks_WithNameFilter_ShouldFilterResults()
    {
        // Arrange (Search is obsolete: the API has no generic search term)
        var searchParams = new ShortLinkListParamsModel
        {
            Name = "test"
        };

        // Act
        var result = await _client.ListAsync(
            searchParams,
            new Core.Models.PaginationParams { PageSize = 10 }
        );

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Items);
    }

    [Fact]
    public async Task UpdateShortLink_ShouldUpdateSuccessfully()
    {
        // Arrange - Create a short link first
        var createRequest = new ShortLinkCreateRequestModel
        {
            Name = "Original Name",
            BaseUrl = "https://posty5.com",
            TemplateId = TestConfig.TemplateId
        };
        var created = await _client.CreateAsync(createRequest);
        TestConfig.CreatedResources.ShortLinks.Add(created.Id!);

        // Act
        var newName = $"Updated Short Link - {DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}";
        var updateRequest = new ShortLinkUpdateRequestModel
        {
            Name = newName,
            BaseUrl = "https://guide.posty5.com",
            TemplateId = TestConfig.TemplateId
        };
        var result = await _client.UpdateAsync(created.Id!, updateRequest);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(created.Id, result.Id);
    }

    [Fact]
    public async Task UpdateShortLink_BaseUrl_ShouldUpdateSuccessfully()
    {
        // Arrange - Create a short link first
        var createRequest = new ShortLinkCreateRequestModel
        {
            Name = "Link to Update URL",
            BaseUrl = "https://posty5.com",
            TemplateId = TestConfig.TemplateId
        };
        var created = await _client.CreateAsync(createRequest);
        TestConfig.CreatedResources.ShortLinks.Add(created.Id!);

        // Act
        var updateRequest = new ShortLinkUpdateRequestModel
        {
            BaseUrl = "https://updated.posty5.com",
            TemplateId = TestConfig.TemplateId
        };
        var result = await _client.UpdateAsync(created.Id!, updateRequest);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(created.Id, result.Id);
    }

    [Fact]
    public async Task DeleteShortLink_ShouldDeleteSuccessfully()
    {
        // Arrange - Create a short link first
        var createRequest = new ShortLinkCreateRequestModel
        {
            Name = "Link to Delete",
            BaseUrl = "https://posty5.com",
            TemplateId = TestConfig.TemplateId
        };
        var created = await _client.CreateAsync(createRequest);
        TestConfig.CreatedResources.ShortLinks.Add(created.Id!);

        // Act
        await _client.DeleteAsync(created.Id!);

        // Assert - Remove from tracking since we successfully deleted it
        TestConfig.CreatedResources.ShortLinks.Remove(created.Id!);
    }

    // ─── Live facts that need the API's link-qr truth pass ───────────────────

    [LinkQrTruthPassFact]
    public async Task DeepLinks_RoundTrip_CreateGetKeepClear()
    {
        var created = await _client.CreateAsync(new ShortLinkCreateRequestModel
        {
            Name = $"Deep link round trip - {DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}",
            BaseUrl = "https://posty5.com",
            TemplateId = TestConfig.TemplateId,
            AndroidUrl = "myapp://item/1",
            IosUrl = "myapp://item/1"
        });
        TestConfig.CreatedResources.ShortLinks.Add(created.Id!);

        var read = await _client.GetAsync(created.Id!);
        Assert.Equal("myapp://item/1", read.AndroidUrl);
        Assert.Equal("myapp://item/1", read.IosUrl);
        Assert.True(read.IsSupportAndroidDeepUrl);

        // Same BaseUrl, no deep-link keys: the stored values are kept.
        await _client.UpdateAsync(created.Id!, new ShortLinkUpdateRequestModel
        {
            Name = "Renamed",
            BaseUrl = "https://posty5.com",
            TemplateId = TestConfig.TemplateId
        });
        Assert.Equal("myapp://item/1", (await _client.GetAsync(created.Id!)).AndroidUrl);

        // "" clears.
        await _client.UpdateAsync(created.Id!, new ShortLinkUpdateRequestModel
        {
            BaseUrl = "https://posty5.com",
            TemplateId = TestConfig.TemplateId,
            AndroidUrl = ""
        });
        var cleared = await _client.GetAsync(created.Id!);
        Assert.True(string.IsNullOrEmpty(cleared.AndroidUrl));
        Assert.False(cleared.IsSupportAndroidDeepUrl ?? false);
    }

    [LinkQrTruthPassFact]
    public async Task DeepLinks_JavascriptScheme_IsRefused()
    {
        await Assert.ThrowsAsync<Posty5ValidationException>(() => _client.CreateAsync(new ShortLinkCreateRequestModel
        {
            BaseUrl = "https://posty5.com",
            TemplateId = TestConfig.TemplateId,
            AndroidUrl = "javascript:alert(1)"
        }));
    }

    [LinkQrTruthPassFact]
    public async Task Update_WithoutIsEnableLandingPage_KeepsLandingPageOn()
    {
        var title = $"TP landing {DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}";
        var created = await _client.CreateAsync(new ShortLinkCreateRequestModel
        {
            BaseUrl = "https://posty5.com",
            TemplateId = TestConfig.TemplateId,
            IsEnableLandingPage = true,
            PageInfo = new ShortLinkPageInfoModel { Title = title, Description = "Landing page kept on update" }
        });
        TestConfig.CreatedResources.ShortLinks.Add(created.Id!);

        await _client.UpdateAsync(created.Id!, new ShortLinkUpdateRequestModel
        {
            Name = "Renamed, landing page untouched",
            BaseUrl = "https://posty5.com",
            TemplateId = TestConfig.TemplateId
        });

        Assert.True((await _client.GetAsync(created.Id!)).IsEnableLandingPage);

        // The pageInfo.title filter finds it (3.0.0 sent pageinfo.title, which matched nothing).
        var byTitle = await _client.ListAsync(new ShortLinkListParamsModel { PageInfoTitle = title });
        Assert.Contains(byTitle.Items, link => link.Id == created.Id);
    }

    [LinkQrTruthPassFact]
    public async Task Create_WithRefIdAndTag_ListsUnderThatRefId()
    {
        var refId = $"TP-REF-{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}";
        var created = await _client.CreateAsync(new ShortLinkCreateRequestModel
        {
            BaseUrl = "https://posty5.com",
            TemplateId = TestConfig.TemplateId,
            RefId = refId,
            Tag = "tp-tag"
        });
        TestConfig.CreatedResources.ShortLinks.Add(created.Id!);

        var listed = await _client.ListAsync(new ShortLinkListParamsModel { RefId = refId });

        var item = Assert.Single(listed.Items);
        Assert.Equal(created.Id, item.Id);
        Assert.Equal("tp-tag", item.Tag);
        Assert.NotNull(item.NumberOfVisitors);
        Assert.NotNull(item.Status);
    }

    // ─── Live facts that need the API's visit analytics (VA) ─────────────────

    [LinkQrVisitAnalyticsFact]
    public async Task GetAnalytics_NewLink_ReturnsZerosAndMeta()
    {
        var created = await CreateAnalyticsLinkAsync();

        var analytics = await _client.GetAnalyticsAsync(created.Id!);

        Assert.Equal(0, analytics.Totals.Visits);
        Assert.Equal(0, analytics.Totals.UniqueVisitors);
        Assert.Equal(0, analytics.Totals.BotVisits);
        Assert.All(analytics.Series, point => Assert.Equal(0, point.Visits));
        Assert.False(string.IsNullOrEmpty(analytics.Meta.AnalyticsStartedAt));
        Assert.False(string.IsNullOrEmpty(analytics.Meta.Timezone));
    }

    [LinkQrVisitAnalyticsFact]
    public async Task GetAnalytics_AllBreakdowns_ReturnsAllowedOnes_AndListsTheRestAsLocked()
    {
        var created = await CreateAnalyticsLinkAsync();

        var analytics = await _client.GetAnalyticsAsync(created.Id!, new LinkAnalyticsQuery { AllBreakdowns = true });

        // channel and device are on every plan (VA-D3).
        Assert.Contains(LinkAnalyticsBreakdown.Channel.Value, analytics.Breakdowns.Keys);
        Assert.Contains(LinkAnalyticsBreakdown.Device.Value, analytics.Breakdowns.Keys);
        foreach (var locked in analytics.Meta.Locked)
        {
            Assert.DoesNotContain(locked.Breakdown, analytics.Breakdowns.Keys);
            Assert.False(string.IsNullOrEmpty(locked.RequiredPlan));
        }
    }

    [LinkQrVisitAnalyticsFact]
    public async Task GetAnalytics_ExplicitList_ReturnsThoseBreakdowns()
    {
        var created = await CreateAnalyticsLinkAsync();

        var analytics = await _client.GetAnalyticsAsync(created.Id!, new LinkAnalyticsQuery
        {
            Breakdown = new[] { LinkAnalyticsBreakdown.Channel, LinkAnalyticsBreakdown.Device },
            Interval = LinkAnalyticsInterval.Week
        });

        Assert.Contains(LinkAnalyticsBreakdown.Channel.Value, analytics.Breakdowns.Keys);
        Assert.Contains(LinkAnalyticsBreakdown.Device.Value, analytics.Breakdowns.Keys);
        Assert.Equal(LinkAnalyticsInterval.Week.Value, analytics.Meta.Interval);
    }

    [LinkQrVisitAnalyticsFact]
    public async Task GetAnalytics_InvalidInterval_IsRefused()
    {
        var created = await CreateAnalyticsLinkAsync();

        await Assert.ThrowsAsync<Posty5ValidationException>(() => _client.GetAnalyticsAsync(created.Id!, new LinkAnalyticsQuery
        {
            Interval = new LinkAnalyticsInterval("fortnight")
        }));
    }

    private async Task<ShortLinkModel> CreateAnalyticsLinkAsync()
    {
        var created = await _client.CreateAsync(new ShortLinkCreateRequestModel
        {
            Name = $"VA analytics - {DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}",
            BaseUrl = "https://posty5.com",
            TemplateId = TestConfig.TemplateId
        });
        TestConfig.CreatedResources.ShortLinks.Add(created.Id!);
        return created;
    }

    public void Dispose()
    {
        // Cleanup is handled by collection fixture if needed
    }
}

/// <summary>
/// What <see cref="ShortLinkClient"/> puts on the wire, pinned against a local
/// <see cref="RecordingServer"/> - no network, no API key.
/// </summary>
public class ShortLinkClientPayloadTests : IDisposable
{
    private readonly RecordingServer _server = new();
    private readonly ShortLinkClient _client;

    public ShortLinkClientPayloadTests()
    {
        _client = new ShortLinkClient(new Posty5HttpClient(new Posty5Options { ApiKey = "test-key", BaseUrl = _server.BaseUrl }));
    }

    public void Dispose() => _server.Dispose();

    [Fact]
    public async Task CreateAsync_SendsEveryAcceptedField_AndNoMonetization()
    {
#pragma warning disable CS0618 // the obsolete property is set on purpose: it must not reach the wire
        await _client.CreateAsync(new ShortLinkCreateRequestModel
        {
            Name = "Spring sale",
            BaseUrl = "https://example.com/sale",
            TemplateId = "tpl-1",
            CustomLandingId = "spring-sale",
            RefId = "REF-1",
            Tag = "campaign",
            IsEnableLandingPage = true,
            PageInfo = new ShortLinkPageInfoModel { Title = "Spring", Description = "Up to 40% off" },
            AndroidUrl = "myapp://item/1",
            IosUrl = "myapp://item/2",
            IsEnableMonetization = true
        });
#pragma warning restore CS0618

        var (method, path, body) = _server.Requests.Single();
        Assert.Equal("POST", method);
        Assert.Equal("/api/short-link", path);

        using var json = JsonDocument.Parse(body);
        var root = json.RootElement;
        Assert.Equal("Spring sale", root.GetProperty("name").GetString());
        Assert.Equal("https://example.com/sale", root.GetProperty("baseUrl").GetString());
        Assert.Equal("tpl-1", root.GetProperty("templateId").GetString());
        Assert.Equal("spring-sale", root.GetProperty("customLandingId").GetString());
        Assert.Equal("REF-1", root.GetProperty("refId").GetString());
        Assert.Equal("campaign", root.GetProperty("tag").GetString());
        Assert.True(root.GetProperty("isEnableLandingPage").GetBoolean());
        Assert.Equal("Spring", root.GetProperty("pageInfo").GetProperty("title").GetString());
        Assert.Equal("Up to 40% off", root.GetProperty("pageInfo").GetProperty("description").GetString());
        Assert.Equal("myapp://item/1", root.GetProperty("androidUrl").GetString());
        Assert.Equal("myapp://item/2", root.GetProperty("iosUrl").GetString());
        Assert.Equal("user", root.GetProperty("templateType").GetString());
        Assert.Equal("dotnetPackage", root.GetProperty("createdFrom").GetString());
        Assert.False(root.TryGetProperty("isEnableMonetization", out _));
        Assert.DoesNotContain("onetization", body);
    }

    [Fact]
    public async Task CreateAsync_UnsetOptionalFields_AreNotSent()
    {
        await _client.CreateAsync(new ShortLinkCreateRequestModel { BaseUrl = "https://example.com", TemplateId = "tpl-1" });

        using var json = JsonDocument.Parse(_server.Requests.Single().Body);
        var root = json.RootElement;
        foreach (var key in new[] { "refId", "tag", "customLandingId", "isEnableLandingPage", "pageInfo", "androidUrl", "iosUrl", "isEnableMonetization" })
        {
            Assert.False(root.TryGetProperty(key, out _), $"'{key}' should not be sent when unset");
        }
    }

    [Fact]
    public async Task UpdateAsync_OmitsIsEnableLandingPage_AndDeepLinks_WhenUnset()
    {
#pragma warning disable CS0618
        await _client.UpdateAsync("id1", new ShortLinkUpdateRequestModel
        {
            BaseUrl = "https://example.com/new",
            TemplateId = "tpl-1",
            IsEnableMonetization = true
        });
#pragma warning restore CS0618

        var (method, path, body) = _server.Requests.Single();
        Assert.Equal("PUT", method);
        Assert.Equal("/api/short-link/id1", path);

        using var json = JsonDocument.Parse(body);
        var root = json.RootElement;
        Assert.Equal("https://example.com/new", root.GetProperty("baseUrl").GetString());
        Assert.Equal("tpl-1", root.GetProperty("templateId").GetString());
        // Omitted = the API keeps the stored landing-page flag and deep links (or re-derives them for a new BaseUrl).
        foreach (var key in new[] { "isEnableLandingPage", "androidUrl", "iosUrl", "name", "refId", "tag", "pageInfo", "customLandingId", "isEnableMonetization" })
        {
            Assert.False(root.TryGetProperty(key, out _), $"'{key}' should not be sent when unset");
        }
    }

    [Fact]
    public async Task UpdateAsync_SendsExplicitValues_IncludingFalseAndEmptyToClear()
    {
        await _client.UpdateAsync("id1", new ShortLinkUpdateRequestModel
        {
            BaseUrl = "https://example.com",
            TemplateId = "tpl-1",
            IsEnableLandingPage = false,
            AndroidUrl = "",
            IosUrl = "myapp://item/9",
            RefId = "REF-2",
            Tag = "t2"
        });

        using var json = JsonDocument.Parse(_server.Requests.Single().Body);
        var root = json.RootElement;
        Assert.False(root.GetProperty("isEnableLandingPage").GetBoolean());
        Assert.Equal("", root.GetProperty("androidUrl").GetString());
        Assert.Equal("myapp://item/9", root.GetProperty("iosUrl").GetString());
        Assert.Equal("REF-2", root.GetProperty("refId").GetString());
        Assert.Equal("t2", root.GetProperty("tag").GetString());
        Assert.Equal("user", root.GetProperty("templateType").GetString());
    }

    [Fact]
    public async Task UpdateAsync_EmptyBaseUrl_ThrowsBeforeSending()
    {
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _client.UpdateAsync("id1", new ShortLinkUpdateRequestModel { BaseUrl = " ", TemplateId = "tpl-1" }));
        Assert.Empty(_server.Requests);
    }

    [Fact]
    public async Task ListAsync_SendsPageInfoTitleKey_AndDropsFiltersTheApiIgnores()
    {
#pragma warning disable CS0618
        await _client.ListAsync(new ShortLinkListParamsModel
        {
            PageInfoTitle = "Spring",
            RefId = "REF-1",
            Search = "ignored",
            FromDate = new DateTime(2026, 1, 1),
            ToDate = new DateTime(2026, 2, 1),
            IsEnableMonetization = true
        });
#pragma warning restore CS0618

        var (method, path, _) = _server.Requests.Single();
        Assert.Equal("GET", method);
        Assert.Contains("pageInfo.title=Spring", path);
        Assert.DoesNotContain("pageinfo.title", path);
        Assert.Contains("refId=REF-1", path);
        foreach (var key in new[] { "search=", "fromDate=", "toDate=", "isEnableMonetization=" })
        {
            Assert.DoesNotContain(key, path);
        }
    }

    [Fact]
    public async Task Responses_IgnoreIsEnableMonetization_AndReadListFields()
    {
        _server.ResultJson = "{\"items\":[{\"_id\":\"l1\",\"isEnableMonetization\":true,\"numberOfVisitors\":7,\"isEnableLandingPage\":true,\"status\":\"approved\"}]}";

        var item = Assert.Single((await _client.ListAsync()).Items);

#pragma warning disable CS0618
        Assert.Null(item.IsEnableMonetization);
#pragma warning restore CS0618
        Assert.Equal(7, item.NumberOfVisitors);
        Assert.True(item.IsEnableLandingPage);
        Assert.Equal(ShortLinkStatusType.Approved, item.Status);
    }

    // ─── GetAnalyticsAsync (VA) ──────────────────────────────────────────────

    /// <summary>A C2 answer as the API sends it inside <c>result</c>.</summary>
    internal const string AnalyticsSampleJson = """
        {
          "totals": { "visits": 12, "uniqueVisitors": 9, "botVisits": 4 },
          "series": [
            { "date": "2026-10-01", "visits": 5, "uniqueVisitors": 4 },
            { "date": "2026-10-02", "visits": 7, "uniqueVisitors": 5 }
          ],
          "breakdowns": {
            "channel": [ { "key": "click", "visits": 8, "uniqueVisitors": 6 }, { "key": "scan", "visits": 4, "uniqueVisitors": 3 } ],
            "device": [ { "key": "mobile", "visits": 12, "uniqueVisitors": 9 } ],
            "referrer": [ { "key": "unknown", "visits": 10, "uniqueVisitors": 7 }, { "key": "other", "visits": 2, "uniqueVisitors": 2 } ]
          },
          "meta": {
            "from": "2026-10-01", "to": "2026-10-02", "interval": "day", "timezone": "Africa/Cairo",
            "source": "events", "analyticsStartedAt": "2026-10-01",
            "locked": [ { "breakdown": "country", "requiredPlan": "basic" } ],
            "maxHistoryDays": 30
          }
        }
        """;

    [Fact]
    public async Task GetAnalyticsAsync_NoQuery_CallsTheAnalyticsPath_WithNoQueryString()
    {
        _server.ResultJson = AnalyticsSampleJson;

        await _client.GetAnalyticsAsync("l1");

        var (method, path, _) = _server.Requests.Single();
        Assert.Equal("GET", method);
        Assert.Equal("/api/short-link/l1/analytics", path);
    }

    [Fact]
    public async Task GetAnalyticsAsync_SendsEveryQueryField_DatesAsIsoDays()
    {
        _server.ResultJson = AnalyticsSampleJson;

        await _client.GetAnalyticsAsync("l1", new LinkAnalyticsQuery
        {
            From = new DateTime(2026, 9, 1, 23, 30, 0),
            To = new DateTime(2026, 9, 30),
            Interval = LinkAnalyticsInterval.Week,
            Tz = "Africa/Cairo",
            Breakdown = new[] { LinkAnalyticsBreakdown.Country, LinkAnalyticsBreakdown.Device },
            Limit = 5
        });

        var query = Query(_server.Requests.Single().PathAndQuery);
        Assert.Equal("2026-09-01", query["from"]);
        Assert.Equal("2026-09-30", query["to"]);
        Assert.Equal("week", query["interval"]);
        Assert.Equal("Africa/Cairo", query["tz"]);
        Assert.Equal("country,device", query["breakdown"]);
        Assert.Equal("5", query["limit"]);
    }

    [Fact]
    public async Task GetAnalyticsAsync_DatesIgnoreTheCurrentCulture()
    {
        _server.ResultJson = AnalyticsSampleJson;
        var culture = CultureInfo.CurrentCulture;
        try
        {
            // ar-SA formats dates on the Hijri calendar by default.
            CultureInfo.CurrentCulture = new CultureInfo("ar-SA");
            await _client.GetAnalyticsAsync("l1", new LinkAnalyticsQuery { From = new DateTime(2026, 9, 1) });
        }
        finally
        {
            CultureInfo.CurrentCulture = culture;
        }

        Assert.Equal("2026-09-01", Query(_server.Requests.Single().PathAndQuery)["from"]);
    }

    [Fact]
    public async Task GetAnalyticsAsync_AllBreakdowns_SendsBreakdownAll()
    {
        _server.ResultJson = AnalyticsSampleJson;

        await _client.GetAnalyticsAsync("l1", new LinkAnalyticsQuery { AllBreakdowns = true });

        Assert.Equal("all", Query(_server.Requests.Single().PathAndQuery)["breakdown"]);
    }

    [Fact]
    public async Task GetAnalyticsAsync_EmptyBreakdownList_OmitsTheParameter()
    {
        _server.ResultJson = AnalyticsSampleJson;

        await _client.GetAnalyticsAsync("l1", new LinkAnalyticsQuery { Breakdown = Array.Empty<LinkAnalyticsBreakdown>() });

        Assert.Equal("/api/short-link/l1/analytics", _server.Requests.Single().PathAndQuery);
    }

    [Fact]
    public async Task GetAnalyticsAsync_AllBreakdownsAndAList_OrAnEmptyId_ThrowsBeforeSending()
    {
        await Assert.ThrowsAsync<ArgumentException>(() => _client.GetAnalyticsAsync("l1", new LinkAnalyticsQuery
        {
            AllBreakdowns = true,
            Breakdown = new[] { LinkAnalyticsBreakdown.Country }
        }));
        await Assert.ThrowsAsync<ArgumentException>(() => _client.GetAnalyticsAsync(" "));

        Assert.Empty(_server.Requests);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(51)]
    [InlineData(-1)]
    public async Task GetAnalyticsAsync_LimitOutside1To50_ThrowsBeforeSending(int limit)
    {
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
            _client.GetAnalyticsAsync("l1", new LinkAnalyticsQuery { Limit = limit }));

        Assert.Empty(_server.Requests);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(50)]
    public async Task GetAnalyticsAsync_LimitAtTheBounds_IsSent(int limit)
    {
        _server.ResultJson = AnalyticsSampleJson;

        await _client.GetAnalyticsAsync("l1", new LinkAnalyticsQuery { Limit = limit });

        Assert.Equal(limit.ToString(CultureInfo.InvariantCulture), Query(_server.Requests.Single().PathAndQuery)["limit"]);
    }

    [Fact]
    public async Task GetAnalyticsAsync_MissingLink_Is400WithTheApiMessage()
    {
        _server.Status = HttpStatusCode.BadRequest;
        _server.Message = "The Short Link Is Not Found";
        _server.ResultJson = "null";

        var error = await Assert.ThrowsAsync<Posty5ValidationException>(() => _client.GetAnalyticsAsync("missing"));

        Assert.Equal(400, error.StatusCode);
        Assert.Contains("The Short Link Is Not Found", error.Message);
    }

    [Fact]
    public async Task GetAnalyticsAsync_StarterAnswer_HasNullMaxHistoryDays()
    {
        _server.ResultJson = AnalyticsSampleJson
            .Replace("\"maxHistoryDays\": 30", "\"maxHistoryDays\": null")
            .Replace("\"source\": \"events\"", "\"source\": \"mixed\"");

        var analytics = await _client.GetAnalyticsAsync("l1");

        Assert.Null(analytics.Meta.MaxHistoryDays);
        Assert.Equal("mixed", analytics.Meta.Source);
    }

    [Fact]
    public async Task GetAnalyticsAsync_ReadsTheWholeAnswer()
    {
        _server.ResultJson = AnalyticsSampleJson;

        var analytics = await _client.GetAnalyticsAsync("l1");

        Assert.Equal(12, analytics.Totals.Visits);
        Assert.Equal(9, analytics.Totals.UniqueVisitors);
        Assert.Equal(4, analytics.Totals.BotVisits);
        Assert.Equal(new[] { "2026-10-01", "2026-10-02" }, analytics.Series.Select(point => point.Date));
        Assert.Equal(7, analytics.Series[1].Visits);
        Assert.Equal("scan", analytics.Breakdowns["channel"][1].Key);
        Assert.Equal(3, analytics.Breakdowns["channel"][1].UniqueVisitors);
        Assert.Equal(new[] { "unknown", "other" }, analytics.Breakdowns["referrer"].Select(row => row.Key));
        Assert.Equal("Africa/Cairo", analytics.Meta.Timezone);
        Assert.Equal("events", analytics.Meta.Source);
        Assert.Equal("2026-10-01", analytics.Meta.AnalyticsStartedAt);
        Assert.Equal(30, analytics.Meta.MaxHistoryDays);
        var locked = Assert.Single(analytics.Meta.Locked);
        Assert.Equal("country", locked.Breakdown);
        Assert.Equal("basic", locked.RequiredPlan);
    }

    [Fact]
    public async Task GetAnalyticsAsync_FeatureLock403_CarriesTheApiMessage_InResponseBody()
    {
        const string apiMessage = "This feature is not available on your current plan.";
        _server.Status = HttpStatusCode.Forbidden;
        _server.Message = apiMessage;
        _server.ResultJson = "null";

        var error = await Assert.ThrowsAsync<Posty5Exception>(() => _client.GetAnalyticsAsync("l1", new LinkAnalyticsQuery
        {
            Breakdown = new[] { LinkAnalyticsBreakdown.Country }
        }));

        Assert.Equal(403, error.StatusCode);
        Assert.Contains(apiMessage, error.ResponseBody);
    }

    /// <summary>The decoded query of a recorded request.</summary>
    internal static NameValueCollection Query(string pathAndQuery) =>
        HttpUtility.ParseQueryString(new Uri(new Uri("http://localhost"), pathAndQuery).Query);

    // ─── GetStatisticsAsync (VA) ─────────────────────────────────────────────

    /// <summary>A rebuilt <c>/api/short-link/statistics</c> answer inside <c>result</c>.</summary>
    internal const string StatisticsSampleJson = """
        {
          "range": { "from": "2026-09-06T00:00:00.000Z", "to": "2026-10-05T23:59:59.999Z", "period": "30d" },
          "data": {
            "totals": { "totalLinks": 4, "totalVisitors": 120, "avgVisitorsPerLink": 30, "visitsInRange": 18, "uniqueVisitorsInRange": 11, "botVisitsInRange": 6 },
            "daily": [
              { "_id": "2026-10-01", "createdCount": 2, "visitorsSum": 0 },
              { "_id": "2026-10-02", "createdCount": 0, "visitorsSum": 18 }
            ],
            "topLinks": [
              { "_id": "l1", "name": "Spring", "baseUrl": "https://example.com", "shortLinkId": "abc", "numberOfVisitors": 90, "createdAt": "2026-10-01T10:00:00.000Z", "visitsInRange": 18 }
            ]
          }
        }
        """;

    [Fact]
    public async Task GetStatisticsAsync_NoQuery_CallsStatistics_WithNoQueryString()
    {
        _server.ResultJson = StatisticsSampleJson;

        await _client.GetStatisticsAsync();

        var (method, path, _) = _server.Requests.Single();
        Assert.Equal("GET", method);
        Assert.Equal("/api/short-link/statistics", path);
    }

    [Fact]
    public async Task GetStatisticsAsync_SendsPeriod()
    {
        _server.ResultJson = StatisticsSampleJson;

        await _client.GetStatisticsAsync(new LinkStatisticsQuery { Period = LinkStatisticsPeriod.Last7Days });

        var query = Query(_server.Requests.Single().PathAndQuery);
        Assert.Equal("7d", query["period"]);
        Assert.Null(query["from"]);
    }

    [Fact]
    public async Task GetStatisticsAsync_CustomRange_SendsIsoDays()
    {
        _server.ResultJson = StatisticsSampleJson;

        await _client.GetStatisticsAsync(new LinkStatisticsQuery
        {
            Period = LinkStatisticsPeriod.Custom,
            From = new DateTime(2026, 9, 1, 18, 0, 0),
            To = new DateTime(2026, 9, 30)
        });

        var query = Query(_server.Requests.Single().PathAndQuery);
        Assert.Equal("custom", query["period"]);
        Assert.Equal("2026-09-01", query["from"]);
        Assert.Equal("2026-09-30", query["to"]);
    }

    [Fact]
    public async Task GetStatisticsAsync_DatesWithAPresetPeriod_OrFromAfterTo_ThrowBeforeSending()
    {
        await Assert.ThrowsAsync<ArgumentException>(() => _client.GetStatisticsAsync(new LinkStatisticsQuery
        {
            Period = LinkStatisticsPeriod.Today,
            From = new DateTime(2026, 9, 1)
        }));
        await Assert.ThrowsAsync<ArgumentException>(() => _client.GetStatisticsAsync(new LinkStatisticsQuery
        {
            From = new DateTime(2026, 9, 30),
            To = new DateTime(2026, 9, 1)
        }));

        Assert.Empty(_server.Requests);
    }

    [Fact]
    public async Task GetStatisticsAsync_ReadsTheRebuiltAnswer()
    {
        _server.ResultJson = StatisticsSampleJson;

        var stats = await _client.GetStatisticsAsync();

        Assert.Equal("30d", stats.Range.Period);
        Assert.Equal(new DateTime(2026, 9, 6, 0, 0, 0, DateTimeKind.Utc), stats.Range.From?.ToUniversalTime());
        Assert.Equal(4, stats.Data.Totals.TotalLinks);
        Assert.Equal(120, stats.Data.Totals.TotalVisitors);
        Assert.Equal(30, stats.Data.Totals.AvgVisitorsPerLink);
        Assert.Equal(18, stats.Data.Totals.VisitsInRange);
        Assert.Equal(11, stats.Data.Totals.UniqueVisitorsInRange);
        Assert.Equal(6, stats.Data.Totals.BotVisitsInRange);
        Assert.Equal(new[] { "2026-10-01", "2026-10-02" }, stats.Data.Daily.Select(day => day.Day));
        Assert.Equal(2, stats.Data.Daily[0].CreatedCount);
        Assert.Equal(18, stats.Data.Daily[1].VisitorsSum);
        var top = Assert.Single(stats.Data.TopLinks);
        Assert.Equal("l1", top.Id);
        Assert.Equal("abc", top.ShortLinkId);
        Assert.Equal(90, top.NumberOfVisitors);
        Assert.Equal(18, top.VisitsInRange);
    }
}
