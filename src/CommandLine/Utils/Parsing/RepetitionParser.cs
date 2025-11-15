namespace VideoGallery.CommandLine.Utils.Parsing;

public record RepetitionParser<T>(string? Separator, IParser<T> SubParser) : BaseParser<T>
{
    public override ParserSyntax Syntax => ParserSyntax.Repetition(SubParser.Syntax, Separator);

    protected override ParseStatus<T> RawParse(ParseStatus<T> former)
    {
        ParseStatus<T> result = former;
        while (true)
        {
            var result2 = SubParser.Parse(result);
            if (result2.IsError) break;
            result = result2;
            if (Separator != null && result2.Args.ElementAtOrDefault(0) != Separator) break;

            if (Separator != null)
                result = result with { Args = result.Args[1..] };
        }

        return result;
    }
}