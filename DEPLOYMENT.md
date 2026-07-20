# Deployment Guide — OmniBlox

## Architecture

| Component | Platform | Notes |
|-----------|----------|-------|
| Backend (OmniBlox.Api) | Render (Docker) | .NET 10 ASP.NET Core Web API |
| Frontend (OmniBlox.Web) | Vercel | Next.js app |
| Database | Render Managed Postgres | |

---

## Prerequisites

- Docker (for local build verification)
- .NET 10 SDK (for local migration runs)
- PostgreSQL client (psql) for connecting to the database

---

## Required Environment Variables

Set these on the **Render** service (backend) and **Vercel** (frontend).

### Backend (Render — Environment Variables)

| Variable | Purpose |
|----------|---------|
| `ConnectionStrings__Postgres` | Full Postgres connection string |
| `Jwt__Secret` | JWT signing key (min 32 chars, use `openssl rand -base64 48`) |
| `Jwt__Issuer` | JWT issuer (e.g. `omniblox-api`) |
| `Jwt__Audience` | JWT audience (e.g. `omniblox-app`) |
| `Cors__Origins` | Comma-separated frontend URLs (e.g. `https://omniblox.vercel.app` or `https://site1.com,https://site2.com`) |
| `ASPNETCORE_ENVIRONMENT` | `Production` |

**Secret rotation notes:**
- The Postgres password `1234` was previously committed in git history. **Rotate it** — change the password on your Postgres instance immediately.
- The JWT secret `PLACEHOLDER-at-least-32-characters-for-hmac-security` was also in git history. **Generate a new one** with `openssl rand -base64 48` before going live.

### Frontend (Vercel — Environment Variables)

| Variable | Purpose |
|----------|---------|
| `NEXT_PUBLIC_API_URL` | Backend API URL (e.g. `https://omniblox-api.onrender.com`) |
| `API_URL` | Same as above, for SSR calls (not prefixed `NEXT_PUBLIC_`) |

---

## Database Migrations

### Development

Migrations run automatically on startup via `db.Database.Migrate()` when `ASPNETCORE_ENVIRONMENT=Development`.

### Production

**Do NOT use auto-migrate in production.** Migration is gated behind Development env only.

**Render pre-deploy command** — set this in your Render service's settings:

```
dotnet ef database update --project src/OmniBlox.Infrastructure --connection-string "$ConnectionStrings__Postgres"
```

This runs after the Docker build completes but before the new version starts serving traffic, ensuring zero-downtime migrations.

### Adding a new migration (development)

```bash
cd src/OmniBlox.Infrastructure
dotnet ef migrations add <MigrationName>
```

After creating the migration, commit it and deploy. The pre-deploy command will apply it in production.

---

## Docker

### Build locally

```bash
docker build -f src/OmniBlox.Api/Dockerfile -t omniblox-api .
```

### Run locally

```bash
docker run -p 8080:8080 \
  -e ConnectionStrings__Postgres="Host=host.docker.internal;Port=5432;Database=Omniblox;Username=postgres;Password=CHANGE_ME" \
  -e Jwt__Secret="local-dev-at-least-32-characters-for-hmac!!" \
  -e Jwt__Issuer="omniblox-api" \
  -e Jwt__Audience="omniblox-app" \
  -e Cors__Origins__0="http://localhost:3000" \
  -e ASPNETCORE_ENVIRONMENT=Production \
  omniblox-api
```

### Health check

The API exposes `GET /health` returning `{ "status": "healthy", "timestamp": "..." }` with no auth required. Render uses this to monitor service health.

---

## CORS

CORS origins are read from the `Cors:Origins` configuration array.

- Local dev: `["http://localhost:3000"]` (in `appsettings.json`)
- Production: set via `Cors__Origins` env var (e.g. `https://omniblox.vercel.app`)

**Security note:** If you add Vercel preview deployment support later, do NOT use a blanket `*.vercel.app` wildcard since `AllowCredentials()` is enabled. Instead, use a scoped pattern matching your project's preview URLs (e.g. `omniblox-*-username.vercel.app`) once the exact Vercel URL format is confirmed.

---

## Deploy Checklist

1. [ ] Postgres password rotated (the old one was in git history)
2. [ ] JWT secret rotated (generate new with `openssl rand -base64 48`)
3. [ ] Backend env vars set on Render (see table above)
4. [ ] Frontend env vars set on Vercel (see table above)
5. [ ] Render pre-deploy command configured for migrations
6. [ ] Render health check path set to `/health`
7. [ ] Render service port set to `8080` (matches Dockerfile EXPOSE)
8. [ ] Vercel project linked to frontend directory (`src/OmniBlox.Web`)
9. [ ] Vercel rewrites/functions configured to handle client-side routing
