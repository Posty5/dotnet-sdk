namespace Posty5.Core.Configuration;

/// <summary>
/// Header names and error codes of the API's optimistic-concurrency contract:
/// every update, delete and state change carries the version the caller read,
/// and the API refuses the write when the stored version has moved on.
/// </summary>
public static class Posty5ConcurrencyConst
{
    /// <summary>The request header that carries the version: <c>If-Match: "&lt;v&gt;"</c>.</summary>
    public const string IfMatchHeader = "If-Match";

    /// <summary>The response header that carries the new version after a versioned write.</summary>
    public const string ETagHeader = "ETag";

    /// <summary>
    /// The response header the API sends during the rollout's report mode on a
    /// write that should have carried a version.
    /// </summary>
    public const string ConcurrencyHeader = "X-Posty5-Concurrency";

    /// <summary>The <see cref="ConcurrencyHeader"/> value meaning "this write carried no version".</summary>
    public const string MissingVersionValue = "missing-version";

    /// <summary>409: the stored version differs from the one sent.</summary>
    public const string VersionConflictCode = "VERSION_CONFLICT";

    /// <summary>428: a versioned route was called without <c>If-Match</c>.</summary>
    public const string VersionRequiredCode = "VERSION_REQUIRED";

    /// <summary>400: <c>If-Match</c> is malformed, or a bulk <c>versions</c> map does not cover every id.</summary>
    public const string VersionInvalidCode = "VERSION_INVALID";

    /// <summary>A bulk <c>skipped</c> entry whose id was not found.</summary>
    public const string NotFoundCode = "NOT_FOUND";

    /// <summary>The <c>If-Match</c> value for <paramref name="version"/>: the version, quoted.</summary>
    /// <param name="version">The document version the caller read (its <c>__v</c>).</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="version"/> is negative.</exception>
    public static string IfMatchValue(long version)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(version);
        return $"\"{version}\"";
    }
}
