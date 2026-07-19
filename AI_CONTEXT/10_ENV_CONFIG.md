# 10 - Environment and Configuration

> Names and purposes only. Read actual values only when a task explicitly requires it and never copy them into documentation or output.

| Name | Purpose | Source | Category |
| --- | --- | --- | --- |
| `POSTY5_API_KEY` | Test/example API key; secret. | `tests/Posty5.Tests/TestConfig.cs` | environment |
| `POSTY5_BASE_URL` | Test API origin override. | `tests/Posty5.Tests/TestConfig.cs` | environment |
| `Posty5Options.BaseUrl` | Per-client API base URL. | `src/Posty5.Core/Configuration/Posty5Options.cs` | runtime |
| `Posty5Options.ApiKey` | Per-client X-API-Key credential; secret. | `src/Posty5.Core/Configuration/Posty5Options.cs` | runtime |
| `Posty5Options.Debug` | Debug behavior; avoid sensitive data. | `src/Posty5.Core/Configuration/Posty5Options.cs` | runtime |

## Rules

- Keep server secrets out of browser bundles and public package metadata.
- Update all typed variants/contracts when adding a build-time key.
- Update [ENV_INDEX.json](ENV_INDEX.json) with names, purpose, owner, and sensitivity - never values.
