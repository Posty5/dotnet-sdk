# 11 - Local Development

| Task | Command |
| --- | --- |
| Restore | `dotnet restore Posty5.sln` |
| Build | `dotnet build Posty5.sln` |
| Test | `dotnet test Posty5.sln` |
| Release build | `dotnet build Posty5.sln -c Release` |
| Pack | `dotnet pack Posty5.sln -c Release` |
| Run selected tests | `dotnet test tests/Posty5.Tests/Posty5.Tests.csproj --filter <filter>` |

## Working rules

- Prefer clean installs from the lockfile.
- Run commands from the `dotnet-sdk/` project root.
- Do not commit generated output, dependencies, archives, credentials, or local logs.
- Confirm command names against the current manifest/project files when documentation and source disagree.
