# Data Access

> Cross-references: [Docs Index](INDEX.md) · [Architecture](architecture.md) · [API Reference](api-reference.md)

## Stack

EF Core 8 Code-First + `Npgsql.EntityFrameworkCore.PostgreSQL`. One `DbContext` (`GalleryDbContext`), no repository abstraction — handlers inject `GalleryDbContext` directly (see [Architecture — deliberately absent](architecture.md#deliberately-absent-and-why-thats-fine-here)).

## Schema

| Table | Columns | Notes |
|---|---|---|
| `artworks` | id (PK, int), title, artist, medium, price `numeric(10,2)`, status (string-converted enum), exhibit_id (nullable FK), created_at_utc | `Status` stored as string via `HasConversion<string>()`, not an int, so the DB is human-readable |
| `exhibits` | id (PK, int), name, start_date, end_date (`DateOnly`) | |

Both configured in `GalleryDbContext.OnModelCreating` via Fluent API — no `IEntityTypeConfiguration<T>` split files (would be over-structuring for 2 entities).

## Migrations

Standard EF Core migrations (`dotnet ef migrations add` / `dotnet ef database update`). `InitialCreate` (2026-07-07) is committed and applied — creates `artworks`/`exhibits` on the hosted Neon Postgres project (`gallery-manager`, db `gallerymanager`; connection string via `dotnet user-secrets`, not `appsettings.json` — see [README](../README.md)).

## Raw SQL: `get_exhibit_revenue`

`GetExhibitRevenue.cs` deliberately calls a **Postgres function** instead of expressing the query in LINQ:

```sql
CREATE OR REPLACE FUNCTION get_exhibit_revenue(exhibit_id_param INT)
RETURNS TABLE(artwork_title TEXT, sale_price NUMERIC) AS $$
BEGIN
    RETURN QUERY
    SELECT a."Title", a."Price"
    FROM artworks a
    WHERE a."ExhibitId" = exhibit_id_param
      AND a."Status" = 'Sold'
    ORDER BY a."Title";
END;
$$ LANGUAGE plpgsql;
```

Columns are quoted PascalCase (`"Title"`, `"Price"`, `"ExhibitId"`, `"Status"`) because Npgsql/EF Core preserves C# property casing as quoted identifiers — unquoted lowercase refs (the original version of this script) resolve to different, non-existent columns and fail at call time, not at `CREATE FUNCTION` time.

Script lives at `src/GalleryManager.Api/Data/Sql/001_create_get_exhibit_revenue_function.sql` and must be run manually against the DB once (not wired into a migration — a `migrationBuilder.Sql(...)` call would be the natural next step if this became a real project).

Called from C# via parameterized `SqlQuery<RevenueRow>`:
```csharp
db.Database.SqlQuery<RevenueRow>($"SELECT * FROM get_exhibit_revenue({exhibitId})")
```
The `$""` here is EF Core's `FromSqlInterpolated`-style parameterization — it's *not* string concatenation, so it's not SQL-injectable despite looking like interpolation.

**Why raw SQL at all**: the target job posting calls out SQL as the #1 skill. This slice exists to show comfort writing and calling actual SQL/stored logic, alongside EF Core LINQ everywhere else — not because the query itself needed a stored function.

## Last synced
2026-07-07 — Neon hosting + `get_exhibit_revenue` column-quoting fix
