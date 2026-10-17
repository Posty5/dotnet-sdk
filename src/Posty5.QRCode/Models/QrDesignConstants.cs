namespace Posty5.QRCode.Models;

// The API's QR design vocabularies. Plain string constants, not enums, so a
// value added by a newer API reads back as its string instead of failing.

/// <summary>Data module shapes for <see cref="QRCodeOptionsModel.DotShape"/>.</summary>
public static class QrDotShapes
{
    /// <summary>Square modules (the legacy look).</summary>
    public const string Square = "square";
    /// <summary>Rounded corners, joined to neighbours.</summary>
    public const string Rounded = "rounded";
    /// <summary>Round dots.</summary>
    public const string Dots = "dots";
    /// <summary>Two opposite rounded corners.</summary>
    public const string Classy = "classy";
    /// <summary>Strongly rounded, joined to neighbours.</summary>
    public const string ExtraRounded = "extraRounded";
}

/// <summary>Eye outer ring shapes for <see cref="QRCodeOptionsModel.EyeFrameShape"/>.</summary>
public static class QrEyeFrameShapes
{
    /// <summary>Square.</summary>
    public const string Square = "square";
    /// <summary>Rounded square.</summary>
    public const string Rounded = "rounded";
    /// <summary>Circle.</summary>
    public const string Circle = "circle";
    /// <summary>Leaf (two opposite rounded corners).</summary>
    public const string Leaf = "leaf";
}

/// <summary>Eye centre shapes for <see cref="QRCodeOptionsModel.EyeBallShape"/>.</summary>
public static class QrEyeBallShapes
{
    /// <summary>Square.</summary>
    public const string Square = "square";
    /// <summary>Rounded square.</summary>
    public const string Rounded = "rounded";
    /// <summary>Circle.</summary>
    public const string Circle = "circle";
    /// <summary>Diamond.</summary>
    public const string Diamond = "diamond";
}

/// <summary>Fill types for <see cref="QrFillModel.Type"/>.</summary>
public static class QrFillTypes
{
    /// <summary>One colour (one stop).</summary>
    public const string Solid = "solid";
    /// <summary>Linear gradient (2-3 stops, optional rotation).</summary>
    public const string Linear = "linear";
    /// <summary>Radial gradient (2-3 stops).</summary>
    public const string Radial = "radial";
}

/// <summary>Frame ids for <see cref="QrFrameModel.Id"/>.</summary>
public static class QrFrameIds
{
    /// <summary>The legacy title strip, as a frame.</summary>
    public const string TitleStrip = "title-strip";
    /// <summary>Banner under the code.</summary>
    public const string BannerBottom = "banner-bottom";
    /// <summary>Banner above the code.</summary>
    public const string BannerTop = "banner-top";
    /// <summary>Rounded box around the code.</summary>
    public const string RoundedBox = "rounded-box";
    /// <summary>Speech bubble.</summary>
    public const string SpeechBubble = "speech-bubble";
    /// <summary>Ticket.</summary>
    public const string Ticket = "ticket";
    /// <summary>Ring around the code.</summary>
    public const string CircleRing = "circle-ring";
}

/// <summary>How a logo was chosen, for <see cref="QRCodeOptionsModel.LogoSource"/>.</summary>
public static class QrLogoSources
{
    /// <summary>A Posty5 library logo.</summary>
    public const string Library = "library";
    /// <summary>Your own upload (<see cref="QRCodeTemplateClient.UploadLogoAsync"/>).</summary>
    public const string Upload = "upload";
    /// <summary>Any image URL; re-hosted on the Posty5 CDN when saved.</summary>
    public const string Url = "url";
}

/// <summary>Render formats for <see cref="QrExportUrlOptions.Format"/>.</summary>
public static class QrRenderFormats
{
    /// <summary>PNG (every plan).</summary>
    public const string Png = "png";
    /// <summary>SVG (vector export).</summary>
    public const string Svg = "svg";
    /// <summary>PDF, page = artwork size (vector export).</summary>
    public const string Pdf = "pdf";
}

/// <summary>Print presets for <see cref="QrExportUrlOptions.PresetId"/>.</summary>
public static class QrPrintPresetIds
{
    /// <summary>30 mm.</summary>
    public const string Sticker3cm = "sticker-3cm";
    /// <summary>25 mm.</summary>
    public const string BusinessCard = "business-card";
    /// <summary>50 mm (the PDF default).</summary>
    public const string Flyer5cm = "flyer-5cm";
    /// <summary>80 mm.</summary>
    public const string TableTent8cm = "table-tent-8cm";
    /// <summary>150 mm.</summary>
    public const string Poster15cm = "poster-15cm";
    /// <summary>20-500 mm, from <see cref="QrExportUrlOptions.CustomMm"/>.</summary>
    public const string Custom = "custom";
}

/// <summary>Upload types accepted by <see cref="QRCodeTemplateClient.UploadLogoAsync"/>.</summary>
public static class QrLogoMimeTypes
{
    /// <summary>PNG.</summary>
    public const string Png = "image/png";
    /// <summary>JPEG.</summary>
    public const string Jpeg = "image/jpeg";
    /// <summary>WebP.</summary>
    public const string Webp = "image/webp";
    /// <summary>SVG (sanitised by the API before use).</summary>
    public const string Svg = "image/svg+xml";
}

/// <summary>Scannability verdicts and check statuses (<see cref="QrScannabilityReportModel.Verdict"/>).</summary>
public static class QrScannabilityStatuses
{
    /// <summary>Will scan.</summary>
    public const string Good = "good";
    /// <summary>May fail on some phones or sizes.</summary>
    public const string Warning = "warning";
    /// <summary>Likely to fail.</summary>
    public const string Poor = "poor";
}

/// <summary>Scannability check codes (<see cref="QrScannabilityCheckModel.Code"/>).</summary>
public static class QrScannabilityCheckCodes
{
    /// <summary>WCAG contrast of the darkest-or-worst foreground against the background.</summary>
    public const string Contrast = "contrast";
    /// <summary>Foreground lighter than background.</summary>
    public const string Inverted = "inverted";
    /// <summary>Logo area as a share of the symbol, by error-correction level.</summary>
    public const string LogoCoverage = "logoCoverage";
    /// <summary>Quiet zone in modules.</summary>
    public const string QuietZone = "quietZone";
    /// <summary>Module edge at the print size (mm) or PNG size (px).</summary>
    public const string ModuleSize = "moduleSize";
    /// <summary>Smallest dot scale.</summary>
    public const string DotScale = "dotScale";
    /// <summary>Symbol density (version) for the size.</summary>
    public const string Density = "density";
}
