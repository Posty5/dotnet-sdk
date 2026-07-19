# 08 - Workflows

| Workflow | Flow |
| --- | --- |
| Client call | Consumer -> typed feature client -> Posty5HttpClient -> API -> System.Text.Json model/error. |
| Signed upload | Client requests upload config -> uploads stream -> completes domain operation. |
| Build | dotnet build Posty5.sln builds eight libraries and tests. |
| Test | dotnet test runs xUnit tests; configured credentials may make them integration tests. |
| Pack | dotnet pack creates NuGet packages under chosen output. |

For common edit sequences, see [13_CHANGE_PLAYBOOK.md](13_CHANGE_PLAYBOOK.md).
