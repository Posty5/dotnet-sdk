namespace Posty5.Store.Models;

/// <summary>
/// An .xlsx upload. The file travels base64-encoded in the JSON body: this API
/// has no multipart middleware, and an import-sized sheet is tens of KB.
/// </summary>
public class ExcelUploadInput
{
    /// <summary>The .xlsx file, base64-encoded.</summary>
    public string FileBase64 { get; set; } = string.Empty;

    /// <summary>Original filename, for the import report.</summary>
    public string? FileName { get; set; }
}

/// <summary>One row's outcome in a bulk create or Excel import.</summary>
public class BulkRowResult
{
    /// <summary>1-based row number in the source payload or sheet.</summary>
    public int Row { get; set; }

    /// <summary>The row's name, when it parsed far enough to have one.</summary>
    public string? Name { get; set; }

    /// <summary><c>imported</c> or <c>failed</c>.</summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>Why the row failed, empty when it imported.</summary>
    public List<string> Errors { get; set; } = new();
}

/// <summary>
/// Report returned by every bulk create and Excel import. A row that fails
/// validation is reported and skipped — it does not abort the batch.
/// </summary>
public class BulkImportReport
{
    /// <summary>Rows read from the payload or sheet.</summary>
    public int TotalRows { get; set; }

    /// <summary>Rows that were created.</summary>
    public int Imported { get; set; }

    /// <summary>Rows that were rejected.</summary>
    public int Failed { get; set; }

    /// <summary>Credits actually charged, one per created row.</summary>
    public int CreditsCharged { get; set; }

    /// <summary>Per-row outcome.</summary>
    public List<BulkRowResult> Rows { get; set; } = new();
}

/// <summary>
/// The product-specific name this report shipped under. Kept so existing code
/// keeps compiling — <see cref="BulkImportReport"/> is the same shape and is
/// what the tag import returns too.
/// </summary>
public class BulkProductsReport : BulkImportReport
{
}
