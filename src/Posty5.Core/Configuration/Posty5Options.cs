namespace Posty5.Core.Configuration;

/// <summary>
/// Configuration options for the Posty5 SDK
/// </summary>
public class Posty5Options
{
    /// <summary>
    /// Base URL for the Posty5 API
    /// </summary>
    public string BaseUrl { get; set; } = "https://api.posty5.com";

    /// <summary>
    /// API key for authentication
    /// </summary>
    public string? ApiKey { get; set; }

    /// <summary>
    /// Enable debug logging
    /// </summary>
    public bool Debug { get; set; } = false;

    /// <summary>
    /// Extra headers sent on every request, e.g. a correlation id your own logs
    /// use.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Read once, when <see cref="Http.Posty5HttpClient"/> is constructed;
    /// changing the dictionary afterwards does not affect a client that already
    /// exists.
    /// </para>
    /// <para>
    /// <b><c>X-API-Key</c> cannot be set here</b> — the constructor throws
    /// <see cref="ArgumentException"/> rather than let a header silently decide
    /// which key a request uses. Use <see cref="ApiKey"/> or
    /// <see cref="Http.Posty5HttpClient.SetApiKey"/>.
    /// </para>
    /// <para>
    /// An entry named <c>X-Posty5-Client</c> replaces the SDK's own
    /// <c>posty5-dotnet/&lt;version&gt;</c> label — for a wrapper that wants to
    /// be told apart in the API's logs. The label is recorded, never trusted.
    /// Content headers such as <c>Content-Type</c> are rejected too: they belong
    /// to a request body, not to every request.
    /// </para>
    /// </remarks>
    public Dictionary<string, string>? DefaultHeaders { get; set; }

    /// <summary>
    /// The <c>createdFrom</c> label stamped on every record this client creates —
    /// a value of your own choosing to filter your records by later (the API
    /// accepts any string).
    /// </summary>
    /// <remarks>
    /// When null or blank, each package keeps the label it has always sent
    /// (<see cref="CreatedFromDefaults.Package"/> for the tool clients,
    /// <see cref="CreatedFromDefaults.StoreOrder"/> for store orders). A value set
    /// on an individual request, where the request model has one, wins over this.
    /// </remarks>
    public string? CreatedFrom { get; set; }

    /// <summary>
    /// Optional logger. The SDK writes one warning through it the first time
    /// the API answers with <c>X-Posty5-Concurrency: missing-version</c>: a
    /// write that will be refused once the API enforces versions.
    /// </summary>
    public Microsoft.Extensions.Logging.ILogger? Logger { get; set; }

    /// <summary>
    /// Request timeout in seconds
    /// </summary>
    public readonly int TimeoutSeconds = 120;

    /// <summary>
    /// Maximum number of retry attempts
    /// </summary>
    public readonly int MaxRetries = 3;

    /// <summary>
    /// Retry delay in milliseconds
    /// </summary>
    public readonly int RetryDelayMilliseconds = 1000;
}
