# Posty5.HtmlHostingVariables

HTML Hosting Variables management client for Posty5 .NET SDK. This package allows you to create, manage, and organize environment variables for your hosted HTML pages.

## Features

- 🔑 **Variable Management**: Create, read, update, and delete variables
- ✅ **Key Validation**: Automatic validation of variable key prefixes
- 🔍 **Advanced Filtering**: Search and filter variables by multiple criteria
- 🏷️ **Tagging Support**: Organize variables with custom tags
- 📊 **Pagination**: Efficient pagination for large variable lists
- 🔗 **Reference Tracking**: Link variables to external systems with RefId

## Installation

```bash
dotnet add package Posty5.HtmlHostingVariables
```

## Quick Start

```csharp
using Posty5.Core;
using Posty5.HtmlHostingVariables;

// Initialize the client
var config = new Posty5Config("your-api-key");
var httpClient = new Posty5HttpClient(config);
var client = new HtmlHostingVariablesClient(httpClient);

// Create a variable
await client.CreateAsync(new CreateHtmlHostingVariableRequest
{
    Name = "API Key",
    Key = "pst5_api_key",      // MUST start with pst5_
    Value = "sk_test_123456",
    Tag = "production"
});
```

## Important: Key Prefix Requirement

⚠️ **All variable keys MUST start with `pst5_`**

This prefix is enforced by the SDK and the API for security and namespacing purposes.

```csharp
// ✅ CORRECT - Key starts with pst5_
Key = "pst5_api_key"
Key = "pst5_database_url"
Key = "pst5_stripe_secret"

// ❌ WRONG - Missing pst5_ prefix (will throw ArgumentException)
Key = "api_key"
Key = "database_url"
```

## Usage Examples

### Creating Variables

#### Basic Variable Creation

```csharp
await client.CreateAsync(new CreateHtmlHostingVariableRequest
{
    Name = "API Key",
    Key = "pst5_api_key",
    Value = "sk_test_123456"
});
```

#### With Tags and RefId

```csharp
await client.CreateAsync(new CreateHtmlHostingVariableRequest
{
    Name = "Database URL",
    Key = "pst5_database_url",
    Value = "postgresql://user:pass@host:5432/db",
    Tag = "production",
    RefId = "env-prod-001"
});
```

#### Handling Key Validation Errors

```csharp
try
{
    await client.CreateAsync(new CreateHtmlHostingVariableRequest
    {
        Name = "Bad Key Example",
        Key = "api_key",  // ❌ Missing pst5_ prefix
        Value = "value"
    });
}
catch (ArgumentException ex)
{
    // Error message: "Key must start with 'pst5_', change to pst5_api_key"
    Console.WriteLine(ex.Message);
}
```

### Retrieving Variables

#### Get Single Variable

```csharp
var variable = await client.GetAsync("variable_id_123");

Console.WriteLine($"Name: {variable.Name}");
Console.WriteLine($"Key: {variable.Key}");
Console.WriteLine($"Value: {variable.Value}");
Console.WriteLine($"Tag: {variable.Tag}");
Console.WriteLine($"Created: {variable.CreatedAt}");
```

#### List All Variables

```csharp
var result = await client.ListAsync(
    pagination: new PaginationParams
    {
        Page = 1,
        PageSize = 20
    }
);

Console.WriteLine($"Total Variables: {result.Total}");
foreach (var variable in result.Items)
{
    Console.WriteLine($"{variable.Key}: {variable.Value}");
}
```

### Filtering Variables

#### Filter by Tag

```csharp
var prodVariables = await client.ListAsync(
    new ListHtmlHostingVariablesParams
    {
        Tag = "production"
    }
);
```

#### Filter by Key Pattern

```csharp
var apiVariables = await client.ListAsync(
    new ListHtmlHostingVariablesParams
    {
        Key = "pst5_api"  // Partial match
    }
);
```

#### Filter by Name

```csharp
var dbVariables = await client.ListAsync(
    new ListHtmlHostingVariablesParams
    {
        Name = "Database"  // Partial match
    }
);
```

#### Multiple Filters

```csharp
var result = await client.ListAsync(
    new ListHtmlHostingVariablesParams
    {
        Tag = "production",
        Key = "pst5_",
        RefId = "env-prod"
    },
    new PaginationParams { Page = 1, PageSize = 10 }
);
```

### Updating Variables

```csharp
await client.UpdateAsync("variable_id_123", new CreateHtmlHostingVariableRequest
{
    Name = "Updated API Key",
    Key = "pst5_api_key",        // Must still have pst5_ prefix
    Value = "sk_live_new_value",
    Tag = "production-updated"
});
```

### Deleting Variables

```csharp
await client.DeleteAsync("variable_id_123");
Console.WriteLine("Variable deleted successfully");
```

## Common Use Cases

### Environment Configuration

Manage different environment configurations:

```csharp
// Development environment
await client.CreateAsync(new CreateHtmlHostingVariableRequest
{
    Name = "API Base URL",
    Key = "pst5_api_base_url",
    Value = "https://api-dev.example.com",
    Tag = "development"
});

// Production environment
await client.CreateAsync(new CreateHtmlHostingVariableRequest
{
    Name = "API Base URL",
    Key = "pst5_api_base_url",
    Value = "https://api.example.com",
    Tag = "production"
});
```

### API Keys Management

Store and manage API keys for third-party services:

```csharp
var apiKeys = new[]
{
    new { Name = "Stripe API Key", Key = "pst5_stripe_key", Value = "sk_test_..." },
    new { Name = "SendGrid API Key", Key = "pst5_sendgrid_key", Value = "SG...." },
    new { Name = "Google Maps API Key", Key = "pst5_google_maps_key", Value = "AIza..." }
};

foreach (var apiKey in apiKeys)
{
    await client.CreateAsync(new CreateHtmlHostingVariableRequest
    {
        Name = apiKey.Name,
        Key = apiKey.Key,
        Value = apiKey.Value,
        Tag = "api-keys"
    });
}
```

### Feature Flags

Manage feature flags:

```csharp
await client.CreateAsync(new CreateHtmlHostingVariableRequest
{
    Name = "Enable New UI",
    Key = "pst5_feature_new_ui",
    Value = "true",
    Tag = "feature-flags"
});
```

### Database Credentials

Store database connection strings:

```csharp
await client.CreateAsync(new CreateHtmlHostingVariableRequest
{
    Name = "PostgreSQL Connection",
    Key = "pst5_db_connection",
    Value = "postgresql://user:pass@localhost:5432/mydb",
    Tag = "database",
    RefId = "db-main-001"
});
```

## Pagination

Handle large variable lists with pagination:

```csharp
int currentPage = 1;
int pageSize = 50;
int totalProcessed = 0;

while (true)
{
    var result = await client.ListAsync(
        pagination: new PaginationParams
        {
            Page = currentPage,
            PageSize = pageSize
        }
    );

    foreach (var variable in result.Items)
    {
        Console.WriteLine($"Processing: {variable.Key}");
        totalProcessed++;
    }

    if (result.Items.Count < pageSize || totalProcessed >= result.Total)
    {
        break;
    }

    currentPage++;
}

Console.WriteLine($"Processed {totalProcessed} variables");
```

## API Reference

### Methods

| Method | Description | Returns |
|--------|-------------|---------|
| `CreateAsync()` | Create variable with key validation | `Task` |
| `GetAsync()` | Get variable by ID | `HtmlHostingVariableModel` |
| `UpdateAsync()` | Update variable with key validation | `Task` |
| `DeleteAsync()` | Delete variable | `Task` |
| `ListAsync()` | List/search variables | `PaginationResponse<HtmlHostingVariableModel>` |

### Models

**`HtmlHostingVariableModel`** - Full variable details
- `Id` - MongoDB document ID
- `Name` - Human-readable name
- `Key` - Variable key (must start with pst5_)
- `Value` - Variable value
- `UserId` - Owner user ID
- `Tag` - Custom tag for filtering
- `RefId` - External reference ID
- `CreatedAt` - Creation timestamp
- `UpdatedAt` - Last update timestamp

**`CreateHtmlHostingVariableRequest`** - Create/Update request
- `Name` - Variable name (required)
- `Key` - Variable key (required, must start with pst5_)
- `Value` - Variable value (required)
- `Tag` - Custom tag (optional)
- `RefId` - External reference ID (optional)

**`ListHtmlHostingVariablesParams`** - Filter parameters
- `Name` - Filter by name
- `Key` - Filter by key
- `Value` - Filter by value
- `Tag` - Filter by tag
- `RefId` - Filter by reference ID

## Error Handling

### Key Validation Errors

```csharp
try
{
    await client.CreateAsync(new CreateHtmlHostingVariableRequest
    {
        Name = "Test",
        Key = "invalid_key",  // Missing pst5_ prefix
        Value = "value"
    });
}
catch (ArgumentException ex)
{
    Console.WriteLine($"Validation error: {ex.Message}");
    // Output: "Key must start with 'pst5_', change to pst5_invalid_key"
}
```

### Not Found Errors

```csharp
try
{
    var variable = await client.GetAsync("non_existent_id");
}
catch (InvalidOperationException ex)
{
    Console.WriteLine($"Error: {ex.Message}");
    // Output: "Variable not found"
}
```

### Network Errors

```csharp
try
{
    await client.ListAsync();
}
catch (HttpRequestException ex)
{
    Console.WriteLine($"Network error: {ex.Message}");
}
```

## Best Practices

### 1. Use Meaningful Key Names

```csharp
// ✅ Good
Key = "pst5_stripe_api_key"
Key = "pst5_database_connection_string"
Key = "pst5_feature_enable_analytics"

// ❌ Avoid
Key = "pst5_var1"
Key = "pst5_x"
```

### 2. Organize with Tags

```csharp
// Group related variables
Tag = "production"
Tag = "api-keys"
Tag = "feature-flags"
Tag = "database"
```

### 3. Use RefId for External Systems

```csharp
// Link to your deployment system
RefId = "deployment-prod-v1.2.3"

// Link to your config management
RefId = "config-group-frontend"
```

### 4. Implement Proper Error Handling

```csharp
public async Task<bool> CreateVariableSafe(string name, string key, string value)
{
    try
    {
        await client.CreateAsync(new CreateHtmlHostingVariableRequest
        {
            Name = name,
            Key = key.StartsWith("pst5_") ? key : $"pst5_{key}",
            Value = value
        });
        return true;
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Failed to create variable: {ex.Message}");
        return false;
    }
}
```

## Security Considerations

- **Never commit API keys**: Use environment variables or secure key vaults
- **Rotate credentials regularly**: Update variable values periodically
- **Use tags for access control**: Organize sensitive variables with specific tags
- **Audit variable access**: Track who creates and modifies variables using RefId

## Related Packages

- **Posty5.Core** - Core functionality and HTTP client
- **Posty5.HtmlHosting** - HTML page hosting management
- **Posty5.ShortLink** - Short link management
- **Posty5.QRCode** - QR code generation and management

## Support

- **Documentation**: [https://posty5.com/docs](https://posty5.com/docs)
- **API Reference**: [https://posty5.com/api](https://posty5.com/api)
- **GitHub**: [https://github.com/posty5/dotnet-sdk](https://github.com/posty5/dotnet-sdk)

## License

MIT License - see LICENSE file for details
