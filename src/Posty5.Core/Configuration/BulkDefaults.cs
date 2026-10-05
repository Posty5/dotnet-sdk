namespace Posty5.Core.Configuration;

/// <summary>
/// Fixed values the bulk methods (<c>CreateManyAsync</c>, <c>WaitForBulkJobAsync</c>,
/// exports) use on <c>Posty5.ShortLink</c> and <c>Posty5.QRCode</c>. Declared once
/// so both clients and the tests read the same numbers.
/// </summary>
public static class BulkDefaults
{
    /// <summary>The most rows the sync bulk routes accept in one request (BW-D3).</summary>
    public const int MaxChunkSize = 100;

    /// <summary>Rows sent per request by <c>CreateManyAsync</c> unless the caller asks for fewer.</summary>
    public const int ChunkSize = MaxChunkSize;

    /// <summary>Attempts per chunk: the first one plus one retry with the same <c>Idempotency-Key</c>.</summary>
    public const int ChunkAttempts = 2;

    /// <summary>Delay before the retry of a failed chunk.</summary>
    public static readonly TimeSpan ChunkRetryDelay = TimeSpan.FromSeconds(2);

    /// <summary>How often <c>WaitForBulkJobAsync</c> polls the job by default.</summary>
    public static readonly TimeSpan PollInterval = TimeSpan.FromSeconds(3);

    /// <summary>How long <c>WaitForBulkJobAsync</c> waits by default before giving up.</summary>
    public static readonly TimeSpan WaitTimeout = TimeSpan.FromMinutes(30);

    /// <summary>The request header that makes a retried bulk chunk create and charge nothing twice.</summary>
    public const string IdempotencyKeyHeader = "Idempotency-Key";

    /// <summary>The API path of the bulk-job resource.</summary>
    public const string BulkJobsPath = "/api/link-bulk-jobs";

    /// <summary>The sub-path of the sync bulk route under a client's base path.</summary>
    public const string BulkSubPath = "bulk";

    /// <summary>The sub-path of the export route under a client's base path.</summary>
    public const string ExportSubPath = "export";

    /// <summary>Largest HTTP status code that is not a server error; anything above is retried once.</summary>
    public const int LastClientStatusCode = 499;
}
