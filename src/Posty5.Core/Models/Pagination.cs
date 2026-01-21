namespace Posty5.Core.Models;

/// <summary>
/// Pagination parameters for list operations
/// </summary>
public class PaginationParams
{
    /// <summary>
    /// Page number (starts from 0)
    /// </summary>
    public int PageNumber { get; set; } = 0;

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
    /// Total count of items
    /// </summary>
    public int TotalCount { get; set; }

    /// <summary>
    /// Current page number
    /// </summary>
    public int PageNumber { get; set; }

    /// <summary>
    /// Page size
    /// </summary>
    public int PageSize { get; set; }

    /// <summary>
    /// Indicates if there are more pages available
    /// </summary>
    public bool HasNextPage => (PageNumber + 1) * PageSize < TotalCount;
}
