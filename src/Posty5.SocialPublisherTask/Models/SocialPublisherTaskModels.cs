using Posty5.Core.Converts;
using System.Text.Json.Serialization;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Posty5.SocialPublisherTask.Models;

// ============================================================================
// PLATFORM CONFIGURATION MODELS
// ============================================================================

/// <summary>
/// YouTube-specific configuration
/// </summary>
public class YouTubeConfig
{
    /// <summary>
    /// Video title (required, max 100 characters)
    /// </summary>
    public string Title { get; set; } = string.Empty;
    
    /// <summary>
    /// Video description (required, max 5000 characters)
    /// </summary>
    public string Description { get; set; } = string.Empty;
    
    /// <summary>
    /// Video tags for discovery (required)
    /// </summary>
    public List<string> Tags { get; set; } = new();
    
    /// <summary>
    /// Whether the video is made for kids (COPPA compliance)
    /// </summary>
    public bool? MadeForKids { get; set; }
    
    /// <summary>
    /// Default language code
    /// </summary>
    public string? DefaultLanguage { get; set; }
    
    /// <summary>
    /// Default audio language code
    /// </summary>
    public string? DefaultAudioLanguage { get; set; }
    
    /// <summary>
    /// YouTube category ID
    /// </summary>
    public string? CategoryId { get; set; }
    
    /// <summary>
    /// Localization languages
    /// </summary>
    public List<string>? LocalizationLanguages { get; set; }
}

/// <summary>
/// TikTok-specific configuration
/// </summary>
public class TikTokConfig
{
    /// <summary>
    /// Video caption (required, max 2200 characters)
    /// </summary>
    public string Caption { get; set; } = string.Empty;
    
    /// <summary>
    /// Disable duet feature
    /// </summary>
    [JsonPropertyName("disable_duet")]
    public bool DisableDuet { get; set; }
    
    /// <summary>
    /// Disable stitch feature
    /// </summary>
    [JsonPropertyName("disable_stitch")]
    public bool DisableStitch { get; set; }
    
    /// <summary>
    /// Disable comments
    /// </summary>
    [JsonPropertyName("disable_comment")]
    public bool DisableComment { get; set; }

    /// <summary>
    /// Privacy level ("PUBLIC" | "PRIVATE" | "FRIENDS" | "FOLLOWER_OF_CREATOR")
    /// </summary>
    [JsonPropertyName("privacy_level")]
    public string PrivacyLevel { get; set; } = "PUBLIC";
}

/// <summary>
/// Facebook Page-specific configuration
/// </summary>
public class FacebookPageConfig
{
    /// <summary>
    /// Post description (required)
    /// </summary>
    public string Description { get; set; } = string.Empty;
    
    /// <summary>
    /// Post title (optional)
    /// </summary>
    public string? Title { get; set; }
}

/// <summary>
/// Instagram-specific configuration
/// </summary>
public class InstagramConfig
{
    /// <summary>
    /// Post caption/description (required, max 2200 characters)
    /// </summary>
    public string Description { get; set; } = string.Empty;
    
    /// <summary>
    /// Share to Instagram feed
    /// </summary>
    public bool? ShareToFeed { get; set; }
    
    /// <summary>
    /// Publish to both feed and story
    /// </summary>
    public bool? IsPublishedToBothFeedAndStory { get; set; }
}

/// <summary>
/// Schedule configuration
/// </summary>
public class ScheduleConfig
{
    /// <summary>
    /// Schedule type: "now" or "schedule"
    /// </summary>
    public string Type { get; set; } = "now";
    
    /// <summary>
    /// Scheduled date/time (required if type is "schedule")
    /// </summary>
    public DateTime? ScheduledAt { get; set; }
}

// ============================================================================
// REQUEST MODELS
// ============================================================================

/// <summary>
/// Task settings for publishing
/// </summary>
public class TaskSettings
{
    /// <summary>
    /// Workspace ID (required)
    /// </summary>
    public string WorkspaceId { get; set; } = string.Empty;
    
    /// <summary>
    /// Enable YouTube publishing
    /// </summary>
    public bool IsAllowYouTube { get; set; }
    
    /// <summary>
    /// Enable TikTok publishing
    /// </summary>
    public bool IsAllowTiktok { get; set; }
    
    /// <summary>
    /// Enable Facebook publishing
    /// </summary>
    public bool IsAllowFacebookPage { get; set; }
    
    /// <summary>
    /// Enable Instagram publishing
    /// </summary>
    public bool IsAllowInstagram { get; set; }
    
    /// <summary>
    /// YouTube configuration (required if IsAllowYouTube is true)
    /// </summary>
    public YouTubeConfig? YouTube { get; set; }
    
    /// <summary>
    /// TikTok configuration (required if IsAllowTiktok is true)
    /// </summary>
    public TikTokConfig? Tiktok { get; set; }
    
    /// <summary>
    /// Facebook configuration (required if IsAllowFacebookPage is true)
    /// </summary>
    public FacebookPageConfig? Facebook { get; set; }
    
    /// <summary>
    /// Instagram configuration (required if IsAllowInstagram is true)
    /// </summary>
    public InstagramConfig? Instagram { get; set; }
    
    /// <summary>
    /// Schedule configuration
    /// </summary>
    public ScheduleConfig? Schedule { get; set; }
    
    /// <summary>
    /// Source type (file, url, facebook, tiktok, youtube)
    /// </summary>
    public string Source { get; set; } = string.Empty;
    
    /// <summary>
    /// Custom tag for filtering
    /// </summary>
    public string? Tag { get; set; }
    
    /// <summary>
    /// External reference ID
    /// </summary>
    public string? RefId { get; set; }
}

/// <summary>
/// Create social publisher task request
/// </summary>
public class CreateSocialPublisherTaskRequest
{
    /// <summary>
    /// Workspace ID (required)
    /// </summary>
    public string WorkspaceId { get; set; } = string.Empty;

    /// <summary>
    /// Source type (video-file, video-url, facebook-video, youtube-video, tiktok-video )
    /// </summary>
    public string Source { get; set; } = string.Empty;
    
    /// <summary>
    /// Enable YouTube publishing
    /// </summary>
    public bool IsAllowYouTube { get; set; }
    
    /// <summary>
    /// Enable TikTok publishing
    /// </summary>
    public bool IsAllowTiktok { get; set; }
    
    /// <summary>
    /// Enable Facebook publishing
    /// </summary>
    public bool IsAllowFacebookPage { get; set; }
    
    /// <summary>
    /// Enable Instagram publishing
    /// </summary>
    public bool IsAllowInstagram { get; set; }
    
    /// <summary>
    /// YouTube configuration
    /// </summary>
    public YouTubeConfig? YouTube { get; set; }
    
    /// <summary>
    /// TikTok configuration
    /// </summary>
    public TikTokConfig? Tiktok { get; set; }
    
    /// <summary>
    /// Facebook configuration
    /// </summary>
    public FacebookPageConfig? Facebook { get; set; }
    
    /// <summary>
    /// Instagram configuration
    /// </summary>
    public InstagramConfig? Instagram { get; set; }
    
    /// <summary>
    /// Video URL (for url/repost sources)
    /// </summary>
    public string? VideoURL { get; set; }
    
    /// <summary>
    /// Thumbnail URL
    /// </summary>
    public string? ThumbURL { get; set; }
    
    /// <summary>
    /// Post URL (for repost sources)
    /// </summary>
    public string? PostURL { get; set; }
    
    /// <summary>
    /// Schedule configuration
    /// </summary>
    public ScheduleConfig? Schedule { get; set; }
    
    /// <summary>
    /// Custom tag
    /// </summary>
    public string? Tag { get; set; }
    
    /// <summary>
    /// External reference ID
    /// </summary>
    public string? RefId { get; set; }
}

/// <summary>
/// Generate upload URLs request
/// </summary>
public class GenerateUploadUrlsRequest
{
    /// <summary>
    /// Thumbnail file content type (e.g., image/jpeg)
    /// </summary>
    public string? ThumbFileType { get; set; }
    
    /// <summary>
    /// Video file content type (e.g., video/mp4)
    /// </summary>
    public string? VideoFileType { get; set; }
}

/// <summary>
/// Parameters for listing/filtering tasks
/// </summary>
public class ListTasksParams
{
    /// <summary>
    /// Filter by caption/title
    /// </summary>
    public string? Caption { get; set; }
    
    /// <summary>
    /// Filter by task numbering
    /// </summary>
    public string? Numbering { get; set; }
    
    /// <summary>
    /// Filter by status (pending, processing, published, failed)
    /// </summary>
    public SocialPublisherTaskStatusType? CurrentStatus { get; set; }
    
    /// <summary>
    /// Filter by workspace ID
    /// </summary>
    public string? WorkspaceId { get; set; }
    
    /// <summary>
    /// Filter by external reference ID
    /// </summary>
    public string? RefId { get; set; }
    
    /// <summary>
    /// Filter by custom tag
    /// </summary>
    public string? Tag { get; set; }
    
    /// <summary>
    /// Filter by YouTube publishing status
    /// </summary>
    [JsonPropertyName("youtube.postInfo.isAllow")]
    public bool? YouTubeIsAllow { get; set; }
    
    /// <summary>
    /// Filter by Facebook publishing status
    /// </summary>
    [JsonPropertyName("facebook.postInfo.isAllow")]
    public bool? FacebookIsAllow { get; set; }
    
    /// <summary>
    /// Filter by Instagram publishing status
    /// </summary>
    [JsonPropertyName("instagram.postInfo.isAllow")]
    public bool? InstagramIsAllow { get; set; }
    
    /// <summary>
    /// Filter by TikTok publishing status
    /// </summary>
    [JsonPropertyName("tiktok.postInfo.isAllow")]
    public bool? TiktokIsAllow { get; set; }
}

// ============================================================================
// UPLOAD MODELS
// ============================================================================

/// <summary>
/// Upload URL information
/// </summary>
public class UploadUrlInfo
{
    /// <summary>
    /// Public URL to access the uploaded file
    /// </summary>
    public string? FileURL { get; set; }
    
    /// <summary>
    /// Pre-signed upload URL for uploading to R2
    /// </summary>
    public string? UploadFileURL { get; set; }
    
    /// <summary>
    /// Bucket file path
    /// </summary>
    public string? BucketFilePath { get; set; }
}

/// <summary>
/// Generate upload URLs response
/// </summary>
public class GenerateUploadUrlsResponse
{
    /// <summary>
    /// Task ID
    /// </summary>
    public string TaskId { get; set; } = string.Empty;
    
    /// <summary>
    /// Thumbnail upload information
    /// </summary>
    public UploadUrlInfo Thumb { get; set; } = new();
    
    /// <summary>
    /// Video upload information
    /// </summary>
    public UploadUrlInfo Video { get; set; } = new();
}

// ============================================================================
// RESPONSE MODELS
// ============================================================================

/// <summary>
/// Simplified task model for list operations
/// </summary>
public class TaskModel
{
    /// <summary>
    /// MongoDB document ID
    /// </summary>
    [JsonPropertyName("_id")]
    public string Id { get; set; } = string.Empty;
    
    /// <summary>
    /// Task numbering/identifier
    /// </summary>
    public string Numbering { get; set; } = string.Empty;
    
    /// <summary>
    /// Task caption/title
    /// </summary>
    public string Caption { get; set; } = string.Empty;
    
    /// <summary>
    /// Created timestamp
    /// </summary>
    public DateTime CreatedAt { get; set; }
    
    /// <summary>
    /// Current task status
    /// </summary>
    public SocialPublisherTaskStatusType CurrentStatus { get; set; } 
     
    /// <summary>
    /// External reference ID
    /// </summary>
    public string? RefId { get; set; }
    
    /// <summary>
    /// Custom tag
    /// </summary>
    public string? Tag { get; set; }
    public TaskModelIsAllowAccount IisAllow { get; set; }
    public TaskModeWorkspace Workspace { get; set; }
    public ScheduleConfig Schedule { get; set; }
}

public class TaskModelIsAllowAccount
{
    public bool Youtube { get; set; }
    public bool Facebook { get; set; }
    public bool Instagram { get; set; }
    public bool Tiktok { get; set; }
}

public class TaskModeWorkspace
{
    [JsonPropertyName("workspace._id")]
    public string Id { get; set; }
    public string Name { get; set; }
}


/// <summary>
/// Task status response with full details
/// </summary>
public class TaskStatusResponse
{
    /// <summary>
    /// MongoDB document ID
    /// </summary>
    [JsonPropertyName("_id")]
    public string Id { get; set; } = string.Empty;
    
    /// <summary>
    /// Task numbering
    /// </summary>
    public string Numbering { get; set; } = string.Empty;
    
    /// <summary>
    /// Task type (shortVideo)
    /// </summary>
    public string Type { get; set; } = string.Empty;
    
    /// <summary>
    /// Source type (file, url, facebook, tiktok, youtube)
    /// </summary>
    public string Source { get; set; } = string.Empty;
    
    /// <summary>
    /// Current task status
    /// </summary>
    public SocialPublisherTaskStatusType CurrentStatus { get; set; }
    
    /// <summary>
    /// Current error message
    /// </summary>
    public string? CurrentError {get; set; }
    
    /// <summary>
    /// Current status changed timestamp
    /// </summary>
    public DateTime CurrentStatusChangedAt { get; set; }
    
    /// <summary>
    /// Created timestamp
    /// </summary>
    public DateTime CreatedAt { get; set; }
    
    /// <summary>
    /// Started timestamp
    /// </summary>
    public DateTime? StartedAt { get; set; }
}

/// <summary>
/// Next and previous task IDs for navigation
/// </summary>
public class NextPreviousResponse
{
    /// <summary>
    /// Next task ID
    /// </summary>
    public string? NextId { get; set; }
    
    /// <summary>
    /// Previous task ID
    /// </summary>
    public string? PreviousId { get; set; }
}

/// <summary>
/// Default settings response
/// </summary>
public class DefaultSettingsResponse
{
    // Generic dictionary for system defaults
    [JsonExtensionData]
    public Dictionary<string, object>? ExtensionData { get; set; }
}

[JsonConverter(typeof(StringValueObjectConverter<SocialPublisherTaskStatusType>))]
public readonly record struct SocialPublisherTaskStatusType (string Value)
{
    /// <summary>
    /// The video is waiting in the queue and hasn't been processed yet.
    /// </summary>
    public static readonly SocialPublisherTaskStatusType Pending = new("pending");

    /// <summary>
    /// The video is currently being uploaded by the job system.
    /// </summary>
    public static readonly SocialPublisherTaskStatusType Processing = new("processing");

    /// <summary>
    /// In Instagram and TikTok, the file is sent in one step and published in another step.
    /// </summary>
    public static readonly SocialPublisherTaskStatusType ProcessingInPlatform = new("processingInPlatform");

    /// <summary>
    /// The video was sent to the platform but an error occurred during platform processing.
    /// </summary>
    public static readonly SocialPublisherTaskStatusType FailedByPlatform = new("failedByPlatform");

    /// <summary>
    /// The video was successfully uploaded.
    /// </summary>
    public static readonly SocialPublisherTaskStatusType Done = new("done");

    /// <summary>
    /// The upload failed due to an error (network, API failure, etc.).
    /// </summary>
    public static readonly SocialPublisherTaskStatusType Error = new("error");

    /// <summary>
    /// The user or system canceled the upload before it started.
    /// </summary>
    public static readonly SocialPublisherTaskStatusType Canceled = new("canceled");

    /// <summary>
    /// The task requires maintenance because some platform has issues.
    /// </summary>
    public static readonly SocialPublisherTaskStatusType NeedsMaintenance = new("needsMaintenance");

    /// <summary>
    /// The provided video URL is invalid or inaccessible.
    /// </summary>
    public static readonly SocialPublisherTaskStatusType InvalidVideoURL = new("invalidVideoURL");

    /// <summary>
    /// The platform video URL (Facebook, TikTok, YouTube) is invalid or inaccessible.
    /// </summary>
    public static readonly SocialPublisherTaskStatusType InvalidPostVideoURL = new("invalidPostVideoURL");

    /// <summary>
    /// The task is being retried after an error.
    /// </summary>
    public static readonly SocialPublisherTaskStatusType Retrying = new("retrying");

    //private static readonly HashSet<string> Allowed = new()
    //{
    //    "pending",
    //    "processing",
    //    "processingInPlatform",
    //    "failedByPlatform",
    //    "done",
    //    "error",
    //    "canceled",
    //    "needsMaintenance",
    //    "invalidVideoURL",
    //    "invalidPostVideoURL",
    //    "retrying"
    //};

    //public static SocialPublisherTaskStatusType From (string value)
    //{
    //    if (!Allowed.Contains(value))
    //        throw new ArgumentException($"Invalid SocialPublisherTaskStatusType: {value}");

    //    return new SocialPublisherTaskStatusType(value);
    //}

    public override string ToString ( ) => Value;
}
