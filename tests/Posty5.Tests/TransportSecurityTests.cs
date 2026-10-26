using System.Net;
using Microsoft.Extensions.Logging;
using Posty5.Core.Configuration;
using Posty5.Core.Exceptions;
using Posty5.Core.Helpers;
using Posty5.Core.Http;
using Posty5.ShortLink;
using Xunit;

namespace Posty5.Tests.Unit;

/// <summary>
/// Offline: the API key stays on the API's host, and a caller-supplied id
/// cannot steer a request to another route.
/// </summary>
public class TransportSecurityTests : IDisposable
{
    private readonly RecordingServer _api = new();
    private readonly RecordingServer _elsewhere = new();

    public void Dispose()
    {
        _api.Dispose();
        _elsewhere.Dispose();
    }

    // ─── Redirects ──────────────────────────────────────────────────────────

    [Fact]
    public async Task ARedirect_IsNotFollowed_SoTheKeyNeverReachesTheOtherHost()
    {
        _api.Status = HttpStatusCode.Found;
        _api.ResponseHeaders["Location"] = $"{_elsewhere.BaseUrl}/collect";

        using var http = _api.Http();
        await Assert.ThrowsAsync<Posty5Exception>(() => http.GetAsync<object>("/api/a"));

        Assert.Single(_api.Requests);
        Assert.Empty(_elsewhere.Requests);
    }

    // ─── Plain http ─────────────────────────────────────────────────────────

    [Theory]
    [InlineData("http://api.example.test", 1)]
    [InlineData("https://api.example.test", 0)]
    [InlineData("http://127.0.0.1:5000", 0)]
    [InlineData("http://localhost:5000", 0)]
    public void APlainHttpBaseUrl_OffLoopback_LogsAWarning(string baseUrl, int expectedWarnings)
    {
        var logger = new CountingLogger();
        using var http = new Posty5HttpClient(new Posty5Options { ApiKey = "test-key", BaseUrl = baseUrl, Logger = logger });

        Assert.Equal(expectedWarnings, logger.Warnings);
    }

    // ─── Path segments ──────────────────────────────────────────────────────

    [Theory]
    [InlineData("abc/../../user/current", "/api/short-link/abc%2F..%2F..%2Fuser%2Fcurrent")]
    [InlineData("abc?x=1#y", "/api/short-link/abc%3Fx%3D1%23y")]
    [InlineData("6a1b2c3d4e5f", "/api/short-link/6a1b2c3d4e5f")]
    public async Task AnId_StaysOneSegmentOfItsOwnRoute(string id, string expectedPath)
    {
        _api.ResultJson = "{}";
        using var http = _api.Http();
        await new ShortLinkClient(http).GetAsync(id);

        Assert.Equal(expectedPath, _api.Requests.Single().PathAndQuery);
    }

    [Theory]
    [InlineData(".")]
    [InlineData("..")]
    [InlineData("")]
    [InlineData("  ")]
    public async Task ADotSegmentOrBlankId_IsRefusedBeforeAnyRequest(string id)
    {
        using var http = _api.Http();
        await Assert.ThrowsAsync<ArgumentException>(() => new ShortLinkClient(http).GetAsync(id));

        Assert.Empty(_api.Requests);
    }

    [Fact]
    public void PathSegmentHelper_RefusesNull()
    {
        Assert.Throws<ArgumentNullException>(() => PathSegmentHelper.Escape(null!, "id"));
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
