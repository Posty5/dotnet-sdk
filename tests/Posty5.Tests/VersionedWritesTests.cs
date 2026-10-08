using System.Net;
using Microsoft.Extensions.Logging;
using Posty5.Core.Configuration;
using Posty5.Core.Exceptions;
using Posty5.Core.Http;
using Posty5.ShortLink;
using Posty5.ShortLink.Models;
using Xunit;

namespace Posty5.Tests;

/// <summary>
/// Optimistic concurrency: every update and delete sends the version it read as
/// <c>If-Match</c> on its own request, and the API's 409 / 428 answers map to
/// typed exceptions. Runs against <see cref="RecordingServer"/>, no network.
/// </summary>
public class VersionedWritesTests : IDisposable
{
    private readonly RecordingServer _server = new();

    public void Dispose() => _server.Dispose();

    private static ShortLinkUpdateRequestModel Update() => new() { BaseUrl = "https://example.com", TemplateId = "t1" };

    [Fact]
    public async Task Update_SendsIfMatchWithTheVersion()
    {
        _server.ResultJson = """{"_id":"l1","__v":5}""";
        using var http = _server.Http();

        var link = await new ShortLinkClient(http).UpdateAsync("l1", Update(), 4);

        Assert.Equal("\"4\"", _server.Headers.Single()[Posty5ConcurrencyConst.IfMatchHeader]);
        Assert.Equal(5, link.Version);
    }

    [Fact]
    public async Task Delete_SendsIfMatchWithTheVersion()
    {
        using var http = _server.Http();

        await new ShortLinkClient(http).DeleteAsync("l1", 9);

        var request = Assert.Single(_server.Requests);
        Assert.Equal("DELETE", request.Method);
        Assert.Equal("\"9\"", _server.Headers.Single()[Posty5ConcurrencyConst.IfMatchHeader]);
    }

    [Fact]
    public async Task EnvelopeVersion_IsCopiedOntoTheResult()
    {
        _server.RawBody = """{"message":"Updated","result":{"_id":"l1","__v":2},"version":6}""";
        using var http = _server.Http();

        var response = await http.PutAsync<ShortLinkModel>("/api/short-link/l1", new { }, 5);

        Assert.Equal(6, response.Version);
        Assert.Equal(6, response.Result!.Version);
    }

    [Fact]
    public async Task ETag_IsReadWhenTheEnvelopeHasNoVersion()
    {
        _server.ResultJson = """{"_id":"l1"}""";
        _server.ResponseHeaders["ETag"] = "\"8\"";
        using var http = _server.Http();

        var response = await http.PutAsync<ShortLinkModel>("/api/short-link/l1", new { }, 7);

        Assert.Equal(8, response.Version);
    }

    [Fact]
    public async Task ParallelWrites_DoNotLeakVersionsIntoEachOther()
    {
        using var http = _server.Http();
        var client = new ShortLinkClient(http);

        var calls = Enumerable.Range(0, 20)
            .Select(i => client.UpdateAsync($"l{i}", Update(), i * 10));
        await Task.WhenAll(calls);

        Assert.Equal(20, _server.Requests.Count);
        for (var r = 0; r < _server.Requests.Count; r++)
        {
            var id = int.Parse(_server.Requests[r].PathAndQuery.Split('/').Last()[1..]);
            Assert.Equal($"\"{id * 10}\"", _server.Headers[r][Posty5ConcurrencyConst.IfMatchHeader]);
        }
    }

    [Fact]
    public async Task UnversionedCalls_SendNoIfMatch()
    {
        using var http = _server.Http();

        await http.PutAsync<object>("/api/x", new { });
        await http.GetAsync<object>("/api/x");

        Assert.All(_server.Headers, h => Assert.False(h.ContainsKey(Posty5ConcurrencyConst.IfMatchHeader)));
    }

    [Fact]
    public async Task NegativeVersion_IsRefusedBeforeAnyCall()
    {
        using var http = _server.Http();

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => new ShortLinkClient(http).UpdateAsync("l1", Update(), -1));
        Assert.Empty(_server.Requests);
    }

    [Fact]
    public async Task VersionConflict_RaisesTheConflictException()
    {
        _server.Status = HttpStatusCode.Conflict;
        _server.RawBody = """{"message":"Changed by someone else","code":"VERSION_CONFLICT","result":{"_id":"l1","currentVersion":12}}""";
        using var http = _server.Http();

        var ex = await Assert.ThrowsAsync<Posty5ConflictException>(() => new ShortLinkClient(http).UpdateAsync("l1", Update(), 3));

        Assert.Equal(12, ex.CurrentVersion);
        Assert.Equal("l1", ex.ResourceId);
        Assert.Equal(409, ex.StatusCode);
        Assert.Equal("Changed by someone else", ex.Message);
    }

    [Fact]
    public async Task OtherConflict_StaysGeneric()
    {
        _server.Status = HttpStatusCode.Conflict;
        _server.RawBody = """{"message":"Offset mismatch"}""";
        using var http = _server.Http();

        var ex = await Assert.ThrowsAsync<Posty5Exception>(() => http.PutAsync<object>("/api/x", new { }, 1));

        Assert.IsNotType<Posty5ConflictException>(ex);
        Assert.Equal(409, ex.StatusCode);
    }

    [Fact]
    public async Task MissingVersion_RaisesTheVersionRequiredException()
    {
        _server.Status = (HttpStatusCode)428;
        _server.RawBody = """{"message":"Version required","code":"VERSION_REQUIRED"}""";
        using var http = _server.Http();

        var ex = await Assert.ThrowsAsync<Posty5VersionRequiredException>(() => http.PutAsync<object>("/api/x", new { }));

        Assert.Equal(428, ex.StatusCode);
    }

    [Fact]
    public async Task VersionedWrites_StillCarryTheClientHeader()
    {
        using var http = _server.Http();

        await new ShortLinkClient(http).DeleteAsync("l1", 1);

        Assert.Equal(Posty5ClientIdentity.HeaderValue, _server.Headers.Single()[Posty5HttpDefaults.ClientHeader]);
    }

    [Fact]
    public async Task ReportModeHeader_LogsOneWarning()
    {
        _server.ResponseHeaders[Posty5ConcurrencyConst.ConcurrencyHeader] = Posty5ConcurrencyConst.MissingVersionValue;
        var logger = new CountingLogger();
        using var http = _server.Http(new Posty5Options { ApiKey = "test-key", Logger = logger });

        await http.PutAsync<object>("/api/x", new { });
        await http.PutAsync<object>("/api/x", new { });

        Assert.Equal(1, logger.Warnings);
    }

    [Fact]
    public async Task BulkWrite_SendsTheVersionsMapInTheBody()
    {
        _server.RawBody = """{"message":"ok","result":{},"versions":{"a":4}}""";
        using var http = _server.Http();

        var response = await http.SendBulkVersionedAsync<object>(
            HttpMethod.Post, "/api/x/bulk",
            new Dictionary<string, object?> { ["ids"] = new[] { "a" } },
            new Dictionary<string, long> { ["a"] = 3 });

        Assert.Contains("\"versions\":{\"a\":3}", _server.Requests.Single().Body);
        Assert.False(_server.Headers.Single().ContainsKey(Posty5ConcurrencyConst.IfMatchHeader));
        Assert.Equal(4, response.Versions!["a"]);
    }

    private sealed class CountingLogger : ILogger
    {
        public int Warnings;
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
        public bool IsEnabled(LogLevel logLevel) => true;
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            if (logLevel == LogLevel.Warning) Interlocked.Increment(ref Warnings);
        }
    }
}
