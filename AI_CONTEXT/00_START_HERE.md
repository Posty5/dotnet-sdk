# 00 - Start Here

> Orientation for AI assistants. Continue with [14_AI_TASK_ROUTING.md](14_AI_TASK_ROUTING.md) for task-specific files.

## What this project does

Eight NuGet-ready .NET projects sharing Posty5.Core and typed clients/models for links, QR codes, hosting, variables, form submissions, and social publishing.

## Quick facts

| Fact | Value |
| --- | --- |
| Project | Posty5 .NET SDK |
| Type | .NET 8 multi-project SDK solution |
| Runtime | .NET 8 |
| Package/build system | dotnet / NuGet |
| Scope root | dotnet-sdk/ |
| Generated context date | 2026-07-19 |

## Main entrypoints

| Role | Path |
| --- | --- |
| Solution | `Posty5.sln` |
| Core options | `src/Posty5.Core/Configuration/Posty5Options.cs` |
| Core HTTP client | `src/Posty5.Core/Http/HttpClient.cs` |
| Source projects | `src` |
| Tests | `tests/Posty5.Tests` |
| Examples | `examples/Examples.cs` |

## Runtime/control flow

Consumer -> typed feature client -> Posty5HttpClient -> API -> System.Text.Json model/error.

## How to approach changes

1. Use [14_AI_TASK_ROUTING.md](14_AI_TASK_ROUTING.md) to find the owning area.
2. Read its entry in [04_MODULES.md](04_MODULES.md) and the matching JSON index.
3. Check [15_RISKY_AREAS.md](15_RISKY_AREAS.md).
4. Change maintained source only; do not edit generated artifacts.
5. Run the checks in [11_LOCAL_DEVELOPMENT.md](11_LOCAL_DEVELOPMENT.md).
6. Update context per [16_DOCUMENTATION_MAINTENANCE.md](16_DOCUMENTATION_MAINTENANCE.md).
