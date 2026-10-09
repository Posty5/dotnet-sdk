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
        var result = await _client.UpdateFreeTextAsync(created.Id!, updateRequest, created.Version);

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
        var result = await _client.UpdateEmailAsync(created.Id!, updateRequest, created.Version);

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
        var result = await _client.UpdateWifiAsync(created.Id!, updateRequest, created.Version);

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
        var result = await _client.UpdateCallAsync(created.Id!, updateRequest, created.Version);

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
        var result = await _client.UpdateSMSAsync(created.Id!, updateRequest, created.Version);

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
        var result = await _client.UpdateURLAsync(created.Id!, updateRequest, created.Version);

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
        var result = await _client.UpdateGeolocationAsync(created.Id!, updateRequest, created.Version);

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
        await _client.DeleteAsync(created.Id!, created.Version);

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

    #region Dynamic QR (DQ)

    [Fact]
    public async Task CreateURL_Dynamic_TargetUpdateKeepsLandingPageUrl()
    {
        var created = await _client.CreateURLAsync(new QRCodeCreateURLRequestModel
        {
            Name = $"Dynamic QR - {DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}",
            TemplateId = TestConfig.TemplateId,
            Mode = QRCodeMode.Dynamic,
            Url = new QRCodeUrlTargetModel { Url = "https://example.com/a" }
        });
        TestConfig.CreatedResources.QRCodes.Add(created.Id!);
        Assert.Equal(QRCodeMode.Dynamic, created.Mode);
        Assert.NotNull(created.DynamicSince);

        var updated = await _client.UpdateURLAsync(created.Id!, new QRCodeUpdateURLRequestModel
        {
            Name = created.Name,
            TemplateId = TestConfig.TemplateId,
            Url = new QRCodeUrlTargetModel { Url = "https://example.com/b" }
        }, created.Version);

        Assert.Equal(created.QrCodeLandingPageURL, updated.QrCodeLandingPageURL);
        Assert.Equal(QRCodeMode.Dynamic, updated.Mode);
    }

    [Fact]
    public async Task CreateURL_WithoutMode_IsStatic()
    {
        var created = await _client.CreateURLAsync(new QRCodeCreateURLRequestModel
        {
            Name = $"Static QR - {DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}",
            TemplateId = TestConfig.TemplateId,
            Url = new QRCodeUrlTargetModel { Url = "https://example.com" }
        });
        TestConfig.CreatedResources.QRCodes.Add(created.Id!);

        Assert.Equal(QRCodeMode.Static, created.Mode);
        Assert.Null(created.DynamicSince);
    }

    [Fact]
    public async Task List_ModeFilter_ReturnsOnlyThatMode()
    {
        var result = await _client.ListAsync(new QRCodeListParamsModel { Mode = QRCodeMode.Dynamic });

        Assert.All(result.Items, item => Assert.Equal(QRCodeMode.Dynamic, item.Mode));
    }

    /// <summary>Needs a Starter+ test account; on Free the create answers 403.</summary>
    [Fact]
    public async Task Dynamic_ScanRules_SetReadAndClear()
    {
        var created = await _client.CreateURLAsync(new QRCodeCreateURLRequestModel
        {
            Name = $"Scan rules QR - {DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}",
            TemplateId = TestConfig.TemplateId,
            Mode = QRCodeMode.Dynamic,
            Url = new QRCodeUrlTargetModel { Url = "https://example.com" },
            Access = new QRCodeAccessModel { MaxVisits = 10, FallbackUrl = "https://example.com/over" }
        });
        TestConfig.CreatedResources.QRCodes.Add(created.Id!);
        Assert.Equal(10, created.Access?.MaxVisits);
        Assert.Equal("https://example.com/over", created.Access?.FallbackUrl);

        var cleared = await _client.UpdateURLAsync(created.Id!, new QRCodeUpdateURLRequestModel
        {
            Name = created.Name,
            TemplateId = TestConfig.TemplateId,
            Url = new QRCodeUrlTargetModel { Url = "https://example.com" },
            ClearAccess = true
        }, created.Version);
        Assert.Null(cleared.Access);
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
        await _client.UpdateEmailAsync("q1", new QRCodeUpdateEmailRequestModel { TemplateId = t, IsEnableMonetization = true, Email = new() { Email = "a@b.c" } }, 0);
        await _client.UpdateWifiAsync("q1", new QRCodeUpdateWifiRequestModel { TemplateId = t, IsEnableMonetization = true, Wifi = new() { Name = "Net", AuthenticationType = "nopass" } }, 0);
        await _client.UpdateCallAsync("q1", new QRCodeUpdateCallRequestModel { TemplateId = t, IsEnableMonetization = true, Call = new() { PhoneNumber = "+1" } }, 0);
        await _client.UpdateSMSAsync("q1", new QRCodeUpdateSMSRequestModel { TemplateId = t, IsEnableMonetization = true, Sms = new() { PhoneNumber = "+1", Message = "hi" } }, 0);
        await _client.UpdateURLAsync("q1", new QRCodeUpdateURLRequestModel { TemplateId = t, IsEnableMonetization = true, Url = new() { Url = "https://example.com" } }, 0);
        await _client.UpdateGeolocationAsync("q1", new QRCodeUpdateGeolocationRequestModel { TemplateId = t, IsEnableMonetization = true, Geolocation = new() { Latitude = "1", Longitude = "2" } }, 0);
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
        await _client.UpdateFreeTextAsync("q1", new QRCodeUpdateFreeTextRequestModel { TemplateId = "tpl-1", Text = "bye", IsEnableMonetization = true }, 0);

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
        await _client.UpdateURLAsync("q1", new QRCodeUpdateURLRequestModel { TemplateId = "tpl-1", Url = new() { Url = "https://example.com" } }, 0);

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

    // ─── Dynamic QR (DQ) ─────────────────────────────────────────────────────

    [Fact]
    public async Task WithoutMode_NoModeOnTheWire()
    {
        await _client.CreateURLAsync(new QRCodeCreateURLRequestModel { TemplateId = "tpl-1", Url = new() { Url = "https://example.com" } });
        await _client.UpdateWifiAsync("q1", new QRCodeUpdateWifiRequestModel { TemplateId = "tpl-1", Wifi = new() { Name = "Net" } }, 0);

        foreach (var r in _server.Requests)
        {
            using var json = JsonDocument.Parse(r.Body);
            Assert.False(json.RootElement.TryGetProperty("mode", out _), $"{r.PathAndQuery}: mode sent without a value");
        }
    }

    [Fact]
    public async Task OldStyleCall_SerialisesTheSameTopLevelJsonAsBeforeDq()
    {
        // A pre-DQ call (no Mode, no Access, no ClearAccess) must serialise exactly the pre-DQ keys, in order.
        await _client.CreateURLAsync(new QRCodeCreateURLRequestModel { Name = "N", TemplateId = "tpl-1", Url = new() { Url = "https://example.com" } });
        await _client.CreateFreeTextAsync(new QRCodeCreateFreeTextRequestModel { Name = "N", TemplateId = "tpl-1", Text = "T" });

        var keys = _server.Requests.Select(r =>
        {
            using var json = JsonDocument.Parse(r.Body);
            return string.Join(",", json.RootElement.EnumerateObject().Select(p => p.Name));
        }).ToList();
        Assert.Equal(new[]
        {
            "name,templateId,qrCodeTarget,templateType,createdFrom",
            "qrCodeTarget,name,templateId,options,templateType,createdFrom",
        }, keys);
    }

    [Fact]
    public async Task Mode_IsSentOnCreateAndUpdate()
    {
        await _client.CreateURLAsync(new QRCodeCreateURLRequestModel { TemplateId = "tpl-1", Mode = QRCodeMode.Dynamic, Url = new() { Url = "https://example.com" } });
        await _client.UpdateFreeTextAsync("q1", new QRCodeUpdateFreeTextRequestModel { TemplateId = "tpl-1", Mode = QRCodeMode.Static, Text = "x" }, 0);
        await _client.CreateWifiAsync(new QRCodeCreateWifiRequestModel { TemplateId = "tpl-1", Mode = QRCodeMode.Static, Wifi = new() { Name = "Net" } });

        var modes = _server.Requests.Select(r =>
        {
            using var json = JsonDocument.Parse(r.Body);
            return json.RootElement.GetProperty("mode").GetString();
        }).ToList();
        Assert.Equal(new[] { "dynamic", "static", "static" }, modes);
    }

    [Fact]
    public async Task DynamicWifi_ThrowsBeforeAnyCall()
    {
        await Assert.ThrowsAsync<ArgumentException>(() => _client.CreateWifiAsync(new QRCodeCreateWifiRequestModel { TemplateId = "tpl-1", Mode = QRCodeMode.Dynamic, Wifi = new() { Name = "Net" } }));
        await Assert.ThrowsAsync<ArgumentException>(() => _client.UpdateWifiAsync("q1", new QRCodeUpdateWifiRequestModel { TemplateId = "tpl-1", Mode = QRCodeMode.Dynamic, Wifi = new() { Name = "Net" } }, 0));
        Assert.Empty(_server.Requests);
    }

    [Fact]
    public async Task ListAsync_SendsModeFilter()
    {
        await _client.ListAsync(new QRCodeListParamsModel { Mode = QRCodeMode.Dynamic });

        Assert.Contains("mode=dynamic", _server.Requests.Single().PathAndQuery);
    }

    [Fact]
    public async Task Responses_ReadModeAndDynamicSince_AndTolerateUnknownModes()
    {
        _server.ResultJson = "{\"items\":[{\"_id\":\"q1\",\"mode\":\"dynamic\",\"dynamicSince\":\"2026-10-01T10:00:00.000Z\"},{\"_id\":\"q2\",\"mode\":\"static\",\"dynamicSince\":null},{\"_id\":\"q3\",\"mode\":\"future\"}]}";

        var items = (await _client.ListAsync()).Items;

        Assert.Equal(QRCodeMode.Dynamic, items[0].Mode);
        Assert.Equal(new DateTime(2026, 10, 1, 10, 0, 0, DateTimeKind.Utc), items[0].DynamicSince?.ToUniversalTime());
        Assert.Equal(QRCodeMode.Static, items[1].Mode);
        Assert.Null(items[1].DynamicSince);
        Assert.Equal("future", items[2].Mode?.Value);
    }

    // ─── Scan rules (DQ Part B) ──────────────────────────────────────────────

    [Fact]
    public async Task WithoutAccess_NoAccessOnTheWire()
    {
        await _client.CreateURLAsync(new QRCodeCreateURLRequestModel { TemplateId = "tpl-1", Url = new() { Url = "https://example.com" } });
        await _client.UpdateURLAsync("q1", new QRCodeUpdateURLRequestModel { TemplateId = "tpl-1", Url = new() { Url = "https://example.com" } }, 0);

        foreach (var r in _server.Requests)
        {
            using var json = JsonDocument.Parse(r.Body);
            Assert.False(json.RootElement.TryGetProperty("access", out _), $"{r.PathAndQuery}: access sent without a value");
        }
    }

    [Fact]
    public async Task Access_IsSentAsObject()
    {
        await _client.CreateURLAsync(new QRCodeCreateURLRequestModel
        {
            TemplateId = "tpl-1",
            Mode = QRCodeMode.Dynamic,
            Url = new() { Url = "https://example.com" },
            Access = new QRCodeAccessModel
            {
                ExpiresAt = new DateTime(2026, 12, 31, 0, 0, 0, DateTimeKind.Utc),
                MaxVisits = 100,
                FallbackUrl = "https://example.com/over"
            }
        });

        using var json = JsonDocument.Parse(_server.Requests.Single().Body);
        var access = json.RootElement.GetProperty("access");
        Assert.Equal(JsonValueKind.Object, access.ValueKind);
        Assert.Equal(100, access.GetProperty("maxVisits").GetInt32());
        Assert.Equal("https://example.com/over", access.GetProperty("fallbackUrl").GetString());
        Assert.StartsWith("2026-12-31T00:00:00", access.GetProperty("expiresAt").GetString());
        Assert.False(access.TryGetProperty("activeFrom", out _));
    }

    [Fact]
    public async Task ClearAccess_SendsJsonNull()
    {
        await _client.UpdateURLAsync("q1", new QRCodeUpdateURLRequestModel
        {
            TemplateId = "tpl-1",
            Url = new() { Url = "https://example.com" },
            Access = new QRCodeAccessModel { MaxVisits = 5 },
            ClearAccess = true
        }, 0);

        using var json = JsonDocument.Parse(_server.Requests.Single().Body);
        Assert.Equal(JsonValueKind.Null, json.RootElement.GetProperty("access").ValueKind);
        Assert.False(json.RootElement.TryGetProperty("clearAccess", out _));
    }

    [Fact]
    public async Task Responses_ReadAccess()
    {
        _server.ResultJson = "{\"items\":[{\"_id\":\"q1\",\"access\":{\"activeFrom\":null,\"expiresAt\":\"2026-12-31T00:00:00.000Z\",\"maxVisits\":3,\"fallbackUrl\":null}},{\"_id\":\"q2\",\"access\":null}]}";

        var items = (await _client.ListAsync()).Items;

        Assert.Equal(3, items[0].Access?.MaxVisits);
        Assert.Null(items[0].Access?.ActiveFrom);
        Assert.Equal(new DateTime(2026, 12, 31, 0, 0, 0, DateTimeKind.Utc), items[0].Access?.ExpiresAt?.ToUniversalTime());
        Assert.Null(items[1].Access);
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

/// <summary>
/// Content types (QC): vCard, event, WhatsApp, review, social (pass 1) and app store, file
/// (pass 2). Routes, verbs and body shape against the local <see cref="RecordingServer"/>.
/// </summary>
public class QRCodeClientContentTypeTests : IDisposable
{
    private const string Tpl = "tpl-1";
    private readonly RecordingServer _server = new();
    private readonly QRCodeClient _client;

    public QRCodeClientContentTypeTests()
    {
        _client = new QRCodeClient(_server.Http());
    }

    public void Dispose() => _server.Dispose();

    private static JsonElement Target(string body, string type)
    {
        var root = JsonDocument.Parse(body).RootElement;
        Assert.False(root.TryGetProperty("options", out _), "options.text is built by the API");
        var target = root.GetProperty("qrCodeTarget");
        Assert.Equal(type, target.GetProperty("type").GetString());
        return target.GetProperty(type);
    }

    /// <summary>A result that reads both as an upload ticket and as a QR code.</summary>
    private void AnswerWithTicket(string uploadUrl, int expiresInSeconds = 60)
        => _server.ResultJson = JsonSerializer.Serialize(new Dictionary<string, object>
        {
            ["uploadFileURL"] = uploadUrl,
            ["bucketFilePath"] = "qr-code-files/u1/menu.pdf",
            ["expiresInSeconds"] = expiresInSeconds,
            ["_id"] = "q1"
        });

    // ─── Pass 1 ──────────────────────────────────────────────────────────────

    [Fact]
    public async Task Pass1Types_CreateAndUpdate_HitTheirRoutes_WithTheTargetOnly()
    {
        var vcard = new QRCodeVCardTargetModel { FirstName = "Ada", Phones = new() { new QRCodeVCardPhoneModel { Kind = QRCodeVCardPhoneKinds.Mobile, Number = "+201000000000" } } };
        var ev = new QRCodeEventTargetModel { Title = "Launch", StartsAt = DateTimeOffset.Parse("2026-11-01T18:00:00Z") };
        var wa = new QRCodeWhatsAppTargetModel { PhoneNumber = "+201000000000", Message = "Hi" };
        var review = new QRCodeReviewTargetModel { Platform = QRCodeReviewPlatforms.Google, PlaceId = "ChIJ123" };
        var social = new QRCodeSocialTargetModel { Profiles = new() { new QRCodeSocialProfileModel { Platform = QRCodeSocialPlatforms.Instagram, Handle = "posty5" } } };

        await _client.CreateVCardAsync(new QRCodeCreateVCardRequestModel { TemplateId = Tpl, VCard = vcard });
        await _client.CreateEventAsync(new QRCodeCreateEventRequestModel { TemplateId = Tpl, Event = ev });
        await _client.CreateWhatsAppAsync(new QRCodeCreateWhatsAppRequestModel { TemplateId = Tpl, WhatsApp = wa });
        await _client.CreateReviewAsync(new QRCodeCreateReviewRequestModel { TemplateId = Tpl, Review = review });
        await _client.CreateSocialAsync(new QRCodeCreateSocialRequestModel { TemplateId = Tpl, Social = social });
        await _client.UpdateVCardAsync("q1", new QRCodeUpdateVCardRequestModel { Name = "n", TemplateId = Tpl, VCard = vcard }, 0);
        await _client.UpdateEventAsync("q1", new QRCodeUpdateEventRequestModel { Name = "n", TemplateId = Tpl, Event = ev }, 0);
        await _client.UpdateWhatsAppAsync("q1", new QRCodeUpdateWhatsAppRequestModel { Name = "n", TemplateId = Tpl, WhatsApp = wa }, 0);
        await _client.UpdateReviewAsync("q1", new QRCodeUpdateReviewRequestModel { Name = "n", TemplateId = Tpl, Review = review }, 0);
        await _client.UpdateSocialAsync("q1", new QRCodeUpdateSocialRequestModel { Name = "n", TemplateId = Tpl, Social = social }, 0);

        var types = new[] { "vcard", "event", "whatsapp", "review", "social" };
        Assert.Equal(
            types.Select(t => $"POST /api/qr-code/{t}").Concat(types.Select(t => $"PUT /api/qr-code/{t}/q1")),
            _server.Requests.Select(r => $"{r.Method} {r.PathAndQuery}"));

        foreach (var (_, path, body) in _server.Requests)
        {
            Assert.Equal(Tpl, JsonDocument.Parse(body).RootElement.GetProperty("templateId").GetString());
            Target(body, path.Split('/')[3]);
        }
        Assert.Equal("posty5", Target(_server.Requests[4].Body, "social").GetProperty("profiles")[0].GetProperty("handle").GetString());
        Assert.Equal("ChIJ123", Target(_server.Requests[3].Body, "review").GetProperty("placeId").GetString());
        Assert.Equal("+201000000000", Target(_server.Requests[0].Body, "vcard").GetProperty("phones")[0].GetProperty("number").GetString());
    }

    [Fact]
    public async Task Event_SendsIso8601Times()
    {
        await _client.CreateEventAsync(new QRCodeCreateEventRequestModel
        {
            TemplateId = Tpl,
            Event = new() { Title = "Launch", StartsAt = DateTimeOffset.Parse("2026-11-01T18:00:00Z"), EndsAt = DateTimeOffset.Parse("2026-11-01T20:00:00Z") }
        });

        var ev = Target(_server.Requests.Single().Body, "event");
        Assert.Equal(DateTimeOffset.Parse("2026-11-01T18:00:00Z"), DateTimeOffset.Parse(ev.GetProperty("startsAt").GetString()!));
        Assert.Equal(DateTimeOffset.Parse("2026-11-01T20:00:00Z"), DateTimeOffset.Parse(ev.GetProperty("endsAt").GetString()!));
    }

    [Fact]
    public async Task Pass1Types_ModeIsPassedThroughOnlyWhenSet()
    {
        await _client.CreateWhatsAppAsync(new QRCodeCreateWhatsAppRequestModel { TemplateId = Tpl, Mode = QRCodeMode.Dynamic, WhatsApp = new() { PhoneNumber = "+1" } });
        await _client.CreateWhatsAppAsync(new QRCodeCreateWhatsAppRequestModel { TemplateId = Tpl, WhatsApp = new() { PhoneNumber = "+1" } });

        Assert.Equal("dynamic", JsonDocument.Parse(_server.Requests[0].Body).RootElement.GetProperty("mode").GetString());
        Assert.False(JsonDocument.Parse(_server.Requests[1].Body).RootElement.TryGetProperty("mode", out _));
    }

    [Fact]
    public async Task Update_WithoutId_ThrowsBeforeAnyCall()
    {
        await Assert.ThrowsAsync<ArgumentException>(() => _client.UpdateVCardAsync("", new QRCodeUpdateVCardRequestModel { Name = "n", TemplateId = Tpl }, 0));
        await Assert.ThrowsAsync<ArgumentException>(() => _client.UpdateAppStoreAsync(" ", new QRCodeUpdateAppStoreRequestModel { Name = "n", TemplateId = Tpl }, 0));
        await Assert.ThrowsAsync<ArgumentException>(() => _client.UpdateFileAsync("", new QRCodeUpdateFileRequestModel { Name = "n", TemplateId = Tpl }, 0));
        Assert.Empty(_server.Requests);
    }

    [Fact]
    public async Task Social_SendsUpTo12Profiles_OnADynamicCode()
    {
        var profiles = Enumerable.Range(1, 12).Select(i => new QRCodeSocialProfileModel { Platform = QRCodeSocialPlatforms.Instagram, Handle = $"h{i}" }).ToList();

        await _client.CreateSocialAsync(new QRCodeCreateSocialRequestModel { TemplateId = Tpl, Mode = QRCodeMode.Dynamic, Social = new() { Profiles = profiles } });

        Assert.Equal(12, Target(_server.Requests.Single().Body, "social").GetProperty("profiles").GetArrayLength());
    }

    [Fact]
    public async Task Responses_KeepUnknownPlatformsAndReadTheNewTargets()
    {
        _server.ResultJson =
            "{ \"_id\": \"q1\", \"qrCodeTarget\": { \"type\": \"social\", " +
            "\"social\": { \"profiles\": [ { \"platform\": \"mastodon\", \"handle\": \"x\" } ] }, " +
            "\"appStore\": { \"fallbackUrl\": \"https://example.com\" }, " +
            "\"file\": { \"fileName\": \"menu.pdf\", \"fileURL\": \"https://files/menu.pdf\", \"mimeType\": \"application/pdf\", \"sizeBytes\": 42 } } }";

        var qr = await _client.GetAsync("q1");

        Assert.Equal("mastodon", qr.QrCodeTarget!.Social!.Profiles[0].Platform);
        Assert.Equal("https://example.com", qr.QrCodeTarget.AppStore!.FallbackUrl);
        Assert.Equal(42, qr.QrCodeTarget.File!.SizeBytes);
        Assert.Equal(QRCodeFileMimeTypes.Pdf, qr.QrCodeTarget.File.MimeType);
    }

    // ─── Pass 2: app store ───────────────────────────────────────────────────

    [Fact]
    public async Task AppStore_CreateAndUpdate_SendTheTarget()
    {
        var target = new QRCodeAppStoreTargetModel { AndroidUrl = "https://play.google.com/store/apps/details?id=x", IosUrl = "https://apps.apple.com/app/id1", FallbackUrl = "https://example.com/app" };

        await _client.CreateAppStoreAsync(new QRCodeCreateAppStoreRequestModel { TemplateId = Tpl, AppStore = target });
        await _client.UpdateAppStoreAsync("q1", new QRCodeUpdateAppStoreRequestModel { Name = "n", TemplateId = Tpl, Mode = QRCodeMode.Dynamic, AppStore = target }, 0);

        Assert.Equal(new[] { "POST /api/qr-code/appStore", "PUT /api/qr-code/appStore/q1" }, _server.Requests.Select(r => $"{r.Method} {r.PathAndQuery}"));
        var sent = Target(_server.Requests[0].Body, "appStore");
        Assert.Equal("https://example.com/app", sent.GetProperty("fallbackUrl").GetString());
        Assert.Equal("https://apps.apple.com/app/id1", sent.GetProperty("iosUrl").GetString());
        Assert.Equal("dynamic", JsonDocument.Parse(_server.Requests[1].Body).RootElement.GetProperty("mode").GetString());
    }

    [Fact]
    public async Task AppStoreAndFile_StaticMode_ThrowsBeforeAnyCall()
    {
        await Assert.ThrowsAsync<ArgumentException>(() => _client.CreateAppStoreAsync(new QRCodeCreateAppStoreRequestModel { TemplateId = Tpl, Mode = QRCodeMode.Static }));
        await Assert.ThrowsAsync<ArgumentException>(() => _client.UpdateAppStoreAsync("q1", new QRCodeUpdateAppStoreRequestModel { Name = "n", TemplateId = Tpl, Mode = QRCodeMode.Static }, 0));
        await Assert.ThrowsAsync<ArgumentException>(() => _client.CreateFileAsync(new QRCodeCreateFileRequestModel { TemplateId = Tpl, Mode = QRCodeMode.Static }, new MemoryStream(new byte[] { 1 }), QRCodeFileMimeTypes.Pdf));
        await Assert.ThrowsAsync<ArgumentException>(() => _client.UpdateFileAsync("q1", new QRCodeUpdateFileRequestModel { Name = "n", TemplateId = Tpl, Mode = QRCodeMode.Static }, 0));
        Assert.Empty(_server.Requests);
    }

    // ─── Pass 2: file ────────────────────────────────────────────────────────

    [Fact]
    public async Task CreateFile_UploadUrl_ThenPut_ThenCreateWithBucketFilePath()
    {
        AnswerWithTicket($"{_server.BaseUrl}/r2/signed-put");
        var bytes = new byte[] { 0x25, 0x50, 0x44, 0x46, 0x2D };

        var qr = await _client.CreateFileAsync(
            new QRCodeCreateFileRequestModel { Name = "Menu", TemplateId = Tpl, File = new() { FileName = "menu.pdf" } },
            new MemoryStream(bytes),
            QRCodeFileMimeTypes.Pdf);

        Assert.Equal("q1", qr.Id);
        Assert.Equal(
            new[] { "POST /api/qr-code/file/upload-url", "PUT /r2/signed-put", "POST /api/qr-code/file" },
            _server.Requests.Select(r => $"{r.Method} {r.PathAndQuery}"));

        var ticketRequest = JsonDocument.Parse(_server.Requests[0].Body).RootElement;
        Assert.Equal("menu.pdf", ticketRequest.GetProperty("fileName").GetString());
        Assert.Equal("application/pdf", ticketRequest.GetProperty("mimeType").GetString());
        Assert.Equal(bytes.Length, ticketRequest.GetProperty("sizeBytes").GetInt64());

        Assert.Equal("application/pdf", _server.Headers[1]["Content-Type"]);
        Assert.False(_server.Headers[1].ContainsKey("Authorization"), "the API key is not sent to the signed URL");
        Assert.False(_server.Headers[1].ContainsKey("x-api-key"), "the API key is not sent to the signed URL");

        var file = Target(_server.Requests[2].Body, "file");
        Assert.Equal("qr-code-files/u1/menu.pdf", file.GetProperty("bucketFilePath").GetString());
        Assert.Equal("menu.pdf", file.GetProperty("fileName").GetString());
        Assert.False(file.TryGetProperty("mimeType", out _), "mimeType is set by the API");
    }

    [Fact]
    public async Task CreateFile_WithoutFileName_SendsFile()
    {
        AnswerWithTicket($"{_server.BaseUrl}/r2/signed-put");

        await _client.CreateFileAsync(new QRCodeCreateFileRequestModel { TemplateId = Tpl }, new MemoryStream(new byte[] { 1, 2 }), QRCodeFileMimeTypes.Png);

        Assert.Equal("file", JsonDocument.Parse(_server.Requests[0].Body).RootElement.GetProperty("fileName").GetString());
    }

    [Fact]
    public async Task CreateFile_InvalidInput_ThrowsBeforeAnyCall()
    {
        var data = new QRCodeCreateFileRequestModel { TemplateId = Tpl };
        await Assert.ThrowsAsync<ArgumentException>(() => _client.CreateFileAsync(data, new MemoryStream(), QRCodeFileMimeTypes.Pdf));
        await Assert.ThrowsAsync<ArgumentException>(() => _client.CreateFileAsync(data, new MemoryStream(new byte[] { 1 }), ""));
        await Assert.ThrowsAsync<ArgumentException>(() => _client.CreateFileAsync(data, new NonSeekableStream(new byte[] { 1 }), QRCodeFileMimeTypes.Pdf));
        Assert.Empty(_server.Requests);
    }

    [Fact]
    public async Task CreateFile_NonSeekableStream_UsesFileSizeBytes()
    {
        AnswerWithTicket($"{_server.BaseUrl}/r2/signed-put");

        await _client.CreateFileAsync(
            new QRCodeCreateFileRequestModel { TemplateId = Tpl, File = new() { FileName = "a.png", SizeBytes = 3 } },
            new NonSeekableStream(new byte[] { 1, 2, 3 }),
            QRCodeFileMimeTypes.Png);

        Assert.Equal(3, JsonDocument.Parse(_server.Requests[0].Body).RootElement.GetProperty("sizeBytes").GetInt64());
        Assert.Equal(3, _server.Requests.Count);
    }

    [Fact]
    public async Task CreateFile_NetworkErrorOnThePut_IsRetriedOnceThenThrown()
    {
        // A closed port: every PUT is a network error (no HTTP status).
        var probe = new System.Net.Sockets.TcpListener(IPAddress.Loopback, 0);
        probe.Start();
        var closedPort = ((IPEndPoint)probe.LocalEndpoint).Port;
        probe.Stop();
        AnswerWithTicket($"http://127.0.0.1:{closedPort}/r2/signed-put");

        var error = await Assert.ThrowsAsync<HttpRequestException>(() => _client.CreateFileAsync(
            new QRCodeCreateFileRequestModel { TemplateId = Tpl }, new MemoryStream(new byte[] { 1 }), QRCodeFileMimeTypes.Pdf));

        Assert.Null(error.StatusCode);
        Assert.Equal(new[] { "POST /api/qr-code/file/upload-url" }, _server.Requests.Select(r => $"{r.Method} {r.PathAndQuery}"));
    }

    [Fact]
    public async Task UpdateFile_WithoutContent_KeepsTheStoredFile()
    {
        await _client.UpdateFileAsync("q1", new QRCodeUpdateFileRequestModel { Name = "Menu", TemplateId = Tpl, File = new() { FileName = "menu-2026.pdf" } }, 0);

        var (method, path, body) = _server.Requests.Single();
        Assert.Equal("PUT /api/qr-code/file/q1", $"{method} {path}");
        var file = Target(body, "file");
        Assert.Equal("menu-2026.pdf", file.GetProperty("fileName").GetString());
        Assert.False(file.TryGetProperty("bucketFilePath", out _));
    }

    [Fact]
    public async Task UpdateFile_WithContent_UploadsFirst_ThenPutsTheNewBucketFilePath()
    {
        AnswerWithTicket($"{_server.BaseUrl}/r2/signed-put");

        await _client.UpdateFileAsync("q1", new QRCodeUpdateFileRequestModel { Name = "Menu", TemplateId = Tpl }, 0, new MemoryStream(new byte[] { 1, 2 }), QRCodeFileMimeTypes.Webp);

        Assert.Equal(
            new[] { "POST /api/qr-code/file/upload-url", "PUT /r2/signed-put", "PUT /api/qr-code/file/q1" },
            _server.Requests.Select(r => $"{r.Method} {r.PathAndQuery}"));
        Assert.Equal("qr-code-files/u1/menu.pdf", Target(_server.Requests[2].Body, "file").GetProperty("bucketFilePath").GetString());
    }

    [Fact]
    public async Task UpdateFile_WithContentButNoContentType_ThrowsBeforeAnyCall()
    {
        await Assert.ThrowsAsync<ArgumentException>(() => _client.UpdateFileAsync("q1", new QRCodeUpdateFileRequestModel { Name = "n", TemplateId = Tpl }, 0, new MemoryStream(new byte[] { 1 })));
        Assert.Empty(_server.Requests);
    }

    [Fact]
    public void UploadExpiredException_CarriesARetryHint()
    {
        var error = new QRCodeFileUploadExpiredException(60, new HttpRequestException("x"));
        Assert.IsAssignableFrom<Posty5Exception>(error);
        Assert.Contains("60 s", error.Message);
        Assert.Contains("retry", error.Message);
    }

    /// <summary>A stream that cannot seek, like a network body.</summary>
    private sealed class NonSeekableStream : MemoryStream
    {
        public NonSeekableStream(byte[] bytes) : base(bytes) { }
        public override bool CanSeek => false;
    }
}
