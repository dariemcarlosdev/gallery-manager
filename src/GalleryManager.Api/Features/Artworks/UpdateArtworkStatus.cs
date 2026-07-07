using GalleryManager.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace GalleryManager.Api.Features.Artworks;

public static class UpdateArtworkStatus
{
    public record Request(string Status, int? ExhibitId);

    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPatch("/api/artworks/{id:int}/status", async (
            int id,
            Request request,
            GalleryDbContext db,
            CancellationToken ct) =>
        {
            if (!Enum.TryParse<ArtworkStatus>(request.Status, true, out var newStatus))
            {
                return Results.BadRequest(new { error = $"Unknown status '{request.Status}'." });
            }

            var artwork = await db.Artworks.FirstOrDefaultAsync(a => a.Id == id, ct);
            if (artwork is null)
            {
                return Results.NotFound();
            }

            artwork.Status = newStatus;
            artwork.ExhibitId = newStatus == ArtworkStatus.OnLoan ? request.ExhibitId : artwork.ExhibitId;

            await db.SaveChangesAsync(ct);

            return Results.NoContent();
        })
        .WithName("UpdateArtworkStatus")
        .WithTags("Artworks")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status404NotFound);
    }
}
