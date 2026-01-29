using System.Text.Json.Serialization;

namespace Posty5.SocialPublisherWorkspace.Models;

/// <summary>
/// Account details for a social media platform
/// </summary>
public class AccountDetails
{
    /// <summary>
    /// Link to the social media account
    /// </summary>
    public string Link { get; set; } = string.Empty;
    
    /// <summary>
    /// Account name/username
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Account thumbnail/profile picture URL
    /// </summary>
    public string Thumbnail { get; set; } = string.Empty;
    
    /// <summary>
    /// Platform-specific account ID
    /// </summary>
    public string PlatformAccountId { get; set; } = string.Empty;
    
    /// <summary>
    /// Account status (active, inactive, authenticationExpired)
    /// </summary>
    public string Status { get; set; } = string.Empty;
}

/// <summary>
/// Social media accounts associated with a workspace
/// </summary>
public class WorkspaceAccount
{
    /// <summary>
    /// YouTube account details
    /// </summary>
    public AccountDetails? Youtube { get; set; }
    
    /// <summary>
    /// Facebook account details
    /// </summary>
    public AccountDetails? Facebook { get; set; }
    
    /// <summary>
    /// Instagram account details
    /// </summary>
    public AccountDetails? Instagram { get; set; }
    
    /// <summary>
    /// TikTok account details
    /// </summary>
    public AccountDetails? Tiktok { get; set; }
    
    /// <summary>
    /// Facebook platform page ID
    /// </summary>
    public string? FacebookPlatformPageId { get; set; }
    
    /// <summary>
    /// Instagram platform account ID
    /// </summary>
    public string? InstagramPlatformAccountId { get; set; }
}

/// <summary>
/// Full workspace model with account details
/// </summary>
public class WorkspaceModel
{
    /// <summary>
    /// MongoDB document ID
    /// </summary>
    [JsonPropertyName("_id")]
    public string Id { get; set; } = string.Empty;
    
    /// <summary>
    /// Workspace name
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Social media accounts associated with this workspace
    /// </summary>
    public WorkspaceAccount Account { get; set; } = new();
}

/// <summary>
/// Simplified workspace details for list operations
/// </summary>
public class WorkspaceSampleDetails
{
    /// <summary>
    /// MongoDB document ID
    /// </summary>
    [JsonPropertyName("_id")]
    public string Id { get; set; } = string.Empty;
    
    /// <summary>
    /// Workspace name
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Workspace description
    /// </summary>
    public string Description { get; set; } = string.Empty;
    
    /// <summary>
    /// Workspace logo/image URL
    /// </summary>
    public string? ImageUrl { get; set; }
    
    /// <summary>
    /// Created timestamp
    /// </summary>
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? RefId { get; set; }
    public string? Tag { get; set; }

}

/// <summary>
/// Upload image configuration for R2 storage
/// </summary>
public class UploadImageConfig
{
    /// <summary>
    /// Pre-signed URL for uploading image to R2
    /// </summary>
    public string UploadUrl { get; set; } = string.Empty;
    
    /// <summary>
    /// Public URL where the uploaded image will be accessible
    /// </summary>
    public string ImageUrl { get; set; } = string.Empty;
}

/// <summary>
/// Response from create/update operations with workspace ID and upload config
/// </summary>
public class CreateWorkspaceResponse
{
    /// <summary>
    /// Created/updated workspace ID
    /// </summary>
    public string WorkspaceId { get; set; } = string.Empty;
    
    /// <summary>
    /// Upload configuration for workspace image (null if no image)
    /// </summary>
    public UploadImageConfig? UploadImageConfig { get; set; }
}

/// <summary>
/// Base workspace request
/// </summary>
public class WorkspaceRequest
{
    /// <summary>
    /// Workspace name (required)
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Workspace description (required)
    /// </summary>
    public string Description { get; set; } = string.Empty;
    
    /// <summary>
    /// Custom tag for filtering/categorization
    /// </summary>
    public string? Tag { get; set; }
    
    /// <summary>
    /// External reference ID for filtering/tracking
    /// </summary>
    public string? RefId { get; set; }
}

/// <summary>
/// Create workspace request
/// </summary>
public class CreateWorkspaceRequest : WorkspaceRequest
{
}

/// <summary>
/// Update workspace request
/// </summary>
public class UpdateWorkspaceRequest : WorkspaceRequest
{
}

/// <summary>
/// Parameters for listing/filtering workspaces
/// </summary>
public class ListWorkspacesParams
{
    /// <summary>
    /// Filter by workspace name
    /// </summary>
    public string? Name { get; set; }
    
    /// <summary>
    /// Filter by workspace description
    /// </summary>
    public string? Description { get; set; }
    
    /// <summary>
    /// Filter by custom tag
    /// </summary>
    public string? Tag { get; set; }
    
    /// <summary>
    /// Filter by external reference ID
    /// </summary>
    public string? RefId { get; set; }
}
