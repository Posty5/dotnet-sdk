using System.Text.Json;
using System.Text.Json.Serialization;

namespace Posty5.SocialPublisherPost.Models;

// ============================================================================
// RESULTS OF CREATING AND REMOVING POSTS (Posty5.SocialPublisherPost 4.6.0+)
// ============================================================================

/// <summary>
/// What the API answers when a text post or a story is created.
/// </summary>
public class CreatePostResult
{
    /// <summary>The new post's id — pass it to <c>GetStatusAsync</c>.</summary>
    [JsonPropertyName("_id")]
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Connected platforms left out because they cannot take this kind of post
    /// (e.g. YouTube, Instagram and TikTok for text). Null when none were.
    /// </summary>
    public List<string>? SkippedPlatforms { get; set; }

    /// <summary>
    /// Connected platforms dropped from this post, with why — X on a plan below
    /// Pro, or when this month's X quota is used up. Null when none were.
    /// </summary>
    public List<LongVideoRefusedTarget>? RefusedTargets { get; set; }

    /// <summary>
    /// Platforms whose text was built from the shared caption and cut to fit
    /// (X only). Send <see cref="TwitterConfig.Text"/> to avoid it.
    /// </summary>
    public List<string>? TruncatedTargets { get; set; }

    /// <summary>
    /// Per platform, the hashtags the API merged into the text, as it reports
    /// them. Present only when hashtags were requested.
    /// </summary>
    public Dictionary<string, JsonElement>? Hashtags { get; set; }
}

/// <summary>
/// What the API answers when a published post is removed from the platforms.
/// </summary>
public class RemovePostResult : Posty5.Core.Models.IVersioned
{
    /// <summary>The document's version (<c>__v</c>); pass it to the next update or delete.</summary>
    [JsonPropertyName("__v")]
    public long Version { get; set; }

    /// <summary>The post's id.</summary>
    [JsonPropertyName("_id")]
    public string Id { get; set; } = string.Empty;

    /// <summary>The outcome on each platform the post was published to, keyed by platform.</summary>
    public Dictionary<string, PlatformRemovalResult> Results { get; set; } = new();
}

/// <summary>The outcome of removing a post from one platform.</summary>
public class PlatformRemovalResult
{
    /// <summary>True when the media is gone from the platform (or there was nothing to delete).</summary>
    public bool Success { get; set; }

    /// <summary>True when the platform's API offers no way to delete a post (Instagram, TikTok).</summary>
    public bool? NotSupported { get; set; }

    /// <summary>True when there was no published media to delete.</summary>
    public bool? Skipped { get; set; }

    /// <summary>Why the removal failed, when it did.</summary>
    public string? Error { get; set; }
}
