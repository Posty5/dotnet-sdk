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
