using VideoGallery.Library.Parsing;

namespace VideoGallery.Tests;

public class ImpliesTests
{
    [Fact]
    public void Compile_ImpliesIsNotAOrB()
    {
        var result = BooleanExpressionParser.Parse("A implies B")
            .Map(expr => BooleanExpressionCompiler.Compile<HashSet<string>>(
                expr, atom => tags => tags.Contains(atom)));
        Assert.True(result.IsSuccess, result.Match(_ => "", e => e));

        var predicate = result.Match(e => e.Compile(), _ => throw new Exception());
        Assert.True(predicate([]));                    // ¬A ∨ B — both false → true
        Assert.True(predicate(["B"]));                 // ¬A ∨ B
        Assert.False(predicate(["A"]));                // A and not B → false
        Assert.True(predicate(new HashSet<string> { "A", "B" })); // A and B → true
    }

    [Fact]
    public void Parse_ImpliesBindsWeakerThanOrAndAnd()
    {
        // A or B implies C and D  ≡  (A or B) implies (C and D)
        var result = BooleanExpressionParser.Parse("A or B implies C and D");
        Assert.True(result.IsSuccess, result.Match(_ => "", e => e));
        Assert.IsType<ImpliesExpression>(result.Match(e => e, _ => null!));
    }

    [Fact]
    public void Parse_ImpliesIsRightAssociative()
    {
        var result = BooleanExpressionParser.Parse("A implies B implies C");
        Assert.True(result.IsSuccess, result.Match(_ => "", e => e));
        var expr = result.Match(e => e, _ => throw new Exception());
        var outer = Assert.IsType<ImpliesExpression>(expr);
        Assert.IsType<AtomExpression>(outer.Left);
        Assert.IsType<ImpliesExpression>(outer.Right);
    }
}
