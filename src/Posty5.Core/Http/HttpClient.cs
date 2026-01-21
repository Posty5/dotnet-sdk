using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Polly;
using Polly.Retry;
using Posty5.Core.Configuration;
using Posty5.Core.Exceptions;
using Posty5.Core.Models;

namespace Posty5.Core.Http;

/// <summary>
/// HTTP client for making requests to the Posty5 API
/// </summary>
public class Posty5HttpClient : IDisposable
{
    private readonly System.Net.Http.HttpClient _httpClient;
    private readonly Posty5Options _options;
    private readonly JsonSerializerOptions _jsonOptions;

    /// <summary>
    /// Creates a new instance of the HTTP client
    /// </summary>
    /// <param name="options">Configuration options</param>
    public Posty5HttpClient(Posty5Options? options = null)
    {
        _options = options ?? new Posty5Options();
        
        // Configure JSON serialization options
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            WriteIndented = false
        };

        // Create HTTP client
        _httpClient = new System.Net.Http.HttpClient
        {
            BaseAddress = new Uri(_options.BaseUrl),
            Timeout = TimeSpan.FromSeconds(_options.TimeoutSeconds)
        };

        // Set default headers
        _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
        
        if (!string.IsNullOrEmpty(_options.ApiKey))
        {
            _httpClient.DefaultRequestHeaders.Add("X-API-Key", _options.ApiKey);
        }
    }

    /// <summary>
    /// Set or update the API key
    /// </summary>
    /// <param name="apiKey">API key</param>
    public void SetApiKey(string apiKey)
    {
        _options.ApiKey = apiKey;
        _httpClient.DefaultRequestHeaders.Remove("X-API-Key");
        _httpClient.DefaultRequestHeaders.Add("X-API-Key", apiKey);
    }

    /// <summary>
    /// Perform a GET request
    /// </summary>
    /// <typeparam name="T">Response type</typeparam>
    /// <param name="path">API endpoint path</param>
    /// <param name="queryParams">Query parameters</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>API response</returns>
    public async Task<ApiResponse<T>> GetAsync<T>(
        string path, 
        Dictionary<string, object?>? queryParams = null,
        CancellationToken cancellationToken = default)
    {
        var url = BuildUrl(path, queryParams);
        
        if (_options.Debug)
        {
            Console.WriteLine($"[Posty5 SDK] GET {url}");
        }

        try
        {
            var response = await _httpClient.GetAsync(url, cancellationToken);
            return await ProcessResponseAsync<T>(response);
        }
        catch (Exception ex) when (ex is not Posty5Exception)
        {
            throw new Posty5Exception($"GET request to {path} failed", ex);
        }
    }

    /// <summary>
    /// Perform a POST request
    /// </summary>
    /// <typeparam name="T">Response type</typeparam>
    /// <param name="path">API endpoint path</param>
    /// <param name="data">Request body data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>API response</returns>
    public async Task<ApiResponse<T>> PostAsync<T>(
        string path, 
        object data,
        CancellationToken cancellationToken = default)
    {
        if (_options.Debug)
        {
            Console.WriteLine($"[Posty5 SDK] POST {path}");
        }

        try
        {
            var json = JsonSerializer.Serialize(data, _jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            var response = await _httpClient.PostAsync(path, content, cancellationToken);
            return await ProcessResponseAsync<T>(response);
        }
        catch (Exception ex) when (ex is not Posty5Exception)
        {
            throw new Posty5Exception($"POST request to {path} failed", ex);
        }
    }

    /// <summary>
    /// Perform a PUT request
    /// </summary>
    /// <typeparam name="T">Response type</typeparam>
    /// <param name="path">API endpoint path</param>
    /// <param name="data">Request body data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>API response</returns>
    public async Task<ApiResponse<T>> PutAsync<T>(
        string path, 
        object data,
        CancellationToken cancellationToken = default)
    {
        if (_options.Debug)
        {
            Console.WriteLine($"[Posty5 SDK] PUT {path}");
        }

        try
        {
            var json = JsonSerializer.Serialize(data, _jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            var response = await _httpClient.PutAsync(path, content, cancellationToken);
            return await ProcessResponseAsync<T>(response);
        }
        catch (Exception ex) when (ex is not Posty5Exception)
        {
            throw new Posty5Exception($"PUT request to {path} failed", ex);
        }
    }

    /// <summary>
    /// Perform a DELETE request
    /// </summary>
    /// <typeparam name="T">Response type</typeparam>
    /// <param name="path">API endpoint path</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>API response</returns>
    public async Task<ApiResponse<T>> DeleteAsync<T>(
        string path,
        CancellationToken cancellationToken = default)
    {
        if (_options.Debug)
        {
            Console.WriteLine($"[Posty5 SDK] DELETE {path}");
        }

        try
        {
            var response = await _httpClient.DeleteAsync(path, cancellationToken);
            return await ProcessResponseAsync<T>(response);
        }
        catch (Exception ex) when (ex is not Posty5Exception)
        {
            throw new Posty5Exception($"DELETE request to {path} failed", ex);
        }
    }

    private async Task<ApiResponse<T>> ProcessResponseAsync<T>(HttpResponseMessage response)
    {
        var content = await response.Content.ReadAsStringAsync();

        if (_options.Debug)
        {
            Console.WriteLine($"[Posty5 SDK] Response: {response.StatusCode}");
        }

        if (!response.IsSuccessStatusCode)
        {
            HandleErrorResponse(response.StatusCode, content);
        }

        try
        {
            var apiResponse = JsonSerializer.Deserialize<ApiResponse<T>>(content, _jsonOptions);
            return apiResponse ?? throw new Posty5Exception("Failed to deserialize response");
        }
        catch (JsonException ex)
        {
            throw new Posty5Exception("Failed to parse API response", ex);
        }
    }

    private void HandleErrorResponse(HttpStatusCode statusCode, string content)
    {
        var message = $"API request failed with status code {statusCode}";

        throw statusCode switch
        {
            HttpStatusCode.Unauthorized => new Posty5AuthenticationException("Authentication failed. Please check your API key."),
            HttpStatusCode.NotFound => new Posty5NotFoundException("The requested resource was not found."),
            HttpStatusCode.BadRequest => new Posty5ValidationException($"Request validation failed: {content}"),
            HttpStatusCode.TooManyRequests => new Posty5RateLimitException("Rate limit exceeded. Please try again later."),
            _ => new Posty5Exception(message, (int)statusCode, content)
        };
    }

    private string BuildUrl(string path, Dictionary<string, object?>? queryParams)
    {
        if (queryParams == null || queryParams.Count == 0)
        {
            return path;
        }

        var queryString = string.Join("&", 
            queryParams
                .Where(kvp => kvp.Value != null)
                .Select(kvp => $"{Uri.EscapeDataString(kvp.Key)}={Uri.EscapeDataString(kvp.Value!.ToString()!)}"));

        return string.IsNullOrEmpty(queryString) ? path : $"{path}?{queryString}";
    }

    public void Dispose()
    {
        _httpClient?.Dispose();
        GC.SuppressFinalize(this);
    }
}
