namespace Posty5.Core.Models;

/// <summary>
/// A document that carries the API's optimistic-concurrency version
/// (<c>__v</c>). Pass <see cref="Version"/> back to the next update or delete
/// of the same document.
/// </summary>
public interface IVersioned
{
    /// <summary>The document's version, mapped from <c>__v</c>. A document stored before versioning reads as 0.</summary>
    long Version { get; set; }
}
