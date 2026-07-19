# 15 - Risky Areas

| Area | Path | Why risky |
| --- | --- | --- |
| Core transport | `src/Posty5.Core/Http/HttpClient.cs` | Affects authentication, retry, serialization, disposal, and every package. |
| Public models | `src/*/Models` | Renaming properties/types is a breaking NuGet change. |
| Package versions | `src/*/*.csproj` | Current projects are not all on the same major/minor; coordinate releases deliberately. |
| Live tests | `tests/Posty5.Tests/TestConfig.cs` | Can mutate configured API data. |
| Uploads | `src/Posty5.SocialPublisherPost/SocialPublisherPostClient.cs` | Large streams, signed URLs, and cancellation need care. |
| Generated artifacts | `bin, obj, nupkg` | Never edit or index as source. |

Before editing: trace callers/consumers, identify compatibility and security impact, take the narrowest change, and run both focused and structural checks.
