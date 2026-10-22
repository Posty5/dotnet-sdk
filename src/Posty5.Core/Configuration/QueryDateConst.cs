namespace Posty5.Core.Configuration;

/// <summary>
/// Wire format and keys of the dates the SDK sends as query parameters, used
/// by <see cref="Helpers.QueryDateHelper"/>.
/// </summary>
internal static class QueryDateConst
{
    /// <summary>A query date: an ISO date, no time, invariant culture.</summary>
    public const string DateFormat = "yyyy-MM-dd";

    /// <summary>Created-at range start of the list endpoints.</summary>
    public const string FromDateKey = "fromDate";

    /// <summary>Created-at range end of the list endpoints.</summary>
    public const string ToDateKey = "toDate";
}
