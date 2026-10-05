using Posty5.Core.Http;
using Posty5.Core.Models;
using Posty5.QRCode.Models;

namespace Posty5.QRCode;

/// <summary>
/// List the QR code templates a QR code or short link can be created with —
/// <c>/api/qr-code-template</c>.
/// </summary>
/// <remarks>
/// Read-only. A template's id is the <c>TemplateId</c> that
/// <see cref="QRCodeClient"/>'s create methods (and the short-link client's)
/// take. Designing a template is done in the dashboard.
/// </remarks>
/// <example>
/// <code>
/// var templates = new QRCodeTemplateClient(http);
/// var mine = await templates.ListUserTemplatesAsync();
/// var templateId = mine.Items.FirstOrDefault()?.Id
///     ?? (await templates.ListPublicTemplatesAsync()).Items.First().Id;
/// </code>
/// </example>
public class QRCodeTemplateClient
{
    private readonly Posty5HttpClient _http;

    /// <summary>
    /// Creates a new QR code template client.
    /// </summary>
    /// <param name="httpClient">HTTP client instance from Posty5.Core</param>
    public QRCodeTemplateClient(Posty5HttpClient httpClient)
    {
        _http = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    }

    /// <summary>
    /// The templates you have made, a page at a time.
    /// </summary>
    /// <param name="term">Name contains this, case-insensitive; null for all.</param>
    /// <param name="pagination">Cursor and page size; the API's default page size applies when null.</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A page of templates.</returns>
    public Task<PaginationResponse<QRCodeTemplateLookupItemModel>> ListUserTemplatesAsync(
        string? term = null,
        PaginationParams? pagination = null,
        CancellationToken cancellationToken = default)
        => ListAsync(QRCodeTemplateRoutes.UserLookup, term, null, pagination, cancellationToken);

    /// <summary>
    /// Posty5's public templates, a page at a time. Needs no API key.
    /// </summary>
    /// <param name="term">Name contains this, case-insensitive; null for all.</param>
    /// <param name="schemeType">Only templates of this scheme type; null for all.</param>
    /// <param name="pagination">Cursor and page size; the API's default page size applies when null.</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A page of templates.</returns>
    public Task<PaginationResponse<QRCodeTemplateLookupItemModel>> ListPublicTemplatesAsync(
        string? term = null,
        string? schemeType = null,
        PaginationParams? pagination = null,
        CancellationToken cancellationToken = default)
        => ListAsync(QRCodeTemplateRoutes.PublicLookup, term, schemeType, pagination, cancellationToken);

    private async Task<PaginationResponse<QRCodeTemplateLookupItemModel>> ListAsync(
        string path,
        string? term,
        string? schemeType,
        PaginationParams? pagination,
        CancellationToken cancellationToken)
    {
        var queryParams = new Dictionary<string, object?>();
        if (!string.IsNullOrEmpty(term))
            queryParams["term"] = term;
        if (!string.IsNullOrEmpty(schemeType))
            queryParams["schemeType"] = schemeType;

        if (pagination != null)
        {
            if (!string.IsNullOrEmpty(pagination.Cursor))
                queryParams["cursor"] = pagination.Cursor;
            queryParams["pageSize"] = pagination.PageSize;
        }

        var response = await _http.GetAsync<PaginationResponse<QRCodeTemplateLookupItemModel>>(
            path,
            queryParams,
            cancellationToken);

        return response.Result ?? new PaginationResponse<QRCodeTemplateLookupItemModel>();
    }
}
