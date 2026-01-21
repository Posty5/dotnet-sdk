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
    public int TimeoutSeconds { get; set; } = 30;

    /// <summary>
    /// Maximum number of retry attempts
    /// </summary>
    public int MaxRetries { get; set; } = 3;

    /// <summary>
    /// Retry delay in milliseconds
    /// </summary>
    public int RetryDelayMilliseconds { get; set; } = 1000;
}
