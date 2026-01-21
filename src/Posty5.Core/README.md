# Posty5.Core

Core HTTP client and utilities for the Posty5 .NET SDK ecosystem. This package provides the foundational infrastructure that powers all other Posty5 SDK modules.

---

## 🌟 What is Posty5?

**Posty5** is a comprehensive suite of free online tools designed to enhance your digital marketing and social media presence. With over 4+ powerful tools and counting, Posty5 provides everything you need to:

- 🔗 **Shorten URLs** - Create memorable, trackable short links
- 📱 **Generate QR Codes** - Transform URLs, WiFi credentials, contact cards, and more into scannable codes
- 🌐 **Host HTML Pages** - Deploy static HTML pages with dynamic variables and form submission handling
- 📢 **Automate Social Media** - Schedule and manage social media posts across multiple platforms
- 📊 **Track Performance** - Monitor and analyze your digital marketing efforts

Posty5 empowers businesses, marketers, and developers to streamline their online workflows—all from a unified control panel.

**Learn more:** [https://posty5.com](https://posty5.com)

---

## 📦 About This Package

`Posty5.Core` is the **foundation package** for the entire Posty5 .NET SDK ecosystem. It provides:

- **HTTP Client** - System.Net.Http-based client with built-in retry logic using Polly
- **Authentication** - API key management for secure API communication
- **Error Handling** - Typed exception classes for robust error management
- **Type Definitions** - Full C# type support with comprehensive models
- **Configuration** - Flexible configuration options with dependency injection support
- **.NET 8.0 Support** - Built with the latest .NET features

### Role in the Posty5 Ecosystem

This package serves as the **core dependency** for all Posty5 SDK modules. It handles:

- API authentication and request management
- Network communication with the Posty5 API
- Standardized error handling across all SDK packages
- Retry logic with exponential backoff for transient failures

---

## 📥 Installation

Install via NuGet Package Manager:

```bash
dotnet add package Posty5.Core
```

Or via Package Manager Console:

```powershell
Install-Package Posty5.Core
```

---

## ⚠️ Important: Not a Standalone Package

**This package is NOT designed to work as a standalone solution.**

`Posty5.Core` provides the foundational infrastructure and utilities that other Posty5 SDK packages depend on. While it can be used directly for low-level API interactions, it is primarily intended to be used **in combination with other Posty5 tool packages** such as:

- `Posty5.ShortLink` - For URL shortening
- `Posty5.QRCode` - For QR code generation
- `Posty5.HtmlHosting` - For HTML page hosting
- `Posty5.SocialPublisher` - For social media workspace and task management

For most use cases, you should install the specific tool package you need, which will automatically include `Posty5.Core` as a dependency.

---

## 🎯 Why This Package Matters

### The Value of Posty5.Core

1. **Unified API Communication**
   - Provides a single, consistent HTTP client for all Posty5 SDK packages
   - Eliminates the need for each package to implement its own API communication layer

2. **Automatic Retry Logic**
   - Built-in retry mechanism using Polly for transient network failures
   - Configurable retry policies with exponential backoff

3. **Type Safety**
   - Strong typing with C# generics for all API responses
   - Nullable reference types enabled for compile-time null safety

4. **Error Handling**
   - Custom exception hierarchy for specific error scenarios
   - Detailed error messages with HTTP status codes

5. **Performance**
   - Efficient JSON serialization with System.Text.Json
   - Connection pooling and HTTP/2 support

---

## 🚀 Quick Start

### Basic Usage

```csharp
using Posty5.Core.Configuration;
using Posty5.Core.Http;

// Initialize the HTTP client with your API key
var options = new Posty5Options
{
    BaseUrl = "https://api.posty5.com",
    ApiKey = "your-api-key", // Get from https://studio.posty5.com/account/settings?tab=APIKeys
    Debug = false // Set to true for debugging
};

var httpClient = new Posty5HttpClient(options);

// The client is now ready to be used by other Posty5 packages
```

### With Dependency Injection

```csharp
using Microsoft.Extensions.DependencyInjection;
using Posty5.Core.Configuration;
using Posty5.Core.Http;

var services = new ServiceCollection();

// Register Posty5 HTTP client
services.AddSingleton(sp =>
{
    var options = new Posty5Options
    {
        BaseUrl = "https://api.posty5.com",
        ApiKey = Environment.GetEnvironmentVariable("POSTY5_API_KEY") ?? "",
        TimeoutSeconds = 30,
        MaxRetries = 3
    };
    return new Posty5HttpClient(options);
});

var serviceProvider = services.BuildServiceProvider();
var httpClient = serviceProvider.GetRequiredService<Posty5HttpClient>();
```

---

## 📖 Configuration Options

### Posty5Options

| Property         | Type     | Default                    | Description                      |
| ---------------- | -------- | -------------------------- | -------------------------------- |
| `ApiKey`         | `string` | `""`                       | Your Posty5 API key (required)   |
| `BaseUrl`        | `string` | `"https://api.posty5.com"` | Base URL for the API             |
| `TimeoutSeconds` | `int`    | `30`                       | Request timeout in seconds       |
| `MaxRetries`     | `int`    | `3`                        | Maximum number of retry attempts |
| `Debug`          | `bool`   | `false`                    | Enable debug logging             |

---

## 🔒 Error Handling

The package includes a comprehensive exception hierarchy:

### Exception Types

- **`Posty5Exception`** - Base exception for all Posty5 errors
- **`Posty5AuthenticationException`** - Authentication failures (401)
- **`Posty5NotFoundException`** - Resource not found (404)
- **`Posty5ValidationException`** - Validation errors (400)
- **`Posty5RateLimitException`** - Rate limit exceeded (429)

### Example

```csharp
using Posty5.Core.Exceptions;

try
{
    var response = await httpClient.GetAsync<MyModel>("/api/endpoint");
    var result = response.Result;
}
catch (Posty5AuthenticationException ex)
{
    Console.WriteLine("Authentication failed: " + ex.Message);
}
catch (Posty5NotFoundException ex)
{
    Console.WriteLine("Resource not found: " + ex.Message);
}
catch (Posty5Exception ex)
{
    Console.WriteLine($"API error: {ex.Message} (Status: {ex.StatusCode})");
}
```

---

## 📚 API Reference

### Posty5HttpClient

The main HTTP client for making API requests.

#### Methods

**GetAsync\<T\>(string path, Dictionary\<string, object?\>? queryParams, CancellationToken cancellationToken)**

- Performs a GET request
- Returns `ApiResponse<T>` with the result

**PostAsync\<T\>(string path, object? body, CancellationToken cancellationToken)**

- Performs a POST request
- Returns `ApiResponse<T>` with the result

**PutAsync\<T\>(string path, object? body, CancellationToken cancellationToken)**

- Performs a PUT request
- Returns `ApiResponse<T>` with the result

**DeleteAsync(string path, CancellationToken cancellationToken)**

- Performs a DELETE request
- Returns `bool` indicating success

**SetApiKey(string apiKey)**

- Updates the API key for subsequent requests

---

## 🔧 Advanced Usage

### Custom Retry Policy

The client uses Polly for retry logic with the following defaults:

- Maximum 3 retry attempts
- Exponential backoff: 2^attempt seconds
- Retries on 408, 429, 500, 502, 503, 504 status codes

### Debug Logging

Enable debug logging to see request/response details:

```csharp
var options = new Posty5Options
{
    ApiKey = "your-api-key",
    Debug = true // Logs to Console
};
```

---

## 🤝 Related Packages

- **[Posty5.QRCode](../Posty5.QRCode)** - QR code generation and management
- **[Posty5.ShortLink](../Posty5.ShortLink)** - URL shortening functionality
- **[Posty5.HtmlHosting](../Posty5.HtmlHosting)** - HTML page hosting
- **[Posty5.SocialPublisher](../Posty5.SocialPublisher)** - Social media publishing

---

## 📄 License

MIT License - See [LICENSE](../../LICENSE) file in the root directory.

---

## 🔗 Resources

- **Documentation**: [https://posty5.com/docs](https://posty5.com/docs)
- **API Reference**: [https://posty5.com/api](https://posty5.com/api)
- **Get API Key**: [https://studio.posty5.com/account/settings?tab=APIKeys](https://studio.posty5.com/account/settings?tab=APIKeys)
- **Support**: [https://posty5.com/contact-us](https://posty5.com/contact-us)

---

Made with ❤️ by the Posty5 Team
