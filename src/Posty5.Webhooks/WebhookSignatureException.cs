namespace Posty5.Webhooks;

/// <summary>Why a delivery failed verification.</summary>
public enum WebhookSignatureFailureReason
{
    /// <summary><c>webhook-id</c>, <c>webhook-timestamp</c> or <c>webhook-signature</c> is missing, or the secret is malformed.</summary>
    MissingHeaders,
    /// <summary>The timestamp is unreadable or further from now than the tolerance (a replay or a skewed clock).</summary>
    TimestampOutOfRange,
    /// <summary>No <c>v1</c> signature matches the body — wrong secret or a changed body.</summary>
    NoMatchingSignature,
    /// <summary>The signature is valid but the body is not a webhook event.</summary>
    InvalidJson
}

/// <summary>Thrown by <see cref="WebhookSignature.Verify(string, IDictionary{string, string}, string, TimeSpan?)"/>; answer 400 and do not process the request.</summary>
public class WebhookSignatureException : Exception
{
    /// <summary>Why verification failed.</summary>
    public WebhookSignatureFailureReason Reason { get; }

    /// <summary>Creates the exception.</summary>
    public WebhookSignatureException(WebhookSignatureFailureReason reason, string message, Exception? inner = null)
        : base(message, inner)
    {
        Reason = reason;
    }
}
