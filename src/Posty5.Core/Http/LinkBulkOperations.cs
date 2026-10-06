using System.Text.Json;
using Posty5.Core.Configuration;
using Posty5.Core.Exceptions;
using Posty5.Core.Models;

namespace Posty5.Core.Http;

/// <summary>
/// The bulk mechanics <c>ShortLinkClient</c> and <c>QRCodeClient</c> share:
/// chunked sync create with per-chunk idempotency keys, export, and the
/// <c>/api/link-bulk-jobs</c> resource for one job kind. Each client wraps it in
/// its own typed methods; call those rather than this class.
/// </summary>
public sealed class LinkBulkOperations
{
    private readonly Posty5HttpClient _http;
    private readonly string _basePath;
    private readonly LinkBulkJobKind _kind;

    /// <summary>Creates the helper for one client.</summary>
    /// <param name="http">The client's HTTP transport.</param>
    /// <param name="basePath">The client's API path, e.g. <c>/api/short-link</c>.</param>
    /// <param name="kind">The job kind this client submits.</param>
    public LinkBulkOperations(Posty5HttpClient http, string basePath, LinkBulkJobKind kind)
    {
        _http = http ?? throw new ArgumentNullException(nameof(http));
        _basePath = basePath;
        _kind = kind;
    }

    /// <summary>
    /// Sends <paramref name="rows"/> in sequential chunks to <c>{basePath}/bulk</c>,
    /// each with <c>Idempotency-Key: {key}-{chunk}</c>, retrying a chunk once with
    /// the same key on a network error or 5xx, and renumbers every row result to
    /// its position in <paramref name="rows"/> (1-based).
    /// </summary>
    /// <typeparam name="TRow">The row type.</typeparam>
    /// <param name="rows">Rows to create.</param>
    /// <param name="options">Chunking, defaults, key and progress.</param>
    /// <param name="buildBody">Builds the request body of one chunk.</param>
    /// <param name="cancellationToken">Checked between chunks and during the retry delay.</param>
    /// <returns>The combined result.</returns>
    public async Task<BulkCreateResult> CreateManyAsync<TRow>(
        IReadOnlyList<TRow> rows,
        CreateManyOptions? options,
        Func<IReadOnlyList<TRow>, CreateManyOptions, object> buildBody,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(rows);
        options ??= new CreateManyOptions();

        var chunkSize = options.ChunkSize ?? BulkDefaults.ChunkSize;
        if (chunkSize < 1 || chunkSize > BulkDefaults.MaxChunkSize)
            throw new ArgumentOutOfRangeException(nameof(options), $"ChunkSize must be 1-{BulkDefaults.MaxChunkSize}.");

        var key = string.IsNullOrWhiteSpace(options.IdempotencyKey) ? Guid.NewGuid().ToString("N") : options.IdempotencyKey;
        var result = new BulkCreateResult();
        var path = $"{_basePath}/{BulkDefaults.BulkSubPath}";

        for (int offset = 0, chunk = 0; offset < rows.Count; offset += chunkSize, chunk++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var slice = rows.Skip(offset).Take(chunkSize).ToList();
            var headers = new Dictionary<string, string> { [BulkDefaults.IdempotencyKeyHeader] = $"{key}-{chunk}" };
            var chunkResult = await PostChunkAsync(path, buildBody(slice, options), headers, cancellationToken);

            foreach (var item in chunkResult.Items)
            {
                item.Row += offset;
                result.Items.Add(item);
            }
            result.Created += chunkResult.Created;
            result.Failed += chunkResult.Failed;

            options.Progress?.Report(new BulkProgress
            {
                Processed = Math.Min(offset + slice.Count, rows.Count),
                Total = rows.Count,
                Created = result.Created,
                Failed = result.Failed
            });
        }

        return result;
    }

    private async Task<BulkCreateResult> PostChunkAsync(
        string path, object body, IDictionary<string, string> headers, CancellationToken cancellationToken)
    {
        for (var attempt = 1; ; attempt++)
        {
            try
            {
                var response = await _http.PostAsync<BulkCreateResult>(path, body, headers, cancellationToken);
                return response.Result ?? new BulkCreateResult();
            }
            catch (Posty5Exception ex) when (attempt < BulkDefaults.ChunkAttempts && IsRetryable(ex, cancellationToken))
            {
                await Task.Delay(BulkDefaults.ChunkRetryDelay, cancellationToken);
            }
        }
    }

    /// <summary>A network failure or a 5xx is retried; a 4xx and a caller's cancellation are not.</summary>
    internal static bool IsRetryable(Posty5Exception ex, CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested) return false;
        if (ex.StatusCode is int status) return status > BulkDefaults.LastClientStatusCode;
        return ex.InnerException is HttpRequestException or TaskCanceledException;
    }

    /// <summary>Downloads <c>{basePath}/export</c> as CSV or JSON.</summary>
    public Task<FileResponse> ExportAsync(ExportOptions? options, CancellationToken cancellationToken)
    {
        options ??= new ExportOptions();
        var query = options.Filters != null
            ? new Dictionary<string, object?>(options.Filters)
            : new Dictionary<string, object?>();

        query["format"] = EnumName(options.Format);
        if (options.Columns is { Count: > 0 }) query["columns"] = string.Join(",", options.Columns);

        return _http.GetBytesAsync($"{_basePath}/{BulkDefaults.ExportSubPath}", query, cancellationToken);
    }

    /// <summary>Submits a job of this client's kind (202).</summary>
    public async Task<LinkBulkJob> CreateBulkJobAsync(CreateBulkJobRequest request, CancellationToken cancellationToken)
    {
        var response = await _http.PostAsync<LinkBulkJob>(BulkDefaults.BulkJobsPath, JobBody(request, false), KeyHeader(request), cancellationToken);
        return response.Result ?? throw new Posty5Exception("The API answered no bulk job.");
    }

    /// <summary>Validates a job's input without creating anything.</summary>
    public async Task<BulkJobDryRunReport> ValidateBulkJobAsync(CreateBulkJobRequest request, CancellationToken cancellationToken)
    {
        var response = await _http.PostAsync<BulkJobDryRunReport>(BulkDefaults.BulkJobsPath, JobBody(request, true), cancellationToken);
        return response.Result ?? new BulkJobDryRunReport();
    }

    /// <summary>Lists this kind's jobs, newest first.</summary>
    public async Task<PaginationResponse<LinkBulkJob>> ListBulkJobsAsync(
        BulkJobListParams? listParams, PaginationParams? pagination, CancellationToken cancellationToken)
    {
        var query = new Dictionary<string, object?> { ["kind"] = EnumName(_kind) };
        if (listParams?.Status is LinkBulkJobStatus status) query["status"] = EnumName(status);
        if (pagination != null)
        {
            if (!string.IsNullOrEmpty(pagination.Cursor)) query["cursor"] = pagination.Cursor;
            query["pageSize"] = pagination.PageSize;
        }

        var response = await _http.GetAsync<PaginationResponse<LinkBulkJob>>(BulkDefaults.BulkJobsPath, query, cancellationToken);
        return response.Result ?? new PaginationResponse<LinkBulkJob>();
    }

    /// <summary>Reads one job.</summary>
    public async Task<LinkBulkJob> GetBulkJobAsync(string id, CancellationToken cancellationToken)
    {
        RequireId(id);
        var response = await _http.GetAsync<LinkBulkJob>($"{BulkDefaults.BulkJobsPath}/{Uri.EscapeDataString(id)}", cancellationToken: cancellationToken);
        return response.Result ?? throw new Posty5Exception("The API answered no bulk job.");
    }

    /// <summary>Mints a 15-minute signed link to one of a job's files.</summary>
    public async Task<BulkJobResultUrl> GetBulkJobResultUrlAsync(string id, BulkJobFile file, CancellationToken cancellationToken)
    {
        RequireId(id);
        var response = await _http.GetAsync<BulkJobResultUrl>(
            $"{BulkDefaults.BulkJobsPath}/{Uri.EscapeDataString(id)}/result-url",
            new Dictionary<string, object?> { ["file"] = EnumName(file) },
            cancellationToken);
        return response.Result ?? throw new Posty5Exception("The API answered no result link.");
    }

    /// <summary>Cancels a queued or running job; rows already created stay.</summary>
    public async Task<LinkBulkJob> CancelBulkJobAsync(string id, CancellationToken cancellationToken)
    {
        RequireId(id);
        var response = await _http.PostAsync<LinkBulkJob>($"{BulkDefaults.BulkJobsPath}/{Uri.EscapeDataString(id)}/cancel", new { }, cancellationToken);
        return response.Result ?? throw new Posty5Exception("The API answered no bulk job.");
    }

    /// <summary>Polls a job until it finishes, reporting progress after each poll.</summary>
    /// <exception cref="TimeoutException">The job did not finish within <paramref name="timeout"/>.</exception>
    public async Task<LinkBulkJob> WaitForBulkJobAsync(
        string id, TimeSpan? interval, TimeSpan? timeout, IProgress<BulkProgress>? progress, CancellationToken cancellationToken)
    {
        var pollEvery = interval ?? BulkDefaults.PollInterval;
        var deadline = DateTime.UtcNow + (timeout ?? BulkDefaults.WaitTimeout);

        while (true)
        {
            var job = await GetBulkJobAsync(id, cancellationToken);
            progress?.Report(new BulkProgress
            {
                Processed = job.Progress.Processed,
                Total = job.Source.RowCount,
                Created = job.Progress.Created,
                Failed = job.Progress.Failed
            });

            if (job.IsFinished) return job;
            if (DateTime.UtcNow + pollEvery > deadline)
                throw new TimeoutException($"Bulk job {id} did not finish in time; it is still {EnumName(job.Status)}.");

            await Task.Delay(pollEvery, cancellationToken);
        }
    }

    private object JobBody(CreateBulkJobRequest request, bool dryRun)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (string.IsNullOrEmpty(request.Content))
            throw new ArgumentException("Content is required.", nameof(request));

        return new
        {
            kind = _kind,
            format = request.Format,
            content = request.Content,
            fileName = request.FileName,
            defaults = request.Defaults,
            options = request.Options,
            dryRun = dryRun ? true : (bool?)null
        };
    }

    private static Dictionary<string, string>? KeyHeader(CreateBulkJobRequest request) =>
        string.IsNullOrWhiteSpace(request.IdempotencyKey)
            ? null
            : new Dictionary<string, string> { [BulkDefaults.IdempotencyKeyHeader] = request.IdempotencyKey };

    private static void RequireId(string id)
    {
        if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException("A job id is required.", nameof(id));
    }

    private static string EnumName<TEnum>(TEnum value) where TEnum : struct, Enum =>
        JsonNamingPolicy.CamelCase.ConvertName(value.ToString());
}
