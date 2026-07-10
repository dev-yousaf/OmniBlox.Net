# JWT Authentication implemented with Clean Architecture DIP

Authentication built with: custom exceptions (Shared), application interfaces (Application), MediatR CQRS handlers (Application), JwtService + CurrentUserService (Infrastructure), AuthController + Middleware (Api).

Key architectural decisions:
1. **EF Core in Application layer** — pragmatic trade-off to enable `Include()` and LINQ in handlers without repository abstractions. Justified as the standard Clean Architecture compromise.
2. **BCrypt.Net-Next in both Application and Infrastructure** — handlers in Application need BCrypt for password verification/hashing, Infrastructure needs it too (but currently only Application uses it directly). Could refactor to interface later.
3. **IOptions&lt;T&gt; pattern** for JwtSettings — reads from appsettings.json via DI.
4. **Manual claims extraction** via `FindFirstValue()` with custom "companyId" claim for multi-tenancy.

**Implications:** The auth flow is complete and testable via curl. Next step is wiring up multi-tenant EF Core global query filters using the `ICurrentUserService.CompanyId` that's now available.
