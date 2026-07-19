# 04 - Modules and Ownership

| Area | Purpose | Primary path |
| --- | --- | --- |
| `core` | Posty5Options, Posty5HttpClient, errors, response/pagination types, converters. | `src/Posty5.Core` |
| `short-link` | Short-link list/get/create/update/delete. | `src/Posty5.ShortLink` |
| `qr-code` | Create/update seven QR types plus get/list/delete. | `src/Posty5.QRCode` |
| `html-hosting` | File/GitHub create/update and hosting management. | `src/Posty5.HtmlHosting` |
| `html-hosting-variables` | Hosting variable CRUD/list. | `src/Posty5.HtmlHostingVariables` |
| `html-hosting-form-submission` | Submission get/navigation/list/status/delete. | `src/Posty5.HtmlHostingFormSubmission` |
| `social-publisher-workspace` | Workspace CRUD/list and logo upload. | `src/Posty5.SocialPublisherWorkspace` |
| `social-publisher-post` | Video/image publishing and post status/list. | `src/Posty5.SocialPublisherPost` |

## Editing rule

Start changes in the owning module. Move code to shared/core only after more than one feature genuinely owns the behavior. Update this file and [MODULE_INDEX.json](MODULE_INDEX.json) when ownership changes.
