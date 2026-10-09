# Posty5.Account

Account client for the [Posty5](https://posty5.com) .NET SDK — **who an API key
belongs to, what its owner can spend, where the credits went, and what every
operation costs**.

## Install

```bash
dotnet add package Posty5.Account
dotnet add package Posty5.Core
```

## Authenticate

Create an [API key](https://studio.posty5.com) and pass it to the core
`Posty5HttpClient` (sent as the `X-API-Key` header). Every method describes the
owner of that key, except `GetOperationCostsAsync`, which is public.

```csharp
using Posty5.Account;
using Posty5.Account.Models;
using Posty5.Core.Configuration;
using Posty5.Core.Http;
using Posty5.Core.Models;

var http = new Posty5HttpClient(new Posty5Options { ApiKey = Environment.GetEnvironmentVariable("POSTY5_API_KEY") });
var account = new AccountClient(http);
```

Nothing in this package is charged, and nothing in it changes anything.

## Who am I

```csharp
var me = await account.GetCurrentAsync();

Console.WriteLine($"{me.User.UserName} ({me.Plan?.Name ?? "no plan"})");
Console.WriteLine($"Spendable: {me.Credits.Spendable} credits");
Console.WriteLine($"Key '{me.ApiKey.Name}' sees {(me.ApiKey.RecordScope == ApiKeyRecordScopes.Account ? "every record of its owner" : "only the records it created")}");
```

Use it to validate a key before doing anything else: an unknown or revoked key
throws `Posty5AuthenticationException`. `Mcp` is set when the key has an MCP
connection (its access level and toolsets). The response carries no email,
phone or address.

## Credits

```csharp
var wallet = await account.GetCreditsAsync();          // balance + per-feature counters

var page = await account.GetCreditUsageAsync(
    new CreditUsageListParams { Kind = CreditUsageKinds.Charge, Settled = false },   // deferred charges still owed
    new PaginationParams { PageSize = 50 });

var summary = await account.GetCreditUsageSummaryAsync(
    new CreditUsageListParams { FromDate = "2026-10-01", ToDate = "2026-10-31" });
Console.WriteLine($"{summary.Range.Operations} charges, {summary.Range.CreditsSpent} credits spent");
```

The history lists every row the owner pays for — including operations a
store's staff performed (`CreditUsageEntry.Actor`). `FromDate` and `ToDate`
(`yyyy-MM-dd`) only apply together. `StoreId` narrows the history; the summary
does not filter by store.

## Prices

```csharp
var catalog = await account.GetOperationCostsAsync();
var textPost = catalog.Modules
    .SelectMany(m => m.Operations)
    .First(o => o.FeaturePath == "socialMediaPublisher.textPost");
Console.WriteLine($"{textPost.Name}: {textPost.Cost} {catalog.Currency}");
```

Prices change — read them here rather than hard-coding them. An operation costs
the same whether it is called from this SDK, the dashboard or an MCP assistant.

## API

| Method | Endpoint |
| --- | --- |
| `GetCurrentAsync()` | `GET /api/api-key/current` |
| `GetCreditsAsync()` | `GET /api/user/current/credits` |
| `GetCreditUsageAsync(filters?, pagination?)` | `GET /api/user/current/credit-usage` |
| `GetCreditUsageSummaryAsync(filters?)` | `GET /api/user/current/credit-usage/summary` |
| `GetOperationCostsAsync(activeOnly = true)` | `GET /api/plans/operation-costs` (public) |

Every method takes an optional `CancellationToken`.

## Pagination

`GetCreditUsageAsync` pages by opaque cursor, like every other list in the SDK:

```csharp
string? cursor = null;
do
{
    var page = await account.GetCreditUsageAsync(null, new PaginationParams { Cursor = cursor, PageSize = 100 });
    Handle(page.Items);
    cursor = page.Pagination.NextCursor;
} while (cursor != null);
```

## License

MIT
