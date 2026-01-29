using Xunit;
using Posty5.SocialPublisherWorkspace;
using Posty5.SocialPublisherWorkspace.Models;
using Posty5.Core.Models;
using System.Text;

namespace Posty5.Tests.Integration;

[Collection("Sequential")]
public class SocialPublisherWorkspaceClientTests : IDisposable
{
    private readonly SocialPublisherWorkspaceClient _client;
    private readonly string _testImagePath;

    public SocialPublisherWorkspaceClientTests()
    {
        var httpClient = TestConfig.CreateHttpClient();
        _client = new SocialPublisherWorkspaceClient(httpClient);
        _testImagePath = Path.Combine("Assets", "logo.png");
    }

    #region Create Tests

    [Fact]
    public async Task Create_WithoutImage_ShouldReturnWorkspaceId()
    {
        // Arrange
        var request = new CreateWorkspaceRequest
        {
            Name = $"Test Workspace - {DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}",
            Description = "Test workspace without image",
            Tag = "test"
        };

        // Act
        var workspaceId = await _client.CreateAsync(request);

        // Assert
        Assert.NotNull(workspaceId);
        Assert.NotEmpty(workspaceId);

        TestConfig.CreatedResources.Workspaces.Add(workspaceId);
    }

    [Fact]
    public async Task Create_WithImage_ShouldUploadAndReturnWorkspaceId()
    {
        // Arrange
        var request = new CreateWorkspaceRequest
        {
            Name = $"Workspace with Image - {DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}",
            Description = "Test workspace with logo",
            Tag = "test-image"
        };

        using var imageStream = File.OpenRead(_testImagePath);

        // Act
        var workspaceId = await _client.CreateAsync(request, imageStream, "image/png");

        // Assert
        Assert.NotNull(workspaceId);
        Assert.NotEmpty(workspaceId);

        TestConfig.CreatedResources.Workspaces.Add(workspaceId);
    }

    [Fact]
    public async Task Create_WithMemoryStreamImage_ShouldSucceed()
    {
        // Arrange
        var request = new CreateWorkspaceRequest
        {
            Name = "Memory Stream Workspace"+ new Random().Next(),
            Description = "Using memory stream for image"
        };

        // Create a simple 1x1 PNG in memory
        byte[] pngBytes = File.ReadAllBytes(_testImagePath);
        using var memoryStream = new MemoryStream(pngBytes);

        // Act
        var workspaceId = await _client.CreateAsync(request, memoryStream, "image/png");

        // Assert
        Assert.NotNull(workspaceId);

        TestConfig.CreatedResources.Workspaces.Add(workspaceId);
    }

    [Fact]
    public async Task Create_WithTagAndRefId_ShouldSucceed()
    {
        // Arrange
        var request = new CreateWorkspaceRequest
        {
            Name = "Tagged Workspace"+new Random().Next(),
            Description = "Workspace with metadata",
            Tag = "production",
            RefId = "workspace-ref-001"
        };

        // Act
        var workspaceId = await _client.CreateAsync(request);

        // Assert
        Assert.NotNull(workspaceId);

        TestConfig.CreatedResources.Workspaces.Add(workspaceId);
    }

    #endregion

    #region Get Tests

    [Fact]
    public async Task Get_WithValidId_ShouldReturnWorkspace()
    {
        // Arrange - Create a workspace first
        var createRequest = new CreateWorkspaceRequest
        {
            Name = "Workspace for Get Test"+ new Random().Next(),
            Description = "Testing get operation",
            Tag = "get-test"
        };

        var workspaceId = await _client.CreateAsync(createRequest);
        TestConfig.CreatedResources.Workspaces.Add(workspaceId);

        // Wait a moment for the workspace to be fully created
        await Task.Delay(1000);

        // Act
        var result = await _client.GetAsync(workspaceId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(workspaceId, result.Id);
        Assert.NotNull(result.Account);
    }

    #endregion

    #region List Tests

    [Fact]
    public async Task List_WithoutFilters_ShouldReturnPaginatedResults()
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
    public async Task List_FilterByTag_ShouldReturnMatchingWorkspaces()
    {
        // Arrange
        var uniqueTag = $"tag_{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}";

        // Create workspaces with the same tag
        var workspace1Id = await _client.CreateAsync(new CreateWorkspaceRequest
        {
            Name = "Tagged Workspace 1"+new Random().Next(),
            Description = "First workspace",
            Tag = uniqueTag
        });

        var workspace2Id = await _client.CreateAsync(new CreateWorkspaceRequest
        {
            Name = "Tagged Workspace 2"+new Random().Next(),
            Description = "Second workspace",
            Tag = uniqueTag
        });

        TestConfig.CreatedResources.Workspaces.Add(workspace1Id);
        TestConfig.CreatedResources.Workspaces.Add(workspace2Id);

        // Act
        var result = await _client.ListAsync(
            new ListWorkspacesParams { Tag = uniqueTag }
        );

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Items.Count >= 2);
        foreach (var item in result.Items)
        {
            Assert.Contains("Tagged Workspace", item.Name);
        }
    }

    [Fact]
    public async Task List_FilterByName_ShouldReturnMatchingWorkspaces()
    {
        // Arrange
        var uniqueName = $"NameFilter_{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}";

        var workspaceId = await _client.CreateAsync(new CreateWorkspaceRequest
        {
            Name = uniqueName,
            Description = "Name filter test"
        });

        TestConfig.CreatedResources.Workspaces.Add(workspaceId);

        // Act
        var result = await _client.ListAsync(
            new ListWorkspacesParams { Name = uniqueName }
        );

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result.Items);
    }

    [Fact]
    public async Task List_WithPagination_ShouldRespectPageSize()
    {
        // Act
        var result = await _client.ListAsync(
            pagination: new PaginationParams { Page = 1, PageSize = 5 }
        );

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Items.Count <= 5);
    }

    #endregion

    #region Update Tests

    [Fact]
    public async Task Update_WithoutImage_ShouldUpdateWorkspace()
    {
        // Arrange - Create a workspace
        var workspaceId = await _client.CreateAsync(new CreateWorkspaceRequest
        {
            Name = "Original Name"+ new Random().Next(),
            Description = "Original description"
        });

        TestConfig.CreatedResources.Workspaces.Add(workspaceId);

        // Act
        await _client.UpdateAsync(workspaceId, new UpdateWorkspaceRequest
        {
            Name = "Updated Name"+ new Random().Next(),
            Description = "Updated description"
        });

        // Assert
        var updated = await _client.GetAsync(workspaceId);
    }

    [Fact]
    public async Task Update_WithNewImage_ShouldUploadImage()
    {
        // Arrange - Create a workspace
        var workspaceId = await _client.CreateAsync(new CreateWorkspaceRequest
        {
            Name = "Workspace to Update"+ new Random().Next(),
            Description = "Will add image"
        });

        TestConfig.CreatedResources.Workspaces.Add(workspaceId);

        // Act - Update with image
        using var imageStream = File.OpenRead(_testImagePath);
        await _client.UpdateAsync(
            workspaceId,
            new UpdateWorkspaceRequest
            {
                Name = "Updated with Image"+ new Random().Next(),
                Description = "Now has an image"
            },
            imageStream,
            "image/png"
        );

        // Assert - No exception means success
        Assert.True(true);
    }

    [Fact]
    public async Task Update_ChangeTagAndRefId_ShouldSucceed()
    {
        // Arrange
        var workspaceId = await _client.CreateAsync(new CreateWorkspaceRequest
        {
            Name = "Metadata Update Test"+ new Random().Next(),
            Description = "Testing metadata updates",
            Tag = "old-tag",
            RefId = "old-ref"
        });

        TestConfig.CreatedResources.Workspaces.Add(workspaceId);

        // Act
        await _client.UpdateAsync(workspaceId, new UpdateWorkspaceRequest
        {
            Name = "Metadata Update Test"+ new Random().Next(),
            Description = "Testing metadata updates",
            Tag = "new-tag",
            RefId = "new-ref"
        });

        // Assert - No exception means success
        Assert.True(true);
    }

    #endregion

    #region Delete Tests

    [Fact]
    public async Task Delete_WithValidId_ShouldDeleteWorkspace()
    {
        // Arrange
        var workspaceId = await _client.CreateAsync(new CreateWorkspaceRequest
        {
            Name = "Workspace to Delete",
            Description = "This will be deleted"
        });

        // Act
        await _client.DeleteAsync(workspaceId);

        // Assert - No exception means success
        // Don't add to cleanup list since we already deleted it
    }

    #endregion

    #region Workflow Tests

    [Fact]
    public async Task CompleteWorkflow_CreateGetUpdateDelete_ShouldSucceed()
    {
        // Create
        var workspaceId = await _client.CreateAsync(new CreateWorkspaceRequest
        {
            Name = "Workflow Test Workspace",
            Description = "Full CRUD workflow",
            Tag = "workflow-test"
        });

        Assert.NotNull(workspaceId);

        // Get
        var workspace = await _client.GetAsync(workspaceId);
        Assert.Equal("Workflow Test Workspace", workspace.Name);

        // Update
        await _client.UpdateAsync(workspaceId, new UpdateWorkspaceRequest
        {
            Name = "Updated Workflow Workspace",
            Description = "Updated in workflow",
            Tag = "workflow-test"
        });

        // Verify update
        var updated = await _client.GetAsync(workspaceId);
        Assert.Equal("Updated Workflow Workspace", updated.Name);

        // Delete
        await _client.DeleteAsync(workspaceId);
    }

    [Fact]
    public async Task CompleteWorkflow_WithImage_ShouldSucceed()
    {
        // Create with image
        using (var createStream = File.OpenRead(_testImagePath))
        {
            var workspaceId = await _client.CreateAsync(
                new CreateWorkspaceRequest
                {
                    Name = "Image Workflow Test",
                    Description = "Testing with images"
                },
                createStream,
                "image/png"
            );

            // Update with new image
            using (var updateStream = File.OpenRead(_testImagePath))
            {
                await _client.UpdateAsync(
                    workspaceId,
                    new UpdateWorkspaceRequest
                    {
                        Name = "Updated Image Workspace",
                        Description = "With new image"
                    },
                    updateStream,
                    "image/png"
                );
            }

            // Delete
            await _client.DeleteAsync(workspaceId);
        }
    }

    #endregion

    #region Edge Cases

    [Fact]
    public async Task Create_WithLongDescription_ShouldSucceed()
    {
        // Arrange
        var longDescription = new string('x', 500);
        var request = new CreateWorkspaceRequest
        {
            Name = "Long Description Workspace" + new Random().Next(),
            Description = longDescription
        };

        // Act
        var workspaceId = await _client.CreateAsync(request);

        // Assert
        Assert.NotNull(workspaceId);

        TestConfig.CreatedResources.Workspaces.Add(workspaceId);
    }

    [Fact]
    public async Task List_FilterByRefId_ShouldReturnMatchingWorkspaces()
    {
        // Arrange
        var uniqueRef = $"ref_{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}";

        var workspaceId = await _client.CreateAsync(new CreateWorkspaceRequest
        {
            Name = "RefId Test"+ new Random().Next(),
            Description = "Testing refId filter",
            RefId = uniqueRef
        });

        TestConfig.CreatedResources.Workspaces.Add(workspaceId);

        // Act
        var result = await _client.ListAsync(
            new ListWorkspacesParams { RefId = uniqueRef }
        );

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result.Items);
    }

    #endregion

    public void Dispose()
    {
        // Cleanup is handled by test framework if needed
    }
}
