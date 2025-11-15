namespace VideoGallery.CommandLine.Utils.Parsing;

public record TransformParser<T, T2> : BaseParser<T>
{
    private readonly T2 _former2;
    private readonly Func<T, T2, T> _f;
    private readonly IParser<T2> _parserImplementation;

    public TransformParser(
        IParser<T2> parserImplementation,
        T2 former2,
        Func<T, T2, T> f)
    {
        _former2 = former2;
        _f = f;
        _parserImplementation = parserImplementation;
    }
    public override ParserSyntax Syntax => ParserSyntax.Transform(_parserImplementation.Syntax);

    protected override ParseStatus<T> RawParse(ParseStatus<T> former)
    {
        var curr = _parserImplementation.Parse(new (_former2, former.Args));
        return new (_f(former.Value, curr.Value), curr.Args, curr.ErrorMessage);
    }
}