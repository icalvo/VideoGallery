using System.Globalization;
using VideoGallery.Library;

namespace VideoGallery.CommandLine.Utils.Parsing;

public record TimeSpanParser(string[] Formats) : BaseParser<TimeSpan>
{
    public override ParserSyntax Syntax => ParserSyntax.Direct(Formats.StrJoin("|").ToUpperInvariant(), "A time span");
    protected override ParseStatus<TimeSpan> RawParse(ParseStatus<TimeSpan> former)
    {
        if (TimeSpan.TryParseExact(former.Args.ElementAtOrDefault(0), Formats, CultureInfo.InvariantCulture, out var idx))
        {
            return former with { Value = idx, Args = former.Args[1..] };
        }
        
        return former with { ErrorMessage = "Not a time span" };
    }
}