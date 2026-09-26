using Xunit;

namespace Posty5.Tests;

/// <summary>
/// A live store fact that is reported as <b>skipped</b>, with the missing
/// variable named, when its fixtures are not set — instead of passing without
/// doing anything. xunit 2 has no dynamic skip; setting <see cref="FactAttribute.Skip"/>
/// at discovery time is the package-free way to get one.
/// </summary>
/// <remarks>
/// Always needs <c>POSTY5_API_KEY</c> and <c>POSTY5_TEST_STORE_ID</c>
/// (<see cref="TestConfig.HasStoreFixture"/>); pass any further variable names
/// the fact needs. A variable ending in <c>_ALLOW_…</c> must equal <c>true</c>.
/// </remarks>
[AttributeUsage(AttributeTargets.Method)]
public sealed class StoreFixtureFactAttribute : FactAttribute
{
    public StoreFixtureFactAttribute(params string[] requiredVariables)
    {
        var missing = new[] { TestConfig.ApiKeyVar, TestConfig.StoreIdVar }
            .Concat(requiredVariables)
            .Distinct()
            .Where(name => !IsSet(name))
            .ToList();

        if (missing.Count > 0)
        {
            Skip = $"Live store fixture not set: {string.Join(", ", missing)}.";
        }
    }

    private static bool IsSet(string name)
    {
        var value = TestConfig.Env(name);
        return name.Contains("_ALLOW_", StringComparison.Ordinal)
            ? string.Equals(value, "true", StringComparison.OrdinalIgnoreCase)
            : value is not null;
    }
}
