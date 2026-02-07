using Posty5.Core.Http;
using Posty5.Core.Models;
using Posty5.QRCode.Models;

namespace Posty5.QRCode;

/// <summary>
/// Client for managing QR codes via Posty5 API
/// </summary>
/// <example>
/// <code>
/// var httpClient = new Posty5HttpClient(new Posty5HttpClientOptions
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
///     Url = new QRCodeUrlTarget { Url = "https://example.com" }
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
            data.IsEnableMonetization,
            data.PageInfo,
            options = new
            {
                text = qrCodeTarget.freeText.text
            },
            templateType = "user",
            createdFrom = "dotnetPackage"
        };

        var response = await _http.PostAsync<QRCodeModel>($"{BasePath}/freetext", payload, cancellationToken);
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
    ///     Email = new QRCodeEmailTarget
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
            data.IsEnableMonetization,
            data.PageInfo,
            qrCodeTarget,
            options = new
            {
                text = $"mailto:{qrCodeTarget.email.Email}?subject={qrCodeTarget.email.Subject}&body={qrCodeTarget.email.Body}"
            },
            templateType = "user",
            createdFrom = "dotnetPackage"
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
    ///     Wifi = new QRCodeWifiTarget
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
            data.IsEnableMonetization,
            data.PageInfo,
            qrCodeTarget,
            options = new
            {
                text = $"WIFI:T:{qrCodeTarget.wifi.AuthenticationType};S:{qrCodeTarget.wifi.Name};P:{qrCodeTarget.wifi.Password};"
            },
            templateType = "user",
            createdFrom = "dotnetPackage"
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
    ///     Call = new QRCodeCallTarget
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
            data.IsEnableMonetization,
            data.PageInfo,
            qrCodeTarget,
            options = new
            {
                text = $"tel:{qrCodeTarget.call.PhoneNumber}"
            },
            templateType = "user",
            createdFrom = "dotnetPackage"
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
    ///     Sms = new QRCodeSmsTarget
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
            data.IsEnableMonetization,
            data.PageInfo,
            qrCodeTarget,
            options = new
            {
                text = $"sms:{qrCodeTarget.sms.PhoneNumber}?body={qrCodeTarget.sms.Message}"
            },
            templateType = "user",
            createdFrom = "dotnetPackage"
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
    ///     Url = new QRCodeUrlTarget { Url = "https://example.com" },
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
            data.IsEnableMonetization,
            data.PageInfo,
            qrCodeTarget,
            options = new
            {
                text = qrCodeTarget.url.Url
            },
            templateType = "user",
            createdFrom = "dotnetPackage"
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
    ///     Geolocation = new QRCodeGeolocationTarget
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
            data.IsEnableMonetization,
            data.PageInfo,
            qrCodeTarget,
            options = new
            {
                text = $"geo:{qrCodeTarget.geolocation.Latitude},{qrCodeTarget.geolocation.Longitude}"
            },
            templateType = "user",
            createdFrom = "dotnetPackage"
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
            data.IsEnableMonetization,
            data.PageInfo,
            options = new
            {
                text = qrCodeTarget.freeText.text
            },
            templateType = "user",
            createdFrom = "dotnetPackage"
        };

        var response = await _http.PutAsync<QRCodeModel>($"{BasePath}/{id}", payload, cancellationToken);
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
    ///     Email = new QRCodeEmailTarget
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
            data.IsEnableMonetization,
            data.PageInfo,
            qrCodeTarget,
            options = new
            {
                text = $"mailto:{qrCodeTarget.email.Email}?subject={qrCodeTarget.email.Subject}&body={qrCodeTarget.email.Body}"
            },
            templateType = "user",
            createdFrom = "dotnetPackage"
        };

        var response = await _http.PutAsync<QRCodeModel>($"{BasePath}/{id}", payload, cancellationToken);
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
    ///     Wifi = new QRCodeWifiTarget
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
            data.IsEnableMonetization,
            data.PageInfo,
            qrCodeTarget,
            options = new
            {
                text = $"WIFI:T:{qrCodeTarget.wifi.AuthenticationType};S:{qrCodeTarget.wifi.Name};P:{qrCodeTarget.wifi.Password};"
            },
            templateType = "user",
            createdFrom = "dotnetPackage"
        };

        var response = await _http.PutAsync<QRCodeModel>($"{BasePath}/{id}", payload, cancellationToken);
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
    ///     Call = new QRCodeCallTarget
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
            data.IsEnableMonetization,
            data.PageInfo,
            qrCodeTarget,
            options = new
            {
                text = $"tel:{qrCodeTarget.call.PhoneNumber}"
            },
            templateType = "user",
            createdFrom = "dotnetPackage"
        };

        var response = await _http.PutAsync<QRCodeModel>($"{BasePath}/{id}", payload, cancellationToken);
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
    ///     Sms = new QRCodeSmsTarget
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
            data.IsEnableMonetization,
            data.PageInfo,
            qrCodeTarget,
            options = new
            {
                text = $"sms:{qrCodeTarget.sms.PhoneNumber}?body={qrCodeTarget.sms.Message}"
            },
            templateType = "user",
            createdFrom = "dotnetPackage"
        };

        var response = await _http.PutAsync<QRCodeModel>($"{BasePath}/{id}", payload, cancellationToken);
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
    ///     Url = new QRCodeUrlTarget { Url = "https://example.com" },
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
            data.IsEnableMonetization,
            data.PageInfo,
            qrCodeTarget,
            options = new
            {
                text = qrCodeTarget.url.Url
            },
            templateType = "user",
            createdFrom = "dotnetPackage"
        };

        var response = await _http.PutAsync<QRCodeModel>($"{BasePath}/{id}", payload, cancellationToken);
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
    ///     Geolocation = new QRCodeGeolocationTarget
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
            data.IsEnableMonetization,
            data.PageInfo,
            qrCodeTarget,
            options = new
            {
                text = $"geo:{qrCodeTarget.geolocation.Latitude},{qrCodeTarget.geolocation.Longitude}"
            },
            templateType = "user",
            createdFrom = "dotnetPackage"
        };

        var response = await _http.PutAsync<QRCodeModel>($"{BasePath}/{id}", payload, cancellationToken);
        return response.Result ?? throw new InvalidOperationException("Failed to update geolocation QR code");
    }

    #endregion

    #region CRUD Methods

    /// <summary>
    /// Get a QR code by ID
    /// </summary>
    /// <param name="id">QR code ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>QR code details</returns>
    /// <example>
    /// <code>
    /// var qrCode = await qrCodeClient.GetAsync("qr123");
    /// Console.WriteLine(qrCode.Name);
    /// Console.WriteLine(qrCode.NumberOfVisitors);
    /// </code>
    /// </example>
    public async Task<QRCodeModel> GetAsync(string id, CancellationToken cancellationToken = default)
    {
        var response = await _http.GetAsync<QRCodeModel>($"{BasePath}/{id}", cancellationToken: cancellationToken);
        return response.Result ?? throw new InvalidOperationException("QR code not found");
    }

    /// <summary>
    /// Delete a QR code
    /// </summary>
    /// <param name="id">QR code ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <example>
    /// <code>
    /// await qrCodeClient.DeleteAsync("qr123");
    /// </code>
    /// </example>
    public async Task DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        await _http.DeleteAsync<object>($"{BasePath}/{id}", cancellationToken);
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
    ///     new QRCodeListParamsModel { Status = "approved", Tag = "marketing" },
    ///     new PaginationParams { Cursor = null, PageSize = 20 }
    /// );
    /// // Get next page using cursor from previous response
    /// var nextPage = await qrCodeClient.ListAsync(
    ///     new QRCodeListParamsModel { Status = "approved" },
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
            if (listParams.IsEnableMonetization.HasValue)
                queryParams["isEnableMonetization"] = listParams.IsEnableMonetization.Value;
            if (listParams.Status.HasValue)
                queryParams["status"] = listParams.Status.Value.ToString();
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
