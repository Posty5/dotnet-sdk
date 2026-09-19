using System.Text.Json;
using Xunit;
using Posty5.SocialPublisherPost.Models;

namespace Posty5.Tests.Unit;

/// <summary>
/// The multi-comment shapes.
///
/// Everything this SDK contributes to the feature is a shape and a document, so
/// what is worth pinning is that the shape serializes the way the API reads it,
/// that the deprecated singular still compiles, and that the limits say what the
/// server says. No API is needed and none is reached.
/// </summary>
public class PostCommentsTests
{
    private static readonly JsonSerializerOptions Json = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
    };

    [Fact]
    public void A_post_carries_a_list_of_comments()
    {
        var settings = new PostSettings
        {
            WorkspaceId = "w1",
            Comments = new List<CommentRequest>
            {
                new() { Text = "First, straight away." },
                new() { Text = "Second, an hour later.", DelayMinutes = 60 },
            },
        };

        Assert.Equal(2, settings.Comments!.Count);
        Assert.Equal(60, settings.Comments[1].DelayMinutes);
    }

    [Fact]
    public void The_deprecated_singular_still_compiles()
    {
        // Obsolete, not removed: one major version of overlap, so nobody's
        // build breaks on the upgrade.
#pragma warning disable CS0618
        var settings = new PostSettings { WorkspaceId = "w1", Comment = new CommentRequest { Text = "The old way." } };

        Assert.Equal("The old way.", settings.Comment!.Text);
#pragma warning restore CS0618
    }

    [Fact]
    public void A_comment_serializes_the_fields_the_api_declares()
    {
        var comment = new CommentRequest
        {
            Text = "Nice one.",
            DelayMinutes = 30,
            ImageUrl = "https://cdn.example.com/a.jpg",
            PostToFacebook = true,
            PostToInstagram = false,
        };

        var json = JsonSerializer.Serialize(comment, Json);

        // camelCase, because Joi refuses an undeclared key outright rather than
        // ignoring it — a PascalCase field would be a 400, not a dropped option.
        Assert.Contains("\"text\":\"Nice one.\"", json);
        Assert.Contains("\"delayMinutes\":30", json);
        Assert.Contains("\"imageUrl\":\"https://cdn.example.com/a.jpg\"", json);
        Assert.Contains("\"postToFacebook\":true", json);
        Assert.Contains("\"postToInstagram\":false", json);
    }

    [Fact]
    public void An_unset_field_is_omitted_rather_than_sent_as_null()
    {
        var json = JsonSerializer.Serialize(new CommentRequest { Text = "Just text." }, Json);

        Assert.DoesNotContain("delayMinutes", json);
        Assert.DoesNotContain("imageUrl", json);
        Assert.DoesNotContain("imageStorageKey", json);
    }

    /// <summary>
    /// The two image fields are not interchangeable: the KEY is what tells the
    /// API the object is ours, and therefore what its cleanup path uses when the
    /// post is deleted. A public URL sent in the key field makes the image read
    /// as external and it is never cleaned up.
    /// </summary>
    [Fact]
    public void An_image_is_either_a_url_or_an_uploaded_key()
    {
        var byUrl = new CommentRequest { Text = "a", ImageUrl = "https://cdn.example.com/a.jpg" };
        var byKey = new CommentRequest { Text = "a", ImageStorageKey = "users/u1/accounts/a1/a.jpg" };

        Assert.Null(byUrl.ImageStorageKey);
        Assert.Null(byKey.ImageUrl);
    }

    [Fact]
    public void The_limits_match_the_server()
    {
        Assert.Equal(5, CommentLimits.MaxPerPost);
        Assert.Equal(2200, CommentLimits.MaxLength);
        Assert.Equal(24 * 60, CommentLimits.MaxDelayMinutes);
    }

    [Fact]
    public void A_status_carries_one_entry_per_comment_in_posting_order()
    {
        var facebook = new FacebookFullDetailsConfig
        {
            Comments = new List<CommentStatusInfo>
            {
                new() { Order = 0, Text = "First", CurrentStatus = CommentStatus.Done },
                new() { Order = 1, Text = "Second", DelayMinutes = 60, CurrentStatus = CommentStatus.Pending },
            },
        };

        Assert.Equal(new[] { 0, 1 }, facebook.Comments!.Select(c => c.Order));
        Assert.Equal(CommentStatus.Done, facebook.Comments[0].CurrentStatus);
    }

    // TikTok exposes no public comment-posting endpoint, so this is not a
    // failure state — it is the permanent answer.
    [Fact]
    public void Every_tiktok_comment_is_not_supported()
    {
        var tiktok = new TikTokFullDetailsConfig
        {
            Comments = new List<CommentStatusInfo>
            {
                new() { Order = 0, CurrentStatus = CommentStatus.NotSupported },
                new() { Order = 1, CurrentStatus = CommentStatus.NotSupported },
            },
        };

        Assert.All(tiktok.Comments!, c => Assert.Equal(CommentStatus.NotSupported, c.CurrentStatus));
    }

    [Fact]
    public void A_status_carries_the_origin_of_a_copied_default()
    {
        var status = new CommentStatusInfo
        {
            Order = 0,
            Text = "Thanks for watching!",
            DefaultCommentId = "d1",
            CurrentStatus = CommentStatus.Done,
        };

        Assert.Equal("d1", status.DefaultCommentId);
    }
}
