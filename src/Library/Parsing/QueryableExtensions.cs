using System.Linq.Expressions;

namespace VideoGallery.Library.Parsing;

public static class QueryableExtensions
{
    public static IQueryable<TSource> Sort<TSource, TKey>(
        this IQueryable<TSource> q,
        SortingType sortType,
        Expression<Func<TSource, TKey>> keySelector
    ) =>
        sortType switch
        {
            SortingType.Ascending when q is IOrderedQueryable<TSource> oq => oq.ThenBy(keySelector),
            SortingType.Descending when q is IOrderedQueryable<TSource> oq => oq.ThenBy(keySelector),
            SortingType.Ascending => q.OrderBy(keySelector),
            SortingType.Descending => q.OrderByDescending(keySelector),
            _ => q
        };
}