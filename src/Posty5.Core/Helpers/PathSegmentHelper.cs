namespace Posty5.Core.Helpers;

/// <summary>
/// Turns a caller-supplied value (an id, usually) into one URL path segment, so
/// a value such as <c>abc/../../other</c> or <c>abc?x=1</c> cannot reach a
/// different route with the caller's API key.
/// </summary>
public static class PathSegmentHelper
{
    /// <summary>
    /// The value percent-encoded as a single path segment. Refuses an empty value
    /// and the dot segments <c>.</c> and <c>..</c>, which the URI resolver would
    /// otherwise collapse into the parent route.
    /// </summary>
    /// <param name="value">The value to place in the path.</param>
    /// <param name="paramName">The caller's parameter name, for the exception.</param>
    /// <returns>The escaped segment.</returns>
    /// <exception cref="ArgumentException">The value is empty, whitespace, <c>.</c> or <c>..</c>.</exception>
    public static string Escape(string value, string paramName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value, paramName);
        if (value is "." or "..")
            throw new ArgumentException($"'{value}' is not a valid path segment.", paramName);

        return Uri.EscapeDataString(value);
    }
}
