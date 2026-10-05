using Posty5.Core.Configuration;
using Posty5.Core.Exceptions;
using Posty5.Core.Helpers;
using Posty5.Core.Http;
using Posty5.Core.Models;
using Posty5.QRCode.Models;

namespace Posty5.QRCode;

/// <summary>
/// Client for managing QR codes via Posty5 API
/// </summary>
/// <remarks>
/// The typed create/update methods send <c>qrCodeTarget</c> only; the API builds
/// and escapes the text the QR image encodes (<c>options.text</c>) from it, so a
/// subject with <c>&amp;</c> or a Wi-Fi password with <c>;</c> encodes correctly.
/// Free text still sends <c>options.text</c> equal to the text.
/// </remarks>
/// <example>
/// <code>
/// var httpClient = new Posty5HttpClient(new Posty5Options
/// {
///     BaseUrl = "https://api.posty5.com",
///     ApiKey = "your-api-key"
/// });
/// 
/// var qrCodeClient = new QRCodeClient(httpClient);
/// 
/// // Create a URL QR code
/// var qrCode = await qrCodeClient.CreateURLAsync(new QRCodeCreateURLRequestModel
/// {
///     Name = "My Website",
///     TemplateId = "template_123",
///     Url = new QRCodeUrlTargetModel { Url = "https://example.com" }
/// });
/// </code>
/// </example>
public partial class QRCodeClient
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

    #region Create Methods

    /// <summary>
    /// Create a free text QR code with custom text content
    /// </summary>
    /// <param name="data">Free text QR code creation data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created QR code with ID and landing page URL</returns>
    /// <example>
    /// <code>
    /// var qrCode = await qrCodeClient.CreateFreeTextAsync(new QRCodeCreateFreeTextRequestModel
    /// {
    ///     Name = "Custom Text QR",
    ///     TemplateId = "template_123",
    ///     Text = "Any custom text you want to encode"
    /// });
    /// Console.WriteLine($"QR Code URL: {qrCode.QrCodeLandingPageURL}");
    /// </code>
    /// </example>
    public async Task<QRCodeModel> CreateFreeTextAsync(
        QRCodeCreateFreeTextRequestModel data,
        CancellationToken cancellationToken = default)
    {
        var qrCodeTarget = new
        {
            freeText = new { text = data.Text },
            type = "freeText"
        };

        // Clear text in original data
        data.Text = null!;

        var payload = new
        {
            qrCodeTarget,
            data.Name,
            data.TemplateId,
            data.CustomLandingId,
            data.RefId,
            data.Tag,
            data.IsEnableLandingPage,
            data.PageInfo,
            data.Mode,
            options = new
            {
                text = qrCodeTarget.freeText.text
            },
            templateType = "user",
            createdFrom = _http.ResolveCreatedFrom(CreatedFromDefaults.Package)
        };

        var response = await _http.PostAsync<QRCodeModel>($"{BasePath}/freeText", payload, cancellationToken);
        return response.Result ?? throw new InvalidOperationException("Failed to create free text QR code");
    }

    /// <summary>
    /// Create an email QR code that opens the default email client
    /// </summary>
    /// <param name="data">Email QR code creation data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created QR code with ID and landing page URL</returns>
    /// <example>
    /// <code>
    /// var qrCode = await qrCodeClient.CreateEmailAsync(new QRCodeCreateEmailRequestModel
    /// {
    ///     Name = "Contact Us",
    ///     TemplateId = "template_123",
    ///     Email = new QRCodeEmailTargetModel
    ///     {
    ///         Email = "contact@example.com",
    ///         Subject = "Inquiry from QR Code",
    ///         Body = "Hello, I would like to know more about..."
    ///     }
    /// });
    /// </code>
    /// </example>
    public async Task<QRCodeModel> CreateEmailAsync(
        QRCodeCreateEmailRequestModel data,
        CancellationToken cancellationToken = default)
    {
        var qrCodeTarget = new
        {
            email = data.Email,
            type = "email"
        };

        // Clear email in original data
        data.Email = null!;

        var payload = new
        {
            data.Name,
            data.TemplateId,
            data.CustomLandingId,
            data.RefId,
            data.Tag,
            data.IsEnableLandingPage,
            data.PageInfo,
            data.Mode,
            qrCodeTarget,
            templateType = "user",
            createdFrom = _http.ResolveCreatedFrom(CreatedFromDefaults.Package)
        };

        var response = await _http.PostAsync<QRCodeModel>($"{BasePath}/email", payload, cancellationToken);
        return response.Result ?? throw new InvalidOperationException("Failed to create email QR code");
    }

    /// <summary>
    /// Create a WiFi QR code for easy network connection
    /// </summary>
    /// <param name="data">WiFi QR code creation data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created QR code with ID and landing page URL</returns>
    /// <example>
    /// <code>
    /// var qrCode = await qrCodeClient.CreateWifiAsync(new QRCodeCreateWifiRequestModel
    /// {
    ///     Name = "Office WiFi",
    ///     TemplateId = "template_123",
    ///     Wifi = new QRCodeWifiTargetModel
    ///     {
    ///         Name = "OfficeNetwork",
    ///         AuthenticationType = "WPA",
    ///         Password = "secret123"
    ///     }
    /// });
    /// </code>
    /// </example>
    public async Task<QRCodeModel> CreateWifiAsync(
        QRCodeCreateWifiRequestModel data,
        CancellationToken cancellationToken = default)
    {
        if (data.Mode == QRCodeMode.Dynamic)
            throw new ArgumentException(QRCodeConst.WifiDynamicNotSupported, nameof(data));

        var qrCodeTarget = new
        {
            wifi = data.Wifi,
            type = "wifi"
        };

        // Clear wifi in original data
        data.Wifi = null!;

        var payload = new
        {
            data.Name,
            data.TemplateId,
            data.CustomLandingId,
            data.RefId,
            data.Tag,
            data.IsEnableLandingPage,
            data.PageInfo,
            data.Mode,
            qrCodeTarget,
            templateType = "user",
            createdFrom = _http.ResolveCreatedFrom(CreatedFromDefaults.Package)
        };

        var response = await _http.PostAsync<QRCodeModel>($"{BasePath}/wifi", payload, cancellationToken);
        return response.Result ?? throw new InvalidOperationException("Failed to create WiFi QR code");
    }

    /// <summary>
    /// Create a phone call QR code that initiates a call
    /// </summary>
    /// <param name="data">Call QR code creation data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created QR code with ID and landing page URL</returns>
    /// <example>
    /// <code>
    /// var qrCode = await qrCodeClient.CreateCallAsync(new QRCodeCreateCallRequestModel
    /// {
    ///     Name = "Call Support",
    ///     TemplateId = "template_123",
    ///     Call = new QRCodeCallTargetModel
    ///     {
    ///         PhoneNumber = "+1234567890"
    ///     }
    /// });
    /// </code>
    /// </example>
    public async Task<QRCodeModel> CreateCallAsync(
        QRCodeCreateCallRequestModel data,
        CancellationToken cancellationToken = default)
    {
        var qrCodeTarget = new
        {
            call = data.Call,
            type = "call"
        };

        // Clear call in original data
        data.Call = null!;

        var payload = new
        {
            data.Name,
            data.TemplateId,
            data.CustomLandingId,
            data.RefId,
            data.Tag,
            data.IsEnableLandingPage,
            data.PageInfo,
            data.Mode,
            qrCodeTarget,
            templateType = "user",
            createdFrom = _http.ResolveCreatedFrom(CreatedFromDefaults.Package)
        };

        var response = await _http.PostAsync<QRCodeModel>($"{BasePath}/call", payload, cancellationToken);
        return response.Result ?? throw new InvalidOperationException("Failed to create call QR code");
    }

    /// <summary>
    /// Create an SMS QR code with pre-filled message
    /// </summary>
    /// <param name="data">SMS QR code creation data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created QR code with ID and landing page URL</returns>
    /// <example>
    /// <code>
    /// var qrCode = await qrCodeClient.CreateSMSAsync(new QRCodeCreateSMSRequestModel
    /// {
    ///     Name = "Text Us",
    ///     TemplateId = "template_123",
    ///     Sms = new QRCodeSmsTargetModel
    ///     {
    ///         PhoneNumber = "+1234567890",
    ///         Message = "I scanned your QR code!"
    ///     }
    /// });
    /// </code>
    /// </example>
    public async Task<QRCodeModel> CreateSMSAsync(
        QRCodeCreateSMSRequestModel data,
        CancellationToken cancellationToken = default)
    {
        var qrCodeTarget = new
        {
            sms = data.Sms,
            type = "sms"
        };

        // Clear sms in original data
        data.Sms = null!;

        var payload = new
        {
            data.Name,
            data.TemplateId,
            data.CustomLandingId,
            data.RefId,
            data.Tag,
            data.IsEnableLandingPage,
            data.PageInfo,
            data.Mode,
            qrCodeTarget,
            templateType = "user",
            createdFrom = _http.ResolveCreatedFrom(CreatedFromDefaults.Package)
        };

        var response = await _http.PostAsync<QRCodeModel>($"{BasePath}/sms", payload, cancellationToken);
        return response.Result ?? throw new InvalidOperationException("Failed to create SMS QR code");
    }

    /// <summary>
    /// Create a URL QR code that opens a website
    /// </summary>
    /// <param name="data">URL QR code creation data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created QR code with ID and landing page URL</returns>
    /// <example>
    /// <code>
    /// var qrCode = await qrCodeClient.CreateURLAsync(new QRCodeCreateURLRequestModel
    /// {
    ///     Name = "Website Link",
    ///     TemplateId = "template_123",
    ///     Url = new QRCodeUrlTargetModel { Url = "https://example.com" },
    ///     Tag = "marketing",
    ///     RefId = "CAMPAIGN-001"
    /// });
    /// </code>
    /// </example>
    public async Task<QRCodeModel> CreateURLAsync(
        QRCodeCreateURLRequestModel data,
        CancellationToken cancellationToken = default)
    {
        var qrCodeTarget = new
        {
            url = data.Url,
            type = "url"
        };

        // Clear url in original data
        data.Url = null!;

        var payload = new
        {
            data.Name,
            data.TemplateId,
            data.CustomLandingId,
            data.RefId,
            data.Tag,
            data.IsEnableLandingPage,
            data.PageInfo,
            data.Mode,
            qrCodeTarget,
            templateType = "user",
            createdFrom = _http.ResolveCreatedFrom(CreatedFromDefaults.Package)
        };

        var response = await _http.PostAsync<QRCodeModel>($"{BasePath}/url", payload, cancellationToken);
        return response.Result ?? throw new InvalidOperationException("Failed to create URL QR code");
    }

    /// <summary>
    /// Create a geolocation QR code that opens map coordinates
    /// </summary>
    /// <param name="data">Geolocation QR code creation data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created QR code with ID and landing page URL</returns>
    /// <example>
    /// <code>
    /// var qrCode = await qrCodeClient.CreateGeolocationAsync(new QRCodeCreateGeolocationRequestModel
    /// {
    ///     Name = "Our Office Location",
    ///     TemplateId = "template_123",
    ///     Geolocation = new QRCodeGeolocationTargetModel
    ///     {
    ///         Latitude = "40.7128",
    ///         Longitude = "-74.0060"
    ///     }
    /// });
    /// </code>
    /// </example>
    public async Task<QRCodeModel> CreateGeolocationAsync(
        QRCodeCreateGeolocationRequestModel data,
        CancellationToken cancellationToken = default)
    {
        var qrCodeTarget = new
        {
            geolocation = data.Geolocation,
            type = "geolocation"
        };

        // Clear geolocation in original data
        data.Geolocation = null!;

        var payload = new
        {
            data.Name,
            data.TemplateId,
            data.CustomLandingId,
            data.RefId,
            data.Tag,
            data.IsEnableLandingPage,
            data.PageInfo,
            data.Mode,
            qrCodeTarget,
            templateType = "user",
            createdFrom = _http.ResolveCreatedFrom(CreatedFromDefaults.Package)
        };

        var response = await _http.PostAsync<QRCodeModel>($"{BasePath}/geolocation", payload, cancellationToken);
        return response.Result ?? throw new InvalidOperationException("Failed to create geolocation QR code");
    }

    #endregion

    #region Update Methods

    /// <summary>
    /// Update a free text QR code with custom text content
    /// </summary>
    /// <param name="id">QR code ID</param>
    /// <param name="data">Free text QR code update data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated QR code with ID and landing page URL</returns>
    /// <example>
    /// <code>
    /// var qrCode = await qrCodeClient.UpdateFreeTextAsync("qr_code_id", new QRCodeUpdateFreeTextRequestModel
    /// {
    ///     Name = "Updated Text QR",
    ///     TemplateId = "template_123",
    ///     Text = "Updated text content"
    /// });
    /// </code>
    /// </example>
    public async Task<QRCodeModel> UpdateFreeTextAsync(
        string id,
        QRCodeUpdateFreeTextRequestModel data,
        CancellationToken cancellationToken = default)
    {
        var qrCodeTarget = new
        {
            freeText = new { text = data.Text },
            type = "freeText"
        };

        // Clear text in original data
        data.Text = null!;

        var payload = new
        {
            qrCodeTarget,
            data.Name,
            data.TemplateId,
            data.CustomLandingId,
            data.RefId,
            data.Tag,
            data.IsEnableLandingPage,
            data.PageInfo,
            data.Mode,
            options = new
            {
                text = qrCodeTarget.freeText.text
            },
            templateType = "user",
            createdFrom = _http.ResolveCreatedFrom(CreatedFromDefaults.Package)
        };

        var response = await _http.PutAsync<QRCodeModel>($"{BasePath}/freeText/{id}", payload, cancellationToken);
        return response.Result ?? throw new InvalidOperationException("Failed to update free text QR code");
    }

    /// <summary>
    /// Update an email QR code that opens the default email client
    /// </summary>
    /// <param name="id">QR code ID</param>
    /// <param name="data">Email QR code update data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated QR code with ID and landing page URL</returns>
    /// <example>
    /// <code>
    /// var qrCode = await qrCodeClient.UpdateEmailAsync("qr_code_id", new QRCodeUpdateEmailRequestModel
    /// {
    ///     Name = "Contact Us",
    ///     TemplateId = "template_123",
    ///     Email = new QRCodeEmailTargetModel
    ///     {
    ///         Email = "contact@example.com",
    ///         Subject = "Inquiry from QR Code",
    ///         Body = "Hello, I would like to know more about..."
    ///     }
    /// });
    /// </code>
    /// </example>
    public async Task<QRCodeModel> UpdateEmailAsync(
        string id,
        QRCodeUpdateEmailRequestModel data,
        CancellationToken cancellationToken = default)
    {
        var qrCodeTarget = new
        {
            email = data.Email,
            type = "email"
        };

        // Clear email in original data
        data.Email = null!;

        var payload = new
        {
            data.Name,
            data.TemplateId,
            data.CustomLandingId,
            data.RefId,
            data.Tag,
            data.IsEnableLandingPage,
            data.PageInfo,
            data.Mode,
            qrCodeTarget,
            templateType = "user",
            createdFrom = _http.ResolveCreatedFrom(CreatedFromDefaults.Package)
        };

        var response = await _http.PutAsync<QRCodeModel>($"{BasePath}/email/{id}", payload, cancellationToken);
        return response.Result ?? throw new InvalidOperationException("Failed to update email QR code");
    }

    /// <summary>
    /// Update a WiFi QR code for easy network connection
    /// </summary>
    /// <param name="id">QR code ID</param>
    /// <param name="data">WiFi QR code update data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated QR code with ID and landing page URL</returns>
    /// <example>
    /// <code>
    /// var qrCode = await qrCodeClient.UpdateWifiAsync("qr_code_id", new QRCodeUpdateWifiRequestModel
    /// {
    ///     Name = "Office WiFi",
    ///     TemplateId = "template_123",
    ///     Wifi = new QRCodeWifiTargetModel
    ///     {
    ///         Name = "OfficeNetwork",
    ///         AuthenticationType = "WPA",
    ///         Password = "secret123"
    ///     }
    /// });
    /// </code>
    /// </example>
    public async Task<QRCodeModel> UpdateWifiAsync(
        string id,
        QRCodeUpdateWifiRequestModel data,
        CancellationToken cancellationToken = default)
    {
        if (data.Mode == QRCodeMode.Dynamic)
            throw new ArgumentException(QRCodeConst.WifiDynamicNotSupported, nameof(data));

        var qrCodeTarget = new
        {
            wifi = data.Wifi,
            type = "wifi"
        };

        // Clear wifi in original data
        data.Wifi = null!;

        var payload = new
        {
            data.Name,
            data.TemplateId,
            data.CustomLandingId,
            data.RefId,
            data.Tag,
            data.IsEnableLandingPage,
            data.PageInfo,
            data.Mode,
            qrCodeTarget,
            templateType = "user",
            createdFrom = _http.ResolveCreatedFrom(CreatedFromDefaults.Package)
        };

        var response = await _http.PutAsync<QRCodeModel>($"{BasePath}/wifi/{id}", payload, cancellationToken);
        return response.Result ?? throw new InvalidOperationException("Failed to update WiFi QR code");
    }

    /// <summary>
    /// Update a phone call QR code that initiates a call
    /// </summary>
    /// <param name="id">QR code ID</param>
    /// <param name="data">Call QR code update data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated QR code with ID and landing page URL</returns>
    /// <example>
    /// <code>
    /// var qrCode = await qrCodeClient.UpdateCallAsync("qr_code_id", new QRCodeUpdateCallRequestModel
    /// {
    ///     Name = "Call Support",
    ///     TemplateId = "template_123",
    ///     Call = new QRCodeCallTargetModel
    ///     {
    ///         PhoneNumber = "+1234567890"
    ///     }
    /// });
    /// </code>
    /// </example>
    public async Task<QRCodeModel> UpdateCallAsync(
        string id,
        QRCodeUpdateCallRequestModel data,
        CancellationToken cancellationToken = default)
    {
        var qrCodeTarget = new
        {
            call = data.Call,
            type = "call"
        };

        // Clear call in original data
        data.Call = null!;

        var payload = new
        {
            data.Name,
            data.TemplateId,
            data.CustomLandingId,
            data.RefId,
            data.Tag,
            data.IsEnableLandingPage,
            data.PageInfo,
            data.Mode,
            qrCodeTarget,
            templateType = "user",
            createdFrom = _http.ResolveCreatedFrom(CreatedFromDefaults.Package)
        };

        var response = await _http.PutAsync<QRCodeModel>($"{BasePath}/call/{id}", payload, cancellationToken);
        return response.Result ?? throw new InvalidOperationException("Failed to update call QR code");
    }

    /// <summary>
    /// Update an SMS QR code with pre-filled message
    /// </summary>
    /// <param name="id">QR code ID</param>
    /// <param name="data">SMS QR code update data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated QR code with ID and landing page URL</returns>
    /// <example>
    /// <code>
    /// var qrCode = await qrCodeClient.UpdateSMSAsync("qr_code_id", new QRCodeUpdateSMSRequestModel
    /// {
    ///     Name = "Text Us",
    ///     TemplateId = "template_123",
    ///     Sms = new QRCodeSmsTargetModel
    ///     {
    ///         PhoneNumber = "+1234567890",
    ///         Message = "I scanned your QR code!"
    ///     }
    /// });
    /// </code>
    /// </example>
    public async Task<QRCodeModel> UpdateSMSAsync(
        string id,
        QRCodeUpdateSMSRequestModel data,
        CancellationToken cancellationToken = default)
    {
        var qrCodeTarget = new
        {
            sms = data.Sms,
            type = "sms"
        };

        // Clear sms in original data
        data.Sms = null!;

        var payload = new
        {
            data.Name,
            data.TemplateId,
            data.CustomLandingId,
            data.RefId,
            data.Tag,
            data.IsEnableLandingPage,
            data.PageInfo,
            data.Mode,
            qrCodeTarget,
            templateType = "user",
            createdFrom = _http.ResolveCreatedFrom(CreatedFromDefaults.Package)
        };

        var response = await _http.PutAsync<QRCodeModel>($"{BasePath}/sms/{id}", payload, cancellationToken);
        return response.Result ?? throw new InvalidOperationException("Failed to update SMS QR code");
    }

    /// <summary>
    /// Update a URL QR code that opens a website
    /// </summary>
    /// <param name="id">QR code ID</param>
    /// <param name="data">URL QR code update data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated QR code with ID and landing page URL</returns>
    /// <example>
    /// <code>
    /// var qrCode = await qrCodeClient.UpdateURLAsync("qr_code_id", new QRCodeUpdateURLRequestModel
    /// {
    ///     Name = "Website Link",
    ///     TemplateId = "template_123",
    ///     Url = new QRCodeUrlTargetModel { Url = "https://example.com" },
    ///     Tag = "marketing",
    ///     RefId = "CAMPAIGN-001"
    /// });
    /// </code>
    /// </example>
    public async Task<QRCodeModel> UpdateURLAsync(
        string id,
        QRCodeUpdateURLRequestModel data,
        CancellationToken cancellationToken = default)
    {
        var qrCodeTarget = new
        {
            url = data.Url,
            type = "url"
        };

        // Clear url in original data
        data.Url = null!;

        var payload = new
        {
            data.Name,
            data.TemplateId,
            data.CustomLandingId,
            data.RefId,
            data.Tag,
            data.IsEnableLandingPage,
            data.PageInfo,
            data.Mode,
            qrCodeTarget,
            templateType = "user",
            createdFrom = _http.ResolveCreatedFrom(CreatedFromDefaults.Package)
        };

        var response = await _http.PutAsync<QRCodeModel>($"{BasePath}/url/{id}", payload, cancellationToken);
        return response.Result ?? throw new InvalidOperationException("Failed to update URL QR code");
    }

    /// <summary>
    /// Update a geolocation QR code that opens map coordinates
    /// </summary>
    /// <param name="id">QR code ID</param>
    /// <param name="data">Geolocation QR code update data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated QR code with ID and landing page URL</returns>
    /// <example>
    /// <code>
    /// var qrCode = await qrCodeClient.UpdateGeolocationAsync("qr_code_id", new QRCodeUpdateGeolocationRequestModel
    /// {
    ///     Name = "Our Office Location",
    ///     TemplateId = "template_123",
    ///     Geolocation = new QRCodeGeolocationTargetModel
    ///     {
    ///         Latitude = "40.7128",
    ///         Longitude = "-74.0060"
    ///     }
    /// });
    /// </code>
    /// </example>
    public async Task<QRCodeModel> UpdateGeolocationAsync(
        string id,
        QRCodeUpdateGeolocationRequestModel data,
        CancellationToken cancellationToken = default)
    {
        var qrCodeTarget = new
        {
            geolocation = data.Geolocation,
            type = "geolocation"
        };

        // Clear geolocation in original data
        data.Geolocation = null!;

        var payload = new
        {
            data.Name,
            data.TemplateId,
            data.CustomLandingId,
            data.RefId,
            data.Tag,
            data.IsEnableLandingPage,
            data.PageInfo,
            data.Mode,
            qrCodeTarget,
            templateType = "user",
            createdFrom = _http.ResolveCreatedFrom(CreatedFromDefaults.Package)
        };

        var response = await _http.PutAsync<QRCodeModel>($"{BasePath}/geolocation/{id}", payload, cancellationToken);
        return response.Result ?? throw new InvalidOperationException("Failed to update geolocation QR code");
    }

    #endregion

    #region CRUD Methods

    /// <summary>
    /// Get a QR code by ID with full details
    /// </summary>
    /// <param name="id">QR code ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>QR code full details including populated template, user, and API key</returns>
    /// <example>
    /// <code>
    /// var qrCode = await qrCodeClient.GetAsync("qr123");
    /// Console.WriteLine(qrCode.Name);
    /// Console.WriteLine(qrCode.NumberOfVisitors);
    /// Console.WriteLine(qrCode.Template?.Name); // Access populated template
    /// Console.WriteLine(qrCode.User?.FullName); // Access user who created it
    /// Console.WriteLine(qrCode.Options?.ColorDark); // Access styling options
    /// </code>
    /// </example>
    public async Task<QRCodeFullDetailsModel> GetAsync(string id, CancellationToken cancellationToken = default)
    {
        var response = await _http.GetAsync<QRCodeFullDetailsModel>($"{BasePath}/{id}", cancellationToken: cancellationToken);
        return response.Result ?? throw new InvalidOperationException("QR code not found");
    }

    /// <summary>
    /// Visit analytics of one QR code: totals, a series and breakdowns
    /// (<c>GET /api/qr-code/{id}/analytics</c>). Reading analytics costs no credits.
    /// </summary>
    /// <remarks>
    /// <para>Bots and link-preview crawlers are excluded from every <c>Visits</c>
    /// and counted only in <see cref="LinkAnalyticsTotals.BotVisits"/>.</para>
    /// <para><c>UniqueVisitors</c> over more than one day is the sum of daily
    /// uniques (the visitor hash rotates daily).</para>
    /// <para>Data starts on <see cref="LinkAnalyticsMeta.AnalyticsStartedAt"/>,
    /// when Posty5 started recording visits; nothing earlier exists.</para>
    /// <para>A static QR code (text, Wi-Fi, …) counts visits to its Posty5 page
    /// only; a scan that never reaches Posty5 cannot be counted. The
    /// <c>channel</c> breakdown tells scans from clicks.</para>
    /// <para>Plan limits come from the API: unless you name breakdowns, you get
    /// every breakdown your plan allows and the rest are listed in
    /// <see cref="LinkAnalyticsMeta.Locked"/>; naming one
    /// in <see cref="LinkAnalyticsQuery.Breakdown"/>, or a
    /// <see cref="LinkAnalyticsQuery.From"/> older than your plan's history, throws
    /// <see cref="Posty5Exception"/> with <see cref="Posty5Exception.StatusCode"/>
    /// 403 and the API's message (<c>This feature is not available on your current plan.</c>,
    /// or <c>You Have Not Permission</c>) in <see cref="Posty5Exception.ResponseBody"/>.</para>
    /// </remarks>
    /// <param name="id">QR code ID</param>
    /// <param name="query">Range, interval, time zone and breakdowns; <c>null</c> for the API defaults (last 30 days, by day, every breakdown your plan allows).</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The analytics answer</returns>
    /// <exception cref="ArgumentOutOfRangeException"><see cref="LinkAnalyticsQuery.Limit"/> is outside 1-50.</exception>
    /// <exception cref="ArgumentException"><paramref name="id"/> is empty, or both <see cref="LinkAnalyticsQuery.AllBreakdowns"/> and <see cref="LinkAnalyticsQuery.Breakdown"/> are set.</exception>
    /// <exception cref="Posty5ValidationException">
    /// 400: the API refused the query (e.g. an unknown interval or time zone), or no
    /// QR code with that ID is visible to this API key (<c>The QR Code Is Not Found</c>; the API answers
    /// 400, not 404, for a missing record).
    /// </exception>
    /// <example>
    /// <code>
    /// var analytics = await qrCodeClient.GetAnalyticsAsync("qr123", new LinkAnalyticsQuery
    /// {
    ///     From = new DateTime(2026, 10, 1),
    ///     To = new DateTime(2026, 10, 31),
    ///     Interval = LinkAnalyticsInterval.Week,
    ///     AllBreakdowns = true
    /// });
    /// Console.WriteLine(analytics.Totals.Visits);
    /// foreach (var row in analytics.Breakdowns.GetValueOrDefault("device") ?? new())
    ///     Console.WriteLine($"{row.Key}: {row.Visits}");
    /// </code>
    /// </example>
    public async Task<LinkAnalyticsModel> GetAnalyticsAsync(
        string id,
        LinkAnalyticsQuery? query = null,
        CancellationToken cancellationToken = default)
    {
        var response = await _http.GetAsync<LinkAnalyticsModel>(
            LinkAnalyticsQueryHelper.BuildPath(BasePath, id),
            LinkAnalyticsQueryHelper.ToQueryParams(query),
            cancellationToken);

        return response.Result ?? throw new InvalidOperationException("QR code analytics were not returned");
    }

    /// <summary>
    /// Account-wide statistics over all your QR codes (<c>GET /api/qr-code/statistics</c>):
    /// lifetime totals, visit totals in the range, a <c>daily</c> list in UTC days
    /// and the top ten QR codes by visits in the range.
    /// </summary>
    /// <remarks>
    /// <para><c>VisitsInRange</c>, <c>UniqueVisitorsInRange</c>, <c>BotVisitsInRange</c>,
    /// <c>Daily[].VisitorsSum</c> and <c>TopQRCodes[].VisitsInRange</c> come from visit
    /// analytics: bots excluded, uniques summed per UTC day, nothing before
    /// analytics launched. <c>TotalVisitors</c> is the lifetime counter and still
    /// includes older visits.</para>
    /// <para>Visits are visits to the codes' Posty5 pages; a scan of a static code
    /// opens its content directly and is not seen by Posty5.</para>
    /// <para>Days are UTC days whatever your account time zone; use
    /// <see cref="GetAnalyticsAsync"/> for one QR code in your time zone.</para>
    /// </remarks>
    /// <param name="query">Preset period or custom <c>From</c>/<c>To</c>; <c>null</c> for the last 30 days.</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The resolved range and the statistics</returns>
    /// <exception cref="ArgumentException"><c>From</c>/<c>To</c> set with a non-custom <c>Period</c>, or <c>From</c> after <c>To</c>.</exception>
    /// <example>
    /// <code>
    /// var stats = await qrCodeClient.GetStatisticsAsync(new LinkStatisticsQuery { Period = LinkStatisticsPeriod.Last7Days });
    /// Console.WriteLine($"{stats.Data.Totals.VisitsInRange} visits since {stats.Range.From:d}");
    /// foreach (var day in stats.Data.Daily)
    ///     Console.WriteLine($"{day.Day}: {day.VisitorsSum}");
    /// </code>
    /// </example>
    public async Task<QRCodeStatisticsModel> GetStatisticsAsync(
        LinkStatisticsQuery? query = null,
        CancellationToken cancellationToken = default)
    {
        var response = await _http.GetAsync<QRCodeStatisticsModel>(
            LinkAnalyticsQueryHelper.BuildStatisticsPath(BasePath),
            LinkAnalyticsQueryHelper.ToStatisticsQueryParams(query),
            cancellationToken);

        return response.Result ?? throw new InvalidOperationException("QR code statistics were not returned");
    }

    /// <summary>
    /// Delete a QR code
    /// </summary>
    /// <param name="id">QR code ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Deletion confirmation response</returns>
    /// <example>
    /// <code>
    /// var result = await qrCodeClient.DeleteAsync("qr123");
    /// Console.WriteLine(result.Message); // "Deleted" or success message
    /// </code>
    /// </example>
    public async Task<DeleteResponse> DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        var response = await _http.DeleteAsync<DeleteResponse>($"{BasePath}/{id}", cancellationToken);
        return response.Result ?? new DeleteResponse { Message = "Deleted" };
    }

    /// <summary>
    /// List QR codes with pagination and optional filters
    /// </summary>
    /// <param name="listParams">Filter parameters (optional)</param>
    /// <param name="pagination">Pagination parameters (optional)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paginated list of QR codes</returns>
    /// <example>
    /// <code>
    /// // List all QR codes
    /// var result = await qrCodeClient.ListAsync();
    /// Console.WriteLine(result.Items);
    /// 
    /// // List with filters and cursor pagination
    /// var filtered = await qrCodeClient.ListAsync(
    ///     new QRCodeListParamsModel { Status = QRCodeStatusType.Approved, Tag = "marketing" },
    ///     new PaginationParams { Cursor = null, PageSize = 20 }
    /// );
    /// // Get next page using cursor from previous response
    /// var nextPage = await qrCodeClient.ListAsync(
    ///     new QRCodeListParamsModel { Status = QRCodeStatusType.Approved },
    ///     new PaginationParams { Cursor = filtered.Pagination.NextCursor, PageSize = 20 }
    /// );
    /// </code>
    /// </example>
    public async Task<PaginationResponse<QRCodeModel>> ListAsync(
        QRCodeListParamsModel? listParams = null,
        PaginationParams? pagination = null,
        CancellationToken cancellationToken = default)
    {
        var queryParams = new Dictionary<string, object?>();

        if (listParams != null)
        {
            if (!string.IsNullOrEmpty(listParams.Name))
                queryParams["name"] = listParams.Name;
            if (!string.IsNullOrEmpty(listParams.QrCodeId))
                queryParams["qrCodeId"] = listParams.QrCodeId;
            if (!string.IsNullOrEmpty(listParams.TemplateId))
                queryParams["templateId"] = listParams.TemplateId;
            if (!string.IsNullOrEmpty(listParams.Tag))
                queryParams["tag"] = listParams.Tag;
            if (!string.IsNullOrEmpty(listParams.RefId))
                queryParams["refId"] = listParams.RefId;
            if (listParams.Status.HasValue)
                queryParams["status"] = listParams.Status.Value.ToString();
            if (listParams.Mode.HasValue)
                queryParams["mode"] = listParams.Mode.Value.ToString();
            if (!string.IsNullOrEmpty(listParams.CreatedFrom))
                queryParams["createdFrom"] = listParams.CreatedFrom;
        }

        if (pagination != null)
        {
            if (!string.IsNullOrEmpty(pagination.Cursor))
                queryParams["cursor"] = pagination.Cursor;
            queryParams["pageSize"] = pagination.PageSize;
        }

        var response = await _http.GetAsync<PaginationResponse<QRCodeModel>>(
            BasePath,
            queryParams,
            cancellationToken);

        return response.Result ?? new PaginationResponse<QRCodeModel>();
    }

    #endregion
}
