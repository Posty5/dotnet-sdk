# Changelog

All notable changes to the Posty5 .NET SDK will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/), and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

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
