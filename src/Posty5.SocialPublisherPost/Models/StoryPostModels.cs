namespace Posty5.SocialPublisherPost.Models;

// ============================================================================
// STORY MODELS (Posty5.SocialPublisherPost 4.6.0+)
//
// Transcribed from the API's /swagger.json (`/story/workspace`, `/story/account`).
// A story is one image or one video for Facebook Pages and Instagram. It has no
// caption, comments, hashtags or link — the API refuses a story that sends any
// of them — and it disappears 24 hours after it goes out.
//
// This release takes media by URL only: an image by `Image.ExternalUrl`
// (source `image-url`), a video by `VideoURL`.
// ============================================================================

/// <summary>
/// The fields a story carries whichever way it is targeted. Use
/// <see cref="CreateStoryPostToWorkspaceRequest"/> or
/// <see cref="CreateStoryPostToAccountRequest"/>.
/// </summary>
public abstract class CreateStoryPostRequestBase
{
    /// <summary>
    /// <see cref="StoryKinds.Image"/> or <see cref="StoryKinds.Video"/> (required).
    /// Decides which of <see cref="Image"/> and <see cref="VideoURL"/> is sent;
    /// the other must be left null.
    /// </summary>
    public string Kind { get; set; } = StoryKinds.Image;

    /// <summary>
    /// For an image story: the image, with <see cref="ImageRequest.Source"/>
    /// <see cref="ImageSource.ImageUrl"/> and a public http(s)
    /// <see cref="ImageRequest.ExternalUrl"/>.
    /// </summary>
    public ImageRequest? Image { get; set; }

    /// <summary>For a video story: a direct, public http(s) video URL.</summary>
    public string? VideoURL { get; set; }

    /// <summary>
    /// When to publish. Omitted or <c>now</c> publishes immediately; a future
    /// time requires the Pro plan or higher.
    /// </summary>
    public ScheduleConfig? Schedule { get; set; }

    /// <summary>Custom tag for filtering.</summary>
    public string? Tag { get; set; }

    /// <summary>Your own reference id.</summary>
    public string? RefId { get; set; }

    /// <summary>
    /// Origin label. Leave it null and the client fills it in when the story is
    /// sent: <c>Posty5Options.CreatedFrom</c> when set, otherwise
    /// <c>dotnetPackage</c>. A value set here wins over both.
    /// </summary>
    public string? CreatedFrom { get; set; }
}

/// <summary>
/// A story to every story-capable account connected to a workspace (Facebook
/// Pages and Instagram; YouTube and TikTok are skipped and reported in
/// <see cref="CreatePostResult.SkippedPlatforms"/>).
/// </summary>
public class CreateStoryPostToWorkspaceRequest : CreateStoryPostRequestBase
{
    /// <summary>The workspace (required).</summary>
    public string WorkspaceId { get; set; } = string.Empty;
}

/// <summary>
/// A story to one connected account. An account on a platform that has no
/// stories through its API is refused.
/// </summary>
public class CreateStoryPostToAccountRequest : CreateStoryPostRequestBase
{
    /// <summary>The connected account (required).</summary>
    public string AccountId { get; set; } = string.Empty;
}
