using Posty5.Core.Http;
using Posty5.Core.Models;
using Posty5.HtmlHostingVariables.Models;

namespace Posty5.HtmlHostingVariables;

/// <summary>
/// Client for managing HTML hosting variables via Posty5 API
/// </summary>
public class HtmlHostingVariablesClient
{
    private readonly Posty5HttpClient _http;
    private const string BasePath = "/api/html-hosting-variables";

    /// <summary>
    /// Creates a new HTML Hosting Variables client
    /// </summary>
    /// <param name="httpClient">HTTP client instance from Posty5.Core</param>
    public HtmlHostingVariablesClient(Posty5HttpClient httpClient)
    {
        _http = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    }

    /// <summary>
    /// Create a new HTML hosting variable
    /// </summary>
    /// <param name="data">Variable data (name, key, value)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <exception cref="ArgumentException">Thrown when key doesn't start with 'pst5_'</exception>
    /// <example>
    /// <code>
    /// await client.CreateAsync(new CreateHtmlHostingVariableRequest
    /// {
    ///     Name = "API Key",
    ///     Key = "pst5_api_key",
    ///     Value = "sk_test_123456"
    /// });
    /// </code>
    /// </example>
    public async Task CreateAsync(
        CreateHtmlHostingVariableRequest data,
        CancellationToken cancellationToken = default)
    {
        // Validate key prefix
        if (!data.Key.StartsWith("pst5_"))
        {
            throw new ArgumentException(
                $"Key must start with 'pst5_', change to pst5_{data.Key}",
                nameof(data));
        }

        var payload = new
        {
            data.Name,
            data.Key,
            data.Value,
            data.Tag,
            data.RefId,
            createdFrom = "dotnetPackage"
        };

        await _http.PostAsync<object>(BasePath, payload, cancellationToken);
    }

    /// <summary>
    /// Get an HTML hosting variable by ID
    /// </summary>
    /// <param name="id">Variable ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Variable details</returns>
    /// <example>
    /// <code>
    /// var variable = await client.GetAsync("variable_id_123");
    /// Console.WriteLine($"{variable.Key}: {variable.Value}");
    /// </code>
    /// </example>
    public async Task<HtmlHostingVariableModel> GetAsync(
        string id,
        CancellationToken cancellationToken = default)
    {
        var response = await _http.GetAsync<HtmlHostingVariableModel>(
            $"{BasePath}/{id}",
            cancellationToken: cancellationToken);
        
        return response.Result ?? throw new InvalidOperationException("Variable not found");
    }

    /// <summary>
    /// Update an HTML hosting variable
    /// </summary>
    /// <param name="id">Variable ID to update</param>
    /// <param name="data">Updated variable data (name, key, value)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <exception cref="ArgumentException">Thrown when key doesn't start with 'pst5_'</exception>
    /// <example>
    /// <code>
    /// await client.UpdateAsync("variable_id_123", new CreateHtmlHostingVariableRequest
    /// {
    ///     Name = "Updated API Key",
    ///     Key = "pst5_api_key",
    ///     Value = "sk_live_789012"
    /// });
    /// </code>
    /// </example>
    public async Task UpdateAsync(
        string id,
        CreateHtmlHostingVariableRequest data,
        CancellationToken cancellationToken = default)
    {
        // Validate key prefix
        if (!data.Key.StartsWith("pst5_"))
        {
            throw new ArgumentException(
                $"Key must start with 'pst5_', change to pst5_{data.Key}",
                nameof(data));
        }

        await _http.PutAsync<object>($"{BasePath}/{id}", data, cancellationToken);
    }

    /// <summary>
    /// Delete an HTML hosting variable
    /// </summary>
    /// <param name="id">Variable ID to delete</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <example>
    /// <code>
    /// await client.DeleteAsync("variable_id_123");
    /// </code>
    /// </example>
    public async Task DeleteAsync(
        string id,
        CancellationToken cancellationToken = default)
    {
        await _http.DeleteAsync<object>($"{BasePath}/{id}", cancellationToken);
    }

    /// <summary>
    /// List HTML hosting variables with pagination and optional filters
    /// </summary>
    /// <param name="listParams">Filter parameters</param>
    /// <param name="pagination">Pagination parameters</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paginated list of variables</returns>
    /// <example>
    /// <code>
    /// var result = await client.ListAsync(
    ///     new ListHtmlHostingVariablesParams { Tag = "production" },
    ///     new PaginationParams { Page = 1, PageSize = 10 }
    /// );
    /// Console.WriteLine($"Total: {result.Total}");
    /// foreach (var variable in result.Items)
    /// {
    ///     Console.WriteLine($"{variable.Key}: {variable.Value}");
    /// }
    /// </code>
    /// </example>
    public async Task<PaginationResponse<HtmlHostingVariableModel>> ListAsync(
        ListHtmlHostingVariablesParams? listParams = null,
        PaginationParams? pagination = null,
        CancellationToken cancellationToken = default)
    {
        var queryParams = new Dictionary<string, object?>();

        if (listParams != null)
        {
            if (!string.IsNullOrEmpty(listParams.Name))
                queryParams["name"] = listParams.Name;
            if (!string.IsNullOrEmpty(listParams.Key))
                queryParams["key"] = listParams.Key;
            if (!string.IsNullOrEmpty(listParams.Value))
                queryParams["value"] = listParams.Value;
            if (!string.IsNullOrEmpty(listParams.Tag))
                queryParams["tag"] = listParams.Tag;
            if (!string.IsNullOrEmpty(listParams.RefId))
                queryParams["refId"] = listParams.RefId;
        }

        if (pagination != null)
        {
            queryParams["pageNumber"] = pagination.Page;
            queryParams["pageSize"] = pagination.PageSize;
        }

        var response = await _http.GetAsync<PaginationResponse<HtmlHostingVariableModel>>(
            BasePath,
            queryParams,
            cancellationToken);

        return response.Result ?? new PaginationResponse<HtmlHostingVariableModel>();
    }
}
