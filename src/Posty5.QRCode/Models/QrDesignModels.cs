using System.Text.Json;
using System.Text.Json.Serialization;

namespace Posty5.QRCode.Models;

/// <summary>
/// The fill of a v2 design's data modules: one solid colour or a 2-3 stop gradient.
/// </summary>
public class QrFillModel
{
    /// <summary><c>solid</c>, <c>linear</c> or <c>radial</c>, see <see cref="QrFillTypes"/>.</summary>
    public string Type { get; set; } = QrFillTypes.Solid;

    /// <summary>Gradient angle in degrees, 0-360 (linear only).</summary>
    public double? Rotation { get; set; }

    /// <summary>One stop for <c>solid</c>, 2-3 for a gradient.</summary>
    public List<QrFillStopModel> Stops { get; set; } = new();
}

/// <summary>One colour stop of a <see cref="QrFillModel"/>.</summary>
public class QrFillStopModel
{
    /// <summary>Position along the gradient, 0-1.</summary>
    public double Offset { get; set; }

    /// <summary><c>#rgb</c>, <c>#rrggbb</c>, <c>#rrggbbaa</c>, <c>rgb()</c> or <c>rgba()</c>.</summary>
    public string Color { get; set; } = string.Empty;
}

/// <summary>A v2 design's frame: a catalogue frame with an optional call to action.</summary>
public class QrFrameModel
{
    /// <summary>The frame, see <see cref="QrFrameIds"/>.</summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>The call to action as shown (at most 32 characters; not an i18n key).</summary>
    public string? Text { get; set; }

    /// <summary>Frame colour; the catalogue's default when null.</summary>
    public string? Color { get; set; }

    /// <summary>Text colour; the catalogue's default when null.</summary>
    public string? TextColor { get; set; }
}

/// <summary>
/// What to render: format, print size and resolution. Every property is optional;
/// an empty options object renders the plain PNG.
/// </summary>
/// <remarks>
/// SVG, PDF, a print preset, a DPI or a PNG over 2048 px need vector export on the
/// account's plan; otherwise the API answers 403 <c>This feature is not available
/// on your current plan.</c> Plain PNG works on every plan. Rendering is free.
/// </remarks>
public class QrExportUrlOptions
{
    /// <summary><c>png</c> (default), <c>svg</c> or <c>pdf</c>, see <see cref="QrRenderFormats"/>.</summary>
    public string? Format { get; set; }

    /// <summary>Print size, see <see cref="QrPrintPresetIds"/>. A PDF defaults to <c>flyer-5cm</c>.</summary>
    public string? PresetId { get; set; }

    /// <summary>Width in mm (20-500) for <see cref="QrPrintPresetIds.Custom"/>.</summary>
    public double? CustomMm { get; set; }

    /// <summary>PNG resolution at a preset: 150, 300 or 600 (needs <see cref="PresetId"/>).</summary>
    public int? Dpi { get; set; }

    /// <summary>PNG / SVG width in px, 64-6000. Over 2048 counts as print export.</summary>
    public int? SizePx { get; set; }
}

/// <summary>
/// <see cref="QrExportUrlOptions"/> plus <see cref="Download"/>, for the render methods
/// that answer with the file itself.
/// </summary>
public class QrRenderOptions : QrExportUrlOptions
{
    /// <summary>
    /// Ask for the file as an attachment; <see cref="Posty5.Core.Models.FileResponse.FileName"/>
    /// is then <c>&lt;name&gt;-&lt;preset|size&gt;.&lt;ext&gt;</c>.
    /// </summary>
    public bool Download { get; set; }
}

/// <summary>A signed download link for a QR code image.</summary>
public class QrExportUrlModel
{
    /// <summary>The link; it needs no API key and works until <see cref="ExpiresAt"/>.</summary>
    public string Url { get; set; } = string.Empty;

    /// <summary>When the link stops working (1 hour after it was minted).</summary>
    public DateTime? ExpiresAt { get; set; }
}

/// <summary>An entry of a <see cref="QrDesignCatalogueModel"/> list.</summary>
public class QrCatalogueItemModel
{
    /// <summary>The value to store in the design (a shape, frame or preset id).</summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>The i18n key of the entry's label.</summary>
    public string? LabelKey { get; set; }

    /// <summary>A small SVG preview, when the catalogue has one.</summary>
    public string? PreviewSvg { get; set; }

    /// <summary>Fields this class does not name.</summary>
    [JsonExtensionData]
    public Dictionary<string, JsonElement>? AdditionalProperties { get; set; }
}

/// <summary>A frame in the design catalogue.</summary>
public class QrFrameCatalogueItemModel : QrCatalogueItemModel
{
    /// <summary>Whether the frame shows <see cref="QrFrameModel.Text"/>.</summary>
    public bool? HasText { get; set; }

    /// <summary>Frame colour used when the design sets none.</summary>
    public string? DefaultColor { get; set; }

    /// <summary>Text colour used when the design sets none.</summary>
    public string? DefaultTextColor { get; set; }
}

/// <summary>A call-to-action preset in the design catalogue.</summary>
public class QrCtaPresetCatalogueItemModel : QrCatalogueItemModel
{
    /// <summary>The text by language code (<c>en</c>, <c>ar</c>), ready for <see cref="QrFrameModel.Text"/>.</summary>
    public Dictionary<string, string>? Text { get; set; }
}

/// <summary>A print preset in the design catalogue.</summary>
public class QrPrintPresetCatalogueItemModel : QrCatalogueItemModel
{
    /// <summary>Printed width in mm (fixed presets).</summary>
    public double? Mm { get; set; }

    /// <summary>Smallest width in mm (<c>custom</c>).</summary>
    public double? MinMm { get; set; }

    /// <summary>Largest width in mm (<c>custom</c>).</summary>
    public double? MaxMm { get; set; }
}

/// <summary>The design limits the API enforces.</summary>
public class QrDesignLimitsModel
{
    /// <summary>Smallest stored width in px.</summary>
    public int? WidthMin { get; set; }

    /// <summary>Largest stored width in px.</summary>
    public int? WidthMax { get; set; }

    /// <summary>Largest raster edge in px (6000).</summary>
    public int? MaxRasterEdgePx { get; set; }

    /// <summary>The accepted PNG DPI values.</summary>
    public List<int>? DpiValues { get; set; }

    /// <summary>The DPI used when none is given.</summary>
    public int? DpiDefault { get; set; }

    /// <summary>Longest frame text.</summary>
    public int? FrameTextMaxChars { get; set; }

    /// <summary>Fewest gradient stops.</summary>
    public int? FillStopsMin { get; set; }

    /// <summary>Most gradient stops.</summary>
    public int? FillStopsMax { get; set; }

    /// <summary>Limits this class does not name.</summary>
    [JsonExtensionData]
    public Dictionary<string, JsonElement>? AdditionalProperties { get; set; }
}

/// <summary>
/// The design catalogue (<c>GET /api/qr-code-template/design-catalogue</c>): the frames,
/// call-to-action presets, print presets and shapes a design may use, and the limits.
/// </summary>
public class QrDesignCatalogueModel
{
    /// <summary>Frames.</summary>
    public List<QrFrameCatalogueItemModel> Frames { get; set; } = new();

    /// <summary>Call-to-action presets.</summary>
    public List<QrCtaPresetCatalogueItemModel> CtaPresets { get; set; } = new();

    /// <summary>Print presets.</summary>
    public List<QrPrintPresetCatalogueItemModel> PrintPresets { get; set; } = new();

    /// <summary>Data module shapes.</summary>
    public List<QrCatalogueItemModel> DotShapes { get; set; } = new();

    /// <summary>Eye frame shapes.</summary>
    public List<QrCatalogueItemModel> EyeFrameShapes { get; set; } = new();

    /// <summary>Eye ball shapes.</summary>
    public List<QrCatalogueItemModel> EyeBallShapes { get; set; } = new();

    /// <summary>The design limits.</summary>
    public QrDesignLimitsModel Limits { get; set; } = new();
}

/// <summary>
/// A signed upload target for your own QR logo
/// (<c>POST /api/qr-code-template/logo-upload-url</c>).
/// </summary>
public class QrLogoUploadUrlModel
{
    /// <summary>The signed PUT URL; PUT the file with the same <c>Content-Type</c>.</summary>
    [JsonPropertyName("uploadFileURL")]
    public string UploadFileURL { get; set; } = string.Empty;

    /// <summary>The public URL to save as <see cref="QRCodeOptionsModel.Logo"/> with <c>LogoSource = "upload"</c>.</summary>
    [JsonPropertyName("fileURL")]
    public string FileURL { get; set; } = string.Empty;

    /// <summary>The file's path in the bucket.</summary>
    public string? BucketFilePath { get; set; }

    /// <summary>Resumable (tus) upload endpoint, when enabled.</summary>
    public string? TusEndpoint { get; set; }

    /// <summary>Resumable (tus) upload ticket, when enabled.</summary>
    public string? Ticket { get; set; }
}
