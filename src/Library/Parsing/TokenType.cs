namespace VideoGallery.Library.Parsing;

public enum TokenType
{
    Atom,
    And,
    Or,
    Implies,
    Not,
    LeftParen,
    RightParen,
    EndOfInput,
    Error
}