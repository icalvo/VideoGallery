using System.Linq.Expressions;

namespace VideoGallery.Library.Parsing;

public static class QueryBuilder
{
    public static IQueryable<T> BuildQuery<T>(
        IQueryable<T> q, 
        IEnumerable<CustomQueryExpression<T>> customQueryExpressions, 
        Func<IQueryable<T>, QuerySpec, IQueryable<T>> addSorting,
        QuerySpec querySpec)
    {
        q = addSorting(q, querySpec);
        if (!string.IsNullOrWhiteSpace(querySpec.Search))
            q = q.Where(QueryExpression(querySpec, customQueryExpressions.ToArray())
                .GetOrThrow(s => new Exception(s)));

        return q;
    }
    

    private static Result<Expression<Func<T, bool>>> QueryExpression<T>(QuerySpec querySpec, CustomQueryExpression<T>[] customQueryExpressions)
    {
        return querySpec.Compile(x => x, x => customQueryExpressions.First(cqe => cqe.Condition(x)).Expression(x));
    }
}