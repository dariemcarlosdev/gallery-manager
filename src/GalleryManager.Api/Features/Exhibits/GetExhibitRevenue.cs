using GalleryManager.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace GalleryManager.Api.Features.Exhibits;

public static class GetExhibitRevenue
{
    // Maps 1:1 to columns returned by the get_exhibit_revenue() Postgres function.
    // See Data/Sql/001_create_get_exhibit_revenue_function.sql
    public class RevenueRow
    {
        public string ArtworkTitle { get; set; } = string.Empty;
        public decimal SalePrice { get; set; }
    }

    public record Response(string ArtworkTitle, decimal SalePrice);

    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/exhibits/{exhibitId:int}/revenue", async (
            int exhibitId,
            GalleryDbContext db,
            CancellationToken ct) =>
        {
            var rows = await db.Database
                .SqlQuery<RevenueRow>(
                    $"SELECT * FROM get_exhibit_revenue({exhibitId})")
                .ToListAsync(ct);

            var total = rows.Sum(r => r.SalePrice);

            return Results.Ok(new
            {
                exhibitId,
                total,
                lines = rows.Select(r => new Response(r.ArtworkTitle, r.SalePrice))
            });
        })
        .WithName("GetExhibitRevenue")
        .WithTags("Exhibits")
        .Produces(StatusCodes.Status200OK);
    }
}
