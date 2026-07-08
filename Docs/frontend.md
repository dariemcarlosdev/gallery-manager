# Frontend

> Cross-references: [Docs Index](INDEX.md) · [Architecture](architecture.md) · [API Reference](api-reference.md)

## Stack

Angular 19, standalone components (no `NgModule`), routing enabled, SCSS, no SSR. Scaffolded via `@angular/cli@19` — pinned because this machine's Node version (22.19) is below what the latest Angular CLI requires.

## Structure

```
src/gallery-manager-web/src/
  environments/
    environment.ts               — prod placeholder apiUrl
    environment.development.ts   — apiUrl: https://localhost:7080/api/v1
  app/
    app.config.ts                — provideHttpClient(), provideRouter()
    models/
      artwork.model.ts           — Artwork, ArtworkStatus union, Create/UpdateStatus DTOs
      exhibit.model.ts           — Exhibit, ExhibitRevenue, ExhibitRevenueLine
    services/
      artwork.service.ts         — getArtworks, createArtwork, updateStatus
      exhibit.service.ts         — getExhibits, assignArtwork, getRevenue
    pages/
      artworks-page/             — list + status filter + create form + inline status update
      exhibits-page/             — list + assign-artwork action + revenue view
    app.routes.ts                 — lazy-loaded routes: /artworks (default), /exhibits
```

Two routed pages, no shared component library — each page owns its template/styles like the backend's vertical slices. Shared visual primitives (`.btn`, `.badge`, `.panel`) live in `src/styles.scss` since there are only two pages; not worth a components/ folder yet.

Models mirror the backend `Response`/`Request` records field-for-field — see [API Reference](api-reference.md) for the exact shapes.

## Services → Endpoints

All services target the versioned base URL (`/api/v1/`) configured in `src/environments/environment*.ts`. Collection endpoints return `PagedResponse<T>` — services unwrap `.data` so components receive flat arrays.

| Service method | Endpoint |
|---|---|
| `ArtworkService.getArtworks(status?)` | `GET /api/v1/artworks` (paginated, sortable, filterable) |
| `ArtworkService.createArtwork(req)` | `POST /api/v1/artworks` (idempotency-key support) |
| `ArtworkService.updateStatus(id, req)` | `PATCH /api/v1/artworks/{id}/status` |
| `ExhibitService.getExhibits()` | `GET /api/v1/exhibits` (paginated, sortable, filterable) |
| `ExhibitService.assignArtwork(exhibitId, artworkId)` | `POST /api/v1/exhibits/{exhibitId}/artworks/{artworkId}` |
| `ExhibitService.getRevenue(exhibitId)` | `GET /api/v1/exhibits/{exhibitId}/revenue` |

Pagination model: `src/app/models/paged-response.model.ts` — generic `PagedResponse<T>` interface matching backend shape.

Env swap for dev vs prod handled by `angular.json` → `architect.build.configurations.development.fileReplacements` (not present by default in Angular 17+ scaffolds — added manually).

## Design

Editorial gallery aesthetic: warm paper background, `Fraunces` display serif for headings, `Work Sans` for body, oxblood accent (`--color-accent`) for primary actions. Tokens live as CSS custom properties in `src/styles.scss` (colors, spacing scale, radius, shadow) — no design-token library, just `:root` variables.

State is signals (`signal`/`update`), no NgRx — two pages don't need a store. Forms are template-driven (`FormsModule` + `ngModel`) rather than Reactive Forms, since each form is 3-4 fields with no cross-field validation.

## Verified

Build (`ng build --configuration development`) is clean. Manually verified via dev server: routing between `/artworks` and `/exhibits`, accessible label/control structure (checked via a11y snapshot), and no horizontal overflow at 375px and desktop widths. Not verified against a live API/database in this environment — no Postgres instance was configured here, so only the "API unreachable" error-banner path was exercised end-to-end.

## Not built yet

Delete-artwork and edit-exhibit are out of scope — the backend doesn't expose those endpoints either (see [API Reference](api-reference.md)).

## Running

```
cd src/gallery-manager-web
npm install
npx ng serve
```
CORS on the API side already allows `http://localhost:4200` (Angular's default dev port) — see `Program.cs`.

Local dev server config also lives at `.claude/launch.json` (`gallery-manager-web`, port 4200, `cwd: src/gallery-manager-web`) for tooling that drives `ng serve` directly.

## Last synced
2026-07-07
