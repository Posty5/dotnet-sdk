using System.Net.Http.Headers;

namespace Posty5.Core.Http;

/// <summary>
/// The one PUT of a file to a signed (pre-signed R2) upload URL, shared by every
/// client that uploads through an <c>upload-url</c> route. The signed URL carries
/// its own authorization, so the Posty5 API key is never sent.
/// </summary>
public static class R2UploadHelper
{
    /// <summary>
    /// PUT <paramref name="content"/> to <paramref name="uploadUrl"/> with the given
    /// <c>Content-Type</c>. Throws <see cref="HttpRequestException"/> on a network error
    /// or a non-success status. The stream is consumed and disposed with the request.
    /// </summary>
    /// <param name="uploadUrl">Signed upload URL</param>
    /// <param name="content">File content</param>
    /// <param name="contentType">MIME type; must match the one the URL was signed for</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <param name="timeout">Request timeout; <c>null</c> keeps the <see cref="HttpClient"/> default (100 s)</param>
    public static async Task UploadAsync(
        string uploadUrl,
        Stream content,
        string contentType,
        CancellationToken cancellationToken = default,
        TimeSpan? timeout = null)
    {
        if (string.IsNullOrWhiteSpace(uploadUrl))
            throw new ArgumentException("Upload URL is required", nameof(uploadUrl));
        ArgumentNullException.ThrowIfNull(content);

        using var client = new HttpClient();
        if (timeout.HasValue) client.Timeout = timeout.Value;
        using var body = new StreamContent(content);
        body.Headers.ContentType = new MediaTypeHeaderValue(contentType);

        var response = await client.PutAsync(uploadUrl, body, cancellationToken);
        response.EnsureSuccessStatusCode();
    }
}
