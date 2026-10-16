using System.Globalization;
using Posty5.QRCode.Models;

namespace Posty5.QRCode;

/// <summary>
/// Builds the query of the render and export-url routes, shared by
/// <see cref="QRCodeClient"/> and <see cref="QRCodeTemplateClient"/>.
/// </summary>
internal static class QrRenderQueryHelper
{
    /// <summary>The value sent as <c>download</c>; the API only checks that it is present.</summary>
    private const string DownloadFlag = "1";

    /// <summary>
    /// The query for <paramref name="options"/>; unset properties are left out. With
    /// <paramref name="withDownload"/> (the render routes), <see cref="QrRenderOptions.Download"/>
    /// adds <c>download</c>; the export-url route never takes it. Numbers are written
    /// in the invariant culture, whatever the caller's culture is.
    /// </summary>
    public static Dictionary<string, object?> ToQueryParams(QrExportUrlOptions? options, bool withDownload)
    {
        var query = new Dictionary<string, object?>();
        if (options == null) return query;

        if (!string.IsNullOrEmpty(options.Format)) query["format"] = options.Format;
        if (!string.IsNullOrEmpty(options.PresetId)) query["presetId"] = options.PresetId;
        if (options.CustomMm.HasValue) query["customMm"] = options.CustomMm.Value.ToString(CultureInfo.InvariantCulture);
        if (options.Dpi.HasValue) query["dpi"] = options.Dpi.Value.ToString(CultureInfo.InvariantCulture);
        if (options.SizePx.HasValue) query["sizePx"] = options.SizePx.Value.ToString(CultureInfo.InvariantCulture);
        if (withDownload && options is QrRenderOptions { Download: true }) query["download"] = DownloadFlag;

        return query;
    }

    /// <summary>Throw when <paramref name="id"/> is empty, before any request.</summary>
    public static void RequireId(string id, string paramName)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("An id is required.", paramName);
    }
}
