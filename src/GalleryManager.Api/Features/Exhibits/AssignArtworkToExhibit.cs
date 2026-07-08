using GalleryManager.Api.Data;
using GalleryManager.Api.Features.Artworks;
using Microsoft.EntityFrameworkCore;

namespace GalleryManager.Api.Features.Exhibits;

public static class AssignArtworkToExhibit
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/exhibits/{exhibitId:int}/artworks/{artworkId:int}", async (
            int exhibitId,
            int artworkId,
            GalleryDbContext db,
            CancellationToken ct) =>
        {
            var exhibit = await db.Exhibits.FirstOrDefaultAsync(e => e.Id == exhibitId, ct);
            if (exhibit is null)
            {
                return Results.Problem(
                    detail: $"Exhibit with id {exhibitId} was not found.",
                    statusCode: StatusCodes.Status404NotFound,
                    title: "Exhibit not found");
            }

            var artwork = await db.Artworks.FirstOrDefaultAsync(a => a.Id == artworkId, ct);
            if (artwork is null)
            {
                return Results.Problem(
                    detail: $"Artwork with id {artworkId} was not found.",
                    statusCode: StatusCodes.Status404NotFound,
                    title: "Artwork not found");
            }

            if (artwork.ExhibitId == exhibitId)
                return Results.NoContent();

            artwork.ExhibitId = exhibitId;
            await db.SaveChangesAsync(ct);

            return Results.NoContent();
        })
        .WithName("AssignArtworkToExhibit")
        .WithTags("Exhibits")
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status429TooManyRequests);
    }
}
