# Gallery Manager API

[![API CI](https://github.com/dariemcarlosdev/gallery-manager/actions/workflows/api-ci.yml/badge.svg)](https://github.com/dariemcarlosdev/gallery-manager/actions/workflows/api-ci.yml)
[![.NET 8](https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![Angular 19](https://img.shields.io/badge/Angular-19-DD0031?logo=angular)](https://angular.dev/)
[![PostgreSQL](https://img.shields.io/badge/PostgreSQL-Neon-4169E1?logo=postgresql)](https://neon.tech/)
[![Deployed on Render](https://img.shields.io/badge/API-Render-46E3B7?logo=render)](https://gallery-manager-api.onrender.com/health)
[![Frontend on Vercel](https://img.shields.io/badge/Frontend-Vercel-000000?logo=vercel)](https://gallery-manager-henna.vercel.app)

Artwork and exhibit inventory management API for a gallery. Built as a functional POC demonstrating **Vertical Slice Architecture**, **REST API best practices**, and **full-stack .NET + Angular** delivery.

---

## Tech Stack

| Layer | Technology | Purpose |
|-------|-----------|---------|
| **API** | ASP.NET Core 8 Minimal API | Lightweight HTTP endpoints, no controller ceremony |
| **ORM** | EF Core 8 + Npgsql | Code-first migrations, LINQ queries |
| **Database** | PostgreSQL (Neon serverless) | Managed Postgres with connection pooling |
| **Validation** | FluentValidation 11.9 | Inline request validation per endpoint |
| **Versioning** | Asp.Versioning.Http 8.1 | URL-segment versioning (`/api/v1/`) |
| **Frontend** | Angular 19 (standalone components) | SPA with signals, lazy-loaded routes |
| **CI/CD** | GitHub Actions | Build + publish on push to `main` |
| **Hosting** | Render (API) + Vercel (SPA) | Auto-deploy from `main` branch |

## Architecture

### Vertical Slice

Each feature owns its entire stack — entity, request/response DTOs, validation, and endpoint registration — in a single file under `Features/{Domain}/`. No shared Controllers, Services, or Repositories folders.

```
src/GalleryManager.Api/
  Program.cs                          — composition root
  Common/PaginationParams.cs          — shared pagination, sorting, extension methods
  Data/
    GalleryDbContext.cs               — EF Core context + Fluent API config
    Sql/001_create_get_exhibit_revenue_function.sql
  Features/
    Artworks/
      Artwork.cs                      — entity + ArtworkStatus enum
      GetArtworks.cs                  — GET    /api/v1/artworks
      CreateArtwork.cs                — POST   /api/v1/artworks
      UpdateArtworkStatus.cs          — PATCH  /api/v1/artworks/{id}/status
    Exhibits/
      Exhibit.cs                      — entity
      GetExhibits.cs                  — GET    /api/v1/exhibits
      AssignArtworkToExhibit.cs       — POST   /api/v1/exhibits/{id}/artworks/{artworkId}
      GetExhibitRevenue.cs            — GET    /api/v1/exhibits/{id}/revenue
```

### Frontend (Angular SPA)

```
src/gallery-manager-web/src/app/
  models/         — TypeScript interfaces matching backend DTOs
  services/       — HttpClient wrappers per domain (ArtworkService, ExhibitService)
  pages/          — lazy-loaded route components (artworks-page, exhibits-page)
  environments/   — dev/prod API base URL configuration
```

### System Diagram

```
┌───────────────────┐     HTTPS/JSON      ┌─────────────────────────┐     Npgsql      ┌────────────────┐
│  Angular 19 SPA   │ ──────────────────▶  │  ASP.NET Core 8 API    │ ──────────────▶  │  PostgreSQL    │
│  (Vercel)         │ ◀──────────────────  │  (Render)              │ ◀──────────────  │  (Neon)        │
└───────────────────┘                      └─────────────────────────┘                  └────────────────┘
```

No API gateway, message queue, or cache layer — a 2-entity POC doesn't need them.

## Architectural Decisions

| Decision | Rationale |
|----------|-----------|
| **Vertical Slice over Clean Architecture layers** | Feature cohesion > layer separation at this scale. One folder = one feature = one place to look. |
| **Minimal API over MVC controllers** | Lighter boilerplate, direct route-to-handler mapping, aligns with .NET 8 direction. |
| **Direct DbContext injection** | No repository abstraction — 2 entities don't justify the indirection. |
| **FluentValidation inline** | Validator lives next to the handler it validates. No pipeline behavior middleware. |
| **Raw SQL for revenue calculation** | `GetExhibitRevenue` calls a Postgres function via `FromSqlInterpolated` — demonstrates SQL comfort alongside ORM fluency. |
| **URL-segment versioning** | `/api/v1/` is explicit, cache-friendly, and visible in every request. |
| **Offset pagination over cursor** | Simpler for a POC. Cursor pagination would be the next step at scale. |
| **Neon serverless Postgres** | Zero-ops managed database, free tier, no local Docker needed. |

## REST API Best Practices

Eight practices applied across all endpoints:

| Practice | Implementation |
|----------|---------------|
| **Pagination** | `PagedResponse<T>` wrapper with `page`, `pageSize`, `totalCount`, `totalPages`, `hasNextPage`, `hasPreviousPage` |
| **Sorting** | `sortBy` + `sortDirection` query params with whitelisted field dictionaries per endpoint |
| **Filtering** | Domain-specific query filters (`?status=`, `?artist=`, `?medium=`, `?name=`) using PostgreSQL `ILike` |
| **Idempotency** | `Idempotency-Key` HTTP header on `POST /artworks` — returns existing resource on duplicate key |
| **API Versioning** | URL segment `/api/v1/` via `Asp.Versioning.Http` |
| **RFC 7807 Errors** | All errors return `application/problem+json` via `Results.Problem()` |
| **Rate Limiting** | Fixed window (100 req/min) with `429 Too Many Requests` + `Retry-After` header |
| **OpenAPI Metadata** | `.Produces<T>()` / `.ProducesProblem()` annotations on all endpoints |

Full details: [Docs/rest-api-best-practices.md](Docs/rest-api-best-practices.md)

## API Endpoints

All routes are versioned under `/api/v1/`.

| Method | Route | Description |
|--------|-------|-------------|
| `GET` | `/api/v1/artworks` | List artworks (paginated, sortable, filterable) |
| `POST` | `/api/v1/artworks` | Create artwork (idempotency-key support) |
| `PATCH` | `/api/v1/artworks/{id}/status` | Update artwork status |
| `GET` | `/api/v1/exhibits` | List exhibits (paginated, sortable, filterable) |
| `POST` | `/api/v1/exhibits/{id}/artworks/{artworkId}` | Assign artwork to exhibit |
| `GET` | `/api/v1/exhibits/{id}/revenue` | Get exhibit revenue (Postgres function) |
| `GET` | `/health` | Health check |

Full reference: [Docs/api-reference.md](Docs/api-reference.md)

## Data Model

Two entities with a one-to-many relationship:

- **Artwork** — `Id`, `Title`, `Artist`, `Medium`, `Price`, `Status` (Available/OnLoan/Sold), `ExhibitId?`, `IdempotencyKey?`, `CreatedAtUtc`
- **Exhibit** — `Id`, `Name`, `StartDate`, `EndDate`, `Artworks` (navigation)

`get_exhibit_revenue(exhibit_id)` is a Postgres function that calculates total revenue from sold artworks in an exhibit. Defined in `Data/Sql/001_create_get_exhibit_revenue_function.sql`.

## Local Setup

**Prerequisites:** .NET 8 SDK, Node.js 18+, `dotnet-ef` global tool

1. Clone and restore:
   ```bash
   git clone https://github.com/dariemcarlosdev/gallery-manager.git
   cd gallery-manager
   dotnet restore GalleryManager.sln
   ```

2. Configure database connection (Neon Postgres — no local Docker needed):
   ```bash
   dotnet user-secrets set "ConnectionStrings:GalleryDb" "<your-neon-connection-string>" \
     --project src/GalleryManager.Api
   ```

3. Apply migrations:
   ```bash
   dotnet ef database update --project src/GalleryManager.Api
   ```

4. Run the revenue function script against your DB (one-time):
   `src/GalleryManager.Api/Data/Sql/001_create_get_exhibit_revenue_function.sql`

5. Start the API:
   ```bash
   dotnet run --project src/GalleryManager.Api
   ```
   Swagger UI at `/swagger` (dev environment only).

6. Start the frontend:
   ```bash
   cd src/gallery-manager-web
   npm install
   npx ng serve
   ```
   Dev server defaults to `http://localhost:4200`. CORS is pre-configured for ports 4200, 4301, 4302.

## CI/CD Pipeline

GitHub Actions workflow (`.github/workflows/api-ci.yml`):

1. **Build & Publish** — Restore → Build (Release) → Publish → Upload artifact
2. **Deploy** — On `main` push, triggers Render deploy hook (Render builds from Dockerfile server-side)

Frontend auto-deploys to Vercel on push to `main` via Vercel's GitHub integration.

## Deployment

| Component | Host | URL |
|-----------|------|-----|
| API | Render (Docker) | `https://gallery-manager-api.onrender.com` |
| Frontend | Vercel | `https://gallery-manager-henna.vercel.app` |
| Database | Neon | Serverless PostgreSQL (connection-pooled) |

## Documentation

| Doc | Covers |
|-----|--------|
| [Docs/INDEX.md](Docs/INDEX.md) | Documentation index and scope rules |
| [Docs/architecture.md](Docs/architecture.md) | System design, request flow, vertical slice pattern |
| [Docs/api-reference.md](Docs/api-reference.md) | Full endpoint reference with request/response shapes |
| [Docs/rest-api-best-practices.md](Docs/rest-api-best-practices.md) | 8 REST practices with file references |
| [Docs/frontend-backend-flow.md](Docs/frontend-backend-flow.md) | End-to-end request flow from UI to DB |
| [Docs/data-access.md](Docs/data-access.md) | EF Core model, Postgres schema, revenue function |
| [Docs/frontend.md](Docs/frontend.md) | Angular app structure, services, design tokens |
| [Docs/deployment.md](Docs/deployment.md) | CI/CD and hosting setup |

## Design Rationale (Interview Context)

- **Vertical Slice** mirrors the job description directly — "here's everything for [feature] in one place"
- **Minimal API** — lightweight, no MVC ceremony, aligns with modern .NET
- **Raw SQL function** in `GetExhibitRevenue` — demonstrates SQL fluency (the JD's #1 skill)
- **EF Core Code-First** for CRUD — shows ORM proficiency without pretending raw SQL is the only tool
- **REST best practices** — pagination, sorting, filtering, idempotency, versioning, RFC 7807 errors, rate limiting — production-grade API design on a POC-scale project
- **Full-stack delivery** — API + Angular SPA + CI/CD + cloud hosting, end to end
