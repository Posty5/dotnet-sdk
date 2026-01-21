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
