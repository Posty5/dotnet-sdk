namespace Posty5.Core.Models;

/// <summary>
/// Pagination parameters for list operations
/// </summary>
public class PaginationParams
{
    /// <summary>
    /// Page number (starts from 0)
    /// </summary>
    public int Page { get; set; } = 0;

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
    /// Pagination Meta
    /// </summary>
    public PaginationMeta Pagination { get; set; }
}

public class PaginationMeta
{

           /// <summary>
    /// Total count of items
    /// </summary>
    public int TotalCount { get; set; }

    /// <summary>
    /// Current page number
    /// </summary>
    public int Page { get; set; }

    /// <summary>
    /// Page size
    /// </summary>
    public int PageSize { get; set; }
  
    public int TotalPages { get; set; }
    public bool NoMoreOfResults { get; set; }
}