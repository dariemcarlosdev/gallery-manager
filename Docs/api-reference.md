# API Reference

> Cross-references: [Docs Index](INDEX.md) · [Architecture](architecture.md) · [Data Access](data-access.md)
> Base path (dev): `https://localhost:7080/api` (see `src/GalleryManager.Api/Properties/launchSettings.json`)
> Swagger UI: `/swagger` (Development environment only)

## Artworks

| Method | Route | Request body | Response | Validation |
|---|---|---|---|---|
| GET | `/api/artworks?status={status}` | — | `Response[]` (id, title, artist, medium, price, status, exhibitId) | `status` optional, must parse to `ArtworkStatus` enum or filter is ignored |
| POST | `/api/artworks` | `{ title, artist, medium, price }` | `201` + `{ id, title, status }` | title/artist/medium required, max length 200/150/100; price ≥ 0 |
| PATCH | `/api/artworks/{id}/status` | `{ status, exhibitId? }` | `204`, or `404` if id not found | `status` must parse to `ArtworkStatus`; `400` if not |

`ArtworkStatus` enum: `Available`, `OnLoan`, `Sold`. `exhibitId` is only applied when transitioning to `OnLoan`.

## Exhibits

| Method | Route | Request body | Response | Validation |
|---|---|---|---|---|
| GET | `/api/exhibits` | — | `Response[]` (id, name, startDate, endDate) | — |
| POST | `/api/exhibits/{exhibitId}/artworks/{artworkId}` | — | `204`, or `404` if exhibit or artwork not found | — |
| GET | `/api/exhibits/{exhibitId}/revenue` | — | `{ exhibitId, total, lines: [{ artworkTitle, salePrice }] }` | — |

`GetExhibitRevenue` calls the Postgres function `get_exhibit_revenue(exhibit_id)` — see [Data Access](data-access.md).

## Health

| Method | Route | Response |
|---|---|---|
| GET | `/health` | `{ status: "ok" }` |

## Error shape

Validation failures use ASP.NET's standard `ValidationProblem` shape (`Results.ValidationProblem`). Not-found and bad-status errors return a plain `{ error: "..." }` object — no unified `ProblemDetails` wrapper yet (would be the first thing to add if this grew past POC).

## Last synced
2026-07-07 — matches `Features/Artworks/*` and `Features/Exhibits/*`.
