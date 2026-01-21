using Xunit;
using Posty5.HtmlHosting;
using Posty5.HtmlHosting.Models;
using Posty5.Core.Exceptions;

namespace Posty5.Tests.Integration;

[Collection("Sequential")]
public class HtmlHostingClientTests : IDisposable
{
    private readonly HtmlHostingClient _client;
    private string? _createdId;

    public HtmlHostingClientTests()
    {
        var httpClient = TestConfig.CreateHttpClient();
        _client = new HtmlHostingClient(httpClient);
    }

    private string GetContactFormHtml()
    {
        var htmlPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "contact_form.html");
        if (!File.Exists(htmlPath))
        {
            throw new FileNotFoundException($"Test asset not found: {htmlPath}");
        }
        return File.ReadAllText(htmlPath);
    }

    [Fact]
    public async Task CreateHtmlHosting_WithHtmlContent_ShouldReturnValidPage()
    {
        // Arrange
        var htmlContent = GetContactFormHtml();
        var request = new CreateHtmlHostingRequest
        {
            Name = $"Test Contact Form - {DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}",
            HtmlContent = htmlContent,
            TemplateId = TestConfig.TemplateId
        };

        // Act
        var result = await _client.CreateAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Id);
        Assert.NotNull(result.PublicUrl);

        _createdId = result.Id;
        TestConfig.CreatedResources.HtmlHostings.Add(_createdId);
    }

    [Fact]
    public async Task CreateHtmlHosting_WithSimpleHtml_ShouldReturnValidPage()
    {
        // Arrange
        var htmlContent = @"
<!DOCTYPE html>
<html>
<head>
    <title>Test Page</title>
    <style>
        body { font-family: Arial; text-align: center; padding: 50px; }
        h1 { color: #333; }
    </style>
</head>
<body>
    <h1>Welcome to Test Page</h1>
    <p>This is hosted via Posty5 .NET SDK!</p>
</body>
</html>";

        var request = new CreateHtmlHostingRequest
        {
            Name = $"Simple Test Page - {DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}",
            HtmlContent = htmlContent
        };

        // Act
        var result = await _client.CreateAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Id);
        Assert.NotNull(result.PublicUrl);

        TestConfig.CreatedResources.HtmlHostings.Add(result.Id!);
    }

    [Fact]
    public async Task GetHtmlHostingById_WithValidId_ShouldReturnPage()
    {
        // Arrange - Create a page first
        var htmlContent = "<html><body><h1>Test</h1></body></html>";
        var createRequest = new CreateHtmlHostingRequest
        {
            Name = "Test Page for Get",
            HtmlContent = htmlContent
        };
        var created = await _client.CreateAsync(createRequest);
        TestConfig.CreatedResources.HtmlHostings.Add(created.Id!);

        // Act
        var result = await _client.GetAsync(created.Id!);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(created.Id, result.Id);
        Assert.NotNull(result.Name);
        Assert.NotNull(result.PublicUrl);
    }

    [Fact]
    public async Task GetHtmlHostingById_WithInvalidId_ShouldThrowException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<Posty5Exception>(async () =>
            await _client.GetAsync("invalid-id-123")
        );
    }

    [Fact]
    public async Task ListHtmlHostings_ShouldReturnPaginatedResults()
    {
        // Act
        var result = await _client.ListAsync(
            pagination: new Core.Models.PaginationParams { PageNumber = 0, PageSize = 10 }
        );

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Items);
        Assert.True(result.TotalCount >= 0);
    }

    [Fact]
    public async Task ListHtmlHostings_WithSearch_ShouldFilterResults()
    {
        // Arrange
        var searchParams = new ListHtmlHostingParams
        {
            Search = "test"
        };

        // Act
        var result = await _client.ListAsync(
            searchParams,
            new Core.Models.PaginationParams { PageNumber = 0, PageSize = 10 }
        );

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Items);
    }

    [Fact]
    public async Task UpdateHtmlHosting_ShouldUpdateSuccessfully()
    {
        // Arrange - Create a page first
        var originalHtml = "<html><body><h1>Original</h1></body></html>";
        var createRequest = new CreateHtmlHostingRequest
        {
            Name = "Original Page",
            HtmlContent = originalHtml
        };
        var created = await _client.CreateAsync(createRequest);
        TestConfig.CreatedResources.HtmlHostings.Add(created.Id!);

        // Act
        var updatedHtml = "<html><body><h1>Updated Content</h1></body></html>";
        var updateRequest = new UpdateHtmlHostingRequest
        {
            HtmlContent = updatedHtml,
            Name = $"Updated Page - {DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}"
        };
        var result = await _client.UpdateAsync(created.Id!, updateRequest);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(created.Id, result.Id);
    }

    [Fact]
    public async Task UpdateHtmlHosting_ContentOnly_ShouldUpdateSuccessfully()
    {
        // Arrange - Create a page first
        var createRequest = new CreateHtmlHostingRequest
        {
            Name = "Page to Update Content",
            HtmlContent = "<html><body><h1>Original</h1></body></html>"
        };
        var created = await _client.CreateAsync(createRequest);
        TestConfig.CreatedResources.HtmlHostings.Add(created.Id!);

        // Act
        var updateRequest = new UpdateHtmlHostingRequest
        {
            HtmlContent = "<html><body><h1>New Content</h1><p>Updated!</p></body></html>"
        };
        var result = await _client.UpdateAsync(created.Id!, updateRequest);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(created.Id, result.Id);
    }

    [Fact]
    public async Task DeleteHtmlHosting_ShouldDeleteSuccessfully()
    {
        // Arrange - Create a page first
        var createRequest = new CreateHtmlHostingRequest
        {
            Name = "Page to Delete",
            HtmlContent = "<html><body><h1>Delete Me</h1></body></html>"
        };
        var created = await _client.CreateAsync(createRequest);

        // Act
        var deleteResult = await _client.DeleteAsync(created.Id!);

        // Assert
        Assert.True(deleteResult);

        // Verify deletion
        await Assert.ThrowsAsync<Posty5Exception>(async () =>
            await _client.GetAsync(created.Id!)
        );
    }

    public void Dispose()
    {
        // Cleanup is handled by collection fixture if needed
    }
}
