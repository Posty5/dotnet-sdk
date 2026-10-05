using System.Text.Json;
using Xunit;
using Posty5.ShortLink;
using Posty5.ShortLink.Models;
using Posty5.Core.Configuration;
using Posty5.Core.Exceptions;
using Posty5.Core.Http;
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
}

