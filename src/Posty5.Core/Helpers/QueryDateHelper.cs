using System.Globalization;
using Posty5.Core.Configuration;

namespace Posty5.Core.Helpers;

/// <summary>
/// Formats the dates the SDK sends in a query string, so every client sends
/// the same <c>yyyy-MM-dd</c> whatever the current culture.
/// </summary>
public static class QueryDateHelper
{
    /// <summary>The date part only, culture-independent: no time-zone conversion.</summary>
    /// <param name="value">The date to send.</param>
    /// <returns>The date as <c>yyyy-MM-dd</c>.</returns>
    public static string FormatDate(DateTime value) =>
        value.ToString(QueryDateConst.DateFormat, CultureInfo.InvariantCulture);

    /// <summary>
    /// Adds the list endpoints' created-at window (<c>fromDate</c> /
    /// <c>toDate</c>, <c>yyyy-MM-dd</c>) to <paramref name="queryParams"/>,
    /// each key only when its date is set.
    /// </summary>
    /// <remarks>
    /// The pair is not enforced here: the API applies the window only when both
    /// are sent (one alone filters nothing) and rounds it to whole UTC days.
    /// </remarks>
    /// <param name="queryParams">The query being built.</param>
    /// <param name="fromDate">Created on or after this day.</param>
    /// <param name="toDate">Created on or before this day.</param>
    public static void AddCreatedAtRange(IDictionary<string, object?> queryParams, DateTime? fromDate, DateTime? toDate)
    {
        ArgumentNullException.ThrowIfNull(queryParams);
        if (fromDate.HasValue)
            queryParams[QueryDateConst.FromDateKey] = FormatDate(fromDate.Value);
        if (toDate.HasValue)
            queryParams[QueryDateConst.ToDateKey] = FormatDate(toDate.Value);
    }
}
