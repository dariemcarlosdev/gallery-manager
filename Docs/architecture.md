# Architecture

> Cross-references: [Docs Index](INDEX.md) · [API Reference](api-reference.md) · [Data Access](data-access.md) · [Frontend](frontend.md) · [REST Best Practices](rest-api-best-practices.md) · [Frontend–Backend Flow](frontend-backend-flow.md)

## System Design

Two deployables, one datastore:

```
┌─────────────────────┐        HTTPS/JSON        ┌──────────────────────────┐
│  Angular 19 SPA      │ ───────────────────────▶ │  ASP.NET Core 8 Minimal   │
│  gallery-manager-web │ ◀─────────────────────── │  API — GalleryManager.Api │
│  (Vercel)            │                          │  (Render — Docker)        │
└──────────────────────┘                          │                           │
                                                    └────────────┬─────────────┘
                                                                 │ Npgsql (EF Core)
                                                                 ▼
                                                    ┌──────────────────────────┐
                                                    │  PostgreSQL (Neon)       │
                                                    │  artworks, exhibits      │
                                                    │  + get_exhibit_revenue() │
                                                    └──────────────────────────┘
```

No API gateway, no message queue, no cache layer — a 2-table POC doesn't need them.

## Request Flow (example: create an artwork)

| Step | Component |
|---|---|
| 1 | Angular `ArtworkService.createArtwork()` → `HttpClient.post` |
| 2 | Middleware pipeline: CORS → HTTPS redirect → rate limiter → route matching |
| 3 | Versioned route group `/api/v1/` → `POST /api/v1/artworks` (`CreateArtwork.MapEndpoint`) |
| 4 | Optional `Idempotency-Key` header checked — if duplicate, return existing resource (200) |
| 5 | Inline `FluentValidation` validator runs against `Request` |
| 6 | On valid: new `Artwork` entity built, `db.SaveChangesAsync()` |
| 7 | `201 Created` with `Response` DTO returned |

No pipeline behaviors, no MediatR, no separate command/handler split — the whole flow lives in one file. For the full downstream trace, see [Frontend–Backend Flow](frontend-backend-flow.md).

## Vertical Slice Architecture

Each feature folder is self-contained: entity, request/response records, validator, and endpoint registration all live in one or two files under `Features/{Domain}/`. There is no cross-cutting Controllers/Services/Repositories layer.

```
Common/
  PaginationParams.cs              — shared PagedRequest, PagedResponse<T>, sorting/pagination extensions

Features/Artworks/
  Artwork.cs              — entity + ArtworkStatus enum + IdempotencyKey
  GetArtworks.cs           — GET    /api/v1/artworks  (paginated, sortable, filterable)
  CreateArtwork.cs         — POST   /api/v1/artworks  (idempotency-key support)
  UpdateArtworkStatus.cs   — PATCH  /api/v1/artworks/{id}/status

Features/Exhibits/
  Exhibit.cs
  GetExhibits.cs                  — GET    /api/v1/exhibits  (paginated, sortable, filterable)
  AssignArtworkToExhibit.cs       — POST   /api/v1/exhibits/{id}/artworks/{artworkId}
  GetExhibitRevenue.cs            — GET    /api/v1/exhibits/{id}/revenue
```

**Why**: mirrors "vertical slice" directly for the interview — point at one folder, that's the whole feature. See [Data Access](data-access.md) for why `GetExhibitRevenue` calls raw SQL instead of LINQ.

## Deliberately absent (and why that's fine here)

| Missing | Why it's OK for this POC |
|---|---|
| Auth / `[Authorize]` | No real users; adding auth would be scope-inflation for a demo |
| Repository/Service layer | 2 entities, `DbContext` injected directly is simpler and equally correct |
| Tests | CI has a test step commented out, ready to enable once a test project exists |
| CQRS/MediatR | Minimal API handlers are the whole "command" — no benefit at this size |

## Last synced
2026-07-07 — Postgres now hosted on Neon
