using Xunit;
using Posty5.SocialPublisherPost;
using Posty5.SocialPublisherPost.Models;
using Posty5.Core.Models;

namespace Posty5.Tests.Integration;

/// <summary>
/// Tests for Social Publisher Post SDK
/// Based on TypeScript test file: social-publisher-post.test.ts
/// Uses unified PublishShortVideoToWorkspaceAsync() method with auto-detection
/// </summary>
[Collection("Sequential")]
public class SocialPublisherPostClientTests : IDisposable
{
    private readonly SocialPublisherPostClient _client;
    private readonly string _testVideoPath;
    private readonly string _testThumbnailPath;
    private readonly string _workspaceId;
    private readonly string _youtubeAccountId;
    private readonly string _tiktokAccountId;
    private string? _createdPostId;

    // Test URLs from TypeScript tests
    private const string ThumbnailURL = "https://images.unsplash.com/3/GoWildImages_MtEverest_NEP0555.jpg";
    private const string VideoURL = "https://commondatastorage.googleapis.com/gtv-videos-bucket/sample/ForBiggerEscapes.mp4";
    private const string FacebookReelURL = "https://www.facebook.com/reel/1794235308045414";
    private const string YouTubeShortsURL = "https://www.youtube.com/shorts/jkiHUTnDJnk";
    private const string TikTokVideoURL = "https://www.tiktok.com/@tamra.ai/video/7592228093834841362";

    public SocialPublisherPostClientTests()
    {
        var httpClient = TestConfig.CreateHttpClient();
        _client = new SocialPublisherPostClient(httpClient);
        _testVideoPath = Path.Combine("Assets", "video.mp4");
        _testThumbnailPath = Path.Combine("Assets", "thumb.jpg");
        
        // TODO: Replace with actual workspace ID from setup
        _workspaceId = "69922068aa6ee6fa8eb8f9c2";
        _youtubeAccountId = "69921cc7aa6ee6fa8eb8f8a9";
        _tiktokAccountId = "69921c96aa6ee6fa8eb8f88f";
    }

    #region CREATE - Video File Upload Tests

    [Fact]
    public async Post PublishShortVideo_VideoFileWithThumbnailURL_ShouldSucceed()
    {
        // Arrange
        using var videoStream = File.OpenRead(_testVideoPath);

        // Act - Using unified PublishShortVideoToWorkspaceAsync
        var postId = await _client.PublishShortVideoToWorkspaceAsync(
            workspaceId: _workspaceId,
            video: videoStream,
            thumbnail: ThumbnailURL,
            platforms: new List<string> { "youtube" },
            youtube: new YouTubeConfig
            {
                Title = $"Video File + Thumb URL - {DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}",
                Description = "Testing video file with thumbnail URL",
                Tags = new List<string> { "test", "sdk" }
            },
            videoContentType: "video/mp4"
        );

        // Assert
        Assert.NotNull(postId);
        Assert.NotEmpty(postId);
        _createdPostId = postId;
        TestConfig.CreatedResources.Posts.Add(postId);
    }

    [Fact]
    public async Post PublishShortVideo_VideoFileWithThumbnailFile_ShouldSucceed()
    {
        // Arrange
        using var videoStream = File.OpenRead(_testVideoPath);
        using var thumbStream = File.OpenRead(_testThumbnailPath);

        // Act - Auto-detects file upload
        var postId = await _client.PublishShortVideoToWorkspaceAsync(
            workspaceId: _workspaceId,
            video: videoStream,
            thumbnail: thumbStream,
            platforms: new List<string> { "tiktok" },
            tiktok: new TikTokConfig
            {
                Caption = $"Video File + Thumb File - {DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}",
                PrivacyLevel = "SELF_ONLY",
                DisableDuet = false,
                DisableStitch = false,
                DisableComment = false
            },
            videoContentType: "video/mp4",
            thumbnailContentType: "image/jpeg"
        );

        // Assert
        Assert.NotNull(postId);
        TestConfig.CreatedResources.Posts.Add(postId);
    }

    #endregion

    #region CREATE - Video URL Tests

    [Fact]
    public async Post PublishShortVideo_VideoURLWithThumbnailFile_ShouldSucceed()
    {
        // Arrange
        using var thumbStream = File.OpenRead(_testThumbnailPath);

        // Act - Auto-detects URL
        var postId = await _client.PublishShortVideoToWorkspaceAsync(
            workspaceId: _workspaceId,
            video: VideoURL,
            thumbnail: thumbStream,
            platforms: new List<string> { "youtube" },
            youtube: new YouTubeConfig
            {
                Title = $"Video URL + Thumb File - {DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}",
                Description = "Testing video URL with thumbnail file",
                Tags = new List<string> { "test", "url" }
            },
            thumbnailContentType: "image/jpeg"
        );

        // Assert
        Assert.NotNull(postId);
        TestConfig.CreatedResources.Posts.Add(postId);
    }

    [Fact]
    public async Post PublishShortVideo_VideoURLWithThumbnailURL_MultiPlatform_ShouldSucceed()
    {
        // Act - Multi-platform publishing
        var postId = await _client.PublishShortVideoToWorkspaceAsync(
            workspaceId: _workspaceId,
            video: VideoURL,
            thumbnail: ThumbnailURL,
            platforms: new List<string> { "youtube", "tiktok" },
            youtube: new YouTubeConfig
            {
                Title = $"Video URL + Thumb URL - {DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}",
                Description = "Testing video URL with thumbnail URL",
                Tags = new List<string> { "test", "multi-platform" }
            },
            tiktok: new TikTokConfig
            {
                Caption = "Multi-platform test",
                PrivacyLevel = "SELF_ONLY",
                DisableDuet = false,
                DisableStitch = false,
                DisableComment = false
            }
        );

        // Assert
        Assert.NotNull(postId);
        TestConfig.CreatedResources.Posts.Add(postId);
    }

    #endregion

    #region CREATE - Repost Tests (Auto-Detection)

    [Fact]
    public async Post PublishShortVideo_FacebookReelURL_ShouldAutoDetectAndRepost()
    {
        // Act - Auto-detects Facebook repost
        var postId = await _client.PublishShortVideoToWorkspaceAsync(
            workspaceId: _workspaceId,
            video: FacebookReelURL,
            platforms: new List<string> { "youtube" },
            youtube: new YouTubeConfig
            {
                Title = $"Reposted from Facebook - {DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}",
                Description = "Testing Facebook repost with auto-detection",
                Tags = new List<string> { "repost", "facebook" }
            }
        );

        // Assert
        Assert.NotNull(postId);
        TestConfig.CreatedResources.Posts.Add(postId);
    }

    [Fact]
    public async Post PublishShortVideo_YouTubeShortsURL_ShouldAutoDetectAndRepost()
    {
        // Act - Auto-detects YouTube Shorts repost
        var postId = await _client.PublishShortVideoToWorkspaceAsync(
            workspaceId: _workspaceId,
            video: YouTubeShortsURL,
            platforms: new List<string> { "tiktok" },
            tiktok: new TikTokConfig
            {
                Caption = "Reposted from YouTube Shorts",
                PrivacyLevel = "SELF_ONLY",
                DisableDuet = false,
                DisableStitch = false,
                DisableComment = false
            }
        );

        // Assert
        Assert.NotNull(postId);
        TestConfig.CreatedResources.Posts.Add(postId);
    }

    [Fact]
    public async Post PublishShortVideo_TikTokVideoURL_ShouldAutoDetectAndRepost()
    {
        // Act - Auto-detects TikTok repost
        var postId = await _client.PublishShortVideoToWorkspaceAsync(
            workspaceId: _workspaceId,
            video: TikTokVideoURL,
            platforms: new List<string> { "youtube" },
            youtube: new YouTubeConfig
            {
                Title = $"Reposted from TikTok - {DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}",
                Description = "Testing TikTok repost with auto-detection",
                Tags = new List<string> { "repost", "tiktok" }
            }
        );

        // Assert
        Assert.NotNull(postId);
        TestConfig.CreatedResources.Posts.Add(postId);
    }

    #endregion

    #region CREATE - Account Tests

    [Fact]
    public async Post PublishShortVideoToAccount_VideoFile_ShouldSucceed()
    {
        // Arrange
        using var videoStream = File.OpenRead(_testVideoPath);
        using var thumbStream = File.OpenRead(_testThumbnailPath);

        // Act
        var postId = await _client.PublishShortVideoToAccountAsync(
            accountId: _tiktokAccountId,
            video: videoStream,
            thumbnail: thumbStream,
            platforms: new List<string> { "tiktok" },
            tiktok: new TikTokConfig
            {
                Caption = $"Account Video File - {DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}",
                PrivacyLevel = "SELF_ONLY"
            },
            videoContentType: "video/mp4",
            thumbnailContentType: "image/jpeg"
        );

        // Assert
        Assert.NotNull(postId);
        TestConfig.CreatedResources.Posts.Add(postId);
    }

    [Fact]
    public async Post PublishShortVideoToAccount_VideoURL_ShouldSucceed()
    {
        // Arrange
        using var thumbStream = File.OpenRead(_testThumbnailPath);

        // Act
        var postId = await _client.PublishShortVideoToAccountAsync(
            accountId: _youtubeAccountId,
            video: VideoURL,
            thumbnail: thumbStream,
            platforms: new List<string> { "youtube" },
            youtube: new YouTubeConfig
            {
                Title = $"Account Video URL - {DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}",
                Description = "Testing account publishing via URL",
                Tags = new List<string> { "test", "account" }
            },
            thumbnailContentType: "image/jpeg"
        );

        // Assert
        Assert.NotNull(postId);
        TestConfig.CreatedResources.Posts.Add(postId);
    }

    [Fact]
    public async Post PublishShortVideoToAccount_Repost_ShouldSucceed()
    {
        // Act
        var postId = await _client.PublishShortVideoToAccountAsync(
            accountId: _tiktokAccountId,
            video: YouTubeShortsURL,
            platforms: new List<string> { "tiktok" },
            tiktok: new TikTokConfig
            {
                Caption = "Account Repost Test",
                PrivacyLevel = "SELF_ONLY"
            }
        );

        // Assert
        Assert.NotNull(postId);
        TestConfig.CreatedResources.Posts.Add(postId);
    }

    #endregion

    #region Utility Method Tests

    [Fact]
    public async Post GetDefaultSettings_ShouldReturnSettings()
    {
        // Act
        var result = await _client.GetDefaultSettingsAsync();

        // Assert
        Assert.NotNull(result);
    }

    [Fact]
    public async Post GetStatus_WithValidId_ShouldReturnStatus()
    {
        // Arrange
        if (_createdPostId == null)
        {
            // Skip if no post created
            return;
        }

        // Act
        var result = await _client.GetStatusAsync(_createdPostId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(_createdPostId, result.Id);
    }

    [Fact]
    public async Post GetNextAndPrevious_WithValidId_ShouldReturnNavigation()
    {
        // Arrange
        if (_createdPostId == null)
        {
            return;
        }

        // Act
        var result = await _client.GetNextAndPreviousAsync(_createdPostId);

        // Assert
        Assert.NotNull(result);
        // NextId and PreviousId may be null if this is the only/first/last post
    }

    #endregion

    #region List and Filter Tests

    [Fact]
    public async Post List_WithPagination_ShouldReturnResults()
    {
        // Act
        var result = await _client.ListAsync(
            new ListPostsParams(),
            new PaginationParams { PageSize = 10 }
        );

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Items);
        Assert.True(result.Items.Count <= 10);
    }

    [Fact]
    public async Post List_FilterByWorkspace_ShouldReturnFilteredResults()
    {
        // Arrange
        var filterParams = new ListPostsParams
        {
            WorkspaceId = _workspaceId
        };

        // Act
        var result = await _client.ListAsync(
            filterParams,
            new PaginationParams { PageSize = 10 }
        );

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Items);
    }

    [Fact]
    public async Post List_FilterByStatus_ShouldReturnFilteredResults()
    {
        // Arrange
        var filterParams = new ListPostsParams
        {
            CurrentStatus = SocialPublisherPostStatusType.Pending
        };

        // Act
        var result = await _client.ListAsync(
            filterParams,
            new PaginationParams { PageSize = 10 }
        );

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Items);
    }

    #endregion

    #region Advanced Tests

    [Fact]
    public async Post PublishShortVideo_WithScheduling_ShouldSucceed()
    {
        // Arrange
        var scheduleDate = DateTime.UtcNow.AddHours(2);

        // Act - Schedule for future
        var postId = await _client.PublishShortVideoToWorkspaceAsync(
            workspaceId: _workspaceId,
            video: VideoURL,
            platforms: new List<string> { "youtube" },
            youtube: new YouTubeConfig
            {
                Title = "Scheduled Video Test",
                Description = "Testing scheduled publishing",
                Tags = new List<string> { "test", "scheduled" }
            },
            schedule: scheduleDate
        );

        // Assert
        Assert.NotNull(postId);
        TestConfig.CreatedResources.Posts.Add(postId);
    }

    [Fact]
    public async Post PublishShortVideo_WithTagAndRefId_ShouldSucceed()
    {
        // Act
        var postId = await _client.PublishShortVideoToWorkspaceAsync(
            workspaceId: _workspaceId,
            video: VideoURL,
            platforms: new List<string> { "youtube" },
            youtube: new YouTubeConfig
            {
                Title = "Tagged Video Test",
                Description = "Testing with metadata",
                Tags = new List<string> { "test" }
            },
            tag: "campaign-2024",
            refId: "ext-ref-12345"
        );

        // Assert
        Assert.NotNull(postId);
        TestConfig.CreatedResources.Posts.Add(postId);
    }

    #endregion

    public void Dispose()
    {
        // Cleanup is handled by test framework
    }
}

