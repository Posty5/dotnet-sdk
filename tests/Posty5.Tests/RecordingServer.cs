using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using Posty5.Core.Configuration;
using Posty5.Core.Http;

namespace Posty5.Tests;

/// <summary>
/// A local HTTP server the real <see cref="Posty5HttpClient"/> is pointed at. It
/// records each request and answers with the API's envelope, so routes, verbs,
/// query strings, bodies and headers are pinned without the network.
/// </summary>
internal sealed class RecordingServer : IDisposable
{
    /// <summary>Held while a port is probed and registered, so parallel test classes never pick the same one.</summary>
    private static readonly object StartLock = new();

    private readonly HttpListener _listener;
    private readonly CancellationTokenSource _stop = new();

    public List<(string Method, string PathAndQuery, string Body)> Requests { get; } = new();

    /// <summary>The headers of each request, in the same order as <see cref="Requests"/>.</summary>
    public List<Dictionary<string, string?>> Headers { get; } = new();

    /// <summary>The JSON placed in the envelope's <c>result</c>.</summary>
    public string ResultJson { get; set; } = "{}";
    /// <summary>The envelope's <c>message</c>; an error answer carries the API's text here.</summary>
    public string Message { get; set; } = "ok";
    public HttpStatusCode Status { get; set; } = HttpStatusCode.OK;
    /// <summary>When set, sent as the whole response body instead of the envelope (e.g. an error with a <c>code</c>).</summary>
    public string? RawBody { get; set; }
    /// <summary>When set, sent as the whole response body as bytes (a file download), with <see cref="ResponseContentType"/>.</summary>
    public byte[]? RawBytes { get; set; }
    /// <summary>The response's <c>Content-Type</c>.</summary>
    public string ResponseContentType { get; set; } = "application/json";
    /// <summary>Extra headers on every response, e.g. <c>ETag</c> or <c>X-Posty5-Concurrency</c>.</summary>
    public Dictionary<string, string> ResponseHeaders { get; } = new();
    public string BaseUrl { get; }

    public RecordingServer()
    {
        (_listener, BaseUrl) = StartOnFreePort();
        _ = Task.Run(LoopAsync);
    }

    /// <summary>
    /// Starts a listener on a free loopback port. The <see cref="TcpListener"/>
    /// probe only proposes a port: it is released before http.sys registers the
    /// prefix, and in that gap the port can be taken ("being used by another
    /// process") or already registered ("conflicts with an existing
    /// registration"). A failed start is retried on a freshly probed port.
    /// </summary>
    private static (HttpListener Listener, string BaseUrl) StartOnFreePort()
    {
        for (var attempt = 1; ; attempt++)
        {
            lock (StartLock)
            {
                var baseUrl = $"http://127.0.0.1:{ProbeFreePort()}";
                var listener = new HttpListener();
                listener.Prefixes.Add($"{baseUrl}/");
                try
                {
                    listener.Start();
                    return (listener, baseUrl);
                }
                catch (HttpListenerException)
                {
                    // A listener whose Start failed is closed for good; drop it and probe again.
                    listener.Close();
                    if (attempt >= TestConfig.RecordingServerStartAttempts) throw;
                }
            }
        }
    }

    /// <summary>A port the OS reports free on the loopback address right now.</summary>
    private static int ProbeFreePort()
    {
        var probe = new TcpListener(IPAddress.Loopback, 0);
        probe.Start();
        try { return ((IPEndPoint)probe.LocalEndpoint).Port; }
        finally { probe.Stop(); }
    }

    private async Task LoopAsync()
    {
        while (!_stop.IsCancellationRequested)
        {
            HttpListenerContext context;
            try { context = await _listener.GetContextAsync(); }
            catch { return; }

            using var reader = new StreamReader(context.Request.InputStream, Encoding.UTF8);
            var body = await reader.ReadToEndAsync();
            var headers = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
            foreach (var name in context.Request.Headers.AllKeys)
            {
                if (name != null) headers[name] = context.Request.Headers[name];
            }
            lock (Requests)
            {
                Requests.Add((context.Request.HttpMethod, context.Request.RawUrl ?? "", body));
                Headers.Add(headers);
            }

            var payload = RawBytes ?? Encoding.UTF8.GetBytes(RawBody ?? $"{{\"message\":{JsonSerializer.Serialize(Message)},\"result\":{ResultJson}}}");
            context.Response.StatusCode = (int)Status;
            foreach (var (name, value) in ResponseHeaders)
                context.Response.Headers[name] = value;
            context.Response.ContentType = ResponseContentType;
            await context.Response.OutputStream.WriteAsync(payload);
            context.Response.Close();
        }
    }

    /// <summary>A real HTTP client pointed at this server, with a test key unless <paramref name="options"/> says otherwise.</summary>
    public Posty5HttpClient Http(Posty5Options? options = null)
    {
        options ??= new Posty5Options { ApiKey = "test-key" };
        options.BaseUrl = BaseUrl;
        return new Posty5HttpClient(options);
    }

    public void Dispose()
    {
        _stop.Cancel();
        _listener.Close();
    }
}
