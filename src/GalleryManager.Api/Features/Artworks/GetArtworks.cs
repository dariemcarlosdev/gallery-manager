using System.Linq.Expressions;
using GalleryManager.Api.Common;
using GalleryManager.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace GalleryManager.Api.Features.Artworks;

public static class GetArtworks
{
    public record Response(
        int Id,
        string Title,
        string Artist,
        string Medium,
        decimal Price,
        string Status,
        int? ExhibitId);

    private static readonly Dictionary<string, Expression<Func<Artwork, object>>> SortableFields = new(StringComparer.OrdinalIgnoreCase)
    {
        ["title"] = a => a.Title,
        ["artist"] = a => a.Artist,
        ["price"] = a => a.Price,
        ["createdAtUtc"] = a => a.CreatedAtUtc
    };

    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/artworks", async (
            GalleryDbContext db,
            string? status,
            string? artist,
            string? medium,
            string? sortBy,
            string? sortDirection,
            int page = 1,
            int pageSize = 20,
            CancellationToken ct = default) =>
        {
            var query = db.Artworks.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(status) &&
                Enum.TryParse<ArtworkStatus>(status, true, out var parsedStatus))
            {
                query = query.Where(a => a.Status == parsedStatus);
            }

            if (!string.IsNullOrWhiteSpace(artist))
                query = query.Where(a => EF.Functions.ILike(a.Artist, $"%{artist}%"));

            if (!string.IsNullOrWhiteSpace(medium))
                query = query.Where(a => EF.Functions.ILike(a.Medium, $"%{medium}%"));

            query = query.ApplySorting(sortBy, sortDirection ?? "asc", SortableFields);

            if (string.IsNullOrWhiteSpace(sortBy))
                query = query.OrderByDescending(a => a.CreatedAtUtc);

            var paged = new PagedRequest { Page = page, PageSize = pageSize };

            var result = await query
                .Select(a => new Response(
                    a.Id, a.Title, a.Artist, a.Medium, a.Price, a.Status.ToString(), a.ExhibitId))
                .ToPagedResponseAsync(paged.EffectivePage, paged.EffectivePageSize, ct);

            return Results.Ok(result);
        })
        .WithName("GetArtworks")
        .WithTags("Artworks")
        .CacheOutput("Short")
        .Produces<PagedResponse<Response>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status429TooManyRequests);
    }
}
