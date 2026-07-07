# Gallery Manager API

POC for Park Art gallery interview. Artwork + exhibit inventory management.

Full docs: [Docs/INDEX.md](Docs/INDEX.md) (architecture, API reference, data access, frontend).

## Stack

- ASP.NET Core 8 Minimal API
- EF Core 8 + Npgsql (PostgreSQL)
- Vertical Slice Architecture (each feature owns its endpoint, DTOs, query — no shared repository layer)
- FluentValidation for request validation
- Swagger/OpenAPI (dev only)

## Structure

```
GalleryManager.sln
src/GalleryManager.Api/
  Program.cs                 # composition root - wires DbContext, CORS, endpoint mapping
  Data/
    GalleryDbContext.cs
    Sql/001_create_get_exhibit_revenue_function.sql
  Features/
    Artworks/
      Artwork.cs              # entity + status enum
      GetArtworks.cs           # GET  /api/artworks
      CreateArtwork.cs         # POST /api/artworks
      UpdateArtworkStatus.cs   # PATCH /api/artworks/{id}/status
    Exhibits/
      Exhibit.cs
      GetExhibits.cs                  # GET  /api/exhibits
      AssignArtworkToExhibit.cs       # POST /api/exhibits/{id}/artworks/{artworkId}
      GetExhibitRevenue.cs            # GET  /api/exhibits/{id}/revenue (calls Postgres function)
```

Each feature file is self-contained: request/response records, validation, and the
`MapEndpoint` call live together instead of being spread across
Controllers/Services/Repositories layers.

## Local setup (Visual Studio)

1. Open `GalleryManager.sln`.
2. NuGet restore should run automatically on load; if not, right-click solution →
   Restore NuGet Packages.
3. Postgres is hosted on [Neon](https://neon.tech) (project `gallery-manager`, db
   `gallerymanager`) — no local Postgres/Docker needed. Get the connection string from
   the Neon console (or `neonctl`/MCP) and set it via user secrets (never commit it to
   `appsettings.json`):
   ```
   dotnet user-secrets set "ConnectionStrings:GalleryDb" "..." --project src/GalleryManager.Api
   ```
4. The `InitialCreate` migration is already committed. Apply it against Neon:
   ```
   dotnet ef database update --project src/GalleryManager.Api
   ```
   (Requires `dotnet tool install --global dotnet-ef` if not already installed. Only
   run `dotnet ef migrations add <Name>` again if you change the EF Core model.)
5. Run the Postgres function script once against your DB (already applied on the
   shared Neon instance, only needed for a fresh DB):
   `Data/Sql/001_create_get_exhibit_revenue_function.sql`
6. F5 in Visual Studio — Swagger UI opens at `/swagger`.

## Frontend

Angular 19 app scaffolded at `src/gallery-manager-web/`. Run:
```
cd src/gallery-manager-web
npm install
npx ng serve
```
Talks to the API via `ArtworkService`/`ExhibitService` (`src/app/services/`), base URL set in
`src/environments/environment.development.ts` (defaults to `https://localhost:7080/api`, matching
`launchSettings.json`).

## Deployment note

Frontend (Angular) deploys to Vercel. This API does **not** —
Vercel has no .NET runtime, so the API needs a separate host (Render, Fly.io,
or Azure App Service are the common free/cheap options). `.github/workflows/api-ci.yml`
builds/publishes on every push to `main`; the deploy step is a placeholder until
a host is picked.

## Why this shape, for the interview

- **Vertical Slice**: mirrors the job's ask directly — one folder per feature,
  easy to point at and say "here's everything for creating an artwork, in one place."
- **Minimal API**: matches "lightweight" API design requirement, no MVC controller
  ceremony.
- **Raw SQL function call** (`GetExhibitRevenue`): deliberately not hidden behind
  pure LINQ — shows comfort writing and calling actual SQL/stored-logic, which the
  JD calls out as the #1 skill ("2+ years SQL Server — this is key!").
- **EF Core Code-First** for the rest: shows ORM fluency without pretending raw
  SQL is the only tool in the box.

## Deployment

Backend deploys to Render (Docker), frontend to Vercel. See .github/workflows/api-ci.yml.
