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

## D10 - A field the API never accepted is obsoleted and ignored, not deleted, in a minor (TP-D5).

**Status:** decided 2026-10-05 (link-qr truth pass, `.agent/tasks/link-qr-truth-pass/plan.md` TP-D5). `IsEnableMonetization` on the short-link and QR models is `[Obsolete]` + `[JsonIgnore]`, and the clients build explicit payloads so it never reaches the wire (the API's Joi rejected it with a 400). Old code keeps compiling with a CS0618 warning and stops failing; the property is deleted in the 4.0.0 tool majors. Same pattern for any later "never accepted" field.

## D11 - List filters the API does not read are obsoleted and no longer sent.

**Status:** decided 2026-10-05 (link-qr truth pass). `ShortLinkListParamsModel.Search`, `FromDate`, `ToDate` were sent but the API's short-link search reads none of them (`api/.../short-link/module.ts` search filter lists). They are `[Obsolete]` and dropped from the query rather than kept as silent no-ops; adding real filters is an API change, not an SDK one.

## D12 - Fields the API requires for API-key callers use the C# `required` modifier.

**Status:** decided 2026-10-05 (link-qr truth pass, TP-D7). `TemplateId` (short-link and QR request models) and short-link `BaseUrl` are `required`, so a request without them fails to compile - the .NET form of the npm SDK's non-optional property and the feature's acceptance criterion 6. Cost: a caller that assigns them after construction must move them into the object initializer.

## D13 - Link/QR analytics models live in Posty5.Core; the answer keeps the API's strings.

**Status:** decided 2026-10-05 (link-qr visit analytics, `.agent/tasks/link-qr-visit-analytics/dotnet-sdk/link-qr-analytics-methods/plan.md`). The C2 answer is identical for short links and QR codes, so `LinkAnalyticsQuery` / `LinkAnalyticsModel` and `LinkAnalyticsQueryHelper` are declared once in `Posty5.Core` (3.2.0; 3.1.0 is the MCP release, which merges first), like the npm SDK's `@posty5/core` interfaces. The query uses the `StringValueObjectConverter` value types (`LinkAnalyticsInterval`, `LinkAnalyticsBreakdown`); the answer keeps `Meta.*`, `Locked[].Breakdown` and `Series[].Date` as strings, because the converter throws on an unknown value and a later API release may add one, and a `DateTime` would invite a time-zone shift of a day bucket. `AllBreakdowns` with a non-empty `Breakdown` throws `ArgumentException` rather than silently picking one.

## D14 - The analytics feature-lock 403 uses the core's existing mapping.

**Status:** decided 2026-10-05 (VA, C4). A 403 already surfaces as `Posty5Exception` with `StatusCode == 403` and the API body (its `message`) in `ResponseBody`; `Exception.Message` is the generic status text. VA does not change `Posty5HttpClient` error mapping, because that would change every client's 403 and `feat/mcp-wave-2` is editing the same file. Surfacing the API's `message` as `Exception.Message` for every status is a separate Core change.

## D15 - Per-request headers are overloads on Posty5HttpClient; bulk mechanics live once in Core.

**Status:** decided 2026-10-06 (BW, C7). `Idempotency-Key` needs per-request headers and `versioned-writes-major` had not added them, so BW defines them: `PostAsync`/`PutAsync`/`DeleteAsync`/`GetBytesAsync` gain an overload taking `IDictionary<string,string>? headers` (the old signatures forward to it, so existing callers and positional `CancellationToken` arguments are unchanged). `X-API-Key` is refused per request as it is in `DefaultHeaders`. The concurrency task reuses this overload for `If-Match`. The chunking/retry/job code is one `LinkBulkOperations` class in Core and the shared answer/job models are in `Posty5.Core.Models.LinkBulkModels`, because short links and QR codes use the same routes and shapes; only row types live per package.

## D16 - Versioned writes replace the old update/delete signatures; no `[Obsolete]` overloads are kept.

**Status:** decided 2026-10-06 (optimistic concurrency, `.agent/tasks/optimistic-concurrency/dotnet-sdk/versioned-writes-major/plan.md`). Every update, delete, state-change and bulk method takes the document's version (`long version` before `CancellationToken`, before any optional parameter) and the SDK sends it as `If-Match: "<v>"`. This **deviates from the CHANGELOG's "obsolete for one major" practice**: an old overload kept with `[Obsolete]` would compile but fail at runtime with 428 once the API enforces versions (rollout phase D), and a method that always fails is worse than a compile error. The tool majors are 4.0.0 (`Posty5.SocialPublisherPost` 5.0.0); `Posty5.Account` has no writes and gets a dependency-only bump (3.2.0).

## D17 - No retry policy for versioned writes.

**Status:** decided 2026-10-06 (optimistic concurrency). Polly is referenced but no policy is wired, and none is added for writes carrying `If-Match`: a retried versioned write either applies twice or conflicts with itself. `Posty5Options.MaxRetries` / `RetryDelayMilliseconds` stay unused for them. A `Posty5ConflictException` is surfaced to the caller, who re-reads and reapplies.

## D18 - Concurrency mechanics live once in Posty5.Core.

**Status:** decided 2026-10-06 (optimistic concurrency). `Posty5HttpClient` gains `PutAsync`/`PatchAsync`/`PostAsync`/`DeleteAsync` overloads taking `long version`, routed through a private `SendVersionedAsync` that sets `If-Match` on the request's own `HttpRequestMessage` (never `DefaultRequestHeaders`, which is shared across concurrent calls), and `SendBulkVersionedAsync` for the bulk `versions` body. `ApiResponse<T>` gains `Version`, `Versions`, `Code`; the new version (envelope `version`, else `ETag`) is copied onto any `IVersioned` result. `HandleErrorResponse` parses `code`: 409 + `VERSION_CONFLICT` raises `Posty5ConflictException` (`CurrentVersion`, `ResourceId`), any other 409 stays generic (the tus re-sync in `ResumableUpload.cs` uses its own client and is unaffected), 428 raises `Posty5VersionRequiredException`. `X-Posty5-Concurrency: missing-version` logs one warning through `Posty5Options.Logger` (new dependency: `Microsoft.Extensions.Logging.Abstractions` 8.0.0). The client header (`X-Posty5-Client: posty5-dotnet/<version>`) was already added by D15's predecessor and is reused. Which route is versioned follows the API's classification tables in `.agent/tasks/optimistic-concurrency/api/*/plan.md`; exempt routes (creates, reorder, job triggers, tag assign/unassign) keep their old signatures.

Do not invent historical rationale. Record evidence-based current decisions and label unknown rationale explicitly.
