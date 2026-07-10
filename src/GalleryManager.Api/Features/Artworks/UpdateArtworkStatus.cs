using GalleryManager.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace GalleryManager.Api.Features.Artworks;

/// <summary>Feature slice for PATCH /artworks/{id}/status — transitions an artwork's status.</summary>
public static class UpdateArtworkStatus
{
    /// <summary>New status name plus an optional exhibit id (applied only when moving to OnLoan).</summary>
    public record Request(string Status, int? ExhibitId);

    /// <summary>
    /// Registers the PATCH /artworks/{id}/status endpoint. Validates the status name, returns 404
    /// if the artwork is missing, and sets <see cref="Artwork.ExhibitId"/> only when transitioning to OnLoan.
    /// </summary>
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPatch("/artworks/{id:int}/status", async (
            int id,
            Request request,
            GalleryDbContext db,
            CancellationToken ct) =>
        {
            if (!Enum.TryParse<ArtworkStatus>(request.Status, true, out var newStatus))
            {
                return Results.Problem(
                    detail: $"Unknown status '{request.Status}'. Valid values: {string.Join(", ", Enum.GetNames<ArtworkStatus>())}",
                    statusCode: StatusCodes.Status400BadRequest,
                    title: "Invalid status value");
            }

            var artwork = await db.Artworks.FirstOrDefaultAsync(a => a.Id == id, ct);
            if (artwork is null)
            {
                return Results.Problem(
                    detail: $"Artwork with id {id} was not found.",
                    statusCode: StatusCodes.Status404NotFound,
                    title: "Artwork not found");
            }

            artwork.Status = newStatus;
            artwork.ExhibitId = newStatus == ArtworkStatus.OnLoan ? request.ExhibitId : artwork.ExhibitId;

            await db.SaveChangesAsync(ct);

            return Results.NoContent();
        })
        .WithName("UpdateArtworkStatus")
        .WithTags("Artworks")
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status429TooManyRequests);
    }
}
