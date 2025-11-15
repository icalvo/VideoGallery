namespace VideoGallery.CommandLine.Utils.Parsing;

public record RepetitionSyntax(ParserSyntax SubParser, string? Separator) : ParserSyntax;