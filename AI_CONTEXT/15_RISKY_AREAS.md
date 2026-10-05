# 15 - Risky Areas

| Area | Path | Why risky |
| --- | --- | --- |
| Core transport | `src/Posty5.Core/Http/HttpClient.cs` | Affects authentication, retry, serialization, disposal, and every package. |
| Public models | `src/*/Models` | Renaming properties/types is a breaking NuGet change. |
| Package versions | `src/*/*.csproj` | Current projects are not all on the same major/minor; coordinate releases deliberately. |
| Live tests | `tests/Posty5.Tests/TestConfig.cs` | Can mutate configured API data. |
| Uploads | `src/Posty5.SocialPublisherPost/SocialPublisherPostClient.cs` | Large streams, signed URLs, and cancellation need care. |
| Generated artifacts | `bin, obj, nupkg` | Never edit or index as source. |
| Publish pack list | `.github/workflows/publish-nuget.yml` | A package missing from the list builds but never ships (Posty5.Store once did); add every new project. |
| Status vocabularies | `src/Posty5.SocialPublisherPost/Models/SocialPublisherPostModels.cs` (`SocialPublisherPostStatusType`) | The `StringValueObjectConverter` throws on an unknown value, so a status added on the server breaks every read of a post carrying it until the SDK adds it. |

Before editing: trace callers/consumers, identify compatibility and security impact, take the narrowest change, and run both focused and structural checks.
