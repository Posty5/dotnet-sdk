# 05 - Public API Surface

| Public surface | Behavior | Source |
| --- | --- | --- |
| `Posty5Options` | BaseUrl, ApiKey, Debug plus fixed timeout/retry settings. | `src/Posty5.Core/Configuration/Posty5Options.cs` |
| `Posty5HttpClient` | GetAsync/PostAsync/PutAsync/DeleteAsync and SetApiKey. | `src/Posty5.Core/Http/HttpClient.cs` |
| `ShortLinkClient` | ListAsync/GetAsync/CreateAsync/UpdateAsync/DeleteAsync. | `src/Posty5.ShortLink/ShortLinkClient.cs` |
| `QRCodeClient` | Create/update by type plus GetAsync/ListAsync/DeleteAsync. | `src/Posty5.QRCode/QRCodeClient.cs` |
| `HtmlHostingClient` | File/GitHub create/update, get/list/lookups/cache/delete. | `src/Posty5.HtmlHosting/HtmlHostingClient.cs` |
| `HtmlHostingVariablesClient` | CreateAsync/GetAsync/UpdateAsync/DeleteAsync/ListAsync. | `src/Posty5.HtmlHostingVariables/HtmlHostingVariablesClient.cs` |
| `HtmlHostingFormSubmissionClient` | Get/list/navigation/status/delete. | `src/Posty5.HtmlHostingFormSubmission/HtmlHostingFormSubmissionClient.cs` |
| `SocialPublisherWorkspaceClient` | List/get/get-for-new/create/update/delete. | `src/Posty5.SocialPublisherWorkspace/SocialPublisherWorkspaceClient.cs` |
| `SocialPublisherPostClient` | List/defaults/status/uploads and video/image publish. | `src/Posty5.SocialPublisherPost/SocialPublisherPostClient.cs` |

This is a compatibility surface. Treat exported names, operations, parameter values, types, and behavior as semver-sensitive.

Machine-readable routing metadata lives in [ROUTE_INDEX.json](ROUTE_INDEX.json).
