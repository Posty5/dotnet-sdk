using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Posty5.Webhooks.Models;

namespace Posty5.Webhooks;

/// <summary>
/// Verifies a Posty5 webhook delivery (Standard Webhooks:
/// <c>base64(HMAC-SHA256(secret, "{webhook-id}.{webhook-timestamp}.{rawBody}"))</c>)
/// and reads its event.
/// </summary>
/// <remarks>
/// Pass the body exactly as received — read it before model binding; a
/// re-serialised object will not match. During a secret rotation the header holds
/// two signatures and either one passes.
/// </remarks>
public static class WebhookSignature
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    /// <summary>Verifies the delivery against the current clock and returns its event.</summary>
    /// <param name="payload">The raw request body.</param>
    /// <param name="headers">The request headers (names are matched case-insensitively).</param>
    /// <param name="secret">The endpoint's <c>whsec_…</c> secret.</param>
    /// <param name="tolerance">Allowed clock distance; default 5 minutes.</param>
    /// <returns>The event, as its concrete type.</returns>
    /// <exception cref="WebhookSignatureException">The delivery is not authentic, is stale, or is not an event.</exception>
    public static WebhookEvent Verify(string payload, IDictionary<string, string> headers, string secret, TimeSpan? tolerance = null)
        => Verify(payload, headers, secret, tolerance, DateTimeOffset.UtcNow);

    /// <summary>As <see cref="Verify(string, IDictionary{string, string}, string, TimeSpan?)"/>, against a given clock.</summary>
    public static WebhookEvent Verify(string payload, IDictionary<string, string> headers, string secret, TimeSpan? tolerance, DateTimeOffset now)
    {
        ArgumentNullException.ThrowIfNull(payload);
        ArgumentNullException.ThrowIfNull(headers);

        var id = Header(headers, WebhookConst.IdHeader);
        var timestamp = Header(headers, WebhookConst.TimestampHeader);
        var signatures = Header(headers, WebhookConst.SignatureHeader);
        if (id == null || timestamp == null || signatures == null)
            throw new WebhookSignatureException(WebhookSignatureFailureReason.MissingHeaders, "The webhook signature headers are missing.");

        var key = DecodeSecret(secret);

        if (!long.TryParse(timestamp, out var seconds))
            throw new WebhookSignatureException(WebhookSignatureFailureReason.TimestampOutOfRange, "The webhook timestamp is not a number.");
        var sentAt = DateTimeOffset.FromUnixTimeSeconds(seconds);
        if ((now - sentAt).Duration() > (tolerance ?? WebhookConst.DefaultTolerance))
            throw new WebhookSignatureException(WebhookSignatureFailureReason.TimestampOutOfRange, "The webhook timestamp is outside the tolerance.");

        var expected = Sign(key, id, timestamp, payload);
        if (!AnyMatches(signatures, expected))
            throw new WebhookSignatureException(WebhookSignatureFailureReason.NoMatchingSignature, "No webhook signature matches.");

        try
        {
            return JsonSerializer.Deserialize<WebhookEvent>(payload, JsonOptions)
                ?? throw new WebhookSignatureException(WebhookSignatureFailureReason.InvalidJson, "The webhook body is empty.");
        }
        catch (JsonException ex)
        {
            throw new WebhookSignatureException(WebhookSignatureFailureReason.InvalidJson, "The webhook body is not a valid event.", ex);
        }
    }

    /// <summary>
    /// Computes the <c>v1,…</c> signature of a delivery — for tests of your own
    /// receiver. Posty5 signs with the same function.
    /// </summary>
    public static string ComputeSignature(string secret, string id, string timestamp, string payload)
        => $"{WebhookConst.SignatureVersion},{Convert.ToBase64String(Sign(DecodeSecret(secret), id, timestamp, payload))}";

    private static byte[] Sign(byte[] key, string id, string timestamp, string payload)
    {
        using var hmac = new HMACSHA256(key);
        return hmac.ComputeHash(Encoding.UTF8.GetBytes($"{id}.{timestamp}.{payload}"));
    }

    private static bool AnyMatches(string header, byte[] expected)
    {
        var matched = false;
        foreach (var entry in header.Split(' ', StringSplitOptions.RemoveEmptyEntries))
        {
            var comma = entry.IndexOf(',');
            if (comma <= 0 || entry[..comma] != WebhookConst.SignatureVersion) continue;

            byte[] given;
            try { given = Convert.FromBase64String(entry[(comma + 1)..]); }
            catch (FormatException) { continue; }

            // Check every entry so the time taken does not say which one matched.
            matched |= CryptographicOperations.FixedTimeEquals(given, expected);
        }
        return matched;
    }

    private static byte[] DecodeSecret(string secret)
    {
        if (string.IsNullOrWhiteSpace(secret))
            throw new WebhookSignatureException(WebhookSignatureFailureReason.MissingHeaders, "The webhook secret is empty.");

        var raw = secret.StartsWith(WebhookConst.SecretPrefix, StringComparison.Ordinal)
            ? secret[WebhookConst.SecretPrefix.Length..]
            : secret;
        try
        {
            return Convert.FromBase64String(raw);
        }
        catch (FormatException ex)
        {
            throw new WebhookSignatureException(WebhookSignatureFailureReason.MissingHeaders, "The webhook secret is not a whsec_ secret.", ex);
        }
    }

    private static string? Header(IDictionary<string, string> headers, string name)
    {
        foreach (var (key, value) in headers)
            if (string.Equals(key, name, StringComparison.OrdinalIgnoreCase))
                return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        return null;
    }
}
