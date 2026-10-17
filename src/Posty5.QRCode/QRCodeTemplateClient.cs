using Posty5.Core.Exceptions;
using Posty5.Core.Http;
using Posty5.Core.Models;
using Posty5.QRCode.Models;

namespace Posty5.QRCode;

/// <summary>
/// QR code templates (designs) — <c>/api/qr-code-template</c>: pick one, design and
/// save your own, render it, read the design catalogue and upload a logo.
/// </summary>
/// <remarks>
/// A template's id is the <c>TemplateId</c> that <see cref="QRCodeClient"/>'s create
/// methods (and the short-link client's) take. Update and delete take the template's
/// <see cref="QRCodeTemplateModel.Version"/> as last read (sent as <c>If-Match</c>).
/// </remarks>
/// <example>
/// <code>
/// var templates = new QRCodeTemplateClient(http);
/// var mine = await templates.ListUserTemplatesAsync();
/// var templateId = mine.Items.FirstOrDefault()?.Id
///     ?? (await templates.ListPublicTemplatesAsync()).Items.First().Id;
///
/// var created = await templates.CreateAsync(new QRCodeTemplateRequestModel
/// {
///     Name = "Menu card",
///     Options = new QRCodeOptionsModel
///     {
///         DotShape = QrDotShapes.Rounded,
///         EyeFrameShape = QrEyeFrameShapes.Leaf,
///         Fill = new QrFillModel
///         {
///             Type = QrFillTypes.Linear,
///             Rotation = 45,
///             Stops = { new() { Offset = 0, Color = "#0059f7" }, new() { Offset = 1, Color = "#7a00f7" } }
///         },
///         Frame = new QrFrameModel { Id = QrFrameIds.BannerBottom, Text = "Scan for menu" }
///     }
/// });
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
        => LookupAsync(QRCodeTemplateRoutes.UserLookup, term, null, pagination, cancellationToken);

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
        => LookupAsync(QRCodeTemplateRoutes.PublicLookup, term, schemeType, pagination, cancellationToken);

    /// <summary>
    /// Save a design as a private template of yours (<c>POST /api/qr-code-template</c>).
    /// </summary>
    /// <remarks>
    /// Shapes, gradients and frames are allowed on every plan. A logo URL from outside
    /// Posty5 is re-hosted on the Posty5 CDN when saved; an uploaded logo must be your own
    /// (<see cref="UploadLogoAsync"/>). Creating a template uses credits as listed in the
    /// account's operation costs.
    /// </remarks>
    /// <param name="data">Name and design</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The new template, with its id and version</returns>
    /// <exception cref="Posty5ValidationException">400: the API refused the design (it names the field).</exception>
    public async Task<QRCodeTemplateModel> CreateAsync(
        QRCodeTemplateRequestModel data,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(data);
        var response = await _http.PostAsync<QRCodeTemplateModel>(
            QRCodeTemplateRoutes.Base,
            new { data.Name, data.Options },
            cancellationToken);

        return response.Result ?? throw new InvalidOperationException("Failed to create the QR code template");
    }

    /// <summary>
    /// Replace a template's name and design (<c>PUT /api/qr-code-template</c>, the id in the body).
    /// Only the template's owner can update it.
    /// </summary>
    /// <remarks>
    /// A stored v2 design keeps its v2 fields when <see cref="QRCodeOptionsModel.DesignVersion"/>
    /// is not sent (the API says so in its message); send <c>DesignVersion = 1</c> to remove them.
    /// </remarks>
    /// <param name="id">Template ID</param>
    /// <param name="data">Name and design</param>
    /// <param name="version">The template's version as last read (<see cref="QRCodeTemplateModel.Version"/>), sent as <c>If-Match</c></param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated template, with its new version</returns>
    /// <exception cref="ArgumentException"><paramref name="id"/> is empty.</exception>
    /// <exception cref="Posty5ConflictException">409: the template changed since <paramref name="version"/> was read.</exception>
    public async Task<QRCodeTemplateModel> UpdateAsync(
        string id,
        QRCodeTemplateRequestModel data,
        long version,
        CancellationToken cancellationToken = default)
    {
        QrRenderQueryHelper.RequireId(id, nameof(id));
        ArgumentNullException.ThrowIfNull(data);
        var response = await _http.PutAsync<QRCodeTemplateModel>(
            QRCodeTemplateRoutes.Base,
            new Dictionary<string, object?> { ["_id"] = id, ["name"] = data.Name, ["options"] = data.Options },
            version,
            cancellationToken);

        return response.Result ?? throw new InvalidOperationException("Failed to update the QR code template");
    }

    /// <summary>
    /// One of your templates with its design (<c>GET /api/qr-code-template/{id}</c>).
    /// Only the template's owner can read it this way.
    /// </summary>
    /// <param name="id">Template ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The template and its design</returns>
    /// <exception cref="ArgumentException"><paramref name="id"/> is empty.</exception>
    public async Task<QRCodeTemplateDetailsModel> GetAsync(string id, CancellationToken cancellationToken = default)
    {
        QrRenderQueryHelper.RequireId(id, nameof(id));
        var response = await _http.GetAsync<QRCodeTemplateDetailsModel>(QRCodeTemplateRoutes.Item(id), null, cancellationToken);
        return response.Result ?? throw new InvalidOperationException("The QR code template was not returned");
    }

    /// <summary>
    /// Your templates with usage counts, a page at a time (<c>GET /api/qr-code-template</c>).
    /// </summary>
    /// <param name="name">Name contains this, case-insensitive; null for all.</param>
    /// <param name="pagination">Cursor and page size; the API's default page size applies when null.</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A page of templates.</returns>
    public async Task<PaginationResponse<QRCodeTemplateModel>> ListAsync(
        string? name = null,
        PaginationParams? pagination = null,
        CancellationToken cancellationToken = default)
    {
        var queryParams = new Dictionary<string, object?>();
        if (!string.IsNullOrEmpty(name))
            queryParams["name"] = name;
        AddPagination(queryParams, pagination);

        var response = await _http.GetAsync<PaginationResponse<QRCodeTemplateModel>>(
            QRCodeTemplateRoutes.Base,
            queryParams,
            cancellationToken);

        return response.Result ?? new PaginationResponse<QRCodeTemplateModel>();
    }

    /// <summary>
    /// Delete one of your templates (<c>DELETE /api/qr-code-template/{id}</c>). Refused
    /// while QR codes or short links still use it.
    /// </summary>
    /// <param name="id">Template ID</param>
    /// <param name="version">The template's version as last read, sent as <c>If-Match</c></param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Deletion confirmation</returns>
    /// <exception cref="ArgumentException"><paramref name="id"/> is empty.</exception>
    /// <exception cref="Posty5ValidationException">400: the template is in use, or not found.</exception>
    /// <exception cref="Posty5ConflictException">409: the template changed since <paramref name="version"/> was read.</exception>
    public async Task<DeleteResponse> DeleteAsync(string id, long version, CancellationToken cancellationToken = default)
    {
        QrRenderQueryHelper.RequireId(id, nameof(id));
        var response = await _http.DeleteAsync<DeleteResponse>(QRCodeTemplateRoutes.Item(id), version, cancellationToken);
        return response.Result ?? new DeleteResponse { Message = "Deleted" };
    }

    /// <summary>
    /// A template's image as PNG, SVG or PDF (<c>GET /api/qr-code-template/{id}/render</c>):
    /// one of yours or any public template.
    /// </summary>
    /// <remarks>
    /// SVG, PDF, a print preset, a DPI or a PNG over 2048 px need vector export on the
    /// account's plan; otherwise this throws <see cref="Posty5Exception"/> with
    /// <see cref="Posty5Exception.StatusCode"/> 403 and the API's message
    /// (<c>This feature is not available on your current plan.</c>). Plain PNG works on
    /// every plan. Rendering is free. The file is buffered in memory; the largest raster
    /// edge is 6000 px.
    /// </remarks>
    /// <param name="id">Template ID</param>
    /// <param name="options">Format, print preset, DPI, size and download; <c>null</c> for the plain PNG.</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The file's bytes, content type and, with <see cref="QrRenderOptions.Download"/>, its name</returns>
    /// <exception cref="ArgumentException"><paramref name="id"/> is empty.</exception>
    /// <exception cref="Posty5NotFoundException">No such template, or a private one that is not yours.</exception>
    public Task<FileResponse> RenderAsync(
        string id,
        QrRenderOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        QrRenderQueryHelper.RequireId(id, nameof(id));
        return _http.GetBytesAsync(QRCodeTemplateRoutes.Render(id), QrRenderQueryHelper.ToQueryParams(options, withDownload: true), cancellationToken);
    }

    /// <summary>
    /// The design catalogue (<c>GET /api/qr-code-template/design-catalogue</c>): frames,
    /// call-to-action presets, print presets, dot and eye shapes, and the design limits.
    /// Needs no API key.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The catalogue</returns>
    public async Task<QrDesignCatalogueModel> GetDesignCatalogueAsync(CancellationToken cancellationToken = default)
    {
        var response = await _http.GetAsync<QrDesignCatalogueModel>(QRCodeTemplateRoutes.DesignCatalogue, null, cancellationToken);
        return response.Result ?? throw new InvalidOperationException("The design catalogue was not returned");
    }

    /// <summary>
    /// A signed upload target for your own logo (<c>POST /api/qr-code-template/logo-upload-url</c>).
    /// <see cref="UploadLogoAsync"/> does this and the upload in one call.
    /// </summary>
    /// <remarks>
    /// PNG, JPEG, WebP or SVG (sanitised before use), up to 2 MB. Logo upload needs a paid
    /// plan; otherwise this throws <see cref="Posty5Exception"/> with
    /// <see cref="Posty5Exception.StatusCode"/> 403 and the API's message. Free.
    /// </remarks>
    /// <param name="fileType">MIME type, see <see cref="QrLogoMimeTypes"/></param>
    /// <param name="fileSize">Size in bytes</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The signed PUT URL and the public URL to save as the logo</returns>
    /// <exception cref="ArgumentException"><paramref name="fileType"/> is empty or <paramref name="fileSize"/> is not positive.</exception>
    public async Task<QrLogoUploadUrlModel> GetLogoUploadUrlAsync(
        string fileType,
        long fileSize,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(fileType))
            throw new ArgumentException("A file type is required.", nameof(fileType));
        if (fileSize <= 0)
            throw new ArgumentException("The file is empty.", nameof(fileSize));

        var response = await _http.PostAsync<QrLogoUploadUrlModel>(
            QRCodeTemplateRoutes.LogoUploadUrl,
            new { fileType, fileSize },
            cancellationToken);

        return response.Result ?? throw new InvalidOperationException("Failed to get a logo upload URL");
    }

    /// <summary>
    /// Upload your own logo: <see cref="GetLogoUploadUrlAsync"/>, then the PUT to the signed URL.
    /// Save the returned URL as <see cref="QRCodeOptionsModel.Logo"/> with
    /// <c>LogoSource = QrLogoSources.Upload</c>.
    /// </summary>
    /// <remarks>
    /// The stream is read to its end (a logo is at most 2 MB) and is not disposed.
    /// Same types, size and plan rule as <see cref="GetLogoUploadUrlAsync"/>.
    /// </remarks>
    /// <param name="content">The logo file</param>
    /// <param name="fileType">MIME type, see <see cref="QrLogoMimeTypes"/></param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The logo's public URL</returns>
    /// <exception cref="ArgumentException">An empty file or file type.</exception>
    /// <exception cref="HttpRequestException">The PUT to the signed URL failed.</exception>
    /// <example>
    /// <code>
    /// await using var file = File.OpenRead("logo.png");
    /// var logoUrl = await templates.UploadLogoAsync(file, QrLogoMimeTypes.Png);
    /// design.Logo = logoUrl;
    /// design.LogoSource = QrLogoSources.Upload;
    /// </code>
    /// </example>
    public async Task<string> UploadLogoAsync(
        Stream content,
        string fileType,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(content);

        using var buffer = new MemoryStream();
        await content.CopyToAsync(buffer, cancellationToken);
        if (buffer.Length == 0)
            throw new ArgumentException("The file is empty.", nameof(content));

        var target = await GetLogoUploadUrlAsync(fileType, buffer.Length, cancellationToken);
        buffer.Position = 0;
        await R2UploadHelper.UploadAsync(target.UploadFileURL, buffer, fileType, cancellationToken);
        return target.FileURL;
    }

    private async Task<PaginationResponse<QRCodeTemplateLookupItemModel>> LookupAsync(
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
        AddPagination(queryParams, pagination);

        var response = await _http.GetAsync<PaginationResponse<QRCodeTemplateLookupItemModel>>(
            path,
            queryParams,
            cancellationToken);

        return response.Result ?? new PaginationResponse<QRCodeTemplateLookupItemModel>();
    }

    /// <summary>Add the cursor and page size to a list query.</summary>
    private static void AddPagination(Dictionary<string, object?> queryParams, PaginationParams? pagination)
    {
        if (pagination == null) return;
        if (!string.IsNullOrEmpty(pagination.Cursor))
            queryParams["cursor"] = pagination.Cursor;
        queryParams["pageSize"] = pagination.PageSize;
    }
}
