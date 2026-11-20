using System.Linq.Expressions;
using VideoGallery.Library.Parsing;

namespace VideoGallery.Tests;

public class QueryReferencesTests
{
    [Fact]
    public void TryGetReferencedName_ParsesBackticks()
    {
        Assert.True(QueryReferences.TryGetReferencedName("`P`", out var name));
        Assert.Equal("P", name);
        Assert.False(QueryReferences.TryGetReferencedName("P", out _));
        Assert.False(QueryReferences.TryGetReferencedName("``", out _));
    }

    [Fact]
    public void GetReferencedNames_FindsBacktickAtoms()
    {
        var names = QueryReferences.GetReferencedNames("`P` and C or `Q`").ToArray();
        Assert.Equal(new[] { "P", "Q" }, names);
    }

    [Fact]
    public void GetReferencedNames_IgnoresBackticksInsideQuotedAtoms()
    {
        var names = QueryReferences.GetReferencedNames("\"hello `P` world\" and C").ToArray();
        Assert.Empty(names);
    }

    [Fact]
    public void RewriteReferences_UpdatesExactBacktickNames()
    {
        Assert.Equal("`P2` and C", QueryReferences.RewriteReferences("`P` and C", "P", "P2"));
        Assert.Equal("`Pref` and C", QueryReferences.RewriteReferences("`Pref` and C", "P", "P2"));
    }

    [Fact]
    public void Compile_ExpandsNamedQueryReferencesWithCorrectPrecedence()
    {
        // P = "A or B"; "`P` and C" must mean "(A or B) and C", not "A or (B and C)".
        var namedQueries = new Dictionary<string, string>
        {
            ["P"] = "A or B"
        };
        var spec = new QuerySpec("`P` and C");
        var result = CompileWithNames(spec, namedQueries);
        Assert.True(result.IsSuccess, result.Match(_ => "", e => e));

        var predicate = result.Match(e => e.Compile(), _ => throw new Exception());
        Assert.True(predicate(Tags("A", "C")));
        Assert.True(predicate(Tags("B", "C")));
        Assert.False(predicate(Tags("A")));
        Assert.False(predicate(Tags("C")));
        Assert.False(predicate(Tags("A", "B")));
    }

    [Fact]
    public void Compile_ExpandsNestedReferences()
    {
        var namedQueries = new Dictionary<string, string>
        {
            ["P"] = "A or B",
            ["Q"] = "`P` and C"
        };
        var spec = new QuerySpec("`Q`");
        var result = CompileWithNames(spec, namedQueries);
        Assert.True(result.IsSuccess, result.Match(_ => "", e => e));

        var predicate = result.Match(e => e.Compile(), _ => throw new Exception());
        Assert.True(predicate(Tags("A", "C")));
        Assert.False(predicate(Tags("A")));
    }

    [Fact]
    public void Compile_DetectsCircularReferences()
    {
        var namedQueries = new Dictionary<string, string>
        {
            ["P"] = "`Q`",
            ["Q"] = "`P`"
        };
        var spec = new QuerySpec("`P`");
        var result = CompileWithNames(spec, namedQueries);
        Assert.True(result.IsFailure);
        Assert.Contains("Circular", result.Match(_ => "", e => e));
    }

    private static HashSet<string> Tags(params string[] tags) => [..tags];

    private static Result<Expression<Func<HashSet<string>, bool>>> CompileWithNames(
        QuerySpec spec,
        IReadOnlyDictionary<string, string> namedQueries)
    {
        Func<string, string> alias = atom =>
        {
            if (QueryReferences.TryGetReferencedName(atom, out var name)
                && namedQueries.TryGetValue(name, out var search))
                return search;
            return atom;
        };

        return spec.Compile<HashSet<string>>(
            alias,
            atom => tags => tags.Contains(atom),
            namedQueries.Count + 1);
    }
}
