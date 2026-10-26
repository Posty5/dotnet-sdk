# 06 - Authentication and Security

## Rules and trust boundaries

- API keys are X-API-Key credentials and must not be committed, logged, or placed in package metadata.
- Tests read user-level environment variables and can call a live API; avoid production credentials.
- HttpClient retry/error behavior is shared by every package.
- Signed upload URLs and file streams must be disposed and handled without leaking credentials.
- Public classes/models are NuGet semver contracts.
- `Posty5HttpClient` does not follow redirects: the handler drops only `Authorization` on a redirect, so `X-API-Key` would reach the host a `Location` names. A plain-http `BaseUrl` off loopback logs a warning through `Posty5Options.Logger`.
- Every caller-supplied id placed in a route goes through `PathSegmentHelper.Escape(id, nameof(id))` (Core 5.0.0): percent-encoded as one segment, with blank, `.` and `..` refused.

## Secret handling

- Document configuration names and purposes only, never values.
- Never print credentials, tokens, cookies, signed URLs, private endpoints, or production payloads.
- Browser/client checks are not backend authorization.
- Read [15_RISKY_AREAS.md](15_RISKY_AREAS.md) before security-sensitive work.
