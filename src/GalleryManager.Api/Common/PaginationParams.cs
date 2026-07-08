using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

namespace GalleryManager.Api.Common;

public record PagedRequest
{
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;

    public int EffectivePage => Page < 1 ? 1 : Page;
    public int EffectivePageSize => PageSize < 1 ? 20 : Math.Min(PageSize, 100);
}

public record PagedResponse<T>(
    IReadOnlyList<T> Data,
    int Page,
    int PageSize,
    int TotalCount,
    int TotalPages,
    bool HasNextPage,
    bool HasPreviousPage);

public static class QueryableExtensions
{
    public static async Task<PagedResponse<T>> ToPagedResponseAsync<T>(
        this IQueryable<T> query,
        int page,
        int pageSize,
        CancellationToken ct = default)
    {
        var totalCount = await query.CountAsync(ct);
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        var data = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return new PagedResponse<T>(
            data, page, pageSize, totalCount, totalPages,
            HasNextPage: page < totalPages,
            HasPreviousPage: page > 1);
    }

    public static IQueryable<T> ApplySorting<T>(
        this IQueryable<T> query,
        string? sortBy,
        string sortDirection,
        Dictionary<string, Expression<Func<T, object>>> allowedFields)
    {
        if (string.IsNullOrWhiteSpace(sortBy) || !allowedFields.TryGetValue(sortBy.ToLowerInvariant(), out var keySelector))
            return query;

        return sortDirection.Equals("desc", StringComparison.OrdinalIgnoreCase)
            ? query.OrderByDescending(keySelector)
            : query.OrderBy(keySelector);
    }
}
