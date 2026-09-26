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
every route, verb, query and body is pinned without the network. The live class
reads only, and returns early unless `POSTY5_TEST_STORE_ID` is set.

