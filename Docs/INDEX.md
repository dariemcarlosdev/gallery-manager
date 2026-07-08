# Gallery Manager — Docs Index

> Status: **POC / interview demo**. Small on purpose — see [Scope & Non-Goals](#scope--non-goals) before adding anything.

| Doc | Covers |
|---|---|
| [architecture.md](architecture.md) | System design, request flow, Vertical Slice pattern |
| [api-reference.md](api-reference.md) | Every HTTP endpoint — method, route, request/response, validation |
| [rest-api-best-practices.md](rest-api-best-practices.md) | 8 REST best practices — pagination, sorting, filtering, idempotency, versioning, RFC 7807, rate limiting, OpenAPI |
| [frontend-backend-flow.md](frontend-backend-flow.md) | End-to-end request flow from Angular UI → API → PostgreSQL (worked example: GetArtworks) |
| [data-access.md](data-access.md) | EF Core model, Postgres schema, the raw-SQL revenue function |
| [frontend.md](frontend.md) | Angular app structure, services, models, env config |
| [deployment.md](deployment.md) | GitHub/CI/CD checklist — Render (BE) + Vercel (FE) |

Related, not duplicated here:
- [../README.md](../README.md) — local setup steps (DB, migrations, running)
- [../CLAUDE.md](../CLAUDE.md) — AI-agent-facing build commands & code patterns

## Scope & Non-Goals

This is a **mock functional project for a job interview**, not production software. Two entities (Artwork, Exhibit), six endpoints, no auth, no tests yet. That's intentional — it exists to demonstrate specific skills (Vertical Slice API design, EF Core + raw SQL, Angular consuming a REST API), not to be a complete product.

**Strong rule for any future work on this repo**: keep additions proportional to what's already here. Don't introduce layers, patterns, or docs heavier than a 2-entity CRUD app needs — no repository abstraction, no CQRS/MediatR, no microservices, no auth framework, no multi-module doc hierarchy. If a change would only make sense "at scale," it doesn't belong here. When in doubt, prefer the simpler option even if it's less "correct" architecturally.

## Last synced
2026-07-07 — matches backend (`Artworks`, `Exhibits` features), frontend (`gallery-manager-web`), and hosted Neon Postgres DB as of that date.
