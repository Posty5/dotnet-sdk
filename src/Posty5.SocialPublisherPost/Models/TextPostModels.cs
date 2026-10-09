using System.Text.Json.Serialization;

namespace Posty5.SocialPublisherPost.Models;

// ============================================================================
// TEXT POST MODELS (Posty5.SocialPublisherPost 4.6.0+)
//
// Transcribed from the API's /swagger.json (`/text/workspace`, `/text/account`).
// A text post is a status update with no media. It goes to the platforms that
// take text — Facebook Pages, Threads and X; YouTube, Instagram and TikTok have
// no text surface and are skipped, not failed.
// ============================================================================

/// <summary>
/// The fields a text post carries whichever way it is targeted. Use
/// <see cref="CreateTextPostToWorkspaceRequest"/> or
/// <see cref="CreateTextPostToAccountRequest"/>.
/// </summary>
public abstract class CreateTextPostRequestBase
{
    /// <summary>
    /// The post's text (required). Used on every platform that has no block of
    /// its own below. Threads takes at most 500 characters and refuses a longer
    /// caption unless <see cref="Threads"/> carries its own text; X cuts it at a
    /// word to fit 280 weighted characters and reports that in
    /// <see cref="CreatePostResult.TruncatedTargets"/>.
    /// </summary>
    public string Caption { get; set; } = string.Empty;

    /// <summary>Overrides the caption on Facebook, and adds a link preview. Optional.</summary>
    public TextPostFacebookConfig? Facebook { get; set; }

    /// <summary>Overrides the caption on Threads. Optional.</summary>
    public ThreadsTextConfig? Threads { get; set; }

    /// <summary>
    /// X settings. <b>Publishing to X requires the Pro plan or higher</b>: on a
    /// lower plan a workspace post drops X and lists it in
    /// <see cref="CreatePostResult.RefusedTargets"/>; an account post is refused.
    /// Optional.
    /// </summary>
    public TwitterConfig? Twitter { get; set; }

    /// <summary>
    /// When to publish. Omitted or <c>now</c> publishes immediately on every
    /// plan; a future time requires the Pro plan or higher.
    /// </summary>
    public ScheduleConfig? Schedule { get; set; }

    /// <summary>
    /// Up to <see cref="CommentLimits.MaxPerPost"/> comments, posted in order
    /// once the post is live. Each comment that posts is charged separately.
    /// </summary>
    public List<CommentRequest>? Comments { get; set; }

    /// <summary>Ids of saved hashtag groups to merge into the text (at most 5; yours only).</summary>
    public List<string>? HashtagGroupIds { get; set; }

    /// <summary>Ad-hoc hashtags to merge in, with or without the leading <c>#</c> (at most 30).</summary>
    public List<string>? Hashtags { get; set; }

    /// <summary>
    /// Replace each link with a Posty5 short link per platform so clicks can be
    /// counted. Requires the Pro plan or higher and is charged per link.
    /// </summary>
    public bool? TrackLinks { get; set; }

    /// <summary>UTM values for tracked links.</summary>
    public TrackedLinkUtm? Utm { get; set; }

    /// <summary>Custom tag for filtering.</summary>
    public string? Tag { get; set; }

    /// <summary>Your own reference id.</summary>
    public string? RefId { get; set; }

    /// <summary>
    /// Origin label. Leave it null and the client fills it in when the post is
    /// sent: <c>Posty5Options.CreatedFrom</c> when set, otherwise
    /// <c>dotnetPackage</c>. A value set here wins over both.
    /// </summary>
    public string? CreatedFrom { get; set; }
}

/// <summary>A text post to every text-capable account connected to a workspace.</summary>
public class CreateTextPostToWorkspaceRequest : CreateTextPostRequestBase
{
    /// <summary>The workspace (required).</summary>
    public string WorkspaceId { get; set; } = string.Empty;
}

/// <summary>
/// A text post to one connected account. The platform is the account's own; an
/// account on a platform that takes no text is refused.
/// </summary>
public class CreateTextPostToAccountRequest : CreateTextPostRequestBase
{
    /// <summary>The connected account (required).</summary>
    public string AccountId { get; set; } = string.Empty;
}

/// <summary>The Facebook block of a text post.</summary>
public class TextPostFacebookConfig
{
    /// <summary>The status text on Facebook (required when the block is sent).</summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>Kept for other surfaces; Facebook ignores it.</summary>
    public string? Title { get; set; }

    /// <summary>
    /// An http(s) link Facebook fetches to build a preview. Posty5 never
    /// requests it.
    /// </summary>
    public string? Link { get; set; }
}

/// <summary>The Threads block of a text post.</summary>
public class ThreadsTextConfig
{
    /// <summary>The Threads post, at most 500 characters (required when the block is sent).</summary>
    public string Text { get; set; } = string.Empty;

    /// <summary>
    /// Who may reply: <c>everyone</c> (default), <c>accounts_you_follow</c>,
    /// <c>mentioned_only</c>, <c>parent_post_author_only</c>, <c>followers_only</c>.
    /// </summary>
    [JsonPropertyName("reply_control")]
    public string? ReplyControl { get; set; }

    /// <summary>
    /// The one topic Threads links (letters, digits, <c>_</c>). Without it the
    /// post's first hashtag becomes the topic.
    /// </summary>
    [JsonPropertyName("topic_tag")]
    public string? TopicTag { get; set; }

    /// <summary>An http(s) link Threads shows as a preview card.</summary>
    public string? Link { get; set; }
}

/// <summary>The X (Twitter) block of a post.</summary>
public class TwitterConfig
{
    /// <summary>
    /// At most 280 weighted characters — a link counts 23, most CJK and emoji
    /// count 2 (required when the block is sent).
    /// </summary>
    public string Text { get; set; } = string.Empty;

    /// <summary>Who may reply: <c>everyone</c> (default), <c>mentionedUsers</c>, <c>following</c>.</summary>
    [JsonPropertyName("reply_settings")]
    public string? ReplySettings { get; set; }

    /// <summary>A poll. Text posts only, and not together with <see cref="QuoteTweetId"/>.</summary>
    public TwitterPollConfig? Poll { get; set; }

    /// <summary>The X post this one quotes: its id or its x.com / twitter.com link.</summary>
    [JsonPropertyName("quote_tweet_id")]
    public string? QuoteTweetId { get; set; }
}

/// <summary>A poll on an X text post.</summary>
public class TwitterPollConfig
{
    /// <summary>2 to 4 choices, 25 characters each.</summary>
    public List<string> Options { get; set; } = new();

    /// <summary>How long the poll stays open, 5 minutes to 7 days (10080).</summary>
    [JsonPropertyName("duration_minutes")]
    public int DurationMinutes { get; set; }
}

/// <summary>
/// UTM values for tracked links. <c>utm_source</c> is always the platform and
/// <c>utm_content</c> the post id.
/// </summary>
public class TrackedLinkUtm
{
    /// <summary><c>utm_medium</c>; the API uses <c>social</c> when unset.</summary>
    public string? Medium { get; set; }

    /// <summary><c>utm_campaign</c>; defaults to the post's <c>refId</c>, else <c>post-&lt;number&gt;</c>.</summary>
    public string? Campaign { get; set; }
}
