namespace Posty5.Core.Configuration;

/// <summary>
/// The <c>createdFrom</c> label this SDK stamps on the records it creates when
/// <see cref="Posty5Options.CreatedFrom"/> is not set.
/// </summary>
public static class CreatedFromDefaults
{
    /// <summary>
    /// Stamped by the tool clients — short links, QR codes, HTML hosting and its
    /// variables, social workspaces and posts.
    /// </summary>
    public const string Package = "dotnetPackage";
}
