using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

namespace GalleryManager.Api.Common;

/// <summary>Raw paging inputs from a request, with clamped "effective" values for safe use.</summary>
public record PagedRequest
{
    /// <summary>Requested 1-based page number.</summary>
    public int Page { get; init; } = 1;
    /// <summary>Requested page size.</summary>
    public int PageSize { get; init; } = 20;

    /// <summary>Page coerced to a minimum of 1.</summary>
    public int EffectivePage => Page < 1 ? 1 : Page;
    /// <summary>Page size coerced into the 1–100 range (defaults to 20 when invalid).</summary>
    public int EffectivePageSize => PageSize < 1 ? 20 : Math.Min(PageSize, 100);
}

/// <summary>Generic paged result envelope with items plus navigation metadata.</summary>
public record PagedResponse<T>(
    IReadOnlyList<T> Data,
    int Page,
    int PageSize,
    int TotalCount,
    int TotalPages,
    bool HasNextPage,
    bool HasPreviousPage);

/// <summary>Reusable pagination and sorting helpers for EF Core queries.</summary>
public static class QueryableExtensions
{
    /// <summary>Counts the query, applies skip/take, and wraps the page in a <see cref="PagedResponse{T}"/>.</summary>
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

    /// <summary>
    /// Applies ordering when <paramref name="sortBy"/> matches an entry in <paramref name="allowedFields"/>
    /// (ascending unless <paramref name="sortDirection"/> is "desc"); otherwise returns the query unchanged.
    /// The whitelist prevents sorting on arbitrary client-supplied fields.
    /// </summary>
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
