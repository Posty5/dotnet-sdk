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
- `Posty5.SocialPublisher` - For social media workspace and post management

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
        ApiKey = Environment.GetEnvironmentVariable("POSTY5_API_KEY") ?? "",
    };
    return new Posty5HttpClient(options);
});

var serviceProvider = services.BuildServiceProvider();
var httpClient = serviceProvider.GetRequiredService<Posty5HttpClient>();
```

---

## 📖 Configuration Options

### Posty5Options

| Property         | Type                          | Default                  | Description |
| ---------------- | ----------------------------- | ------------------------ | ----------- |
| `ApiKey`         | `string?`                     | `null`                   | Your Posty5 API key, sent as `X-API-Key` (required for every route except the public ones) |
| `BaseUrl`        | `string`                      | `https://api.posty5.com` | API origin |
| `Debug`          | `bool`                        | `false`                  | Enable debug logging |
| `DefaultHeaders` | `Dictionary<string, string>?` | `null`                   | Headers sent on every request. `X-API-Key` is refused (`ArgumentException`); an `X-Posty5-Client` entry replaces the SDK's label; content headers are refused |
| `CreatedFrom`    | `string?`                     | `null`                   | The `createdFrom` label on every record the clients create. Null keeps each package's default (`dotnetPackage`; `dotnet` for store orders) |

Every request also carries `X-Posty5-Client: posty5-dotnet/<Posty5.Core version>`
(`Posty5ClientIdentity.HeaderValue`), which the API logs to tell SDK traffic apart.

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

### No automatic retries

The client does **not** retry. A request that fails — a network error, a 5xx, a
429 — is reported as an exception and never repeated, so a create can never be
sent twice. Retry in your own code where an operation is safe to repeat. (The
`MaxRetries` and `RetryDelayMilliseconds` fields on `Posty5Options` are not
used.) Requests time out after 120 seconds.

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

## 📖 Resources

- **Official Guides**: [https://guide.posty5.com](https://guide.posty5.com)
- **API Reference**: [https://docs.posty5.com](https://docs.posty5.com)
- **Source Code**: [https://github.com/Posty5/dotnet-sdk](https://github.com/Posty5/dotnet-sdk)

---

## 📦 Packages

This SDK ecosystem contains the following tool packages:

| Package                                                                 | Description                   | Version | NuGet                                                                       |
| ----------------------------------------------------------------------- | ----------------------------- | ------- | --------------------------------------------------------------------------- |
| [Posty5.Core](../Posty5.Core)                                           | Core HTTP client and models   | 1.0.0   | [📦 NuGet](https://www.nuget.org/packages/Posty5.Core)                      |
| [Posty5.ShortLink](../Posty5.ShortLink)                                 | URL shortener client          | 1.0.0   | [📦 NuGet](https://www.nuget.org/packages/Posty5.ShortLink)                 |
| [Posty5.QRCode](../Posty5.QRCode)                                       | QR code generator client      | 1.0.0   | [📦 NuGet](https://www.nuget.org/packages/Posty5.QRCode)                    |
| [Posty5.HtmlHosting](../Posty5.HtmlHosting)                             | HTML hosting client           | 1.0.0   | [📦 NuGet](https://www.nuget.org/packages/Posty5.HtmlHosting)               |
| [Posty5.HtmlHostingVariables](../Posty5.HtmlHostingVariables)           | Variable management           | 1.0.0   | [📦 NuGet](https://www.nuget.org/packages/Posty5.HtmlHostingVariables)      |
| [Posty5.HtmlHostingFormSubmission](../Posty5.HtmlHostingFormSubmission) | Form submission management    | 1.0.0   | [📦 NuGet](https://www.nuget.org/packages/Posty5.HtmlHostingFormSubmission) |
| [Posty5.SocialPublisherWorkspace](../Posty5.SocialPublisherWorkspace)   | Social workspace management   | 1.0.0   | [📦 NuGet](https://www.nuget.org/packages/Posty5.SocialPublisherWorkspace)  |
| [Posty5.SocialPublisherPost](../Posty5.SocialPublisherPost)             | Social publishing post client | 1.0.0   | [📦 NuGet](https://www.nuget.org/packages/Posty5.SocialPublisherPost)       |

---

## 🆘 Support

We're here to help you succeed with Posty5!

### Get Help

- **Documentation**: [https://guide.posty5.com](https://guide.posty5.com)
- **Contact Us**: [https://posty5.com/contact-us](https://posty5.com/contact-us)
- **GitHub Issues**: [Report bugs or request features](https://github.com/Posty5/npm-sdk/issues)
- **API Status**: Check API status and uptime at [https://status.posty5.com](https://status.posty5.com)

---

## 📄 License

MIT License - see [LICENSE](../../LICENSE) file for details.

---

## 🔗 Useful Links

- **Website**: [https://posty5.com](https://posty5.com)
- **Dashboard**: [studio.posty5.com/account/settings?tab=APIKeys](studio.posty5.com/account/settings?tab=APIKeys)
- **API Documentation**: [https://docs.posty5.com](https://docs.posty5.com)
- **GitHub**: [https://github.com/Posty5/npm-sdk](https://github.com/Posty5/npm-sdk)

---

Made with ❤️ by the Posty5 Team
