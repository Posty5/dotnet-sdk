namespace Posty5.SocialPublisherWorkspace;

/// <summary>
/// The API routes <see cref="SocialPublisherAccountClient"/> calls, in one place
/// rather than inline in the client.
/// </summary>
internal static class SocialPublisherAccountRoutes
{
    /// <summary>List, and the base of every account route.</summary>
    public const string Base = "/api/social-publisher-account";

    /// <summary>Lookup by name, for pickers.</summary>
    public const string Lookup = Base + "/lookup";
}
