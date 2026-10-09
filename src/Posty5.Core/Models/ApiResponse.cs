namespace Posty5.Core.Models;

/// <summary>
/// Standard API response wrapper
/// </summary>
/// <typeparam name="T">Type of the result data</typeparam>
public class ApiResponse<T>
{
    /// <summary>
    /// Response message
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Indicates if the request was successful
    /// </summary>
    public bool IsSuccess { get; set; }

    /// <summary>
    /// Indicates if there are no more results available (for pagination)
    /// </summary>
    public bool NoMoreOfResult { get; set; }

    /// <summary>
    /// The result data
    /// </summary>
    public T? Result { get; set; }

    /// <summary>
    /// Exception information if any error occurred
    /// </summary>
    public object? Exception { get; set; }

    /// <summary>
    /// The written document's new version, on a versioned update (also sent as
    /// <c>ETag</c>). Null on a read, a create and a delete.
    /// </summary>
    public long? Version { get; set; }

    /// <summary>The new version of every applied document, by id, on a versioned bulk write.</summary>
    public IDictionary<string, long>? Versions { get; set; }

    /// <summary>A stable error or outcome code, e.g. <c>VERSION_CONFLICT</c>.</summary>
    public string? Code { get; set; }
}
