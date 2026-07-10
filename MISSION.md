# Mission: ASP.NET Core for OmniBlox ERP

## Why
I built a full-scale multi-tenant ERP platform (OmniBlox) with Next.js + NestJS + Prisma, and I want to rebuild it in ASP.NET Core. The goal is to deeply understand ASP.NET Core by implementing every layer of the real app — not following toy tutorials. When I'm done, I'll own a production-grade .NET codebase and be able to build any backend in the .NET ecosystem.

## Success looks like
- All 30 database models migrated from Prisma to EF Core, with multitenant query filters
- All ~100 REST endpoints working with JWT auth, RBAC, validation, and audit logging
- Clean Architecture with CQRS/MediatR, pipeline behaviors, and vertical slice organization
- Frontend (Next.js) consuming JWT from the new .NET API
- A running OmniBlox instance with Docker Compose

## Constraints
- Learning must happen alongside implementation — every concept is learned by building it
- Prefer short video explanations (<15 min) for new concepts
- Have basic C# knowledge but no ASP.NET experience
- Project skeleton already exists with packages installed; focus is on writing the real code

## Out of scope
- Advanced DevOps/cloud deployment (for now)
- Microservices — this is a clean monolith first
- Blazor or Razor Pages — API-only focus
