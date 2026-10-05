using Posty5.Core.Configuration;
using Posty5.Core.Http;
using Posty5.Core.Models;
using Posty5.QRCode.Models;

namespace Posty5.QRCode;

/// <summary>Bulk create, export and bulk jobs (with a ZIP of images) for QR codes.</summary>
public partial class QRCodeClient
{
    private LinkBulkOperations? _bulk;

    private LinkBulkOperations Bulk => _bulk ??= new LinkBulkOperations(_http, BasePath, LinkBulkJobKind.QrCodes);

    /// <summary>
    /// Creates many QR codes through <c>POST /api/qr-code/bulk</c>, in sequential
    /// chunks of up to 100 rows, each with <c>Idempotency-Key: {key}-{chunk}</c> and
    /// one retry with the same key on a network error or 5xx. Each created row
    /// costs the normal create cost. Row numbers are 1-based positions in <paramref name="rows"/>.
    /// </summary>
    /// <remarks>
    /// Cancelling stops between chunks and returns no partial result; chunks already
    /// sent stay created. A static QR code encodes its content directly and produces
    /// no scan events. Needs the bulk-generate feature on your plan (403 otherwise).
    /// </remarks>
    public Task<BulkCreateResult> CreateManyAsync(
        IReadOnlyList<QRCodeBulkRow> rows,
        CreateManyOptions? options = null,
        CancellationToken cancellationToken = default)
        => Bulk.CreateManyAsync(rows, options, (chunk, o) => new
        {
            items = chunk,
            defaults = o.Defaults,
            createdFrom = _http.ResolveCreatedFrom(CreatedFromDefaults.Package)
        }, cancellationToken);

    /// <summary>
    /// Downloads your QR codes as CSV or JSON (<c>GET /api/qr-code/export</c>),
    /// with the list's filters and API-key scoping. Free.
    /// </summary>
    public Task<FileResponse> ExportAsync(ExportOptions? options = null, CancellationToken cancellationToken = default)
        => Bulk.ExportAsync(options, cancellationToken);

    /// <summary>Submits a CSV or JSON file of up to 5,000 QR rows as a background job (202); the job also builds a ZIP.</summary>
    public Task<LinkBulkJob> CreateBulkJobAsync(CreateBulkJobRequest request, CancellationToken cancellationToken = default)
        => Bulk.CreateBulkJobAsync(request, cancellationToken);

    /// <summary>Checks a job's input (<c>dryRun</c>) and answers the refused rows; nothing is created.</summary>
    public Task<BulkJobDryRunReport> ValidateBulkJobAsync(CreateBulkJobRequest request, CancellationToken cancellationToken = default)
        => Bulk.ValidateBulkJobAsync(request, cancellationToken);

    /// <summary>Lists your QR bulk jobs.</summary>
    public Task<PaginationResponse<LinkBulkJob>> ListBulkJobsAsync(
        BulkJobListParams? listParams = null, PaginationParams? pagination = null, CancellationToken cancellationToken = default)
        => Bulk.ListBulkJobsAsync(listParams, pagination, cancellationToken);

    /// <summary>Reads one bulk job.</summary>
    public Task<LinkBulkJob> GetBulkJobAsync(string id, CancellationToken cancellationToken = default)
        => Bulk.GetBulkJobAsync(id, cancellationToken);

    /// <summary>Mints a signed link (valid 15 minutes) to a finished job's result CSV, errors CSV or ZIP.</summary>
    public Task<BulkJobResultUrl> GetBulkJobResultUrlAsync(string id, BulkJobFile file = BulkJobFile.Zip, CancellationToken cancellationToken = default)
        => Bulk.GetBulkJobResultUrlAsync(id, file, cancellationToken);

    /// <summary>Cancels a queued or running job; QR codes already created stay.</summary>
    public Task<LinkBulkJob> CancelBulkJobAsync(string id, CancellationToken cancellationToken = default)
        => Bulk.CancelBulkJobAsync(id, cancellationToken);

    /// <summary>Polls a job until it finishes (default every 3 s, up to 30 min).</summary>
    /// <exception cref="TimeoutException">Not finished within <paramref name="timeout"/>; the job keeps running.</exception>
    public Task<LinkBulkJob> WaitForBulkJobAsync(
        string id,
        TimeSpan? interval = null,
        TimeSpan? timeout = null,
        IProgress<BulkProgress>? progress = null,
        CancellationToken cancellationToken = default)
        => Bulk.WaitForBulkJobAsync(id, interval, timeout, progress, cancellationToken);
}
