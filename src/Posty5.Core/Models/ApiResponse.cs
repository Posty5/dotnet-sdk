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
}
