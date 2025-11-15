namespace VideoGallery.CommandLine.Utils.Parsing;

public record OptionalParser<T>(IParser<T> SubParser, Func<T, T>? Default = null) : BaseParser<T>
{
    public override ParserSyntax Syntax => ParserSyntax.Optional(SubParser.Syntax);
    protected override ParseStatus<T> RawParse(ParseStatus<T> former)
    {
        var tryParse = SubParser.Parse(former);
        return
            tryParse.IsError
                ? Default == null
                    ? former
                    : former with { Value = Default(former.Value) }
                : tryParse;
    }
}