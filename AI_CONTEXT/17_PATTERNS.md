# 17 - Established Patterns

| Pattern | Rule | Example |
| --- | --- | --- |
| Shared transport injection | Feature clients accept Posty5HttpClient. | `src/Posty5.ShortLink/ShortLinkClient.cs` |
| Async plus cancellation | Public network methods use Async suffix and CancellationToken. | `src/Posty5.HtmlHosting/HtmlHostingClient.cs` |
| Domain model file | Each package groups request/response models under Models. | `src/Posty5.QRCode/Models/QRCodeModels.cs` |
| Package project reference | Feature csproj references Posty5.Core. | `src/Posty5.ShortLink/Posty5.ShortLink.csproj` |
| Shared integration tests | One xUnit project covers every package. | `tests/Posty5.Tests/Posty5.Tests.csproj` |
| Offline route pinning | A new or changed route gets a test driving the real `Posty5HttpClient` against `RecordingServer`, asserting verb, path+query, body (and headers where relevant). | `tests/Posty5.Tests/SocialPublisherPostRouteTests.cs` |
| Guarded live facts | Live tests use `[ApiKeyFact]` / `[StoreFixtureFact]`, so a missing fixture is reported as skipped, never a silent pass. | `tests/Posty5.Tests/ApiKeyFactAttribute.cs` |
| Routes in a routes file (new clients) | Clients added from 2026-10 keep route constants in an internal `*Routes.cs` beside the client (workspace AI_RULES §4); older clients keep their private `BasePath` const until they are next reworked. | `src/Posty5.Account/AccountRoutes.cs` |
| createdFrom through ResolveCreatedFrom | A create stamps `request value ?? _http.ResolveCreatedFrom(CreatedFromDefaults.X)`; never a literal. | `src/Posty5.SocialPublisherPost/SocialPublisherPostClient.cs` |
| String vocabularies as constants | API vocabularies are string constants in a static class, not enums, so a new server value never breaks deserialization. | `src/Posty5.Account/Models/AccountConstants.cs`, `src/Posty5.Store/Models/SupplierConstants.cs` |

Patterns describe current source, not aspirational refactors. Add a pattern only when multiple maintained examples or a clear architectural boundary support it.
