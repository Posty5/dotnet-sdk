namespace Posty5.Core.Configuration;

/// <summary>
/// Header names and values every request through <see cref="Http.Posty5HttpClient"/>
/// carries. Declared once here so the transport, the clients and the tests read
/// the same spelling.
/// </summary>
public static class Posty5HttpDefaults
{
    /// <summary>The header the API key travels in.</summary>
    public const string ApiKeyHeader = "X-API-Key";

    /// <summary>
    /// The header naming the SDK and its version, for the API's logs. It is a
    /// label, not a credential: the API records it and trusts nothing because of it.
    /// </summary>
    public const string ClientHeader = "X-Posty5-Client";

    /// <summary>This SDK's name in <see cref="ClientHeader"/>: <c>posty5-dotnet/&lt;version&gt;</c>.</summary>
    public const string ClientName = "posty5-dotnet";

    /// <summary>The media type every request asks for.</summary>
    public const string AcceptJson = "application/json";
}
