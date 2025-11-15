namespace VideoGallery.CommandLine.Utils.Parsing;

public abstract record BaseParser<T> : IParser<T>
{
    public abstract ParserSyntax Syntax { get; }
    public ParseStatus<T> Parse(ParseStatus<T> former) => former.IsError ? former : RawParse(former);

    protected abstract ParseStatus<T> RawParse(ParseStatus<T> former);
}