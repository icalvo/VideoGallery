using System.Linq.Expressions;

namespace VideoGallery.Library.Parsing;

public static class QueryBuilder
{
    public static IQueryable<T> BuildQuery<T>(
        IQueryable<T> q, 
        IEnumerable<CustomQueryExpression<T>> customQueryExpressions, 
        Func<IQueryable<T>, QuerySpec, IQueryable<T>> addSorting,
        QuerySpec querySpec,
        IReadOnlyDictionary<string, string>? namedQueries = null)
    {
        q = addSorting(q, querySpec);
        if (!string.IsNullOrWhiteSpace(querySpec.Search))
            q = q.Where(QueryExpression(querySpec, customQueryExpressions.ToArray(), namedQueries)
                .GetOrThrow(s => new Exception(s)));

        return q;
    }
    

    private static Result<Expression<Func<T, bool>>> QueryExpression<T>(
        QuerySpec querySpec,
        CustomQueryExpression<T>[] customQueryExpressions,
        IReadOnlyDictionary<string, string>? namedQueries)
    {
        Func<string, string> alias = x => x;
        var maxAliasIterations = 100;
        if (namedQueries is { Count: > 0 })
        {
            maxAliasIterations = namedQueries.Count + 1;
            alias = atom =>
            {
                if (QueryReferences.TryGetReferencedName(atom, out var name)
                    && namedQueries.TryGetValue(name, out var referencedSearch))
                    return referencedSearch;
                return atom;
            };
        }

        return querySpec.Compile(
            alias,
            x => customQueryExpressions.First(cqe => cqe.Condition(x)).Expression(x),
            maxAliasIterations);
    }
}