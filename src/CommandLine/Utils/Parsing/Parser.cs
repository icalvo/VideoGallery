namespace VideoGallery.CommandLine.Utils.Parsing;

public static class Parser
{
    public static IParser<T> Either<T>(params IParser<T>[] parsers)
        => new EitherParser<T>(parsers);
    public static IParser<T> Constant<T>(string name, string help, T result)
        => new ConstantParser<T>(name, help, _ => result);
    public static IParser<T> Constant<T>(string name, string help, Func<T, T> parser)
        => new ConstantParser<T>(name, help, parser);
    public static IParser<T> Direct<T>(string syntax, string help, Func<ParseStatus<T>, ParseStatus<T>> parser)
        => new DirectParser<T>(syntax, help, parser);
    public static IParser<T> Concat<T>(
        IParser<T> arg, IParser<T> arg2) =>
        new ConcatParser<T>(arg, arg2);
    public static IParser<T> Concat<T>(
        string name,
        string help,
        IParser<T> arg) =>
        Concat(
            new ConstantParser<T>(name, help, x => x),
            arg);

    public static OptionalParser<T> Optional<T>(IParser<T> subParser, Func<T, T>? def = null) =>
        new (subParser, def);

    public static RepetitionParser<T> Repeat<T>(string? separator, IParser<T> subParser) =>
        new (separator, subParser);

    public static TransformParser<T, T2> Transform<T, T2>(
        IParser<T2> parserImplementation,
        T2 former2,
        Func<T, T2, T> f) => new(parserImplementation, former2, f);
}