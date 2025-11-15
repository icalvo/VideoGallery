using VideoGallery.Library.Parsing;

namespace VideoGallery.Tests;

public class LexerTests
{
    private static string[] Check(Result<List<Token>> result)
    {
        return result.Match(
            s => s.Select(t => t switch
            {
                { Type: TokenType.And } => "AND",
                { Type: TokenType.Or } => "OR",
                { Type: TokenType.Not } => "NOT",
                { Type: TokenType.Atom } a => $"A:{a.Value}",
                { Type: TokenType.LeftParen } a => $"{a.Value}",
                { Type: TokenType.RightParen } a => $"{a.Value}",
                { Type: TokenType.Error } a => "ERROR",
                { Type: TokenType.EndOfInput } a => "EOI",
                _ => throw new ArgumentOutOfRangeException(nameof(t), t, null)
            }).ToArray(),
            e => [e]);
    }

    [Fact]
    public void Quotes()
    {
        var result = Lexer.Tokenize("\"one phrase\" and b", "and", "or", "not");
        Assert.True(result.IsSuccess);
        Assert.Equal(new[] { "A:one phrase", "AND", "A:b", "EOI" }, Check(result));
    }

    [Fact]
    public void Regression1()
    {
        var result = Lexer.Tokenize("(x:pending or not lastview:1y) and not x:unprocessed and not q:", "and", "or", "not");
        Assert.True(result.IsSuccess);
        Assert.Equal(
            new[]
            {
                "(", "A:x:pending", "OR", "NOT", "A:lastview:1y", ")", "AND", "NOT", "A:x:unprocessed", "AND", "NOT", "A:q:",
                "EOI"
            },
            Check(result));
    }
}