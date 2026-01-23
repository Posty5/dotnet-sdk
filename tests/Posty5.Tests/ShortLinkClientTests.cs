using Xunit;
using Posty5.ShortLink;
using Posty5.ShortLink.Models;
using Posty5.Core.Exceptions;

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
        var request = new CreateShortLinkRequest
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
        var request = new CreateShortLinkRequest
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
        var createRequest = new CreateShortLinkRequest
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
            pagination: new Core.Models.PaginationParams { Page = 0, PageSize = 10 }
        );

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Items);
    }

    [Fact]
    public async Task ListShortLinks_WithSearch_ShouldFilterResults()
    {
        // Arrange
        var searchParams = new ListShortLinksParams
        {
            Search = "test"
        };

        // Act
        var result = await _client.ListAsync(
            searchParams,
            new Core.Models.PaginationParams { Page = 0, PageSize = 10 }
        );

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Items);
    }

    [Fact]
    public async Task UpdateShortLink_ShouldUpdateSuccessfully()
    {
        // Arrange - Create a short link first
        var createRequest = new CreateShortLinkRequest
        {
            Name = "Original Name",
            BaseUrl = "https://posty5.com",
            TemplateId = TestConfig.TemplateId
        };
        var created = await _client.CreateAsync(createRequest);
        TestConfig.CreatedResources.ShortLinks.Add(created.Id!);

        // Act
        var newName = $"Updated Short Link - {DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}";
        var updateRequest = new UpdateShortLinkRequest
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
        var createRequest = new CreateShortLinkRequest
        {
            Name = "Link to Update URL",
            BaseUrl = "https://posty5.com",
            TemplateId = TestConfig.TemplateId
        };
        var created = await _client.CreateAsync(createRequest);
        TestConfig.CreatedResources.ShortLinks.Add(created.Id!);

        // Act
        var updateRequest = new UpdateShortLinkRequest
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
        var createRequest = new CreateShortLinkRequest
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

    public void Dispose()
    {
        // Cleanup is handled by collection fixture if needed
    }
}
