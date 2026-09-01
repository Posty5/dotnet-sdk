using System.Net.Http.Headers;
using Posty5.Core.Http;
using Posty5.Core.Models;
using Posty5.SocialPublisherPost.Models;

namespace Posty5.SocialPublisherPost;

/// <summary>
 /// Client for managing social publisher posts via Posty5 API
/// Supports publishing videos to YouTube, TikTok, Facebook, and Instagram
/// </summary>
public class SocialPublisherPostClient
{
    private readonly Posty5HttpClient _http;
    private const string BasePath = "/api/social-publisher-post";
    
    /// <summary>
    /// Maximum video upload size (4GB)
    /// </summary>
    public const long MaxVideoUploadSizeBytes = 4L * 1024 * 1024 * 1024;

    /// <summary>
    /// Creates a new Social Publisher Post client
    /// </summary>
    /// <param name="httpClient">HTTP client instance from Posty5.Core</param>
    public SocialPublisherPostClient(Posty5HttpClient httpClient)
    {
        _http = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    }

    // ============================================================================
    // CORE CRUD METHODS (PUBLIC)
    // ============================================================================

    /// <summary>
    /// List/search posts with pagination and filters
    /// </summary>
    public async Task<PaginationResponse<PostModel>> ListAsync(
        ListPostsParams? listParams = null,
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

        var response = await _http.GetAsync<PaginationResponse<PostModel>>(
            BasePath,
            queryParams,
            cancellationToken);

        return response.Result ?? new PaginationResponse<PostModel>();
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
    /// Get post status by ID with full details including platform configurations
    /// </summary>
    public async Task<PostStatusFullDetailsResponse> GetStatusAsync(
        string id,
        CancellationToken cancellationToken = default)
    {
        var response = await _http.GetAsync<PostStatusFullDetailsResponse>(
            $"{BasePath}/{id}",
            cancellationToken: cancellationToken);

        return response.Result ?? throw new InvalidOperationException("Post not found");
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
    /// Get next and previous post IDs for navigation
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
    /// Publish a short video to connected social media accounts.
    /// Use original or authorized content only. TikTok Direct Post requires
    /// the creator-controlled Posty5 review and confirmation flow.
    /// </summary>
    public async Task<string> PublishShortVideoToWorkspaceAsync(
        string workspaceId,
        object video,
        object? thumbnail = null,
        YouTubeConfig? youtube = null,
        TikTokConfig? tiktok = null,
        FacebookPageConfig? facebook = null,
        InstagramConfig? instagram = null,
        object? schedule = null,
        string? tag = null,
        string? refId = null,
        string? videoContentType = null,
        string? thumbnailContentType = null,
        CommentRequest? comment = null,
        CancellationToken cancellationToken = default)
    {
        // Build post settings.
        // The server derives which platforms to publish to from the
        // workspace's connected accounts; the client just supplies the
        // platform configs that match (and any platform not connected
        // is silently ignored).
        var settings = new PostSettings
        {
            WorkspaceId = workspaceId,
            Youtube = youtube,
            Tiktok = tiktok,
            Facebook = facebook,
            Instagram = instagram,
            Comment = comment,
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
            _ => throw new InvalidOperationException($"Unknown video source type: {sourceType}")
        };
    }

    /// <summary>
    /// Publish a short video to a connected social media account.
    /// Use original or authorized content only.
    /// </summary>
    public async Task<string> PublishShortVideoToAccountAsync(
        string accountId,
        object video,
        object? thumbnail = null,
        YouTubeConfig? youtube = null,
        TikTokConfig? tiktok = null,
        FacebookPageConfig? facebook = null,
        InstagramConfig? instagram = null,
        object? schedule = null,
        string? tag = null,
        string? refId = null,
        string? videoContentType = null,
        string? thumbnailContentType = null,
        CommentRequest? comment = null,
        CancellationToken cancellationToken = default)
    {
        // Build post settings.
        // The server derives the publish target from the account's platform field;
        // the client supplies whichever platform config matches that account.
        var settings = new PostSettings
        {
            AccountId = accountId,
            Youtube = youtube,
            Tiktok = tiktok,
            Facebook = facebook,
            Instagram = instagram,
            Comment = comment,
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
            _ => throw new InvalidOperationException($"Unknown video source type: {sourceType}")
        };
    }


    // ============================================================================
    // PRIVATE PUBLISHING METHODS
    // ============================================================================

    private async Task<string> PublishShortVideoToWorkspaceByFileAsync(
        PostSettings settings, Stream videoStream, string videoContentType,
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

        return await CreateToWorkspaceByFileAsync(new CreateSocialPublisherPostRequest
        {
            WorkspaceId = settings.WorkspaceId,
            Source = "video-file",
            VideoURL = uploadConfig.Video.FileURL,
            ThumbURL = thumbUrl,
            Youtube = settings.Youtube,
            Tiktok = settings.Tiktok,
            Facebook = settings.Facebook,
            Instagram = settings.Instagram,
            Schedule = settings.Schedule,
            Comment = settings.Comment,
            Tag = settings.Tag,
            RefId = settings.RefId
        }, uploadConfig.PostId, cancellationToken);
    }

    private async Task<string> PublishShortVideoToWorkspaceByUrlAsync(
        PostSettings settings, string videoUrl,
        Stream? thumbnailStream, string? thumbnailContentType, string? thumbnailUrl,
        CancellationToken cancellationToken)
    {
        string? thumbUrl = null;
        string? postId = null;

        if (thumbnailStream != null)
        {
            var uploadConfig = await GenerateUploadUrlsAsync(new GenerateUploadUrlsRequest
            {
                ThumbFileType = thumbnailContentType
            }, cancellationToken);

            thumbUrl = await HandleThumbnailUploadAsync(thumbnailStream, thumbnailContentType, thumbnailUrl,
                uploadConfig.Thumb, cancellationToken);
            postId = uploadConfig.PostId;
        }
        else
        {
            thumbUrl = thumbnailUrl;
        }

        return await CreateToWorkspaceByUrlAsync(new CreateSocialPublisherPostRequest
        {
            WorkspaceId = settings.WorkspaceId,
            Source = "video-url",
            VideoURL = videoUrl,
            ThumbURL = thumbUrl,
            Youtube = settings.Youtube,
            Tiktok = settings.Tiktok,
            Facebook = settings.Facebook,
            Instagram = settings.Instagram,
            Schedule = settings.Schedule,
            Comment = settings.Comment,
            Tag = settings.Tag,
            RefId = settings.RefId
        }, postId, cancellationToken);
    }

    private async Task<string> PublishShortVideoToAccountByFileAsync(
        PostSettings settings, Stream videoStream, string videoContentType,
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

        return await CreateToAccountByFileAsync(new CreateSocialPublisherAccountPostRequest
        {
            AccountId = settings.AccountId!,
            Source = "video-file",
            VideoURL = uploadConfig.Video.FileURL,
            ThumbURL = thumbUrl,
            Youtube = settings.Youtube,
            Tiktok = settings.Tiktok,
            Facebook = settings.Facebook,
            Instagram = settings.Instagram,
            Schedule = settings.Schedule,
            Comment = settings.Comment,
            Tag = settings.Tag,
            RefId = settings.RefId
        }, uploadConfig.PostId, cancellationToken);
    }

    private async Task<string> PublishShortVideoToAccountByUrlAsync(
        PostSettings settings, string videoUrl,
        Stream? thumbnailStream, string? thumbnailContentType, string? thumbnailUrl,
        CancellationToken cancellationToken)
    {
        string? thumbUrl = null;
        string? postId = null;

        if (thumbnailStream != null)
        {
            var uploadConfig = await GenerateUploadUrlsAsync(new GenerateUploadUrlsRequest
            {
                ThumbFileType = thumbnailContentType
            }, cancellationToken);

            thumbUrl = await HandleThumbnailUploadAsync(thumbnailStream, thumbnailContentType, thumbnailUrl,
                uploadConfig.Thumb, cancellationToken);
            postId = uploadConfig.PostId;
        }
        else
        {
            thumbUrl = thumbnailUrl;
        }

        return await CreateToAccountByUrlAsync(new CreateSocialPublisherAccountPostRequest
        {
            AccountId = settings.AccountId!,
            Source = "video-url",
            VideoURL = videoUrl,
            ThumbURL = thumbUrl,
            Youtube = settings.Youtube,
            Tiktok = settings.Tiktok,
            Facebook = settings.Facebook,
            Instagram = settings.Instagram,
            Schedule = settings.Schedule,
            Comment = settings.Comment,
            Tag = settings.Tag,
            RefId = settings.RefId
        }, postId, cancellationToken);
    }

    // ============================================================================
    // PRIVATE CREATE METHODS - Matches TypeScript createByFile/createByURL
    // ============================================================================

    private async Task<string> CreateToWorkspaceByFileAsync(
        CreateSocialPublisherPostRequest request, string? id, CancellationToken cancellationToken)
    {
        var path = string.IsNullOrEmpty(id) ? $"{BasePath}/short-video/workspace/by-file" : $"{BasePath}/short-video/workspace/by-file/{id}";
        var payload = new
        {
            request.WorkspaceId, request.Source, request.Youtube, request.Tiktok,
            request.Facebook, request.Instagram, request.VideoURL, request.ThumbURL,
            request.Schedule, request.Comment, request.Tag, request.RefId, createdFrom = "dotnetPackage"
        };

        var response = await _http.PostAsync<Dictionary<string, object>>(path, payload, cancellationToken);
        if (response.Result != null && response.Result.TryGetValue("_id", out var postIdObj))
            return postIdObj?.ToString() ?? throw new InvalidOperationException("Post ID not returned");

        throw new InvalidOperationException("Failed to create post");
    }

    private async Task<string> CreateToWorkspaceByUrlAsync(
        CreateSocialPublisherPostRequest request, string? id, CancellationToken cancellationToken)
    {
        var path = string.IsNullOrEmpty(id) ? $"{BasePath}/short-video/workspace/by-url" : $"{BasePath}/short-video/workspace/by-url/{id}";
        var payload = new
        {
            request.WorkspaceId, request.Source, request.Youtube, request.Tiktok,
            request.Facebook, request.Instagram, request.VideoURL, request.ThumbURL,
            request.Schedule, request.Comment, request.Tag, request.RefId, createdFrom = "dotnetPackage"
        };

        var response = await _http.PostAsync<Dictionary<string, object>>(path, payload, cancellationToken);
        if (response.Result != null && response.Result.TryGetValue("_id", out var postIdObj))
            return postIdObj?.ToString() ?? throw new InvalidOperationException("Post ID not returned");

        throw new InvalidOperationException("Failed to create post");
    }

    private async Task<string> CreateToAccountByFileAsync(
        CreateSocialPublisherAccountPostRequest request, string? id, CancellationToken cancellationToken)
    {
        var path = string.IsNullOrEmpty(id) ? $"{BasePath}/short-video/account/by-file" : $"{BasePath}/short-video/account/by-file/{id}";
        var payload = new
        {
            request.AccountId, request.Source, request.Youtube, request.Tiktok,
            request.Facebook, request.Instagram, request.VideoURL, request.ThumbURL,
            request.Schedule, request.Comment, request.Tag, request.RefId, createdFrom = "dotnetPackage"
        };

        var response = await _http.PostAsync<Dictionary<string, object>>(path, payload, cancellationToken);
        if (response.Result != null && response.Result.TryGetValue("_id", out var postIdObj))
            return postIdObj?.ToString() ?? throw new InvalidOperationException("Post ID not returned");

        throw new InvalidOperationException("Failed to create post");
    }

    private async Task<string> CreateToAccountByUrlAsync(
        CreateSocialPublisherAccountPostRequest request, string? id, CancellationToken cancellationToken)
    {
        var path = string.IsNullOrEmpty(id) ? $"{BasePath}/short-video/account/by-url" : $"{BasePath}/short-video/account/by-url/{id}";
        var payload = new
        {
            request.AccountId, request.Source, request.Youtube, request.Tiktok,
            request.Facebook, request.Instagram, request.VideoURL, request.ThumbURL,
            request.Schedule, request.Comment, request.Tag, request.RefId, createdFrom = "dotnetPackage"
        };

        var response = await _http.PostAsync<Dictionary<string, object>>(path, payload, cancellationToken);
        if (response.Result != null && response.Result.TryGetValue("_id", out var postIdObj))
            return postIdObj?.ToString() ?? throw new InvalidOperationException("Post ID not returned");

        throw new InvalidOperationException("Failed to create post");
    }

    // ============================================================================
    // PRIVATE HELPER METHODS
    // ============================================================================

// ============================================================================
    // LONG VIDEO (up to 60 minutes)
    // ============================================================================

    /// <summary>
    /// Price a long video before publishing it. Creates nothing and charges
    /// nothing.
    /// </summary>
    /// <remarks>
    /// The server reads the video, measures its duration, and returns the exact
    /// cost plus what each platform would do with a video that long. The units
    /// come from the same calculation the publish call charges with, so the
    /// quote and the bill cannot disagree.
    /// <para>
    /// <c>videoUrl</c> is the URL of an uploaded or externally hosted video.
    /// </para>
    /// </remarks>
    public async Task<LongVideoQuoteResponse> GetLongVideoQuoteAsync(
        string videoUrl,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(videoUrl))
            throw new ArgumentException("videoUrl is required", nameof(videoUrl));

        var response = await _http.PostAsync<LongVideoQuoteResponse>(
            $"{BasePath}/long-video/quote",
            new LongVideoQuoteRequest { VideoURL = videoUrl },
            cancellationToken: cancellationToken);

        return response.Result ?? new LongVideoQuoteResponse();
    }

    /// <summary>
    /// Publish a video of up to 60 minutes to every account connected to a
    /// workspace.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Cost is duration-based — 50 credits per started 5 minutes — so a
    /// 12-minute video costs 150. Call <see cref="GetLongVideoQuoteAsync"/>
    /// first if you need to show the price. There is no duration parameter and
    /// one would be ignored: the server measures the video itself, because a
    /// client-supplied duration would be a client-supplied price.
    /// </para>
    /// <para>
    /// Platforms disagree about "long": a 40-minute video publishes to YouTube
    /// and Facebook but is refused by Instagram, whose Reels cap at 15 minutes.
    /// Those targets come back in
    /// <see cref="PublishLongVideoResult.RefusedTargets"/> — the post still
    /// publishes to the rest.
    /// </para>
    /// <para>
    /// Cancelling mid-upload aborts the transfer. The reserved post is never
    /// created, so nothing is charged.
    /// </para>
    /// <para>
    /// <c>video</c> is a <see cref="Stream"/> to upload or a URL string;
    /// <c>progress</c> reports upload progress and needs a seekable stream to
    /// know the total.
    /// </para>
    /// <para>
    /// A seekable stream is uploaded resumably, so a dropped connection costs
    /// one chunk rather than the whole transfer. <c>onUploadUrl</c> receives the
    /// upload's URL once it exists — persist it and pass it back as
    /// <c>resumeFrom</c> to continue an interrupted transfer, including after a
    /// process restart. The ticket expires in minutes, but an upload that
    /// already exists resumes by its own URL and is unaffected.
    /// </para>
    /// </remarks>
    public async Task<PublishLongVideoResult> PublishLongVideoToWorkspaceAsync(
        string workspaceId,
        object video,
        object? thumbnail = null,
        YouTubeConfig? youtube = null,
        TikTokConfig? tiktok = null,
        FacebookPageConfig? facebook = null,
        InstagramConfig? instagram = null,
        object? schedule = null,
        string? tag = null,
        string? refId = null,
        string? videoContentType = null,
        string? thumbnailContentType = null,
        CommentRequest? comment = null,
        IProgress<UploadProgress>? progress = null,
        Action<string>? onUploadUrl = null,
        string? resumeFrom = null,
        CancellationToken cancellationToken = default,
        // After the token rather than beside `resumeFrom`, even though that reads
        // oddly: inserting an optional parameter ahead of an existing one is
        // source-breaking for anyone passing the token positionally.
        bool terminateOnCancel = false)
    {
        if (string.IsNullOrWhiteSpace(workspaceId))
            throw new ArgumentException("workspaceId is required", nameof(workspaceId));
        if (video == null)
            throw new ArgumentNullException(nameof(video));
        if (youtube == null && tiktok == null && facebook == null && instagram == null)
            throw new ArgumentException("Provide at least one platform configuration (youtube / tiktok / facebook / instagram)");

        var settings = new PostSettings
        {
            WorkspaceId = workspaceId,
            Youtube = youtube,
            Tiktok = tiktok,
            Facebook = facebook,
            Instagram = instagram,
            Comment = comment,
            Tag = tag,
            RefId = refId,
            Schedule = BuildSchedule(schedule)
        };

        var isFile = DetectVideoSource(video) == "file";
        string videoUrl;
        string? postId = null;
        string? thumbUrl;

        if (isFile)
        {
            var upload = await UploadLongVideoAsync(
                (Stream)video, videoContentType ?? "video/mp4",
                thumbnail as Stream, thumbnailContentType, progress, cancellationToken,
                onUploadUrl, resumeFrom, terminateOnCancel: terminateOnCancel);

            videoUrl = upload.VideoUrl;
            postId = upload.PostId;
            thumbUrl = await HandleThumbnailUploadAsync(
                thumbnail as Stream, thumbnailContentType, thumbnail as string, upload.Config.Thumb, cancellationToken);
        }
        else
        {
            videoUrl = (string)video;
            thumbUrl = thumbnail as string;
        }

        var body = new CreateSocialPublisherPostRequest
        {
            WorkspaceId = workspaceId,
            Source = isFile ? "video-file" : "video-url",
            VideoURL = videoUrl,
            ThumbURL = thumbUrl,
            Youtube = settings.Youtube,
            Tiktok = settings.Tiktok,
            Facebook = settings.Facebook,
            Instagram = settings.Instagram,
            Schedule = settings.Schedule,
            Comment = settings.Comment,
            Tag = settings.Tag,
            RefId = settings.RefId
        };

        var segment = isFile ? "by-file" : "by-url";
        var path = string.IsNullOrEmpty(postId)
            ? $"{BasePath}/long-video/workspace/{segment}"
            : $"{BasePath}/long-video/workspace/{segment}/{postId}";

        var response = await _http.PostAsync<PublishLongVideoResult>(path, body, cancellationToken: cancellationToken);
        return response.Result ?? new PublishLongVideoResult();
    }

    /// <summary>
    /// Publish a video of up to 60 minutes to a single connected account.
    /// </summary>
    /// <remarks>
    /// With one target there is no partial success: if that platform will not
    /// take a video this long, the whole call is refused and nothing is charged.
    /// See <see cref="PublishLongVideoToWorkspaceAsync"/> for the cost model.
    /// </remarks>
    public async Task<PublishLongVideoResult> PublishLongVideoToAccountAsync(
        string accountId,
        object video,
        object? thumbnail = null,
        YouTubeConfig? youtube = null,
        TikTokConfig? tiktok = null,
        FacebookPageConfig? facebook = null,
        InstagramConfig? instagram = null,
        object? schedule = null,
        string? tag = null,
        string? refId = null,
        string? videoContentType = null,
        string? thumbnailContentType = null,
        CommentRequest? comment = null,
        IProgress<UploadProgress>? progress = null,
        Action<string>? onUploadUrl = null,
        string? resumeFrom = null,
        CancellationToken cancellationToken = default,
        // After the token rather than beside `resumeFrom`, even though that reads
        // oddly: inserting an optional parameter ahead of an existing one is
        // source-breaking for anyone passing the token positionally.
        bool terminateOnCancel = false)
    {
        if (string.IsNullOrWhiteSpace(accountId))
            throw new ArgumentException("accountId is required", nameof(accountId));
        if (video == null)
            throw new ArgumentNullException(nameof(video));
        if (youtube == null && tiktok == null && facebook == null && instagram == null)
            throw new ArgumentException("Provide the platform configuration matching the account (youtube / tiktok / facebook / instagram)");

        var isFile = DetectVideoSource(video) == "file";
        string videoUrl;
        string? postId = null;
        string? thumbUrl;

        if (isFile)
        {
            var upload = await UploadLongVideoAsync(
                (Stream)video, videoContentType ?? "video/mp4",
                thumbnail as Stream, thumbnailContentType, progress, cancellationToken,
                onUploadUrl, resumeFrom, terminateOnCancel: terminateOnCancel);

            videoUrl = upload.VideoUrl;
            postId = upload.PostId;
            thumbUrl = await HandleThumbnailUploadAsync(
                thumbnail as Stream, thumbnailContentType, thumbnail as string, upload.Config.Thumb, cancellationToken);
        }
        else
        {
            videoUrl = (string)video;
            thumbUrl = thumbnail as string;
        }

        var body = new CreateSocialPublisherAccountPostRequest
        {
            AccountId = accountId,
            Source = isFile ? "video-file" : "video-url",
            VideoURL = videoUrl,
            ThumbURL = thumbUrl,
            Youtube = youtube,
            Tiktok = tiktok,
            Facebook = facebook,
            Instagram = instagram,
            Schedule = BuildSchedule(schedule),
            Comment = comment,
            Tag = tag,
            RefId = refId
        };

        var segment = isFile ? "by-file" : "by-url";
        var path = string.IsNullOrEmpty(postId)
            ? $"{BasePath}/long-video/account/{segment}"
            : $"{BasePath}/long-video/account/{segment}/{postId}";

        var response = await _http.PostAsync<PublishLongVideoResult>(path, body, cancellationToken: cancellationToken);
        return response.Result ?? new PublishLongVideoResult();
    }

    /// <summary>
    /// Re-schedule a post that has not published yet, or send it out now.
    /// Costs no credits.
    /// </summary>
    /// <remarks>
    /// Only posts still pending with a future publish time are eligible;
    /// anything that has started publishing is refused by the server with a
    /// reason.
    /// <para>
    /// <c>schedule</c> is a <see cref="DateTime"/> for the new time or the
    /// string "now"; <c>caption</c> optionally replaces the caption too.
    /// </para>
    /// </remarks>
    /// <summary>
    /// Delete a post that has not published yet, releasing its uploaded media in
    /// the same request.
    /// </summary>
    /// <remarks>
    /// Free, and nothing is refunded — nothing was charged for a post that never
    /// went out. A post that HAS published is refused; use the remove flow to
    /// take down media that is already live.
    /// </remarks>
    public async Task DeletePostAsync(string id, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("id is required", nameof(id));

        await _http.DeleteAsync<object>($"{BasePath}/{id}", cancellationToken: cancellationToken);
    }

    public async Task ReschedulePostAsync(
        string id,
        object schedule,
        string? caption = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("id is required", nameof(id));

        var built = BuildSchedule(schedule)
            ?? throw new ArgumentException("schedule must be a DateTime or the string \"now\"", nameof(schedule));

        await _http.PutAsync<object>(
            $"{BasePath}/{id}",
            new ReschedulePostRequest { Schedule = built, Caption = caption },
            cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Upload a long video and return the URL to publish from.
    /// </summary>
    /// <remarks>
    /// Declares <c>PostType = "longVideo"</c> so the server refuses now — on
    /// plan gating or an empty balance — rather than after an hour of transfer.
    /// The thumbnail slot is requested in the SAME call so both files land in
    /// this post's folder; a separate request would allocate a second post id.
    /// </remarks>
    private async Task<(string VideoUrl, string PostId, GenerateUploadUrlsResponse Config)> UploadLongVideoAsync(
        Stream videoStream,
        string videoContentType,
        Stream? thumbnailStream,
        string? thumbnailContentType,
        IProgress<UploadProgress>? progress,
        CancellationToken cancellationToken,
        Action<string>? onUploadUrl = null,
        string? resumeFrom = null,
        string fileName = "upload",
        bool terminateOnCancel = false)
    {
        var config = await GenerateUploadUrlsAsync(new GenerateUploadUrlsRequest
        {
            VideoFileType = videoContentType,
            ThumbFileType = thumbnailContentType,
            PostType = "longVideo"
        }, cancellationToken);

        // Prefer the resumable transfer for a long video — this is the case a
        // single PUT handles worst, since a dropped connection at 90% of an hour
        // of footage otherwise starts again from zero. Servers without the
        // resumable service omit the tus fields, and the signed PUT still works.
        // A non-seekable stream cannot resume, so it also takes the PUT path.
        if (ResumableUpload.IsSupported(config.Video) && videoStream.CanSeek)
        {
            await ResumableUpload.UploadAsync(
                config.Video,
                videoStream,
                videoContentType,
                fileName,
                progress,
                onUploadUrl,
                resumeFrom,
                cancellationToken: cancellationToken,
                terminateOnCancel: terminateOnCancel);
        }
        else
        {
            if (string.IsNullOrEmpty(config.Video.UploadFileURL))
                throw new InvalidOperationException("Video upload URL not provided");

            await UploadLargeFileAsync(config.Video.UploadFileURL, videoStream, videoContentType, progress, cancellationToken);
        }

        return (config.Video.FileURL ?? string.Empty, config.PostId, config);
    }

    /// <summary>
    /// Upload a large file with no request timeout.
    /// </summary>
    /// <remarks>
    /// <see cref="HttpClient"/> defaults to a 100-second timeout, which an hour
    /// of video will blow through long before the transfer finishes. The
    /// timeout is disabled on THIS client only — a per-call decision, not a
    /// global one — and cancellation is what bounds the operation instead.
    /// </remarks>
    private static async Task UploadLargeFileAsync(
        string uploadUrl,
        Stream fileStream,
        string contentType,
        IProgress<UploadProgress>? progress,
        CancellationToken cancellationToken)
    {
        using var client = new HttpClient { Timeout = Timeout.InfiniteTimeSpan };

        long? total = fileStream.CanSeek ? fileStream.Length : null;
        progress?.Report(new UploadProgress { BytesTransferred = 0, TotalBytes = total });

        using var content = new StreamContent(fileStream);
        content.Headers.ContentType = new MediaTypeHeaderValue(contentType);

        var response = await client.PutAsync(uploadUrl, content, cancellationToken);
        response.EnsureSuccessStatusCode();

        progress?.Report(new UploadProgress { BytesTransferred = total ?? 0, TotalBytes = total });
    }

    /// <summary>
    /// Translate the loosely typed schedule argument ("now" or a DateTime) into
    /// the wire shape. Returns null when nothing was supplied, letting the
    /// server apply its own default.
    /// </summary>
    private static ScheduleConfig? BuildSchedule(object? schedule) => schedule switch
    {
        null => null,
        string s when s == "now" => new ScheduleConfig { Type = "now" },
        DateTime when schedule is DateTime dt => new ScheduleConfig { Type = "schedule", ScheduledAt = dt },
        _ => null
    };

    private string DetectVideoSource(object video)
    {
        if (video is Stream) return "file";
        
        if (video is string url)
        {
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

    // ========================================================================
    // IMAGE POST (task 6, 4.1.0+)
    // ========================================================================

    /// <summary>
    /// Create an image post targeting a workspace. The image is published
    /// to every connected account (Facebook / Instagram / TikTok photo).
    /// YouTube community posts are always reported as <c>notSupported</c>.
    /// Cost: 5 credits, plus +1 if <see cref="CreateImagePostToWorkspaceRequest.Comment"/> is supplied.
    /// </summary>
    public async Task<string> CreateImagePostToWorkspaceAsync(
        CreateImagePostToWorkspaceRequest request,
        string? id = null,
        CancellationToken cancellationToken = default)
    {
        var path = string.IsNullOrEmpty(id)
            ? $"{BasePath}/image/workspace"
            : $"{BasePath}/image/workspace/{id}";

        var payload = new
        {
            request.WorkspaceId,
            request.Image,
            request.Caption,
            request.AiEnhanced,
            request.Youtube,
            request.Tiktok,
            request.Facebook,
            request.Instagram,
            request.Schedule,
            request.Comment,
            request.Tag,
            request.RefId,
            createdFrom = "dotnetPackage",
        };

        var response = await _http.PostAsync<Dictionary<string, object>>(path, payload, cancellationToken);
        if (response.Result != null && response.Result.TryGetValue("_id", out var postIdObj))
            return postIdObj?.ToString() ?? throw new InvalidOperationException("Post ID not returned");

        throw new InvalidOperationException("Failed to create image post");
    }

    /// <summary>
    /// Create an image post targeting a single account. The server derives
    /// the target platform from <c>account.platform</c>; config blocks for
    /// other platforms are ignored.
    /// </summary>
    public async Task<string> CreateImagePostToAccountAsync(
        CreateImagePostToAccountRequest request,
        string? id = null,
        CancellationToken cancellationToken = default)
    {
        var path = string.IsNullOrEmpty(id)
            ? $"{BasePath}/image/account"
            : $"{BasePath}/image/account/{id}";

        var payload = new
        {
            request.AccountId,
            request.Image,
            request.Caption,
            request.AiEnhanced,
            request.Youtube,
            request.Tiktok,
            request.Facebook,
            request.Instagram,
            request.Schedule,
            request.Comment,
            request.Tag,
            request.RefId,
            createdFrom = "dotnetPackage",
        };

        var response = await _http.PostAsync<Dictionary<string, object>>(path, payload, cancellationToken);
        if (response.Result != null && response.Result.TryGetValue("_id", out var postIdObj))
            return postIdObj?.ToString() ?? throw new InvalidOperationException("Post ID not returned");

        throw new InvalidOperationException("Failed to create image post");
    }
}
