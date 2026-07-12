# Feature Development Pattern — The Rule for Adding Any Feature

> Status: **Authoritative pattern**. Any new functionality — however small or large — MUST follow this end-to-end, Front-End → API → Database.
> Cross-references: [Docs Index](INDEX.md) · [Frontend→Backend Flow](frontend-backend-flow.md) · [Architecture](architecture.md) · [SDD Framework](sdd-framework/README-SDD-FRAMEWORK.md) · [API Best Practices](rest-api-best-practices.md)

This is the instruction set every LLM (and human) follows to add a feature to Gallery Manager. It operationalizes the [SDD principles](sdd-framework/SDD-QUICK-REFERENCE.md) across the five layers documented in [frontend-backend-flow.md](frontend-backend-flow.md). Read it top to bottom before writing any code.

---

## Phase 0 — SDD Gate (before any code)

You MUST clear this gate first. No file is touched until all four rows are satisfied.

| SDD principle | What you MUST do here |
|---|---|
| **Think before coding** | State 3–5 explicit assumptions about the request. If any is uncertain, ask — do not pick silently. |
| **Simplicity first** | Choose the minimum design for a 2-entity POC. No repository/CQRS/MediatR/microservices/auth framework (see [Scope & Non-Goals](INDEX.md#scope--non-goals)). |
| **Surgical changes** | List exactly which files you will create/modify. Every planned change must trace to the request. |
| **Goal-driven** | Write acceptance criteria as observable checks (e.g. `GET /api/v1/x returns 200 with PagedResponse`). These are your definition of done. |

**Spec step (proportional to size):** For a non-trivial feature, capture Problem / Goals / Acceptance criteria in the PR description or a short `Docs/specs/<feature>.md`. For a one-endpoint tweak, the assumptions + acceptance criteria above are enough. Do **not** create a doc hierarchy the POC doesn't need.

✅ Gate passed when: assumptions stated, plan (file list) proposed, acceptance criteria written.

---

## The Five Layers — build in this order

Follow the same downstream path a request takes. Mirror the canonical exemplar file for each layer instead of inventing new structure.

| # | Layer | You create / modify | Canonical file to mirror |
|---|---|---|---|
| 1 | **Database** | Entity + Fluent config; migration; raw SQL only if needed | `Features/{Domain}/{Entity}.cs`, `Data/GalleryDbContext.cs`, `Data/Sql/` |
| 2 | **API endpoint** | Vertical-slice feature file (`Request`/`Response`/`Validator`/`MapEndpoint`) | `Features/Artworks/GetArtworks.cs` (read) · `CreateArtwork.cs` (write) · `GetExhibitRevenue.cs` (raw SQL) |
| 3 | **Composition** | Register the endpoint | `Program.cs` — call `{Feature}.MapEndpoint(app)` in the versioned route group |
| 4 | **FE service + model** | Typed model + `HttpClient` method targeting `/api/v1` | `services/artwork.service.ts`, `models/artwork.model.ts`, `models/paged-response.model.ts` |
| 5 | **FE page** | Page component that calls the service and renders state | `pages/artworks-page/artworks-page.component.ts` |

### Layer rules (non-negotiable)

| Layer | MUST | MUST NOT |
|---|---|---|
| **DB / EF Core** | Configure columns via Fluent API in `OnModelCreating`; add a migration; use `FromSqlInterpolated` for raw SQL | Use `FromSqlRaw` with string concatenation; hide `DbContext` behind an abstraction |
| **API** | Keep the whole slice in one feature file; validate inline with a `Validator : AbstractValidator<Request>`; return typed `Results` + `PagedResponse<T>` for lists; follow [REST best practices](rest-api-best-practices.md) (versioning, RFC 7807, pagination/sorting/filtering) | Add Controllers/Services/Repositories/DTO folders; put validation in middleware |
| **Composition** | Register under the `/api/v1` route group in `Program.cs` | Add new middleware unless the feature truly requires it |
| **FE service** | Target base URL from `environments/environment.ts`; unwrap `PagedResponse<T>` to `T[]` so components stay pagination-agnostic | Hardcode URLs; leak pagination shape into components |
| **FE page** | Hold state in signals; call the service on init | Put HTTP logic in the component |

---

## Reusable "New Feature" Template

Copy this block per feature and fill every `<slot>`. An LLM completes it top-down, then implements layer by layer.

```
## Feature: <name>
Route:              <METHOD> /api/v1/<resource>
Domain:             <Artworks | Exhibits | new-domain>

# Phase 0 — SDD Gate
Assumptions:        <3–5 bullets>
Simplicity check:   <why this is the minimum design for a 2-entity POC>
Files to create:    <explicit list>
Files to modify:    <explicit list>
Acceptance criteria:<observable checks — status codes, response shape, UI behavior>

# Layer 1 — Database
Entity change:      <new entity | new column | nav property | none>
Migration name:     <PascalCase, or "none">
Raw SQL:            <function name + Data/Sql/ file, or "none">

# Layer 2 — API endpoint (mirror <exemplar file>)
Request record:     <fields>
Response record:    <fields>
Validator rules:    <field → rule>
Result codes:       <200/201/400/404 …>

# Layer 3 — Composition
Register in Program.cs: <Feature>.MapEndpoint(app)  ✔

# Layer 4 — FE service + model
Model:              models/<entity>.model.ts — <fields matching Response>
Service method:     services/<entity>.service.ts → <method>()

# Layer 5 — FE page
Component:          pages/<feature>-page/<feature>-page.component.ts
State + render:     <signals held, what the template shows>

# Verify (see checklist below)
```

---

## Worked Example — "Featured Artwork" flag

A minimal proof of concept that exercises all five layers. Filled template below; each layer names the exact file to mirror.

```
## Feature: Featured Artwork flag
Route:              GET /api/v1/artworks/featured
Domain:             Artworks

# Phase 0 — SDD Gate
Assumptions:        1. Artwork is the aggregate; a bool flag is enough (no separate entity).
                    2. "Featured" is curator-set; no auth needed for this read POC.
                    3. Default IsFeatured = false for existing rows.
                    4. Endpoint returns the same PagedResponse<ArtworkResponse> shape as GetArtworks.
                    5. FE shows a badge; no new page.
Simplicity check:   One nullable-free bool column + one filtered read. No new entity, no join. Minimum for a 2-entity POC.
Files to create:    Features/Artworks/GetFeaturedArtworks.cs
Files to modify:    Features/Artworks/Artwork.cs · Data/GalleryDbContext.cs · Program.cs ·
                    services/artwork.service.ts · models/artwork.model.ts · pages/artworks-page/artworks-page.component.(ts|html)
Acceptance criteria:- GET /api/v1/artworks/featured returns 200 with PagedResponse<ArtworkResponse>, only IsFeatured == true rows.
                    - Swagger lists the endpoint.
                    - Artworks page renders a "Featured" badge on flagged cards from live data.

# Layer 1 — Database
Entity change:      Add bool IsFeatured to Artwork
Migration name:     AddArtworkIsFeatured
Raw SQL:            none

# Layer 2 — API endpoint (mirror Features/Artworks/GetArtworks.cs)
Request record:     paging params (page, pageSize) — same as GetArtworks
Response record:    reuse ArtworkResponse (already has the fields) + IsFeatured
Validator rules:    page >= 1; pageSize 1..100 (copy GetArtworks validator)
Result codes:       200 (empty list allowed) · 400 (bad paging)

# Layer 3 — Composition
Register in Program.cs: GetFeaturedArtworks.MapEndpoint(app)  ✔ (in the /api/v1 group)

# Layer 4 — FE service + model
Model:              models/artwork.model.ts — add isFeatured: boolean
Service method:     services/artwork.service.ts → getFeatured() — unwrap PagedResponse to Artwork[]

# Layer 5 — FE page
Component:          pages/artworks-page/artworks-page.component.ts
State + render:     hold artworks signal; template shows a "Featured" badge when artwork.isFeatured

# Verify: run the checklist below.
```

| Layer | Mirror this file | One-line note |
|---|---|---|
| 1 DB | `Features/Artworks/Artwork.cs`, `Data/GalleryDbContext.cs` | Add the property; configure default `false` in `OnModelCreating`; scaffold `AddArtworkIsFeatured`. |
| 2 API | `Features/Artworks/GetArtworks.cs` | Copy the slice; add `.Where(a => a.IsFeatured)` before paging. |
| 3 Compose | `Program.cs` | One line in the versioned group — nothing else. |
| 4 FE service | `services/artwork.service.ts`, `models/artwork.model.ts` | Add `isFeatured` to model; `getFeatured()` hits `/api/v1/artworks/featured`, unwraps `PagedResponse`. |
| 5 FE page | `pages/artworks-page/artworks-page.component.ts` | Signal + badge in template; no HTTP in the component. |

---

## Verification & Completion Checklist

A feature is **not done** until every box is checked (Goal-driven principle).

- [ ] `dotnet build GalleryManager.sln` succeeds
- [ ] Migration applied (`dotnet ef database update --project src/GalleryManager.Api`) if the schema changed
- [ ] Endpoint appears in Swagger and returns the acceptance-criteria shape
- [ ] Frontend builds and the page renders live data from `/api/v1`
- [ ] Every changed line traces to the request (no unrelated edits, matching style)
- [ ] Only orphans **you** created were removed
- [ ] Docs updated: new row in [api-reference.md](api-reference.md) and the relevant [feature doc](features/); `INDEX.md` "Last synced" date bumped if structure changed

Any unchecked box → do not report complete; surface what failed.

---

## Scope Guardrail

Before adding anything not covered above, re-read [Scope & Non-Goals](INDEX.md#scope--non-goals). If a change only makes sense "at scale," it does not belong in this POC. When in doubt, choose the simpler option.

---

## Last synced
2026-07-12 — added "Featured Artwork" worked example (all 5 layers).
2026-07-09 — aligned with `frontend-backend-flow.md` (5-layer flow) and SDD framework v1.1.
