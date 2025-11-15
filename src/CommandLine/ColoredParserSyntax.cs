using Spectre.Console;
using Spectre.Console.Rendering;
using VideoGallery.CommandLine.Utils.Parsing;
using VideoGallery.Library;

namespace VideoGallery.CommandLine;

public static class ColoredParserSyntax
{
    public static IRenderable SyntaxMarkup(this ParserSyntax px) => new Markup(SyntaxMarkupAux(px));
    
    private static string SyntaxMarkupAux(this ParserSyntax px) =>
        px switch
        {
            DirectSyntax p =>
                p.Syntax switch
                    {
                        "Duration" => "[blue]" + "[M:SS]..[M:SS]".EscapeMarkup() + "[/]",
                        _ => $"[yellow]{p.Syntax.EscapeMarkup()}[/]"
                    },
            OptionalSyntax p => $"[red][[[/]{SyntaxMarkupAux(p.SubParser)}[red]]][/]",
            RepetitionSyntax p =>
                p.Separator == null
                ? $"[red]{{[/]{SyntaxMarkupAux(p.SubParser)} [red]...}}[/]"
                : $"[red]{{[/]{SyntaxMarkupAux(p.SubParser)} [green]{p.Separator}[/][red]...}}[/]",
            EitherSyntax p => p.SubParsers.Select(SyntaxMarkupAux).StrJoin("[red]|[/]"),
            ConstantSyntax p => $"[green]{p.Name.EscapeMarkup()}[/]",
            ConcatSyntax p => p.SubParsers.Select(SyntaxMarkupAux).StrJoin(" "),
            TransformSyntax p => SyntaxMarkupAux(p.SubParser),
            _ => throw new ArgumentOutOfRangeException(nameof(px), "Unrecognized parser kind")
        };

    public static ParserSyntax[] Leaves(this ParserSyntax px) =>
        px switch
        {
            DirectSyntax p => [p],
            ConstantSyntax p => [p],
            OptionalSyntax p => Leaves(p.SubParser),
            RepetitionSyntax p => Leaves(p.SubParser),
            EitherSyntax p => p.SubParsers.SelectMany(Leaves).ToArray(),
            ConcatSyntax p => p.SubParsers.SelectMany(Leaves).ToArray(),
            TransformSyntax p => Leaves(p.SubParser),
            _ => throw new ArgumentOutOfRangeException(nameof(px), "Unrecognized parser kind")
        };

    public static string SyntaxHelp(this ParserSyntax px) =>
        px switch
        {
            DirectSyntax p => p.Help,
            ConstantSyntax p => p.Help,
            _ => ""
        };
    public static string SyntaxName(this ParserSyntax px) =>
        px switch
        {
            DirectSyntax p => p.Syntax,
            ConstantSyntax p => p.Name,
            _ => ""
        };

    public static IEnumerable<IRenderable> SyntaxDescriptions(this ParserSyntax px) =>
        Leaves(px).Select(l => new Markup($"[blue]{SyntaxName(l).EscapeMarkup()}[/]: {SyntaxHelp(l).EscapeMarkup()}"));
}