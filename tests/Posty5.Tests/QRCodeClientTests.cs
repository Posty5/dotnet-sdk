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

    [Fact]
    public async Task CreateUrlQRCode_ShouldReturnValidQRCode()
    {
        // Arrange
        var request = new CreateUrlQRCodeRequest
        {
            Name = $"Test URL QR Code - {DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}",
            TemplateId = TestConfig.TemplateId,
            QrCodeTarget = new UrlQRTarget
            {
                Url = "https://posty5.com"
            }
        };

        // Act
        var result = await _client.CreateUrlAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Id);
        Assert.NotNull(result.QrCodeLandingPage);

        _createdId = result.Id;
        TestConfig.CreatedResources.QRCodes.Add(_createdId);
    }

    [Fact]
    public async Task CreateFreeTextQRCode_ShouldReturnValidQRCode()
    {
        // Arrange
        var request = new CreateFreeTextQRCodeRequest
        {
            Name = "Test Free Text QR",
            TemplateId = TestConfig.TemplateId,
            QrCodeTarget = new FreeTextQRTarget
            {
                Text = "Hello from QR Code Test!"
            }
        };

        // Act
        var result = await _client.CreateFreeTextAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Id);
        Assert.NotNull(result.QrCodeLandingPage);

        TestConfig.CreatedResources.QRCodes.Add(result.Id!);
    }

    [Fact]
    public async Task CreateEmailQRCode_ShouldReturnValidQRCode()
    {
        // Arrange
        var request = new CreateEmailQRCodeRequest
        {
            Name = "Test Email QR",
            TemplateId = TestConfig.TemplateId,
            QrCodeTarget = new EmailQRTarget
            {
                Email = "test@example.com",
                Subject = "Test Subject",
                Body = "Test Body"
            }
        };

        // Act
        var result = await _client.CreateEmailAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Id);
        Assert.NotNull(result.QrCodeLandingPage);

        TestConfig.CreatedResources.QRCodes.Add(result.Id!);
    }

    [Fact]
    public async Task CreateWifiQRCode_ShouldReturnValidQRCode()
    {
        // Arrange
        var request = new CreateWifiQRCodeRequest
        {
            Name = "Test WiFi QR",
            TemplateId = TestConfig.TemplateId,
            QrCodeTarget = new WifiQRTarget
            {
                Ssid = "TestNetwork",
                Password = "testpassword123",
                SecurityType = "WPA",
                Hidden = false
            }
        };

        // Act
        var result = await _client.CreateWifiAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Id);
        Assert.NotNull(result.QrCodeLandingPage);

        TestConfig.CreatedResources.QRCodes.Add(result.Id!);
    }

    [Fact]
    public async Task CreateCallQRCode_ShouldReturnValidQRCode()
    {
        // Arrange
        var request = new CreateCallQRCodeRequest
        {
            Name = "Test Call QR",
            TemplateId = TestConfig.TemplateId,
            QrCodeTarget = new CallQRTarget
            {
                PhoneNumber = "+1234567890"
            }
        };

        // Act
        var result = await _client.CreateCallAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Id);
        Assert.NotNull(result.QrCodeLandingPage);

        TestConfig.CreatedResources.QRCodes.Add(result.Id!);
    }

    [Fact]
    public async Task CreateSmsQRCode_ShouldReturnValidQRCode()
    {
        // Arrange
        var request = new CreateSmsQRCodeRequest
        {
            Name = "Test SMS QR",
            TemplateId = TestConfig.TemplateId,
            QrCodeTarget = new SmsQRTarget
            {
                PhoneNumber = "+1234567890",
                Message = "Hello from QR Code!"
            }
        };

        // Act
        var result = await _client.CreateSmsAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Id);
        Assert.NotNull(result.QrCodeLandingPage);

        TestConfig.CreatedResources.QRCodes.Add(result.Id!);
    }

    [Fact]
    public async Task CreateGeolocationQRCode_ShouldReturnValidQRCode()
    {
        // Arrange
        var request = new CreateGeolocationQRCodeRequest
        {
            Name = "Test Location QR",
            TemplateId = TestConfig.TemplateId,
            QrCodeTarget = new GeolocationQRTarget
            {
                Latitude = 40.7128,
                Longitude = -74.0060
            }
        };

        // Act
        var result = await _client.CreateGeolocationAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Id);
        Assert.NotNull(result.QrCodeLandingPage);

        TestConfig.CreatedResources.QRCodes.Add(result.Id!);
    }

    [Fact]
    public async Task GetQRCodeById_WithValidId_ShouldReturnQRCode()
    {
        // Arrange - Create a QR code first
        var createRequest = new CreateUrlQRCodeRequest
        {
            Name = "Test QR for Get",
            TemplateId = TestConfig.TemplateId,
            QrCodeTarget = new UrlQRTarget { Url = "https://posty5.com" }
        };
        var created = await _client.CreateUrlAsync(createRequest);
        TestConfig.CreatedResources.QRCodes.Add(created.Id!);

        // Act
        var result = await _client.GetAsync(created.Id!);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(created.Id, result.Id);
        Assert.NotNull(result.Name);
    }

    [Fact]
    public async Task GetQRCodeById_WithInvalidId_ShouldThrowException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<Posty5Exception>(async () =>
            await _client.GetAsync("invalid-id-123")
        );
    }

    [Fact]
    public async Task ListQRCodes_ShouldReturnPaginatedResults()
    {
        // Act
        var result = await _client.ListAsync(
            pagination: new Core.Models.PaginationParams { PageNumber = 0, PageSize = 10 }
        );

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Items);
        Assert.True(result.TotalCount >= 0);
    }

    [Fact]
    public async Task ListQRCodes_WithSearch_ShouldFilterResults()
    {
        // Arrange
        var searchParams = new ListQRCodesParams
        {
            Search = "test"
        };

        // Act
        var result = await _client.ListAsync(
            searchParams,
            new Core.Models.PaginationParams { PageNumber = 0, PageSize = 10 }
        );

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Items);
    }

    [Fact]
    public async Task UpdateQRCode_ShouldUpdateSuccessfully()
    {
        // Arrange - Create a QR code first
        var createRequest = new CreateUrlQRCodeRequest
        {
            Name = "Original Name",
            TemplateId = TestConfig.TemplateId,
            QrCodeTarget = new UrlQRTarget { Url = "https://posty5.com" }
        };
        var created = await _client.CreateUrlAsync(createRequest);
        TestConfig.CreatedResources.QRCodes.Add(created.Id!);

        // Act
        var updateRequest = new CreateUrlQRCodeRequest
        {
            Name = $"Updated Name - {DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}",
            TemplateId = TestConfig.TemplateId,
            QrCodeTarget = new UrlQRTarget { Url = "https://updated.posty5.com" }
        };
        var result = await _client.UpdateAsync(created.Id!, updateRequest);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(created.Id, result.Id);
    }

    [Fact]
    public async Task DeleteQRCode_ShouldDeleteSuccessfully()
    {
        // Arrange - Create a QR code first
        var createRequest = new CreateUrlQRCodeRequest
        {
            Name = "QR to Delete",
            TemplateId = TestConfig.TemplateId,
            QrCodeTarget = new UrlQRTarget { Url = "https://posty5.com" }
        };
        var created = await _client.CreateUrlAsync(createRequest);

        // Act
        var deleteResult = await _client.DeleteAsync(created.Id!);

        // Assert
        Assert.True(deleteResult);

        // Verify deletion
        await Assert.ThrowsAsync<Posty5Exception>(async () =>
            await _client.GetAsync(created.Id!)
        );
    }

    public void Dispose()
    {
        // Cleanup is handled by collection fixture if needed
    }
}
