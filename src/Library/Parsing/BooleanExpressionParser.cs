namespace VideoGallery.Library.Parsing;

public class BooleanExpressionParser
{
    private List<Token> _tokens = [];
    private int _position = 0;

    private BooleanExpressionParser()
    {
    }
    public static Result<BooleanExpression> Parse(string input, string andOperator = "and", string orOperator = "or", string notOperator = "not") =>
        Lexer.Tokenize(input, andOperator, orOperator, notOperator).Bind(
            tokens =>
            {
                var parser = new BooleanExpressionParser();
                return parser.ParseExpression(tokens);
            });

    private Result<BooleanExpression> ParseExpression(List<Token> tokens)
    {
        _tokens = tokens;
        _position = 0;

        var result = ParseOrExpression();

        if (result.IsFailure)
            return result;

        if (CurrentToken.Type != TokenType.EndOfInput)
        {
            return Result<BooleanExpression>.Failure($"Unexpected token '{CurrentToken.Value}' at position {CurrentToken.Position}. Expected end of input.");
        }

        return result;
    }

    private Token CurrentToken =>
        _position < _tokens.Count 
            ? _tokens[_position] 
            : new Token(TokenType.EndOfInput, "", _tokens.Count > 0 ? _tokens[^1].Position : 0);

    private void Advance() => _position++;

    private Result<BooleanExpression> ParseOrExpression()
    {
        var leftResult = ParseAndExpression();
        if (leftResult.IsFailure)
            return leftResult;

        var left = ((Success<BooleanExpression>)leftResult).Value;

        while (CurrentToken.Type == TokenType.Or)
        {
            Advance(); // consume OR
            var rightResult = ParseAndExpression();
            if (rightResult.IsFailure)
                return rightResult;

            var right = ((Success<BooleanExpression>)rightResult).Value;
            left = BooleanExpression.Or(left, right);
        }

        return Result<BooleanExpression>.Success(left);
    }

    private Result<BooleanExpression> ParseAndExpression()
    {
        var leftResult = ParseNotExpression();
        if (leftResult.IsFailure)
            return leftResult;

        var left = ((Success<BooleanExpression>)leftResult).Value;

        while (CurrentToken.Type == TokenType.And)
        {
            Advance(); // consume AND
            var rightResult = ParseNotExpression();
            if (rightResult.IsFailure)
                return rightResult;

            var right = ((Success<BooleanExpression>)rightResult).Value;
            left = BooleanExpression.And(left, right);
        }

        return Result<BooleanExpression>.Success(left);
    }

    private Result<BooleanExpression> ParseNotExpression()
    {
        if (CurrentToken.Type != TokenType.Not) return ParsePrimaryExpression();
        Advance(); // consume NOT
        var expressionResult = ParseNotExpression(); // NOT is right-associative
        if (expressionResult.IsFailure)
            return expressionResult;

        var expression = ((Success<BooleanExpression>)expressionResult).Value;
        return Result<BooleanExpression>.Success(BooleanExpression.Not(expression));

    }

    private Result<BooleanExpression> ParsePrimaryExpression()
    {
        switch (CurrentToken.Type)
        {
            case TokenType.Atom:
                var atomName = CurrentToken.Value;
                Advance();
                return Result<BooleanExpression>.Success(BooleanExpression.Atom(atomName));

            case TokenType.LeftParen:
                Advance(); // consume (
                var expressionResult = ParseOrExpression();
                if (expressionResult.IsFailure)
                    return expressionResult;

                if (CurrentToken.Type != TokenType.RightParen)
                {
                    return Result<BooleanExpression>.Failure($"Expected ')' at position {CurrentToken.Position}, but found '{CurrentToken.Value}'");
                }

                Advance(); // consume )
                return expressionResult;

            case TokenType.And:
            case TokenType.Or:
            case TokenType.Not:
            case TokenType.RightParen:
            case TokenType.EndOfInput:
            case TokenType.Error:
            default:
                return Result<BooleanExpression>.Failure($"Unexpected token '{CurrentToken.Value}' at position {CurrentToken.Position}. Expected atom, NOT, or '('");
        }
    }
}
