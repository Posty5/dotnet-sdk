# Posty5.HtmlHostingVariables

Manage dynamic variables for your Posty5-hosted HTML pages with the .NET SDK. This package provides a client for creating, updating, and managing key-value variables that can be used across all your hosted HTML content for dynamic content injection and configuration management.

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

`Posty5.HtmlHostingVariables` is a **specialized tool package** for managing dynamic variables that can be injected into HTML pages hosted on the Posty5 platform. It enables developers to build content management systems with centralized configuration and dynamic content replacement.

### Key Capabilities

- **🔑 Key-Value Storage** - Store dynamic values (API keys, configuration, content) as reusable variables
- **🔄 Real-Time Updates** - Modify variables instantly via API; changes reflect immediately on hosted pages
- **🎯 Variable Injection** - Use variables in HTML pages via `{{variable_key}}` syntax for dynamic content
- **🏷️ Tag & Reference Support** - Organize variables with custom tags and reference IDs
- **🔍 Advanced Filtering** - Search and filter variables by name, key, value, tag, or reference ID
- **⚡ Prefix Validation** - Automatic enforcement of `pst5_` prefix for namespace consistency
- **📝 CRUD Operations** - Complete create, read, update, delete operations for variable management
- **🎨 Use Cases** - Perfect for configuration management, A/B testing, feature flags, multi-language content

### Role in the Posty5 Ecosystem

This package works seamlessly with other Posty5 SDK packages:

- Use `Posty5.HtmlHosting` to create HTML pages that reference variables
- Combine with configuration management systems for centralized settings
- Build dynamic landing pages, promotional banners, or multi-tenant applications

---

## 📥 Installation

Install via NuGet Package Manager:

```bash
dotnet add package Posty5.HtmlHostingVariables
```

Or via Package Manager Console:

```powershell
Install-Package Posty5.HtmlHostingVariables
```

---

## 🚀 Quick Start

Here's a minimal example to get you started:

```csharp
using Posty5.Core.Configuration;
using Posty5.Core.Http;
using Posty5.HtmlHostingVariables;
using Posty5.HtmlHostingVariables.Models;

// Initialize the HTTP client with your API key
var options = new Posty5Options
{
    ApiKey = "your-api-key" // Get from https://studio.posty5.com/account/settings?tab=APIKeys
};
var httpClient = new Posty5HttpClient(options);

// Create the Variables client
var variables = new HtmlHostingVariablesClient(httpClient);

// Create a new variable
await variables.CreateAsync(new CreateHtmlHostingVariableRequest
{
    Name = "Company Name", // Human-readable name
    Key = "pst5_company_name", // Key used in HTML (must start with pst5_)
    Value = "Acme Corporation" // The actual value
});

// List all variables
var allVariables = await variables.ListAsync(
    null,
    new PaginationParams { PageNumber = 1, PageSize = 10 }
);

Console.WriteLine($"Total variables: {allVariables.Pagination.TotalItems}");
foreach (var variable in allVariables.Data)
{
    Console.WriteLine($"{variable.Key} = {variable.Value}");
}

// Get a specific variable
var companyName = await variables.GetAsync("variable-id-123");
Console.WriteLine($"Company: {companyName.Value}");

// Update variable value
await variables.UpdateAsync("variable-id-123", new CreateHtmlHostingVariableRequest
{
    Name = "Company Name",
    Key = "pst5_company_name",
    Value = "Acme Corp (Updated)",
    Tag = "",
    RefId = ""
});

// Use in HTML page: {{pst5_company_name}} will be replaced with "Acme Corp (Updated)"
```

---

## 📚 API Reference & Examples

### Creating Variables

#### CreateAsync

Create a new HTML hosting variable with a name, key, and value. The key must start with `pst5_` prefix for namespace consistency.

**Parameters:**

- `data` (CreateHtmlHostingVariableRequest): Variable data
  - `Name` (string, **required**): Human-readable variable name
  - `Key` (string, **required**): Variable key for HTML injection (must start with `pst5_`)
  - `Value` (string, **required**): Variable value
  - `Tag` (string?, optional): Custom tag for grouping/filtering
  - `RefId` (string?, optional): External reference ID from your system

**Returns:** `Task`

**Example:**

```csharp
// Basic variable creation
await variables.CreateAsync(new CreateHtmlHostingVariableRequest
{
    Name = "Support Email",
    Key = "pst5_support_email",
    Value = "support@acme.com"
});
```

```csharp
// Variable with tag and reference ID
await variables.CreateAsync(new CreateHtmlHostingVariableRequest
{
    Name = "Production API URL",
    Key = "pst5_api_url",
    Value = "https://api.acme.com",
    Tag = "production", // Group by environment
    RefId = "env-prod-001" // Your system's identifier
});
```

**Important:** Keys must start with `pst5_`. If you provide a key without this prefix, an `ArgumentException` will be thrown.

---

### Retrieving Variables

#### GetAsync

Retrieve complete details of a specific variable by ID.

**Parameters:**

- `id` (string): The unique variable ID

**Returns:** `Task<HtmlHostingVariableModel>` - Variable details

**Example:**

```csharp
var variable = await variables.GetAsync("variable-id-123");

Console.WriteLine("Variable Details:");
Console.WriteLine($"  Name: {variable.Name}");
Console.WriteLine($"  Key: {variable.Key}");
Console.WriteLine($"  Value: {variable.Value}");
Console.WriteLine($"  Created: {variable.CreatedAt}");
```

---

#### ListAsync

Search and filter variables with advanced pagination and filtering options.

**Parameters:**

- `listParams` (ListHtmlHostingVariablesParams?, optional): Filter criteria
  - `Name` (string?): Filter by variable name
  - `Key` (string?): Filter by variable key
  - `Value` (string?): Filter by variable value
  - `Tag` (string?): Filter by tag
  - `RefId` (string?): Filter by reference ID
- `pagination` (PaginationParams?, optional): Pagination options
  - `PageNumber` (int): Page number (default: 1)
  - `PageSize` (int): Items per page (default: 10)

**Returns:** `Task<PaginationResponse<HtmlHostingVariableModel>>`

**Example:**

```csharp
// Get all variables
var allVariables = await variables.ListAsync(
    null,
    new PaginationParams { PageNumber = 1, PageSize = 50 }
);

Console.WriteLine($"Total: {allVariables.Pagination.TotalItems}");
foreach (var variable in allVariables.Data)
{
    Console.WriteLine($"{variable.Name} ({variable.Key}) = {variable.Value}");
}
```

```csharp
// Filter by tag
var prodVars = await variables.ListAsync(new ListHtmlHostingVariablesParams
{
    Tag = "production"
});
```

---

### Managing Variables

#### UpdateAsync

Update an existing variable's name, key, or value. The key must still start with `pst5_` prefix.

**Parameters:**

- `id` (string): Variable ID to update
- `data` (CreateHtmlHostingVariableRequest): Updated variable data
  - `Name` (string): Updated variable name
  - `Key` (string): Updated variable key (must start with `pst5_`)
  - `Value` (string): Updated variable value
  - `Tag` (string?, optional): Updated tag
  - `RefId` (string?, optional): Updated reference ID

**Returns:** `Task`

**Example:**

```csharp
// Update variable value
await variables.UpdateAsync("variable-id-123", new CreateHtmlHostingVariableRequest
{
    Name = "Support Email",
    Key = "pst5_support_email",
    Value = "help@acme.com", // Changed from support@acme.com
    Tag = "",
    RefId = ""
});
```

---

#### DeleteAsync

Permanently delete a variable. Once deleted, the variable key will no longer be replaced in HTML pages.

**Parameters:**

- `id` (string): Variable ID to delete

**Returns:** `Task`

**Example:**

```csharp
await variables.DeleteAsync("variable-id-123");
Console.WriteLine("Variable deleted");
```

---

## 🔒 Error Handling

All methods may throw exceptions from `Posty5.Core.Exceptions` or `System.ArgumentException` (for invalid keys).

```csharp
using Posty5.Core.Exceptions;

try
{
    await variables.CreateAsync(new CreateHtmlHostingVariableRequest
    {
        Name = "Test Variable",
        Key = "invalid_key", // Missing pst5_ prefix
        Value = "test"
    });
}
catch (ArgumentException ex) {
    Console.WriteLine($"Invalid argument: {ex.Message}");
    // Key must start with 'pst5_', change to pst5_invalid_key
}
catch (Posty5AuthenticationException ex)
{
    Console.WriteLine("Invalid API key");
}
catch (Posty5Exception ex)
{
    Console.WriteLine($"API Error: {ex.Message}");
}
```

---

## 📖 Resources

- **Official Guides**: [https://guide.posty5.com](https://guide.posty5.com)
- **API Reference**: [https://docs.posty5.com](https://docs.posty5.com)
- **Source Code**: [https://github.com/Posty5/dotnet-sdk](https://github.com/Posty5/dotnet-sdk)

---

## 📦 Packages

This SDK ecosystem contains the following tool packages:

| Package | Description | Version | NuGet |
| --- | --- | --- | --- |
| [Posty5.Core](../Posty5.Core) | Core HTTP client and models | 1.0.0 | [📦 NuGet](https://www.nuget.org/packages/Posty5.Core) |
| [Posty5.ShortLink](../Posty5.ShortLink) | URL shortener client | 1.0.0 | [📦 NuGet](https://www.nuget.org/packages/Posty5.ShortLink) |
| [Posty5.QRCode](../Posty5.QRCode) | QR code generator client | 1.0.0 | [📦 NuGet](https://www.nuget.org/packages/Posty5.QRCode) |
| [Posty5.HtmlHosting](../Posty5.HtmlHosting) | HTML hosting client | 1.0.0 | [📦 NuGet](https://www.nuget.org/packages/Posty5.HtmlHosting) |
| [Posty5.HtmlHostingVariables](../Posty5.HtmlHostingVariables) | Variable management | 1.0.0 | [📦 NuGet](https://www.nuget.org/packages/Posty5.HtmlHostingVariables) |
| [Posty5.HtmlHostingFormSubmission](../Posty5.HtmlHostingFormSubmission) | Form submission management | 1.0.0 | [📦 NuGet](https://www.nuget.org/packages/Posty5.HtmlHostingFormSubmission) |
| [Posty5.SocialPublisherWorkspace](../Posty5.SocialPublisherWorkspace) | Social workspace management | 1.0.0 | [📦 NuGet](https://www.nuget.org/packages/Posty5.SocialPublisherWorkspace) |
| [Posty5.SocialPublisherPost](../Posty5.SocialPublisherPost) | Social publishing post client | 1.0.0 | [📦 NuGet](https://www.nuget.org/packages/Posty5.SocialPublisherPost) |

---

## 🆘 Support

We're here to help you succeed with Posty5!

### Get Help

- **Documentation**: [https://guide.posty5.com](https://guide.posty5.com)
- **Contact Us**: [https://posty5.com/contact-us](https://posty5.com/contact-us)
- **GitHub Issues**: [Report bugs or request features](https://github.com/Posty5/npm-sdk/issues)
- **API Status**: Check API status and uptime at [https://status.posty5.com](https://status.posty5.com)

### Common Issues

1. **Authentication Errors**
   - Ensure your API key is valid and active
   - Get your API key from [studio.posty5.com/account/settings?tab=APIKeys](studio.posty5.com/account/settings?tab=APIKeys)

2. **Network Errors**
   - Check your internet connection
   - Verify firewall settings allow connections to `api.posty5.com`

3. **Rate Limiting**
   - The SDK includes automatic retry logic
   - Check your API plan limits in the dashboard

---

## 📄 License

MIT License - see [LICENSE](../../LICENSE) file for details.

---

Made with ❤️ by the Posty5 team
