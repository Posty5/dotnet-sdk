namespace Posty5.Store;

/// <summary>
/// Routes <see cref="StoreClient"/> calls itself (the sub-clients keep their own).
/// </summary>
internal static class StoreRoutes
{
    /// <summary>The stores the caller owns or is staff on, by name or slug.</summary>
    public const string Lookup = "/api/store/lookup";
}
