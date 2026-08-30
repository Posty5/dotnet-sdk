using System.Text.Json.Serialization;

namespace Posty5.SocialPublisherPost.Models;

/// <summary>
/// Long-video limits and pricing.
/// </summary>
/// <remarks>
/// These mirror what the API enforces and are exposed so a caller can sanity
/// check a file before starting a long upload. The server measures the real
/// duration and remains the only authority on both the length and the price —
/// a client-supplied duration would be a client-supplied price.
/// </remarks>
public static class LongVideo
{
    /// <summary>Longest video accepted for a long-video post: 60 minutes.</summary>
    public const int MaxDurationSeconds = 3600;

    /// <summary>One charge unit covers each started 5 minutes of video.</summary>
    public const int CreditUnitSeconds = 300;

    /// <summary>
    /// Charge units for a video of the given length: each started
    /// <see cref="CreditUnitSeconds"/> counts as a whole unit. A 12-minute
    /// video is 3 units; so is a 15-minute one, while 15m01s is 4.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when the duration is not positive. A non-positive duration means
    /// the video was never measured, which is a bug at the call site rather
    /// than a user error — failing loudly beats quoting a free publish.
    /// </exception>
    public static int CreditUnits(int durationSeconds)
    {
        if (durationSeconds <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(durationSeconds),
                durationSeconds,
                "A probed duration in seconds is required.");
        }

        return (int)Math.Ceiling(durationSeconds / (double)CreditUnitSeconds);
    }

    /// <summary>Whether a duration is within the accepted ceiling.</summary>
    public static bool IsDurationAllowed(int durationSeconds) =>
        durationSeconds > 0 && durationSeconds <= MaxDurationSeconds;
}

/// <summary>
/// Long-video metadata carried on a post. Present only when the post's type is
/// <c>longVideo</c>.
/// </summary>
public class VideoDto
{
    /// <summary>Duration measured server-side, in seconds.</summary>
    public int DurationSeconds { get; set; }

    /// <summary>Charge units billed — one per started 5 minutes.</summary>
    public int? CreditUnits { get; set; }

    /// <summary>Container format reported by the probe.</summary>
    public string? Container { get; set; }
    /// <summary>Video codec reported by the probe.</summary>
    public string? VideoCodec { get; set; }
    /// <summary>Audio codec reported by the probe.</summary>
    public string? AudioCodec { get; set; }
    /// <summary>Frame width in pixels.</summary>
    public int? Width { get; set; }
    /// <summary>Frame height in pixels.</summary>
    public int? Height { get; set; }
    /// <summary>File size in bytes, when the probe could determine it.</summary>
    public long? SizeBytes { get; set; }
}

/// <summary>What one platform would do with a video of a given length.</summary>
public class LongVideoPlatformVerdict
{
    /// <summary>youtube, facebook, instagram or tiktok.</summary>
    public string Platform { get; set; } = string.Empty;

    /// <summary>Whether this platform accepts a video that long.</summary>
    public bool Accepted { get; set; }

    /// <summary>The limit that applied, in seconds, when one did.</summary>
    public int? LimitSeconds { get; set; }

    /// <summary>Why the platform refused. Set only when <see cref="Accepted"/> is false.</summary>
    public string? Reason { get; set; }
}

/// <summary>
/// What a long video will cost, measured and priced before anything is created
/// or charged.
/// </summary>
public class LongVideoQuoteResponse
{
    /// <summary>Duration measured server-side, in seconds, rounded up.</summary>
    public int DurationSeconds { get; set; }

    /// <summary>The ceiling the API enforces (3600).</summary>
    public int MaxDurationSeconds { get; set; }

    /// <summary>False when the video is over the ceiling; <see cref="Reason"/> then says by how much.</summary>
    public bool WithinLimit { get; set; }

    /// <summary>Charge units — one per started 5 minutes. Zero when over the limit.</summary>
    public int Units { get; set; }

    /// <summary>This plan's cost per unit (50 by default).</summary>
    public int CreditsPerUnit { get; set; }

    /// <summary>Total credits the post will cost.</summary>
    public int Credits { get; set; }

    /// <summary>True when the plan does not include long video posting.</summary>
    public bool IsGated { get; set; }

    /// <summary>Container format reported by the probe.</summary>
    public string? Container { get; set; }
    /// <summary>Video codec reported by the probe.</summary>
    public string? VideoCodec { get; set; }
    /// <summary>Frame width in pixels.</summary>
    public int? Width { get; set; }
    /// <summary>Frame height in pixels.</summary>
    public int? Height { get; set; }

    /// <summary>What each platform would do with a video this long.</summary>
    public List<LongVideoPlatformVerdict> Platforms { get; set; } = new();

    /// <summary>Set only when <see cref="WithinLimit"/> is false.</summary>
    public string? Reason { get; set; }
}

/// <summary>
/// A connected platform dropped from a workspace post because the video is too
/// long for it. The post still publishes to the remaining targets.
/// </summary>
public class LongVideoRefusedTarget
{
    /// <summary>youtube, facebook, instagram or tiktok.</summary>
    public string Platform { get; set; } = string.Empty;

    /// <summary>Names that platform's limit and this video's length.</summary>
    public string Reason { get; set; } = string.Empty;
}

/// <summary>
/// Result of publishing a long video. Unlike the short-video helpers, which
/// return only an id, this carries what was measured, what was charged, and
/// which targets — if any — were dropped for exceeding their own limit.
/// </summary>
public class PublishLongVideoResult
{
    /// <summary>Created post ID.</summary>
    [JsonPropertyName("_id")]
    public string Id { get; set; } = string.Empty;

    /// <summary>Duration measured server-side, in seconds.</summary>
    public int DurationSeconds { get; set; }

    /// <summary>Charge units billed.</summary>
    public int CreditUnits { get; set; }

    /// <summary>Credits actually charged.</summary>
    public int Credits { get; set; }

    /// <summary>
    /// Targets dropped because the video exceeded their duration limit. Empty
    /// when every connected account accepted it, and always empty for
    /// account-targeted posts, which are refused outright rather than
    /// partially published.
    /// </summary>
    public List<LongVideoRefusedTarget> RefusedTargets { get; set; } = new();
}

/// <summary>Request body for the long-video quote endpoint.</summary>
public class LongVideoQuoteRequest
{
    /// <summary>URL of an uploaded or externally hosted video.</summary>
    public string VideoURL { get; set; } = string.Empty;
}

/// <summary>
/// Move a not-yet-published post to another time, or send it out now.
/// </summary>
public class ReschedulePostRequest
{
    /// <summary>New publish moment. Type is "now" or "schedule".</summary>
    public ScheduleConfig Schedule { get; set; } = new();

    /// <summary>Optionally replace the caption at the same time.</summary>
    public string? Caption { get; set; }
}

/// <summary>Progress of a long upload, reported through <c>IProgress&lt;T&gt;</c>.</summary>
public class UploadProgress
{
    /// <summary>Bytes transferred so far.</summary>
    public long BytesTransferred { get; set; }

    /// <summary>Total bytes to transfer, when the stream can report its length.</summary>
    public long? TotalBytes { get; set; }

    /// <summary>Percentage complete, when <see cref="TotalBytes"/> is known.</summary>
    public double? Percentage => TotalBytes is > 0 ? BytesTransferred * 100.0 / TotalBytes.Value : null;
}
