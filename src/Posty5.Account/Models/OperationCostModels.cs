namespace Posty5.Account.Models;

// ============================================================================
// PRICE LIST — GET /api/plans/operation-costs
// Transcribed from the API's /swagger.json (plus the optional fields the
// handler also sends).
// ============================================================================

/// <summary>
/// What every paid operation costs, grouped by module. Prices change: read them
/// here rather than hard-coding them.
/// </summary>
public class OperationCostCatalog
{
    /// <summary>The unit prices are in — always <c>credits</c>.</summary>
    public string? Currency { get; set; }

    /// <summary>How many operations the catalog lists.</summary>
    public int OperationsCount { get; set; }

    /// <summary>The operations, grouped by module and sorted by module name.</summary>
    public List<OperationCostModule> Modules { get; set; } = new();
}

/// <summary>One module's operations.</summary>
public class OperationCostModule
{
    /// <summary>The module key, e.g. <c>socialMediaPublisher</c>.</summary>
    public string? Module { get; set; }

    /// <summary>The module's display name.</summary>
    public string? ModuleName { get; set; }

    /// <summary>The module's operations, sorted by name.</summary>
    public List<OperationCost> Operations { get; set; } = new();
}

/// <summary>The price of one operation.</summary>
public class OperationCost
{
    /// <summary>The operation within its module, e.g. <c>textPost</c>.</summary>
    public string? OperationType { get; set; }

    /// <summary>The full feature path, e.g. <c>socialMediaPublisher.textPost</c>.</summary>
    public string? FeaturePath { get; set; }

    /// <summary>The operation's display name.</summary>
    public string? Name { get; set; }

    /// <summary>What the operation does.</summary>
    public string? Description { get; set; }

    /// <summary>A grouping for display, when the API has one.</summary>
    public string? Category { get; set; }

    /// <summary>The price in credits; <c>0</c> when free.</summary>
    public decimal Cost { get; set; }

    /// <summary>True when <see cref="Cost"/> is 0.</summary>
    public bool IsFree { get; set; }

    /// <summary>True when the operation is enabled and costs something.</summary>
    public bool Charged { get; set; }

    /// <summary>True when the operation may run without balance and be paid later.</summary>
    public bool? AllowDeferred { get; set; }

    /// <summary>Who pays, as the API records it.</summary>
    public string? PayerType { get; set; }

    /// <summary>Whether the operation is enabled.</summary>
    public bool Enabled { get; set; }
}
