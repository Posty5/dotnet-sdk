# 10 - Environment and Configuration

> Names and purposes only. Read actual values only when a task explicitly requires it and never copy them into documentation or output.

| Name | Purpose | Source | Category |
| --- | --- | --- | --- |
| `POSTY5_API_KEY` | Test/example API key; secret. | `tests/Posty5.Tests/TestConfig.cs` | environment |
| `POSTY5_BASE_URL` | Live-test API origin. Unset: live tests aim at a closed loopback port and the live facts skip. | `tests/Posty5.Tests/TestConfig.cs` | environment |
| `POSTY5_TEST_ALLOW_PRODUCTION` | `true` lets live tests use production when `POSTY5_BASE_URL` is unset. | `tests/Posty5.Tests/TestConfig.cs` | environment |
| `Posty5Options.AllowInsecureHttp` | Allow a plain-http `BaseUrl` off loopback (refused by default). | `src/Posty5.Core/Configuration/Posty5Options.cs` | runtime |
| `Posty5Options.BaseUrl` | Per-client API base URL. | `src/Posty5.Core/Configuration/Posty5Options.cs` | runtime |
| `Posty5Options.ApiKey` | Per-client X-API-Key credential; secret. | `src/Posty5.Core/Configuration/Posty5Options.cs` | runtime |
| `Posty5Options.Debug` | Debug behavior; avoid sensitive data. | `src/Posty5.Core/Configuration/Posty5Options.cs` | runtime |
| `Posty5Options.DefaultHeaders` | Headers on every request; `X-API-Key` refused (constructor throws), `X-Posty5-Client` overridable, content headers refused. | `src/Posty5.Core/Configuration/Posty5Options.cs` | runtime |
| `Posty5Options.CreatedFrom` | `createdFrom` label on created records; null keeps `CreatedFromDefaults.Package` (`dotnetPackage`). | `src/Posty5.Core/Configuration/Posty5Options.cs` | runtime |

## Rules

- Keep server secrets out of browser bundles and public package metadata.
- Update all typed variants/contracts when adding a build-time key.
- Update [ENV_INDEX.json](ENV_INDEX.json) with names, purpose, owner, and sensitivity - never values.
