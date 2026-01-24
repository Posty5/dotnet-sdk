# Posty5.HtmlHostingFormSubmission

Track, manage, and process form submissions from your Posty5-hosted HTML pages with the .NET SDK. This package provides a client for managing form submissions with features like status tracking, Google Sheets integration, navigation, and comprehensive filtering.

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

`Posty5.HtmlHostingFormSubmission` is a **specialized tool package** for managing form submissions captured from HTML pages hosted on the Posty5 platform. It enables developers to build powerful form management systems with status workflows and data export capabilities.

### Key Capabilities

- **📋 Form Submission Tracking** - Automatically capture and store form submissions from hosted HTML pages
- **🔄 Status Management** - Track submission lifecycle with 11 customizable status types
- **📊 Status History** - Complete audit trail of all status changes with timestamps and notes
- **📱 Google Sheets Integration** - Auto-sync form data to Google Sheets for analysis
- **🔍 Advanced Filtering** - Search and filter submissions by page, form, status, and custom fields
- **⬅️➡️ Navigation** - Easily navigate between previous and next submissions
- **📈 Visitor Tracking** - Link submissions to visitor sessions for behavior analysis
- **🔐 API Key Filtering** - Scope resources by API key for multi-tenant applications
- **🔢 Automatic Numbering** - Each submission gets a unique sequential number

### Role in the Posty5 Ecosystem

This package works seamlessly with other Posty5 SDK packages:

- Use `Posty5.HtmlHosting` to create and manage HTML pages with forms
- Use `Posty5.HtmlHostingVariables` to inject dynamic data into forms
- Combine with analytics tools to track conversion rates and form performance

---

## 📥 Installation

Install via NuGet Package Manager:

```bash
dotnet add package Posty5.HtmlHostingFormSubmission
```

Or via Package Manager Console:

```powershell
Install-Package Posty5.HtmlHostingFormSubmission
```

---

## 🚀 Quick Start

Here's a minimal example to get you started:

```csharp
using Posty5.Core.Configuration;
using Posty5.Core.Http;
using Posty5.HtmlHostingFormSubmission;
using Posty5.HtmlHostingFormSubmission.Models;

// Initialize the HTTP client with your API key
var options = new Posty5Options
{
    ApiKey = "your-api-key" // Get from https://studio.posty5.com/account/settings?tab=APIKeys
};
var httpClient = new Posty5HttpClient(options);

// Create the Form Submission client
var formSubmissions = new HtmlHostingFormSubmissionClient(httpClient);

// List all submissions for a specific HTML page
var submissions = await formSubmissions.ListAsync(
    new ListFormSubmissionsParams { HtmlHostingId = "your-html-page-id" },
    new PaginationParams { PageNumber = 1, PageSize = 20 }
);

Console.WriteLine($"Found {submissions.Pagination.TotalItems} submissions");
foreach (var submission in submissions.Data)
{
    Console.WriteLine($"#{submission.Numbering}: {submission.Status}");
    // Form data is a Dictionary<string, object>
    if (submission.Data.TryGetValue("email", out var email))
    {
        Console.WriteLine($"Email: {email}");
    }
}

// Get a specific submission
var detail = await formSubmissions.GetAsync("submission-id-123");
Console.WriteLine($"Submission Status: {detail.Status}");

// Get navigation
var nav = await formSubmissions.GetNextPreviousAsync("submission-id-123");
if (nav.Next != null)
{
    Console.WriteLine($"Next Submission ID: {nav.Next.Id}");
}
```

---

## 📚 API Reference & Examples

### Retrieving Form Submissions

#### GetAsync

Retrieve complete details of a specific form submission by ID.

**Parameters:**

- `id` (string): The unique submission ID

**Returns:** `Task<FormSubmissionModel>` - Complete submission details

**Example:**

```csharp
var submission = await formSubmissions.GetAsync("submission-id-123");

// Access form data
Console.WriteLine($"Status: {submission.Status}");
foreach (var field in submission.Data)
{
    Console.WriteLine($"{field.Key}: {field.Value}");
}

// Review status history
foreach (var history in submission.StatusHistory)
{
    Console.WriteLine($"{history.Status} - {history.ChangedAt}");
    if (!string.IsNullOrEmpty(history.Notes))
    {
        Console.WriteLine($"  Notes: {history.Notes}");
    }
}
```

---

#### ListAsync

Search and filter form submissions with advanced pagination and filtering options.

**Parameters:**

- `listParams` (ListFormSubmissionsParams): Filter criteria
  - `HtmlHostingId` (string, **required**): HTML page ID to get submissions from
  - `FormId` (string?): Filter by specific form ID
  - `Numbering` (string?): Search by submission number
  - `Status` (string?): Filter by status
  - `FilteredFields` (string?): Comma-separated field names to enable search on
- `pagination` (PaginationParams?, optional): Pagination options
  - `PageNumber` (int): Page number (default: 1)
  - `PageSize` (int): Items per page (default: 10)

**Returns:** `Task<PaginationResponse<FormSubmissionModel>>`

**Example:**

```csharp
// Get all submissions for an HTML page
var allSubmissions = await formSubmissions.ListAsync(
    new ListFormSubmissionsParams { HtmlHostingId = "html-page-123" },
    new PaginationParams { PageNumber = 1, PageSize = 50 }
);

Console.WriteLine($"Total: {allSubmissions.Pagination.TotalItems}");
foreach (var sub in allSubmissions.Data)
{
     Console.WriteLine($"#{sub.Numbering}: {sub.Status}");
}
```

```csharp
// Filter by status - get only new submissions
var newSubmissions = await formSubmissions.ListAsync(new ListFormSubmissionsParams
{
    HtmlHostingId = "html-page-123",
    Status = "New"
});
```

```csharp
// Filter by specific form on a page
var formSubmissionsList = await formSubmissions.ListAsync(new ListFormSubmissionsParams
{
    HtmlHostingId = "html-page-123",
    FormId = "contact-form"
});
```

---

#### GetNextPreviousAsync

Get references to the next and previous submissions for easy navigation.

**Parameters:**

- `id` (string): Current submission ID

**Returns:** `Task<NextPreviousSubmissionsResponse>`

- `Previous` (SubmissionRef?): Previous submission reference
- `Next` (SubmissionRef?): Next submission reference

**Example:**

```csharp
var nav = await formSubmissions.GetNextPreviousAsync("submission-id-123");

if (nav.Previous != null)
{
    Console.WriteLine($"← Previous: #{nav.Previous.Numbering} (ID: {nav.Previous.Id})");
}

if (nav.Next != null)
{
    Console.WriteLine($"→ Next: #{nav.Next.Numbering} (ID: {nav.Next.Id})");
}
```

---

### Managing Submissions

#### DeleteAsync

Permanently delete a form submission.

**Parameters:**

- `id` (string): Submission ID to delete

**Returns:** `Task`

**Example:**

```csharp
await formSubmissions.DeleteAsync("submission-id-123");
Console.WriteLine("Submission deleted");
```

---

## 🔒 Error Handling

All methods may throw exceptions from `Posty5.Core.Exceptions`.

```csharp
using Posty5.Core.Exceptions;

try
{
    var submission = await formSubmissions.GetAsync("invalid-id");
}
catch (Posty5AuthenticationException ex)
{
    Console.WriteLine("Invalid API key");
}
catch (Posty5NotFoundException ex)
{
    Console.WriteLine("Submission not found");
}
catch (Posty5Exception ex)
{
    Console.WriteLine($"Error: {ex.Message}");
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
| [Posty5.SocialPublisherTask](../Posty5.SocialPublisherTask) | Social publishing task client | 1.0.0 | [📦 NuGet](https://www.nuget.org/packages/Posty5.SocialPublisherTask) |

---

## 📄 License

MIT License - see [LICENSE](../../LICENSE) file for details.

---

Made with ❤️ by the Posty5 team
