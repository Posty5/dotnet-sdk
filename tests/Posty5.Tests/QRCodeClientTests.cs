using System.Net;
using System.Text.Json;
using Xunit;
using Posty5.QRCode;
using Posty5.QRCode.Models;
using Posty5.Core.Configuration;
using Posty5.Core.Exceptions;
using Posty5.Core.Http;
using Posty5.Core.Models;
using Posty5.Tests.Store;

namespace Posty5.Tests.Integration;

[Collection("Sequential")]
public class QRCodeClientTests : IDisposable
{
    private readonly QRCodeClient _client;
    private string? _createdId;

    public QRCodeClientTests()
    {
        var httpClient = TestConfig.CreateHttpClient();
        _client = new QRCodeClient(httpClient);
    }

    #region Free Text QR Code Tests

    [Fact]
    public async Task CreateFreeText_ShouldReturnValidQRCode()
    {
        // Arrange
        var request = new QRCodeCreateFreeTextRequestModel
        {
            Name = $"Test Free Text QR - {DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}",
            TemplateId = TestConfig.TemplateId,
            Text = "This is a test QR code with custom text content"
        };

        // Act
        var result = await _client.CreateFreeTextAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Id);
        Assert.NotNull(result.QrCodeId);
        Assert.NotNull(result.QrCodeLandingPageURL);
        Assert.Equal(request.Name, result.Name);

        _createdId = result.Id;
        TestConfig.CreatedResources.QRCodes.Add(_createdId);
    }

    [Fact]
    public async Task UpdateFreeText_ShouldUpdateSuccessfully()
    {
        // Arrange - Create first
        var createRequest = new QRCodeCreateFreeTextRequestModel
        {
            Name = "Original Free Text",
            TemplateId = TestConfig.TemplateId,
            Text = "Original text"
        };
        var created = await _client.CreateFreeTextAsync(createRequest);
        TestConfig.CreatedResources.QRCodes.Add(created.Id!);

        // Act
        var updateRequest = new QRCodeUpdateFreeTextRequestModel
        {
            Name = $"Updated Free Text - {DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}",
            TemplateId = TestConfig.TemplateId,
            Text = "Updated text content"
        };
        var result = await _client.UpdateFreeTextAsync(created.Id!, updateRequest);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(created.Id, result.Id);
        Assert.Equal(updateRequest.Name, result.Name);
    }

    #endregion

    #region Email QR Code Tests

    [Fact]
    public async Task CreateEmail_ShouldReturnValidQRCode()
    {
        // Arrange
        var request = new QRCodeCreateEmailRequestModel
        {
            Name = $"Test Email QR - {DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}",
            TemplateId = TestConfig.TemplateId,
            Email = new QRCodeEmailTargetModel
            {
                Email = "test@example.com",
                Subject = "Test Subject",
                Body = "This is a test email body"
            }
        };

        // Act
        var result = await _client.CreateEmailAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Id);
        Assert.NotNull(result.QrCodeLandingPageURL);
        Assert.Equal(request.Name, result.Name);

        TestConfig.CreatedResources.QRCodes.Add(result.Id!);
    }

    [Fact]
    public async Task UpdateEmail_ShouldUpdateSuccessfully()
    {
        // Arrange - Create first
        var createRequest = new QRCodeCreateEmailRequestModel
        {
            Name = "Original Email QR",
            TemplateId = TestConfig.TemplateId,
            Email = new QRCodeEmailTargetModel
            {
                Email = "original@example.com",
                Subject = "Original Subject",
                Body = "Original body"
            }
        };
        var created = await _client.CreateEmailAsync(createRequest);
        TestConfig.CreatedResources.QRCodes.Add(created.Id!);

        // Act
        var updateRequest = new QRCodeUpdateEmailRequestModel
        {
            Name = $"Updated Email QR - {DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}",
            TemplateId = TestConfig.TemplateId,
            Email = new QRCodeEmailTargetModel
            {
                Email = "updated@example.com",
                Subject = "Updated Subject",
                Body = "Updated body"
            }
        };
        var result = await _client.UpdateEmailAsync(created.Id!, updateRequest);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(created.Id, result.Id);
    }

    #endregion

    #region WiFi QR Code Tests

    [Fact]
    public async Task CreateWifi_ShouldReturnValidQRCode()
    {
        // Arrange
        var request = new QRCodeCreateWifiRequestModel
        {
            Name = $"Test WiFi QR - {DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}",
            TemplateId = TestConfig.TemplateId,
            Wifi = new QRCodeWifiTargetModel
            {
                Name = "TestNetwork",
                AuthenticationType = "WPA",
                Password = "testpassword123"
            }
        };

        // Act
        var result = await _client.CreateWifiAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Id);
        Assert.NotNull(result.QrCodeLandingPageURL);
        Assert.Equal(request.Name, result.Name);

        TestConfig.CreatedResources.QRCodes.Add(result.Id!);
    }

    [Fact]
    public async Task UpdateWifi_ShouldUpdateSuccessfully()
    {
        // Arrange - Create first
        var createRequest = new QRCodeCreateWifiRequestModel
        {
            Name = "Original WiFi QR",
            TemplateId = TestConfig.TemplateId,
            Wifi = new QRCodeWifiTargetModel
            {
                Name = "OriginalNetwork",
                AuthenticationType = "WPA",
                Password = "originalpass"
            }
        };
        var created = await _client.CreateWifiAsync(createRequest);
        TestConfig.CreatedResources.QRCodes.Add(created.Id!);

        // Act
        var updateRequest = new QRCodeUpdateWifiRequestModel
        {
            Name = $"Updated WiFi QR - {DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}",
            TemplateId = TestConfig.TemplateId,
            Wifi = new QRCodeWifiTargetModel
            {
                Name = "UpdatedNetwork",
                AuthenticationType = "WPA2",
                Password = "updatedpass"
            }
        };
        var result = await _client.UpdateWifiAsync(created.Id!, updateRequest);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(created.Id, result.Id);
    }

    #endregion

    #region Phone Call QR Code Tests

    [Fact]
    public async Task CreateCall_ShouldReturnValidQRCode()
    {
        // Arrange
        var request = new QRCodeCreateCallRequestModel
        {
            Name = $"Test Call QR - {DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}",
            TemplateId = TestConfig.TemplateId,
            Call = new QRCodeCallTargetModel
            {
                PhoneNumber = "+1234567890"
            }
        };

        // Act
        var result = await _client.CreateCallAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Id);
        Assert.NotNull(result.QrCodeLandingPageURL);
        Assert.Equal(request.Name, result.Name);

        TestConfig.CreatedResources.QRCodes.Add(result.Id!);
    }

    [Fact]
    public async Task UpdateCall_ShouldUpdateSuccessfully()
    {
        // Arrange - Create first
        var createRequest = new QRCodeCreateCallRequestModel
        {
            Name = "Original Call QR",
            TemplateId = TestConfig.TemplateId,
            Call = new QRCodeCallTargetModel
            {
                PhoneNumber = "+1111111111"
            }
        };
        var created = await _client.CreateCallAsync(createRequest);
        TestConfig.CreatedResources.QRCodes.Add(created.Id!);

        // Act
        var updateRequest = new QRCodeUpdateCallRequestModel
        {
            Name = $"Updated Call QR - {DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}",
            TemplateId = TestConfig.TemplateId,
            Call = new QRCodeCallTargetModel
            {
                PhoneNumber = "+9999999999"
            }
        };
        var result = await _client.UpdateCallAsync(created.Id!, updateRequest);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(created.Id, result.Id);
    }

    #endregion

    #region SMS QR Code Tests

    [Fact]
    public async Task CreateSMS_ShouldReturnValidQRCode()
    {
        // Arrange
        var request = new QRCodeCreateSMSRequestModel
        {
            Name = $"Test SMS QR - {DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}",
            TemplateId = TestConfig.TemplateId,
            Sms = new QRCodeSmsTargetModel
            {
                PhoneNumber = "+1234567890",
                Message = "Hello from QR code test!"
            }
        };

        // Act
        var result = await _client.CreateSMSAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Id);
        Assert.NotNull(result.QrCodeLandingPageURL);
        Assert.Equal(request.Name, result.Name);

        TestConfig.CreatedResources.QRCodes.Add(result.Id!);
    }

    [Fact]
    public async Task UpdateSMS_ShouldUpdateSuccessfully()
    {
        // Arrange - Create first
        var createRequest = new QRCodeCreateSMSRequestModel
        {
            Name = "Original SMS QR",
            TemplateId = TestConfig.TemplateId,
            Sms = new QRCodeSmsTargetModel
            {
                PhoneNumber = "+1111111111",
                Message = "Original message"
            }
        };
        var created = await _client.CreateSMSAsync(createRequest);
        TestConfig.CreatedResources.QRCodes.Add(created.Id!);

        // Act
        var updateRequest = new QRCodeUpdateSMSRequestModel
        {
            Name = $"Updated SMS QR - {DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}",
            TemplateId = TestConfig.TemplateId,
            Sms = new QRCodeSmsTargetModel
            {
                PhoneNumber = "+9999999999",
                Message = "Updated message"
            }
        };
        var result = await _client.UpdateSMSAsync(created.Id!, updateRequest);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(created.Id, result.Id);
    }

    #endregion

    #region URL QR Code Tests

    [Fact]
    public async Task CreateURL_ShouldReturnValidQRCode()
    {
        // Arrange
        var request = new QRCodeCreateURLRequestModel
        {
            Name = $"Test URL QR - {DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}",
            TemplateId = TestConfig.TemplateId,
            Url = new QRCodeUrlTargetModel
            {
                Url = "https://posty5.com"
            },
            Tag = "test",
            RefId = $"TEST-{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}"
        };

        // Act
        var result = await _client.CreateURLAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Id);
        Assert.NotNull(result.QrCodeLandingPageURL);
        Assert.Equal(request.Name, result.Name);
        Assert.Equal(request.Tag, result.Tag);
        Assert.Equal(request.RefId, result.RefId);

        TestConfig.CreatedResources.QRCodes.Add(result.Id!);
    }

    [Fact]
    public async Task CreateURL_WithCustomLandingId_ShouldContainSlug()
    {
        // Arrange
        var customSlug = $"test-qr-{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}";
        var request = new QRCodeCreateURLRequestModel
        {
            Name = "Custom Slug QR",
            TemplateId = TestConfig.TemplateId,
            CustomLandingId = customSlug,
            Url = new QRCodeUrlTargetModel
            {
                Url = "https://example.com"
            }
        };

        // Act
        var result = await _client.CreateURLAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Id);
        Assert.Contains(customSlug, result.QrCodeLandingPageURL);

        TestConfig.CreatedResources.QRCodes.Add(result.Id!);
    }

    [Fact]
    public async Task UpdateURL_ShouldUpdateSuccessfully()
    {
        // Arrange - Create first
        var createRequest = new QRCodeCreateURLRequestModel
        {
            Name = "Original URL QR",
            TemplateId = TestConfig.TemplateId,
            Url = new QRCodeUrlTargetModel
            {
                Url = "https://posty5.com"
            }
        };
        var created = await _client.CreateURLAsync(createRequest);
        TestConfig.CreatedResources.QRCodes.Add(created.Id!);

        // Act
        var updateRequest = new QRCodeUpdateURLRequestModel
        {
            Name = $"Updated URL QR - {DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}",
            TemplateId = TestConfig.TemplateId,
            Url = new QRCodeUrlTargetModel
            {
                Url = "https://guide.posty5.com"
            }
        };
        var result = await _client.UpdateURLAsync(created.Id!, updateRequest);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(created.Id, result.Id);
    }

    #endregion

    #region Geolocation QR Code Tests

    [Fact]
    public async Task CreateGeolocation_ShouldReturnValidQRCode()
    {
        // Arrange
        var request = new QRCodeCreateGeolocationRequestModel
        {
            Name = $"Test Geolocation QR - {DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}",
            TemplateId = TestConfig.TemplateId,
            Geolocation = new QRCodeGeolocationTargetModel
            {
                Latitude = "40.7128",
                Longitude = "-74.0060"
            }
        };

        // Act
        var result = await _client.CreateGeolocationAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Id);
        Assert.NotNull(result.QrCodeLandingPageURL);
        Assert.Equal(request.Name, result.Name);

        TestConfig.CreatedResources.QRCodes.Add(result.Id!);
    }

    [Fact]
    public async Task UpdateGeolocation_ShouldUpdateSuccessfully()
    {
        // Arrange - Create first
        var createRequest = new QRCodeCreateGeolocationRequestModel
        {
            Name = "Original Geolocation QR",
            TemplateId = TestConfig.TemplateId,
            Geolocation = new QRCodeGeolocationTargetModel
            {
                Latitude = "40.7128",
                Longitude = "-74.0060"
            }
        };
        var created = await _client.CreateGeolocationAsync(createRequest);
        TestConfig.CreatedResources.QRCodes.Add(created.Id!);

        // Act
        var updateRequest = new QRCodeUpdateGeolocationRequestModel
        {
            Name = $"Updated Geolocation QR - {DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}",
            TemplateId = TestConfig.TemplateId,
            Geolocation = new QRCodeGeolocationTargetModel
            {
                Latitude = "34.0522",
                Longitude = "-118.2437"
            }
        };
        var result = await _client.UpdateGeolocationAsync(created.Id!, updateRequest);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(created.Id, result.Id);
    }

    #endregion

    #region CRUD Tests

    [Fact]
    public async Task GetQRCodeById_WithValidId_ShouldReturnQRCode()
    {
        // Arrange - Create a QR code first
        var createRequest = new QRCodeCreateURLRequestModel
        {
            Name = "QR for Get Test",
            TemplateId = TestConfig.TemplateId,
            Url = new QRCodeUrlTargetModel { Url = "https://posty5.com" }
        };
        var created = await _client.CreateURLAsync(createRequest);
        TestConfig.CreatedResources.QRCodes.Add(created.Id!);

        // Act
        var result = await _client.GetAsync(created.Id!);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(created.Id, result.Id);
        Assert.NotNull(result.QrCodeLandingPageURL);
        Assert.NotNull(result.QrCodeId);
    }

    [Fact]
    public async Task ListQRCodes_ShouldReturnPaginatedResults()
    {
        // Act
        var result = await _client.ListAsync(
            pagination: new Core.Models.PaginationParams { PageSize = 10 }
        );

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Items);
    }

    [Fact]
    public async Task ListQRCodes_WithFilters_ShouldFilterResults()
    {
        // Arrange
        var tag = $"test-tag-{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}";
        
        // Create a QR code with specific tag
        var createRequest = new QRCodeCreateURLRequestModel
        {
            Name = "Filterable QR",
            TemplateId = TestConfig.TemplateId,
            Tag = tag,
            Url = new QRCodeUrlTargetModel { Url = "https://posty5.com" }
        };
        var created = await _client.CreateURLAsync(createRequest);
        TestConfig.CreatedResources.QRCodes.Add(created.Id!);

        // Act
        var filterParams = new QRCodeListParamsModel
        {
            Tag = tag
        };
        var result = await _client.ListAsync(
            filterParams,
            new Core.Models.PaginationParams { PageSize = 10 }
        );

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Items);
        Assert.True(result.Items.Any(qr => qr.Tag == tag));
    }

    [Fact]
    public async Task ListQRCodes_WithRefIdFilter_ShouldFilterResults()
    {
        // Arrange
        var refId = $"REF-{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}";
        
        // Create a QR code with specific refId
        var createRequest = new QRCodeCreateURLRequestModel
        {
            Name = "RefId Filterable QR",
            TemplateId = TestConfig.TemplateId,
            RefId = refId,
            Url = new QRCodeUrlTargetModel { Url = "https://posty5.com" }
        };
        var created = await _client.CreateURLAsync(createRequest);
        TestConfig.CreatedResources.QRCodes.Add(created.Id!);

        // Act
        var filterParams = new QRCodeListParamsModel
        {
            RefId = refId
        };
        var result = await _client.ListAsync(
            filterParams,
            new Core.Models.PaginationParams { PageSize = 10 }
        );

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Items);
        Assert.True(result.Items.Any(qr => qr.RefId == refId));
    }

    [Fact]
    public async Task DeleteQRCode_ShouldDeleteSuccessfully()
    {
        // Arrange - Create a QR code first
        var createRequest = new QRCodeCreateURLRequestModel
        {
            Name = "QR to Delete",
            TemplateId = TestConfig.TemplateId,
            Url = new QRCodeUrlTargetModel { Url = "https://posty5.com" }
        };
        var created = await _client.CreateURLAsync(createRequest);
        TestConfig.CreatedResources.QRCodes.Add(created.Id!);

        // Act
        await _client.DeleteAsync(created.Id!);

        // Assert - Remove from tracking since we successfully deleted it
        TestConfig.CreatedResources.QRCodes.Remove(created.Id!);
    }

    #endregion

    #region Advanced Features Tests

    [Fact]
    public async Task CreateQRCode_WithLandingPage_ShouldIncludePageInfo()
    {
        // Arrange (replaces the monetization test: the API never accepted IsEnableMonetization)
        var request = new QRCodeCreateURLRequestModel
        {
            Name = $"Landing page QR - {DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}",
            TemplateId = TestConfig.TemplateId,
            IsEnableLandingPage = true,
            PageInfo = new QRCodePageInfoModel
            {
                Title = "Spring Menu",
                Description = "New dishes every week"
            },
            Url = new QRCodeUrlTargetModel { Url = "https://posty5.com" }
        };

        // Act
        var result = await _client.CreateURLAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Id);
        Assert.True(result.IsEnableLandingPage);
        Assert.NotNull(result.PageInfo);

        TestConfig.CreatedResources.QRCodes.Add(result.Id!);
    }

    [LinkQrTruthPassFact]
    public async Task ListQRCodes_RefIdFilter_ReturnsOnlyThatRefId()
    {
        // Before the API's truth pass the filter key was misspelled server-side and every record came back.
        var refId = $"TP-QR-{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}";
        var created = await _client.CreateURLAsync(new QRCodeCreateURLRequestModel
        {
            Name = "TP refId filter",
            TemplateId = TestConfig.TemplateId,
            RefId = refId,
            Url = new QRCodeUrlTargetModel { Url = "https://posty5.com" }
        });
        TestConfig.CreatedResources.QRCodes.Add(created.Id!);

        var result = await _client.ListAsync(new QRCodeListParamsModel { RefId = refId });

        Assert.Equal(created.Id, Assert.Single(result.Items).Id);
    }

    #endregion

    #region Visit analytics (VA)

    [LinkQrVisitAnalyticsFact]
    public async Task GetAnalytics_NewQRCode_ReturnsZerosAndMeta()
    {
        var created = await CreateAnalyticsQRCodeAsync();

        var analytics = await _client.GetAnalyticsAsync(created.Id!);

        Assert.Equal(0, analytics.Totals.Visits);
        Assert.Equal(0, analytics.Totals.BotVisits);
        Assert.False(string.IsNullOrEmpty(analytics.Meta.AnalyticsStartedAt));
    }

    [LinkQrVisitAnalyticsFact]
    public async Task GetAnalytics_AllBreakdowns_IncludesChannel()
    {
        var created = await CreateAnalyticsQRCodeAsync();

        var analytics = await _client.GetAnalyticsAsync(created.Id!, new LinkAnalyticsQuery { AllBreakdowns = true });

        Assert.Contains(LinkAnalyticsBreakdown.Channel.Value, analytics.Breakdowns.Keys);
        Assert.All(analytics.Meta.Locked, locked => Assert.DoesNotContain(locked.Breakdown, analytics.Breakdowns.Keys));
    }

    [LinkQrVisitAnalyticsFact]
    public async Task GetAnalytics_ExplicitList_AndInvalidInterval()
    {
        var created = await CreateAnalyticsQRCodeAsync();

        var analytics = await _client.GetAnalyticsAsync(created.Id!, new LinkAnalyticsQuery
        {
            Breakdown = new[] { LinkAnalyticsBreakdown.Device }
        });
        Assert.Contains(LinkAnalyticsBreakdown.Device.Value, analytics.Breakdowns.Keys);

        await Assert.ThrowsAsync<Posty5ValidationException>(() => _client.GetAnalyticsAsync(created.Id!, new LinkAnalyticsQuery
        {
            Interval = new LinkAnalyticsInterval("fortnight")
        }));
    }

    private async Task<QRCodeModel> CreateAnalyticsQRCodeAsync()
    {
        var created = await _client.CreateURLAsync(new QRCodeCreateURLRequestModel
        {
            Name = $"VA analytics - {DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}",
            TemplateId = TestConfig.TemplateId,
            Url = new QRCodeUrlTargetModel { Url = "https://posty5.com" }
        });
        TestConfig.CreatedResources.QRCodes.Add(created.Id!);
        return created;
    }

    #endregion

    public void Dispose()
    {
        // Cleanup is handled by collection fixture if needed
    }
}

/// <summary>
/// What <see cref="QRCodeClient"/> puts on the wire, pinned against a local
/// <see cref="RecordingServer"/> - no network, no API key.
/// </summary>
public class QRCodeClientPayloadTests : IDisposable
{
    private readonly RecordingServer _server = new();
    private readonly QRCodeClient _client;

    public QRCodeClientPayloadTests()
    {
        _client = new QRCodeClient(new Posty5HttpClient(new Posty5Options { ApiKey = "test-key", BaseUrl = _server.BaseUrl }));
    }

    public void Dispose() => _server.Dispose();

#pragma warning disable CS0618 // IsEnableMonetization is set on purpose below: it must never reach the wire
    /// <summary>The six structured types, created and updated, each with the obsolete flag set.</summary>
    private async Task SendEveryStructuredTypeAsync()
    {
        const string t = "tpl-1";
        await _client.CreateEmailAsync(new QRCodeCreateEmailRequestModel { TemplateId = t, IsEnableMonetization = true, Email = new() { Email = "a@b.c", Subject = "Q&A", Body = "x" } });
        await _client.CreateWifiAsync(new QRCodeCreateWifiRequestModel { TemplateId = t, IsEnableMonetization = true, Wifi = new() { Name = "Net", AuthenticationType = "WPA", Password = "p;w" } });
        await _client.CreateCallAsync(new QRCodeCreateCallRequestModel { TemplateId = t, IsEnableMonetization = true, Call = new() { PhoneNumber = "+1" } });
        await _client.CreateSMSAsync(new QRCodeCreateSMSRequestModel { TemplateId = t, IsEnableMonetization = true, Sms = new() { PhoneNumber = "+1" } });
        await _client.CreateURLAsync(new QRCodeCreateURLRequestModel { TemplateId = t, IsEnableMonetization = true, Url = new() { Url = "https://example.com" } });
        await _client.CreateGeolocationAsync(new QRCodeCreateGeolocationRequestModel { TemplateId = t, IsEnableMonetization = true, Geolocation = new() { Latitude = "1", Longitude = "2" } });
        await _client.UpdateEmailAsync("q1", new QRCodeUpdateEmailRequestModel { TemplateId = t, IsEnableMonetization = true, Email = new() { Email = "a@b.c" } });
        await _client.UpdateWifiAsync("q1", new QRCodeUpdateWifiRequestModel { TemplateId = t, IsEnableMonetization = true, Wifi = new() { Name = "Net", AuthenticationType = "nopass" } });
        await _client.UpdateCallAsync("q1", new QRCodeUpdateCallRequestModel { TemplateId = t, IsEnableMonetization = true, Call = new() { PhoneNumber = "+1" } });
        await _client.UpdateSMSAsync("q1", new QRCodeUpdateSMSRequestModel { TemplateId = t, IsEnableMonetization = true, Sms = new() { PhoneNumber = "+1", Message = "hi" } });
        await _client.UpdateURLAsync("q1", new QRCodeUpdateURLRequestModel { TemplateId = t, IsEnableMonetization = true, Url = new() { Url = "https://example.com" } });
        await _client.UpdateGeolocationAsync("q1", new QRCodeUpdateGeolocationRequestModel { TemplateId = t, IsEnableMonetization = true, Geolocation = new() { Latitude = "1", Longitude = "2" } });
    }

    [Fact]
    public async Task StructuredTypes_SendQrCodeTargetOnly_NoOptionsText_NoMonetization()
    {
        await SendEveryStructuredTypeAsync();

        var types = new[] { "email", "wifi", "call", "sms", "url", "geolocation" };
        Assert.Equal(
            types.Select(type => $"POST /api/qr-code/{type}").Concat(types.Select(type => $"PUT /api/qr-code/{type}/q1")),
            _server.Requests.Select(r => $"{r.Method} {r.PathAndQuery}"));

        foreach (var (_, path, body) in _server.Requests)
        {
            using var json = JsonDocument.Parse(body);
            var root = json.RootElement;
            var type = path.Split('/')[3];
            Assert.False(root.TryGetProperty("options", out _), $"{path}: options.text is built by the API");
            Assert.False(root.TryGetProperty("isEnableMonetization", out _), $"{path}: isEnableMonetization sent");
            Assert.Equal("tpl-1", root.GetProperty("templateId").GetString());
            var target = root.GetProperty("qrCodeTarget");
            Assert.Equal(type, target.GetProperty("type").GetString());
            Assert.True(target.TryGetProperty(type, out _), $"{path}: qrCodeTarget.{type} missing");
        }
    }

    [Fact]
    public async Task FreeText_KeepsOptionsText_AndDropsMonetization()
    {
        await _client.CreateFreeTextAsync(new QRCodeCreateFreeTextRequestModel { TemplateId = "tpl-1", Text = "hello", IsEnableMonetization = true });
        await _client.UpdateFreeTextAsync("q1", new QRCodeUpdateFreeTextRequestModel { TemplateId = "tpl-1", Text = "bye", IsEnableMonetization = true });

        var texts = _server.Requests.Select(r =>
        {
            using var json = JsonDocument.Parse(r.Body);
            Assert.False(json.RootElement.TryGetProperty("isEnableMonetization", out _));
            Assert.Equal("freeText", json.RootElement.GetProperty("qrCodeTarget").GetProperty("type").GetString());
            return json.RootElement.GetProperty("options").GetProperty("text").GetString();
        }).ToList();
        Assert.Equal(new[] { "hello", "bye" }, texts);
    }

    [Fact]
    public async Task ListAsync_DropsMonetizationFilter_AndSendsRefId()
    {
        await _client.ListAsync(new QRCodeListParamsModel { RefId = "REF-1", IsEnableMonetization = true });

        var path = _server.Requests.Single().PathAndQuery;
        Assert.Contains("refId=REF-1", path);
        Assert.DoesNotContain("isEnableMonetization", path);
    }
#pragma warning restore CS0618

    [Fact]
    public async Task Create_SendsLandingPageRefIdTagAndPageInfo()
    {
        await _client.CreateURLAsync(new QRCodeCreateURLRequestModel
        {
            Name = "Menu",
            TemplateId = "tpl-1",
            RefId = "REF-1",
            Tag = "menu",
            CustomLandingId = "spring-menu",
            IsEnableLandingPage = true,
            PageInfo = new QRCodePageInfoModel { Title = "Spring Menu", Description = "New dishes" },
            Url = new QRCodeUrlTargetModel { Url = "https://example.com/menu" }
        });

        using var json = JsonDocument.Parse(_server.Requests.Single().Body);
        var root = json.RootElement;
        Assert.Equal("Menu", root.GetProperty("name").GetString());
        Assert.Equal("REF-1", root.GetProperty("refId").GetString());
        Assert.Equal("menu", root.GetProperty("tag").GetString());
        Assert.Equal("spring-menu", root.GetProperty("customLandingId").GetString());
        Assert.True(root.GetProperty("isEnableLandingPage").GetBoolean());
        Assert.Equal("Spring Menu", root.GetProperty("pageInfo").GetProperty("title").GetString());
        Assert.Equal("https://example.com/menu", root.GetProperty("qrCodeTarget").GetProperty("url").GetProperty("url").GetString());
        Assert.Equal("user", root.GetProperty("templateType").GetString());
        Assert.Equal("dotnetPackage", root.GetProperty("createdFrom").GetString());
    }

    [Fact]
    public async Task Update_WithoutIsEnableLandingPage_DoesNotSendIt()
    {
        await _client.UpdateURLAsync("q1", new QRCodeUpdateURLRequestModel { TemplateId = "tpl-1", Url = new() { Url = "https://example.com" } });

        using var json = JsonDocument.Parse(_server.Requests.Single().Body);
        Assert.False(json.RootElement.TryGetProperty("isEnableLandingPage", out _));
    }

    [Fact]
    public async Task Responses_IgnoreIsEnableMonetization_AndReadListFields()
    {
        _server.ResultJson = "{\"items\":[{\"_id\":\"q1\",\"isEnableMonetization\":true,\"isEnableLandingPage\":true,\"status\":\"approved\",\"qrCodeTarget\":{\"type\":\"sms\",\"sms\":{\"phoneNumber\":\"+1\",\"message\":\"hi\"}}}]}";

        var item = Assert.Single((await _client.ListAsync()).Items);

#pragma warning disable CS0618
        Assert.Null(item.IsEnableMonetization);
#pragma warning restore CS0618
        Assert.True(item.IsEnableLandingPage);
        Assert.Equal(QRCodeStatusType.Approved, item.Status);
        Assert.Equal("hi", item.QrCodeTarget?.Sms?.Message);
    }

    // ─── GetAnalyticsAsync (VA) ──────────────────────────────────────────────

    [Fact]
    public async Task GetAnalyticsAsync_CallsTheQRCodeAnalyticsPath_WithTheSameQueryAsShortLinks()
    {
        _server.ResultJson = ShortLinkClientPayloadTests.AnalyticsSampleJson;

        await _client.GetAnalyticsAsync("q1", new LinkAnalyticsQuery
        {
            From = new DateTime(2026, 9, 1),
            Interval = LinkAnalyticsInterval.Month,
            AllBreakdowns = true,
            Limit = 50
        });

        var (method, path, _) = _server.Requests.Single();
        Assert.Equal("GET", method);
        Assert.StartsWith("/api/qr-code/q1/analytics?", path);
        var query = ShortLinkClientPayloadTests.Query(path);
        Assert.Equal("2026-09-01", query["from"]);
        Assert.Null(query["to"]);
        Assert.Equal("month", query["interval"]);
        Assert.Equal("all", query["breakdown"]);
        Assert.Equal("50", query["limit"]);
    }

    [Fact]
    public async Task GetAnalyticsAsync_ReadsTheAnswer()
    {
        _server.ResultJson = ShortLinkClientPayloadTests.AnalyticsSampleJson;

        var analytics = await _client.GetAnalyticsAsync("q1");

        Assert.Equal(12, analytics.Totals.Visits);
        Assert.Equal(4, analytics.Breakdowns["channel"].Single(row => row.Key == "scan").Visits);
        Assert.Equal("country", Assert.Single(analytics.Meta.Locked).Breakdown);
    }

    [Fact]
    public async Task GetAnalyticsAsync_MissingQRCode_Is400WithTheApiMessage()
    {
        _server.Status = HttpStatusCode.BadRequest;
        _server.Message = "The QR Code Is Not Found";
        _server.ResultJson = "null";

        var error = await Assert.ThrowsAsync<Posty5ValidationException>(() => _client.GetAnalyticsAsync("missing"));

        Assert.Contains("The QR Code Is Not Found", error.Message);
    }

    [Fact]
    public async Task GetAnalyticsAsync_NotTheOwner_Is403YouHaveNotPermission()
    {
        _server.Status = HttpStatusCode.Forbidden;
        _server.Message = "You Have Not Permission";
        _server.ResultJson = "null";

        var error = await Assert.ThrowsAsync<Posty5Exception>(() => _client.GetAnalyticsAsync("q1"));

        Assert.Equal(403, error.StatusCode);
        Assert.Contains("You Have Not Permission", error.ResponseBody);
    }

    [Fact]
    public async Task GetAnalyticsAsync_LimitOutside1To50_ThrowsBeforeSending()
    {
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
            _client.GetAnalyticsAsync("q1", new LinkAnalyticsQuery { Limit = 51 }));

        Assert.Empty(_server.Requests);
    }

    [Fact]
    public async Task GetAnalyticsAsync_FeatureLock403_IsAPosty5ExceptionWithStatus403()
    {
        _server.Status = HttpStatusCode.Forbidden;
        _server.Message = "This feature is not available on your current plan.";
        _server.ResultJson = "null";

        var error = await Assert.ThrowsAsync<Posty5Exception>(() => _client.GetAnalyticsAsync("q1"));

        Assert.Equal(403, error.StatusCode);
        Assert.Contains("not available on your current plan", error.ResponseBody);
    }

    // ─── GetStatisticsAsync (VA) ─────────────────────────────────────────────

    private const string QRStatisticsSampleJson = """
        {
          "range": { "from": "2026-10-05T00:00:00.000Z", "to": "2026-10-05T23:59:59.999Z", "period": "today" },
          "data": {
            "totals": { "totalQRCodes": 2, "totalVisitors": 7, "avgVisitorsPerQRCode": 3.5, "visitsInRange": 3, "uniqueVisitorsInRange": 2, "botVisitsInRange": 1 },
            "daily": [ { "_id": "2026-10-05", "createdCount": 1, "visitorsSum": 3 } ],
            "topQRCodes": [ { "_id": "q1", "name": "Menu", "numberOfVisitors": 5, "createdAt": "2026-10-01T10:00:00.000Z", "visitsInRange": 3 } ]
          }
        }
        """;

    [Fact]
    public async Task GetStatisticsAsync_CallsTheQRCodeStatisticsPath_WithPeriod()
    {
        _server.ResultJson = QRStatisticsSampleJson;

        await _client.GetStatisticsAsync(new LinkStatisticsQuery { Period = LinkStatisticsPeriod.Today });

        var (method, path, _) = _server.Requests.Single();
        Assert.Equal("GET", method);
        Assert.Equal("/api/qr-code/statistics?period=today", path);
    }

    [Fact]
    public async Task GetStatisticsAsync_ReadsTheRebuiltAnswer()
    {
        _server.ResultJson = QRStatisticsSampleJson;

        var stats = await _client.GetStatisticsAsync();

        Assert.Equal(2, stats.Data.Totals.TotalQRCodes);
        Assert.Equal(3.5, stats.Data.Totals.AvgVisitorsPerQRCode);
        Assert.Equal(3, stats.Data.Totals.VisitsInRange);
        Assert.Equal(1, stats.Data.Totals.BotVisitsInRange);
        Assert.Equal("2026-10-05", Assert.Single(stats.Data.Daily).Day);
        var top = Assert.Single(stats.Data.TopQRCodes);
        Assert.Equal("q1", top.Id);
        Assert.Equal(3, top.VisitsInRange);
    }
}
