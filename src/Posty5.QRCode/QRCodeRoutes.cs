namespace Posty5.QRCode;

/// <summary>
/// The design and export routes <see cref="QRCodeClient"/> calls. The older
/// create/update/list routes still use the client's own <c>BasePath</c>.
/// </summary>
internal static class QRCodeRoutes
{
    /// <summary>The base of every QR code route.</summary>
    public const string Base = "/api/qr-code";

    /// <summary>The scannability report.</summary>
    public const string ScannabilityCheck = Base + "/scannability-check";

    /// <summary>A QR code's image.</summary>
    public static string Render(string id) => $"{Base}/{id}/render";

    /// <summary>A signed download link for a QR code's image.</summary>
    public static string ExportUrl(string id) => $"{Base}/{id}/export-url";
}
