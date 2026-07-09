using System.Linq.Expressions;
using GalleryManager.Api.Common;
using GalleryManager.Api.Data;
using GalleryManager.Api.Features.Artworks;
using Microsoft.EntityFrameworkCore;

namespace GalleryManager.Api.Features.Exhibits;

public static class GetExhibits
{
    public record Response(
        int Id,
        string Name,
        DateOnly StartDate,
        DateOnly EndDate,
        int ArtworkCount,
        int AvailableCount,
        int OnLoanCount,
        int SoldCount);

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

            var paged = new PagedRequest { Page = page, PageSize = pageSize };

            var result = await query
                .Select(e => new Response(
                    e.Id,
                    e.Name,
                    e.StartDate,
                    e.EndDate,
                    e.Artworks.Count,
                    e.Artworks.Count(a => a.Status == ArtworkStatus.Available),
                    e.Artworks.Count(a => a.Status == ArtworkStatus.OnLoan),
                    e.Artworks.Count(a => a.Status == ArtworkStatus.Sold)))
                .ToPagedResponseAsync(paged.EffectivePage, paged.EffectivePageSize, ct);

            return Results.Ok(result);
        })
        .WithName("GetExhibits")
        .WithTags("Exhibits")
        .CacheOutput("Short")
        .Produces<PagedResponse<Response>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status429TooManyRequests);
    }
}
