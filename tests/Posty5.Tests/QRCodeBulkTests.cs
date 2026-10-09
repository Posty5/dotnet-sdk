using System.Text.Json;
using Posty5.Core.Configuration;
using Posty5.Core.Models;
using Posty5.QRCode;
using Posty5.QRCode.Models;
using Xunit;

namespace Posty5.Tests;

/// <summary>QR bulk create, export and jobs, pinned offline against <see cref="RecordingServer"/>.</summary>
public class QRCodeBulkTests : IDisposable
{
    private readonly RecordingServer _server = new();

    public void Dispose() => _server.Dispose();

    [Fact]
    public async Task CreateMany_SendsTypeAndTarget_UnderItems()
    {
        _server.ResultJson = """{"created":2,"failed":0,"items":[{"row":1,"status":"created","id":"q1","qrCodeDownloadURL":"https://cdn.example/q1.png"},{"row":2,"status":"created","id":"q2"}]}""";
        using var http = _server.Http();
        var client = new QRCodeClient(http);

        var wifi = QRCodeBulkRow.ForWifi(new QRCodeWifiTargetModel { Name = "Cafe", AuthenticationType = "WPA", Password = "p;w" });
        wifi.FileName = "cafe";
        var result = await client.CreateManyAsync(new[]
        {
            QRCodeBulkRow.ForUrl(new QRCodeUrlTargetModel { Url = "https://example.com" }),
            wifi
        }, new CreateManyOptions { IdempotencyKey = "q" });

        var request = _server.Requests.Single();
        Assert.Equal(("POST", "/api/qr-code/bulk"), (request.Method, request.PathAndQuery));
        Assert.Equal("q-0", _server.Headers[0][BulkDefaults.IdempotencyKeyHeader]);

        using var body = JsonDocument.Parse(request.Body);
        var items = body.RootElement.GetProperty("items");
        Assert.Equal("url", items[0].GetProperty("type").GetString());
        Assert.Equal("https://example.com", items[0].GetProperty("target").GetProperty("url").GetString());
        Assert.Equal("wifi", items[1].GetProperty("type").GetString());
        Assert.Equal("p;w", items[1].GetProperty("target").GetProperty("password").GetString());
        Assert.Equal("cafe", items[1].GetProperty("fileName").GetString());

        Assert.Equal("https://cdn.example/q1.png", result.Items[0].QrCodeDownloadUrl);
    }

    [Fact]
    public async Task Export_And_ZipResultUrl_Routes()
    {
        _server.ResultJson = """{"url":"https://r2.example/z","expiresAt":"2026-10-06T00:15:00Z"}""";
        using var http = _server.Http();
        var client = new QRCodeClient(http);

        await client.ExportAsync();
        await client.GetBulkJobResultUrlAsync("j9");
        await client.CreateBulkJobAsync(new CreateBulkJobRequest
        {
            Content = "type,url,fileName\nurl,https://a.example,a",
            Options = new BulkJobOptions { Image = new BulkJobImageOptions { SizePx = 512 } }
        });

        Assert.Equal("/api/qr-code/export?format=csv", _server.Requests[0].PathAndQuery);
        Assert.Equal("/api/link-bulk-jobs/j9/result-url?file=zip", _server.Requests[1].PathAndQuery);
        using var body = JsonDocument.Parse(_server.Requests[2].Body);
        Assert.Equal("qrCodes", body.RootElement.GetProperty("kind").GetString());
        Assert.Equal("png", body.RootElement.GetProperty("options").GetProperty("image").GetProperty("format").GetString());
        Assert.Equal(512, body.RootElement.GetProperty("options").GetProperty("image").GetProperty("sizePx").GetInt32());
    }
}
