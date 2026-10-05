using Posty5.Account;
using Posty5.Account.Models;
using Posty5.Core.Models;
using Xunit;

namespace Posty5.Tests.Account;

/// <summary>
/// Offline: every <see cref="AccountClient"/> route, verb and query, and the
/// response shapes the API sends, against a <see cref="RecordingServer"/>.
/// </summary>
public class AccountClientRouteTests : IDisposable
{
    private readonly RecordingServer _server = new();

    public void Dispose() => _server.Dispose();

    private AccountClient Client() => new(_server.Http());

    private string Single() => _server.Requests.Select(r => $"{r.Method} {r.PathAndQuery}").Single();

    [Fact]
    public async Task GetCurrentAsync_ReadsTheKeyOwnerPlanCreditsAndMcp()
    {
        _server.ResultJson = """
        {
          "apiKey": { "_id": "k1", "name": "CI", "recordScope": "account", "createdAt": "2026-10-01T00:00:00Z", "lastUsedAt": null },
          "user": { "_id": "u1", "userName": "ahmed", "fullName": "Ahmed", "isDeveloper": true },
          "plan": { "key": "pro", "name": "Pro", "startedAt": "2026-09-01T00:00:00Z", "endedAt": "2026-10-31T00:00:00Z" },
          "credits": { "granted": 1000, "purchased": 200, "used": 150, "debt": 25, "remaining": 1050, "spendable": 1025,
                       "periodStartedAt": "2026-10-01T00:00:00Z", "periodEndsAt": "2026-10-31T00:00:00Z" },
          "mcp": { "access": "write", "toolsets": ["links", "social"] }
        }
        """;

        var me = await Client().GetCurrentAsync();

        Assert.Equal("GET /api/api-key/current", Single());
        Assert.Equal("k1", me.ApiKey.Id);
        Assert.Equal(ApiKeyRecordScopes.Account, me.ApiKey.RecordScope);
        Assert.Null(me.ApiKey.LastUsedAt);
        Assert.Equal("u1", me.User.Id);
        Assert.True(me.User.IsDeveloper);
        Assert.Equal("pro", me.Plan!.Key);
        Assert.Equal(1025m, me.Credits.Spendable);
        Assert.Equal(25m, me.Credits.Debt);
        Assert.Equal(McpAccessLevels.Write, me.Mcp!.Access);
        Assert.Equal(new[] { "links", "social" }, me.Mcp.Toolsets);
    }

    [Fact]
    public async Task GetCurrentAsync_ToleratesNoPlanAndNoMcp()
    {
        _server.ResultJson = """
        { "apiKey": { "_id": "k1", "recordScope": "key" }, "user": { "_id": "u1", "isDeveloper": false },
          "plan": null, "credits": { "granted": 0, "used": 0, "debt": 0, "remaining": 0, "spendable": 0 }, "mcp": null }
        """;

        var me = await Client().GetCurrentAsync();

        Assert.Null(me.Plan);
        Assert.Null(me.Mcp);
        Assert.Equal(ApiKeyRecordScopes.Key, me.ApiKey.RecordScope);
    }

    [Fact]
    public async Task GetCreditsAsync_ReadsTheBalanceAndCounters()
    {
        _server.ResultJson = """
        { "credits": { "granted": 100, "used": 40, "debt": 0, "remaining": 60, "spendable": 60 },
          "usageStats": [ { "featurePath": "socialMediaPublisher.textPost", "count": 4, "creditsSpent": 40 } ] }
        """;

        var wallet = await Client().GetCreditsAsync();

        Assert.Equal("GET /api/user/current/credits", Single());
        Assert.Equal(60m, wallet.Credits.Spendable);
        var stat = Assert.Single(wallet.UsageStats);
        Assert.Equal("socialMediaPublisher.textPost", stat.FeaturePath);
        Assert.Equal(4, stat.Count);
    }

    [Fact]
    public async Task GetCreditUsageAsync_SendsEveryFilterAndThePage()
    {
        _server.ResultJson = "{\"items\":[],\"pagination\":{\"nextCursor\":null,\"hasMore\":false,\"pageSize\":25}}";

        await Client().GetCreditUsageAsync(
            new CreditUsageListParams
            {
                Module = "socialMediaPublisher",
                OperationType = "textPost",
                Kind = CreditUsageKinds.Charge,
                Settled = false,
                StoreId = "s1",
                FromDate = "2026-10-01",
                ToDate = "2026-10-31",
            },
            new PaginationParams { Cursor = "c1", PageSize = 25 });

        Assert.Equal(
            "GET /api/user/current/credit-usage?module=socialMediaPublisher&operationType=textPost&kind=charge&settled=false&storeId=s1&fromDate=2026-10-01&toDate=2026-10-31&cursor=c1&pageSize=25",
            Single());
    }

    [Fact]
    public async Task GetCreditUsageAsync_WithNoArguments_SendsNoQuery()
    {
        _server.ResultJson = "{\"items\":[],\"pagination\":{}}";

        await Client().GetCreditUsageAsync();

        Assert.Equal("GET /api/user/current/credit-usage", Single());
    }

    [Fact]
    public async Task GetCreditUsageAsync_ReadsALedgerRowWithItsReferenceAndActor()
    {
        _server.ResultJson = """
        { "items": [ {
            "_id": "r1", "kind": "charge", "direction": "out", "signedCredits": "-25", "module": "onlineStore",
            "operationType": "orderReceived", "reason": "Order ORD-0042", "credits": 25, "balanceBefore": 100, "balanceAfter": 75,
            "settled": true, "reference": { "type": "storeOrder", "id": "o42", "label": "ORD-0042" },
            "actorUserId": { "_id": "u2", "userName": "staff", "fullName": "Staff Member" },
            "createdAt": "2026-10-04T10:00:00Z" } ],
          "pagination": { "nextCursor": "c2", "previousCursor": null, "hasMore": true, "pageSize": 1 } }
        """;

        var page = await Client().GetCreditUsageAsync(pagination: new PaginationParams { PageSize = 1 });

        var row = Assert.Single(page.Items);
        Assert.Equal(CreditDirections.Out, row.Direction);
        Assert.Equal(25m, row.Credits);
        Assert.Equal("storeOrder", row.Reference!.Type);
        Assert.Equal("ORD-0042", row.Reference.Label);
        Assert.Equal("staff", row.Actor!.UserName);
        Assert.Equal("c2", page.Pagination.NextCursor);
        Assert.True(page.Pagination.HasMore);
    }

    [Fact]
    public async Task GetCreditUsageSummaryAsync_SendsTheFiltersButNotTheStore_AndReadsTheNestedTotals()
    {
        _server.ResultJson = """
        { "balance": 60, "remaining": 60, "debt": 0, "granted": 100, "purchased": 0, "used": 40,
          "remainingBreakdown": { "monthly": 60, "purchased": 0 },
          "periodStartedAt": "2026-10-01T00:00:00Z", "periodEndsAt": "2026-10-31T00:00:00Z",
          "range": { "operations": 4, "creditsSpent": 40, "creditsOwed": 0, "creditsAdded": 100 } }
        """;

        var summary = await Client().GetCreditUsageSummaryAsync(new CreditUsageListParams { Kind = CreditUsageKinds.Charge, Settled = true, StoreId = "s1" });

        Assert.Equal("GET /api/user/current/credit-usage/summary?kind=charge&settled=true", Single());
        Assert.Equal(60m, summary.Balance);
        Assert.Equal(4, summary.Range.Operations);
        Assert.Equal(40m, summary.Range.CreditsSpent);
        Assert.Equal(60m, summary.RemainingBreakdown!.Monthly);
    }

    [Fact]
    public async Task GetOperationCostsAsync_SendsActiveOnlyOnlyWhenOptingOut()
    {
        _server.ResultJson = """
        { "currency": "credits", "operationsCount": 1, "modules": [ { "module": "socialMediaPublisher", "moduleName": "Social Media Publisher",
            "operations": [ { "operationType": "textPost", "featurePath": "socialMediaPublisher.textPost", "name": "Text post",
                              "description": "A status update", "cost": 5, "isFree": false, "charged": true, "allowDeferred": false,
                              "payerType": "user", "enabled": true } ] } ] }
        """;

        var catalog = await Client().GetOperationCostsAsync();
        await Client().GetOperationCostsAsync(activeOnly: false);

        Assert.Equal(new[]
        {
            "GET /api/plans/operation-costs",
            "GET /api/plans/operation-costs?activeOnly=false",
        }, _server.Requests.Select(r => $"{r.Method} {r.PathAndQuery}"));

        Assert.Equal("credits", catalog.Currency);
        var op = Assert.Single(Assert.Single(catalog.Modules).Operations);
        Assert.Equal("socialMediaPublisher.textPost", op.FeaturePath);
        Assert.Equal(5m, op.Cost);
        Assert.True(op.Charged);
    }

    [Fact]
    public void Constructor_RefusesANullClient()
    {
        Assert.Throws<ArgumentNullException>(() => new AccountClient(null!));
    }
}

/// <summary>
/// Live and read-only: skipped unless <c>POSTY5_API_KEY</c> is set
/// (<see cref="ApiKeyFactAttribute"/>). Needs an API with the mcp-server
/// wave-1 routes.
/// </summary>
[Collection("Sequential")]
public class AccountClientLiveTests
{
    private readonly AccountClient _account = new(TestConfig.CreateHttpClient());

    [ApiKeyFact]
    public async Task GetCurrentAsync_DescribesTheKey()
    {
        var me = await _account.GetCurrentAsync();

        Assert.False(string.IsNullOrEmpty(me.ApiKey.Id));
        Assert.False(string.IsNullOrEmpty(me.User.Id));
        Assert.Contains(me.ApiKey.RecordScope, new[] { ApiKeyRecordScopes.Key, ApiKeyRecordScopes.Account });
        Assert.True(me.Credits.Spendable >= 0);
    }

    [ApiKeyFact]
    public async Task GetCreditsAndUsage_Read()
    {
        var wallet = await _account.GetCreditsAsync();
        Assert.True(wallet.Credits.Spendable >= 0);

        var page = await _account.GetCreditUsageAsync(pagination: new PaginationParams { PageSize = 5 });
        Assert.True(page.Items.Count <= 5);

        var summary = await _account.GetCreditUsageSummaryAsync();
        Assert.True(summary.Range.Operations >= 0);
    }

    [ApiKeyFact]
    public async Task GetOperationCostsAsync_ListsPricedOperations()
    {
        var catalog = await _account.GetOperationCostsAsync();

        Assert.NotEmpty(catalog.Modules);
        Assert.All(catalog.Modules.SelectMany(m => m.Operations), op => Assert.True(op.Cost >= 0));
    }
}
