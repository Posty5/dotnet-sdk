using Xunit;
using Posty5.HtmlHosting;
using Posty5.HtmlHosting.Models;
using Posty5.Core.Models;
using System.Text;

namespace Posty5.Tests.Integration;

[Collection("Sequential")]
public class HtmlHostingClientTests : IDisposable
{
    private readonly HtmlHostingClient _client;
    private readonly string _testHtmlPath;

    public HtmlHostingClientTests()
    {
        var httpClient = TestConfig.CreateHttpClient();
        _client = new HtmlHostingClient(httpClient);
        _testHtmlPath = Path.Combine("Assets", "contact_form.html");
    }

    #region Create Tests

    [Fact]
    public async Task CreateWithFile_ShouldReturnValidResponse()
    {
        // Arrange
        var fileName = $"test-page-{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}.html";
        var request = new HtmlHostingCreatePageFileRequestModel
        {
            Name = $"Test HTML Page - {DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}",
            FileName = fileName,
            Tag = "test",
            RefId = "test-create-001"
        };

        using var fileStream = File.OpenRead(_testHtmlPath);

        // Act
        var result = await _client.CreateWithFileAsync(request, fileStream);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Id);
        Assert.NotNull(result.ShorterLink);
        Assert.NotNull(result.FileUrl);

        TestConfig.CreatedResources.HtmlHostings.Add(result.Id);
    }

    [Fact]
    public async Task CreateWithFile_FromMemoryStream_ShouldSucceed()
    {
        // Arrange
        var htmlContent = @"
<!DOCTYPE html>
<html>
<head>
    <title>Test Page</title>
</head>
<body>
    <h1>Hello from .NET SDK Test!</h1>
    <p>This page was created using a MemoryStream.</p>
</body>
</html>";

        var request = new HtmlHostingCreatePageFileRequestModel
        {
            Name = $"Memory Stream Page - {DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}",
            FileName = "memory-test.html",
            IsEnableMonetization = true
        };

        using var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(htmlContent));

        // Act
        var result = await _client.CreateWithFileAsync(request, memoryStream, "text/html");

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Id);
        Assert.NotNull(result.ShorterLink);
        Assert.NotNull(result.FileUrl);

        TestConfig.CreatedResources.HtmlHostings.Add(result.Id);
    }

    [Fact]
    public async Task CreateWithGithubFile_ShouldReturnValidResponse()
    {
        // Arrange
        var request = new HtmlHostingCreatePageGithubRequestModel
        {
            Name = $"GitHub Page - {DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}",
            GithubInfo = new HtmlHostingGithubInfoModel
            {
                FileURL = "https://raw.githubusercontent.com/posty5/examples/main/index.html"
            },
            Tag = "github-test"
        };

        // Act
        var result = await _client.CreateWithGithubFileAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Id);
        Assert.NotNull(result.ShorterLink);
        Assert.NotNull(result.GithubInfo);
        Assert.Equal(request.GithubInfo.FileURL, result.GithubInfo.FileURL);

        TestConfig.CreatedResources.HtmlHostings.Add(result.Id);
    }

    [Fact]
    public async Task CreateWithFile_WithCustomLandingId_ShouldContainSlug()
    {
        // Arrange
        var customSlug = $"html-test-{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}";
        var request = new HtmlHostingCreatePageFileRequestModel
        {
            Name = "Custom Slug HTML Page",
            FileName = "custom-slug.html",
            CustomLandingId = customSlug
        };

        var htmlContent = "<html><body><h1>Custom Slug Page</h1></body></html>";
        using var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(htmlContent));

        // Act
        var result = await _client.CreateWithFileAsync(request, memoryStream);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Id);
        Assert.Contains(customSlug, result.ShorterLink);

        TestConfig.CreatedResources.HtmlHostings.Add(result.Id);
    }

    [Fact]
    public async Task CreateWithFile_WithAutoSaveGoogleSheet_ShouldSucceed()
    {
        // Arrange
        var request = new HtmlHostingCreatePageFileRequestModel
        {
            Name = $"Form Page - {DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}",
            FileName = "contact_form.html",
            AutoSaveInGoogleSheet = true,
            Tag = "forms"
        };

        using var fileStream = File.OpenRead(_testHtmlPath);

        // Act
        var result = await _client.CreateWithFileAsync(request, fileStream);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Id);

        TestConfig.CreatedResources.HtmlHostings.Add(result.Id);
    }

    #endregion

    #region Read Tests

    [Fact]
    public async Task Get_WithValidId_ShouldReturnPage()
    {
        // Arrange - Create a page first
        var createRequest = new HtmlHostingCreatePageFileRequestModel
        {
            Name = "Page for Get Test",
            FileName = "get-test.html"
        };

        var htmlContent = "<html><body><h1>Get Test</h1></body></html>";
        using var createStream = new MemoryStream(Encoding.UTF8.GetBytes(htmlContent));
        var created = await _client.CreateWithFileAsync(createRequest, createStream);
        TestConfig.CreatedResources.HtmlHostings.Add(created.Id!);

        // Act
        var result = await _client.GetAsync(created.Id!);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(created.Id, result.Id);
        Assert.NotNull(result.Name);
        Assert.NotNull(result.ShorterLink);
        Assert.Equal("file", result.SourceType);
        Assert.NotNull(result.FileUrl);
    }

    [Fact]
    public async Task List_ShouldReturnPaginatedResults()
    {
        // Act
        var result = await _client.ListAsync(
            pagination: new PaginationParams { Page = 1, PageSize = 10 }
        );

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Items);
        Assert.True(result.Items.Count <= 10);
    }

    [Fact]
    public async Task List_WithFilters_ShouldFilterResults()
    {
        // Arrange
        var filterParams = new HtmlHostingListPagesParamsModel
        {
            SourceType = "file",
            Tag = "test"
        };

        // Act
        var result = await _client.ListAsync(
            filterParams,
            new PaginationParams { Page = 1, PageSize = 20 }
        );

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Items);
        
        // All returned items should be of type "file"
        foreach (var item in result.Items)
        {
            if (item.SourceType != null)
            {
                Assert.Equal("file", item.SourceType);
            }
        }
    }

    [Fact]
    public async Task List_WithNameSearch_ShouldFilterByName()
    {
        // Arrange
        var searchParams = new HtmlHostingListPagesParamsModel
        {
            Name = "Test"
        };

        // Act
        var result = await _client.ListAsync(searchParams);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Items);
    }

    [Fact]
    public async Task Lookup_ShouldReturnSimplifiedList()
    {
        // Act
        var result = await _client.LookupAsync();

        // Assert
        Assert.NotNull(result);
        Assert.IsType<List<HtmlHostingPageLookupItemModel>>(result);
        
        // Verify structure if items exist
        if (result.Count > 0)
        {
            var firstItem = result[0];
            Assert.NotNull(firstItem.Id);
            Assert.NotNull(firstItem.Name);
        }
    }

    [Fact]
    public async Task LookupForms_WithValidPageId_ShouldReturnForms()
    {
        // Arrange - Create a page with a form first
        var createRequest = new HtmlHostingCreatePageFileRequestModel
        {
            Name = "Form Lookup Test",
            FileName = "contact_form.html"
        };

        using var fileStream = File.OpenRead(_testHtmlPath);
        var created = await _client.CreateWithFileAsync(createRequest, fileStream);
        TestConfig.CreatedResources.HtmlHostings.Add(created.Id!);

        // Wait a bit for form processing
        await Task.Delay(2000);

        // Act
        var result = await _client.LookupFormsAsync(created.Id!);

        // Assert
        Assert.NotNull(result);
        Assert.IsType<List<HtmlHostingFormLookupItemModel>>(result);
    }

    #endregion

    #region Update Tests

    [Fact]
    public async Task UpdateWithFile_ShouldUpdateSuccessfully()
    {
        // Arrange - Create a page first
        var createRequest = new HtmlHostingCreatePageFileRequestModel
        {
            Name = "Original Page",
            FileName = "original.html"
        };

        var originalContent = "<html><body><h1>Original</h1></body></html>";
        using var createStream = new MemoryStream(Encoding.UTF8.GetBytes(originalContent));
        var created = await _client.CreateWithFileAsync(createRequest, createStream);
        TestConfig.CreatedResources.HtmlHostings.Add(created.Id!);

        // Act
        var updateRequest = new HtmlHostingUpdatePageFileRequestModel
        {
            Name = "Updated Page",
            FileName = "updated.html"
        };

        var updatedContent = "<html><body><h1>Updated Content</h1></body></html>";
        using var updateStream = new MemoryStream(Encoding.UTF8.GetBytes(updatedContent));
        var result = await _client.UpdateWithFileAsync(created.Id!, updateRequest, updateStream);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(created.Id, result.Id);
        Assert.NotNull(result.ShorterLink);
        Assert.NotNull(result.FileUrl);
    }

    [Fact]
    public async Task UpdateWithGithubFile_ShouldUpdateSuccessfully()
    {
        // Arrange - Create a GitHub page first
        var createRequest = new HtmlHostingCreatePageGithubRequestModel
        {
            Name = "GitHub Page to Update",
            GithubInfo = new HtmlHostingGithubInfoModel
            {
                FileURL = "https://raw.githubusercontent.com/posty5/examples/main/index.html"
            }
        };

        var created = await _client.CreateWithGithubFileAsync(createRequest);
        TestConfig.CreatedResources.HtmlHostings.Add(created.Id!);

        // Act
        var updateRequest = new HtmlHostingUpdatePageGithubRequestModel
        {
            Name = "Updated GitHub Page",
            GithubInfo = new HtmlHostingGithubInfoModel
            {
                FileURL = "https://raw.githubusercontent.com/posty5/examples/main/about.html"
            }
        };

        var result = await _client.UpdateWithGithubFileAsync(created.Id!, updateRequest);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(created.Id, result.Id);
        Assert.NotNull(result.GithubInfo);
    }

    #endregion

    #region Delete and Cache Tests

    [Fact]
    public async Task Delete_ShouldDeleteSuccessfully()
    {
        // Arrange - Create a page first
        var createRequest = new HtmlHostingCreatePageFileRequestModel
        {
            Name = "Page to Delete",
            FileName = "delete-test.html"
        };

        var htmlContent = "<html><body><h1>Delete Me</h1></body></html>";
        using var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(htmlContent));
        var created = await _client.CreateWithFileAsync(createRequest, memoryStream);

        // Act
        await _client.DeleteAsync(created.Id!);

        // Assert - Remove from tracking since we successfully deleted it
        // (No exception means success)
    }

    [Fact]
    public async Task CleanCache_WithValidId_ShouldSucceed()
    {
        // Arrange - Create a page first
        var createRequest = new HtmlHostingCreatePageFileRequestModel
        {
            Name = "Page for Cache Test",
            FileName = "cache-test.html"
        };

        var htmlContent = "<html><body><h1>Cache Test</h1></body></html>";
        using var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(htmlContent));
        var created = await _client.CreateWithFileAsync(createRequest, memoryStream);
        TestConfig.CreatedResources.HtmlHostings.Add(created.Id!);

        // Act
        await _client.CleanCacheAsync(created.Id!);

        // Assert - No exception means success
        Assert.True(true);
    }

    #endregion

    #region Edge Cases and Validation Tests

    [Fact]
    public async Task CreateWithFile_WithAllParameters_ShouldSucceed()
    {
        // Arrange - Test all optional parameters
        var request = new HtmlHostingCreatePageFileRequestModel
        {
            Name = "Full Parameters Test",
            FileName = "full-params.html",
            Tag = "comprehensive-test",
            RefId = "ref-12345",
            IsEnableMonetization = true,
            AutoSaveInGoogleSheet = true,
            CustomLandingId = $"full-test-{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}"
        };

        var htmlContent = "<html><body><h1>Full Parameters</h1></body></html>";
        using var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(htmlContent));

        // Act
        var result = await _client.CreateWithFileAsync(request, memoryStream);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Id);
        Assert.NotNull(result.ShorterLink);

        TestConfig.CreatedResources.HtmlHostings.Add(result.Id);
    }

    [Fact]
    public async Task List_WithMultipleFilters_ShouldCombineFilters()
    {
        // Arrange
        var filterParams = new HtmlHostingListPagesParamsModel
        {
            SourceType = "file",
            IsEnableMonetization = true,
            Tag = "test"
        };

        // Act
        var result = await _client.ListAsync(
            filterParams,
            new PaginationParams { Page = 1, PageSize = 5 }
        );

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Items);
        Assert.True(result.Items.Count <= 5);
    }

    [Fact]
    public async Task CreateAndRetrieve_Workflow_ShouldMaintainData()
    {
        // Arrange
        var testTag = $"workflow-{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}";
        var createRequest = new HtmlHostingCreatePageFileRequestModel
        {
            Name = "Workflow Test Page",
            FileName = "workflow.html",
            Tag = testTag,
            RefId = "workflow-001"
        };

        var htmlContent = "<html><body><h1>Workflow Test</h1></body></html>";
        using var createStream = new MemoryStream(Encoding.UTF8.GetBytes(htmlContent));

        // Act
        var created = await _client.CreateWithFileAsync(createRequest, createStream);
        TestConfig.CreatedResources.HtmlHostings.Add(created.Id!);

        var retrieved = await _client.GetAsync(created.Id!);

        // Assert
        Assert.Equal(created.Id, retrieved.Id);
        Assert.Equal("Workflow Test Page", retrieved.Name);
        Assert.Equal("file", retrieved.SourceType);
        Assert.Equal(testTag, retrieved.Tag);
        Assert.Equal("workflow-001", retrieved.RefId);
    }

    #endregion

    public void Dispose()
    {
        // Cleanup is handled by test framework if needed
    }
}
