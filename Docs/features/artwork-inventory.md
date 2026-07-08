# Feature: Artwork Inventory

> Reverse-engineered from `src/GalleryManager.Api/Features/Artworks/`.

## What it does

Tracks the gallery's artwork stock: create new pieces, list/search/filter/sort them, and
transition a piece's status as it moves through the gallery's lifecycle (available → on
loan/in an exhibit → sold).

## Entity

`Artwork` (`Features/Artworks/Artwork.cs`):

| Field | Type | Notes |
|---|---|---|
| `Id` | `int` | PK |
| `Title` | `string` | required, max 200 |
| `Artist` | `string` | required, max 150 |
| `Medium` | `string` | required, max 100 |
| `Price` | `decimal` | required, ≥ 0 |
| `Status` | `ArtworkStatus` | `Available` \| `OnLoan` \| `Sold` — defaults to `Available` |
| `ExhibitId` | `int?` | set when the artwork is assigned to an exhibit |
| `IdempotencyKey` | `string?` | dedupe key for `POST /artworks` retries |
| `CreatedAtUtc` | `DateTime` | set at creation |

## Endpoints

| Method | Route | Handler | Behavior |
|---|---|---|---|
| `POST` | `/api/v1/artworks` | `CreateArtwork` | Validates via FluentValidation; honors optional `Idempotency-Key` header (≤ 64 chars) — returns the existing artwork (`200`) instead of creating a duplicate on a retried key, including a race-safe fallback if a unique-constraint violation occurs on `SaveChangesAsync`. Returns `201` with the created resource otherwise. |
| `GET` | `/api/v1/artworks` | `GetArtworks` | Paginated (`page`, `pageSize`), filterable (`status`, `artist` and `medium` via case-insensitive `ILike` partial match), sortable (`sortBy` ∈ `title`/`artist`/`price`/`createdAtUtc`, `sortDirection`). Defaults to newest-first when no sort is given. |
| `PATCH` | `/api/v1/artworks/{id}/status` | `UpdateArtworkStatus` | Transitions `Status`; when moving to `OnLoan` also sets `ExhibitId` from the request body. Returns `404` if the artwork doesn't exist, `400` for an unrecognized status string. |

## Business rules

- **Idempotent creation**: a retried `POST` with the same `Idempotency-Key` never creates a
  second row — it returns the original artwork.
- **Status/Exhibit coupling**: `ExhibitId` is only updated by `UpdateArtworkStatus` when the
  new status is `OnLoan`; other transitions leave the existing `ExhibitId` untouched.
- Price cannot be negative (enforced by the `CreateArtwork.Validator`).

## Related docs

- [../api-reference.md](../api-reference.md) — full request/response shapes
- [../data-access.md](../data-access.md) — EF Core mapping, Postgres schema
- [../rest-api-best-practices.md](../rest-api-best-practices.md) — idempotency, pagination, sorting, filtering mechanics shared across features
