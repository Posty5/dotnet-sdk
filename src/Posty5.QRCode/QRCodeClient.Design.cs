using Posty5.Core.Exceptions;
using Posty5.Core.Models;
using Posty5.QRCode.Models;

namespace Posty5.QRCode;

/// <summary>
/// Rendering, signed export links and the scannability report.
/// </summary>
public partial class QRCodeClient
{
    /// <summary>
    /// A QR code's image as PNG, SVG or PDF (<c>GET /api/qr-code/{id}/render</c>).
    /// </summary>
    /// <remarks>
    /// <para>Only QR codes the calling API key created can be rendered, as with <see cref="GetAsync"/>.</para>
    /// <para>SVG, PDF, a print preset, a DPI or a PNG over 2048 px need vector export on the
    /// account's plan; otherwise this throws <see cref="Posty5Exception"/> with
    /// <see cref="Posty5Exception.StatusCode"/> 403 and the API's message
    /// (<c>This feature is not available on your current plan.</c>). Plain PNG works on
    /// every plan. Rendering is free.</para>
    /// <para>The file is buffered in memory; the largest raster edge is 6000 px.</para>
    /// </remarks>
    /// <param name="id">QR code ID</param>
    /// <param name="options">Format, print preset, DPI, size and download; <c>null</c> for the plain PNG.</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The file's bytes, content type and, with <see cref="QrRenderOptions.Download"/>, its name</returns>
    /// <exception cref="ArgumentException"><paramref name="id"/> is empty.</exception>
    /// <example>
    /// <code>
    /// var pdf = await qrCodeClient.RenderAsync("qr123", new QrRenderOptions
    /// {
    ///     Format = QrRenderFormats.Pdf,
    ///     PresetId = QrPrintPresetIds.Flyer5cm,
    ///     Download = true
    /// });
    /// await File.WriteAllBytesAsync(pdf.FileName ?? "qr.pdf", pdf.Data);
    /// </code>
    /// </example>
    public Task<FileResponse> RenderAsync(
        string id,
        QrRenderOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        QrRenderQueryHelper.RequireId(id, nameof(id));
        return _http.GetBytesAsync(QRCodeRoutes.Render(id), QrRenderQueryHelper.ToQueryParams(options, withDownload: true), cancellationToken);
    }

    /// <summary>
    /// A signed download link for a QR code's image, valid for 1 hour
    /// (<c>GET /api/qr-code/{id}/export-url</c>), for callers that cannot send the API key
    /// with the download (a browser link, an email, a workflow step).
    /// </summary>
    /// <remarks>
    /// Same scope and plan rule as <see cref="RenderAsync"/>, checked when the link is made;
    /// the link then works for anyone who has it until <see cref="QrExportUrlModel.ExpiresAt"/>.
    /// Free.
    /// </remarks>
    /// <param name="id">QR code ID</param>
    /// <param name="options">Format, print preset, DPI and size; <c>null</c> for the plain PNG.</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The link and when it expires</returns>
    /// <exception cref="ArgumentException"><paramref name="id"/> is empty.</exception>
    /// <example>
    /// <code>
    /// var link = await qrCodeClient.GetExportUrlAsync("qr123", new QrExportUrlOptions { Format = QrRenderFormats.Svg });
    /// Console.WriteLine($"{link.Url} (until {link.ExpiresAt:t})");
    /// </code>
    /// </example>
    public async Task<QrExportUrlModel> GetExportUrlAsync(
        string id,
        QrExportUrlOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        QrRenderQueryHelper.RequireId(id, nameof(id));
        var response = await _http.GetAsync<QrExportUrlModel>(
            QRCodeRoutes.ExportUrl(id),
            QrRenderQueryHelper.ToQueryParams(options, withDownload: false),
            cancellationToken);

        return response.Result ?? throw new InvalidOperationException("The export link was not returned");
    }

    /// <summary>
    /// Whether a design will scan (<c>POST /api/qr-code/scannability-check</c>): contrast,
    /// inverted colours, logo coverage, quiet zone, module size and dot scale, each with a
    /// fix hint.
    /// </summary>
    /// <remarks>
    /// Checks one of your QR codes, one of your templates or a public one, or an unsaved
    /// design. The report only warns; nothing is blocked by a poor verdict. No plan rule;
    /// free.
    /// </remarks>
    /// <param name="request">Exactly one source, and optionally the print size to judge at</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The report</returns>
    /// <exception cref="ArgumentException">Not exactly one source is set, or <see cref="QrScannabilityRequest.Options"/> without <see cref="QrScannabilityRequest.Text"/>.</exception>
    /// <exception cref="Posty5NotFoundException">A QR code or template you cannot see.</exception>
    /// <example>
    /// <code>
    /// var report = await qrCodeClient.CheckScannabilityAsync(new QrScannabilityRequest
    /// {
    ///     QrCodeId = "qr123",
    ///     PresetId = QrPrintPresetIds.Sticker3cm
    /// });
    /// foreach (var check in report.Checks.Where(c => c.Status != QrScannabilityStatuses.Good))
    ///     Console.WriteLine($"{check.Code}: {check.Message}");
    /// </code>
    /// </example>
    public async Task<QrScannabilityReportModel> CheckScannabilityAsync(
        QrScannabilityRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var sources = (string.IsNullOrEmpty(request.QrCodeId) ? 0 : 1)
            + (string.IsNullOrEmpty(request.TemplateId) ? 0 : 1)
            + (request.Options == null ? 0 : 1);
        if (sources != 1)
            throw new ArgumentException("Set exactly one of QrCodeId, TemplateId or Options.", nameof(request));
        if (request.Options != null && string.IsNullOrEmpty(request.Text))
            throw new ArgumentException("Text is required with Options.", nameof(request));

        var response = await _http.PostAsync<QrScannabilityReportModel>(
            QRCodeRoutes.ScannabilityCheck,
            request,
            cancellationToken);

        return response.Result ?? throw new InvalidOperationException("The scannability report was not returned");
    }
}
