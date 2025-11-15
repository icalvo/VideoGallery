using System.Linq.Expressions;

namespace VideoGallery.Library.Parsing;

public record QuerySpec
{
    public QuerySpec(string search, SortingType sortType = SortingType.Ascending, string? sortField = null)
    {
        Search = search;
        SortType = sortType;
        SortField = sortField;
    }

    public string Search { get; private set; }

    public SortingType SortType { get; private set; }

    public string? SortField { get; private set; }

    public Result<Expression<Func<T, bool>>> Compile<T>(
        Func<string, string> alias,
        Func<string, Expression<Func<T, bool>>> atomResolver)
    {
        var search = BooleanExpressionParser.Parse(Search)
            .Map(expression => expression.ToString(alias));
        bool aliasFinished = false;
        while (!aliasFinished && search.IsSuccess)
        {
            var newSearch = search.Bind(s =>
            {
                return BooleanExpressionParser.Parse(s)
                    .Map(expression => expression.ToString(alias));
            });
            aliasFinished = search.Bind(oldr => newSearch.Map(newr => oldr == newr))
                .Match(r => r, _ => false);
            search = newSearch;
        }
        return search.Bind(s => BooleanExpressionParser.Parse(s)
            .Map(expression => BooleanExpressionCompiler.Compile(expression, atomResolver)));
    }

    public override string ToString()
    {
        return $"{{ Search = {Search}, SortType = {SortType}, SortField = {SortField} }}";
    }
}