# 12 - Testing and Debugging

- **Never run `dotnet test` without the user's approval** (workspace `AI_RULES.md` §0): ask in chat which suite and why; one yes covers one run. `dotnet build` is not a test suite.
- The shared xUnit project references all SDK projects.
- Several tests require POSTY5_API_KEY and POSTY5_BASE_URL and exercise live endpoints/media. Live tests never default to production: with no `POSTY5_BASE_URL` they aim at `http://127.0.0.1:1` (closed), and `[ApiKeyFact]` and the other live facts skip; `POSTY5_TEST_ALLOW_PRODUCTION=true` targets `https://api.posty5.com` on purpose. The `Posty5.Tests.Integration` classes are **not** guarded: with a key set and no reachable API, they fail (connection refused) rather than skip.
- Run build even when live credentials are unavailable; report skipped integration verification explicitly.
- Package versions are not fully uniform, so release work must audit every csproj.

## Debugging order

1. Reproduce with the smallest owning module or route/API call.
2. Inspect the exact entrypoint and boundary contract.
3. Check configuration names without printing values.
4. Run the narrow check, then the project build/typecheck.
5. Record any check that could not run and why.

## Offline route tests (RecordingServer)

`tests/Posty5.Tests/RecordingServer.cs` points the real `Posty5HttpClient` at a
local `HttpListener` that records each request — verb, path+query, body and
headers — and answers with the API envelope (`ResultJson`, `Status`), so every
route is pinned without the network. `_server.Http(options?)` builds a client
on it. Used by `CoreHeadersAndOriginTests`
(X-Posty5-Client, DefaultHeaders, createdFrom), `AccountClientRouteTests`,
`SocialPublisherPostRouteTests` (status `/{id}/status`, remove, text, story,
comments list), `SocialPublisherAccountClientRouteTests` and
`QRCodeTemplateClientRouteTests`.

Live read-only facts for those clients use `[ApiKeyFact]`
(`tests/Posty5.Tests/ApiKeyFactAttribute.cs`): skipped, naming the variable,
when `POSTY5_API_KEY` is not set. They need an API with the mcp-server wave-1
routes (`/api/api-key/current`, …).

Run only the offline classes (after approval):
`dotnet test tests/Posty5.Tests/Posty5.Tests.csproj --filter "FullyQualifiedName~RouteTests|FullyQualifiedName~CoreHeadersAndOriginTests"`.

Fact variables are read from the User scope, then the process
(`TestConfig.Env`). `POSTY5_TEST_ALLOW_CHARGES=true` (with
`POSTY5_BULK_TEMPLATE_ID`) lets `[BulkChargeFact]` create, and be charged for,
records.

## Short-link and QR payload tests (link-qr truth pass)

`ShortLinkClientPayloadTests` and `QRCodeClientPayloadTests` (in
`ShortLinkClientTests.cs` / `QRCodeClientTests.cs`) use the shared
`RecordingServer` to pin what goes on the wire: no `isEnableMonetization` ever,
every create field, omitted-when-null update keys, the `pageInfo.title` list key,
no client-built `options.text` for structured QR types.

Live facts that need the API's truth-pass release use `[LinkQrTruthPassFact]`
(`tests/Posty5.Tests/LinkQrTruthPassFactAttribute.cs`): skipped unless
`POSTY5_API_KEY` is set and `POSTY5_TEST_LINK_QR_TP=true` says the API under
test carries the release (deep links, landing page kept on update, the
`pageInfo.title` and QR `refId` filters).

## Short-link and QR analytics tests (visit analytics, VA)

`ShortLinkClientPayloadTests` pins `GetAnalyticsAsync` offline: the
`/api/short-link/{id}/analytics` path, every query key (`from`/`to` as
`yyyy-MM-dd` even under `ar-SA`, `breakdown` comma list or `all`, empty list
omitted), `ArgumentException` before sending for `AllBreakdowns` + a list or an
empty id and `ArgumentOutOfRangeException` for a `Limit` outside 1-50, the
missing-record 400 (`Posty5ValidationException`), the full C2 answer (`AnalyticsSampleJson`), and the feature-lock 403
as `Posty5Exception` with `StatusCode == 403`. `QRCodeClientPayloadTests` reuses
the sample and checks the QR path. `GetStatisticsAsync` payload tests (both
files) pin `/statistics`, `period`/`from`/`to`, the argument checks and the
rebuilt answer (`_id` day rows, `...InRange` totals, top rows). `RecordingServer.Message` sets the envelope's
`message` for error answers.

Live facts use `[LinkQrVisitAnalyticsFact]`: skipped unless `POSTY5_API_KEY` is
set and `POSTY5_TEST_LINK_QR_VA=true` (the API under test serves the analytics
endpoints). They create a link / code, then check zeros + `Meta.AnalyticsStartedAt`,
`AllBreakdowns`, an explicit list, and a 400 for an unknown interval.
