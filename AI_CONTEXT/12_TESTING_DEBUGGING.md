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
