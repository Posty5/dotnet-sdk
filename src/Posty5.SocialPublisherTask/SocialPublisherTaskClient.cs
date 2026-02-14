using System.Net.Http.Headers;
using Posty5.Core.Http;
using Posty5.Core.Models;
using Posty5.SocialPublisherTask.Models;

namespace Posty5.SocialPublisherTask;

/// <summary>
 /// Client for managing social publisher tasks via Posty5 API
/// Supports publishing videos to YouTube, TikTok, Facebook, and Instagram
/// </summary>
public class SocialPublisherTaskClient
{
    private readonly Posty5HttpClient _http;
    private const string BasePath = "/api/social-publisher-task";
    
    /// <summary>
    /// Maximum video upload size (4GB)
    /// </summary>
    public const long MaxVideoUploadSizeBytes = 4L * 1024 * 1024 * 1024;

    /// <summary>
    /// Creates a new Social Publisher Task client
    /// </summary>
    /// <param name="httpClient">HTTP client instance from Posty5.Core</param>
    public SocialPublisherTaskClient(Posty5HttpClient httpClient)
    {
        _http = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    }

    // ============================================================================
    // CORE CRUD METHODS (PUBLIC)
    // ============================================================================

    /// <summary>
    /// List/search tasks with pagination and filters
    /// </summary>
    public async Task<PaginationResponse<TaskModel>> ListAsync(
        ListTasksParams? listParams = null,
        PaginationParams? pagination = null,
        CancellationToken cancellationToken = default)
    {
        var queryParams = new Dictionary<string, object?>();

        if (listParams != null)
        {
            if (!string.IsNullOrEmpty(listParams.Caption))
                queryParams["caption"] = listParams.Caption;
            if (!string.IsNullOrEmpty(listParams.Numbering))
                queryParams["numbering"] = listParams.Numbering;
            if (listParams.CurrentStatus.HasValue)
                queryParams["currentStatus"] = listParams.CurrentStatus.ToString();
            if (!string.IsNullOrEmpty(listParams.WorkspaceId))
                queryParams["workspaceId"] = listParams.WorkspaceId;
            if (!string.IsNullOrEmpty(listParams.RefId))
                queryParams["refId"] = listParams.RefId;
            if (!string.IsNullOrEmpty(listParams.Tag))
                queryParams["tag"] = listParams.Tag;
        }

        if (pagination != null)
        {
            if (!string.IsNullOrEmpty(pagination.Cursor))
                queryParams["cursor"] = pagination.Cursor;
            queryParams["pageSize"] = pagination.PageSize;
        }

        var response = await _http.GetAsync<PaginationResponse<TaskModel>>(
            BasePath,
            queryParams,
            cancellationToken);

        return response.Result ?? new PaginationResponse<TaskModel>();
    }

    /// <summary>
    /// Get default settings
    /// </summary>
    public async Task<DefaultSettingsResponse> GetDefaultSettingsAsync(
        CancellationToken cancellationToken = default)
    {
        var response = await _http.GetAsync<DefaultSettingsResponse>(
            $"{BasePath}/default-settings",
            cancellationToken: cancellationToken);

        return response.Result ?? new DefaultSettingsResponse();
    }

    /// <summary>
    /// Get task status by ID with full details including platform configurations
    /// </summary>
    public async Task<TaskStatusFullDetailsResponse> GetStatusAsync(
        string id,
        CancellationToken cancellationToken = default)
    {
        var response = await _http.GetAsync<TaskStatusFullDetailsResponse>(
            $"{BasePath}/{id}",
            cancellationToken: cancellationToken);

        return response.Result ?? throw new InvalidOperationException("Task not found");
    }

    /// <summary>
    /// Generate upload URLs for video and thumbnail
    /// </summary>
    public async Task<GenerateUploadUrlsResponse> GenerateUploadUrlsAsync(
        GenerateUploadUrlsRequest request,
        CancellationToken cancellationToken = default)
    {
        var response = await _http.PostAsync<GenerateUploadUrlsResponse>(
            $"{BasePath}/generate-upload-urls",
            request,
            cancellationToken);

        return response.Result ?? throw new InvalidOperationException("Failed to generate upload URLs");
    }

    /// <summary>
    /// Get next and previous task IDs for navigation
    /// </summary>
    public async Task<NextPreviousResponse> GetNextAndPreviousAsync(
        string id,
        CancellationToken cancellationToken = default)
    {
        var response = await _http.GetAsync<NextPreviousResponse>(
            $"{BasePath}/{id}/next-previous",
            cancellationToken: cancellationToken);

        return response.Result ?? new NextPreviousResponse();
    }

    // ============================================================================
    // MAIN PUBLISHING METHOD (PUBLIC) - Matches TypeScript publishShortVideo
    // ============================================================================

    /// <summary>
    /// Publish a short video to multiple social media platforms with auto-detection.
    /// This is the main recommended method - now explicitly for workspaces.
    /// Automatically detects video source type (file upload, URL, or repost) and handles accordingly.
    /// </summary>
    public async Task<string> PublishShortVideoToWorkspaceAsync(
        string workspaceId,
        object video,
        object? thumbnail = null,
        List<string>? platforms = null,
        YouTubeConfig? youtube = null,
        TikTokConfig? tiktok = null,
        FacebookPageConfig? facebook = null,
        InstagramConfig? instagram = null,
        object? schedule = null,
        string? tag = null,
        string? refId = null,
        string? videoContentType = null,
        string? thumbnailContentType = null,
        CancellationToken cancellationToken = default)
    {
        // Build task settings
        var settings = new TaskSettings
        {
            WorkspaceId = workspaceId,
            IsAllowYouTube = platforms?.Contains("youtube") ?? false,
            IsAllowTiktok = platforms?.Contains("tiktok") ?? false,
            IsAllowFacebookPage = platforms?.Contains("facebook") ?? false,
            IsAllowInstagram = platforms?.Contains("instagram") ?? false,
            YouTube = youtube,
            Tiktok = tiktok,
            Facebook = facebook,
            Instagram = instagram,
            Tag = tag,
            RefId = refId
        };

        // Handle schedule
        if (schedule != null)
        {
            if (schedule is string scheduleStr && scheduleStr == "now")
            {
                settings.Schedule = new ScheduleConfig { Type = "now" };
            }
            else if (schedule is DateTime scheduleDate)
            {
                settings.Schedule = new ScheduleConfig
                {
                    Type = "schedule",
                    ScheduledAt = scheduleDate
                };
            }
        }

        // Detect source type
        var sourceType = DetectVideoSource(video);

        // Route to appropriate private method
        return sourceType switch
        {
            "file" => await PublishShortVideoToWorkspaceByFileAsync(settings, (Stream)video, videoContentType ?? "video/mp4",
                thumbnail as Stream, thumbnailContentType, thumbnail as string, cancellationToken),
            "url" => await PublishShortVideoToWorkspaceByUrlAsync(settings, (string)video, thumbnail as Stream,
                thumbnailContentType, thumbnail as string, cancellationToken),
            "repost" => await PublishRepostVideoToWorkspaceAsync(settings, (string)video, thumbnail as Stream,
                thumbnailContentType, thumbnail as string, cancellationToken),
            _ => throw new InvalidOperationException($"Unknown video source type: {sourceType}")
        };
    }

    /// <summary>
    /// Publish a short video to multiple social media platforms with auto-detection via Account.
    /// Automatically detects video source type (file upload, URL, or repost) and handles accordingly.
    /// </summary>
    public async Task<string> PublishShortVideoToAccountAsync(
        string accountId,
        object video,
        object? thumbnail = null,
        List<string>? platforms = null,
        YouTubeConfig? youtube = null,
        TikTokConfig? tiktok = null,
        FacebookPageConfig? facebook = null,
        InstagramConfig? instagram = null,
        object? schedule = null,
        string? tag = null,
        string? refId = null,
        string? videoContentType = null,
        string? thumbnailContentType = null,
        CancellationToken cancellationToken = default)
    {
        // Build task settings
        var settings = new TaskSettings
        {
            AccountId = accountId,
            IsAllowYouTube = platforms?.Contains("youtube") ?? false,
            IsAllowTiktok = platforms?.Contains("tiktok") ?? false,
            IsAllowFacebookPage = platforms?.Contains("facebook") ?? false,
            IsAllowInstagram = platforms?.Contains("instagram") ?? false,
            YouTube = youtube,
            Tiktok = tiktok,
            Facebook = facebook,
            Instagram = instagram,
            Tag = tag,
            RefId = refId
        };

        // Handle schedule
        if (schedule != null)
        {
            if (schedule is string scheduleStr && scheduleStr == "now")
            {
                settings.Schedule = new ScheduleConfig { Type = "now" };
            }
            else if (schedule is DateTime scheduleDate)
            {
                settings.Schedule = new ScheduleConfig
                {
                    Type = "schedule",
                    ScheduledAt = scheduleDate
                };
            }
        }

        // Detect source type
        var sourceType = DetectVideoSource(video);

        // Route to appropriate private method
        return sourceType switch
        {
            "file" => await PublishShortVideoToAccountByFileAsync(settings, (Stream)video, videoContentType ?? "video/mp4",
                thumbnail as Stream, thumbnailContentType, thumbnail as string, cancellationToken),
            "url" => await PublishShortVideoToAccountByUrlAsync(settings, (string)video, thumbnail as Stream,
                thumbnailContentType, thumbnail as string, cancellationToken),
            "repost" => await PublishRepostVideoToAccountAsync(settings, (string)video, thumbnail as Stream,
                thumbnailContentType, thumbnail as string, cancellationToken),
            _ => throw new InvalidOperationException($"Unknown video source type: {sourceType}")
        };
    }

    [Obsolete("Use PublishShortVideoToWorkspaceAsync instead")]
    public Task<string> PublishShortVideoAsync(
        string workspaceId,
        object video,
        object? thumbnail = null,
        List<string>? platforms = null,
        YouTubeConfig? youtube = null,
        TikTokConfig? tiktok = null,
        FacebookPageConfig? facebook = null,
        InstagramConfig? instagram = null,
        object? schedule = null,
        string? tag = null,
        string? refId = null,
        string? videoContentType = null,
        string? thumbnailContentType = null,
        CancellationToken cancellationToken = default)
    {
        return PublishShortVideoToWorkspaceAsync(workspaceId, video, thumbnail, platforms, youtube, tiktok, facebook, instagram, schedule, tag, refId, videoContentType, thumbnailContentType, cancellationToken);
    }

    // ============================================================================
    // PRIVATE PUBLISHING METHODS
    // ============================================================================

    private async Task<string> PublishShortVideoToWorkspaceByFileAsync(
        TaskSettings settings, Stream videoStream, string videoContentType,
        Stream? thumbnailStream, string? thumbnailContentType, string? thumbnailUrl,
        CancellationToken cancellationToken)
    {
        var uploadConfig = await GenerateUploadUrlsAsync(new GenerateUploadUrlsRequest
        {
            VideoFileType = videoContentType,
            ThumbFileType = thumbnailContentType
        }, cancellationToken);

        if (string.IsNullOrEmpty(uploadConfig.Video.UploadFileURL))
            throw new InvalidOperationException("Video upload URL not provided");

        await UploadToR2Async(uploadConfig.Video.UploadFileURL, videoStream, videoContentType, cancellationToken);

        var thumbUrl = await HandleThumbnailUploadAsync(thumbnailStream, thumbnailContentType, thumbnailUrl,
            uploadConfig.Thumb, cancellationToken);

        return await CreateToWorkspaceByFileAsync(new CreateSocialPublisherTaskRequest
        {
            WorkspaceId = settings.WorkspaceId,
            Source = "file",
            VideoURL = uploadConfig.Video.FileURL,
            ThumbURL = thumbUrl,
            IsAllowYouTube = settings.IsAllowYouTube,
            IsAllowTiktok = settings.IsAllowTiktok,
            IsAllowFacebookPage = settings.IsAllowFacebookPage,
            IsAllowInstagram = settings.IsAllowInstagram,
            YouTube = settings.YouTube,
            Tiktok = settings.Tiktok,
            Facebook = settings.Facebook,
            Instagram = settings.Instagram,
            Schedule = settings.Schedule,
            Tag = settings.Tag,
            RefId = settings.RefId
        }, uploadConfig.TaskId, cancellationToken);
    }

    private async Task<string> PublishShortVideoToWorkspaceByUrlAsync(
        TaskSettings settings, string videoUrl,
        Stream? thumbnailStream, string? thumbnailContentType, string? thumbnailUrl,
        CancellationToken cancellationToken)
    {
        string? thumbUrl = null;
        string? taskId = null;

        if (thumbnailStream != null)
        {
            var uploadConfig = await GenerateUploadUrlsAsync(new GenerateUploadUrlsRequest
            {
                ThumbFileType = thumbnailContentType
            }, cancellationToken);

            thumbUrl = await HandleThumbnailUploadAsync(thumbnailStream, thumbnailContentType, thumbnailUrl,
                uploadConfig.Thumb, cancellationToken);
            taskId = uploadConfig.TaskId;
        }
        else
        {
            thumbUrl = thumbnailUrl;
        }

        return await CreateToWorkspaceByUrlAsync(new CreateSocialPublisherTaskRequest
        {
            WorkspaceId = settings.WorkspaceId,
            Source = "url",
            VideoURL = videoUrl,
            ThumbURL = thumbUrl,
            IsAllowYouTube = settings.IsAllowYouTube,
            IsAllowTiktok = settings.IsAllowTiktok,
            IsAllowFacebookPage = settings.IsAllowFacebookPage,
            IsAllowInstagram = settings.IsAllowInstagram,
            YouTube = settings.YouTube,
            Tiktok = settings.Tiktok,
            Facebook = settings.Facebook,
            Instagram = settings.Instagram,
            Schedule = settings.Schedule,
            Tag = settings.Tag,
            RefId = settings.RefId
        }, taskId, cancellationToken);
    }

    private async Task<string> PublishRepostVideoToWorkspaceAsync(
        TaskSettings settings, string videoUrl,
        Stream? thumbnailStream, string? thumbnailContentType, string? thumbnailUrl,
        CancellationToken cancellationToken)
    {
        string? thumbUrl = null;
        string? taskId = null;

        if (thumbnailStream != null)
        {
            var uploadConfig = await GenerateUploadUrlsAsync(new GenerateUploadUrlsRequest
            {
                ThumbFileType = thumbnailContentType
            }, cancellationToken);

            thumbUrl = await HandleThumbnailUploadAsync(thumbnailStream, thumbnailContentType, thumbnailUrl,
                uploadConfig.Thumb, cancellationToken);
            taskId = uploadConfig.TaskId;
        }
        else
        {
            thumbUrl = thumbnailUrl;
        }

        return await CreateToWorkspaceByUrlAsync(new CreateSocialPublisherTaskRequest
        {
            WorkspaceId = settings.WorkspaceId,
            Source = "repost",
            PostURL = videoUrl,
            ThumbURL = thumbUrl,
            IsAllowYouTube = settings.IsAllowYouTube,
            IsAllowTiktok = settings.IsAllowTiktok,
            IsAllowFacebookPage = settings.IsAllowFacebookPage,
            IsAllowInstagram = settings.IsAllowInstagram,
            YouTube = settings.YouTube,
            Tiktok = settings.Tiktok,
            Facebook = settings.Facebook,
            Instagram = settings.Instagram,
            Schedule = settings.Schedule,
            Tag = settings.Tag,
            RefId = settings.RefId
        }, taskId, cancellationToken);
    }

    private async Task<string> PublishShortVideoToAccountByFileAsync(
        TaskSettings settings, Stream videoStream, string videoContentType,
        Stream? thumbnailStream, string? thumbnailContentType, string? thumbnailUrl,
        CancellationToken cancellationToken)
    {
        var uploadConfig = await GenerateUploadUrlsAsync(new GenerateUploadUrlsRequest
        {
            VideoFileType = videoContentType,
            ThumbFileType = thumbnailContentType
        }, cancellationToken);

        if (string.IsNullOrEmpty(uploadConfig.Video.UploadFileURL))
            throw new InvalidOperationException("Video upload URL not provided");

        await UploadToR2Async(uploadConfig.Video.UploadFileURL, videoStream, videoContentType, cancellationToken);

        var thumbUrl = await HandleThumbnailUploadAsync(thumbnailStream, thumbnailContentType, thumbnailUrl,
            uploadConfig.Thumb, cancellationToken);

        return await CreateToAccountByFileAsync(new CreateSocialPublisherAccountTaskRequest
        {
            AccountId = settings.AccountId!,
            Source = "file",
            VideoURL = uploadConfig.Video.FileURL,
            ThumbURL = thumbUrl,
            IsAllowYouTube = settings.IsAllowYouTube,
            IsAllowTiktok = settings.IsAllowTiktok,
            IsAllowFacebookPage = settings.IsAllowFacebookPage,
            IsAllowInstagram = settings.IsAllowInstagram,
            YouTube = settings.YouTube,
            Tiktok = settings.Tiktok,
            Facebook = settings.Facebook,
            Instagram = settings.Instagram,
            Schedule = settings.Schedule,
            Tag = settings.Tag,
            RefId = settings.RefId
        }, uploadConfig.TaskId, cancellationToken);
    }

    private async Task<string> PublishShortVideoToAccountByUrlAsync(
        TaskSettings settings, string videoUrl,
        Stream? thumbnailStream, string? thumbnailContentType, string? thumbnailUrl,
        CancellationToken cancellationToken)
    {
        string? thumbUrl = null;
        string? taskId = null;

        if (thumbnailStream != null)
        {
            var uploadConfig = await GenerateUploadUrlsAsync(new GenerateUploadUrlsRequest
            {
                ThumbFileType = thumbnailContentType
            }, cancellationToken);

            thumbUrl = await HandleThumbnailUploadAsync(thumbnailStream, thumbnailContentType, thumbnailUrl,
                uploadConfig.Thumb, cancellationToken);
            taskId = uploadConfig.TaskId;
        }
        else
        {
            thumbUrl = thumbnailUrl;
        }

        return await CreateToAccountByUrlAsync(new CreateSocialPublisherAccountTaskRequest
        {
            AccountId = settings.AccountId!,
            Source = "url",
            VideoURL = videoUrl,
            ThumbURL = thumbUrl,
            IsAllowYouTube = settings.IsAllowYouTube,
            IsAllowTiktok = settings.IsAllowTiktok,
            IsAllowFacebookPage = settings.IsAllowFacebookPage,
            IsAllowInstagram = settings.IsAllowInstagram,
            YouTube = settings.YouTube,
            Tiktok = settings.Tiktok,
            Facebook = settings.Facebook,
            Instagram = settings.Instagram,
            Schedule = settings.Schedule,
            Tag = settings.Tag,
            RefId = settings.RefId
        }, taskId, cancellationToken);
    }

    private async Task<string> PublishRepostVideoToAccountAsync(
        TaskSettings settings, string videoUrl,
        Stream? thumbnailStream, string? thumbnailContentType, string? thumbnailUrl,
        CancellationToken cancellationToken)
    {
        string? thumbUrl = null;
        string? taskId = null;

        if (thumbnailStream != null)
        {
            var uploadConfig = await GenerateUploadUrlsAsync(new GenerateUploadUrlsRequest
            {
                ThumbFileType = thumbnailContentType
            }, cancellationToken);

            thumbUrl = await HandleThumbnailUploadAsync(thumbnailStream, thumbnailContentType, thumbnailUrl,
                uploadConfig.Thumb, cancellationToken);
            taskId = uploadConfig.TaskId;
        }
        else
        {
            thumbUrl = thumbnailUrl;
        }

        return await CreateToAccountByUrlAsync(new CreateSocialPublisherAccountTaskRequest
        {
            AccountId = settings.AccountId!,
            Source = "repost",
            PostURL = videoUrl,
            ThumbURL = thumbUrl,
            IsAllowYouTube = settings.IsAllowYouTube,
            IsAllowTiktok = settings.IsAllowTiktok,
            IsAllowFacebookPage = settings.IsAllowFacebookPage,
            IsAllowInstagram = settings.IsAllowInstagram,
            YouTube = settings.YouTube,
            Tiktok = settings.Tiktok,
            Facebook = settings.Facebook,
            Instagram = settings.Instagram,
            Schedule = settings.Schedule,
            Tag = settings.Tag,
            RefId = settings.RefId
        }, taskId, cancellationToken);
    }

    // ============================================================================
    // PRIVATE CREATE METHODS - Matches TypeScript createByFile/createByURL
    // ============================================================================

    private async Task<string> CreateToWorkspaceByFileAsync(
        CreateSocialPublisherTaskRequest request, string? id, CancellationToken cancellationToken)
    {
        var path = string.IsNullOrEmpty(id) ? $"{BasePath}/short-video/workspace/by-file" : $"{BasePath}/short-video/workspace/by-file/{id}";
        var payload = new
        {
            request.WorkspaceId, request.Source, request.IsAllowYouTube, request.IsAllowTiktok,
            request.IsAllowFacebookPage, request.IsAllowInstagram, request.YouTube, request.Tiktok,
            request.Facebook, request.Instagram, request.VideoURL, request.ThumbURL, request.PostURL,
            request.Schedule, request.Tag, request.RefId, createdFrom = "dotnetPackage"
        };

        var response = await _http.PostAsync<Dictionary<string, object>>(path, payload, cancellationToken);
        if (response.Result != null && response.Result.TryGetValue("_id", out var taskIdObj))
            return taskIdObj?.ToString() ?? throw new InvalidOperationException("Task ID not returned");

        throw new InvalidOperationException("Failed to create task");
    }

    private async Task<string> CreateToWorkspaceByUrlAsync(
        CreateSocialPublisherTaskRequest request, string? id, CancellationToken cancellationToken)
    {
        var path = string.IsNullOrEmpty(id) ? $"{BasePath}/short-video/workspace/by-url" : $"{BasePath}/short-video/workspace/by-url/{id}";
        var payload = new
        {
            request.WorkspaceId, request.Source, request.IsAllowYouTube, request.IsAllowTiktok,
            request.IsAllowFacebookPage, request.IsAllowInstagram, request.YouTube, request.Tiktok,
            request.Facebook, request.Instagram, request.VideoURL, request.ThumbURL, request.PostURL,
            request.Schedule, request.Tag, request.RefId, createdFrom = "dotnetPackage"
        };

        var response = await _http.PostAsync<Dictionary<string, object>>(path, payload, cancellationToken);
        if (response.Result != null && response.Result.TryGetValue("_id", out var taskIdObj))
            return taskIdObj?.ToString() ?? throw new InvalidOperationException("Task ID not returned");

        throw new InvalidOperationException("Failed to create task");
    }

    private async Task<string> CreateToAccountByFileAsync(
        CreateSocialPublisherAccountTaskRequest request, string? id, CancellationToken cancellationToken)
    {
        var path = string.IsNullOrEmpty(id) ? $"{BasePath}/short-video/account/by-file" : $"{BasePath}/short-video/account/by-file/{id}";
        var payload = new
        {
            request.AccountId, request.Source, request.IsAllowYouTube, request.IsAllowTiktok,
            request.IsAllowFacebookPage, request.IsAllowInstagram, request.YouTube, request.Tiktok,
            request.Facebook, request.Instagram, request.VideoURL, request.ThumbURL, request.PostURL,
            request.Schedule, request.Tag, request.RefId, createdFrom = "dotnetPackage"
        };

        var response = await _http.PostAsync<Dictionary<string, object>>(path, payload, cancellationToken);
        if (response.Result != null && response.Result.TryGetValue("_id", out var taskIdObj))
            return taskIdObj?.ToString() ?? throw new InvalidOperationException("Task ID not returned");

        throw new InvalidOperationException("Failed to create task");
    }

    private async Task<string> CreateToAccountByUrlAsync(
        CreateSocialPublisherAccountTaskRequest request, string? id, CancellationToken cancellationToken)
    {
        var path = string.IsNullOrEmpty(id) ? $"{BasePath}/short-video/account/by-url" : $"{BasePath}/short-video/account/by-url/{id}";
        var payload = new
        {
            request.AccountId, request.Source, request.IsAllowYouTube, request.IsAllowTiktok,
            request.IsAllowFacebookPage, request.IsAllowInstagram, request.YouTube, request.Tiktok,
            request.Facebook, request.Instagram, request.VideoURL, request.ThumbURL, request.PostURL,
            request.Schedule, request.Tag, request.RefId, createdFrom = "dotnetPackage"
        };

        var response = await _http.PostAsync<Dictionary<string, object>>(path, payload, cancellationToken);
        if (response.Result != null && response.Result.TryGetValue("_id", out var taskIdObj))
            return taskIdObj?.ToString() ?? throw new InvalidOperationException("Task ID not returned");

        throw new InvalidOperationException("Failed to create task");
    }

    // ============================================================================
    // PRIVATE HELPER METHODS
    // ============================================================================

    private string DetectVideoSource(object video)
    {
        if (video is Stream) return "file";
        
        if (video is string url)
        {
            if (System.Text.RegularExpressions.Regex.IsMatch(url,
                @"^https?://(www\.)?(facebook\.com|fb\.watch)/(reel|watch|.*\/videos)/.*",
                System.Text.RegularExpressions.RegexOptions.IgnoreCase))
                return "repost";

       if (System.Text.RegularExpressions.Regex.IsMatch(
    url,
    @"^https?:\/\/(www\.)?(tiktok\.com\/@[^\/]+\/video\/\d+|vm\.tiktok\.com\/\w+|vt\.tiktok\.com\/\w+)",
    System.Text.RegularExpressions.RegexOptions.IgnoreCase
))
    return "repost";

            if (System.Text.RegularExpressions.Regex.IsMatch(url,
                @"^https?://(www\.)?(youtube\.com/shorts/|youtu\.be/)[A-Za-z0-9_-]+",
                System.Text.RegularExpressions.RegexOptions.IgnoreCase))
                return "repost";

            return "url";
        }

        throw new ArgumentException("Invalid video type. Must be Stream or string URL");
    }

    private async Task UploadToR2Async(string uploadUrl, Stream fileStream, string contentType, CancellationToken cancellationToken)
    {
        using var client = new HttpClient();
        using var content = new StreamContent(fileStream);
        content.Headers.ContentType = new MediaTypeHeaderValue(contentType);
        var response = await client.PutAsync(uploadUrl, content, cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    private async Task<string?> HandleThumbnailUploadAsync(Stream? thumbnailStream, string? thumbnailContentType,
        string? thumbnailUrl, UploadUrlInfo thumbInfo, CancellationToken cancellationToken)
    {
        if (thumbnailStream != null && !string.IsNullOrEmpty(thumbInfo.UploadFileURL))
        {
            await UploadToR2Async(thumbInfo.UploadFileURL, thumbnailStream, thumbnailContentType!, cancellationToken);
            return thumbInfo.FileURL;
        }
        return thumbnailUrl;
    }
}
