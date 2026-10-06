# Changelog

All notable changes to the Posty5 .NET SDK will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/), and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## Posty5.Core, Posty5.ShortLink, Posty5.QRCode, new Posty5.Webhooks - unreleased (bulk + webhooks, BW)

Needs the API's bulk and webhook releases (an older API answers 404). Additive;
rides the unreleased 3.2.0 line, so no further version bump. `Posty5.Webhooks`
starts at 3.2.0 (lockstep).

### Added

- Core: per-request header overloads of `PostAsync`, `PutAsync`, `DeleteAsync`
  and `GetBytesAsync` (`IDictionary<string,string>? headers`; `X-API-Key` refused).
  Existing signatures unchanged. `BulkDefaults`, `LinkBulkModels`
  (`BulkCreateResult`, `LinkBulkJob`, `ExportOptions`, …), `LinkBulkOperations`,
  `CamelCaseEnumConverter<T>`.
- ShortLink / QRCode: `CreateManyAsync` (sequential chunks of ≤ 100,
  `Idempotency-Key: {key}-{chunk}`, one retry with the same key on a network
  error or 5xx, rows renumbered to input position, `IProgress<BulkProgress>`),
  `ExportAsync`, `CreateBulkJobAsync`, `ValidateBulkJobAsync` (dry run),
  `ListBulkJobsAsync`, `GetBulkJobAsync`, `GetBulkJobResultUrlAsync`,
  `CancelBulkJobAsync`, `WaitForBulkJobAsync`. Rows: `ShortLinkBulkRow`,
  `QRCodeBulkRow` (`ForUrl`, `ForWifi`, … factories). Cancelling
  `CreateManyAsync` returns no partial result; chunks already sent stay created.
- `Posty5.Webhooks`: `WebhookEndpointClient` (list/get/create/update/delete,
  rotate secret, send test, deliveries, redeliver, event types) and
  `WebhookSignature.Verify` (Standard Webhooks HMAC-SHA256, fixed-time compare,
  rotation, 5-minute tolerance) returning a typed `WebhookEvent`;
  `WebhookSignatureException.Reason`.

## Posty5.QRCode 3.3.0 (dynamic QR codes)

Needs the API's dynamic QR release and `Posty5.Core` 3.2.0. Additive minor.

### Added

- `QRCodeMode` (`Static`, `Dynamic`; string value object, so an unknown mode
  from a newer API reads back without failing).
- `QRCodeRequestBaseModel.Mode`: sent as `mode` only when set, on all seven
  create and seven update methods. Calls without `Mode` send the same JSON as
  before.
- `CreateWifiAsync` / `UpdateWifiAsync` throw `ArgumentException` before any
  call when `Mode` is `Dynamic` (Wi-Fi cannot be dynamic; the API answers 400).
- `QRCodeModel.Mode` and `QRCodeModel.DynamicSince` (also on
  `QRCodeFullDetailsModel`), and the `QRCodeListParamsModel.Mode` filter.
- Scan rules (dynamic codes, Starter+): `QRCodeAccessModel` (`ActiveFrom`,
  `ExpiresAt`, `MaxVisits`, `FallbackUrl`); `QRCodeRequestBaseModel.Access`
  (sent as `access` only when set; replaces the stored rules as a whole) and
  `ClearAccess` (sends `access: null`); `QRCodeModel.Access` on responses.

## Posty5.ShortLink 3.2.0, Posty5.QRCode 3.2.0, Posty5.Core 3.2.0 - unreleased

Link + QR visit analytics (VA), plus the link + QR truth pass (TP) below.
VA is additive; TP's changes are listed per package at the end of this entry.
`GetAnalyticsAsync` needs the API's visit-analytics release (an older API
answers 404). Core is 3.2.0 because `Posty5.Core` 3.1.0 is the MCP release
(`feat/mcp-wave-2`), which ships first.

### Added

- `ShortLinkClient.GetAnalyticsAsync(id, query?, ct)` -
  `GET /api/short-link/{id}/analytics`, and
  `QRCodeClient.GetAnalyticsAsync(id, query?, ct)` -
  `GET /api/qr-code/{id}/analytics`. Same semantics as the npm SDK's
  `getAnalytics`.
- `Posty5.Core.Models`: `LinkAnalyticsQuery` (`From`/`To` sent as
  `yyyy-MM-dd`, culture-independent; `Interval`; `Tz`; `Breakdown` comma list;
  `AllBreakdowns` sends `breakdown=all`; `Limit` 1-50, default 10, overflow
  as key `other`, missing values as `unknown`), the answer
  `LinkAnalyticsModel` (`Totals`, `Series`, `Breakdowns` keyed by wire name,
  `Meta` with `Locked`, `Timezone`, `Source`, `AnalyticsStartedAt`,
  `MaxHistoryDays`), and the value types `LinkAnalyticsInterval` /
  `LinkAnalyticsBreakdown` (the nine C2 names).
- `ShortLinkClient.GetStatisticsAsync(query?, ct)` -
  `GET /api/short-link/statistics`, and `QRCodeClient.GetStatisticsAsync(query?, ct)` -
  `GET /api/qr-code/statistics`: account-wide counts. Query
  `LinkStatisticsQuery` (`Period` = `today|7d|30d|month|custom`, or a custom
  `From`/`To` as `yyyy-MM-dd`; default the last 30 days). Answer
  `ShortLinkStatisticsModel` / `QRCodeStatisticsModel`: `Range`, and `Data` with
  `Totals` (`TotalLinks`/`TotalQRCodes`, lifetime `TotalVisitors`,
  `AvgVisitorsPerLink`/`AvgVisitorsPerQRCode`, plus `VisitsInRange`,
  `UniqueVisitorsInRange`, `BotVisitsInRange`), `Daily` (UTC days:
  `CreatedCount`, `VisitorsSum` = visits by people that day) and
  `TopLinks`/`TopQRCodes` (top ten by visits in the range, each with
  `VisitsInRange`). With the visit-analytics API, `VisitorsSum` means visits
  per day, no longer visitors of links created that day.
- `Posty5.Core.Models`: `LinkStatisticsQuery`, `LinkStatisticsPeriod`,
  `LinkStatisticsResponse<TData>`, `LinkStatisticsRange`,
  `LinkStatisticsVisitTotals`, `LinkStatisticsDailyRow`.
- `Posty5.Core.Helpers.LinkAnalyticsQueryHelper`: the one query builder both
  clients use, for analytics and statistics. Statistics with `From`/`To` and a
  non-custom `Period`, or `From` after `To`, throws `ArgumentException` before
  sending.

### Behaviour

- `AllBreakdowns` together with a non-empty `Breakdown`, or an empty `id`,
  throws `ArgumentException`, and a `Limit` outside 1-50 throws
  `ArgumentOutOfRangeException`, before any request. An unset or empty
  `Breakdown` omits the parameter, and the API then returns every breakdown
  the plan allows (the same answer as `AllBreakdowns`).
- Bots are excluded from `Visits` (counted in `BotVisits`); `UniqueVisitors`
  over more than one day is the sum of daily uniques; data starts on
  `Meta.AnalyticsStartedAt`.
- A breakdown the plan does not include, named explicitly, or a `From` older
  than the plan's history, is the API's feature-lock 403: `Posty5Exception`
  with `StatusCode == 403` and the API's message (`This feature is not
  available on your current plan.`; `You Have Not Permission` for a record you
  may not read) in `ResponseBody` (the core's existing mapping; no plan names
  in the SDK). `Meta.Locked[].RequiredPlan` is a plan key such as `basic`;
  `Meta.MaxHistoryDays` is `30` on Free, `null` on Starter and up;
  `Meta.Source` is `events`, `rollup` or `mixed`. Reading analytics costs no
  credits.
- A missing short link or QR code answers **400** (`Posty5ValidationException`,
  "The Short Link Is Not Found" / "The QR Code Is Not Found"), not 404.
- `Meta` values and `Series[].Date` are kept as the API's strings, so a value a
  later API adds cannot fail deserialization and no time-zone conversion can
  shift a day.

### Posty5.ShortLink 3.2.0 - truth pass (TP)

Link + QR truth pass (TP): the client sends every field the API accepts and
nothing it rejects. Several fields need the API's truth-pass release; they are
marked "(API TP)" below and an older API answers 400 to a request that sets them.

#### Added

- `AndroidUrl` / `IosUrl` on `ShortLinkCreateRequestModel` and
  `ShortLinkUpdateRequestModel` (API TP, S13). Allowed schemes: `https:`,
  `http:` or an app scheme (`myapp://…`), never `javascript:`/`data:`/
  `vbscript:`/`file:`/`about:`/`blob:`. Create: a value wins, absent falls back
  to the target page's `al:*` meta. Update: a value wins and `""` clears;
  `null` is not sent, so the API re-derives the link when `BaseUrl` changed and
  keeps it otherwise.
- `IsEnableLandingPage` on `ShortLinkCreateRequestModel`.

#### Changed

- **`CreateAsync` sends every field.** 3.1.0 and earlier sent only name, base URL, template
  ID and custom landing ID: `RefId`, `Tag` and `PageInfo` never reached the API.
  It now sends those plus `IsEnableLandingPage`, `AndroidUrl` and `IosUrl`.
- **`UpdateAsync` sends an explicit body**: no `IsEnableMonetization` (a 400 in
  3.1.0 and earlier when set), plus `templateType` and `createdFrom` like `CreateAsync`
  (the API resets an omitted `createdFrom` to `"api"`). `IsEnableLandingPage`
  left `null` is not sent, so the API keeps the stored value (API TP; an older
  API turned the landing page off).
- **`TemplateId` and `BaseUrl` are `required`** on the create and update
  models. The API refuses an API-key call without a template ID, so code that
  left it out never succeeded; code that sets it after construction (instead
  of in an object initializer) has to move it into the initializer.
- `UpdateAsync` throws `ArgumentException` for an empty `BaseUrl` instead of
  sending a request the API rejects.
- `ShortLinkListParamsModel.PageInfoTitle` is sent as `pageInfo.title`; 3.1.0 and earlier
  sent `pageinfo.title`, which matched no field.

#### Deprecated

- `IsEnableMonetization` on every short-link model: `[Obsolete]`, `[JsonIgnore]`,
  never sent and always `null` when read (the API never accepted or returned
  it). Removed in 4.0.0. A `TreatWarningsAsErrors` build sees CS0618 for code
  that still sets it.
- `ShortLinkListParamsModel.Search`, `FromDate`, `ToDate`: the API's short-link
  search has no such filters; `[Obsolete]` and no longer sent.

#### Documentation

- README: real type names (`ShortLinkCreateRequestModel`, cursor
  `PaginationParams`, `Items`), landing page and deep-link sections, an
  "Upgrading to 3.2.0" note; monetization and analytics claims removed.

### Posty5.QRCode 3.2.0 - truth pass (TP)

#### Added

- `IsEnableLandingPage` on `QRCodeRequestBaseModel` (every create and update).

#### Changed

- **The typed create/update methods no longer send `options.text`** for email,
  WiFi, call, SMS, URL and geolocation codes; they send `qrCodeTarget` only and
  the API builds the encoded text. 3.1.0 and earlier built it client-side without escaping,
  so a subject with `&` or a WiFi password with `;` produced a code that opened
  the wrong thing. Free text still sends `options.text` equal to the text.
- **`TemplateId` is `required`** on every request model (it was a non-null
  string defaulting to `""`, which the API refuses for API-key calls).

#### Deprecated

- `IsEnableMonetization` on `QRCodeModel`, `QRCodeRequestBaseModel` and
  `QRCodeListParamsModel`: `[Obsolete]`, `[JsonIgnore]`, never sent (every
  payload carried it in 3.1.0 and earlier). Removed in 4.0.0.

#### Documentation

- README and QUICK_REFERENCE use the real type names; "dynamic QR", "scan
  analytics", "short link" and monetization claims removed; `NumberOfVisitors`
  documented as visits to the code's Posty5 page (scanning a downloaded image is
  not counted). Package description no longer says "dynamic".

## Posty5.SocialPublisherPost 4.6.0 - 2026-10-05

### Fixed

- **`ReschedulePostAsync` was refused by the API.** It sent the create routes'
  `schedule: { type, scheduledAt }` object; the edit route
  (`PUT /api/social-publisher-post/{id}`) takes `scheduleType` + `scheduledAt`
  flat and refuses unknown keys. `ReschedulePostRequest` now carries
  `ScheduleType` and `ScheduledAt` (omitted with "now", which the API requires).

- **`GetStatusAsync` called a route that does not exist.** It sent
  `GET /api/social-publisher-post/{id}`, which the API has never served (only
  `PUT` and `DELETE` live there), so every call failed with
  `Posty5NotFoundException`. It now calls `GET /api/social-publisher-post/{id}/status`,
  as the npm SDK always has. An empty id is refused before any request.
- **The comment list never reached the API on short videos and image posts.**
  `Comments` (added in 4.5.0) was accepted by `PublishShortVideoToWorkspaceAsync`,
  `PublishShortVideoToAccountAsync`, `CreateImagePostToWorkspaceAsync` and
  `CreateImagePostToAccountAsync`, but their request bodies only carried the
  deprecated singular `Comment`, so the list was silently dropped. It is sent now.
  (The long-video methods were not affected.)
- `SocialPublisherPostStatusType` gains `Removing`, `Removed` and `RemoveFailed`.
  Without them, reading a post that had been removed from the platforms threw
  while deserializing.

### Added

- **`RemovePostAsync(id)`** — `POST /api/social-publisher-post/{id}/remove`: take a
  *published* post down from every platform it went to, and read the outcome per
  platform (`RemovePostResult.Results`). Instagram and TikTok cannot delete through
  their APIs and report `NotSupported`. A removal that fails anywhere it could
  have succeeded throws `Posty5ValidationException` and charges nothing.
  `DeletePostAsync` remains the call for a post that has not published yet.
- **Text posts** — `CreateTextPostToWorkspaceAsync` / `CreateTextPostToAccountAsync`
  (`POST /text/workspace|account[/{id}]`): a status update with no media for
  Facebook Pages, Threads and X, with per-platform blocks (`TextPostFacebookConfig`
  with a link preview, `ThreadsTextConfig`, `TwitterConfig` with an optional
  poll), comments, hashtags and tracked links. Returns `CreatePostResult`, which
  lists the platforms skipped (no text surface), refused (X below Pro) and
  truncated (X).
- **Stories** — `CreateStoryPostToWorkspaceAsync` / `CreateStoryPostToAccountAsync`
  (`POST /story/workspace|account`): one image or one video for Facebook Pages and
  Instagram, **media by URL** in this release. Only the fields of the story's
  `Kind` are sent, because the API refuses anything else on a story.
- `CommentRequest.PostToThreads` and `PostToTwitter`.
- `PostStatusFullDetailsResponse.CreatedFrom`, `AgentOrigin` (the AI assistant
  that created the post through MCP, when one did) and `StoryExpiresAt`.
- `createdFrom` follows `Posty5Options.CreatedFrom` (see Posty5.Core 3.1.0); a
  text post or story may also set its own `CreatedFrom`.

### Documentation

- `ReschedulePostAsync` had lost its XML documentation to `DeletePostAsync`; both
  are documented again.

## Posty5.Account 3.1.0 - 2026-10-05

New package. `AccountClient` describes the owner of the API key — useful to
validate a key and to check the balance before a paid call:

| Method | Route |
| --- | --- |
| `GetCurrentAsync()` | `GET /api/api-key/current` — key (with its record scope), owner, plan, credits, MCP settings |
| `GetCreditsAsync()` | `GET /api/user/current/credits` |
| `GetCreditUsageAsync(filters?, pagination?)` | `GET /api/user/current/credit-usage` (cursor-paged) |
| `GetCreditUsageSummaryAsync(filters?)` | `GET /api/user/current/credit-usage/summary` |
| `GetOperationCostsAsync(activeOnly = true)` | `GET /api/plans/operation-costs` (public) |

Versioned with the `Posty5.Core` it requires, as `Posty5.Store` was. Packed by
the publish workflow.

## Posty5.Core 3.1.0 - 2026-10-05

### Added

- **`X-Posty5-Client: posty5-dotnet/<version>`** on every request, the version
  read from the `Posty5.Core` assembly (`Posty5ClientIdentity`). The API logs it
  and trusts nothing because of it.
- **`Posty5Options.DefaultHeaders`** — headers sent on every request. `X-API-Key`
  is refused (the constructor throws `ArgumentException`) rather than let a header
  silently decide which key is used; an `X-Posty5-Client` entry replaces the SDK's
  own label; content headers are refused.
- **`Posty5Options.CreatedFrom`** — the `createdFrom` label stamped on every
  record the clients create. When null, each package keeps the label it has
  always sent (`CreatedFromDefaults.Package` = `dotnetPackage`,
  `CreatedFromDefaults.StoreOrder` = `dotnet`). `Posty5HttpClient.ResolveCreatedFrom`
  is how the clients read it.
- `AgentOrigin` — the shape the API uses to say which AI assistant created a record.

### Unchanged on purpose

- No retries were added. `Posty5HttpClient` has never retried, so a create cannot
  be sent twice; `MaxRetries` and `RetryDelayMilliseconds` remain unused.

## Posty5.Store 3.3.0 - 2026-10-05

### Added

- `StoreClient.ListStoresAsync()` / `LookupStoresAsync(term?)` —
  `GET /api/store/lookup`: the stores the key's owner owns or is staff on, as
  `StoreLookupItem` (`Id`, and `Name` as `<slug> - <name>`). The only store calls
  that take no `storeId`. The API returns one page (its default size, normally 10).

### Changed

- `CreateOrderInput.CreatedFrom` is now `string?` and defaults to null; the
  client fills it in from `Posty5Options.CreatedFrom`, else `"dotnet"` as before.

## Posty5.SocialPublisherWorkspace 3.1.0 - 2026-10-05

### Added

- `SocialPublisherAccountClient` — read the connected social accounts:
  `ListAsync(params?, pagination?)` (`GET /api/social-publisher-account`),
  `LookupAsync(term?, platform?)` (`/lookup`) and `GetAsync(id)` (`/{id}`, with the
  platform profile and the account's default post settings and comments).
  Read-only: connecting an account is an OAuth sign-in in the dashboard.

### Changed

- `createdFrom` on `CreateAsync` follows `Posty5Options.CreatedFrom`.

## Posty5.QRCode 3.1.0 - 2026-10-05

### Added

- `QRCodeTemplateClient` — `ListUserTemplatesAsync(term?, pagination?)`
  (`GET /api/qr-code-template/user-lookup`) and
  `ListPublicTemplatesAsync(term?, schemeType?, pagination?)` (`/public-lookup`):
  the template ids the create methods take as `TemplateId`.

### Changed

- `createdFrom` on every create follows `Posty5Options.CreatedFrom`.

## Posty5.ShortLink 3.1.0, Posty5.HtmlHosting 3.1.0, Posty5.HtmlHostingVariables 3.1.0 - 2026-10-05

### Changed

- `createdFrom` on create follows `Posty5Options.CreatedFrom` (default unchanged:
  `dotnetPackage`). Requires `Posty5.Core` 3.1.0.

## Posty5.Store 3.2.0 - 2026-09-26

The first version of `Posty5.Store` to reach NuGet: earlier versions (up to
3.1.0) were never published.

### Added

- **`store.Suppliers`** — `StoreSuppliersClient`, one method per dropshipping
  route under `/api/store-suppliers`: the supplier catalogue; connecting,
  configuring, testing and disconnecting a supplier (and `StartOAuthAsync` for
  suppliers connected by signing in); browsing, previewing and importing
  supplier products (`ImportSupplierProductsResult.IsQueued` tells a queued
  import from an inline one); product links and sync; the supplier-order queue
  and the part actions `SubmitGroupAsync`, `RetryAsync`, `PayAsync`,
  `CancelAsync`, `FulfilGroupManuallyAsync`. `ListSupplierOrdersAsync` pages by
  cursor like every other list: it takes `SupplierOrderSearchParams` plus
  `PaginationParams` (`Cursor`, `PageSize` — max 100; the API uses 25 when no `PaginationParams` is passed) and
  returns `PaginationResponse<StoreSupplierOrder>`. The page-number
  `PagedItems<T>` an unpublished draft used for it is gone.
- **Models** for suppliers, connections (no credential property anywhere),
  products, imports, links, supplier orders and order parts, and
  **`SupplierConstants`** — string constants for every vocabulary rather than
  enums, so a value added on the server never breaks deserialization.
- **Orders**: `StoreOrder.FulfilmentGroups`, `StoreOrder.SupplierOrders`,
  `StoreOrderSummary.FulfilmentSummary`, and `OrderSearchParams.NeedsAttention`.

### Changed

- CI now packs `Posty5.Store` (it was missing from the pack list), and the test
  project references it.

## Posty5.SocialPublisherPost 4.5.0 - 2026-09-19

### Added

- **Up to five comments per post.** `Comments` (a `List<CommentRequest>`) on
  every create-post request and on every publish helper that took a single
  `comment`, each with its own `DelayMinutes`, its own optional image and its own
  per-platform flags. `CommentLimits` exposes `MaxPerPost`, `MaxLength` and
  `MaxDelayMinutes` so a caller can validate before sending rather than after a
  400.
- **An image per comment**, as `ImageUrl` (any plan, costs us no storage) or
  `ImageStorageKey` (uploaded through the account's
  `default-comments/upload-urls`, plan-gated). They are not interchangeable: the
  key is what tells the API the object is ours, and therefore what its cleanup
  path uses when the post or the comment is deleted. A public URL sent in the key
  field makes the image read as external and it is never cleaned up.
- **Per-comment status.** `CommentStatusInfo` on each platform's `Comments`,
  carrying the order, the text, the delay and the outcome.

### Deprecated

- `CommentRequest? Comment` on every request, and the `comment:` parameter on
  every publish helper, in favour of `Comments`. Both are `[Obsolete]` and will
  compile for one major version. Send one or the other - the API refuses a
  request carrying both. `CommentInfo` on each platform's status stays alongside
  `Comments`, mirroring the first entry.

### Fixed

- **The documented credit cost was wrong.** A comment is **25 credits**, not
  "+1": the API charges `socialMediaPublisher.commentOnPost`, which its plan data
  prices at 25. Corrected on the models, the section banner and the README.

### Documentation

- The comment XML docs now state the two caveats a caller has no UI to learn
  from: **TikTok is not supported** (no public comment-posting endpoint, so a
  comment aimed at it reports `NotSupported` rather than failing), and **an image
  is Facebook only** (Instagram's and YouTube's comment endpoints are text-only).

## Posty5.Store 3.1.0 - 2026-09-01

### Added

- **Package profiles and their assignments** (task12), on `StoreShippingClient`:
  `ListProfilesAsync`, `GetProfileAsync`, `CreateProfileAsync`,
  `UpdateProfileAsync`, `DeleteProfileAsync`, `AddProfileConditionsAsync`,
  `RemoveProfileConditionAsync`, `DownloadProfileTemplateAsync`,
  `ImportProfileConditionsAsync`, `ListAssignmentsAsync`, `AssignProfileAsync`,
  `SetDefaultAssignmentAsync` and `RemoveAssignmentAsync`.
  - A profile prices by the size of the parcel and answers BEFORE the flat
    country/governorate/city chain; where no bracket matches, the flat chain
    still answers, so profiles add to a store's setup rather than replacing it.
  - The most specific tier holding profiles owns the answer outright and is
    never merged with the tiers above — including for a parcel none of its own
    brackets fit.
- **The parcel on the product shipping section** (task12). `ProductShippingInput`
  gains `Weight`, `Length`, `Width`, `Height`, plus `PackageProfileId` /
  `PackageConditionKey` as provenance. Unset is "not measured", and an unmeasured
  parcel fits no bracket at all — a limit the cart cannot be compared against is
  unanswered, not satisfied.
- **Stock policy on the product stock section** (task11). `ProductStockInput`
  and `ProductSummary` gain `SaleBuffer` and `OutOfStockBehavior`, so a caller
  can hold units back from sale and decide what a sold-out product looks like
  without dropping to raw HTTP.
  - `SaleBuffer` is nullable because `null` and `0` are different answers:
    `null` inherits the store's reserve, `0` sells down to the last unit
    regardless of it. A product is out of stock at `stock - buffer <= 0`.
  - `OutOfStockBehavior` is a string (`inherit` / `showUnavailable` / `hide`),
    matching how every other status-like field in this SDK is typed — the server
    may add a behaviour without the SDK being the thing that refuses it.

## [4.4.0] - 2026-09-01

### Added — Posty5.SocialPublisherPost

- **Explicit upload termination (tus Termination extension).** Cancelling still
  leaves the partial upload resumable, which is the right default — in most
  interfaces "pause" and "cancel" are the same gesture, and a user who paused a
  40-minute video does not expect to start over. Where the cancellation really is
  final, two ways to say so:
  - `terminateOnCancel: true` on `PublishLongVideoToWorkspaceAsync` and
    `PublishLongVideoToAccountAsync` sends a `DELETE` for the partial upload when
    the token fires. It sits **after** `cancellationToken` in the parameter list
    on purpose: inserting an optional parameter ahead of an existing one is
    source-breaking for anyone passing the token positionally.
  - `ResumableUpload.TerminateAsync(uploadUrl)` discards an upload URL persisted
    from an earlier run. It never throws — a cleanup that fails is not worth an
    exception, since the server expires abandoned uploads after 24 hours — and
    treats 404/410 as success, because "already gone" is the outcome asked for.

## [4.3.0] - 2026-08-30

### Added — Posty5.SocialPublisherPost

- **Resumable uploads (tus 1.0.0).** A long video transfers in 8MiB chunks to
  the API's resumable endpoint, so a dropped connection costs one chunk rather
  than the whole file. Falls back to the signed PUT when the server does not
  offer the resumable service, or when the stream is not seekable.
  - `ResumableUpload.UploadAsync(...)` and `ResumableUpload.IsSupported(target)`
    are public for callers driving their own uploads.
  - `onUploadUrl` and `resumeFrom` on both long-video publish methods: persist
    the first and pass it to the second to continue an interrupted transfer,
    including across a process restart.
  - `UploadRejectedException` distinguishes a server refusal — expired ticket,
    oversized file — from a transient failure, so callers know retrying is
    pointless.

### Fixed

- **The resumable fields on `UploadUrlInfo` had the wrong names.** 4.2.0
  declared `UploadEndpoint` / `UploadTicket`; the API sends `tusEndpoint`,
  `ticket`, `ticketExpiresAt` and `maxSize`, now mapped as `TusEndpoint`,
  `Ticket`, `TicketExpiresAt` and `MaxSize`. The old properties always
  deserialised to null.

## [4.2.0] - 2026-08-30

### Added — Posty5.SocialPublisherPost

- **Long video posts** (up to 60 minutes):
  - `PublishLongVideoToWorkspaceAsync()` and `PublishLongVideoToAccountAsync()`,
    accepting a `Stream` or a URL string and handling upload + create in one call.
    Both take `IProgress<UploadProgress>` and a `CancellationToken`.
  - `GetLongVideoQuoteAsync(videoUrl)` returns `LongVideoQuoteResponse`: the
    server-measured duration, the exact credit cost, and a per-platform verdict,
    so the price can be shown before the user commits.
  - `PublishLongVideoResult` carries `DurationSeconds`, `CreditUnits`, `Credits`
    and `RefusedTargets` — targets dropped because the video exceeds that
    platform's own limit. The post still publishes to the rest.
  - `LongVideo` static class exposing `MaxDurationSeconds` (3600),
    `CreditUnitSeconds` (300), `CreditUnits()` and `IsDurationAllowed()`.
- `ReschedulePostAsync(id, schedule, caption?)` — move a not-yet-published post
  to another time, or send it now. Costs no credits.
- `PostType` on `GenerateUploadUrlsRequest`. Passing `"longVideo"` makes the
  server check plan gating and affordability BEFORE the upload starts.
- `UploadEndpoint` / `UploadTicket` on `UploadUrlInfo` — the resumable (tus)
  destination, which survives a dropped connection where a single PUT does not.

### Fixed

- **Long uploads no longer time out.** The uploader used a default `HttpClient`,
  whose 100-second timeout an hour of video blows through long before the
  transfer completes. Long-video uploads now run on a client with the timeout
  disabled, bounded by the `CancellationToken` instead. This is a per-call
  change; the short-video upload path is untouched.

### Pricing

- Long video costs **50 credits per started 5 minutes**
  (`units = ceil(durationSeconds / 300)`). A 12-minute video costs 150; so does
  a 15-minute one. The duration is measured server-side and cannot be supplied
  by the caller.
- Requires the `socialMediaPublisher.longVideoPost` plan feature.

### Not included

- `DeletePostAsync()` is **not** in this release. The API has no
  `DELETE /api/social-publisher-post/{id}` route yet. `RemovePostAsync()`
  remains the way to take down media that has already published.

## [1.0.0] - 2026-01-21

### Added

- Initial release of Posty5 .NET SDK
- Posty5.Core package with HTTP client and base classes
- Posty5.QRCode package for QR code management
  - Support for URL, Email, WiFi, Call, SMS, FreeText, and Geolocation QR codes
  - CRUD operations for QR codes
  - List and search functionality
- Posty5.ShortLink package for URL shortening
  - Create custom short links
  - Update and delete short links
  - Track click statistics
- Posty5.HtmlHosting package for HTML page hosting
  - Create and host HTML pages
  - Update hosted content
  - Manage multiple pages
- Posty5.SocialPublisher package for social media management
  - Workspace management
  - Post creation and scheduling
  - Multi-platform publishing support
- Comprehensive error handling with custom exceptions
- Retry logic with Polly for resilient HTTP requests
- Support for .NET 8.0
- XML documentation for all public APIs
- Example code and getting started guide

### Dependencies

- Polly 8.3.0
- Polly.Extensions.Http 3.0.0
- System.Text.Json 8.0.0

## [Unreleased]

### Planned

- Support for file uploads
- Webhook event handling
- Analytics and reporting features
- Async enumerable support for pagination
- Additional QR code customization options
