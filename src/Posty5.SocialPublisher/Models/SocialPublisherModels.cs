namespace Posty5.SocialPublisher.Models;

/// <summary>
/// Social platform type
/// </summary>
public enum SocialPlatform
{
    Facebook,
    Instagram,
    Twitter,
    LinkedIn,
    YouTube,
    TikTok
}

/// <summary>
/// Task status
/// </summary>
public enum TaskStatus
{
    Draft,
    Scheduled,
    Published,
    Failed,
    Cancelled
}

/// <summary>
/// Workspace model
/// </summary>
public class WorkspaceModel
{
    public string? Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// Social publisher task model
/// </summary>
public class TaskModel
{
    public string? Id { get; set; }
    public string? WorkspaceId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Content { get; set; }
    public List<SocialPlatform> Platforms { get; set; } = new();
    public TaskStatus Status { get; set; }
    public DateTime? ScheduledAt { get; set; }
    public DateTime? PublishedAt { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// Create workspace request
/// </summary>
public class CreateWorkspaceRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}

/// <summary>
/// Update workspace request
/// </summary>
public class UpdateWorkspaceRequest
{
    public string? Name { get; set; }
    public string? Description { get; set; }
}

/// <summary>
/// Create task request
/// </summary>
public class CreateTaskRequest
{
    public string WorkspaceId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Content { get; set; }
    public List<SocialPlatform> Platforms { get; set; } = new();
    public DateTime? ScheduledAt { get; set; }
}

/// <summary>
/// Update task request
/// </summary>
public class UpdateTaskRequest
{
    public string? Title { get; set; }
    public string? Content { get; set; }
    public List<SocialPlatform>? Platforms { get; set; }
    public DateTime? ScheduledAt { get; set; }
    public TaskStatus? Status { get; set; }
}

/// <summary>
/// List parameters for workspaces
/// </summary>
public class ListWorkspacesParams
{
    public string? Search { get; set; }
}

/// <summary>
/// List parameters for tasks
/// </summary>
public class ListTasksParams
{
    public string? WorkspaceId { get; set; }
    public TaskStatus? Status { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
}
