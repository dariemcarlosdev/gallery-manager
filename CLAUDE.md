# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

Full project docs: [Docs/INDEX.md](Docs/INDEX.md) — read its "Scope & Non-Goals" section before adding architecture, layers, or docs beyond what a 2-entity POC needs.

## Quick Start Commands

```bash
# Build
dotnet build GalleryManager.sln

# Run (F5 in Visual Studio, or cli)
dotnet run --project src/GalleryManager.Api

# Restore dependencies
dotnet restore GalleryManager.sln

# Database migrations (requires dotnet-ef tool)
dotnet ef migrations add <MigrationName> --project src/GalleryManager.Api
dotnet ef database update --project src/GalleryManager.Api

# Run SQL script against DB (manual for now - execute 001_create_get_exhibit_revenue_function.sql)
```

## Architecture

**Vertical Slice** — Each feature owns its entire stack in `src/GalleryManager.Api/Features/{Domain}/`.

```
Features/Artworks/
  Artwork.cs                # Entity + ArtworkStatus enum
  GetArtworks.cs            # GET /api/artworks
  CreateArtwork.cs          # POST /api/artworks
  UpdateArtworkStatus.cs    # PATCH /api/artworks/{id}/status

Features/Exhibits/
  Exhibit.cs
  GetExhibits.cs            # GET /api/exhibits
  AssignArtworkToExhibit.cs # POST /api/exhibits/{id}/artworks/{artworkId}
  GetExhibitRevenue.cs      # GET /api/exhibits/{id}/revenue (calls Postgres function)
```

**No shared layers** — No Controllers, Services, Repositories, or DTOs folders. Each feature file is self-contained:
- `Request` / `Response` records
- `Validator` (inner class, extends `AbstractValidator<Request>`)
- `MapEndpoint` static method that registers the handler

**Composition root** — `Program.cs` wires DbContext, CORS, and calls `MapEndpoint` for each feature.

## Key Patterns

### Feature File Structure
```csharp
namespace GalleryManager.Api.Features.Artworks;

public static class CreateArtwork
{
    public record Request(string Title, string Artist, string Medium, decimal Price);
    public record Response(int Id, string Title, string Status);

    public class Validator : AbstractValidator<Request>
    {
        public Validator()
        {
            RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
            // ...
        }
    }

    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/artworks", async (Request request, GalleryDbContext db, CancellationToken ct) =>
        {
            var validator = new Validator();
            var validation = await validator.ValidateAsync(request, ct);
            if (!validation.IsValid)
                return Results.ValidationProblem(validation.ToDictionary());

            // Business logic inline
            var artwork = new Artwork { /* ... */ };
            db.Artworks.Add(artwork);
            await db.SaveChangesAsync(ct);

            return Results.Created($"/api/artworks/{artwork.Id}", response);
        })
        .WithName("CreateArtwork")
        .WithTags("Artworks")
        .Produces<Response>(StatusCodes.Status201Created)
        .ProducesValidationProblem();
    }
}
```

**FluentValidation** is always inline in the handler — no separate validation pipeline middleware yet.

**DbContext injection** happens directly in the `MapPost`/`MapGet`/etc lambda signature.

### Adding a New Feature

1. Create folder `src/GalleryManager.Api/Features/{Domain}/{FeatureName}.cs`
2. Define `Request`/`Response` records
3. Create inner `Validator` class
4. Write `MapEndpoint` static method
5. Call `{FeatureName}.MapEndpoint(app)` in `Program.cs`
6. If the feature needs a new table, add entity to `GalleryDbContext.cs`, then create & apply migration

## Stack Details

- **ASP.NET Core 8** — Minimal API (no controllers)
- **EF Core 8 + PostgreSQL** — Code-first via Npgsql, migrations in standard `Migrations/` folder
- **FluentValidation 11.9.2** — Request validation, validators live in feature files
- **Swagger 6.6.2** — Auto-generated from Minimal API metadata; dev-only in Program.cs

## Data Access

`GalleryDbContext` is in `src/GalleryManager.Api/Data/GalleryDbContext.cs`. Add DbSet properties for new entities:

```csharp
public DbSet<Artwork> Artworks { get; set; }
public DbSet<Exhibit> Exhibits { get; set; }
```

Entity configurations use Fluent API in `OnModelCreating`.

**Raw SQL** (e.g., `GetExhibitRevenue` calling a Postgres function) uses `FromSqlInterpolated` only — never `FromSqlRaw` with string concatenation.

## Local Setup (First Time)

1. Install `dotnet-ef` tool globally: `dotnet tool install --global dotnet-ef`
2. Set connection string via user secrets (safer than appsettings.json):
   ```bash
   dotnet user-secrets set "ConnectionStrings:GalleryDb" "Host=localhost;Database=gallery;Username=postgres;Password=..."
   ```
   Or use Neon free tier for serverless Postgres.
3. Apply initial migration:
   ```bash
   dotnet ef database update --project src/GalleryManager.Api
   ```
4. Run SQL script manually against your DB:
   ```sql
   -- Data/Sql/001_create_get_exhibit_revenue_function.sql
   ```
5. F5 in Visual Studio or `dotnet run --project src/GalleryManager.Api`
6. Swagger UI opens at `/swagger` in Development environment only

## CI/CD

`.github/workflows/api-ci.yml` runs on pushes to `main` and PRs:
- Restore dependencies
- Build (Release config)
- Publish to `./publish`
- Upload artifact
- Deploy placeholder (fill in when host is selected)

Tests are commented out until a test project is created.

## Common Mistakes to Avoid

- ❌ **Don't create a shared Repository/Service layer.** Each feature should be self-contained.
- ❌ **Don't put validation in a separate middleware.** Validate inline in the handler with FluentValidation.
- ❌ **Don't hide DbContext behind an abstraction yet** — direct injection is fine for MVP.
- ❌ **Don't use `FromSqlRaw` with string concatenation.** Use `FromSqlInterpolated` if you need raw SQL.
- ❌ **Don't add a new feature without running it through the feature file pattern** (Request/Response/Validator/MapEndpoint).

## Design Notes for the Interview Context

- **Vertical Slice mirrors the job description** — "Here's everything for [feature] in one place."
- **Minimal API** — No MVC ceremony; lightweight per the role requirements.
- **Raw SQL function call** in `GetExhibitRevenue` — Shows comfort with SQL and stored logic (the JD emphasizes SQL heavily).
- **EF Core Code-First** for the rest — Shows ORM fluency without pretending raw SQL is the only tool.
- **FluentValidation inline** — Simple, readable, no over-engineering.

## Future Considerations

Frontend (Angular) deploys to Vercel. This API does **not** (Vercel has no .NET runtime). Pick a host (Render, Fly.io, Azure App Service) and wire the deploy step in `.github/workflows/api-ci.yml` when ready.
