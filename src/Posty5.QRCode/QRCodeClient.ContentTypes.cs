using Posty5.Core.Configuration;
using Posty5.Core.Helpers;
using Posty5.Core.Http;
using Posty5.QRCode.Models;

namespace Posty5.QRCode;

/// <summary>
/// The vCard, event, WhatsApp, review, social, app store and file content types. Each sends
/// <c>qrCodeTarget: { type, [type]: content }</c> and never <c>options.text</c>:
/// the API builds and escapes the encoded content from the target.
/// </summary>
public partial class QRCodeClient
{
    #region Content-type Create Methods

    /// <summary>Create a vCard (contact card) QR code. <c>FirstName</c> or <c>Organization</c> is required.</summary>
    /// <example>
    /// <code>
    /// var qr = await qrCodeClient.CreateVCardAsync(new QRCodeCreateVCardRequestModel
    /// {
    ///     Name = "My card",
    ///     TemplateId = "template_123",
    ///     VCard = new QRCodeVCardTargetModel
    ///     {
    ///         FirstName = "Ada",
    ///         Phones = new() { new QRCodeVCardPhoneModel { Kind = QRCodeVCardPhoneKinds.Mobile, Number = "+201000000000" } }
    ///     }
    /// });
    /// </code>
    /// </example>
    public Task<QRCodeModel> CreateVCardAsync(QRCodeCreateVCardRequestModel data, CancellationToken cancellationToken = default)
        => SendContentTypeAsync(HttpMethod.Post, "vcard", null, data, data?.Name, data?.VCard, cancellationToken);

    /// <summary>Create a calendar event QR code. <c>StartsAt</c> / <c>EndsAt</c> are sent as ISO 8601.</summary>
    /// <example>
    /// <code>
    /// var qr = await qrCodeClient.CreateEventAsync(new QRCodeCreateEventRequestModel
    /// {
    ///     Name = "Launch",
    ///     TemplateId = "template_123",
    ///     Event = new QRCodeEventTargetModel { Title = "Launch", StartsAt = DateTimeOffset.Parse("2026-11-01T18:00:00Z") }
    /// });
    /// </code>
    /// </example>
    public Task<QRCodeModel> CreateEventAsync(QRCodeCreateEventRequestModel data, CancellationToken cancellationToken = default)
        => SendContentTypeAsync(HttpMethod.Post, "event", null, data, data?.Name, data?.Event, cancellationToken);

    /// <summary>Create a WhatsApp chat QR code (<c>https://wa.me/…</c>).</summary>
    public Task<QRCodeModel> CreateWhatsAppAsync(QRCodeCreateWhatsAppRequestModel data, CancellationToken cancellationToken = default)
        => SendContentTypeAsync(HttpMethod.Post, "whatsapp", null, data, data?.Name, data?.WhatsApp, cancellationToken);

    /// <summary>Create a review QR code (Google place ID or a review page URL).</summary>
    public Task<QRCodeModel> CreateReviewAsync(QRCodeCreateReviewRequestModel data, CancellationToken cancellationToken = default)
        => SendContentTypeAsync(HttpMethod.Post, "review", null, data, data?.Name, data?.Review, cancellationToken);

    /// <summary>Create a social profiles QR code. A static code takes one profile.</summary>
    public Task<QRCodeModel> CreateSocialAsync(QRCodeCreateSocialRequestModel data, CancellationToken cancellationToken = default)
        => SendContentTypeAsync(HttpMethod.Post, "social", null, data, data?.Name, data?.Social, cancellationToken);

    /// <summary>
    /// Create an app store QR code (dynamic-only): a scan goes to the store of the scanning
    /// device, else to <c>FallbackUrl</c>. <c>Mode = Static</c> throws <see cref="ArgumentException"/> before any call.
    /// </summary>
    /// <example>
    /// <code>
    /// var qr = await qrCodeClient.CreateAppStoreAsync(new QRCodeCreateAppStoreRequestModel
    /// {
    ///     Name = "My app",
    ///     TemplateId = "template_123",
    ///     AppStore = new QRCodeAppStoreTargetModel
    ///     {
    ///         AndroidUrl = "https://play.google.com/store/apps/details?id=com.example",
    ///         IosUrl = "https://apps.apple.com/app/id123456789",
    ///         FallbackUrl = "https://example.com/app"
    ///     }
    /// });
    /// </code>
    /// </example>
    public Task<QRCodeModel> CreateAppStoreAsync(QRCodeCreateAppStoreRequestModel data, CancellationToken cancellationToken = default)
    {
        EnsureDynamicOnly(data, "appStore");
        return SendContentTypeAsync(HttpMethod.Post, "appStore", null, data, data.Name, data.AppStore, cancellationToken);
    }

    /// <summary>
    /// Create a file QR code (dynamic-only): a hosted PDF or image (see <see cref="QRCodeFileMimeTypes"/>).
    /// One call does <c>POST /api/qr-code/file/upload-url</c>, the PUT to the signed URL (valid 60 s,
    /// retried once on a network error) and <c>POST /api/qr-code/file</c> with the returned <c>bucketFilePath</c>.
    /// </summary>
    /// <param name="data">Common fields and <c>File.FileName</c>; <c>File.SizeBytes</c> is required for a non-seekable stream</param>
    /// <param name="content">The file content</param>
    /// <param name="contentType">The file's MIME type</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <exception cref="ArgumentException">Static mode, a missing content type, an empty file, or a non-seekable stream without <c>File.SizeBytes</c>; thrown before any call</exception>
    /// <exception cref="QRCodeFileUploadExpiredException">The signed URL expired before the upload finished</exception>
    /// <example>
    /// <code>
    /// await using var pdf = File.OpenRead("menu.pdf");
    /// var qr = await qrCodeClient.CreateFileAsync(
    ///     new QRCodeCreateFileRequestModel { Name = "Menu", TemplateId = "template_123", File = new() { FileName = "menu.pdf" } },
    ///     pdf,
    ///     QRCodeFileMimeTypes.Pdf);
    /// </code>
    /// </example>
    public async Task<QRCodeModel> CreateFileAsync(QRCodeCreateFileRequestModel data, Stream content, string contentType, CancellationToken cancellationToken = default)
    {
        EnsureDynamicOnly(data, "file");
        ArgumentNullException.ThrowIfNull(content);
        var bucketFilePath = await UploadQRCodeFileAsync(data.File, content, contentType, cancellationToken);
        return await SendContentTypeAsync(HttpMethod.Post, "file", null, data, data.Name, FileTarget(data.File, bucketFilePath), cancellationToken);
    }

    #endregion

    #region Content-type Update Methods

    /// <summary>Update a vCard QR code.</summary>
    public Task<QRCodeModel> UpdateVCardAsync(string id, QRCodeUpdateVCardRequestModel data, CancellationToken cancellationToken = default)
        => SendContentTypeAsync(HttpMethod.Put, "vcard", id, data, data?.Name, data?.VCard, cancellationToken);

    /// <summary>Update a calendar event QR code.</summary>
    public Task<QRCodeModel> UpdateEventAsync(string id, QRCodeUpdateEventRequestModel data, CancellationToken cancellationToken = default)
        => SendContentTypeAsync(HttpMethod.Put, "event", id, data, data?.Name, data?.Event, cancellationToken);

    /// <summary>Update a WhatsApp chat QR code.</summary>
    public Task<QRCodeModel> UpdateWhatsAppAsync(string id, QRCodeUpdateWhatsAppRequestModel data, CancellationToken cancellationToken = default)
        => SendContentTypeAsync(HttpMethod.Put, "whatsapp", id, data, data?.Name, data?.WhatsApp, cancellationToken);

    /// <summary>Update a review QR code.</summary>
    public Task<QRCodeModel> UpdateReviewAsync(string id, QRCodeUpdateReviewRequestModel data, CancellationToken cancellationToken = default)
        => SendContentTypeAsync(HttpMethod.Put, "review", id, data, data?.Name, data?.Review, cancellationToken);

    /// <summary>Update a social profiles QR code.</summary>
    public Task<QRCodeModel> UpdateSocialAsync(string id, QRCodeUpdateSocialRequestModel data, CancellationToken cancellationToken = default)
        => SendContentTypeAsync(HttpMethod.Put, "social", id, data, data?.Name, data?.Social, cancellationToken);

    /// <summary>Update an app store QR code. <c>Mode = Static</c> throws <see cref="ArgumentException"/> before any call.</summary>
    public Task<QRCodeModel> UpdateAppStoreAsync(string id, QRCodeUpdateAppStoreRequestModel data, CancellationToken cancellationToken = default)
    {
        EnsureDynamicOnly(data, "appStore");
        return SendContentTypeAsync(HttpMethod.Put, "appStore", id, data, data.Name, data.AppStore, cancellationToken);
    }

    /// <summary>
    /// Update a file QR code. With <paramref name="content"/>, the new file is uploaded first and
    /// replaces the stored one; without it, the stored file is kept (only <c>File.FileName</c> and
    /// the common fields change).
    /// </summary>
    /// <param name="id">QR code ID</param>
    /// <param name="data">Common fields and <c>File.FileName</c></param>
    /// <param name="content">New file content, or <c>null</c> to keep the stored file</param>
    /// <param name="contentType">The new file's MIME type; required with <paramref name="content"/></param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <example>
    /// <code>
    /// await qrCodeClient.UpdateFileAsync("qr_code_id", new QRCodeUpdateFileRequestModel { Name = "Menu", TemplateId = "template_123", File = new() { FileName = "menu-2026.pdf" } });
    /// </code>
    /// </example>
    public async Task<QRCodeModel> UpdateFileAsync(string id, QRCodeUpdateFileRequestModel data, Stream? content = null, string? contentType = null, CancellationToken cancellationToken = default)
    {
        EnsureDynamicOnly(data, "file");
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("QR code id is required", nameof(id));
        var bucketFilePath = content == null ? null : await UploadQRCodeFileAsync(data.File, content, contentType, cancellationToken);
        return await SendContentTypeAsync(HttpMethod.Put, "file", id, data, data.Name, FileTarget(data.File, bucketFilePath), cancellationToken);
    }

    #endregion

    /// <summary>App store and file codes cannot be static: refuse before any call.</summary>
    private static void EnsureDynamicOnly(QRCodeRequestBaseModel data, string type)
    {
        ArgumentNullException.ThrowIfNull(data);
        if (data.Mode == QRCodeMode.Static)
            throw new ArgumentException($"{type} QR codes are dynamic-only; omit Mode or use QRCodeMode.Dynamic", nameof(data));
    }

    /// <summary><c>qrCodeTarget.file</c>: <c>fileName</c> and, when a file was uploaded, <c>bucketFilePath</c>. <c>mimeType</c> is set by the API.</summary>
    private static Dictionary<string, object?> FileTarget(QRCodeFileInputModel? file, string? bucketFilePath)
    {
        var target = new Dictionary<string, object?>();
        if (file?.FileName != null) target["fileName"] = file.FileName;
        if (bucketFilePath != null) target["bucketFilePath"] = bucketFilePath;
        return target;
    }

    /// <summary>
    /// Steps 1-2 of a <c>file</c> code: ask for a signed upload URL, then PUT the content to it.
    /// Returns the <c>bucketFilePath</c> to send on create / update. The PUT is retried once on
    /// a network error while the URL is still valid; a failure after it expired throws
    /// <see cref="QRCodeFileUploadExpiredException"/>.
    /// </summary>
    private async Task<string> UploadQRCodeFileAsync(QRCodeFileInputModel? file, Stream content, string? contentType, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(contentType))
            throw new ArgumentException("The file content type is required", nameof(contentType));
        long sizeBytes = content.CanSeek
            ? content.Length - content.Position
            : file?.SizeBytes ?? throw new ArgumentException("File.SizeBytes is required for a non-seekable stream", nameof(content));
        if (sizeBytes <= 0)
            throw new ArgumentException("The file is empty", nameof(content));

        // Buffered once so a retried PUT sends the same bytes.
        byte[] bytes;
        using (var buffer = new MemoryStream())
        {
            await content.CopyToAsync(buffer, cancellationToken);
            bytes = buffer.ToArray();
        }

        var fileName = string.IsNullOrWhiteSpace(file?.FileName) ? "file" : file.FileName;
        var response = await _http.PostAsync<QRCodeFileUploadTicketModel>(
            $"{BasePath}/file/upload-url",
            new { fileName, mimeType = contentType, sizeBytes },
            cancellationToken);
        var ticket = response.Result ?? throw new InvalidOperationException("Failed to get a file upload URL");
        var lifetime = ticket.ExpiresInSeconds > 0 ? ticket.ExpiresInSeconds : 60;
        var expiresAt = DateTimeOffset.UtcNow.AddSeconds(lifetime);

        for (var attempt = 1; ; attempt++)
        {
            try
            {
                await R2UploadHelper.UploadAsync(ticket.UploadFileURL, new MemoryStream(bytes, writable: false), contentType, cancellationToken);
                return ticket.BucketFilePath;
            }
            catch (HttpRequestException error)
            {
                if (DateTimeOffset.UtcNow >= expiresAt)
                    throw new QRCodeFileUploadExpiredException(lifetime, error);
                // No status code means a network failure; an HTTP status is not retried.
                if (attempt == 1 && error.StatusCode == null) continue;
                throw;
            }
        }
    }

    /// <summary>
    /// The one builder of the content-type methods: common fields plus
    /// <c>qrCodeTarget: { type, [type]: content }</c>, no <c>options.text</c>.
    /// <paramref name="name"/> is passed separately because the update models hide <c>Name</c>.
    /// The caller's object is not mutated.
    /// </summary>
    private async Task<QRCodeModel> SendContentTypeAsync(
        HttpMethod method,
        string type,
        string? id,
        QRCodeRequestBaseModel data,
        string? name,
        object? content,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(data);
        if (method == HttpMethod.Put && string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("QR code id is required", nameof(id));

        var qrCodeTarget = new Dictionary<string, object?>
        {
            ["type"] = type,
            [type] = content
        };

        var payload = new
        {
            Name = name,
            data.TemplateId,
            data.CustomLandingId,
            data.RefId,
            data.Tag,
            data.IsEnableLandingPage,
            data.PageInfo,
            data.Mode,
            access = AccessPayload(data),
            qrCodeTarget,
            templateType = "user",
            createdFrom = _http.ResolveCreatedFrom(CreatedFromDefaults.Package)
        };

        if (method == HttpMethod.Post)
        {
            var created = await _http.PostAsync<QRCodeModel>($"{BasePath}/{type}", payload, cancellationToken);
            return created.Result ?? throw new InvalidOperationException($"Failed to create {type} QR code");
        }

        var updated = await _http.PutAsync<QRCodeModel>($"{BasePath}/{type}/{id}", payload, cancellationToken);
        return updated.Result ?? throw new InvalidOperationException($"Failed to update {type} QR code");
    }
}
