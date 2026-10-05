namespace Posty5.SocialPublisherPost.Models;

/// <summary>
/// What a story carries, as the API spells it (<c>kind</c>).
/// </summary>
/// <remarks>
/// String constants rather than an enum, like the rest of this package's
/// vocabularies that travel as plain strings.
/// </remarks>
public static class StoryKinds
{
    /// <summary>One image.</summary>
    public const string Image = "image";

    /// <summary>One video.</summary>
    public const string Video = "video";

    /// <summary>
    /// The <c>source</c> a URL video story is sent with. The only one this
    /// release sends: an uploaded video would also need its upload slot's post id.
    /// </summary>
    internal const string VideoUrlSource = "video-url";
}
