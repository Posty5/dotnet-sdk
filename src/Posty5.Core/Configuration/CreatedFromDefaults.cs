namespace Posty5.Core.Configuration;

/// <summary>
/// The <c>createdFrom</c> labels this SDK stamps on the records it creates when
/// <see cref="Posty5Options.CreatedFrom"/> is not set.
/// </summary>
/// <remarks>
/// Two spellings exist for historical reasons and are kept as they are:
/// changing a stored label would split a merchant's existing records into two
/// groups. Normalising them is a separate, server-side follow-up.
/// </remarks>
public static class CreatedFromDefaults
{
    /// <summary>
    /// Stamped by the tool clients — short links, QR codes, HTML hosting and its
    /// variables, social workspaces and posts.
    /// </summary>
    public const string Package = "dotnetPackage";

    /// <summary>Stamped by <c>Posty5.Store</c> on orders created through the SDK.</summary>
    public const string StoreOrder = "dotnet";
}
