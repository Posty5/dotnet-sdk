using Posty5.Core.Http;
using Posty5.Core.Models;
using Posty5.QRCode.Models;

namespace Posty5.QRCode;

/// <summary>
/// Client for managing QR codes via Posty5 API
/// </summary>
/// <example>
/// <code>
/// var options = new Posty5Options 
/// { 
///     BaseUrl = "https://api.posty5.com",
///     ApiKey = "your-api-key"
/// };
/// var httpClient = new HttpClient(options);
/// var qrCodeClient = new QRCodeClient(httpClient);
/// 
/// // Create a URL QR code
/// var qrCode = await qrCodeClient.CreateUrlAsync(new CreateUrlQRCodeRequest
/// {
///     Name = "My Website",
///     QrCodeTarget = new UrlQRTarget { Url = "https://example.com" }
/// });
/// </code>
/// </example>
public class QRCodeClient
{
    private readonly Posty5HttpClient _http;
    private const string BasePath = "/api/qr-code";

    /// <summary>
    /// Creates a new QR Code client
    /// </summary>
    /// <param name="httpClient">HTTP client instance from Posty5.Core</param>
    public QRCodeClient(Posty5HttpClient httpClient)
    {
        _http = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    }

    /// <summary>
    /// Create a free text QR code with custom text content
    /// </summary>
    /// <param name="request">Free text QR code creation data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created QR code with ID and landing page URL</returns>
    public async Task<QRCodeModel> CreateFreeTextAsync(
        CreateFreeTextQRCodeRequest request,
        CancellationToken cancellationToken = default)
    {
        var response = await _http.PostAsync<QRCodeModel>(BasePath, request, cancellationToken);
        return response.Result ?? throw new InvalidOperationException("Failed to create QR code");
    }

    /// <summary>
    /// Create an email QR code
    /// </summary>
    /// <param name="request">Email QR code creation data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created QR code</returns>
    public async Task<QRCodeModel> CreateEmailAsync(
        CreateEmailQRCodeRequest request,
        CancellationToken cancellationToken = default)
    {
        var response = await _http.PostAsync<QRCodeModel>(BasePath, request, cancellationToken);
        return response.Result ?? throw new InvalidOperationException("Failed to create QR code");
    }

    /// <summary>
    /// Create a WiFi QR code
    /// </summary>
    /// <param name="request">WiFi QR code creation data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created QR code</returns>
    public async Task<QRCodeModel> CreateWifiAsync(
        CreateWifiQRCodeRequest request,
        CancellationToken cancellationToken = default)
    {
        var response = await _http.PostAsync<QRCodeModel>(BasePath, request, cancellationToken);
        return response.Result ?? throw new InvalidOperationException("Failed to create QR code");
    }

    /// <summary>
    /// Create a phone call QR code
    /// </summary>
    /// <param name="request">Phone call QR code creation data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created QR code</returns>
    public async Task<QRCodeModel> CreateCallAsync(
        CreateCallQRCodeRequest request,
        CancellationToken cancellationToken = default)
    {
        var response = await _http.PostAsync<QRCodeModel>(BasePath, request, cancellationToken);
        return response.Result ?? throw new InvalidOperationException("Failed to create QR code");
    }

    /// <summary>
    /// Create an SMS QR code
    /// </summary>
    /// <param name="request">SMS QR code creation data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created QR code</returns>
    public async Task<QRCodeModel> CreateSmsAsync(
        CreateSmsQRCodeRequest request,
        CancellationToken cancellationToken = default)
    {
        var response = await _http.PostAsync<QRCodeModel>(BasePath, request, cancellationToken);
        return response.Result ?? throw new InvalidOperationException("Failed to create QR code");
    }

    /// <summary>
    /// Create a URL QR code
    /// </summary>
    /// <param name="request">URL QR code creation data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created QR code</returns>
    public async Task<QRCodeModel> CreateUrlAsync(
        CreateUrlQRCodeRequest request,
        CancellationToken cancellationToken = default)
    {
        var response = await _http.PostAsync<QRCodeModel>(BasePath, request, cancellationToken);
        return response.Result ?? throw new InvalidOperationException("Failed to create QR code");
    }

    /// <summary>
    /// Create a geolocation QR code
    /// </summary>
    /// <param name="request">Geolocation QR code creation data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created QR code</returns>
    public async Task<QRCodeModel> CreateGeolocationAsync(
        CreateGeolocationQRCodeRequest request,
        CancellationToken cancellationToken = default)
    {
        var response = await _http.PostAsync<QRCodeModel>(BasePath, request, cancellationToken);
        return response.Result ?? throw new InvalidOperationException("Failed to create QR code");
    }

    /// <summary>
    /// Get a QR code by ID
    /// </summary>
    /// <param name="id">QR code ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>QR code details</returns>
    public async Task<QRCodeModel> GetAsync(string id, CancellationToken cancellationToken = default)
    {
        var response = await _http.GetAsync<QRCodeModel>($"{BasePath}/{id}", cancellationToken: cancellationToken);
        return response.Result ?? throw new InvalidOperationException("QR code not found");
    }

    /// <summary>
    /// List/search QR codes with pagination and filters
    /// </summary>
    /// <param name="listParams">Filter parameters</param>
    /// <param name="pagination">Pagination parameters</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paginated list of QR codes</returns>
    public async Task<PaginationResponse<QRCodeModel>> ListAsync(
        ListQRCodesParams? listParams = null,
        PaginationParams? pagination = null,
        CancellationToken cancellationToken = default)
    {
        var queryParams = new Dictionary<string, object?>();

        if (listParams != null)
        {
            if (!string.IsNullOrEmpty(listParams.Search))
                queryParams["search"] = listParams.Search;
            if (listParams.FromDate.HasValue)
                queryParams["fromDate"] = listParams.FromDate.Value.ToString("o");
            if (listParams.ToDate.HasValue)
                queryParams["toDate"] = listParams.ToDate.Value.ToString("o");
        }

        if (pagination != null)
        {
            queryParams["pageNumber"] = pagination.PageNumber;
            queryParams["pageSize"] = pagination.PageSize;
        }

        var response = await _http.GetAsync<PaginationResponse<QRCodeModel>>(
            BasePath, 
            queryParams, 
            cancellationToken);
        
        return response.Result ?? new PaginationResponse<QRCodeModel>();
    }

    /// <summary>
    /// Update a QR code
    /// </summary>
    /// <param name="id">QR code ID</param>
    /// <param name="request">Update data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated QR code</returns>
    public async Task<QRCodeModel> UpdateAsync(
        string id,
        object request,
        CancellationToken cancellationToken = default)
    {
        var response = await _http.PutAsync<QRCodeModel>($"{BasePath}/{id}", request, cancellationToken);
        return response.Result ?? throw new InvalidOperationException("Failed to update QR code");
    }

    /// <summary>
    /// Delete a QR code
    /// </summary>
    /// <param name="id">QR code ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if deleted successfully</returns>
    public async Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        var response = await _http.DeleteAsync<object>($"{BasePath}/{id}", cancellationToken);
        return response.IsSuccess;
    }
}
