# 18 - Decisions

## D01 - All projects target net8.0.

**Status:** observed in current source. Revisit only with compatibility, migration, and verification impact documented.

## D02 - Feature libraries depend on Posty5.Core for transport and common models.

**Status:** observed in current source. Revisit only with compatibility, migration, and verification impact documented.

## D03 - Each feature is a separate NuGet package.

**Status:** observed in current source. Revisit only with compatibility, migration, and verification impact documented.

## D04 - Public network methods follow Task/Async and cancellation patterns.

**Status:** observed in current source. Revisit only with compatibility, migration, and verification impact documented.

## D05 - ROUTE_INDEX is empty because this repository is a client SDK.

**Status:** observed in current source. Revisit only with compatibility, migration, and verification impact documented.

## D06 - `Posty5Options.DefaultHeaders` cannot carry `X-API-Key`.

**Status:** decided 2026-10-05 (mcp-server / sdk-agent-gaps). The
`Posty5HttpClient` constructor throws `ArgumentException` for an `X-API-Key`
entry (any case, trimmed) instead of letting a header silently decide which key
a request uses; `ApiKey` and `SetApiKey` are the only ways to set it. An
`X-Posty5-Client` entry is allowed and replaces the SDK's own label (a wrapper
may identify itself; the API logs the label and trusts nothing because of it).
Content headers are refused because they belong to a body, not to every request.

## D07 - The SDK does not retry.

**Status:** observed and kept on purpose (2026-10-05). `Posty5HttpClient` has
never retried — Polly is referenced but unused and `MaxRetries` /
`RetryDelayMilliseconds` are inert — so a POST is never sent twice. The npm SDK
had to remove POST retries (mcp-server decision 13); here there was nothing to
remove. Adding retries needs a written idempotency story first.

## D08 - `createdFrom` defaults stay as they are; `Posty5Options.CreatedFrom` overrides.

**Status:** decided 2026-10-05. The tool clients keep `dotnetPackage` and store
orders keep `dotnet` (`CreatedFromDefaults`); normalising the two spellings is a
server-side follow-up of the mcp-server feature. A request model that carries its
own `CreatedFrom` (store orders, text posts, stories) wins over the option.

## D09 - New packages are versioned with the Core they require.

**Status:** observed (Posty5.Store was born at 3.0.0 with Core 3.0.0) and
followed: Posty5.Account was born at 3.1.0 with Core 3.1.0. Feature packages are
otherwise versioned independently (SocialPublisherPost is on 4.x).

Do not invent historical rationale. Record evidence-based current decisions and label unknown rationale explicitly.
