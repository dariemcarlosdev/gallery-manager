using GalleryManager.Api.Data;
using GalleryManager.Api.Features.Artworks;
using Microsoft.EntityFrameworkCore;

namespace GalleryManager.Api.Features.Exhibits;

public static class AssignArtworkToExhibit
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/exhibits/{exhibitId:int}/artworks/{artworkId:int}", async (
            int exhibitId,
            int artworkId,
            GalleryDbContext db,
            CancellationToken ct) =>
        {
            var exhibit = await db.Exhibits.FirstOrDefaultAsync(e => e.Id == exhibitId, ct);
            if (exhibit is null)
            {
                return Results.NotFound(new { error = "Exhibit not found." });
            }

            var artwork = await db.Artworks.FirstOrDefaultAsync(a => a.Id == artworkId, ct);
            if (artwork is null)
            {
                return Results.NotFound(new { error = "Artwork not found." });
            }

            artwork.ExhibitId = exhibitId;
            await db.SaveChangesAsync(ct);

            return Results.NoContent();
        })
        .WithName("AssignArtworkToExhibit")
        .WithTags("Exhibits")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status404NotFound);
    }
}
