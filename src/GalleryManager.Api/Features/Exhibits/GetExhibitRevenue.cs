using GalleryManager.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace GalleryManager.Api.Features.Exhibits;

public static class GetExhibitRevenue
{
    public class RevenueRow
    {
        public string ArtworkTitle { get; set; } = string.Empty;
        public decimal SalePrice { get; set; }
    }

    public record LineItem(string ArtworkTitle, decimal SalePrice);
    public record Response(int ExhibitId, decimal Total, IReadOnlyList<LineItem> Lines);

    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/exhibits/{exhibitId:int}/revenue", async (
            int exhibitId,
            GalleryDbContext db,
            CancellationToken ct) =>
        {
            var exhibitExists = await db.Exhibits
                .AsNoTracking()
                .AnyAsync(e => e.Id == exhibitId, ct);

            if (!exhibitExists)
            {
                return Results.Problem(
                    detail: $"Exhibit with id {exhibitId} was not found.",
                    statusCode: StatusCodes.Status404NotFound,
                    title: "Exhibit not found");
            }

            var rows = await db.Database
                .SqlQuery<RevenueRow>(
                    $"""SELECT artwork_title AS "ArtworkTitle", sale_price AS "SalePrice" FROM get_exhibit_revenue({exhibitId})""")
                .ToListAsync(ct);

            var total = rows.Sum(r => r.SalePrice);
            var lines = rows.Select(r => new LineItem(r.ArtworkTitle, r.SalePrice)).ToList();

            return Results.Ok(new Response(exhibitId, total, lines));
        })
        .WithName("GetExhibitRevenue")
        .WithTags("Exhibits")
        .CacheOutput("Short")
        .Produces<Response>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status429TooManyRequests);
    }
}
