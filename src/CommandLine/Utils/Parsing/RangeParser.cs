namespace VideoGallery.CommandLine.Utils.Parsing;

public record ObjectRange<T>(T? Min, T? Max);

public record RangeParser<T>(IParser<T> SubParser) : BaseParser<ObjectRange<T>>
{

    public override ParserSyntax Syntax => ParserSyntax.Direct($"[{SubParser.Syntax}]..[{SubParser.Syntax}]", "Range");
    protected override ParseStatus<ObjectRange<T>> RawParse(ParseStatus<ObjectRange<T>> former)
    {
        if (former.Args.Length == 0)
        {
            return former with { Value = new (default, default) };
        }

        if (!former.Args.ElementAtOrDefault(0)?.Contains("..") ?? false)
            return former with { ErrorMessage = "Must be a range: 'A..', '..B', 'A..B'" };

        var endpoints = former.Args[0].Split("..");
        T? min = default;
        T? max = default;
        if (!string.IsNullOrEmpty(endpoints[0]))
        {
            min = SubParser.Parse(new([endpoints[0]])).Value;
        }
        if (!string.IsNullOrEmpty(endpoints[1]))
        {
            max = SubParser.Parse(new([endpoints[1]])).Value;
        }
            
        return former with
        {
            Value = new (min, max),
            Args = former.Args[1..]
        };
    }
}