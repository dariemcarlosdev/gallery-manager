# Frontend → Backend → Database Flow

> Cross-references: [Docs Index](INDEX.md) · [Architecture](architecture.md) · [API Reference](api-reference.md) · [Frontend](frontend.md)

This document traces the full downstream path a request takes from the Angular UI through the ASP.NET Core API to PostgreSQL and back. One endpoint — **GetArtworks** — is used as a worked example; all other endpoints follow the same pattern.

---

## System Overview

```
┌───────────────────┐     HTTPS/JSON      ┌─────────────────────────┐     Npgsql      ┌────────────────┐
│  Angular 19 SPA   │ ──────────────────▶  │  ASP.NET Core 8 API    │ ──────────────▶  │  PostgreSQL    │
│  (Vercel)         │ ◀──────────────────  │  (Render)              │ ◀──────────────  │  (Neon)        │
└───────────────────┘                      └─────────────────────────┘                  └────────────────┘
     UI Layer                                   API Layer                                 Data Layer
```

## Worked Example: GET /api/v1/artworks

### Layer 1 — Angular UI Component

**File:** `src/gallery-manager-web/src/app/pages/artworks-page/artworks-page.component.ts`

The page component calls the service on initialization to load artworks. The user can optionally select a status filter from the UI, which passes the filter value to the service method. The component holds the artwork list as a signal and renders it in the template.

### Layer 2 — Angular Service (HTTP Client)

**File:** `src/gallery-manager-web/src/app/services/artwork.service.ts`

`ArtworkService.getArtworks()` builds the HTTP GET request using Angular's `HttpClient`. It targets the base URL from the environment configuration (`src/gallery-manager-web/src/environments/environment.ts`), which resolves to:

- **Development:** `https://localhost:7080/api/v1`
- **Production:** `https://gallery-manager-api.onrender.com/api/v1`

The service appends query parameters (e.g., `?status=Available`) and maps the response through `PagedResponse<Artwork>`, extracting the `.data` array so downstream components receive a flat `Artwork[]`. This unwrapping isolates components from pagination concerns.

**Model file:** `src/gallery-manager-web/src/app/models/artwork.model.ts` — defines the `Artwork` interface matching the backend response shape.

**Pagination model:** `src/gallery-manager-web/src/app/models/paged-response.model.ts` — generic `PagedResponse<T>` interface matching the backend `PagedResponse<T>` record.

### Layer 3 — ASP.NET Core Minimal API Endpoint

**File:** `src/GalleryManager.Api/Features/Artworks/GetArtworks.cs`

The request hits the versioned route group registered in `src/GalleryManager.Api/Program.cs`. The middleware pipeline runs in order: CORS → HTTPS redirection → rate limiter → route matching.

Inside `GetArtworks.MapEndpoint()`, the handler:

1. Binds query parameters (`page`, `pageSize`, `sortBy`, `sortDirection`, `status`, `artist`, `medium`) from the URL
2. Builds an `IQueryable<Artwork>` against the `GalleryDbContext`
3. Applies filters — `status` via enum match, `artist`/`medium` via PostgreSQL `ILike` for case-insensitive partial matching
4. Applies sorting via the `ApplySorting()` extension (`src/GalleryManager.Api/Common/PaginationParams.cs`) using a whitelisted field dictionary
5. Applies pagination via `ToPagedResponseAsync()`, which runs `CountAsync()` for the total then `Skip/Take` for the page
6. Projects entity to response DTO and returns `200 OK` with a `PagedResponse<Response>` body

### Layer 4 — EF Core + PostgreSQL

**File:** `src/GalleryManager.Api/Data/GalleryDbContext.cs`

EF Core translates the `IQueryable` chain into a single SQL query. Npgsql sends it to the Neon PostgreSQL instance. The query includes:

- `WHERE` clauses for any active filters
- `ORDER BY` for the requested sort column
- `COUNT(*)` over the filtered set (for `totalCount`)
- `OFFSET` / `LIMIT` for the requested page

The `Artwork` entity (`src/GalleryManager.Api/Features/Artworks/Artwork.cs`) maps to the `artworks` table with columns configured via Fluent API in `GalleryDbContext.OnModelCreating()`.

### Layer 5 — Response Journey Back

PostgreSQL returns rows → EF Core materializes `Artwork` entities → the handler projects to `Response` records → ASP.NET Core serializes to JSON → the Angular `HttpClient` deserializes to `PagedResponse<Artwork>` → the service extracts `.data` → the component receives `Artwork[]` and re-renders.

---

## How Other Endpoints Follow the Same Pattern

| Endpoint | UI Component | Service | API Feature File | DB Interaction |
|----------|-------------|---------|-----------------|----------------|
| `POST /api/v1/artworks` | `artworks-page.component.ts` | `artwork.service.ts` → `createArtwork()` | `CreateArtwork.cs` | `db.Artworks.Add()` + `SaveChangesAsync()` |
| `PATCH /api/v1/artworks/{id}/status` | `artworks-page.component.ts` | `artwork.service.ts` → `updateStatus()` | `UpdateArtworkStatus.cs` | `FindAsync()` + property update + `SaveChangesAsync()` |
| `GET /api/v1/exhibits` | `exhibits-page.component.ts` | `exhibit.service.ts` → `getExhibits()` | `GetExhibits.cs` | Same pagination/sorting/filtering pattern |
| `POST /api/v1/exhibits/{id}/artworks/{artworkId}` | `exhibits-page.component.ts` | `exhibit.service.ts` → `assignArtwork()` | `AssignArtworkToExhibit.cs` | `FindAsync()` + navigation property update |
| `GET /api/v1/exhibits/{id}/revenue` | `exhibits-page.component.ts` | `exhibit.service.ts` → `getRevenue()` | `GetExhibitRevenue.cs` | `FromSqlInterpolated()` calling Postgres function `get_exhibit_revenue()` |

The revenue endpoint is the only one that bypasses LINQ — it calls a hand-written Postgres function via `FromSqlInterpolated`, demonstrating raw SQL comfort alongside ORM fluency. See `src/GalleryManager.Api/Data/Sql/001_create_get_exhibit_revenue_function.sql` for the function definition.

---

## Last synced
2026-07-07
