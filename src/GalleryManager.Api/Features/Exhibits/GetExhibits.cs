using System.Linq.Expressions;
using GalleryManager.Api.Common;
using GalleryManager.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace GalleryManager.Api.Features.Exhibits;

public static class GetExhibits
{
    public record Response(int Id, string Name, DateOnly StartDate, DateOnly EndDate);

    private static readonly Dictionary<string, Expression<Func<Exhibit, object>>> SortableFields = new(StringComparer.OrdinalIgnoreCase)
    {
        ["name"] = e => e.Name,
        ["startDate"] = e => e.StartDate,
        ["endDate"] = e => e.EndDate
    };

    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/exhibits", async (
            GalleryDbContext db,
            string? name,
            string? sortBy,
            string? sortDirection,
            int page = 1,
            int pageSize = 20,
            CancellationToken ct = default) =>
        {
            var query = db.Exhibits.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(name))
                query = query.Where(e => EF.Functions.ILike(e.Name, $"%{name}%"));

            query = query.ApplySorting(sortBy, sortDirection ?? "asc", SortableFields);

            if (string.IsNullOrWhiteSpace(sortBy))
                query = query.OrderBy(e => e.StartDate);

            var paged = new PagedRequest(page, pageSize);

            var result = await query
                .Select(e => new Response(e.Id, e.Name, e.StartDate, e.EndDate))
                .ToPagedResponseAsync(paged.Page, paged.PageSize, ct);

            return Results.Ok(result);
        })
        .WithName("GetExhibits")
        .WithTags("Exhibits")
        .Produces<PagedResponse<Response>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status429TooManyRequests);
    }
}
