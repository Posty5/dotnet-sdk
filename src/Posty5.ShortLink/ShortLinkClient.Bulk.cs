using Posty5.Core.Configuration;
using Posty5.Core.Http;
using Posty5.Core.Models;
using Posty5.ShortLink.Models;

namespace Posty5.ShortLink;

/// <summary>Bulk create, export and bulk jobs for short links.</summary>
public partial class ShortLinkClient
{
    private LinkBulkOperations? _bulk;

    private LinkBulkOperations Bulk => _bulk ??= new LinkBulkOperations(_http, ShortLinkConst.BasePath, LinkBulkJobKind.ShortLinks);

    /// <summary>
    /// Creates many short links through <c>POST /api/short-link/bulk</c>, in
    /// sequential chunks of up to 100 rows. Each chunk carries
    /// <c>Idempotency-Key: {key}-{chunk}</c> and is retried once with the same key
    /// on a network error or 5xx, so a retry creates and charges nothing twice.
    /// Each created row costs the normal create cost; a refused row never blocks
    /// the others. Row numbers in the answer are 1-based positions in <paramref name="rows"/>.
    /// </summary>
    /// <remarks>
    /// Cancelling stops between chunks and returns no partial result, but the
    /// chunks already sent stay created; call again with the same
    /// <see cref="CreateManyOptions.IdempotencyKey"/> to finish without duplicates.
    /// Needs the bulk-create feature on your plan (403 otherwise).
    /// </remarks>
    /// <param name="rows">Rows to create.</param>
    /// <param name="options">Defaults, idempotency key, chunk size, progress.</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Per-row results and counts</returns>
    public Task<BulkCreateResult> CreateManyAsync(
        IReadOnlyList<ShortLinkBulkRow> rows,
        CreateManyOptions? options = null,
        CancellationToken cancellationToken = default)
        => Bulk.CreateManyAsync(rows, options, (chunk, o) => new
        {
            links = chunk,
            defaults = o.Defaults,
            fetchMetadata = o.FetchMetadata,
            createdFrom = _http.ResolveCreatedFrom(CreatedFromDefaults.Package)
        }, cancellationToken);

    /// <summary>
    /// Downloads your short links as CSV or JSON (<c>GET /api/short-link/export</c>),
    /// with the same filters and API-key scoping as <see cref="ListAsync"/>. Free.
    /// </summary>
    /// <param name="options">Format, columns, analytics range and list filters.</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The file</returns>
    public Task<FileResponse> ExportAsync(ExportOptions? options = null, CancellationToken cancellationToken = default)
        => Bulk.ExportAsync(options, cancellationToken);

    /// <summary>Submits a CSV or JSON file of up to 5,000 short links as a background job (202).</summary>
    public Task<LinkBulkJob> CreateBulkJobAsync(CreateBulkJobRequest request, CancellationToken cancellationToken = default)
        => Bulk.CreateBulkJobAsync(request, cancellationToken);

    /// <summary>Checks a job's input (<c>dryRun</c>) and answers the refused rows; nothing is created.</summary>
    public Task<BulkJobDryRunReport> ValidateBulkJobAsync(CreateBulkJobRequest request, CancellationToken cancellationToken = default)
        => Bulk.ValidateBulkJobAsync(request, cancellationToken);

    /// <summary>Lists your short-link bulk jobs.</summary>
    public Task<PaginationResponse<LinkBulkJob>> ListBulkJobsAsync(
        BulkJobListParams? listParams = null, PaginationParams? pagination = null, CancellationToken cancellationToken = default)
        => Bulk.ListBulkJobsAsync(listParams, pagination, cancellationToken);

    /// <summary>Reads one bulk job.</summary>
    public Task<LinkBulkJob> GetBulkJobAsync(string id, CancellationToken cancellationToken = default)
        => Bulk.GetBulkJobAsync(id, cancellationToken);

    /// <summary>Mints a signed link (valid 15 minutes) to a finished job's result or errors CSV.</summary>
    public Task<BulkJobResultUrl> GetBulkJobResultUrlAsync(string id, BulkJobFile file = BulkJobFile.Result, CancellationToken cancellationToken = default)
        => Bulk.GetBulkJobResultUrlAsync(id, file, cancellationToken);

    /// <summary>Cancels a queued or running job; links already created stay.</summary>
    public Task<LinkBulkJob> CancelBulkJobAsync(string id, CancellationToken cancellationToken = default)
        => Bulk.CancelBulkJobAsync(id, cancellationToken);

    /// <summary>
    /// Polls a job until it finishes (default every 3 s, up to 30 min).
    /// </summary>
    /// <exception cref="TimeoutException">Not finished within <paramref name="timeout"/>; the job keeps running.</exception>
    public Task<LinkBulkJob> WaitForBulkJobAsync(
        string id,
        TimeSpan? interval = null,
        TimeSpan? timeout = null,
        IProgress<BulkProgress>? progress = null,
        CancellationToken cancellationToken = default)
        => Bulk.WaitForBulkJobAsync(id, interval, timeout, progress, cancellationToken);
}
