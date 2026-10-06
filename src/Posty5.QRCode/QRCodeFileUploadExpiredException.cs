using Posty5.Core.Exceptions;

namespace Posty5.QRCode;

/// <summary>
/// The signed upload URL of a <c>file</c> code (valid 60 s) expired before the
/// file was uploaded. Retrying the whole <c>CreateFileAsync</c> / <c>UpdateFileAsync</c>
/// call asks for a fresh URL.
/// </summary>
public class QRCodeFileUploadExpiredException : Posty5Exception
{
    /// <summary>Lifetime of the expired URL, in seconds.</summary>
    public int ExpiresInSeconds { get; }

    /// <summary>Create the exception.</summary>
    public QRCodeFileUploadExpiredException(int expiresInSeconds, Exception innerException)
        : base($"The upload URL expired ({expiresInSeconds} s) before the file was uploaded; please retry.", innerException)
    {
        ExpiresInSeconds = expiresInSeconds;
    }
}
