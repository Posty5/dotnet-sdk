using System.Globalization;
using System.Net;
using System.Text.Json;
using Posty5.Core.Configuration;
using Posty5.Core.Exceptions;
using Posty5.QRCode;
using Posty5.QRCode.Models;
using Xunit;

namespace Posty5.Tests.Unit;

/// <summary>
/// Offline: the QR design routes (render, export link, scannability, template CRUD,
/// catalogue, logo upload URL) against a <see cref="RecordingServer"/> — verb,
/// path+query, body, If-Match, and the binary <c>FileResponse</c> mapping.
/// </summary>
public class QRCodeDesignRouteTests : IDisposable
{
    private readonly RecordingServer _server = new();

    public void Dispose() => _server.Dispose();

    private QRCodeClient QrClient() => new(_server.Http());
    private QRCodeTemplateClient Templates() => new(_server.Http());
    private string Last() { var r = _server.Requests.Last(); return $"{r.Method} {r.PathAndQuery}"; }

    [Fact]
    public async Task RenderAsync_WithoutOptions_AsksForThePlainPng_AndMapsTheFile()
    {
        _server.RawBytes = new byte[] { 0x89, 0x50, 0x4E, 0x47, 1, 2, 3 };
        _server.ResponseContentType = "image/png";
        _server.ResponseHeaders["Content-Disposition"] = "attachment; filename=\"my%20menu-512.png\"";

        var file = await QrClient().RenderAsync("qr1");

        Assert.Equal("GET /api/qr-code/qr1/render", Last());
        Assert.Equal(_server.RawBytes, file.Data);
        Assert.Equal("image/png", file.ContentType);
        Assert.Equal("my menu-512.png", file.FileName);
    }

    [Fact]
    public async Task RenderAsync_SendsEveryOption_InTheInvariantCulture()
    {
        _server.RawBytes = new byte[] { 0x25, 0x50, 0x44, 0x46 };
        _server.ResponseContentType = "application/pdf";
        var culture = CultureInfo.CurrentCulture;
        CultureInfo.CurrentCulture = new CultureInfo("de-DE");
        try
        {
            await QrClient().RenderAsync("qr1", new QrRenderOptions
            {
                Format = QrRenderFormats.Pdf,
                PresetId = QrPrintPresetIds.Custom,
                CustomMm = 25.5,
                Dpi = 300,
                SizePx = 1024,
                Download = true,
            });
        }
        finally { CultureInfo.CurrentCulture = culture; }

        Assert.Equal("GET /api/qr-code/qr1/render?format=pdf&presetId=custom&customMm=25.5&dpi=300&sizePx=1024&download=1", Last());
    }

    [Fact]
    public async Task RenderAsync_PlanGate_ThrowsWithTheApiMessage()
    {
        _server.Status = HttpStatusCode.Forbidden;
        _server.RawBody = "{\"message\":\"This feature is not available on your current plan.\"}";

        var ex = await Assert.ThrowsAsync<Posty5Exception>(() =>
            QrClient().RenderAsync("qr1", new QrRenderOptions { Format = QrRenderFormats.Svg }));

        Assert.Equal(403, ex.StatusCode);
        Assert.Contains("This feature is not available on your current plan.", ex.ResponseBody);
    }

    [Fact]
    public async Task RenderAsync_EmptyId_ThrowsBeforeAnyRequest()
    {
        await Assert.ThrowsAsync<ArgumentException>(() => QrClient().RenderAsync(" "));
        await Assert.ThrowsAsync<ArgumentException>(() => Templates().RenderAsync(""));
        Assert.Empty(_server.Requests);
    }

    [Fact]
    public async Task GetExportUrlAsync_SendsTheQuery_WithoutDownload_AndReadsTheLink()
    {
        _server.ResultJson = "{\"url\":\"https://test-api.posty5.com/api/qr-code/build-qr/l1?format=svg&exp=1&sig=x\",\"expiresAt\":\"2026-10-17T12:00:00.000Z\"}";

        var link = await QrClient().GetExportUrlAsync("qr1", new QrRenderOptions { Format = QrRenderFormats.Svg, Download = true });

        Assert.Equal("GET /api/qr-code/qr1/export-url?format=svg", Last());
        Assert.StartsWith("https://test-api.posty5.com/api/qr-code/build-qr/l1", link.Url);
        Assert.Equal(new DateTime(2026, 10, 17, 12, 0, 0, DateTimeKind.Utc), link.ExpiresAt!.Value.ToUniversalTime());
    }

    [Fact]
    public async Task CheckScannabilityAsync_PostsTheSource_AndReadsTheReport()
    {
        _server.ResultJson = "{\"verdict\":\"warning\",\"checks\":[{\"code\":\"contrast\",\"status\":\"warning\",\"value\":3.9,\"thresholds\":{\"warning\":4.5,\"poor\":3},\"messageKey\":\"qrScannability.contrast.warning\",\"message\":\"Low contrast\"},{\"code\":\"futureCheck\",\"status\":\"good\",\"value\":1,\"thresholds\":{\"warning\":null,\"poor\":null}}],\"symbol\":{\"version\":3,\"size\":29,\"ecLevel\":\"M\",\"quietZoneModules\":4,\"moduleSizeMm\":0.86}}";

        var report = await QrClient().CheckScannabilityAsync(new QrScannabilityRequest
        {
            TemplateId = "t1",
            PresetId = QrPrintPresetIds.Sticker3cm,
        });

        var request = _server.Requests.Single();
        Assert.Equal("POST /api/qr-code/scannability-check", $"{request.Method} {request.PathAndQuery}");
        using var body = JsonDocument.Parse(request.Body);
        Assert.Equal("t1", body.RootElement.GetProperty("templateId").GetString());
        Assert.Equal("sticker-3cm", body.RootElement.GetProperty("presetId").GetString());
        Assert.False(body.RootElement.TryGetProperty("qrCodeId", out _));

        Assert.Equal(QrScannabilityStatuses.Warning, report.Verdict);
        Assert.Equal(2, report.Checks.Count);
        Assert.Equal(QrScannabilityCheckCodes.Contrast, report.Checks[0].Code);
        Assert.Equal(4.5, report.Checks[0].Thresholds!.Warning);
        Assert.Equal("futureCheck", report.Checks[1].Code);
        Assert.Null(report.Checks[1].Thresholds!.Poor);
        Assert.Equal("M", report.Symbol!.EcLevel);
        Assert.Equal(0.86, report.Symbol.ModuleSizeMm);
    }

    [Fact]
    public async Task CheckScannabilityAsync_NeedsExactlyOneSource()
    {
        var qr = QrClient();
        await Assert.ThrowsAsync<ArgumentException>(() => qr.CheckScannabilityAsync(new QrScannabilityRequest()));
        await Assert.ThrowsAsync<ArgumentException>(() => qr.CheckScannabilityAsync(new QrScannabilityRequest { QrCodeId = "q", TemplateId = "t" }));
        await Assert.ThrowsAsync<ArgumentException>(() => qr.CheckScannabilityAsync(new QrScannabilityRequest { Options = new QRCodeOptionsModel() }));
        Assert.Empty(_server.Requests);
    }

    [Fact]
    public async Task Template_CreateAsync_PostsNameAndTheDesign_WithWireNames()
    {
        _server.ResultJson = "{\"_id\":\"t1\",\"__v\":0,\"name\":\"Menu\"}";

        var created = await Templates().CreateAsync(new QRCodeTemplateRequestModel
        {
            Name = "Menu",
            Options = new QRCodeOptionsModel
            {
                PO_TL = "#111111",
                AI = "#222222",
                Timing_V = "#333333",
                DotShape = QrDotShapes.ExtraRounded,
                Fill = new QrFillModel { Type = QrFillTypes.Radial, Stops = { new() { Offset = 0, Color = "#000" }, new() { Offset = 1, Color = "#00f" } } },
                Frame = new QrFrameModel { Id = QrFrameIds.SpeechBubble, Text = "Scan me" },
                LogoSource = QrLogoSources.Upload,
            },
        });

        var request = _server.Requests.Single();
        Assert.Equal("POST /api/qr-code-template", $"{request.Method} {request.PathAndQuery}");
        using var body = JsonDocument.Parse(request.Body);
        Assert.Equal("Menu", body.RootElement.GetProperty("name").GetString());
        var options = body.RootElement.GetProperty("options");
        Assert.Equal("#111111", options.GetProperty("PO_TL").GetString());
        Assert.Equal("#222222", options.GetProperty("AI").GetString());
        Assert.Equal("#333333", options.GetProperty("timing_V").GetString());
        Assert.Equal("extraRounded", options.GetProperty("dotShape").GetString());
        Assert.Equal("radial", options.GetProperty("fill").GetProperty("type").GetString());
        Assert.Equal(2, options.GetProperty("fill").GetProperty("stops").GetArrayLength());
        Assert.Equal("speech-bubble", options.GetProperty("frame").GetProperty("id").GetString());
        Assert.Equal("upload", options.GetProperty("logoSource").GetString());
        Assert.False(options.TryGetProperty("text", out _));
        Assert.False(options.TryGetProperty("designVersion", out _));
        Assert.Equal("t1", created.Id);
    }

    [Fact]
    public async Task Template_UpdateAsync_PutsTheIdInTheBody_AndTheVersionInIfMatch()
    {
        _server.ResultJson = "{\"_id\":\"t1\",\"__v\":4,\"name\":\"Menu\"}";

        var updated = await Templates().UpdateAsync("t1", new QRCodeTemplateRequestModel
        {
            Name = "Menu",
            Options = new QRCodeOptionsModel { DesignVersion = 1 },
        }, 3);

        var request = _server.Requests.Single();
        Assert.Equal("PUT /api/qr-code-template", $"{request.Method} {request.PathAndQuery}");
        Assert.Equal("\"3\"", _server.Headers.Single()[Posty5ConcurrencyConst.IfMatchHeader]);
        using var body = JsonDocument.Parse(request.Body);
        Assert.Equal("t1", body.RootElement.GetProperty("_id").GetString());
        Assert.Equal(1, body.RootElement.GetProperty("options").GetProperty("designVersion").GetInt32());
        Assert.Equal(4, updated.Version);
    }

    [Fact]
    public async Task Template_GetAsync_ReadsTheDesign_AndKeepsUnknownKeys()
    {
        _server.ResultJson = "{\"_id\":\"t1\",\"__v\":2,\"name\":\"Menu\",\"isPublic\":false,\"options\":{\"designVersion\":2,\"PO_TL\":\"#0059f7\",\"timing_H\":\"#123456\",\"dotShape\":\"futureShape\",\"fill\":{\"type\":\"linear\",\"rotation\":45,\"stops\":[{\"offset\":0,\"color\":\"#000\"},{\"offset\":1,\"color\":\"#fff\"}]},\"frame\":{\"id\":\"ticket\",\"text\":\"Hi\"},\"someNewKey\":{\"a\":1}}}";

        var template = await Templates().GetAsync("t1");

        Assert.Equal("GET /api/qr-code-template/t1", Last());
        Assert.Equal(2, template.Version);
        var options = template.Options!;
        Assert.Equal(2, options.DesignVersion);
        Assert.Equal("#0059f7", options.PO_TL);
        Assert.Equal("#123456", options.Timing_H);
        Assert.Equal("futureShape", options.DotShape);
        Assert.Equal(45, options.Fill!.Rotation);
        Assert.Equal("#fff", options.Fill.Stops[1].Color);
        Assert.Equal("ticket", options.Frame!.Id);
        Assert.True(options.AdditionalProperties!.ContainsKey("someNewKey"));

        // Sent back on update unchanged.
        _server.ResultJson = "{\"_id\":\"t1\",\"__v\":3}";
        await Templates().UpdateAsync("t1", new QRCodeTemplateRequestModel { Name = template.Name!, Options = options }, template.Version);
        using var body = JsonDocument.Parse(_server.Requests.Last().Body);
        Assert.Equal(1, body.RootElement.GetProperty("options").GetProperty("someNewKey").GetProperty("a").GetInt32());
    }

    [Fact]
    public async Task Template_ListAsync_SendsNameAndCursor()
    {
        _server.ResultJson = "{\"items\":[{\"_id\":\"t1\",\"__v\":1,\"name\":\"Menu\",\"numberOfSubQrCodes\":2}],\"pagination\":{\"nextCursor\":\"c2\",\"hasMore\":true,\"pageSize\":1}}";

        var page = await Templates().ListAsync("men", new Posty5.Core.Models.PaginationParams { Cursor = "c1", PageSize = 1 });

        Assert.Equal("GET /api/qr-code-template?name=men&cursor=c1&pageSize=1", Last());
        Assert.Equal(1, page.Items.Single().Version);
        Assert.Equal(2, page.Items.Single().NumberOfSubQrCodes);
        Assert.Equal("c2", page.Pagination.NextCursor);
    }

    [Fact]
    public async Task Template_DeleteAsync_SendsTheVersionInIfMatch()
    {
        await Templates().DeleteAsync("t1", 7);

        Assert.Equal("DELETE /api/qr-code-template/t1", Last());
        Assert.Equal("\"7\"", _server.Headers.Single()[Posty5ConcurrencyConst.IfMatchHeader]);
    }

    [Fact]
    public async Task Template_RenderAsync_UsesTheTemplateRoute()
    {
        _server.RawBytes = new byte[] { 0x3C, 0x73, 0x76, 0x67 };
        _server.ResponseContentType = "image/svg+xml";

        var file = await Templates().RenderAsync("t1", new QrRenderOptions { Format = QrRenderFormats.Svg, SizePx = 800 });

        Assert.Equal("GET /api/qr-code-template/t1/render?format=svg&sizePx=800", Last());
        Assert.Equal("image/svg+xml", file.ContentType);
        Assert.Null(file.FileName);
    }

    [Fact]
    public async Task Template_GetDesignCatalogueAsync_ReadsEveryList()
    {
        _server.ResultJson = "{\"frames\":[{\"id\":\"banner-bottom\",\"labelKey\":\"qrDesign.frame.banner-bottom\",\"hasText\":true,\"defaultColor\":\"#000000\",\"defaultTextColor\":\"#ffffff\",\"previewSvg\":\"<svg/>\"}],\"ctaPresets\":[{\"id\":\"scanMe\",\"labelKey\":\"qrDesign.cta.scanMe\",\"text\":{\"en\":\"SCAN ME\",\"ar\":\"امسحني\"}}],\"printPresets\":[{\"id\":\"custom\",\"labelKey\":\"qrExport.preset.custom\",\"minMm\":20,\"maxMm\":500}],\"dotShapes\":[{\"id\":\"dots\",\"labelKey\":\"qrDesign.dotShape.dots\",\"previewSvg\":\"<svg/>\"}],\"eyeFrameShapes\":[],\"eyeBallShapes\":[],\"limits\":{\"maxRasterEdgePx\":6000,\"dpiValues\":[150,300,600],\"newLimit\":9}}";

        var catalogue = await Templates().GetDesignCatalogueAsync();

        Assert.Equal("GET /api/qr-code-template/design-catalogue", Last());
        Assert.True(catalogue.Frames.Single().HasText);
        Assert.Equal("#ffffff", catalogue.Frames.Single().DefaultTextColor);
        Assert.Equal("امسحني", catalogue.CtaPresets.Single().Text!["ar"]);
        Assert.Equal(500, catalogue.PrintPresets.Single().MaxMm);
        Assert.Equal("<svg/>", catalogue.DotShapes.Single().PreviewSvg);
        Assert.Equal(6000, catalogue.Limits.MaxRasterEdgePx);
        Assert.Equal(new[] { 150, 300, 600 }, catalogue.Limits.DpiValues);
        Assert.True(catalogue.Limits.AdditionalProperties!.ContainsKey("newLimit"));
    }

    [Fact]
    public async Task Template_GetLogoUploadUrlAsync_PostsTypeAndSize()
    {
        _server.ResultJson = "{\"uploadFileURL\":\"https://r2.example/put?sig=1\",\"fileURL\":\"https://cdn.posty5.com/qr-logos/u1/x.png\",\"bucketFilePath\":\"qr-logos/u1/x.png\"}";

        var target = await Templates().GetLogoUploadUrlAsync(QrLogoMimeTypes.Png, 1234);

        var request = _server.Requests.Single();
        Assert.Equal("POST /api/qr-code-template/logo-upload-url", $"{request.Method} {request.PathAndQuery}");
        using var body = JsonDocument.Parse(request.Body);
        Assert.Equal("image/png", body.RootElement.GetProperty("fileType").GetString());
        Assert.Equal(1234, body.RootElement.GetProperty("fileSize").GetInt64());
        Assert.Equal("https://r2.example/put?sig=1", target.UploadFileURL);
        Assert.Equal("https://cdn.posty5.com/qr-logos/u1/x.png", target.FileURL);
    }

    [Fact]
    public async Task Template_UploadLogoAsync_EmptyFile_ThrowsBeforeAnyRequest()
    {
        await Assert.ThrowsAsync<ArgumentException>(() => Templates().UploadLogoAsync(new MemoryStream(), QrLogoMimeTypes.Png));
        Assert.Empty(_server.Requests);
    }
}
