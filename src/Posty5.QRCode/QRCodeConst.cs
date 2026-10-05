namespace Posty5.QRCode;

/// <summary>
/// Fixed values used by <see cref="QRCodeClient"/> and its models.
/// </summary>
internal static class QRCodeConst
{
    /// <summary><c>[Obsolete]</c> text on every <c>IsEnableMonetization</c> (TP-D5).</summary>
    public const string MonetizationObsolete =
        "Never accepted by the API; ignored by this SDK and never sent. Removed in the next major.";

    /// <summary>Thrown by the Wi-Fi create/update methods for <see cref="Models.QRCodeMode.Dynamic"/>.</summary>
    public const string WifiDynamicNotSupported =
        "Wi-Fi QR codes cannot be dynamic: a phone joins the network from the image itself, so there is no link to redirect. Use QRCodeMode.Static or leave Mode unset.";
}
