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
        _httpClient.DefaultRequestHeaders.Add("Accept", Posty5HttpDefaults.AcceptJson);
        _httpClient.DefaultRequestHeaders.Add(Posty5HttpDefaults.ClientHeader, Posty5ClientIdentity.HeaderValue);

        // The caller's own headers go on after the SDK's, so a caller-supplied
        // X-Posty5-Client replaces the default label; X-API-Key is refused.
        try
        {
            ApplyDefaultHeaders(_options.DefaultHeaders);
        }
        catch
        {
            _httpClient.Dispose();
            throw;
        }

        if (!string.IsNullOrEmpty(_options.ApiKey))
        {
            _httpClient.DefaultRequestHeaders.Add(Posty5HttpDefaults.ApiKeyHeader, _options.ApiKey);
        }
    }

    /// <summary>
    /// The <c>createdFrom</c> label configured on <see cref="Posty5Options.CreatedFrom"/>,
    /// or null when none was set. Clients stamp
    /// <see cref="ResolveCreatedFrom"/>, which applies their own default.
    /// </summary>
    public string? CreatedFrom => string.IsNullOrWhiteSpace(_options.CreatedFrom) ? null : _options.CreatedFrom;

    /// <summary>
    /// The <c>createdFrom</c> label a client should stamp on a record it creates:
    /// <see cref="Posty5Options.CreatedFrom"/> when set, otherwise
    /// <paramref name="packageDefault"/> — the label that package has always sent.
    /// </summary>
    /// <param name="packageDefault">The calling package's own label, from <see cref="CreatedFromDefaults"/>.</param>
    /// <returns>The label to send.</returns>
    public string ResolveCreatedFrom(string packageDefault) => CreatedFrom ?? packageDefault;

    /// <summary>
    /// Set or update the API key
    /// </summary>
    /// <param name="apiKey">API key</param>
    public void SetApiKey(string apiKey)
    {
        _options.ApiKey = apiKey;
        _httpClient.DefaultRequestHeaders.Remove(Posty5HttpDefaults.ApiKeyHeader);
        _httpClient.DefaultRequestHeaders.Add(Posty5HttpDefaults.ApiKeyHeader, apiKey);
    }

    /// <summary>
    /// Add <see cref="Posty5Options.DefaultHeaders"/> to every request, refusing
    /// the API-key header and anything that is not a request header.
    /// </summary>
    private void ApplyDefaultHeaders(Dictionary<string, string>? headers)
    {
        if (headers == null) return;

        foreach (var (name, value) in headers)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("A default header needs a name.", nameof(Posty5Options.DefaultHeaders));

            if (string.Equals(name.Trim(), Posty5HttpDefaults.ApiKeyHeader, StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException(
                    $"{Posty5HttpDefaults.ApiKeyHeader} cannot be set through DefaultHeaders. Use Posty5Options.ApiKey or SetApiKey.",
                    nameof(Posty5Options.DefaultHeaders));

            // Contains() answers false for a name that is not a request header,
            // where Remove() would throw — the TryAdd below reports that case.
            if (_httpClient.DefaultRequestHeaders.Contains(name))
                _httpClient.DefaultRequestHeaders.Remove(name);
            if (!_httpClient.DefaultRequestHeaders.TryAddWithoutValidation(name, value))
                throw new ArgumentException(
                    $"'{name}' cannot be sent as a default request header.",
                    nameof(Posty5Options.DefaultHeaders));
        }
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
    public Task<ApiResponse<T>> PostAsync<T>(
        string path,
        object data,
        CancellationToken cancellationToken = default)
        => PostAsync<T>(path, data, null, cancellationToken);

    /// <summary>
    /// Perform a POST request with headers for this request only
    /// (e.g. <c>Idempotency-Key</c>), sent on top of the client's default headers.
    /// </summary>
    /// <typeparam name="T">Response type</typeparam>
    /// <param name="path">API endpoint path</param>
    /// <param name="data">Request body data</param>
    /// <param name="headers">Per-request headers; null for none</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>API response</returns>
    public async Task<ApiResponse<T>> PostAsync<T>(
        string path,
        object data,
        IDictionary<string, string>? headers,
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
            
            var response = await SendAsync(HttpMethod.Post, path, content, headers, cancellationToken);
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
    public Task<ApiResponse<T>> PutAsync<T>(
        string path,
        object data,
        CancellationToken cancellationToken = default)
        => PutAsync<T>(path, data, null, cancellationToken);

    /// <summary>
    /// Perform a PUT request with headers for this request only.
    /// </summary>
    /// <typeparam name="T">Response type</typeparam>
    /// <param name="path">API endpoint path</param>
    /// <param name="data">Request body data</param>
    /// <param name="headers">Per-request headers; null for none</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>API response</returns>
    public async Task<ApiResponse<T>> PutAsync<T>(
        string path,
        object data,
        IDictionary<string, string>? headers,
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
            
            var response = await SendAsync(HttpMethod.Put, path, content, headers, cancellationToken);
            return await ProcessResponseAsync<T>(response);
        }
        catch (Exception ex) when (ex is not Posty5Exception)
        {
            throw new Posty5Exception($"PUT request to {path} failed", ex);
        }
    }

    /// <summary>
    /// Perform a PATCH request
    /// </summary>
    /// <typeparam name="T">Response type</typeparam>
    /// <param name="path">API endpoint path</param>
    /// <param name="data">Request body data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>API response</returns>
    public async Task<ApiResponse<T>> PatchAsync<T>(
        string path,
        object data,
        CancellationToken cancellationToken = default)
    {
        if (_options.Debug)
        {
            Console.WriteLine($"[Posty5 SDK] PATCH {path}");
        }

        try
        {
            var json = JsonSerializer.Serialize(data, _jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PatchAsync(path, content, cancellationToken);
            return await ProcessResponseAsync<T>(response);
        }
        catch (Exception ex) when (ex is not Posty5Exception)
        {
            throw new Posty5Exception($"PATCH request to {path} failed", ex);
        }
    }

    /// <summary>
    /// Perform a GET request for a file download.
    /// </summary>
    /// <remarks>
    /// Separate from <see cref="GetAsync{T}"/> rather than a flag on it: a
    /// download answers with the file itself, not with the
    /// <c>{ message, result }</c> envelope every other endpoint uses, so it
    /// cannot share the return type.
    /// </remarks>
    /// <param name="path">API endpoint path</param>
    /// <param name="queryParams">Query parameters</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The file's bytes, content type and suggested filename</returns>
    public Task<FileResponse> GetBytesAsync(
        string path,
        Dictionary<string, object?>? queryParams = null,
        CancellationToken cancellationToken = default)
        => GetBytesAsync(path, queryParams, null, cancellationToken);

    /// <summary>
    /// Perform a GET request for a file download, with headers for this request only.
    /// </summary>
    /// <param name="path">API endpoint path</param>
    /// <param name="queryParams">Query parameters</param>
    /// <param name="headers">Per-request headers; null for none</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The file's bytes, content type and suggested filename</returns>
    public async Task<FileResponse> GetBytesAsync(
        string path,
        Dictionary<string, object?>? queryParams,
        IDictionary<string, string>? headers,
        CancellationToken cancellationToken = default)
    {
        var url = BuildUrl(path, queryParams);

        if (_options.Debug)
        {
            Console.WriteLine($"[Posty5 SDK] GET (binary) {url}");
        }

        try
        {
            var response = await SendAsync(HttpMethod.Get, url, null, headers, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                HandleErrorResponse(response.StatusCode, await response.Content.ReadAsStringAsync());
            }

            return new FileResponse
            {
                Data = await response.Content.ReadAsByteArrayAsync(),
                ContentType = response.Content.Headers.ContentType?.MediaType ?? "application/octet-stream",
                FileName = ParseFileName(response.Content.Headers.ContentDisposition?.FileName)
            };
        }
        catch (Exception ex) when (ex is not Posty5Exception)
        {
            throw new Posty5Exception($"GET request to {path} failed", ex);
        }
    }

    /// <summary>
    /// Perform a DELETE request
    /// </summary>
    /// <typeparam name="T">Response type</typeparam>
    /// <param name="path">API endpoint path</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>API response</returns>
    public Task<ApiResponse<T>> DeleteAsync<T>(
        string path,
        CancellationToken cancellationToken = default)
        => DeleteAsync<T>(path, null, cancellationToken);

    /// <summary>
    /// Perform a DELETE request with headers for this request only.
    /// </summary>
    /// <typeparam name="T">Response type</typeparam>
    /// <param name="path">API endpoint path</param>
    /// <param name="headers">Per-request headers; null for none</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>API response</returns>
    public async Task<ApiResponse<T>> DeleteAsync<T>(
        string path,
        IDictionary<string, string>? headers,
        CancellationToken cancellationToken = default)
    {
        if (_options.Debug)
        {
            Console.WriteLine($"[Posty5 SDK] DELETE {path}");
        }

        try
        {
            var response = await SendAsync(HttpMethod.Delete, path, null, headers, cancellationToken);
            return await ProcessResponseAsync<T>(response);
        }
        catch (Exception ex) when (ex is not Posty5Exception)
        {
            throw new Posty5Exception($"DELETE request to {path} failed", ex);
        }
    }

    /// <summary>
    /// Send one request with optional per-request headers. The API-key header
    /// cannot be replaced here, as with <see cref="Posty5Options.DefaultHeaders"/>.
    /// </summary>
    private Task<HttpResponseMessage> SendAsync(
        HttpMethod method,
        string url,
        HttpContent? content,
        IDictionary<string, string>? headers,
        CancellationToken cancellationToken)
    {
        var request = new HttpRequestMessage(method, url) { Content = content };

        if (headers != null)
        {
            foreach (var (name, value) in headers)
            {
                if (string.IsNullOrWhiteSpace(name))
                    throw new ArgumentException("A request header needs a name.", nameof(headers));
                if (string.Equals(name.Trim(), Posty5HttpDefaults.ApiKeyHeader, StringComparison.OrdinalIgnoreCase))
                    throw new ArgumentException(
                        $"{Posty5HttpDefaults.ApiKeyHeader} cannot be set per request. Use Posty5Options.ApiKey or SetApiKey.",
                        nameof(headers));
                if (!request.Headers.TryAddWithoutValidation(name, value))
                    throw new ArgumentException($"'{name}' cannot be sent as a request header.", nameof(headers));
            }
        }

        return _httpClient.SendAsync(request, cancellationToken);
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

    /// <summary>
    /// The server sends the filename percent-encoded and wrapped in quotes.
    /// Decode it, and keep the raw value if the encoding is malformed rather
    /// than failing a download that otherwise arrived intact.
    /// </summary>
    private static string? ParseFileName(string? headerValue)
    {
        if (string.IsNullOrWhiteSpace(headerValue)) return null;

        var trimmed = headerValue.Trim('"');
        try
        {
            return Uri.UnescapeDataString(trimmed);
        }
        catch (UriFormatException)
        {
            return trimmed;
        }
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
