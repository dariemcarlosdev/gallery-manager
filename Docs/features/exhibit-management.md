# Feature: Exhibit Management

> Reverse-engineered from `src/GalleryManager.Api/Features/Exhibits/`.

## What it does

Groups artworks into timed exhibits (a name + date range), lets a gallery assign artworks
to an exhibit, and reports how much revenue an exhibit generated from the artworks that
sold during it.

## Entity

`Exhibit` (`Features/Exhibits/Exhibit.cs`):

| Field | Type | Notes |
|---|---|---|
| `Id` | `int` | PK |
| `Name` | `string` | required |
| `StartDate` | `DateOnly` | |
| `EndDate` | `DateOnly` | |
| `Artworks` | nav. collection | one-to-many via `Artwork.ExhibitId` |

## Endpoints

| Method | Route | Handler | Behavior |
|---|---|---|---|
| `GET` | `/api/v1/exhibits` | `GetExhibits` | Paginated (`page`, `pageSize`), filterable by `name` (case-insensitive `ILike` partial match), sortable (`sortBy` ∈ `name`/`startDate`/`endDate`, `sortDirection`). Defaults to ascending `startDate` when no sort is given. Response includes `artworkCount`/`availableCount`/`onLoanCount`/`soldCount`, projected from the `Artworks` nav collection — so status changes made via `UpdateArtworkStatus` show up here. |
| `POST` | `/api/v1/exhibits/{id}/artworks/{artworkId}` | `AssignArtworkToExhibit` | Sets `Artwork.ExhibitId = id`. Returns `404` if the exhibit or artwork doesn't exist. Idempotent no-op (`204`) if the artwork is already assigned to that exhibit. |
| `GET` | `/api/v1/exhibits/{id}/revenue` | `GetExhibitRevenue` | Calls the Postgres function `get_exhibit_revenue(exhibit_id)` via `FromSqlInterpolated`/`SqlQuery<T>`, sums the returned sale-price rows, and returns a total plus a per-artwork line-item breakdown. Returns `404` if the exhibit doesn't exist. |

## Business rules

- **Assignment is idempotent**: re-assigning an artwork already linked to the same exhibit
  returns `204` without a write.
- **Revenue is derived, not stored**: `GetExhibitRevenue` always recomputes from the current
  `Sold` artworks in the exhibit via the DB function — there's no cached/denormalized total.
- An exhibit or artwork lookup miss on assignment/revenue always surfaces as RFC 7807 `404`,
  never a silent no-op.

## Related docs

- [../api-reference.md](../api-reference.md) — full request/response shapes
- [../data-access.md](../data-access.md) — EF Core mapping, Postgres schema, `get_exhibit_revenue` function definition
- [../rest-api-best-practices.md](../rest-api-best-practices.md) — pagination, sorting, filtering mechanics shared across features
- [artwork-inventory.md](artwork-inventory.md) — the `Artwork` entity this feature assigns/reports on
