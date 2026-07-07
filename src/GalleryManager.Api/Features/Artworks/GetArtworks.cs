using GalleryManager.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace GalleryManager.Api.Features.Artworks;

public static class GetArtworks
{
    public record Response(
        int Id,
        string Title,
        string Artist,
        string Medium,
        decimal Price,
        string Status,
        int? ExhibitId);

    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/artworks", async (
            GalleryDbContext db,
            string? status,
            CancellationToken ct) =>
        {
            var query = db.Artworks.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(status) &&
                Enum.TryParse<ArtworkStatus>(status, true, out var parsedStatus))
            {
                query = query.Where(a => a.Status == parsedStatus);
            }

            var artworks = await query
                .OrderByDescending(a => a.CreatedAtUtc)
                .Select(a => new Response(
                    a.Id, a.Title, a.Artist, a.Medium, a.Price, a.Status.ToString(), a.ExhibitId))
                .ToListAsync(ct);

            return Results.Ok(artworks);
        })
        .WithName("GetArtworks")
        .WithTags("Artworks")
        .Produces<List<Response>>(StatusCodes.Status200OK);
    }
}
