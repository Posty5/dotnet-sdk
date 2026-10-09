using System.Text.Json;
using System.Text.Json.Serialization;

namespace Posty5.Core.Converts;

/// <summary>
/// Reads and writes an enum as its camelCase name (<c>PartiallySucceeded</c> ↔
/// <c>"partiallySucceeded"</c>), the spelling the API uses for every string enum.
/// </summary>
/// <typeparam name="TEnum">The enum type.</typeparam>
public sealed class CamelCaseEnumConverter<TEnum> : JsonStringEnumConverter<TEnum>
    where TEnum : struct, Enum
{
    /// <summary>Creates the converter with camelCase naming and no integer values.</summary>
    public CamelCaseEnumConverter() : base(JsonNamingPolicy.CamelCase, allowIntegerValues: false)
    {
    }
}
