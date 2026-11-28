using System.Text.Json.Serialization;

namespace Posty5.SocialPublisherWorkspace.Models;

/// <summary>
/// Post types a default comment may be limited to. Stories take no comments.
/// </summary>
public static class CommentPostTypes
{
    /// <summary>A short video (reel, short, TikTok).</summary>
    public const string ShortVideo = "shortVideo";
    /// <summary>A long video.</summary>
    public const string LongVideo = "longVideo";
    /// <summary>An image or carousel post.</summary>
    public const string Image = "image";
    /// <summary>A text post.</summary>
    public const string Text = "text";
}

/// <summary>
/// One default comment on a workspace. Every new post to the workspace whose type is in
/// <see cref="PostTypes"/> (or any type, when it is empty) starts with it, aimed at the platforms
/// flagged here that the workspace has an account on. Each added comment is charged like any other.
/// </summary>
public class WorkspaceDefaultCommentModel
{
    /// <summary>Send it back to edit this default; leave it null to add one.</summary>
    [JsonPropertyName("_id")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Id { get; set; }

    /// <summary>1–2200 characters.</summary>
    public string Text { get; set; } = string.Empty;

    /// <summary>Only Facebook and Threads render an image on a comment.</summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? ImageUrl { get; set; }

    /// <summary>The <c>BucketFilePath</c> of an upload slot from this workspace.</summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? ImageStorageKey { get; set; }

    /// <summary>Minutes after the post publishes, 0–1440.</summary>
    public int DelayMinutes { get; set; }

    /// <summary>An inactive default is kept but never added.</summary>
    public bool IsActive { get; set; } = true;

    /// <summary>The post types it is added to (<see cref="CommentPostTypes"/>). Empty means all of them.</summary>
    public List<string> PostTypes { get; set; } = new();

    /// <summary>Post it on Facebook.</summary>
    public bool PostToFacebook { get; set; } = true;
    /// <summary>Post it on Instagram.</summary>
    public bool PostToInstagram { get; set; } = true;
    /// <summary>Post it on YouTube.</summary>
    public bool PostToYoutube { get; set; } = true;
    /// <summary>TikTok has no comment API, so it is never attempted or charged.</summary>
    public bool PostToTiktok { get; set; }
    /// <summary>Post it as a Threads reply.</summary>
    public bool PostToThreads { get; set; } = true;
    /// <summary>Post it as a reply on X.</summary>
    public bool PostToTwitter { get; set; } = true;

    /// <summary>Set by the server: the position in the list.</summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? Order { get; set; }
}

/// <summary>What <c>SetDefaultCommentsAsync</c> answers.</summary>
public class WorkspaceDefaultCommentsResult
{
    /// <summary>The saved list, in order.</summary>
    public List<WorkspaceDefaultCommentModel> Comments { get; set; } = new();

    /// <summary>The workspace's new version: pass it to the next versioned write.</summary>
    public long? Version { get; set; }
}

/// <summary>One signed upload slot for a default-comment image.</summary>
public class DefaultCommentUploadSlotModel
{
    /// <summary>Send it as the default's <c>ImageStorageKey</c>.</summary>
    public string BucketFilePath { get; set; } = string.Empty;
    /// <summary>The image's public URL once uploaded: send it as the default's <c>ImageUrl</c>.</summary>
    [JsonPropertyName("fileURL")]
    public string FileUrl { get; set; } = string.Empty;
    /// <summary>Where to PUT the bytes.</summary>
    [JsonPropertyName("uploadFileURL")]
    public string UploadFileUrl { get; set; } = string.Empty;
}

/// <summary>Why some defaults were not added: a platform (or the post) was at its comment ceiling.</summary>
public class DefaultCommentWarningModel
{
    /// <summary><c>DEFAULT_COMMENTS_TRUNCATED</c>.</summary>
    public string Code { get; set; } = string.Empty;
    /// <summary>The platform whose ceiling was hit, or <c>all</c>.</summary>
    public string Platform { get; set; } = string.Empty;
    /// <summary>How many were dropped.</summary>
    public int Dropped { get; set; }
}

/// <summary>One comment a new post would get, in the shape a create request's comments take.</summary>
public class ResolvedDefaultCommentModel
{
    /// <summary>The comment text.</summary>
    public string Text { get; set; } = string.Empty;
    /// <summary>Optional image.</summary>
    public string? ImageUrl { get; set; }
    /// <summary>The image's storage key, when we stored it.</summary>
    public string? ImageStorageKey { get; set; }
    /// <summary>Minutes after the post publishes.</summary>
    public int DelayMinutes { get; set; }
    /// <summary>The default it was copied from.</summary>
    public string DefaultCommentId { get; set; } = string.Empty;
    /// <summary><c>workspace</c> or <c>account</c>.</summary>
    public string DefaultCommentSource { get; set; } = string.Empty;
    /// <summary>Post it on Facebook.</summary>
    public bool PostToFacebook { get; set; }
    /// <summary>Post it on Instagram.</summary>
    public bool PostToInstagram { get; set; }
    /// <summary>Post it on YouTube.</summary>
    public bool PostToYoutube { get; set; }
    /// <summary>Always false: TikTok takes no comments.</summary>
    public bool PostToTiktok { get; set; }
    /// <summary>Post it as a Threads reply.</summary>
    public bool PostToThreads { get; set; }
    /// <summary>Post it as a reply on X.</summary>
    public bool PostToTwitter { get; set; }
}

/// <summary>What <c>ResolveDefaultCommentsAsync</c> answers.</summary>
public class ResolvedDefaultCommentsModel
{
    /// <summary>The comments, in post order.</summary>
    public List<ResolvedDefaultCommentModel> Comments { get; set; } = new();
    /// <summary>Defaults that did not fit.</summary>
    public List<DefaultCommentWarningModel> Warnings { get; set; } = new();
}
