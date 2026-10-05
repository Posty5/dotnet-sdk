namespace Posty5.QRCode;

/// <summary>
/// The API routes <see cref="QRCodeTemplateClient"/> calls, in one place rather
/// than inline in the client.
/// </summary>
internal static class QRCodeTemplateRoutes
{
    /// <summary>The base of every template route.</summary>
    public const string Base = "/api/qr-code-template";

    /// <summary>The caller's own templates.</summary>
    public const string UserLookup = Base + "/user-lookup";

    /// <summary>Posty5's public templates (no API key needed).</summary>
    public const string PublicLookup = Base + "/public-lookup";
}
