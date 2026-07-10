using System.Linq.Expressions;
using GalleryManager.Api.Common;
using GalleryManager.Api.Data;
using GalleryManager.Api.Features.Artworks;
using Microsoft.EntityFrameworkCore;

namespace GalleryManager.Api.Features.Exhibits;

/// <summary>Feature slice for GET /exhibits — listing with per-status artwork counts.</summary>
public static class GetExhibits
{
    /// <summary>Exhibit item plus rolled-up counts of its artworks by status.</summary>
    public record Response(
        int Id,
        string Name,
        DateOnly StartDate,
        DateOnly EndDate,
        int ArtworkCount,
        int AvailableCount,
        int OnLoanCount,
        int SoldCount);

    /// <summary>Whitelist of client-sortable fields mapped to their key selectors.</summary>
    private static readonly Dictionary<string, Expression<Func<Exhibit, object>>> SortableFields = new(StringComparer.OrdinalIgnoreCase)
    {
        ["name"] = e => e.Name,
        ["startDate"] = e => e.StartDate,
        ["endDate"] = e => e.EndDate
    };

    /// <summary>
    /// Registers the GET /exhibits endpoint. Supports optional name filter, whitelisted sorting
    /// (default: by start date), and pagination; projects per-status artwork counts. Output-cached briefly.
    /// </summary>
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
