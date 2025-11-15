namespace VideoGallery.CommandLine.Utils.Parsing;

public record IntParser(string Name, string Help) : BaseParser<int>
{
    public override ParserSyntax Syntax => ParserSyntax.Direct(Name.ToUpperInvariant(), Help);
    protected override ParseStatus<int> RawParse(ParseStatus<int> former)
    {
        if (int.TryParse(former.Args.ElementAtOrDefault(0), out var idx))
        {
            return former with { Value = idx, Args = former.Args[1..] };
        }
        
        return former with { ErrorMessage = "Not an integer" };
    }
}