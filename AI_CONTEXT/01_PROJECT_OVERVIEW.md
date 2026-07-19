# 01 - Project Overview

## Purpose

Eight NuGet-ready .NET projects sharing Posty5.Core and typed clients/models for links, QR codes, hosting, variables, form submissions, and social publishing.

## Capabilities

- **Authenticated HTTP client** - owned by `core`.
- **Short links** - owned by `short-link`.
- **QR codes** - owned by `qr-code`.
- **HTML hosting** - owned by `html-hosting`.
- **Hosting variables** - owned by `html-hosting-variables`.
- **Form submissions** - owned by `html-hosting-form-submission`.
- **Social workspaces** - owned by `social-publisher-workspace`.
- **Social posts** - owned by `social-publisher-post`.

## Stack

- Project type: .NET 8 multi-project SDK solution.
- Runtime: .NET 8.
- Package/build system: dotnet / NuGet.

## Boundaries

- Owns .NET client behavior and public types, not backend behavior.
- Every source project is a separate package and references Posty5.Core.
- bin, obj, nupkg, .vs, and build/test logs are generated or local.

The source of truth for ownership is [MODULE_INDEX.json](MODULE_INDEX.json); feature mapping is in [FEATURE_INDEX.json](FEATURE_INDEX.json).
