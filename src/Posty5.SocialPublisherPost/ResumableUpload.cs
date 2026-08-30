using System.Net;
using System.Net.Http.Headers;
using System.Text;
using Posty5.SocialPublisherPost.Models;

namespace Posty5.SocialPublisherPost;

/// <summary>
/// Resumable uploads (tus 1.0.0) for large media.
/// </summary>
/// <remarks>
/// <para>
/// A signed PUT is all-or-nothing: a dropped connection at 90% of an hour-long
/// video starts again from zero. The API offers the same destination as a
/// resumable transfer, and this is the client for it.
/// </para>
/// <para>
/// The protocol subset needed is small — create, patch, head — so it is
/// implemented directly rather than by taking a tus dependency into a published
/// NuGet package. Resuming across process restarts is the caller's decision:
/// keep the URL handed to <c>onUploadUrl</c> and pass it back as
/// <c>resumeFrom</c>.
/// </para>
/// </remarks>
public static class ResumableUpload
{
    private const string TusVersion = "1.0.0";

    /// <summary>
    /// 8MiB matches the server's multipart part size, so one PATCH becomes one
    /// R2 part with no server-side re-buffering.
    /// </summary>
    public const int DefaultChunkSize = 8 * 1024 * 1024;

    /// <summary>Backoff between chunk retries. The chunk is retried, never the whole file.</summary>
    private static readonly int[] RetryDelaysMs = { 0, 1000, 3000, 5000, 10000 };

    /// <summary>Consecutive rounds ending at the same offset before the transfer is abandoned.</summary>
    private const int MaxConsecutiveStalls = 3;

    /// <summary>
    /// Whether this target can be uploaded resumably. A server without the
    /// resumable service configured omits these fields, and callers fall back
    /// to the signed PUT.
    /// </summary>
    public static bool IsSupported(UploadUrlInfo target) =>
        !string.IsNullOrEmpty(target.TusEndpoint) && !string.IsNullOrEmpty(target.Ticket);

    /// <summary>
    /// Upload a stream resumably and return the public URL it lands at.
    /// </summary>
    /// <remarks>
    /// The <paramref name="content"/> stream must be seekable: resuming and
    /// retrying both require seeking back to a byte offset. A non-seekable
    /// stream can only use the signed-PUT path.
    /// </remarks>
    /// <param name="target">The destination the API authorized.</param>
    /// <param name="content">The file to send. Must be seekable.</param>
    /// <param name="contentType">Type stamped on the finished object.</param>
    /// <param name="fileName">Carried for diagnostics only; the key is inside the ticket.</param>
    /// <param name="progress">Reports bytes transferred.</param>
    /// <param name="onUploadUrl">Receives the upload URL once it exists, so the transfer can be resumed later.</param>
    /// <param name="resumeFrom">Resume a previous attempt instead of creating a new upload.</param>
    /// <param name="chunkSize">Bytes per PATCH. Defaults to the server's part size.</param>
    /// <param name="cancellationToken">Cancels the transfer. Uploaded bytes survive and can be resumed.</param>
    /// <param name="transport">Test seam for driving the protocol against a scripted handler. Leave null.</param>
    public static async Task<string> UploadAsync(
        UploadUrlInfo target,
        Stream content,
        string contentType,
        string fileName = "upload",
        IProgress<UploadProgress>? progress = null,
        Action<string>? onUploadUrl = null,
        string? resumeFrom = null,
        int chunkSize = DefaultChunkSize,
        CancellationToken cancellationToken = default,
        HttpMessageHandler? transport = null)
    {
        if (!IsSupported(target))
            throw new InvalidOperationException("This upload target does not support resumable uploads.");
        if (!content.CanSeek)
            throw new ArgumentException("A resumable upload needs a seekable stream — retrying and resuming both seek to a byte offset.", nameof(content));

        var total = content.Length;
        if (target.MaxSize is > 0 && total > target.MaxSize)
        {
            var limitMb = Math.Round(target.MaxSize.Value / (1024d * 1024d));
            throw new InvalidOperationException($"This file is larger than the {limitMb}MB limit.");
        }

        // The transfer is bounded by cancellation, not by a clock: an hour of
        // video on a weak connection would blow through any timeout worth
        // setting.
        //
        // `transport` exists so the protocol loop can be driven by a scripted
        // handler in tests. Production callers leave it null.
        using var http = transport is null
            ? new HttpClient { Timeout = Timeout.InfiniteTimeSpan }
            : new HttpClient(transport, disposeHandler: false) { Timeout = Timeout.InfiniteTimeSpan };

        string uploadUrl;
        long offset;

        if (!string.IsNullOrEmpty(resumeFrom))
        {
            uploadUrl = resumeFrom!;
            offset = await GetOffsetAsync(http, uploadUrl, cancellationToken);
        }
        else
        {
            uploadUrl = await CreateAsync(http, target, total, fileName, contentType, cancellationToken);
            onUploadUrl?.Invoke(uploadUrl);
            offset = 0;
        }

        progress?.Report(new UploadProgress { BytesTransferred = offset, TotalBytes = total });

        // A round ending where it started is not automatically fatal: a spurious
        // 409 re-syncs to the offset we already had, and retrying that chunk is
        // the right recovery. Only repeated stalls end the transfer.
        var stalls = 0;

        while (offset < total)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var length = (int)Math.Min(chunkSize, total - offset);
            var next = await PatchChunkAsync(http, uploadUrl, content, offset, length, cancellationToken);

            if (next <= offset)
            {
                if (++stalls >= MaxConsecutiveStalls)
                    throw new InvalidOperationException("The upload stopped making progress and was abandoned.");
                continue;
            }

            stalls = 0;
            offset = next;
            progress?.Report(new UploadProgress { BytesTransferred = offset, TotalBytes = total });
        }

        // `FileURL` came back with the ticket, so the final URL is known up
        // front and there is no completion body to parse.
        return target.FileURL ?? string.Empty;
    }

    /// <summary>
    /// Create the upload and return its URL. The destination lives inside the
    /// signed ticket, so filename and filetype are diagnostics only.
    /// </summary>
    private static async Task<string> CreateAsync(
        HttpClient http,
        UploadUrlInfo target,
        long size,
        string fileName,
        string contentType,
        CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, target.TusEndpoint);
        request.Headers.TryAddWithoutValidation("Tus-Resumable", TusVersion);
        request.Headers.TryAddWithoutValidation("Upload-Length", size.ToString());
        request.Headers.TryAddWithoutValidation("Upload-Metadata", EncodeMetadata(
            ("ticket", target.Ticket!),
            ("filename", fileName),
            ("filetype", contentType)));

        using var response = await http.SendAsync(request, cancellationToken);

        if (response.StatusCode != HttpStatusCode.Created)
        {
            var body = await SafeReadAsync(response);
            throw new InvalidOperationException($"Could not start the upload ({(int)response.StatusCode}): {body}");
        }

        var location = response.Headers.Location?.ToString();
        if (string.IsNullOrEmpty(location))
            throw new InvalidOperationException("Upload was created but the server returned no Location header.");

        // A tus server may answer with a relative Location; resolve it against
        // the endpoint so a gateway-relative path still points somewhere usable.
        return new Uri(new Uri(target.TusEndpoint!), location).ToString();
    }

    /// <summary>Ask the server how much of this upload it already holds.</summary>
    private static async Task<long> GetOffsetAsync(HttpClient http, string uploadUrl, CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Head, uploadUrl);
        request.Headers.TryAddWithoutValidation("Tus-Resumable", TusVersion);

        using var response = await http.SendAsync(request, cancellationToken);

        if (response.StatusCode is HttpStatusCode.NotFound or HttpStatusCode.Gone)
            throw new InvalidOperationException("This upload has expired on the server and cannot be resumed.");
        if (!response.IsSuccessStatusCode)
            throw new InvalidOperationException($"Could not read the upload's progress ({(int)response.StatusCode}).");

        return ReadOffset(response) ?? 0;
    }

    /// <summary>
    /// Send one chunk, retrying the chunk itself on a transient failure.
    /// Returns the server's new offset, which is authoritative — a chunk can be
    /// partially accepted, and trusting our own arithmetic would corrupt
    /// everything after it.
    /// </summary>
    private static async Task<long> PatchChunkAsync(
        HttpClient http,
        string uploadUrl,
        Stream content,
        long offset,
        int length,
        CancellationToken cancellationToken)
    {
        Exception? last = null;

        foreach (var delay in RetryDelaysMs)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (delay > 0) await Task.Delay(delay, cancellationToken);

            try
            {
                // Seek every attempt: a failed send may have consumed part of
                // the stream, and re-sending from wherever it stopped would
                // silently corrupt the object.
                content.Position = offset;

                using var request = new HttpRequestMessage(new HttpMethod("PATCH"), uploadUrl);
                request.Headers.TryAddWithoutValidation("Tus-Resumable", TusVersion);
                request.Headers.TryAddWithoutValidation("Upload-Offset", offset.ToString());

                var buffer = new byte[length];
                var read = await ReadExactlyAsync(content, buffer, cancellationToken);

                var body = new ByteArrayContent(buffer, 0, read);
                body.Headers.ContentType = new MediaTypeHeaderValue("application/offset+octet-stream");
                request.Content = body;

                using var response = await http.SendAsync(request, cancellationToken);

                if (response.StatusCode == HttpStatusCode.NoContent)
                    return ReadOffset(response) ?? offset + read;

                // 409 means our offset disagrees with the server's. Re-syncing
                // is the correct recovery, not a retry at the same wrong offset.
                if (response.StatusCode == HttpStatusCode.Conflict)
                    return await GetOffsetAsync(http, uploadUrl, cancellationToken);

                // Any other 4xx is a decision, not a blip — retrying an expired
                // ticket or an oversized file only wastes the user's time.
                if ((int)response.StatusCode >= 400 && (int)response.StatusCode < 500)
                {
                    var text = await SafeReadAsync(response);
                    throw new UploadRejectedException($"Upload rejected ({(int)response.StatusCode}): {text}");
                }

                last = new InvalidOperationException($"Upload failed ({(int)response.StatusCode})");
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (UploadRejectedException)
            {
                throw;
            }
            catch (Exception ex)
            {
                last = ex;
            }
        }

        throw last ?? new InvalidOperationException("Upload failed");
    }

    /// <summary>
    /// Fill as much of the buffer as the stream will give. A single Read is not
    /// guaranteed to return the full count, and a short read would send a
    /// chunk that does not match its declared length.
    /// </summary>
    private static async Task<int> ReadExactlyAsync(Stream stream, byte[] buffer, CancellationToken cancellationToken)
    {
        var read = 0;
        while (read < buffer.Length)
        {
            var n = await stream.ReadAsync(buffer.AsMemory(read, buffer.Length - read), cancellationToken);
            if (n == 0) break;
            read += n;
        }
        return read;
    }

    private static long? ReadOffset(HttpResponseMessage response)
    {
        if (response.Headers.TryGetValues("Upload-Offset", out var values)
            && long.TryParse(values.FirstOrDefault(), out var parsed))
        {
            return parsed;
        }
        return null;
    }

    private static async Task<string> SafeReadAsync(HttpResponseMessage response)
    {
        try
        {
            var text = await response.Content.ReadAsStringAsync();
            return text.Length > 300 ? text[..300] : text;
        }
        catch
        {
            return string.Empty;
        }
    }

    /// <summary>tus metadata is `key base64value` pairs, comma separated.</summary>
    private static string EncodeMetadata(params (string Key, string Value)[] pairs) =>
        string.Join(",", pairs
            .Where(p => !string.IsNullOrEmpty(p.Value))
            .Select(p => $"{p.Key} {Convert.ToBase64String(Encoding.UTF8.GetBytes(p.Value))}"));
}

/// <summary>
/// The server refused the upload outright — an expired ticket, an oversized
/// file, or an unsupported media kind. Distinct from a transient failure
/// because retrying it can never succeed.
/// </summary>
public class UploadRejectedException : Exception
{
    /// <summary>Creates the exception with the server's explanation.</summary>
    public UploadRejectedException(string message) : base(message) { }
}
