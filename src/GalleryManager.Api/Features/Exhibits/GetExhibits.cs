using GalleryManager.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace GalleryManager.Api.Features.Exhibits;

public static class GetExhibits
{
    public record Response(int Id, string Name, DateOnly StartDate, DateOnly EndDate);

    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/exhibits", async (GalleryDbContext db, CancellationToken ct) =>
        {
            var exhibits = await db.Exhibits
                .AsNoTracking()
                .OrderBy(e => e.StartDate)
                .Select(e => new Response(e.Id, e.Name, e.StartDate, e.EndDate))
                .ToListAsync(ct);

            return Results.Ok(exhibits);
        })
        .WithName("GetExhibits")
        .WithTags("Exhibits")
        .Produces<List<Response>>(StatusCodes.Status200OK);
    }
}
