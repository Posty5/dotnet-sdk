namespace Posty5.SocialPublisherPost.Models;

/// <summary>
/// The platforms one removal can target on its own, as the API spells them
/// (the <c>platform</c> of <c>RemovePostAsync(id, version, platform)</c>).
/// </summary>
/// <remarks>
/// String constants rather than an enum, like the rest of this package's
/// vocabularies that travel as plain strings.
/// </remarks>
public static class RemovePlatforms
{
    /// <summary>YouTube.</summary>
    public const string YouTube = "youtube";

    /// <summary>Facebook.</summary>
    public const string Facebook = "facebook";

    /// <summary>Instagram. It has no delete API, so its copy stays live.</summary>
    public const string Instagram = "instagram";

    /// <summary>TikTok. It has no delete API, so its copy stays live.</summary>
    public const string TikTok = "tiktok";

    /// <summary>Threads.</summary>
    public const string Threads = "threads";

    /// <summary>X (Twitter).</summary>
    public const string Twitter = "twitter";

    /// <summary>Every value above, for checking a caller's input.</summary>
    internal static readonly IReadOnlyCollection<string> All = new[] { YouTube, Facebook, Instagram, TikTok, Threads, Twitter };
}
