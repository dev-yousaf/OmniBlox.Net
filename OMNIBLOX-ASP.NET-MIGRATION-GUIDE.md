# OmniBlox — ASP.NET Core Migration Guide

## Overview

This document is a complete reference for migrating **OmniBlox** (a multi-tenant ERP platform) from its current stack (**Next.js 16 + NestJS 11 + Prisma + PostgreSQL**) to **ASP.NET Core 9**.

The original project has ~30 database models, 18 domain modules, ~100 REST endpoints, cookie-based auth with Better Auth, RBAC with 4 roles (OWNER/ADMIN/MANAGER/OBSERVER), email workflows, and auditing.

---

## 1. Recommended Architecture

### Clean Architecture + Vertical Slices + CQRS

```
OmniBlox.sln
├── src/
│   ├── OmniBlox.Api/                 # ASP.NET Core Web API
│   │   ├── Controllers/              # API endpoints
│   │   ├── Middleware/                # Multi-tenant, logging, exception handling
│   │   ├── Program.cs                # Startup / DI registration
│   │   └── appsettings.json
│   │
│   ├── OmniBlox.Application/         # Use cases (CQRS with MediatR)
│   │   ├── Features/                 # One folder per domain
│   │   │   ├── Products/
│   │   │   │   ├── Commands/
│   │   │   │   │   ├── CreateProduct.cs
│   │   │   │   │   ├── UpdateProduct.cs
│   │   │   │   │   └── DeleteProduct.cs
│   │   │   │   ├── Queries/
│   │   │   │   │   ├── GetProducts.cs
│   │   │   │   │   └── GetProductById.cs
│   │   │   │   └── DTOs/
│   │   │   ├── Sales/
│   │   │   ├── Purchases/
│   │   │   └── ... (one per domain)
│   │   ├── Common/
│   │   │   ├── Behaviors/            # MediatR pipeline behaviors
│   │   │   │   ├── ValidationBehavior.cs
│   │   │   │   ├── AuditLogBehavior.cs
│   │   │   │   └── CacheInvalidationBehavior.cs
│   │   │   └── Interfaces/
│   │   └── DependencyInjection.cs
│   │
│   ├── OmniBlox.Domain/             # Core entities (no dependencies)
│   │   ├── Entities/
│   │   ├── Enums/
│   │   ├── ValueObjects/
│   │   └── Interfaces/
│   │
│   ├── OmniBlox.Infrastructure/      # EF Core, Email, Auth, Caching
│   │   ├── Persistence/
│   │   │   ├── AppDbContext.cs
│   │   │   ├── Configurations/      # EF Core Fluent API mappings
│   │   │   ├── Migrations/
│   │   │   └── Repositories/
│   │   ├── Auth/
│   │   │   ├── JwtService.cs
│   │   │   └── CurrentUserService.cs
│   │   ├── Email/
│   │   │   └── EmailService.cs
│   │   ├── Caching/
│   │   │   └── CacheService.cs
│   │   └── DependencyInjection.cs
│   │
│   └── OmniBlox.Shared/             # Cross-cutting
│       ├── Exceptions/
│       ├── Results/
│       └── Extensions/
```

### Key NuGet Packages

| Package | Purpose | Replaces |
|---|---|---|
| `MediatR` | CQRS + pipeline behaviors | NestJS modular services |
| `FluentValidation` | Request validation | class-validator |
| `Microsoft.AspNetCore.Authentication.JwtBearer` | JWT auth | Better Auth |
| `Mapster` (or `AutoMapper`) | DTO mapping | Manual DTOs |
| `Serilog` | Structured logging | NestJS logger |
| `EF Core` | ORM | Prisma |
| `FluentEmail` + `SmtpClient` | Email | Nodemailer |
| `InMemoryCache` / `Redis` | Caching | cache-manager |
| `Swashbuckle` | OpenAPI/Swagger | - |

---

## 2. Database (EF Core → Prisma Mapping)

### Multi-Tenant Strategy

**Use column-based tenant isolation** with EF Core global query filters:

```csharp
// In AppDbContext
protected override void OnModelCreating(ModelBuilder builder)
{
    builder.Entity<Product>().HasQueryFilter(e => e.CompanyId == _currentTenantId);
    builder.Entity<Sale>().HasQueryFilter(e => e.CompanyId == _currentTenantId);
    builder.Entity<Inventory>().HasQueryFilter(e => e.Product.CompanyId == _currentTenantId);
    // Apply to all company-scoped entities
}
```

This automatically scopes every query — you never need to manually add `.Where(e => e.CompanyId == id)`.

### Entity → EF Core Mapping

Map each Prisma model to an EF Core entity. Here's the schema mapping table:

| Prisma Model | EF Core Entity | Key Constraints |
|---|---|---|
| `Company` | `Company` | PK: Id (Guid) |
| `User` | `User` | Unique: Email, FK → Company |
| `Session` | _Drop — use JWT_ | - |
| `Account` | _Drop — use JWT_ | - |
| `Verification` | _Drop — use JWT_ | - |
| `AuthToken` | Keep as `AuthToken` | FK → User |
| `Product` | `Product` | Unique(CompanyId, SKU) |
| `ComboItem` | `ComboItem` | Unique(ComboId, ProductId) |
| `ProductCategory` | `ProductCategory` | Unique(CompanyId, Name) |
| `SubCategory` | `SubCategory` | Unique(CompanyId, Name, CategoryId) |
| `Brand` | `Brand` | Unique(CompanyId, Name) |
| `VariantAttribute` | `VariantAttribute` | Unique(CompanyId, Name) |
| `Unit` | `Unit` | Unique(CompanyId, Name) |
| `Warranty` | `Warranty` | Unique(CompanyId, Name) |
| `Warehouse` | `Warehouse` | FK → Company |
| `Inventory` | `Inventory` | Composite PK (ProductId, WarehouseId) |
| `StockAdjustment` | `StockAdjustment` | Unique(CompanyId, ReferenceNumber) |
| `StockAdjustmentItem` | `StockAdjustmentItem` | FK → StockAdjustment, Product, Warehouse |
| `StockLedger` | `StockLedger` | Index on ProductId, WarehouseId |
| `Sale` | `Sale` | Unique(CompanyId, InvoiceNumber) |
| `SaleItem` | `SaleItem` | FK → Sale, Product |
| `Delivery` | `Delivery` | One-to-one with Sale |
| `Quotation` | `Quotation` | Unique(CompanyId, ReferenceNumber) |
| `QuotationItem` | `QuotationItem` | FK → Quotation, Product |
| `SalesReturn` | `SalesReturn` | Unique(CompanyId, ReferenceNumber) |
| `SalesReturnItem` | `SalesReturnItem` | FK → SalesReturn, optional SaleItem |
| `PurchaseOrder` | `PurchaseOrder` | Unique(CompanyId, ReferenceNumber) |
| `PurchaseOrderItem` | `PurchaseOrderItem` | FK → PurchaseOrder, Product |
| `PurchaseReturn` | `PurchaseReturn` | Unique(CompanyId, ReferenceNumber) |
| `PurchaseReturnItem` | `PurchaseReturnItem` | FK → PurchaseReturn, optional PurchaseOrderItem |
| `Expense` | `Expense` | FK → Company, Category, User |
| `ExpenseAttachment` | `ExpenseAttachment` | FK → Expense |
| `ExpenseCategory` | `ExpenseCategory` | Unique(CompanyId, Name) |
| `Customer` | `Customer` | Unique(CompanyId, Email) |
| `Supplier` | `Supplier` | Unique(CompanyId, Email) |
| `Biller` | `Biller` | Unique(CompanyId, Code) |
| `AuditLog` | `AuditLog` | Index on(CompanyId, CreatedAt) |

### Enum Mapping (Prisma → C#)

| Prisma Enum | C# Enum |
|---|---|
| `UserRole` | `OWNER, ADMIN, MANAGER, OBSERVER` |
| `UserStatus` | `ACTIVE, INVITED, SUSPENDED` |
| `ProductType` | `STANDARD, DIGITAL, SERVICE, COMBO` |
| `ProductStatus` | `ACTIVE, INACTIVE, DISCONTINUED` |
| `OrderStatus` | `DRAFT, PENDING, COMPLETED, CANCELLED, RECEIVED` |
| `PaymentStatus` | `PAID, PENDING, PARTIAL` |
| `PaymentMethod` | `CASH, CREDIT_CARD, BANK_TRANSFER, CHECK` |
| `ReturnStatus` | `PENDING, PROCESSING, COMPLETED, CANCELLED` |
| `ExpenseStatus` | `PENDING, APPROVED, PAID, REJECTED` |
| `BillerStatus` | `ACTIVE, INACTIVE` |
| `DeliveryStatus` | `PENDING, IN_TRANSIT, DELIVERED, CANCELLED` |
| `TokenType` | `EMAIL_VERIFICATION, PASSWORD_RESET, MAGIC_LINK_LOGIN, INVITATION` |

---

## 3. Auth Migration (Better Auth → JWT + ASP.NET Core Identity)

### Original (NestJS/Better Auth)
- Cookie-based sessions stored in DB
- Better Auth handles signup, login, session management
- Custom decorators: `@CompanyId()`, `@UserId()`, `@Roles()`, `@GetCurrentUser()`
- Session carries `companyId` and `role` as additional fields
- Retry logic for read-after-write consistency

### ASP.NET Core Equivalent

**Replace Better Auth with custom JWT authentication:**

```
┌─────────────────────────────────────────────────────────────┐
│ 1. Login: Validate credentials → Issue JWT with claims:    │
│    - sub (userId)                                           │
│    - email                                                  │
│    - role (OWNER/ADMIN/MANAGER/OBSERVER)                     │
│    - companyId (tenant)                                      │
│    - name                                                    │
│                                                              │
│ 2. Every request extracts claims via:                        │
│    - HttpContext.User.GetUserId()                            │
│    - HttpContext.User.GetCompanyId()                         │
│    - HttpContext.User.GetRole()                              │
│                                                              │
│ 3. Authorization:                                            │
│    [Authorize(Roles = "OWNER,ADMIN")]                        │
│    [Authorize(Policy = "ManagementOnly")]                     │
│                                                              │
│ 4. Multi-tenant:                                             │
│    - AppDbContext receives companyId via ICurrentUserService  │
│    - EF Core global query filters auto-apply tenant scope    │
└─────────────────────────────────────────────────────────────┘
```

### Custom Claims Extensions

```csharp
// OmniBlox.Api/Extensions/ClaimsPrincipalExtensions.cs
public static class ClaimsPrincipalExtensions
{
    public static Guid GetUserId(this ClaimsPrincipal user)
        => Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier));

    public static Guid GetCompanyId(this ClaimsPrincipal user)
        => Guid.Parse(user.FindFirstValue("companyId"));

    public static string GetRole(this ClaimsPrincipal user)
        => user.FindFirstValue(ClaimTypes.Role);
}
```

### JWT Token Service

```csharp
// OmniBlox.Infrastructure/Auth/JwtService.cs
public class JwtService
{
    public string GenerateToken(User user, Company company)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role.ToString()),
            new Claim(ClaimTypes.Name, user.Name),
            new Claim("companyId", company.Id.ToString()),
            new Claim("companyName", company.Name),
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Secret"]));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        return new JwtSecurityTokenHandler().WriteToken(
            new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddDays(7),
                signingCredentials: creds
            )
        );
    }
}
```

### Auth Controller (login/signup)

```
POST /auth/signup          → Create Company + Owner user, return JWT
POST /auth/login           → Validate email/password, return JWT
POST /auth/logout          → Client-side token discard (stateless JWT)
GET  /auth/me              → Return user profile from JWT claims + DB
PUT  /auth/profile         → Update name, company settings
PUT  /auth/change-password → Validate current password, update
POST /auth/verify-email    → Verify email token
POST /auth/verify-otp      → Verify 6-digit OTP
POST /auth/resend-otp      → Resend verification OTP
POST /auth/magic-login/request → Send magic link email
POST /auth/magic-login/verify  → Exchange token for JWT
POST /auth/password-reset/request  → Send reset email
POST /auth/password-reset/verify   → Reset password with token
POST /auth/accept-invitation       → Accept invite + set password
```

> **Note:** All auth endpoints remain anonymous (`[AllowAnonymous]`) except `/auth/me`, `/auth/profile`, `/auth/change-password`, `/auth/logout`, which require `[Authorize]`.

---

## 4. API Endpoint Mapping (NestJS → ASP.NET Core)

Each NestJS controller maps to an ASP.NET Core controller. The routing stays identical.

### Auth Module

| Method | Route | NestJS Decorator | ASP.NET Equivalent |
|---|---|---|---|
| POST | `/auth/signup` | `@AllowAnonymous()` | `[AllowAnonymous]` |
| POST | `/auth/login` | `@AllowAnonymous()` | `[AllowAnonymous]` |
| POST | `/auth/logout` | `@UseGuards(AuthGuard)` | `[Authorize]` |
| GET | `/auth/me` | `@UseGuards(AuthGuard)` | `[Authorize]` |
| PUT | `/auth/profile` | `@UseGuards(AuthGuard)` | `[Authorize]` |
| PUT | `/auth/change-password` | `@UseGuards(AuthGuard)` | `[Authorize]` |
| GET | `/auth/validate` | `@UseGuards(AuthGuard)` | `[Authorize]` |
| GET | `/auth/company` | `@UseGuards(AuthGuard)` | `[Authorize]` |
| GET | `/auth/session` | `@UseGuards(AuthGuard)` | `[Authorize]` |
| POST | `/auth/verify-email` | `@AllowAnonymous()` | `[AllowAnonymous]` |
| POST | `/auth/verify-otp` | `@AllowAnonymous()` | `[AllowAnonymous]` |
| POST | `/auth/resend-otp` | `@AllowAnonymous()` | `[AllowAnonymous]` |
| POST | `/auth/update-signup-email` | `@AllowAnonymous()` | `[AllowAnonymous]` |
| POST | `/auth/magic-login/request` | `@AllowAnonymous()` | `[AllowAnonymous]` |
| POST | `/auth/magic-login/verify` | `@AllowAnonymous()` | `[AllowAnonymous]` |
| POST | `/auth/password-reset/request` | `@AllowAnonymous()` | `[AllowAnonymous]` |
| POST | `/auth/password-reset/verify` | `@AllowAnonymous()` | `[AllowAnonymous]` |
| POST | `/auth/accept-invitation` | `@AllowAnonymous()` | `[AllowAnonymous]` |

### Products Module (36 endpoints — the largest)

| Method | Route | Roles | Notes |
|---|---|---|---|
| GET | `/products` | ALL | Paginated, search, filter by category/status/warehouse |
| POST | `/products` | OWNER/ADMIN/MANAGER | Create with auto category/brand creation |
| GET | `/products/categories` | ALL | List category names |
| GET | `/products/brands` | ALL | List brand names |
| GET | `/products/low-stock` | ALL | Products below reorder level |
| GET | `/products/low-stock/details` | ALL | Per-warehouse low stock |
| GET | `/products/expired` | ALL | Expired products |
| GET | `/products/stats` | ALL | Aggregated product stats |
| GET | `/products/export` | ALL | CSV export |
| POST | `/products/import` | OWNER/ADMIN/MANAGER | Bulk CSV import |
| POST | `/products/bulk-update-price` | OWNER/ADMIN/MANAGER | Bulk price update |
| GET | `/products/export-excel` | ALL | Excel export (same CSV format) |
| GET | `/products/sku/{sku}` | ALL | Lookup by SKU |
| POST | `/products/adjustments` | OWNER/ADMIN/MANAGER | Stock adjustment |
| GET | `/products/adjustments` | ALL | List stock adjustments |
| GET | `/products/adjustments/{id}` | ALL | Get single adjustment |
| GET | `/products/{id}` | ALL | Single product |
| PUT | `/products/{id}` | OWNER/ADMIN/MANAGER | Update product |
| PUT | `/products/{id}/stock` | OWNER/ADMIN/MANAGER | Quick stock update |
| DELETE | `/products/{id}` | OWNER/ADMIN/MANAGER | Cascade delete variants + inventory |
| GET | `/products/{id}/ledger` | ALL | Stock movement history |
| GET | `/products/{id}/variants` | ALL | Variant products |
| GET | `/products/{id}/combo-items` | ALL | Combo bundle components |
| GET | `/products/{id}/sales` | ALL | Sales history for product |
| GET | `/products/{id}/quotations` | ALL | Quotation history |
| GET | `/products/{id}/purchases` | ALL | Purchase history |
| GET | `/products/{id}/transfers` | ALL | Transfer history |
| GET | `/products/{id}/adjustments` | ALL | Adjustment history |
| GET | `/products/warehouses` | ALL | Warehouse list |

### General API Pattern (applies to all modules)

```
GET    /{resource}          → List (paginated + filtered)
POST   /{resource}          → Create
GET    /{resource}/{id}     → Get by ID
PUT    /{resource}/{id}     → Update
DELETE /{resource}/{id}     → Delete (hard delete)
```

### Complete Module List

| Module | Route Prefix | Endpoints | Notes |
|---|---|---|---|
| Auth | `/auth` | 18 | Signup, login, password mgmt, verification |
| Products | `/products` | ~30 | Product CRUD, stock adjust, import/export, history |
| Product Categories | `/product-categories` | 5 | CRUD |
| Sub Categories | `/sub-categories` | 5 | CRUD |
| Brands | `/brands` | 5 | CRUD |
| Variant Attributes | `/variant-attributes` | 5 | CRUD |
| Units | `/units` | 5 | CRUD |
| Warranties | `/warranties` | 5 | CRUD |
| Sales | `/sales` | 6 | CRUD + mark paid + stats |
| Sales Returns | `/sales-returns` | 5 | CRUD |
| Purchases | `/purchases` | 5 | CRUD + stats (note: maps to purchase-orders) |
| Purchase Returns | `/purchase-returns` | 5 | CRUD |
| Quotations | `/quotations` | 5 | CRUD |
| Deliveries | `/deliveries` | 5 | CRUD |
| Customers | `/customers` | 5 | CRUD |
| Suppliers | `/suppliers` | 5 | CRUD |
| Billers | `/billers` | 5 | CRUD |
| Warehouses | `/warehouses` | 5 | CRUD |
| Inventory | `/inventory` | 3 | List, get by product+warehouse, transfer |
| Stock Adjustments | `/stock-adjustments` | 3 | List, get, create |
| Expenses | `/expenses` | 5 | CRUD (with attachments) |
| Expense Categories | `/expense-categories` | 5 | CRUD |
| Team | `/team` | 7 | CRUD + reset password + stats |
| Dashboard | `/dashboard` | 4 | KPIs, top selling, recent sales, sales stats |
| Reports | `/reports` | 6 | POST endpoints: expenses, financial, inventory, sales, staff, tax |
| Audit Logs | `/audit-logs` | 2 | List, get by entity |
| Superadmin | `/superadmin` | ~3 | Platform-level admin |

---

## 5. RBAC Middleware Pattern

### Original (NestJS)

```typescript
@Controller('products')
@UseGuards(AuthGuard, RolesGuard)
export class ProductController {
  @Post()
  @Roles(UserRole.OWNER, UserRole.ADMIN, UserRole.MANAGER)
  async create(@Body() dto: CreateProductDto, @CompanyId() companyId: string) {}
  
  @Get(':id')
  @Roles(UserRole.OWNER, UserRole.ADMIN, UserRole.MANAGER, UserRole.OBSERVER)
  async findOne(@Param('id') id: string, @CompanyId() companyId: string) {}
}
```

### Target (ASP.NET Core)

```csharp
[Route("api/products")]
[ApiController]
[Authorize]
public class ProductsController : ControllerBase
{
    [HttpPost]
    [Authorize(Roles = "OWNER,ADMIN,MANAGER")]
    public async Task<ActionResult<ProductDto>> Create(CreateProductCommand command)
    {
        command.CompanyId = User.GetCompanyId();
        command.UserId = User.GetUserId();
        return Ok(await _mediator.Send(command));
    }

    [HttpGet("{id:guid}")]
    [Authorize(Roles = "OWNER,ADMIN,MANAGER,OBSERVER")]
    public async Task<ActionResult<ProductDto>> GetById(Guid id)
    {
        var query = new GetProductQuery { Id = id, CompanyId = User.GetCompanyId() };
        return Ok(await _mediator.Send(query));
    }
}
```

### Role Hierarchy
```
OWNER (highest) > ADMIN > MANAGER > OBSERVER (read-only)
```

Observers can only read — all CUD operations require OWNER/ADMIN/MANAGER.

---

## 6. Multi-Tenant Implementation

### ICurrentUserService (abstraction)

```csharp
// OmniBlox.Application/Common/Interfaces/ICurrentUserService.cs
public interface ICurrentUserService
{
    Guid UserId { get; }
    Guid CompanyId { get; }
    string Role { get; }
    string Email { get; }
}
```

### Infrastructure Implementation

```csharp
// OmniBlox.Infrastructure/Auth/CurrentUserService.cs
public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _http;

    public CurrentUserService(IHttpContextAccessor http)
        => _http = http;

    public Guid UserId => _http.HttpContext?.User.GetUserId() ?? throw new UnauthorizedException();
    public Guid CompanyId => _http.HttpContext?.User.GetCompanyId() ?? throw new UnauthorizedException();
    public string Role => _http.HttpContext?.User.GetRole() ?? string.Empty;
    public string Email => _http.HttpContext?.User.FindFirstValue(ClaimTypes.Email) ?? string.Empty;
}
```

### MediatR Pipeline (auto-inject companyId into commands/queries)

```csharp
// OmniBlox.Application/Common/Behaviors/TenantBehavior.cs
public class TenantBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ICurrentUserService _currentUser;

    public TenantBehavior(ICurrentUserService currentUser)
        => _currentUser = currentUser;

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken ct)
    {
        if (request is ICompanyScoped scoped)
        {
            scoped.CompanyId = _currentUser.CompanyId;
        }

        return await next();
    }
}
```

---

## 7. Email Workflows

The original project uses Nodemailer with Handlebars templates. All four email types must be replicated:

| Email Type | When Sent | Template Variables |
|---|---|---|
| OTP Verification | After signup (unverified) | `{userId}`, `{otp}` (6-digit, 10-min expiry) |
| Magic Link | POST `/auth/magic-login/request` | `{tokenUrl}` (15-min, single-use) |
| Invitation | POST /team (invite) | `{tokenUrl}`, `{companyName}` |
| Password Reset | POST `/auth/password-reset/request` | `{tokenUrl}` (15-min, single-use) |

**ASP.NET Core replacement:** `FluentEmail.Smtp` with Razor or `fluid` templates.

---

## 8. Validation & Logging

### NestJS → ASP.NET Core Equivalent

| NestJS Feature | ASP.NET Core Equivalent |
|---|---|
| `@Body() dto` + class-validator | MediatR + FluentValidation pipeline |
| `ValidationPipe({ whitelist: true })` | `FluentValidation` + `ValidationBehaviour` |
| Class-level `@UseGuards(AuthGuard, RolesGuard)` | Controller-level `[Authorize]` |
| `@CompanyId()` decorator | `ICurrentUserService.CompanyId` |
| Cache Module (`cache-manager`) | `IMemoryCache` / `IDistributedCache` |
| `CommonModule` logging middleware | Custom `ExceptionHandlingMiddleware` |
| Audit Log (manual service calls) | MediatR `AuditLogBehavior` pipeline |

---

## 9. Caching Strategy

The NestJS project caches aggressively. Preserve the same pattern:

| Cache Key Pattern | TTL | Invalidation Trigger |
|---|---|---|
| `products:{companyId}:list:{page}:{search}:...` | 120s | Create/Update/Delete product |
| `products:{companyId}:{id}` | 120s | Update/Delete product |
| `products:{companyId}:sku:{sku}` | 120s | Update/Delete product |
| `products:{companyId}:stats` | 120s | Create/Update/Delete product |
| Dashboard KPIs | 120s | Any related mutation |

Use `IMemoryCache` for single-server or `IDistributedCache` + Redis for scaled deployments.

---

## 10. Cross-Cutting Concerns (MediatR Pipeline Behaviors)

All behaviors run automatically on every command/query:

```
Request
  → LoggingBehavior        (log request type + timing)
  → ValidationBehavior     (FluentValidation validation)
  → TenantBehavior         (inject companyId)
  → AuditLogBehavior       (log mutations to audit_logs table)
  → CacheInvalidationBehavior  (invalidate cache keys)
  → Handler
  → Response
```

Register in `Application/DependencyInjection.cs`:

```csharp services.AddMediatR(cfg => {
    cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly);
    cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
    cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
    cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(TenantBehavior<,>));
    cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(AuditLogBehavior<,>));
});
```

---

## 11. Frontend Compatibility

The Next.js frontend uses this `ApiClient` class (`lib/api.ts`):

```typescript
const api = new ApiClient();

// Auth (cookie-based)
api.login(email, password);       // POST /auth/login
api.logout();                     // POST /auth/logout
api.getProfile();                 // GET /auth/me

// CRUD (cookie-based — credentials: "include")
api.get<T>("/products");
api.post<T>("/products", data);
api.put<T>("/products/:id", data);
api.delete("/products/:id");
```

**To adapt for JWT:**
1. Replace `credentials: "include"` with `Authorization: Bearer <token>` header
2. Store token in `localStorage` or memory (not HttpOnly cookie)
3. Add an axios/fetch interceptor to attach the token to every request:
   ```typescript
   headers: {
     "Authorization": `Bearer ${TokenManager.getAccessToken()}`,
     "Content-Type": "application/json",
   }
   ```
4. Update `TokenManager` to manage JWT instead of cookies
5. Remove `omniblox_logged_in` and `omniblox_workspace` cookie writes from `auth-context.tsx`
6. Update middleware to check for stored token instead of cookies

---

## 12. Testing Strategy

```
OmniBlox.Api.Tests/       # Integration tests (WebApplicationFactory)
OmniBlox.Application.Tests/ # Unit tests (MediatR handlers + validators)
OmniBlox.Domain.Tests/      # Entity logic tests
```

### Integration Test Example

```csharp
public class ProductsControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    [Fact]
    public async Task CreateProduct_AsObserver_ReturnsForbidden()
    {
        var client = _factory.WithRole(UserRole.OBSERVER).CreateClient();
        var response = await client.PostAsync("/api/products", JsonContent.Create(new { }));
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
}
```

---

## 13. Migration Order (Recommended)

### Phase 1 — Foundation (Days 1-3)
1. Create solution with Clean Architecture project structure
2. Set up EF Core with all entities and Fluent API configurations matching Prisma schema
3. Implement JWT auth (login, signup, middleware, claims extraction)
4. Register all pipeline behaviors (logging, validation, tenant, audit)
5. Verify: Auth flow works end-to-end

### Phase 2 — Core CRUD (Days 4-7)
6. Products module (36 endpoints — heaviest, do first)
7. Sales + Sale Returns module
8. Purchases + Purchase Returns module
9. Quotations module
10. Customers, Suppliers, Billers, Warehouses modules

### Phase 3 — Secondary (Days 8-10)
11. Inventory + Stock Adjustments + Stock Ledger
12. Expenses + Expense Categories
13. Deliveries module
14. Product Categories, Sub Categories, Brands, Units, Warranties, Variant Attributes

### Phase 4 — Cross-Cutting (Days 11-12)
15. Dashboard KPIs (4 endpoints)
16. Reports (6 POST endpoints)
17. Audit Logs
18. Superadmin
19. Team module

### Phase 5 — Polish (Days 13-14)
20. CSV/Excel import/export
21. Caching layer with cache invalidation
22. Frontend JWT migration
22. End-to-end testing
23. Docker Compose + deployment config

---

## 14. Project Startup Template

### Program.cs (Minimal API approach)

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddHttpContextAccessor();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
```

### appsettings.json

```json
{
  "ConnectionStrings": {
    "Postgres": "Host=localhost;Database=omniblox;Username=postgres;Password=postgres"
  },
  "Jwt": {
    "Secret": "<256-bit-secret>",
    "Issuer": "omniblox-api",
    "Audience": "omniblox-app",
    "ExpiryDays": 7
  },
  "Smtp": {
    "Host": "localhost",
    "Port": 1025,
    "From": "noreply@omniblox.com"
  },
  "Cors": {
    "Origins": ["http://localhost:3000"]
  }
}
```

---

## Key Differences Summary

| Concept | NestJS | ASP.NET Core |
|---|---|---|
| DI Container | NestJS DI | `Microsoft.Extensions.DependencyInjection` |
| ORM | Prisma | EF Core |
| Auth | Better Auth (cookies) | JWT Bearer |
| Validation | class-validator | FluentValidation |
| CQRS | Manual service methods | MediatR Commands/Queries |
| Pipeline/Interceptors | Guards, Interceptors | MediatR Pipeline Behaviors |
| Multi-tenant | Manual `@CompanyId()` filter | EF Core Global Query Filters |
| Caching | `@nestjs/cache-manager` | `IDistributedCache` / `IMemoryCache` |
| Email | `@nestjs-modules/mailer` + Handlebars | `FluentEmail` + Razor |
| API Docs | Swagger (via NestJS CLI) | Swashbuckle |
| Testing | Jest + supertest | xUnit + WebApplicationFactory |
| Logging | NestJS Logger | Serilog |
| Configuration | dotenv files | appsettings.json + env vars |
| Deployment | Render + Docker | Docker + any cloud |