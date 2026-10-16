namespace Posty5.QRCode.Models;

/// <summary>
/// What to check (<c>POST /api/qr-code/scannability-check</c>). Set exactly one source:
/// <see cref="QrCodeId"/>, <see cref="TemplateId"/>, or <see cref="Options"/> with <see cref="Text"/>.
/// </summary>
public class QrScannabilityRequest
{
    /// <summary>One of your QR codes, as rendered (its template wins).</summary>
    public string? QrCodeId { get; set; }

    /// <summary>One of your templates or a public one.</summary>
    public string? TemplateId { get; set; }

    /// <summary>An unsaved design; needs <see cref="Text"/>.</summary>
    public QRCodeOptionsModel? Options { get; set; }

    /// <summary>
    /// The content to encode: required with <see cref="Options"/>; with
    /// <see cref="TemplateId"/> it defaults to a representative QR URL.
    /// </summary>
    public string? Text { get; set; }

    /// <summary>Print size to judge the module size at, see <see cref="QrPrintPresetIds"/>.</summary>
    public string? PresetId { get; set; }

    /// <summary>Width in mm for <see cref="QrPrintPresetIds.Custom"/>.</summary>
    public double? CustomMm { get; set; }

    /// <summary>PNG width in px to judge the module size at.</summary>
    public int? SizePx { get; set; }
}

/// <summary>
/// A scannability report. It only warns: nothing is blocked by a poor verdict.
/// </summary>
public class QrScannabilityReportModel
{
    /// <summary>The worst check status: <c>good</c>, <c>warning</c> or <c>poor</c>, see <see cref="QrScannabilityStatuses"/>.</summary>
    public string Verdict { get; set; } = string.Empty;

    /// <summary>Each check, with its value, thresholds and a fix hint.</summary>
    public List<QrScannabilityCheckModel> Checks { get; set; } = new();

    /// <summary>The QR symbol the design produces.</summary>
    public QrScannabilitySymbolModel? Symbol { get; set; }
}

/// <summary>One check of a <see cref="QrScannabilityReportModel"/>.</summary>
public class QrScannabilityCheckModel
{
    /// <summary>What was measured, see <see cref="QrScannabilityCheckCodes"/>.</summary>
    public string Code { get; set; } = string.Empty;

    /// <summary><c>good</c>, <c>warning</c> or <c>poor</c>, see <see cref="QrScannabilityStatuses"/>.</summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>The measured value.</summary>
    public double? Value { get; set; }

    /// <summary>The thresholds the value was judged against.</summary>
    public QrScannabilityThresholdsModel? Thresholds { get; set; }

    /// <summary>i18n key of the message: <c>qrScannability.&lt;code&gt;.&lt;status&gt;</c>.</summary>
    public string? MessageKey { get; set; }

    /// <summary>The message in English.</summary>
    public string? Message { get; set; }
}

/// <summary>The thresholds of a <see cref="QrScannabilityCheckModel"/>; null where a level does not apply.</summary>
public class QrScannabilityThresholdsModel
{
    /// <summary>Beyond this the check warns.</summary>
    public double? Warning { get; set; }

    /// <summary>Beyond this the check is poor.</summary>
    public double? Poor { get; set; }
}

/// <summary>The QR symbol a design produces.</summary>
public class QrScannabilitySymbolModel
{
    /// <summary>QR version (1-40).</summary>
    public int? Version { get; set; }

    /// <summary>Modules per side.</summary>
    public int? Size { get; set; }

    /// <summary>Error-correction level: <c>L</c>, <c>M</c>, <c>Q</c> or <c>H</c>.</summary>
    public string? EcLevel { get; set; }

    /// <summary>Quiet zone in modules.</summary>
    public double? QuietZoneModules { get; set; }

    /// <summary>Module edge in mm at the requested print size.</summary>
    public double? ModuleSizeMm { get; set; }
}
