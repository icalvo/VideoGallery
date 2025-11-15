namespace VideoGallery.CommandLine.Utils.Parsing;

public record ParserSyntax
{
    public static ParserSyntax Direct(string syntax, string help) => new DirectSyntax(syntax, help); 
    public static ParserSyntax Optional(ParserSyntax subparser) => new OptionalSyntax(subparser); 
    public static ParserSyntax Repetition(ParserSyntax subparser, string? separator) => new RepetitionSyntax(subparser, separator); 
    public static ParserSyntax Either(params IEnumerable<ParserSyntax> subparsers) => new EitherSyntax(subparsers.ToArray()); 
    public static ParserSyntax Constant(string name, string help) => new ConstantSyntax(name, help); 
    public static ParserSyntax Concat(params IEnumerable<ParserSyntax> subparsers) => new ConcatSyntax(subparsers.ToArray()); 
    public static ParserSyntax Transform(ParserSyntax subparser) => new TransformSyntax(subparser); 
}