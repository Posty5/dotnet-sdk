# 03 - Folder Map

## Maintained source

| Path | Purpose |
| --- | --- |
| `src/Posty5.Core` | Options, HTTP, exceptions, common response/pagination models, converters. |
| `src/Posty5.ShortLink` | Short-link client/models. |
| `src/Posty5.QRCode` | QR client/models. |
| `src/Posty5.HtmlHosting` | HTML hosting client/models. |
| `src/Posty5.HtmlHostingVariables` | Hosting variables client/models. |
| `src/Posty5.HtmlHostingFormSubmission` | Submission client/models. |
| `src/Posty5.SocialPublisherWorkspace` | Social workspace client/models. |
| `src/Posty5.SocialPublisherPost` | Social post client/models/uploads. |
| `tests/Posty5.Tests` | xUnit integration/client tests and assets. |
| `examples` | Consumer usage examples. |

## Root entrypoints

| Role | Path |
| --- | --- |
| Solution | `Posty5.sln` |
| Core options | `src/Posty5.Core/Configuration/Posty5Options.cs` |
| Core HTTP client | `src/Posty5.Core/Http/HttpClient.cs` |
| Source projects | `src` |
| Tests | `tests/Posty5.Tests` |
| Examples | `examples/Examples.cs` |

## Generated/local artifacts

Do not edit or index dependency folders, build output, coverage, caches, archives, logs, IDE state, or nested Git metadata. Common examples are `node_modules`, `dist`, `coverage`, `bin`, `obj`, `.angular`, `.astro`, `.vs`, package archives, and test/build logs. If output is wrong, change its source and rebuild.
