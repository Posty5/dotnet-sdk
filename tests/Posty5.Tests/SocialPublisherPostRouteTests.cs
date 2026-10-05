using System.Net;
using System.Text.Json;
using Posty5.Core.Configuration;
using Posty5.Core.Exceptions;
using Posty5.SocialPublisherPost;
using Posty5.SocialPublisherPost.Models;
using Xunit;

namespace Posty5.Tests.Unit;

/// <summary>
/// Offline: the post routes added or fixed in 4.6.0 — status, remove, text and
/// story — and the comment list the short-video and image payloads used to
/// drop, pinned against a <see cref="RecordingServer"/>.
/// </summary>
public class SocialPublisherPostRouteTests : IDisposable
{
    private const string Base = "/api/social-publisher-post";
    private readonly RecordingServer _server = new();

    public void Dispose() => _server.Dispose();

    private SocialPublisherPostClient Client(Posty5Options? options = null) => new(_server.Http(options));

    private (string Route, JsonElement Body) Last()
    {
        var request = _server.Requests.Last();
        using var json = JsonDocument.Parse(string.IsNullOrEmpty(request.Body) ? "{}" : request.Body);
        return ($"{request.Method} {request.PathAndQuery}", json.RootElement.Clone());
    }

    // ─── Reschedule (fixed): the edit route takes the schedule flat ─────────

    [Fact]
    public async Task ReschedulePostAsync_SendsScheduleTypeAndScheduledAtFlat()
    {
        await Client().ReschedulePostAsync("p1", new DateTime(2026, 9, 20, 8, 0, 0, DateTimeKind.Utc), "New caption");

        var (route, body) = Last();
        Assert.Equal($"PUT {Base}/p1", route);
        Assert.Equal("schedule", body.GetProperty("scheduleType").GetString());
        Assert.Equal(new DateTime(2026, 9, 20, 8, 0, 0, DateTimeKind.Utc), body.GetProperty("scheduledAt").GetDateTime().ToUniversalTime());
        Assert.Equal("New caption", body.GetProperty("caption").GetString());
        Assert.False(body.TryGetProperty("schedule", out _));
    }

    [Fact]
    public async Task ReschedulePostAsync_Now_SendsNoScheduledAt()
    {
        await Client().ReschedulePostAsync("p1", "now");

        var (_, body) = Last();
        Assert.Equal("now", body.GetProperty("scheduleType").GetString());
        Assert.False(body.TryGetProperty("scheduledAt", out _));
        Assert.False(body.TryGetProperty("caption", out _));
    }

    // ─── Status (fixed) and remove (new) ────────────────────────────────────

    [Fact]
    public async Task GetStatusAsync_CallsTheStatusRoute_NotTheBareId()
    {
        _server.ResultJson = """
        { "_id": "p1", "numbering": "0042", "type": "story", "source": "image-url", "currentStatus": "removed",
          "currentStatusChangedAt": "2026-10-05T09:00:00Z", "statusHistoryGrouped": [],
          "createdAt": "2026-10-04T09:00:00Z", "schedule": { "type": "now", "scheduledAt": null, "executedAt": "2026-10-04T09:01:00Z" },
          "storyExpiresAt": "2026-10-05T09:01:00Z", "createdFrom": "mcp",
          "agentOrigin": { "channel": "mcp", "trust": "hosted", "client": { "name": "claude-code", "version": "2.1.0" },
                           "clientLabel": "Claude Code", "provider": "anthropic", "model": "claude-opus-5-5", "modelSource": "agent",
                           "tool": "social_post_publish_story", "callId": "c1", "recordedAt": "2026-10-04T09:00:00Z" } }
        """;

        var status = await Client().GetStatusAsync("p1");

        Assert.Equal($"GET {Base}/p1/status", Last().Route);
        Assert.Equal(SocialPublisherPostStatusType.Removed, status.CurrentStatus);
        Assert.Equal("mcp", status.CreatedFrom);
        Assert.Equal("claude-opus-5-5", status.AgentOrigin!.Model);
        Assert.Equal("claude-code", status.AgentOrigin.Client!.Name);
        Assert.NotNull(status.StoryExpiresAt);
    }

    [Theory]
    [InlineData("removing")]
    [InlineData("removed")]
    [InlineData("removeFailed")]
    public void TheRemovalStatuses_Deserialize(string value)
    {
        var parsed = JsonSerializer.Deserialize<SocialPublisherPostStatusType>($"\"{value}\"");
        Assert.Equal(value, parsed.Value);
    }

    [Fact]
    public async Task GetStatusAsync_RefusesAnEmptyId_BeforeAnyRequest()
    {
        await Assert.ThrowsAsync<ArgumentException>(() => Client().GetStatusAsync(""));
        Assert.Empty(_server.Requests);
    }

    [Fact]
    public async Task RemovePostAsync_PostsToRemove_AndReadsEachPlatform()
    {
        _server.ResultJson = """
        { "_id": "p1", "results": {
            "facebook": { "success": true },
            "instagram": { "success": false, "notSupported": true, "error": "Instagram has no delete API" } } }
        """;

        var result = await Client().RemovePostAsync("p1");

        var (route, body) = Last();
        Assert.Equal($"POST {Base}/p1/remove", route);
        Assert.Equal(JsonValueKind.Object, body.ValueKind);
        Assert.Empty(body.EnumerateObject());
        Assert.Equal("p1", result.Id);
        Assert.True(result.Results["facebook"].Success);
        Assert.True(result.Results["instagram"].NotSupported);
    }

    [Fact]
    public async Task RemovePostAsync_AFailedRemoval_Throws()
    {
        _server.Status = HttpStatusCode.BadRequest;
        await Assert.ThrowsAsync<Posty5ValidationException>(() => Client().RemovePostAsync("p1"));
    }

    // ─── Text posts ─────────────────────────────────────────────────────────

    [Fact]
    public async Task CreateTextPostToWorkspaceAsync_SendsEveryFieldTheApiReads()
    {
        _server.ResultJson = """
        { "_id": "p2", "skippedPlatforms": ["youtube", "instagram"],
          "refusedTargets": [ { "platform": "twitter", "reason": "Pro plan required" } ], "truncatedTargets": ["twitter"],
          "hashtags": { "facebook": { "added": ["#menu"] } } }
        """;

        var result = await Client().CreateTextPostToWorkspaceAsync(new CreateTextPostToWorkspaceRequest
        {
            WorkspaceId = "w1",
            Caption = "Our autumn menu is out.",
            Facebook = new TextPostFacebookConfig { Description = "Menu:", Link = "https://example.com/menu" },
            Threads = new ThreadsTextConfig { Text = "Menu is out", ReplyControl = "everyone", TopicTag = "food" },
            Twitter = new TwitterConfig
            {
                Text = "Which dish?",
                ReplySettings = "following",
                Poll = new TwitterPollConfig { Options = new() { "Soup", "Pie" }, DurationMinutes = 60 },
            },
            Schedule = new ScheduleConfig { Type = "now" },
            Comments = new() { new CommentRequest { Text = "Book a table", PostToThreads = false, PostToTwitter = true } },
            HashtagGroupIds = new() { "0123456789abcdef01234567" },
            Hashtags = new() { "menu" },
            TrackLinks = true,
            Utm = new TrackedLinkUtm { Medium = "social", Campaign = "autumn" },
            Tag = "menu",
            RefId = "r1",
        });

        var (route, body) = Last();
        Assert.Equal($"POST {Base}/text/workspace", route);
        Assert.Equal("w1", body.GetProperty("workspaceId").GetString());
        Assert.Equal("Our autumn menu is out.", body.GetProperty("caption").GetString());
        Assert.Equal("https://example.com/menu", body.GetProperty("facebook").GetProperty("link").GetString());
        Assert.Equal("everyone", body.GetProperty("threads").GetProperty("reply_control").GetString());
        Assert.Equal("food", body.GetProperty("threads").GetProperty("topic_tag").GetString());
        var twitter = body.GetProperty("twitter");
        Assert.Equal("following", twitter.GetProperty("reply_settings").GetString());
        Assert.Equal(60, twitter.GetProperty("poll").GetProperty("duration_minutes").GetInt32());
        Assert.Equal(2, twitter.GetProperty("poll").GetProperty("options").GetArrayLength());
        Assert.Equal("now", body.GetProperty("schedule").GetProperty("type").GetString());
        var comment = body.GetProperty("comments")[0];
        Assert.False(comment.GetProperty("postToThreads").GetBoolean());
        Assert.True(comment.GetProperty("postToTwitter").GetBoolean());
        Assert.Equal(1, body.GetProperty("hashtagGroupIds").GetArrayLength());
        Assert.Equal("menu", body.GetProperty("hashtags")[0].GetString());
        Assert.True(body.GetProperty("trackLinks").GetBoolean());
        Assert.Equal("autumn", body.GetProperty("utm").GetProperty("campaign").GetString());
        Assert.Equal("menu", body.GetProperty("tag").GetString());
        Assert.Equal("r1", body.GetProperty("refId").GetString());
        Assert.Equal("dotnetPackage", body.GetProperty("createdFrom").GetString());
        Assert.False(body.TryGetProperty("comment", out _));

        Assert.Equal("p2", result.Id);
        Assert.Equal(new[] { "youtube", "instagram" }, result.SkippedPlatforms);
        Assert.Equal("twitter", Assert.Single(result.RefusedTargets!).Platform);
        Assert.Equal(new[] { "twitter" }, result.TruncatedTargets);
        Assert.True(result.Hashtags!.ContainsKey("facebook"));
    }

    [Fact]
    public async Task CreateTextPostToWorkspaceAsync_SendsOnlyWhatWasSet()
    {
        _server.ResultJson = "{\"_id\":\"p2\"}";

        await Client().CreateTextPostToWorkspaceAsync(new CreateTextPostToWorkspaceRequest { WorkspaceId = "w1", Caption = "Hi" });

        var names = Last().Body.EnumerateObject().Select(p => p.Name).OrderBy(n => n);
        Assert.Equal(new[] { "caption", "createdFrom", "workspaceId" }, names);
    }

    [Fact]
    public async Task CreateTextPostToAccountAsync_TakesAReservedId_AndARequestLabel()
    {
        _server.ResultJson = "{\"_id\":\"p9\"}";
        var client = Client(new Posty5Options { ApiKey = "k", CreatedFrom = "my-crm" });

        await client.CreateTextPostToAccountAsync(new CreateTextPostToAccountRequest { AccountId = "a1", Caption = "Hi" });
        await client.CreateTextPostToAccountAsync(new CreateTextPostToAccountRequest { AccountId = "a1", Caption = "Hi", CreatedFrom = "campaign-7" }, id: "p9");
        await client.CreateTextPostToWorkspaceAsync(new CreateTextPostToWorkspaceRequest { WorkspaceId = "w1", Caption = "Hi" }, id: "p8");

        Assert.Equal(new[]
        {
            $"POST {Base}/text/account",
            $"POST {Base}/text/account/p9",
            $"POST {Base}/text/workspace/p8",
        }, _server.Requests.Select(r => $"{r.Method} {r.PathAndQuery}"));

        var labels = _server.Requests.Select(r =>
        {
            using var json = JsonDocument.Parse(r.Body);
            return json.RootElement.GetProperty("createdFrom").GetString();
        });
        Assert.Equal(new[] { "my-crm", "campaign-7", "my-crm" }, labels);

        using var first = JsonDocument.Parse(_server.Requests[0].Body);
        Assert.Equal("a1", first.RootElement.GetProperty("accountId").GetString());
        Assert.False(first.RootElement.TryGetProperty("workspaceId", out _));
    }

    [Fact]
    public async Task CreateTextPost_WithoutTargetOrCaption_FailsBeforeAnyRequest()
    {
        var client = Client();

        await Assert.ThrowsAsync<ArgumentException>(() => client.CreateTextPostToWorkspaceAsync(new CreateTextPostToWorkspaceRequest { Caption = "Hi" }));
        await Assert.ThrowsAsync<ArgumentException>(() => client.CreateTextPostToWorkspaceAsync(new CreateTextPostToWorkspaceRequest { WorkspaceId = "w1" }));
        await Assert.ThrowsAsync<ArgumentException>(() => client.CreateTextPostToAccountAsync(new CreateTextPostToAccountRequest { Caption = "Hi" }));
        await Assert.ThrowsAsync<ArgumentNullException>(() => client.CreateTextPostToAccountAsync(null!));

        Assert.Empty(_server.Requests);
    }

    // ─── Stories ────────────────────────────────────────────────────────────

    [Fact]
    public async Task CreateStoryPostToWorkspaceAsync_ImageStory_SendsOnlyTheImageFields()
    {
        _server.ResultJson = "{\"_id\":\"p3\",\"skippedPlatforms\":[\"youtube\"]}";

        var result = await Client().CreateStoryPostToWorkspaceAsync(new CreateStoryPostToWorkspaceRequest
        {
            WorkspaceId = "w1",
            Kind = StoryKinds.Image,
            Image = new ImageRequest { Source = ImageSource.ImageUrl, ExternalUrl = "https://cdn.example.com/s.jpg" },
            Tag = "launch",
        });

        var (route, body) = Last();
        Assert.Equal($"POST {Base}/story/workspace", route);
        Assert.Equal("image", body.GetProperty("kind").GetString());
        Assert.Equal("image-url", body.GetProperty("image").GetProperty("source").GetString());
        Assert.Equal("https://cdn.example.com/s.jpg", body.GetProperty("image").GetProperty("externalUrl").GetString());
        Assert.False(body.GetProperty("image").TryGetProperty("bucketKey", out _));
        // The story schema refuses unknown keys, and these are forbidden on an image story.
        Assert.False(body.TryGetProperty("source", out _));
        Assert.False(body.TryGetProperty("videoURL", out _));
        Assert.False(body.TryGetProperty("caption", out _));
        Assert.Equal(new[] { "createdFrom", "image", "kind", "tag", "workspaceId" }, body.EnumerateObject().Select(p => p.Name).OrderBy(n => n));

        Assert.Equal("p3", result.Id);
        Assert.Equal(new[] { "youtube" }, result.SkippedPlatforms);
    }

    [Fact]
    public async Task CreateStoryPostToAccountAsync_VideoStory_SendsAUrlSourceAndNoImage()
    {
        _server.ResultJson = "{\"_id\":\"p4\"}";

        await Client().CreateStoryPostToAccountAsync(new CreateStoryPostToAccountRequest
        {
            AccountId = "a1",
            Kind = StoryKinds.Video,
            VideoURL = "https://cdn.example.com/s.mp4",
            // Set by mistake; a video story must not carry it.
            Image = new ImageRequest { Source = ImageSource.ImageUrl, ExternalUrl = "https://cdn.example.com/s.jpg" },
            Schedule = new ScheduleConfig { Type = "schedule", ScheduledAt = new DateTime(2026, 12, 1, 9, 0, 0, DateTimeKind.Utc) },
        });

        var (route, body) = Last();
        Assert.Equal($"POST {Base}/story/account", route);
        Assert.Equal("a1", body.GetProperty("accountId").GetString());
        Assert.Equal("video", body.GetProperty("kind").GetString());
        Assert.Equal("video-url", body.GetProperty("source").GetString());
        Assert.Equal("https://cdn.example.com/s.mp4", body.GetProperty("videoURL").GetString());
        Assert.Equal("schedule", body.GetProperty("schedule").GetProperty("type").GetString());
        Assert.False(body.TryGetProperty("image", out _));
        Assert.Equal("dotnetPackage", body.GetProperty("createdFrom").GetString());
    }

    [Fact]
    public async Task CreateStoryPost_WithoutItsMedia_FailsBeforeAnyRequest()
    {
        var client = Client();

        await Assert.ThrowsAsync<ArgumentException>(() => client.CreateStoryPostToWorkspaceAsync(new CreateStoryPostToWorkspaceRequest { WorkspaceId = "w1", Kind = StoryKinds.Video }));
        await Assert.ThrowsAsync<ArgumentException>(() => client.CreateStoryPostToWorkspaceAsync(new CreateStoryPostToWorkspaceRequest { WorkspaceId = "w1", Kind = StoryKinds.Image }));
        await Assert.ThrowsAsync<ArgumentException>(() => client.CreateStoryPostToAccountAsync(new CreateStoryPostToAccountRequest { AccountId = "a1", Kind = "carousel", VideoURL = "https://x" }));
        await Assert.ThrowsAsync<ArgumentException>(() => client.CreateStoryPostToAccountAsync(new CreateStoryPostToAccountRequest { Kind = StoryKinds.Video, VideoURL = "https://x" }));

        Assert.Empty(_server.Requests);
    }

    // ─── The comment list reaches the API (fixed in 4.6.0) ──────────────────

    [Fact]
    public async Task PublishShortVideoByUrl_SendsTheCommentList()
    {
        _server.ResultJson = "{\"_id\":\"p5\"}";

        await Client().PublishShortVideoToWorkspaceAsync(
            workspaceId: "w1",
            video: "https://cdn.example.com/v.mp4",
            thumbnail: "https://cdn.example.com/t.jpg",
            youtube: new YouTubeConfig { Title = "t", Description = "d" },
            comments: new() { new CommentRequest { Text = "First" }, new CommentRequest { Text = "Second", DelayMinutes = 60 } });

        var (route, body) = Last();
        Assert.Equal($"POST {Base}/short-video/workspace/by-url", route);
        Assert.Equal(2, body.GetProperty("comments").GetArrayLength());
        Assert.Equal(60, body.GetProperty("comments")[1].GetProperty("delayMinutes").GetInt32());
        Assert.False(body.TryGetProperty("comment", out _));
    }

    [Fact]
    public async Task CreateImagePost_SendsTheCommentList()
    {
        _server.ResultJson = "{\"_id\":\"p6\"}";

        await Client().CreateImagePostToAccountAsync(new CreateImagePostToAccountRequest
        {
            AccountId = "a1",
            Caption = "c",
            Image = new ImageRequest { Source = ImageSource.ImageUrl, ExternalUrl = "https://cdn.example.com/i.jpg" },
            Comments = new() { new CommentRequest { Text = "First" } },
        });

        var (route, body) = Last();
        Assert.Equal($"POST {Base}/image/account", route);
        Assert.Equal("First", body.GetProperty("comments")[0].GetProperty("text").GetString());
    }
}
