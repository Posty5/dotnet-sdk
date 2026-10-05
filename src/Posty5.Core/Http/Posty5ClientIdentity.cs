using System.Reflection;
using Posty5.Core.Configuration;

namespace Posty5.Core.Http;

/// <summary>
/// What this SDK calls itself in <see cref="Posty5HttpDefaults.ClientHeader"/>.
/// </summary>
/// <remarks>
/// The version is read from the <c>Posty5.Core</c> assembly rather than typed a
/// second time, so the header cannot drift from the package that sent it.
/// </remarks>
public static class Posty5ClientIdentity
{
    /// <summary>
    /// The <c>Posty5.Core</c> package version, e.g. <c>3.1.0</c>. A build
    /// suffix (<c>+&lt;commit&gt;</c>) the compiler appends is dropped; a
    /// pre-release tag is kept.
    /// </summary>
    public static string Version { get; } = ReadVersion(typeof(Posty5ClientIdentity).Assembly);

    /// <summary>The default value of the client header: <c>posty5-dotnet/&lt;version&gt;</c>.</summary>
    public static string HeaderValue { get; } = $"{Posty5HttpDefaults.ClientName}/{Version}";

    private static string ReadVersion(Assembly assembly)
    {
        var informational = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
        if (!string.IsNullOrWhiteSpace(informational))
        {
            var plus = informational.IndexOf('+');
            return plus < 0 ? informational : informational[..plus];
        }

        return assembly.GetName().Version?.ToString(3) ?? "0.0.0";
    }
}
