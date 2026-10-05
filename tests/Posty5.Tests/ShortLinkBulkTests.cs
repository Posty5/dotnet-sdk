using System.Net;
using System.Text.Json;
using Posty5.Core.Configuration;
using Posty5.Core.Exceptions;
using Posty5.Core.Models;
using Posty5.ShortLink;
using Posty5.ShortLink.Models;
using Xunit;

namespace Posty5.Tests;

/// <summary>
/// Short-link bulk create, export and bulk jobs. The recorded tests pin routes,
/// bodies and the per-chunk Idempotency-Key offline; the live ones need
/// POSTY5_API_KEY and an API with the bulk routes deployed.
/// </summary>
public class ShortLinkBulkTests : IDisposable
{
    private readonly RecordingServer _server = new();

    public void Dispose() => _server.Dispose();

    private static List<ShortLinkBulkRow> Rows(int count) =>
        Enumerable.Range(1, count).Select(i => new ShortLinkBulkRow { Url = $"https://example.com/{i}" }).ToList();

    [Fact]
    public async Task CreateMany_SendsChunks_WithKeyPerChunk_AndRenumbersRows()
    {
        _server.ResultJson = """{"created":1,"failed":1,"items":[{"row":1,"status":"created","id":"a"},{"row":2,"status":"failed","errors":[{"field":"url","message":"blocked"}]}]}""";
        using var http = _server.Http();
        var client = new ShortLinkClient(http);
        var progress = new List<BulkProgress>();

        var result = await client.CreateManyAsync(Rows(5), new CreateManyOptions
        {
            ChunkSize = 2,
            IdempotencyKey = "k",
            Defaults = new BulkCreateDefaults { TemplateId = "t1" },
            Progress = new SyncProgress<BulkProgress>(progress.Add)
        });

        Assert.Equal(3, _server.Requests.Count);
        Assert.All(_server.Requests, r => Assert.Equal("POST", r.Method));
        Assert.All(_server.Requests, r => Assert.Equal("/api/short-link/bulk", r.PathAndQuery));
        Assert.Equal(new[] { "k-0", "k-1", "k-2" }, _server.Headers.Select(h => h[BulkDefaults.IdempotencyKeyHeader]));

        using var body = JsonDocument.Parse(_server.Requests[0].Body);
        Assert.Equal(2, body.RootElement.GetProperty("links").GetArrayLength());
        Assert.Equal("https://example.com/1", body.RootElement.GetProperty("links")[0].GetProperty("url").GetString());
        Assert.Equal("t1", body.RootElement.GetProperty("defaults").GetProperty("templateId").GetString());

        Assert.Equal(new[] { 1, 2, 3, 4, 5, 6 }, result.Items.Select(i => i.Row));
        Assert.Equal(3, result.Created);
        Assert.Equal(BulkRowStatus.Failed, result.Items[1].Status);
        Assert.Equal("url", result.Items[1].Errors![0].Field);
        Assert.Equal(new[] { 2, 4, 5 }, progress.Select(p => p.Processed));
    }

    [Fact]
    public async Task CreateMany_RetriesA5xxOnce_WithTheSameKey()
    {
        _server.Status = HttpStatusCode.BadGateway;
        using var http = _server.Http();
        var client = new ShortLinkClient(http);

        var error = await Assert.ThrowsAsync<Posty5Exception>(() =>
            client.CreateManyAsync(Rows(1), new CreateManyOptions { IdempotencyKey = "same" }));

        Assert.Equal(502, error.StatusCode);
        Assert.Equal(BulkDefaults.ChunkAttempts, _server.Requests.Count);
        Assert.All(_server.Headers, h => Assert.Equal("same-0", h[BulkDefaults.IdempotencyKeyHeader]));
    }

    [Fact]
    public async Task CreateMany_DoesNotRetryA4xx()
    {
        _server.Status = HttpStatusCode.Forbidden;
        using var http = _server.Http();
        var client = new ShortLinkClient(http);

        await Assert.ThrowsAsync<Posty5Exception>(() => client.CreateManyAsync(Rows(1)));
        Assert.Single(_server.Requests);
    }

    [Fact]
    public async Task CreateMany_GeneratesAKey_AndRefusesABadChunkSize()
    {
        _server.ResultJson = """{"created":0,"failed":0,"items":[]}""";
        using var http = _server.Http();
        var client = new ShortLinkClient(http);

        await client.CreateManyAsync(Rows(1));
        Assert.EndsWith("-0", _server.Headers[0][BulkDefaults.IdempotencyKeyHeader]);

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
            client.CreateManyAsync(Rows(1), new CreateManyOptions { ChunkSize = BulkDefaults.MaxChunkSize + 1 }));
    }

    [Fact]
    public async Task CreateMany_CancelledBeforeAChunk_SendsNothing()
    {
        using var http = _server.Http();
        var client = new ShortLinkClient(http);
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => client.CreateManyAsync(Rows(3), null, cts.Token));
        Assert.Empty(_server.Requests);
    }

    [Fact]
    public async Task Export_SendsFormatColumnsAndFilters()
    {
        using var http = _server.Http();
        var client = new ShortLinkClient(http);

        await client.ExportAsync(new ExportOptions
        {
            Format = ExportFormat.Json,
            Columns = new() { "id", "shortUrl" },
            Filters = new() { ["tag"] = "spring" }
        });

        var path = _server.Requests.Single().PathAndQuery;
        Assert.StartsWith("/api/short-link/export?", path);
        Assert.Contains("format=json", path);
        Assert.Contains("columns=id%2CshortUrl", path);
        Assert.Contains("tag=spring", path);
    }

    [Fact]
    public async Task BulkJob_Routes()
    {
        _server.ResultJson = """{"_id":"j1","kind":"shortLinks","status":"partiallySucceeded","source":{"format":"csv","rowCount":3},"progress":{"processed":3,"created":2,"failed":1},"files":{"result":true},"createdAt":"2026-10-06T00:00:00Z"}""";
        using var http = _server.Http();
        var client = new ShortLinkClient(http);

        var job = await client.CreateBulkJobAsync(new CreateBulkJobRequest { Content = "url\nhttps://a.example", IdempotencyKey = "job-1" });
        await client.GetBulkJobAsync("j1");
        await client.CancelBulkJobAsync("j1");
        var finished = await client.WaitForBulkJobAsync("j1", TimeSpan.FromMilliseconds(10));

        Assert.Equal(LinkBulkJobStatus.PartiallySucceeded, job.Status);
        Assert.True(finished.IsFinished);
        Assert.Equal(("POST", "/api/link-bulk-jobs"), (_server.Requests[0].Method, _server.Requests[0].PathAndQuery));
        Assert.Equal("job-1", _server.Headers[0][BulkDefaults.IdempotencyKeyHeader]);
        using var body = JsonDocument.Parse(_server.Requests[0].Body);
        Assert.Equal("shortLinks", body.RootElement.GetProperty("kind").GetString());
        Assert.Equal("csv", body.RootElement.GetProperty("format").GetString());
        Assert.False(body.RootElement.TryGetProperty("dryRun", out _));
        Assert.Equal(("GET", "/api/link-bulk-jobs/j1"), (_server.Requests[1].Method, _server.Requests[1].PathAndQuery));
        Assert.Equal(("POST", "/api/link-bulk-jobs/j1/cancel"), (_server.Requests[2].Method, _server.Requests[2].PathAndQuery));
    }

    [Fact]
    public async Task BulkJob_ResultUrl_DryRun_AndList()
    {
        _server.ResultJson = """{"url":"https://r2.example/x","expiresAt":"2026-10-06T00:15:00Z","rowCount":2,"valid":1,"errors":[]}""";
        using var http = _server.Http();
        var client = new ShortLinkClient(http);

        var link = await client.GetBulkJobResultUrlAsync("j1", BulkJobFile.Errors);
        var report = await client.ValidateBulkJobAsync(new CreateBulkJobRequest { Format = BulkJobInputFormat.Json, Content = "[]" });
        await client.ListBulkJobsAsync(new BulkJobListParams { Status = LinkBulkJobStatus.Running });

        Assert.Equal("https://r2.example/x", link.Url);
        Assert.Equal(1, report.Valid);
        Assert.Equal("/api/link-bulk-jobs/j1/result-url?file=errors", _server.Requests[0].PathAndQuery);
        Assert.True(JsonDocument.Parse(_server.Requests[1].Body).RootElement.GetProperty("dryRun").GetBoolean());
        Assert.Equal("/api/link-bulk-jobs?kind=shortLinks&status=running", _server.Requests[2].PathAndQuery);
    }

    [BulkChargeFact]
    public async Task Live_CreateMany_250Rows()
    {
        using var http = TestConfig.CreateHttpClient();
        var client = new ShortLinkClient(http);

        var result = await client.CreateManyAsync(
            Enumerable.Range(1, 250).Select(i => new ShortLinkBulkRow { Url = $"https://example.com/bulk-test/{i}" }).ToList(),
            new CreateManyOptions { Defaults = new BulkCreateDefaults { TemplateId = TestConfig.Env(BulkChargeFactAttribute.TemplateVar) } });

        Assert.Equal(250, result.Items.Count);
        Assert.Equal(Enumerable.Range(1, 250), result.Items.Select(i => i.Row));
    }

    /// <summary>Reports synchronously, unlike <see cref="Progress{T}"/>, so assertions see every report.</summary>
    private sealed class SyncProgress<T> : IProgress<T>
    {
        private readonly Action<T> _report;
        public SyncProgress(Action<T> report) => _report = report;
        public void Report(T value) => _report(value);
    }
}

/// <summary>
/// A live fact that creates (and is charged for) records: needs POSTY5_API_KEY,
/// POSTY5_TEST_ALLOW_CHARGES=true and POSTY5_BULK_TEMPLATE_ID; skipped otherwise.
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public sealed class BulkChargeFactAttribute : FactAttribute
{
    public const string TemplateVar = "POSTY5_BULK_TEMPLATE_ID";

    public BulkChargeFactAttribute()
    {
        if (!TestConfig.IsSet(TestConfig.ApiKeyVar) || !TestConfig.AllowCharges || !TestConfig.IsSet(TemplateVar))
            Skip = $"Needs {TestConfig.ApiKeyVar}, {TestConfig.AllowChargesVar}=true and {TemplateVar}.";
    }
}