namespace VideoGallery.CommandLine.Utils.Parsing;

public record EitherSyntax(ParserSyntax[] SubParsers) : ParserSyntax;