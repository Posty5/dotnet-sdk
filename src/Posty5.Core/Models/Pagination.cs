namespace Posty5.Core.Models;

/// <summary>
/// Pagination parameters for cursor-based list operations
/// </summary>
public class PaginationParams
{
    /// <summary>
    /// Cursor for pagination (opaque string from previous response)
    /// </summary>
    public string? Cursor { get; set; }

    /// <summary>
    /// Number of items per page
    /// </summary>
    public int PageSize { get; set; } = 20;
}

/// <summary>
/// Pagination response with results
/// </summary>
/// <typeparam name="T">Type of items in the list</typeparam>
public class PaginationResponse<T>
{
    /// <summary>
    /// List of items
    /// </summary>
    public List<T> Items { get; set; } = new();

    /// <summary>
    /// Pagination metadata
    /// </summary>
    public PaginationMeta Pagination { get; set; } = new();
}

/// <summary>
/// Cursor-based pagination metadata
/// </summary>
public class PaginationMeta
{
    /// <summary>
    /// Cursor for next page (null if no more data)
    /// </summary>
    public string? NextCursor { get; set; }

    /// <summary>
    /// Cursor for previous page (null if first page)
    /// </summary>
    public string? PreviousCursor { get; set; }

    /// <summary>
    /// Page size
    /// </summary>
    public int PageSize { get; set; }
  
    /// <summary>
    /// Indicates if there are more items available
    /// </summary>
    public bool HasMore { get; set; }
}