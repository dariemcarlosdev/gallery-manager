# Architecture

> Cross-references: [Docs Index](INDEX.md) · [API Reference](api-reference.md) · [Data Access](data-access.md) · [Frontend](frontend.md)

## System Design

Two deployables, one datastore:

```
┌─────────────────────┐        HTTPS/JSON        ┌──────────────────────────┐
│  Angular 19 SPA      │ ───────────────────────▶ │  ASP.NET Core 8 Minimal   │
│  gallery-manager-web │ ◀─────────────────────── │  API — GalleryManager.Api │
│  (Vercel)            │                          │  (Render/Fly.io/Azure —   │
└──────────────────────┘                          │   not yet picked)         │
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
| 2 | Minimal API route `POST /api/artworks` (`CreateArtwork.MapEndpoint`) |
| 3 | Inline `FluentValidation` validator runs against `Request` |
| 4 | On valid: new `Artwork` entity built, `db.SaveChangesAsync()` |
| 5 | `201 Created` with `Response` DTO returned |

No pipeline behaviors, no MediatR, no separate command/handler split — the whole flow lives in one file.

## Vertical Slice Architecture

Each feature folder is self-contained: entity, request/response records, validator, and endpoint registration all live in one or two files under `Features/{Domain}/`. There is no cross-cutting Controllers/Services/Repositories layer.

```
Features/Artworks/
  Artwork.cs              — entity + status enum
  GetArtworks.cs           — GET  /api/artworks
  CreateArtwork.cs         — POST /api/artworks
  UpdateArtworkStatus.cs   — PATCH /api/artworks/{id}/status

Features/Exhibits/
  Exhibit.cs
  GetExhibits.cs                  — GET  /api/exhibits
  AssignArtworkToExhibit.cs       — POST /api/exhibits/{id}/artworks/{artworkId}
  GetExhibitRevenue.cs            — GET  /api/exhibits/{id}/revenue
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
