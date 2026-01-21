using Xunit;
using Posty5.SocialPublisher;
using Posty5.SocialPublisher.Models;
using Posty5.Core.Exceptions;

namespace Posty5.Tests.Integration;

[Collection("Sequential")]
public class SocialPublisherWorkspaceTests : IDisposable
{
    private readonly WorkspaceClient _client;
    private string? _createdId;

    public SocialPublisherWorkspaceTests()
    {
        var httpClient = TestConfig.CreateHttpClient();
        _client = new WorkspaceClient(httpClient);
    }

    [Fact]
    public async Task CreateWorkspace_ShouldReturnValidWorkspace()
    {
        // Arrange
        var request = new CreateWorkspaceRequest
        {
            Name = $"Test Workspace - {DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}",
            Description = "Workspace created during SDK tests"
        };

        // Act
        var result = await _client.CreateAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Id);
        Assert.Equal(request.Name, result.Name);

        _createdId = result.Id;
        TestConfig.CreatedResources.Workspaces.Add(_createdId);
    }

    [Fact]
    public async Task CreateWorkspace_WithDescriptionOnly_ShouldReturnValidWorkspace()
    {
        // Arrange
        var request = new CreateWorkspaceRequest
        {
            Name = $"Tagged Workspace {DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}",
            Description = "Workspace with description created during SDK tests"
        };

        // Act
        var result = await _client.CreateAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Id);

        TestConfig.CreatedResources.Workspaces.Add(result.Id!);
    }

    [Fact]
    public async Task GetWorkspaceById_WithValidId_ShouldReturnWorkspace()
    {
        // Arrange - Create a workspace first
        var createRequest = new CreateWorkspaceRequest
        {
            Name = "Test Workspace for Get",
            Description = "Test description"
        };
        var created = await _client.CreateAsync(createRequest);
        TestConfig.CreatedResources.Workspaces.Add(created.Id!);

        // Act
        var result = await _client.GetAsync(created.Id!);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(created.Id, result.Id);
        Assert.NotNull(result.Name);
    }

    [Fact]
    public async Task GetWorkspaceById_WithInvalidId_ShouldThrowException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<Posty5Exception>(async () =>
            await _client.GetAsync("invalid-id-123")
        );
    }

    [Fact]
    public async Task ListWorkspaces_ShouldReturnPaginatedResults()
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
    public async Task ListWorkspaces_WithSearch_ShouldFilterResults()
    {
        // Arrange
        var searchParams = new ListWorkspacesParams
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
    public async Task UpdateWorkspace_ShouldUpdateSuccessfully()
    {
        // Arrange - Create a workspace first
        var createRequest = new CreateWorkspaceRequest
        {
            Name = "Original Workspace Name",
            Description = "Original description"
        };
        var created = await _client.CreateAsync(createRequest);
        TestConfig.CreatedResources.Workspaces.Add(created.Id!);

        // Act
        var newName = $"Updated Workspace - {DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}";
        var updateRequest = new UpdateWorkspaceRequest
        {
            Name = newName,
            Description = "Updated description"
        };
        var result = await _client.UpdateAsync(created.Id!, updateRequest);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(created.Id, result.Id);
    }

    [Fact]
    public async Task UpdateWorkspace_DescriptionOnly_ShouldUpdateSuccessfully()
    {
        // Arrange - Create a workspace first
        var createRequest = new CreateWorkspaceRequest
        {
            Name = "Workspace to Update Description",
            Description = "Original description"
        };
        var created = await _client.CreateAsync(createRequest);
        TestConfig.CreatedResources.Workspaces.Add(created.Id!);

        // Act
        var updateRequest = new UpdateWorkspaceRequest
        {
            Description = "New updated description"
        };
        var result = await _client.UpdateAsync(created.Id!, updateRequest);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(created.Id, result.Id);
    }

    [Fact]
    public async Task DeleteWorkspace_ShouldDeleteSuccessfully()
    {
        // Arrange - Create a workspace first
        var createRequest = new CreateWorkspaceRequest
        {
            Name = "Workspace to Delete",
            Description = "This workspace will be deleted"
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
