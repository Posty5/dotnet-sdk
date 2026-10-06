using System.Text.Json.Serialization;
using Posty5.Core.Converts;

namespace Posty5.Core.Models;

// Shared by Posty5.ShortLink and Posty5.QRCode: the bulk answer, the bulk-job
// resource and the export options are the same for both kinds (BW contract,
// IBulkCreateResult / ILinkBulkJob). Row types live in each package.

/// <summary>Values applied to every bulk row that does not set its own (<c>IBulkDefaults</c>).</summary>
public class BulkCreateDefaults
{
    /// <summary>Template for rows without one. API-key callers need this or a per-row <c>TemplateId</c>.</summary>
    public string? TemplateId { get; set; }

    /// <summary>Tag for rows without one.</summary>
    public string? Tag { get; set; }

    /// <summary>Your own reference for rows without one.</summary>
    public string? RefId { get; set; }
}

/// <summary>One field error on a refused bulk row.</summary>
public class BulkRowError
{
    /// <summary>The row field the error is about, when it is about one.</summary>
    public string? Field { get; set; }

    /// <summary>Why the row was refused.</summary>
    public string Message { get; set; } = string.Empty;
}

/// <summary>Whether a bulk row was created.</summary>
[JsonConverter(typeof(CamelCaseEnumConverter<BulkRowStatus>))]
public enum BulkRowStatus
{
    /// <summary>The row was created (or had already been created by a retried request).</summary>
    Created,

    /// <summary>The row was refused; see <see cref="BulkRowResult.Errors"/>.</summary>
    Failed
}

/// <summary>The answer for one row (<c>IBulkRowResult</c>).</summary>
public class BulkRowResult
{
    /// <summary>1-based position of the row in the input you passed (the header is row 0).</summary>
    public int Row { get; set; }

    /// <summary>Created or failed.</summary>
    public BulkRowStatus Status { get; set; }

    /// <summary>Id of the created record.</summary>
    public string? Id { get; set; }

    /// <summary>Short URL of the created record.</summary>
    public string? ShortUrl { get; set; }

    /// <summary>Image download URL of a created QR code.</summary>
    [JsonPropertyName("qrCodeDownloadURL")]
    public string? QrCodeDownloadUrl { get; set; }

    /// <summary>Why the row was refused.</summary>
    public List<BulkRowError>? Errors { get; set; }
}

/// <summary>The answer of a bulk create (<c>IBulkCreateResult</c>).</summary>
public class BulkCreateResult
{
    /// <summary>Rows created.</summary>
    public int Created { get; set; }

    /// <summary>Rows refused.</summary>
    public int Failed { get; set; }

    /// <summary>One result per row, numbered over the whole input.</summary>
    public List<BulkRowResult> Items { get; set; } = new();
}

/// <summary>Progress of <c>CreateManyAsync</c> or <c>WaitForBulkJobAsync</c>.</summary>
public class BulkProgress
{
    /// <summary>Rows handled so far.</summary>
    public int Processed { get; set; }

    /// <summary>Rows in total, when known.</summary>
    public int? Total { get; set; }

    /// <summary>Rows created so far.</summary>
    public int Created { get; set; }

    /// <summary>Rows refused so far.</summary>
    public int Failed { get; set; }
}

/// <summary>Options of <c>CreateManyAsync</c>.</summary>
public class CreateManyOptions
{
    /// <summary>Values for rows that do not set their own.</summary>
    public BulkCreateDefaults? Defaults { get; set; }

    /// <summary>
    /// Base of the <c>Idempotency-Key</c>; chunk <c>n</c> is sent as <c>{key}-{n}</c>.
    /// Pass the same key to re-run a call safely; null generates a new one per call.
    /// </summary>
    public string? IdempotencyKey { get; set; }

    /// <summary>Rows per request, 1-100. Defaults to <see cref="Configuration.BulkDefaults.ChunkSize"/>.</summary>
    public int? ChunkSize { get; set; }

    /// <summary>Short links only: fill title and deep links in the background (default true on the API).</summary>
    public bool? FetchMetadata { get; set; }

    /// <summary>Reported after each chunk.</summary>
    public IProgress<BulkProgress>? Progress { get; set; }
}

/// <summary>Export file format.</summary>
[JsonConverter(typeof(CamelCaseEnumConverter<ExportFormat>))]
public enum ExportFormat
{
    /// <summary>CSV (spreadsheet-safe cells).</summary>
    Csv,

    /// <summary>JSON array.</summary>
    Json
}

/// <summary>Options of <c>ExportAsync</c>. List filters go in <see cref="Filters"/>.</summary>
public class ExportOptions
{
    /// <summary>CSV or JSON.</summary>
    public ExportFormat Format { get; set; } = ExportFormat.Csv;

    /// <summary>Column keys to include (unknown keys are ignored); null for all.</summary>
    public List<string>? Columns { get; set; }

    /// <summary>The same filters the list accepts (e.g. <c>tag</c>, <c>refId</c>), by API name.</summary>
    public Dictionary<string, object?>? Filters { get; set; }
}

/// <summary>What a bulk job creates.</summary>
[JsonConverter(typeof(CamelCaseEnumConverter<LinkBulkJobKind>))]
public enum LinkBulkJobKind
{
    /// <summary>Short links.</summary>
    ShortLinks,

    /// <summary>QR codes.</summary>
    QrCodes
}

/// <summary>State of a bulk job.</summary>
[JsonConverter(typeof(CamelCaseEnumConverter<LinkBulkJobStatus>))]
public enum LinkBulkJobStatus
{
    /// <summary>Waiting to start.</summary>
    Queued,
    /// <summary>Creating rows.</summary>
    Running,
    /// <summary>Every row created.</summary>
    Succeeded,
    /// <summary>Some rows refused.</summary>
    PartiallySucceeded,
    /// <summary>The job failed.</summary>
    Failed,
    /// <summary>Cancelled by its owner.</summary>
    Cancelled
}

/// <summary>A downloadable file of a finished job.</summary>
[JsonConverter(typeof(CamelCaseEnumConverter<BulkJobFile>))]
public enum BulkJobFile
{
    /// <summary>CSV mapping every input row to its record or error.</summary>
    Result,
    /// <summary>CSV of the refused rows only.</summary>
    Errors,
    /// <summary>QR jobs: ZIP of the images.</summary>
    Zip
}

/// <summary>Input format of a bulk job.</summary>
[JsonConverter(typeof(CamelCaseEnumConverter<BulkJobInputFormat>))]
public enum BulkJobInputFormat
{
    /// <summary>UTF-8 CSV with a header row.</summary>
    Csv,
    /// <summary>JSON array of rows.</summary>
    Json
}

/// <summary>Image format of a QR job's ZIP.</summary>
[JsonConverter(typeof(CamelCaseEnumConverter<BulkImageFormat>))]
public enum BulkImageFormat
{
    /// <summary>PNG (default).</summary>
    Png,
    /// <summary>SVG (needs the vector-export feature).</summary>
    Svg,
    /// <summary>PDF (needs the vector-export feature).</summary>
    Pdf
}

/// <summary>Image settings of a QR job.</summary>
public class BulkJobImageOptions
{
    /// <summary>Image format.</summary>
    public BulkImageFormat Format { get; set; } = BulkImageFormat.Png;

    /// <summary>Image size in pixels (10-2000); null for the template's width.</summary>
    public int? SizePx { get; set; }
}

/// <summary>Job options.</summary>
public class BulkJobOptions
{
    /// <summary>Short links: fill metadata in the background.</summary>
    public bool? FetchMetadata { get; set; }

    /// <summary>QR codes: image settings for the ZIP.</summary>
    public BulkJobImageOptions? Image { get; set; }
}

/// <summary>A bulk job submission.</summary>
public class CreateBulkJobRequest
{
    /// <summary>Format of <see cref="Content"/>.</summary>
    public BulkJobInputFormat Format { get; set; } = BulkJobInputFormat.Csv;

    /// <summary>The file's text (up to 5,000 rows / 2 MB).</summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>Original file name, for your records.</summary>
    public string? FileName { get; set; }

    /// <summary>Values for rows that do not set their own.</summary>
    public BulkCreateDefaults? Defaults { get; set; }

    /// <summary>Job options.</summary>
    public BulkJobOptions? Options { get; set; }

    /// <summary>Same key + same account answers the same job instead of a second one.</summary>
    [JsonIgnore]
    public string? IdempotencyKey { get; set; }
}

/// <summary>Where a job's input came from.</summary>
public class LinkBulkJobSource
{
    /// <summary>CSV or JSON.</summary>
    public BulkJobInputFormat Format { get; set; }
    /// <summary>File name, if given.</summary>
    public string? FileName { get; set; }
    /// <summary>Data rows.</summary>
    public int RowCount { get; set; }
}

/// <summary>Job counters.</summary>
public class LinkBulkJobProgress
{
    /// <summary>Rows handled.</summary>
    public int Processed { get; set; }
    /// <summary>Rows created.</summary>
    public int Created { get; set; }
    /// <summary>Rows refused.</summary>
    public int Failed { get; set; }
}

/// <summary>Which result files exist.</summary>
public class LinkBulkJobFiles
{
    /// <summary>Result CSV.</summary>
    public bool? Result { get; set; }
    /// <summary>Errors CSV.</summary>
    public bool? Errors { get; set; }
    /// <summary>ZIP of QR images.</summary>
    public bool? Zip { get; set; }
}

/// <summary>A bulk job (<c>ILinkBulkJob</c>).</summary>
public class LinkBulkJob
{
    /// <summary>Job id.</summary>
    [JsonPropertyName("_id")]
    public string Id { get; set; } = string.Empty;
    /// <summary>Short links or QR codes.</summary>
    public LinkBulkJobKind Kind { get; set; }
    /// <summary>State.</summary>
    public LinkBulkJobStatus Status { get; set; }
    /// <summary>Input description.</summary>
    public LinkBulkJobSource Source { get; set; } = new();
    /// <summary>Counters.</summary>
    public LinkBulkJobProgress Progress { get; set; } = new();
    /// <summary>Defaults the job applies.</summary>
    public BulkCreateDefaults? Defaults { get; set; }
    /// <summary>Options the job runs with.</summary>
    public BulkJobOptions? Options { get; set; }
    /// <summary>Which files exist.</summary>
    public LinkBulkJobFiles Files { get; set; } = new();
    /// <summary>When the files are deleted.</summary>
    public DateTime? FilesExpireAt { get; set; }
    /// <summary>Origin label.</summary>
    public string? CreatedFrom { get; set; }
    /// <summary>Submitted at.</summary>
    public DateTime CreatedAt { get; set; }
    /// <summary>Last change.</summary>
    public DateTime? UpdatedAt { get; set; }
    /// <summary>When a worker picked the job up.</summary>
    public DateTime? StartedAt { get; set; }
    /// <summary>Why a <c>failed</c> job stopped.</summary>
    public string? FailureReason { get; set; }
    /// <summary>Finished at.</summary>
    public DateTime? FinishedAt { get; set; }

    /// <summary>True once the job will change no more.</summary>
    [JsonIgnore]
    public bool IsFinished => Status is LinkBulkJobStatus.Succeeded or LinkBulkJobStatus.PartiallySucceeded
        or LinkBulkJobStatus.Failed or LinkBulkJobStatus.Cancelled;
}

/// <summary>The answer of a dry run: what the job would refuse, with nothing created.</summary>
public class BulkJobDryRunReport
{
    /// <summary>Data rows read.</summary>
    public int RowCount { get; set; }
    /// <summary>Rows that would be created.</summary>
    public int Valid { get; set; }
    /// <summary>The first 200 refused rows.</summary>
    public List<BulkRowResult> Errors { get; set; } = new();
    /// <summary>Non-fatal notes, e.g. ignored unknown columns.</summary>
    public List<string> Warnings { get; set; } = new();
}

/// <summary>A signed, expiring download link of a job file.</summary>
public class BulkJobResultUrl
{
    /// <summary>Pre-signed GET URL.</summary>
    public string Url { get; set; } = string.Empty;
    /// <summary>When it stops working (15 minutes; ask again for a new one).</summary>
    public DateTime ExpiresAt { get; set; }
}

/// <summary>Filters for the job list.</summary>
public class BulkJobListParams
{
    /// <summary>Only jobs in this state.</summary>
    public LinkBulkJobStatus? Status { get; set; }
}
