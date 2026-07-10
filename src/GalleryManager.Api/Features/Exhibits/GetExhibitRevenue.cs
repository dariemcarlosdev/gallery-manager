using GalleryManager.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace GalleryManager.Api.Features.Exhibits;

/// <summary>Feature slice for GET /exhibits/{exhibitId}/revenue — totals sales via a Postgres function.</summary>
public static class GetExhibitRevenue
{
    /// <summary>Raw row shape mapped from the <c>get_exhibit_revenue</c> SQL function.</summary>
    public class RevenueRow
    {
        /// <summary>Title of a sold artwork.</summary>
        public string ArtworkTitle { get; set; } = string.Empty;
        /// <summary>Sale price for that artwork.</summary>
        public decimal SalePrice { get; set; }
    }

    /// <summary>One sold-artwork entry in the revenue breakdown.</summary>
    public record LineItem(string ArtworkTitle, decimal SalePrice);
    /// <summary>Revenue summary: the exhibit id, total, and per-artwork lines.</summary>
    public record Response(int ExhibitId, decimal Total, IReadOnlyList<LineItem> Lines);

    /// <summary>
    /// Registers the revenue endpoint. Returns 404 if the exhibit is missing, otherwise calls the
    /// <c>get_exhibit_revenue</c> Postgres function (parameterized), sums the lines, and returns the total. Output-cached briefly.
    /// </summary>
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
