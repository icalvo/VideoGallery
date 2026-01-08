using Spectre.Console;
using Spectre.Console.Rendering;
using VideoGallery.Interfaces;
using VideoGallery.Library;
using VideoGallery.Library.Infrastructure;
using VideoGallery.Library.Parsing;

namespace VideoGallery.CommandLine;

public class RegisterVideo : ICommand
{
    private readonly Options _options;
    private readonly Application _application;

    public RegisterVideo(Options options, Application application)
    {
        _options = options;
        _application = application;
    }

    public IRenderable Description() =>
        new Text("Registers a video already present in the media storage, in the video system");

    public async Task Run(string[] args)
    {
        var fileName = args.ElementAtOrDefault(0);
        if (fileName == null)
        {
            throw new CommandArgumentException("Need a filename");
        }

        AnsiConsole.MarkupLineInterpolated($"[yellow]File to be imported:[/] {fileName}");
        if ((await _application.GetVideos(
                new QuerySpec(
                    "\"" + fileName.Replace("\"", "\\\"") + "\""),
                CancellationToken.None)).Length != 0)
        {
            AnsiConsole.MarkupLine("[red]You already have this video in the DB![/]");
            return;
        }
        
        IVideoManager videoManager = _options.Storage switch
        {
            "dropbox" => new DropboxVideoManager(new ConsoleDropboxClientFactory(Options.DropboxAppKey),
                _options.VideosFolder),
            "local" => new FileSystemVideoManager(_options.VideosFolder),
            _ => throw new Exception("Unknown storage")
        };

        if (!await videoManager.Exists(fileName))
        {
            throw new CommandArgumentException("File does not exist");
        }

        TimeSpan duration = await videoManager.GetDuration(fileName);

        AnsiConsole.WriteLine($"Duration: {duration:g}");
        var newVideo = new Video(id: Guid.NewGuid(), filename: fileName, duration: duration,
            numSequences: AskInteger("# of sequences:"), comments: AskText("[[Optional]] Comments:"));
        var allTags = await _application.GetAllTagNames(CancellationToken.None);
        var tagProposals = allTags.Select(tn => new TagProposal(tn)).ToArray();
        var tags = AskTags(tagProposals).Select(t => t.Name).ToArray();
        var error = await _application.AddVideo(newVideo, tags, CancellationToken.None);
        if (error is not null)
        {
            AnsiConsole.MarkupLine($"[red]{error}[/]");
            return;
        }
        AnsiConsole.WriteLine($"Video added to DB with id {newVideo.Id}");
    }

    public IRenderable Syntax()
    {
        return new Markup("[yellow]filename[/]");
    }

    private readonly CustomPrompt _ip = new();
    private TagProposal[] AskTags(TagProposal[] allTags)
    {
        string[] tagNames = [];
        var forPrint = allTags.GroupBy(x => x.TagCategoryId)
            .OrderBy(x => x.Key)
            .Index()
            .Select(x => (x.Index % 2 == 0 ? "yellow" : "green", x.Item.Select(t => t.Name).Order()));
        foreach (var (color, tags) in forPrint)
        {
            AnsiConsole.MarkupLineInterpolated($"[{color}]{string.Join(", ", tags)}[/]");
        }
        var allTagNames = allTags.Select(t => t.Name).ToArray();
        TagProposal[] superTags;
        while (true)
        {
            var tagInput = _ip.Prompt("Tag list, comma-separated. Remove with -tag", allTagNames);
            var tagInputArray =
                tagInput.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            var removals = tagInputArray.Where(ti => ti.StartsWith('-')).Select(ti => ti[1..]).ToArray();
            var additions = tagInputArray.Where(ti => !ti.StartsWith('-')).ToArray();
            tagNames = tagNames.Except(removals).Union(additions).ToArray();
            var existingTags = allTags.Where(t => tagNames.Contains(t.Name)).ToArray();
            var newTagNames = tagNames.Where(tn => allTags.All(t => t.Name != tn)).ToArray();
            superTags = [..existingTags, ..newTagNames.Select(x => new TagProposal(x))];
            if (existingTags.Length > 0)
                AnsiConsole.MarkupLineInterpolated($"[green]{string.Join(", ", existingTags.Select(t => t.Name).Order())}[/]");
            if (newTagNames.Length > 0)
                AnsiConsole.MarkupLineInterpolated($"[cyan]{string.Join(", ", newTagNames.Order())}[/]");
            var error = _application.ValidateTags(superTags);
            if (error is not null)
            {
                AnsiConsole.MarkupLineInterpolated($"[red]{error}[/]");
                continue;
            }
            
            if (newTagNames.Length > 0 && !AskRequiredBoolean("There will be new tags, are you sure?"))
            {
                continue;
            }

            if (newTagNames.Length == 0 && !AskRequiredBoolean("Are you sure?"))
            {
                continue;
            }

            break;
        }

        return superTags;
    }

    private static bool AskRequiredBoolean(string prompt) =>
        AnsiConsole.Prompt(new TextPrompt<bool>(prompt).AddChoice(true).AddChoice(false)
            .WithConverter(choice => choice ? "y" : "n"));

    private static string? AskText(string prompt) => AnsiConsole.Prompt(new TextPrompt<string?>(prompt).AllowEmpty());
    private static int AskInteger(string prompt) => AnsiConsole.Prompt(new TextPrompt<int>(prompt));
}