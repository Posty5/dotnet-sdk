# 03 - Folder Map

## Maintained source

| Path | Purpose |
| --- | --- |
| `src/Posty5.Core` | Options (`Configuration/` incl. `Posty5HttpDefaults`, `CreatedFromDefaults`), HTTP (`Http/` incl. `Posty5ClientIdentity`), exceptions, common response/pagination/`AgentOrigin` models, converters. |
| `src/Posty5.Account` | Account client (`AccountClient.cs`), its routes (`AccountRoutes.cs`) and `Models/` (who am I, credits, operation costs, string-constant vocabularies). |
| `src/Posty5.ShortLink` | Short-link client/models. |
| `src/Posty5.QRCode` | QR client/models; `QRCodeTemplateClient.cs` + `QRCodeTemplateRoutes.cs` for template lookups. |
| `src/Posty5.HtmlHosting` | HTML hosting client/models. |
| `src/Posty5.HtmlHostingVariables` | Hosting variables client/models. |
| `src/Posty5.HtmlHostingFormSubmission` | Submission client/models. |
| `src/Posty5.SocialPublisherWorkspace` | Social workspace client/models; `SocialPublisherAccountClient.cs` + `SocialPublisherAccountRoutes.cs` for connected accounts. |
| `src/Posty5.SocialPublisherPost` | Social post client/models/uploads (`Models/` has one file per post kind: text, story, long video, results). |
| `src/Posty5.Store` | Online store client/models: `StoreClient.cs` facade (+ `StoreRoutes.cs` for the store lookup), `Clients/` (products, orders, tags, customers, shipping, suppliers on `StoreClientBase`), `Models/` (incl. `SupplierConstants.cs` string vocabularies). |
| `tests/Posty5.Tests` | xUnit tests and assets: offline route tests on the shared `RecordingServer.cs`, live facts guarded by `[ApiKeyFact]` / `[StoreFixtureFact]`, and the older unguarded `Integration` classes. |
| `.github/workflows/publish-nuget.yml` | Tag-triggered pack and NuGet push; a new package must be added to its pack list. |
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
