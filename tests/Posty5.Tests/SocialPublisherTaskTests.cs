using Xunit;
using Posty5.SocialPublisher;
using Posty5.SocialPublisher.Models;
using Posty5.Core.Exceptions;

namespace Posty5.Tests.Integration;

[Collection("Sequential")]
public class SocialPublisherTaskTests : IDisposable
{
    private readonly WorkspaceClient _workspaceClient;
    private readonly TaskClient _taskClient;
    private string? _workspaceId;
    private string? _taskId;

    public SocialPublisherTaskTests()
    {
        var httpClient = TestConfig.CreateHttpClient();
        _workspaceClient = new WorkspaceClient(httpClient);
        _taskClient = new TaskClient(httpClient);
    }

    private async Task<string> CreateTestWorkspaceAsync()
    {
        var request = new CreateWorkspaceRequest
        {
            Name = $"Test Workspace for Tasks - {DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}",
            Description = "Workspace for task testing"
        };
        var workspace = await _workspaceClient.CreateAsync(request);
        _workspaceId = workspace.Id;
        TestConfig.CreatedResources.Workspaces.Add(_workspaceId!);
        return _workspaceId!;
    }

    [Fact]
    public async Task CreateTask_ShouldReturnValidTask()
    {
        // Arrange
        var workspaceId = await CreateTestWorkspaceAsync();
        var request = new CreateTaskRequest
        {
            WorkspaceId = workspaceId,
            Title = $"Test Task - {DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}",
            Content = "This is a test task created by the .NET SDK",
            Platforms = new List<SocialPlatform> { SocialPlatform.Facebook, SocialPlatform.Twitter },
            ScheduledAt = DateTime.UtcNow.AddHours(2)
        };

        // Act
        var result = await _taskClient.CreateAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Id);
        Assert.Equal(request.Title, result.Title);

        _taskId = result.Id;
        TestConfig.CreatedResources.Tasks.Add(_taskId);
    }

    [Fact]
    public async Task CreateTask_WithMultiplePlatforms_ShouldReturnValidTask()
    {
        // Arrange
        var workspaceId = await CreateTestWorkspaceAsync();
        var request = new CreateTaskRequest
        {
            WorkspaceId = workspaceId,
            Title = "Multi-Platform Task",
            Content = "Posting to multiple platforms",
            Platforms = new List<SocialPlatform> 
            { 
                SocialPlatform.Facebook, 
                SocialPlatform.Twitter,
                SocialPlatform.LinkedIn
            },
            ScheduledAt = DateTime.UtcNow.AddHours(1)
        };

        // Act
        var result = await _taskClient.CreateAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Id);
        Assert.Equal(3, result.Platforms.Count);

        TestConfig.CreatedResources.Tasks.Add(result.Id!);
    }

    [Fact]
    public async Task GetTaskById_WithValidId_ShouldReturnTask()
    {
        // Arrange - Create a task first
        var workspaceId = await CreateTestWorkspaceAsync();
        var createRequest = new CreateTaskRequest
        {
            WorkspaceId = workspaceId,
            Title = "Test Task for Get",
            Content = "Test content",
            Platforms = new List<SocialPlatform> { SocialPlatform.Facebook }
        };
        var created = await _taskClient.CreateAsync(createRequest);
        TestConfig.CreatedResources.Tasks.Add(created.Id!);

        // Act
        var result = await _taskClient.GetAsync(created.Id!);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(created.Id, result.Id);
        Assert.NotNull(result.Title);
    }

    [Fact]
    public async Task GetTaskById_WithInvalidId_ShouldThrowException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<Posty5Exception>(async () =>
            await _taskClient.GetAsync("invalid-id-123")
        );
    }

    [Fact]
    public async Task ListTasks_ShouldReturnPaginatedResults()
    {
        // Act
        var result = await _taskClient.ListAsync(
            pagination: new Core.Models.PaginationParams { PageNumber = 0, PageSize = 10 }
        );

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Items);
        Assert.True(result.TotalCount >= 0);
    }

    [Fact]
    public async Task ListTasks_ByWorkspace_ShouldFilterResults()
    {
        // Arrange
        var workspaceId = await CreateTestWorkspaceAsync();
        var listParams = new ListTasksParams
        {
            WorkspaceId = workspaceId
        };

        // Act
        var result = await _taskClient.ListAsync(
            listParams,
            new Core.Models.PaginationParams { PageNumber = 0, PageSize = 10 }
        );

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Items);
    }

    [Fact]
    public async Task ListTasks_ByStatus_ShouldFilterResults()
    {
        // Arrange
        var listParams = new ListTasksParams
        {
            Status = Posty5.SocialPublisher.Models.TaskStatus.Scheduled
        };

        // Act
        var result = await _taskClient.ListAsync(
            listParams,
            new Core.Models.PaginationParams { PageNumber = 0, PageSize = 10 }
        );

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Items);
    }

    [Fact]
    public async Task UpdateTask_ShouldUpdateSuccessfully()
    {
        // Arrange - Create a task first
        var workspaceId = await CreateTestWorkspaceAsync();
        var createRequest = new CreateTaskRequest
        {
            WorkspaceId = workspaceId,
            Title = "Original Task Title",
            Content = "Original content",
            Platforms = new List<SocialPlatform> { SocialPlatform.Facebook }
        };
        var created = await _taskClient.CreateAsync(createRequest);
        TestConfig.CreatedResources.Tasks.Add(created.Id!);

        // Act
        var newTitle = $"Updated Task - {DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}";
        var updateRequest = new UpdateTaskRequest
        {
            Title = newTitle,
            Content = "Updated content"
        };
        var result = await _taskClient.UpdateAsync(created.Id!, updateRequest);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(created.Id, result.Id);
    }

    [Fact]
    public async Task UpdateTask_ChangePlatforms_ShouldUpdateSuccessfully()
    {
        // Arrange - Create a task first
        var workspaceId = await CreateTestWorkspaceAsync();
        var createRequest = new CreateTaskRequest
        {
            WorkspaceId = workspaceId,
            Title = "Task to Change Platforms",
            Content = "Content",
            Platforms = new List<SocialPlatform> { SocialPlatform.Facebook }
        };
        var created = await _taskClient.CreateAsync(createRequest);
        TestConfig.CreatedResources.Tasks.Add(created.Id!);

        // Act
        var updateRequest = new UpdateTaskRequest
        {
            Platforms = new List<SocialPlatform> 
            { 
                SocialPlatform.Facebook, 
                SocialPlatform.Twitter,
                SocialPlatform.LinkedIn 
            }
        };
        var result = await _taskClient.UpdateAsync(created.Id!, updateRequest);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(created.Id, result.Id);
    }

    [Fact]
    public async Task DeleteTask_ShouldDeleteSuccessfully()
    {
        // Arrange - Create a task first
        var workspaceId = await CreateTestWorkspaceAsync();
        var createRequest = new CreateTaskRequest
        {
            WorkspaceId = workspaceId,
            Title = "Task to Delete",
            Content = "This task will be deleted",
            Platforms = new List<SocialPlatform> { SocialPlatform.Facebook }
        };
        var created = await _taskClient.CreateAsync(createRequest);

        // Act
        var deleteResult = await _taskClient.DeleteAsync(created.Id!);

        // Assert
        Assert.True(deleteResult);

        // Verify deletion
        await Assert.ThrowsAsync<Posty5Exception>(async () =>
            await _taskClient.GetAsync(created.Id!)
        );
    }

    public void Dispose()
    {
        // Cleanup is handled by collection fixture if needed
    }
}
