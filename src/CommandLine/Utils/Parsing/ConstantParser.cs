namespace VideoGallery.CommandLine.Utils.Parsing;

public record ConstantParser<T>(string Name, string Help, Func<T, T> Parser) : BaseParser<T>
{
    public override ParserSyntax Syntax => ParserSyntax.Constant(Name, Help);

    protected override ParseStatus<T> RawParse(ParseStatus<T> former) =>
        former.Args.ElementAtOrDefault(0) == Name
        ? new (Parser(former.Value), former.Args[1..])
        : former with { ErrorMessage = "Expected " + Name };
}