# 09 - External Integrations

| System | Purpose | Owner/config source |
| --- | --- | --- |
| Posty5 API | All client operations | `src/Posty5.Core/Http/HttpClient.cs` |
| NuGet | Eight package projects | `src` |
| System.Net.Http | Transport/retries/uploads | `src/Posty5.Core/Http/HttpClient.cs` |
| Signed object storage | Direct file/logo/media uploads | `src/Posty5.SocialPublisherPost/SocialPublisherPostClient.cs` |
| xUnit | Client/integration tests | `tests/Posty5.Tests/Posty5.Tests.csproj` |

## Change rule

When an integration changes, update its owner, configuration names, error/retry behavior, tests, [INTEGRATION_INDEX.json](INTEGRATION_INDEX.json), and risky-area notes. Never document credential values.
