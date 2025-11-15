namespace VideoGallery.CommandLine.Utils.Parsing;

public record DirectParser<T>(string RawSyntax, string Help, Func<ParseStatus<T>, ParseStatus<T>> Parser) : BaseParser<T>
{
    public override ParserSyntax Syntax => ParserSyntax.Direct(RawSyntax, Help);

    protected override ParseStatus<T> RawParse(ParseStatus<T> former)
    {
        return Parser(former);
    }
}