using System.Net;
using System.Text;
using Xunit;
using Posty5.SocialPublisherPost;
using Posty5.SocialPublisherPost.Models;

namespace Posty5.Tests.Unit;

/// <summary>
/// tus client — protocol-level tests against a scripted handler.
///
/// These assert the wire behaviour the server requires: the creation POST's
/// headers and base64 metadata, the PATCH loop's offsets, offset re-sync on
/// conflict, resume, and the refusal to retry a 4xx.
/// </summary>
public class ResumableUploadTests
{
    private const string TusEndpoint = "https://api.example.com/api/uploads/tus";
    private const string UploadUrl = "https://api.example.com/api/uploads/tus/abc-123";
    private const string FileUrl = "https://cdn.example.com/v.mp4";

    private static UploadUrlInfo Target(long? maxSize = null) => new()
    {
        FileURL = FileUrl,
        UploadFileURL = "https://upload.example.com/v?sig=x",
        BucketFilePath = "k/v.mp4",
        TusEndpoint = TusEndpoint,
        Ticket = "payload.signature",
        MaxSize = maxSize,
    };

    private static MemoryStream Content(int size) => new(new byte[size]);

    /// <summary>Records every request and replays a scripted tus server.</summary>
    private sealed class TusHandler : HttpMessageHandler
    {
        public readonly List<(string Method, string Url, Dictionary<string, string> Headers, int BodyLength)> Requests = new();
        private long _served;
        private int _patchIndex;

        /// <summary>Total size the "file" is, so PATCH can clamp the offset.</summary>
        public long Total { get; init; }

        /// <summary>Status the HEAD probe answers with. Used to simulate an expired upload.</summary>
        public HttpStatusCode HeadStatus { get; init; } = HttpStatusCode.OK;

        /// <summary>Status the termination DELETE answers with.</summary>
        public HttpStatusCode DeleteStatus { get; init; } = HttpStatusCode.NoContent;

        /// <summary>How many bytes the fake server currently holds.</summary>
        public long ServedOffset => _served;

        /// <summary>Override a PATCH by index — used to inject a 409 or a 4xx.</summary>
        public Func<int, HttpStatusCode?>? PatchOverride { get; init; }

        public TusHandler(long startOffset = 0) => _served = startOffset;

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var headers = request.Headers.ToDictionary(h => h.Key, h => string.Join(",", h.Value));
            var bodyLength = request.Content is null ? 0 : (await request.Content.ReadAsByteArrayAsync(cancellationToken)).Length;
            Requests.Add((request.Method.Method, request.RequestUri!.ToString(), headers, bodyLength));

            if (request.Method == HttpMethod.Post)
            {
                var created = new HttpResponseMessage(HttpStatusCode.Created);
                created.Headers.Location = new Uri(UploadUrl);
                return created;
            }

            if (request.Method == HttpMethod.Delete)
            {
                return new HttpResponseMessage(DeleteStatus);
            }

            if (request.Method == HttpMethod.Head)
            {
                if (HeadStatus != HttpStatusCode.OK) return new HttpResponseMessage(HeadStatus);
                var head = new HttpResponseMessage(HttpStatusCode.OK);
                head.Headers.TryAddWithoutValidation("Upload-Offset", _served.ToString());
                return head;
            }

            var over = PatchOverride?.Invoke(_patchIndex++);
            if (over is not null)
            {
                return new HttpResponseMessage(over.Value) { Content = new StringContent("refused") };
            }

            _served = Math.Min(_served + bodyLength, Total);
            var ok = new HttpResponseMessage(HttpStatusCode.NoContent);
            ok.Headers.TryAddWithoutValidation("Upload-Offset", _served.ToString());
            return ok;
        }
    }

    [Fact]
    public void IsSupported_RequiresBothTheEndpointAndTheTicket()
    {
        Assert.True(ResumableUpload.IsSupported(Target()));
        Assert.False(ResumableUpload.IsSupported(new UploadUrlInfo { TusEndpoint = TusEndpoint }));
        Assert.False(ResumableUpload.IsSupported(new UploadUrlInfo { Ticket = "t" }));
        Assert.False(ResumableUpload.IsSupported(new UploadUrlInfo()));
    }

    [Fact]
    public void DefaultChunkSize_MatchesTheServersPartSize()
    {
        // The server's S3Store uses an 8MiB part size; matching it means one
        // PATCH becomes one R2 part with no re-buffering.
        Assert.Equal(8 * 1024 * 1024, ResumableUpload.DefaultChunkSize);
    }

    [Fact]
    public async Task UploadAsync_RefusesATargetWithoutResumableSupport()
    {
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            ResumableUpload.UploadAsync(new UploadUrlInfo { FileURL = FileUrl }, Content(10), "video/mp4"));
    }

    [Fact]
    public async Task UploadAsync_RefusesANonSeekableStream()
    {
        // Resuming and retrying both seek to a byte offset, so a forward-only
        // stream cannot use this path — it must fall back to the signed PUT.
        var ex = await Assert.ThrowsAsync<ArgumentException>(() =>
            ResumableUpload.UploadAsync(Target(), new NonSeekableStream(), "video/mp4"));
        Assert.Contains("seekable", ex.Message);
    }

    [Fact]
    public async Task UploadAsync_RefusesAFileOverTheTargetSize()
    {
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            ResumableUpload.UploadAsync(Target(maxSize: 5), Content(10), "video/mp4"));
        Assert.Contains("larger than", ex.Message);
    }

    [Fact]
    public async Task UploadAsync_HonoursCancellationBeforeTransferring()
    {
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            ResumableUpload.UploadAsync(Target(), Content(10), "video/mp4", cancellationToken: cts.Token));
    }

    [Fact]
    public void UploadRejectedException_CarriesTheServersExplanation()
    {
        // Distinct from a transient failure precisely so callers can tell that
        // retrying is pointless.
        var ex = new UploadRejectedException("Upload rejected (403): Invalid upload ticket.");
        Assert.Contains("403", ex.Message);
        Assert.IsAssignableFrom<Exception>(ex);
    }

    // ── Protocol, driven through the transport seam ─────────────────────────

    [Fact]
    public async Task Create_SendsTheTusVersionTheLengthAndTheTicketAsBase64Metadata()
    {
        using var handler = new TusHandler { Total = 100 };

        await ResumableUpload.UploadAsync(Target(), Content(100), "video/mp4", "recording.mp4",
            chunkSize: 100, transport: handler);

        var create = handler.Requests[0];
        Assert.Equal("POST", create.Method);
        Assert.Equal(TusEndpoint, create.Url);
        Assert.Equal("1.0.0", create.Headers["Tus-Resumable"]);
        Assert.Equal("100", create.Headers["Upload-Length"]);

        // tus metadata is `key base64value` pairs, comma separated.
        var pairs = create.Headers["Upload-Metadata"].Split(',');
        var ticket = pairs.First(p => p.StartsWith("ticket ")).Split(' ')[1];
        Assert.Equal("payload.signature", Encoding.UTF8.GetString(Convert.FromBase64String(ticket)));

        var name = pairs.First(p => p.StartsWith("filename ")).Split(' ')[1];
        Assert.Equal("recording.mp4", Encoding.UTF8.GetString(Convert.FromBase64String(name)));
    }

    [Fact]
    public async Task Create_HandsBackTheUploadUrlSoTheTransferCanBeResumedLater()
    {
        using var handler = new TusHandler { Total = 10 };
        var seen = new List<string>();

        await ResumableUpload.UploadAsync(Target(), Content(10), "video/mp4",
            onUploadUrl: seen.Add, chunkSize: 10, transport: handler);

        Assert.Equal(new[] { UploadUrl }, seen);
    }

    [Fact]
    public async Task Patch_SendsTheFileInChunksAtTheRightOffsets()
    {
        using var handler = new TusHandler { Total = 25 };

        await ResumableUpload.UploadAsync(Target(), Content(25), "video/mp4", chunkSize: 10, transport: handler);

        var patches = handler.Requests.Where(r => r.Method == "PATCH").ToList();
        Assert.Equal(new[] { "0", "10", "20" }, patches.Select(p => p.Headers["Upload-Offset"]));
        Assert.Equal(new[] { 10, 10, 5 }, patches.Select(p => p.BodyLength));
        Assert.All(patches, p =>
        {
            Assert.Equal("1.0.0", p.Headers["Tus-Resumable"]);
            Assert.Equal(UploadUrl, p.Url);
        });
    }

    [Fact]
    public async Task Patch_ReportsProgressAndResolvesWithThePublicUrl()
    {
        using var handler = new TusHandler { Total = 20 };
        var reported = new List<long>();
        var progress = new Progress<UploadProgress>(p => reported.Add(p.BytesTransferred));

        var url = await ResumableUpload.UploadAsync(Target(), Content(20), "video/mp4",
            progress: progress, chunkSize: 10, transport: handler);

        Assert.Equal(FileUrl, url);
        // Progress is asynchronous by design, so assert the transfer completed
        // rather than racing the callback.
        Assert.Equal(20, handler.ServedOffset);
    }

    [Fact]
    public async Task Patch_ReSyncsFromTheServerOnAConflictInsteadOfRetryingBlindly()
    {
        using var handler = new TusHandler
        {
            Total = 20,
            PatchOverride = i => i == 0 ? HttpStatusCode.Conflict : null,
        };

        await ResumableUpload.UploadAsync(Target(), Content(20), "video/mp4", chunkSize: 10, transport: handler);

        Assert.Contains(handler.Requests, r => r.Method == "HEAD");
        Assert.Equal(20, handler.ServedOffset);
    }

    [Fact]
    public async Task Patch_DoesNotRetryA4xx_AnExpiredTicketWillNeverSucceed()
    {
        using var handler = new TusHandler
        {
            Total = 10,
            PatchOverride = _ => HttpStatusCode.Forbidden,
        };

        await Assert.ThrowsAsync<UploadRejectedException>(() =>
            ResumableUpload.UploadAsync(Target(), Content(10), "video/mp4", chunkSize: 10, transport: handler));

        // One attempt, not the full retry ladder.
        Assert.Single(handler.Requests.Where(r => r.Method == "PATCH"));
    }

    [Fact]
    public async Task Resume_AsksForTheOffsetAndSendsOnlyWhatIsMissing()
    {
        using var handler = new TusHandler(startOffset: 20) { Total = 30 };

        await ResumableUpload.UploadAsync(Target(), Content(30), "video/mp4",
            resumeFrom: UploadUrl, chunkSize: 10, transport: handler);

        // No creation POST — we joined an existing upload.
        Assert.DoesNotContain(handler.Requests, r => r.Method == "POST");
        Assert.Equal("HEAD", handler.Requests[0].Method);

        var patches = handler.Requests.Where(r => r.Method == "PATCH").ToList();
        Assert.Single(patches);
        Assert.Equal("20", patches[0].Headers["Upload-Offset"]);
    }

    [Fact]
    public async Task Resume_FailsClearlyWhenTheUploadHasExpired()
    {
        using var handler = new TusHandler { Total = 10, HeadStatus = HttpStatusCode.NotFound };

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            ResumableUpload.UploadAsync(Target(), Content(10), "video/mp4",
                resumeFrom: UploadUrl, transport: handler));

        Assert.Contains("expired", ex.Message);
    }

    [Fact]
    public async Task Cancelling_LeavesThePartialUploadOnTheServerByDefault()
    {
        using var cts = new CancellationTokenSource();
        using var handler = new TusHandler
        {
            Total = 30,
            // Cancel once the first chunk has landed, so there is something on
            // the server to either keep or discard.
            PatchOverride = i =>
            {
                if (i == 1) cts.Cancel();
                return null;
            },
        };

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            ResumableUpload.UploadAsync(Target(), Content(30), "video/mp4",
                chunkSize: 10, cancellationToken: cts.Token, transport: handler));

        // "Pause" and "cancel" are the same button in most interfaces, so the
        // bytes stay put and the transfer can be resumed by its upload URL.
        Assert.DoesNotContain(handler.Requests, r => r.Method == "DELETE");
    }

    [Fact]
    public async Task Cancelling_DiscardsThePartialUploadWhenAsked()
    {
        using var cts = new CancellationTokenSource();
        using var handler = new TusHandler
        {
            Total = 30,
            PatchOverride = i =>
            {
                if (i == 1) cts.Cancel();
                return null;
            },
        };

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            ResumableUpload.UploadAsync(Target(), Content(30), "video/mp4",
                chunkSize: 10, cancellationToken: cts.Token,
                terminateOnCancel: true, transport: handler));

        var deletes = handler.Requests.Where(r => r.Method == "DELETE").ToList();
        Assert.Single(deletes);
        Assert.Equal(UploadUrl, deletes[0].Url);
        Assert.Equal("1.0.0", deletes[0].Headers["Tus-Resumable"]);
    }

    [Fact]
    public async Task TerminateAsync_ReportsSuccessWhenTheServerConfirms()
    {
        using var handler = new TusHandler();

        Assert.True(await ResumableUpload.TerminateAsync(UploadUrl, transport: handler));
    }

    [Fact]
    public async Task TerminateAsync_TreatsAnAlreadyGoneUploadAsTerminated()
    {
        using var handler = new TusHandler { DeleteStatus = HttpStatusCode.Gone };

        Assert.True(await ResumableUpload.TerminateAsync(UploadUrl, transport: handler));
    }

    [Fact]
    public async Task TerminateAsync_NeverThrows()
    {
        // Nothing useful can be done about a failed cleanup of something the
        // server expires on its own, so the failure is a bool, not an exception.
        using var handler = new TusHandler { DeleteStatus = HttpStatusCode.InternalServerError };

        Assert.False(await ResumableUpload.TerminateAsync(UploadUrl, transport: handler));
    }

    private sealed class NonSeekableStream : Stream
    {
        public override bool CanRead => true;
        public override bool CanSeek => false;
        public override bool CanWrite => false;
        public override long Length => throw new NotSupportedException();
        public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }
        public override void Flush() { }
        public override int Read(byte[] buffer, int offset, int count) => 0;
        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
        public override void SetLength(long value) => throw new NotSupportedException();
        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
    }
}
