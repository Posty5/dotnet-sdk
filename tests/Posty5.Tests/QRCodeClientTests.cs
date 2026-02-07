using Xunit;
using Posty5.QRCode;
using Posty5.QRCode.Models;
using Posty5.Core.Exceptions;

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
    public async Task CreateQRCode_WithMonetization_ShouldIncludePageInfo()
    {
        // Arrange
        var request = new QRCodeCreateURLRequestModel
        {
            Name = $"Monetized QR - {DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}",
            TemplateId = TestConfig.TemplateId,
            IsEnableMonetization = true,
            PageInfo = new QRCodePageInfoModel
            {
                Title = "Please Wait",
                Description = "You will be redirected shortly..."
            },
            Url = new QRCodeUrlTargetModel { Url = "https://posty5.com" }
        };

        // Act
        var result = await _client.CreateURLAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Id);
        Assert.True(result.IsEnableMonetization);
        Assert.NotNull(result.PageInfo);

        TestConfig.CreatedResources.QRCodes.Add(result.Id!);
    }

    #endregion

    public void Dispose()
    {
        // Cleanup is handled by collection fixture if needed
    }
}

