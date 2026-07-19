# 17 - Established Patterns

| Pattern | Rule | Example |
| --- | --- | --- |
| Shared transport injection | Feature clients accept Posty5HttpClient. | `src/Posty5.ShortLink/ShortLinkClient.cs` |
| Async plus cancellation | Public network methods use Async suffix and CancellationToken. | `src/Posty5.HtmlHosting/HtmlHostingClient.cs` |
| Domain model file | Each package groups request/response models under Models. | `src/Posty5.QRCode/Models/QRCodeModels.cs` |
| Package project reference | Feature csproj references Posty5.Core. | `src/Posty5.ShortLink/Posty5.ShortLink.csproj` |
| Shared integration tests | One xUnit project covers every package. | `tests/Posty5.Tests/Posty5.Tests.csproj` |

Patterns describe current source, not aspirational refactors. Add a pattern only when multiple maintained examples or a clear architectural boundary support it.
