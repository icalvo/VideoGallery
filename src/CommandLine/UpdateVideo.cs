using Spectre.Console;
using Spectre.Console.Rendering;
using VideoGallery.Library;

namespace VideoGallery.CommandLine;

public class UpdateVideo : ICommand
{
    private readonly VideoContext _context;
    private readonly ShellContext _shellContext;

    public UpdateVideo(VideoContext context, ShellContext shellContext)

    {
        _context = context;
        _shellContext = shellContext;
    }

    public IRenderable Description()
    {
        return new Text("Downloads a video (if a URL is provided) and adds it to the DB and folder");
    }

    public async Task Run(string[] args)
    {
        var videoIndexArg = args.ElementAtOrDefault(0);
        if (videoIndexArg == null || !int.TryParse(videoIndexArg, out var videoIndex))
        {
            throw new CommandArgumentException("Need an video id");
        }

        var video = await _context.Videos.FindAsync(_shellContext.SelectedVideo(videoIndex))
                    ?? throw new Exception("Video not found");        

        AnsiConsole.MarkupLineInterpolated($"[yellow]File:[/] {video.Filename}");

        video.NumSequences = AskInteger("# of sequences:", video.NumSequences);
        video.Comments = AskText("[[Optional]] Comments:", video.Comments);

        await _context.SaveChangesAsync();
    }

    public IRenderable Syntax()
    {
        return new Markup("[yellow]URL[/]");
    }

    private static string AskCategory(string[] categories, string currentCategory)
    {
        var cat = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title($"Category (current: {currentCategory})")
                .PageSize(20)
                .MoreChoicesText("[grey](Move up and down to reveal more categories)[/]")
                .AddChoices(["NEW", ..categories]));

        return cat == "NEW" ? AskRequiredText("New category:") : cat;
    }

    private static bool AskRequiredBoolean(string prompt, bool currentValue) =>
        AnsiConsole.Prompt(new TextPrompt<bool>(prompt)
            .DefaultValue(currentValue)
            .AddChoice(true)
            .AddChoice(false)
            .WithConverter(choice => choice ? "y" : "n"));

    private static bool? AskBoolean(string prompt, bool? currentValue) =>
        AnsiConsole.Prompt(new TextPrompt<string>(prompt)
                .DefaultValue(currentValue switch
                {
                    true => "y",
                    false => "n",
                    null => "na"
                })
                .AddChoice("y")
                .AddChoice("n")
                .AddChoice("na")) switch
            {
                "y" => true,
                "n" => false,
                "na" => null,
                _ => throw new ArgumentOutOfRangeException()
            };

    private static string AskRequiredText(string prompt) =>
        AnsiConsole.Prompt(new TextPrompt<string>(prompt));

    private static string? AskText(string prompt, string? currentValue) =>
        AnsiConsole.Prompt(new TextPrompt<string?>(prompt).AllowEmpty().DefaultValue(currentValue));

    private static int AskInteger(string prompt, int currentValue) =>
        AnsiConsole.Prompt(new TextPrompt<int>(prompt).DefaultValue(currentValue));
}