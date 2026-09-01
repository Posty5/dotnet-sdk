# Changelog

All notable changes to the Posty5 .NET SDK will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/), and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

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
