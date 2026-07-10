# EF Core migration gotchas in Clean Architecture

Two non-obvious issues when running `dotnet ef migrations` in a multi-project Clean Architecture solution:

1. **Design package must be in the startup project (Api), not where the DbContext lives (Infrastructure).** The CLI reads `appsettings.json` and `Program.cs` from the startup project, and needs `Microsoft.EntityFrameworkCore.Design` there to function.

2. **First `database update` shows a scary error** about failing to execute `SELECT FROM __EFMigrationsHistory`. This is expected — the database doesn't exist yet on the first run. The command creates it and then applies the migration. Look for `Done.` at the end as the success signal.

**Implication:** Future lessons should include the Design package step explicitly and warn about the first-run error to avoid confusion.
