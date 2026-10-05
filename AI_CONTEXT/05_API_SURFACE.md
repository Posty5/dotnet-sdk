# 05 - Public API Surface

| Public surface | Behavior | Source |
| --- | --- | --- |
| `Posty5Options` | BaseUrl, ApiKey, Debug, `DefaultHeaders` (X-API-Key refused, X-Posty5-Client overridable), `CreatedFrom` (null keeps package defaults), plus fixed timeout (120 s) and unused retry fields. | `src/Posty5.Core/Configuration/Posty5Options.cs` |
| `Posty5HttpClient` | GetAsync/PostAsync/PutAsync/PatchAsync/DeleteAsync, GetBytesAsync (file downloads), SetApiKey, `CreatedFrom` / `ResolveCreatedFrom(packageDefault)`. Sends `X-Posty5-Client: posty5-dotnet/<version>` on every request. Never retries. | `src/Posty5.Core/Http/HttpClient.cs` |
| `Posty5ClientIdentity`, `Posty5HttpDefaults`, `CreatedFromDefaults` | The client label/version, header names, and the package `createdFrom` labels. | `src/Posty5.Core/Http/Posty5ClientIdentity.cs`, `src/Posty5.Core/Configuration/` |
| `AccountClient` | GetCurrentAsync (`/api/api-key/current`), GetCreditsAsync, GetCreditUsageAsync (cursor), GetCreditUsageSummaryAsync, GetOperationCostsAsync (public). | `src/Posty5.Account/AccountClient.cs` |
| `StoreClient` | `ListStoresAsync` / `LookupStoresAsync(term?)` (`/api/store/lookup`, no storeId), six sub-clients — `Products`, `Orders`, `Tags`, `Customers`, `Shipping`, `Suppliers` (dropshipping, `/api/store-suppliers`) — plus four legacy shorthands. | `src/Posty5.Store/StoreClient.cs` |
| `ShortLinkClient` | ListAsync/GetAsync/CreateAsync/UpdateAsync/DeleteAsync. | `src/Posty5.ShortLink/ShortLinkClient.cs` |
| `QRCodeClient` | Create/update by type plus GetAsync/ListAsync/DeleteAsync. | `src/Posty5.QRCode/QRCodeClient.cs` |
| `QRCodeTemplateClient` | ListUserTemplatesAsync (`/user-lookup`), ListPublicTemplatesAsync (`/public-lookup`), cursor-paged. | `src/Posty5.QRCode/QRCodeTemplateClient.cs` |
| `SocialPublisherAccountClient` | ListAsync/LookupAsync/GetAsync on `/api/social-publisher-account` (read-only). | `src/Posty5.SocialPublisherWorkspace/SocialPublisherAccountClient.cs` |
| `HtmlHostingClient` | File/GitHub create/update, get/list/lookups/cache/delete. | `src/Posty5.HtmlHosting/HtmlHostingClient.cs` |
| `HtmlHostingVariablesClient` | CreateAsync/GetAsync/UpdateAsync/DeleteAsync/ListAsync. | `src/Posty5.HtmlHostingVariables/HtmlHostingVariablesClient.cs` |
| `HtmlHostingFormSubmissionClient` | Get/list/navigation/status/delete. | `src/Posty5.HtmlHostingFormSubmission/HtmlHostingFormSubmissionClient.cs` |
| `SocialPublisherWorkspaceClient` | List/get/get-for-new/create/update/delete. | `src/Posty5.SocialPublisherWorkspace/SocialPublisherWorkspaceClient.cs` |
| `SocialPublisherPostClient` | List/defaults/status (`GET /{id}/status`, fixed in 4.6.0)/uploads, short/long video and image publish, text posts and stories (`/text|story/workspace|account`), reschedule, `DeletePostAsync` (unpublished), `RemovePostAsync` (`POST /{id}/remove`, published). | `src/Posty5.SocialPublisherPost/SocialPublisherPostClient.cs` |

This is a compatibility surface. Treat exported names, operations, parameter values, types, and behavior as semver-sensitive.

Machine-readable routing metadata lives in [ROUTE_INDEX.json](ROUTE_INDEX.json).
