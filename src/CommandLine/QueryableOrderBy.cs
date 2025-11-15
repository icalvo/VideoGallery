using System.Linq.Expressions;

namespace VideoGallery.CommandLine;

public static class QueryableOrderBy
{
    public static IOrderedQueryable<TSource> OrderBySimple<TSource, TKey>(
        this IQueryable<TSource> source,
        Expression<Func<TSource, TKey>> keySelector,
        bool ordered) =>
        ordered
            ? ((IOrderedQueryable<TSource>)source).ThenBy(keySelector)
            : source.OrderBy(keySelector);

    public static IOrderedQueryable<TSource> OrderByDescendingSimple<TSource, TKey>(
        this IQueryable<TSource> source,
        Expression<Func<TSource, TKey>> keySelector,
        bool ordered) =>
        ordered
            ? ((IOrderedQueryable<TSource>)source).ThenByDescending(keySelector)
            : source.OrderByDescending(keySelector);
}