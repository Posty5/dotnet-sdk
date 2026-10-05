# 04 - Modules and Ownership

| Area | Purpose | Primary path |
| --- | --- | --- |
| `core` | Posty5Options (incl. `DefaultHeaders`, `CreatedFrom`), Posty5HttpClient (`X-Posty5-Client`, `ResolveCreatedFrom`, no retries), errors, response/pagination/`AgentOrigin` types, converters, shared link/QR analytics models + `LinkAnalyticsQueryHelper`; per-request headers overloads (`PostAsync`/`PutAsync`/`DeleteAsync`/`GetBytesAsync` with `headers`, D15); shared bulk engine `LinkBulkOperations` + `LinkBulkModels` + `BulkDefaults` (BW). | `src/Posty5.Core` |
| `account` | Who the API key is, credits, credit history and summary, live operation costs (read-only). | `src/Posty5.Account` |
| `short-link` | Short-link list/get/create/update/delete; bulk in `ShortLinkClient.Bulk.cs` (`CreateManyAsync`, `ExportAsync`, bulk jobs). | `src/Posty5.ShortLink` |
| `qr-code` | Create/update seven QR types plus get/list/delete; user and public template lists. `Mode` (`QRCodeMode` static/dynamic, sent only when set; Wi-Fi + Dynamic throws `ArgumentException` before the call), `DynamicSince` on responses, `Mode` list filter. Bulk in `QRCodeClient.Bulk.cs` (`QRCodeBulkRow` type + target, ZIP jobs). | `src/Posty5.QRCode` |
| `webhooks` | `WebhookEndpointClient` (`/api/webhook-endpoints`), `WebhookSignature.Verify` (Standard Webhooks HMAC, fixed-time, rotation, tolerance), polymorphic `WebhookEvent` models. | `src/Posty5.Webhooks` |
| `html-hosting` | File/GitHub create/update and hosting management. | `src/Posty5.HtmlHosting` |
| `html-hosting-variables` | Hosting variable CRUD/list. | `src/Posty5.HtmlHostingVariables` |
| `html-hosting-form-submission` | Submission get/navigation/list/status/delete. | `src/Posty5.HtmlHostingFormSubmission` |
| `social-publisher-workspace` | Workspace CRUD/list and logo upload; connected social accounts (read-only). | `src/Posty5.SocialPublisherWorkspace` |
| `social-publisher-post` | Video/image/text/story publishing, post status/list, reschedule, delete (unpublished), remove (published). | `src/Posty5.SocialPublisherPost` |
| `store` | Store lookup, catalogue, orders, tags, customers, shipping, dropshipping suppliers. | `src/Posty5.Store` |

## Editing rule

Start changes in the owning module. Move code to shared/core only after more than one feature genuinely owns the behavior. Update this file and [MODULE_INDEX.json](MODULE_INDEX.json) when ownership changes.
