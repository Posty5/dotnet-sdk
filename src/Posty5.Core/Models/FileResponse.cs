namespace Posty5.Core.Models;

/// <summary>
/// A file download — an endpoint that answers with the file itself rather than
/// with the <c>{ message, result }</c> JSON envelope.
/// </summary>
public class FileResponse
{
    /// <summary>The file's bytes.</summary>
    public byte[] Data { get; set; } = Array.Empty<byte>();

    /// <summary>Value of the response's <c>Content-Type</c> header.</summary>
    public string ContentType { get; set; } = "application/octet-stream";

    /// <summary>
    /// Filename taken from the <c>Content-Disposition</c> header, when the
    /// server sent one.
    /// </summary>
    public string? FileName { get; set; }
}
