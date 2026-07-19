# 06 - Authentication and Security

## Rules and trust boundaries

- API keys are X-API-Key credentials and must not be committed, logged, or placed in package metadata.
- Tests read user-level environment variables and can call a live API; avoid production credentials.
- HttpClient retry/error behavior is shared by every package.
- Signed upload URLs and file streams must be disposed and handled without leaking credentials.
- Public classes/models are NuGet semver contracts.

## Secret handling

- Document configuration names and purposes only, never values.
- Never print credentials, tokens, cookies, signed URLs, private endpoints, or production payloads.
- Browser/client checks are not backend authorization.
- Read [15_RISKY_AREAS.md](15_RISKY_AREAS.md) before security-sensitive work.
