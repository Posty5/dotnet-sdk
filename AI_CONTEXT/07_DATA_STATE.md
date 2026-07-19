# 07 - Data and State

- Posty5HttpClient owns HttpClient, options, headers, retry behavior, and disposal.
- Feature clients receive/use the shared transport.
- Models map the standard API envelope and cursor pagination.
- CancellationToken parameters propagate consumer cancellation.
- TestConfig reads environment variables once for the suite.

## Contract rule

Types, request/response shapes, serialized route parameters, persisted settings, and public models are contracts. Update producers, consumers, tests, and AI indexes together when they change.
