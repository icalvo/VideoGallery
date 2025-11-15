namespace VideoGallery.CommandLine.Utils.Parsing;

public record EitherParser<T>(params IParser<T>[] SubArguments) : BaseParser<T>
{
    public override ParserSyntax Syntax => ParserSyntax.Either(SubArguments.Select(x => x.Syntax));

    protected override ParseStatus<T> RawParse(ParseStatus<T> former)
    {
        var match = SubArguments
            .Select(x => x.Parse(former))
            .FirstOrDefault(x => !x.IsError);

        if (match == null) return former with { ErrorMessage = "No matches" };

        return match;
    }
}