# 12 - Testing and Debugging

- The shared xUnit project references all SDK projects.
- Several tests require POSTY5_API_KEY and optionally POSTY5_BASE_URL and exercise live endpoints/media.
- Run build even when live credentials are unavailable; report skipped integration verification explicitly.
- Package versions are not fully uniform, so release work must audit every csproj.

## Debugging order

1. Reproduce with the smallest owning module or route/API call.
2. Inspect the exact entrypoint and boundary contract.
3. Check configuration names without printing values.
4. Run the narrow check, then the project build/typecheck.
5. Record any check that could not run and why.

## Store dropshipping tests

`tests/Posty5.Tests/StoreSuppliersClientTests.cs` has an offline class
(`StoreSuppliersRouteTests`): the real `Posty5HttpClient` is pointed at a local
`HttpListener` that records each request and answers with the API envelope, so
every route, verb, query and body is pinned without the network.

The live class (`StoreSuppliersLiveTests`) uses `[StoreFixtureFact(...)]`
(`tests/Posty5.Tests/StoreFixtureFactAttribute.cs`): a fact whose fixture is not
set is reported as **skipped**, naming the missing variable, instead of passing
silently. Variables are read from the User scope, then the process
(`TestConfig.Env`).

| Variable | Enables |
| --- | --- |
| `POSTY5_API_KEY` + `POSTY5_TEST_STORE_ID` | Every live fact: catalogue/list, supplier order queue + get, orders `NeedsAttention` + `FulfilmentGroups`, links list. |
| `POSTY5_TEST_SUPPLIER_INTEGRATION_ID` (a **test-mode** connection) | Balance, test, browse, automation round trip (restored in `finally`). |
| + `POSTY5_TEST_SUPPLIER_PRODUCT_ID` | Preview import. |
| + `POSTY5_TEST_PRODUCT_ID` | Create → update → sync → delete link (cleanup in `Dispose`). |
| `POSTY5_TEST_ALLOW_CHARGES=true` | Import one draft (charges credits; deleted in `Dispose`). |
| `POSTY5_TEST_ORDER_ID` + `POSTY5_TEST_GROUP_KEY` | Submit, retry, pay on the test-mode part; each must answer `testMode`. |
| + `POSTY5_TEST_ALLOW_PART_TAKEOVER=true` | Cancel, then fulfil manually — ends the fixture part; re-make it afterwards. |

Never run by any fact: connect, replace credentials, disconnect, set enabled.

