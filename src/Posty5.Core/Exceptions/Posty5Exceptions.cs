namespace Posty5.Core.Exceptions;

/// <summary>
/// Base exception for Posty5 SDK errors
/// </summary>
public class Posty5Exception : Exception
{
    /// <summary>
    /// HTTP status code if available
    /// </summary>
    public int? StatusCode { get; set; }

    /// <summary>
    /// Response body if available
    /// </summary>
    public string? ResponseBody { get; set; }

    public Posty5Exception(string message) : base(message)
    {
    }

    public Posty5Exception(string message, Exception innerException) 
        : base(message, innerException)
    {
    }

    public Posty5Exception(string message, int statusCode, string? responseBody = null) 
        : base(message)
    {
        StatusCode = statusCode;
        ResponseBody = responseBody;
    }
}

/// <summary>
/// Exception thrown when authentication fails
/// </summary>
public class Posty5AuthenticationException : Posty5Exception
{
    public Posty5AuthenticationException(string message) : base(message)
    {
        StatusCode = 401;
    }
}

/// <summary>
/// Exception thrown when a resource is not found
/// </summary>
public class Posty5NotFoundException : Posty5Exception
{
    public Posty5NotFoundException(string message) : base(message)
    {
        StatusCode = 404;
    }
}

/// <summary>
/// Exception thrown when request validation fails
/// </summary>
public class Posty5ValidationException : Posty5Exception
{
    public Posty5ValidationException(string message) : base(message)
    {
        StatusCode = 400;
    }
}

/// <summary>
/// Exception thrown when rate limit is exceeded
/// </summary>
public class Posty5RateLimitException : Posty5Exception
{
    public Posty5RateLimitException(string message) : base(message)
    {
        StatusCode = 429;
    }
}

/// <summary>
/// Exception thrown when a versioned write is refused because the document
/// changed since it was read (HTTP 409, code <c>VERSION_CONFLICT</c>).
/// Read the document again, reapply the change, and write with its new version.
/// </summary>
public class Posty5ConflictException : Posty5Exception
{
    /// <summary>The stable error code, <c>VERSION_CONFLICT</c>.</summary>
    public string Code { get; } = Configuration.Posty5ConcurrencyConst.VersionConflictCode;

    /// <summary>The version the API holds now.</summary>
    public long CurrentVersion { get; }

    /// <summary>The id of the document that conflicted.</summary>
    public string ResourceId { get; }

    public Posty5ConflictException(string message, long currentVersion, string resourceId, string? responseBody = null)
        : base(message, 409, responseBody)
    {
        CurrentVersion = currentVersion;
        ResourceId = resourceId;
    }
}

/// <summary>
/// Exception thrown when a versioned route was called without a version
/// (HTTP 428, code <c>VERSION_REQUIRED</c>). This is a client bug: every SDK
/// update and delete sends one.
/// </summary>
public class Posty5VersionRequiredException : Posty5Exception
{
    /// <summary>The stable error code, <c>VERSION_REQUIRED</c>.</summary>
    public string Code { get; } = Configuration.Posty5ConcurrencyConst.VersionRequiredCode;

    public Posty5VersionRequiredException(string message, string? responseBody = null)
        : base(message, 428, responseBody)
    {
    }
}
