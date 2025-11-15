namespace VideoGallery.CommandLine.Utils.Parsing;

public record ConcatSyntax(ParserSyntax[] SubParsers) : ParserSyntax;