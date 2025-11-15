namespace VideoGallery.CommandLine.Utils.Parsing;

public record ConcatParser<T>(IParser<T> Parser1, IParser<T> Parser2) : BaseParser<T>
{
    public override ParserSyntax Syntax => ParserSyntax.Concat(Parser1.Syntax, Parser2.Syntax);

    protected override ParseStatus<T> RawParse(ParseStatus<T> former) =>
        Parser2.Parse(Parser1.Parse(former));
}