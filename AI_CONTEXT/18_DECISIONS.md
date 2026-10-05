# 18 - Decisions

## D01 - All projects target net8.0.

**Status:** observed in current source. Revisit only with compatibility, migration, and verification impact documented.

## D02 - Feature libraries depend on Posty5.Core for transport and common models.

**Status:** observed in current source. Revisit only with compatibility, migration, and verification impact documented.

## D03 - Each feature is a separate NuGet package.

**Status:** observed in current source. Revisit only with compatibility, migration, and verification impact documented.

## D04 - Public network methods follow Task/Async and cancellation patterns.

**Status:** observed in current source. Revisit only with compatibility, migration, and verification impact documented.

## D05 - ROUTE_INDEX is empty because this repository is a client SDK.

**Status:** observed in current source. Revisit only with compatibility, migration, and verification impact documented.

## D06 - A field the API never accepted is obsoleted and ignored, not deleted, in a minor (TP-D5).

**Status:** decided 2026-10-05 (link-qr truth pass, `.agent/tasks/link-qr-truth-pass/plan.md` TP-D5). `IsEnableMonetization` on the short-link and QR models is `[Obsolete]` + `[JsonIgnore]`, and the clients build explicit payloads so it never reaches the wire (the API's Joi rejected it with a 400). Old code keeps compiling with a CS0618 warning and stops failing; the property is deleted in the 4.0.0 tool majors. Same pattern for any later "never accepted" field.

## D07 - List filters the API does not read are obsoleted and no longer sent.

**Status:** decided 2026-10-05 (link-qr truth pass). `ShortLinkListParamsModel.Search`, `FromDate`, `ToDate` were sent but the API's short-link search reads none of them (`api/.../short-link/module.ts` search filter lists). They are `[Obsolete]` and dropped from the query rather than kept as silent no-ops; adding real filters is an API change, not an SDK one.

## D08 - Fields the API requires for API-key callers use the C# `required` modifier.

**Status:** decided 2026-10-05 (link-qr truth pass, TP-D7). `TemplateId` (short-link and QR request models) and short-link `BaseUrl` are `required`, so a request without them fails to compile - the .NET form of the npm SDK's non-optional property and the feature's acceptance criterion 6. Cost: a caller that assigns them after construction must move them into the object initializer.

## D09 - Link/QR analytics models live in Posty5.Core; the answer keeps the API's strings.

**Status:** decided 2026-10-05 (link-qr visit analytics, `.agent/tasks/link-qr-visit-analytics/dotnet-sdk/link-qr-analytics-methods/plan.md`). The C2 answer is identical for short links and QR codes, so `LinkAnalyticsQuery` / `LinkAnalyticsModel` and `LinkAnalyticsQueryHelper` are declared once in `Posty5.Core` (3.2.0; 3.1.0 is the MCP release, which merges first), like the npm SDK's `@posty5/core` interfaces. The query uses the `StringValueObjectConverter` value types (`LinkAnalyticsInterval`, `LinkAnalyticsBreakdown`); the answer keeps `Meta.*`, `Locked[].Breakdown` and `Series[].Date` as strings, because the converter throws on an unknown value and a later API release may add one, and a `DateTime` would invite a time-zone shift of a day bucket. `AllBreakdowns` with a non-empty `Breakdown` throws `ArgumentException` rather than silently picking one.

## D10 - The analytics feature-lock 403 uses the core's existing mapping.

**Status:** decided 2026-10-05 (VA, C4). A 403 already surfaces as `Posty5Exception` with `StatusCode == 403` and the API body (its `message`) in `ResponseBody`; `Exception.Message` is the generic status text. VA does not change `Posty5HttpClient` error mapping, because that would change every client's 403 and `feat/mcp-wave-2` is editing the same file. Surfacing the API's `message` as `Exception.Message` for every status is a separate Core change.

Do not invent historical rationale. Record evidence-based current decisions and label unknown rationale explicitly.
