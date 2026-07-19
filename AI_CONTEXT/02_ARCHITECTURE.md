# 02 - Architecture

## Topology

Eight NuGet-ready .NET projects sharing Posty5.Core and typed clients/models for links, QR codes, hosting, variables, form submissions, and social publishing.

### Client call

Consumer -> typed feature client -> Posty5HttpClient -> API -> System.Text.Json model/error.

### Signed upload

Client requests upload config -> uploads stream -> completes domain operation.

### Build

dotnet build Posty5.sln builds eight libraries and tests.

### Test

dotnet test runs xUnit tests; configured credentials may make them integration tests.

### Pack

dotnet pack creates NuGet packages under chosen output.

## Ownership rules

- Owns .NET client behavior and public types, not backend behavior.
- Every source project is a separate package and references Posty5.Core.
- bin, obj, nupkg, .vs, and build/test logs are generated or local.

## State and contracts

- Posty5HttpClient owns HttpClient, options, headers, retry behavior, and disposal.
- Feature clients receive/use the shared transport.
- Models map the standard API envelope and cursor pagination.
- CancellationToken parameters propagate consumer cancellation.
- TestConfig reads environment variables once for the suite.

Use [04_MODULES.md](04_MODULES.md) for owner paths and [17_PATTERNS.md](17_PATTERNS.md) for implementation conventions.
