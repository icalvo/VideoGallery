namespace VideoGallery.Library.Parsing;

public enum TokenType
{
    Atom,
    And,
    Or,
    Not,
    LeftParen,
    RightParen,
    EndOfInput,
    Error
}