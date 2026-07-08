# REST API Best Practices

> Cross-references: [Docs Index](INDEX.md) · [API Reference](api-reference.md) · [Architecture](architecture.md)

Nine REST best practices applied to every endpoint in the Gallery Manager API. Each section names the practice, where it lives in the code, and which endpoints use it.

---

## 1. Pagination

All collection endpoints return a `PagedResponse<T>` wrapper instead of bare arrays. Clients pass `page` and `pageSize` query parameters; the API enforces a maximum of 100 items per page (default 20).

Response shape includes `data`, `page`, `pageSize`, `totalCount`, `totalPages`, `hasNextPage`, and `hasPreviousPage`.

| Where | File |
|-------|------|
| Shared types & extension | `src/GalleryManager.Api/Common/PaginationParams.cs` — `PagedRequest`, `PagedResponse<T>`, `ToPagedResponseAsync()` |
| Artworks | `src/GalleryManager.Api/Features/Artworks/GetArtworks.cs` |
| Exhibits | `src/GalleryManager.Api/Features/Exhibits/GetExhibits.cs` |
| Frontend model | `src/gallery-manager-web/src/app/models/paged-response.model.ts` |

## 2. Sorting

Collection endpoints accept `sortBy` and `sortDirection` (asc/desc) query parameters. Each endpoint declares a whitelist dictionary that maps allowed field names to EF Core expressions, preventing arbitrary column injection.

| Where | File |
|-------|------|
| `ApplySorting()` extension | `src/GalleryManager.Api/Common/PaginationParams.cs` |
| Artworks (title, artist, price, createdAtUtc) | `src/GalleryManager.Api/Features/Artworks/GetArtworks.cs` |
| Exhibits (name, startDate, endDate) | `src/GalleryManager.Api/Features/Exhibits/GetExhibits.cs` |

## 3. Filtering

Collection endpoints accept domain-specific query filters. Text filters use PostgreSQL `ILike` for case-insensitive partial matching.

| Endpoint | Filters | File |
|----------|---------|------|
| `GET /api/v1/artworks` | `?status=`, `?artist=`, `?medium=` | `src/GalleryManager.Api/Features/Artworks/GetArtworks.cs` |
| `GET /api/v1/exhibits` | `?name=` | `src/GalleryManager.Api/Features/Exhibits/GetExhibits.cs` |

## 4. Idempotency

`POST /api/v1/artworks` accepts an optional `Idempotency-Key` HTTP header (string, max 64 chars). If a key is provided and an artwork with that key already exists, the endpoint returns `200 OK` with the existing resource instead of creating a duplicate.

The key is stored as a nullable column on the `Artwork` entity with a filtered unique index (non-null only).

| Where | File |
|-------|------|
| Header handling + duplicate check | `src/GalleryManager.Api/Features/Artworks/CreateArtwork.cs` |
| Entity property | `src/GalleryManager.Api/Features/Artworks/Artwork.cs` |
| Index configuration | `src/GalleryManager.Api/Data/GalleryDbContext.cs` |
| DB migration | `src/GalleryManager.Api/Migrations/*_AddIdempotencyKeyToArtworks.cs` |

`POST /api/v1/exhibits/{id}/artworks/{artworkId}` is naturally idempotent — assigning an artwork already assigned to the same exhibit returns `204` without error (see `src/GalleryManager.Api/Features/Exhibits/AssignArtworkToExhibit.cs`).

## 5. API Versioning

All endpoints are served under `/api/v1/` using URL-segment versioning. The version set is declared in `Program.cs` and all feature endpoints register under the versioned route group.

| Where | File |
|-------|------|
| Version configuration & route group | `src/GalleryManager.Api/Program.cs` — `AddApiVersioning()`, `NewApiVersionSet()`, `MapGroup("/api/v{version:apiVersion}")` |
| NuGet package | `Asp.Versioning.Http 8.1.1` in `src/GalleryManager.Api/GalleryManager.Api.csproj` |
| Frontend base URL | `src/gallery-manager-web/src/environments/environment.ts` (`/api/v1`), `environment.development.ts` |

## 6. Consistent Error Responses (RFC 7807)

All error responses use ASP.NET Core's `Results.Problem()` to return `application/problem+json` with `type`, `title`, `status`, and `detail` fields. Validation errors continue to use `Results.ValidationProblem()` which also conforms to RFC 7807.

| Endpoint | Error scenarios | File |
|----------|----------------|------|
| `PATCH /api/v1/artworks/{id}/status` | Invalid status enum, not found | `src/GalleryManager.Api/Features/Artworks/UpdateArtworkStatus.cs` |
| `POST /api/v1/exhibits/{id}/artworks/{artworkId}` | Exhibit or artwork not found | `src/GalleryManager.Api/Features/Exhibits/AssignArtworkToExhibit.cs` |
| `GET /api/v1/exhibits/{id}/revenue` | Exhibit not found | `src/GalleryManager.Api/Features/Exhibits/GetExhibitRevenue.cs` |
| Problem Details DI registration | `src/GalleryManager.Api/Program.cs` — `AddProblemDetails()` |

## 7. Rate Limiting

ASP.NET Core's built-in rate limiter applies a fixed-window policy (100 requests per minute) to all versioned endpoints. Exceeding the limit returns `429 Too Many Requests` with a `Retry-After: 60` header and an RFC 7807 problem response body.

| Where | File |
|-------|------|
| Policy configuration + `OnRejected` handler | `src/GalleryManager.Api/Program.cs` — `AddRateLimiter()`, `RequireRateLimiting("fixed")` |

## 8. OpenAPI Metadata

Every endpoint declares its success and error response types via `.Produces<T>()` and `.ProducesProblem()` annotations, giving Swagger/OpenAPI consumers accurate contract documentation.

| Where | File |
|-------|------|
| All 6 endpoint files | `src/GalleryManager.Api/Features/Artworks/*.cs`, `src/GalleryManager.Api/Features/Exhibits/*.cs` |
| Swagger registration | `src/GalleryManager.Api/Program.cs` — `AddSwaggerGen()`, `UseSwaggerUI()` (dev only) |

---

## 9. Output Caching

Server-side output caching on all GET endpoints using ASP.NET Core's built-in `OutputCache` middleware. Cached responses bypass handler execution entirely for 30 seconds, reducing database round-trips on repeated reads. Cache keys vary by full query string so filtered/paginated/sorted requests cache independently.

| Where | File |
|-------|------|
| Cache service + middleware registration | `src/GalleryManager.Api/Program.cs` — `AddOutputCache()`, `UseOutputCache()` |
| Artworks | `src/GalleryManager.Api/Features/Artworks/GetArtworks.cs` — `.CacheOutput("Short")` |
| Exhibits | `src/GalleryManager.Api/Features/Exhibits/GetExhibits.cs` — `.CacheOutput("Short")` |
| Exhibit revenue | `src/GalleryManager.Api/Features/Exhibits/GetExhibitRevenue.cs` — `.CacheOutput("Short")` |

Base policy is `NoCache` (POST/PATCH/DELETE unaffected). Named policy `"Short"`: 30s expiry, `SetVaryByQuery("*")`.

---

## Last synced
2026-07-08 — all 9 practices verified working against Neon PostgreSQL via full-stack testing.
