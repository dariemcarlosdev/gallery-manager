# API Reference

> Cross-references: [Docs Index](INDEX.md) · [Architecture](architecture.md) · [Data Access](data-access.md) · [REST Best Practices](rest-api-best-practices.md)
> Base path: `/api/v1` (all endpoints versioned via URL segment)
> Swagger UI: `/swagger` (Development environment only)

## Artworks

| Method | Route | Description |
|--------|-------|-------------|
| GET | `/api/v1/artworks` | List artworks (paginated, sortable, filterable) |
| POST | `/api/v1/artworks` | Create artwork |
| PATCH | `/api/v1/artworks/{id}/status` | Update artwork status |

### GET /api/v1/artworks

**Query parameters:**

| Param | Type | Default | Description |
|-------|------|---------|-------------|
| `page` | int | 1 | Page number |
| `pageSize` | int | 20 | Items per page (max 100) |
| `sortBy` | string | — | Sort field: `title`, `artist`, `price`, `createdAtUtc` |
| `sortDirection` | string | `asc` | `asc` or `desc` |
| `status` | string | — | Filter by `ArtworkStatus` enum: `Available`, `OnLoan`, `Sold` |
| `artist` | string | — | Case-insensitive partial match on artist name |
| `medium` | string | — | Case-insensitive partial match on medium |

**Response:** `200 OK` — `PagedResponse<Response>`

```json
{
  "data": [{ "id": 1, "title": "...", "artist": "...", "medium": "...", "price": 0.0, "status": "Available", "exhibitId": null }],
  "page": 1,
  "pageSize": 20,
  "totalCount": 42,
  "totalPages": 3,
  "hasNextPage": true,
  "hasPreviousPage": false
}
```

**File:** `src/GalleryManager.Api/Features/Artworks/GetArtworks.cs`

### POST /api/v1/artworks

**Request body:** `{ title, artist, medium, price }`

**Headers:** `Idempotency-Key` (optional, string, max 64 chars) — if provided and a matching key exists, returns `200` with the existing resource instead of creating a duplicate.

**Validation:** title required (max 200), artist required (max 150), medium required (max 100), price ≥ 0.

**Response:** `201 Created` — `{ id, title, artist, medium, price, status }` (or `200` on idempotent hit). Validation failure: `422` with `ValidationProblem`.

**File:** `src/GalleryManager.Api/Features/Artworks/CreateArtwork.cs`

### PATCH /api/v1/artworks/{id}/status

**Request body:** `{ status, exhibitId? }`

**Validation:** `status` must parse to `ArtworkStatus` enum. `exhibitId` only applied when transitioning to `OnLoan`.

**Response:** `204 No Content`. `404` if artwork not found (RFC 7807). `400` if invalid status (RFC 7807 with valid values listed in `detail`).

**File:** `src/GalleryManager.Api/Features/Artworks/UpdateArtworkStatus.cs`

---

## Exhibits

| Method | Route | Description |
|--------|-------|-------------|
| GET | `/api/v1/exhibits` | List exhibits (paginated, sortable, filterable) |
| POST | `/api/v1/exhibits/{exhibitId}/artworks/{artworkId}` | Assign artwork to exhibit |
| GET | `/api/v1/exhibits/{exhibitId}/revenue` | Get exhibit revenue |

### GET /api/v1/exhibits

**Query parameters:**

| Param | Type | Default | Description |
|-------|------|---------|-------------|
| `page` | int | 1 | Page number |
| `pageSize` | int | 20 | Items per page (max 100) |
| `sortBy` | string | — | Sort field: `name`, `startDate`, `endDate` |
| `sortDirection` | string | `asc` | `asc` or `desc` |
| `name` | string | — | Case-insensitive partial match on exhibit name |

**Response:** `200 OK` — `PagedResponse<Response>` with shape `{ id, name, startDate, endDate, artworkCount, availableCount, onLoanCount, soldCount }` per item.

**File:** `src/GalleryManager.Api/Features/Exhibits/GetExhibits.cs`

### POST /api/v1/exhibits/{exhibitId}/artworks/{artworkId}

Assigns an artwork to an exhibit. Naturally idempotent — if the artwork is already assigned to the same exhibit, returns `204` without error.

**Response:** `204 No Content`. `404` (RFC 7807) if exhibit or artwork not found.

**File:** `src/GalleryManager.Api/Features/Exhibits/AssignArtworkToExhibit.cs`

### GET /api/v1/exhibits/{exhibitId}/revenue

Calls the Postgres function `get_exhibit_revenue(exhibit_id)` via `FromSqlInterpolated` — see [Data Access](data-access.md).

**Response:** `200 OK` — `{ exhibitId, total, lines: [{ artworkTitle, salePrice }] }`. `404` (RFC 7807) if exhibit not found.

**File:** `src/GalleryManager.Api/Features/Exhibits/GetExhibitRevenue.cs`

---

## Health

| Method | Route | Response |
|--------|-------|----------|
| GET | `/health` | `{ status: "ok" }` |

Not versioned — lives outside the `/api/v1/` route group.

## Error Responses

All errors use RFC 7807 Problem Details (`application/problem+json`):

| Status | When | Shape |
|--------|------|-------|
| `400` | Invalid request data (e.g., bad enum value) | `Results.Problem()` with `detail` explaining valid values |
| `404` | Resource not found | `Results.Problem()` with `detail` naming the missing resource |
| `422` | Validation failure | `Results.ValidationProblem()` with per-field error dictionary |
| `429` | Rate limit exceeded (100 req/min) | `Results.Problem()` + `Retry-After: 60` header |

## Last synced
2026-07-08 — `GetExhibits` response extended with `artworkCount`/`availableCount`/`onLoanCount`/`soldCount` (exhibit↔artwork EF relation).
