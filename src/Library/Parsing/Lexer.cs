using System.Text;

namespace VideoGallery.Library.Parsing;

public static class Lexer
{
    private enum State
    {
        OutWord,
        InWord,
        InQuotes,
    }
    public static Result<List<Token>> Tokenize(string input, string andOperator, string orOperator, string notOperator)
    {
        var tokens = new List<Token>();
        var position = 0;
        var state = State.OutWord;
        var wordStart = -1;
        var word = new StringBuilder();
        while (position < input.Length)
        {
            if (state == State.OutWord)
            {
                // Skip whitespace
                if (char.IsWhiteSpace(input[position]))
                {
                    position++;
                    continue;
                }

                // Check for parentheses
                if (input[position] == '(')
                {
                    tokens.Add(new Token(TokenType.LeftParen, "(", position));
                    position++;
                    continue;
                }

                if (input[position] == ')')
                {
                    tokens.Add(new Token(TokenType.RightParen, ")", position));
                    position++;
                    continue;
                }

                if (input[position] == '"')
                {
                    state = State.InQuotes;
                    position++;
                }
                else
                    state = State.InWord;
                wordStart = position;
                word.Clear();
                continue;
            }
            else if (state == State.InWord)
            {
                if (input[position] == '"')
                {
                    state = State.InQuotes;
                    position++;
                    continue;
                }

                // Check for parentheses
                if (input[position] == '(')
                {
                    FinishWord();
                    state = State.OutWord;
                    tokens.Add(new Token(TokenType.LeftParen, "(", position));
                    position++;
                    continue;
                }

                if (input[position] == ')')
                {
                    FinishWord();
                    state = State.OutWord;
                    tokens.Add(new Token(TokenType.RightParen, ")", position));
                    position++;
                    continue;
                }
                if (char.IsWhiteSpace(input[position]))
                {
                    FinishWord();
                    state = State.OutWord;
                    position++;
                    continue;
                }
                
                word.Append(input[position]);
                position++;
            }
            else if (state == State.InQuotes)
            {
                if (input[position] == '"')
                {
                    state = State.InWord;
                    position++;
                    continue;
                }

                if (input[position] == '\\')
                {
                    position++;
                    if (position >= input.Length)
                        return Result<List<Token>>.Failure($"Unexpected end of input while parsing string literal");
                }
                word.Append(input[position]);
                position++;
            }
        }

        if (state != State.OutWord) FinishWord();
        tokens.Add(new Token(TokenType.EndOfInput, "", position));
        return Result<List<Token>>.Success(tokens);
        void FinishWord()
        {
            var wordStr = word.ToString();

            Token? token;
            if (string.Equals(wordStr, andOperator, StringComparison.InvariantCultureIgnoreCase))
                token = new Token(TokenType.And, wordStr, wordStart);
            else if (string.Equals(wordStr, orOperator, StringComparison.InvariantCultureIgnoreCase))
                token = new Token(TokenType.Or, wordStr, wordStart);
            else if (string.Equals(wordStr, notOperator, StringComparison.InvariantCultureIgnoreCase))
                token = new Token(TokenType.Not, wordStr, wordStart);
            else 
                token = new Token(TokenType.Atom, wordStr, wordStart);

            tokens.Add(token);
            word.Clear();
        }
    }
}
