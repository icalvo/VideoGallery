namespace VideoGallery.CommandLine.Utils.Parsing;

public interface IParser<T>
{
    ParserSyntax Syntax { get; }
    ParseStatus<T> Parse(ParseStatus<T> former);
}