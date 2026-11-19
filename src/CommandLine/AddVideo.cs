using Spectre.Console;
using Spectre.Console.Rendering;
using VideoGallery.Interfaces;
using VideoGallery.Library;
using VideoGallery.Library.Infrastructure;
using VideoGallery.Library.Parsing;

namespace VideoGallery.CommandLine;

public class AddVideo : ICommand
{
    private readonly Options _options;
    private readonly Application _application;

    public AddVideo(Options options, Application application)
    {
        _options = options;
        _application = application;
    }

    public IRenderable Description() =>
        new Text("Downloads a video (if a URL is provided), and adds it to the video system");

    public async Task Run(string[] args)
    {
        var urlOrFilePath = args.ElementAtOrDefault(0);
        if (urlOrFilePath == null)
        {
            throw new CommandArgumentException("Need a URL or filename");
        }

        IVideoManager videoManager = VideoManagerFactory.Create(_options);
        var (fileName, import) = await PrepareImport(urlOrFilePath);

        AnsiConsole.MarkupLineInterpolated($"[yellow]File to be imported:[/] {fileName}");
        if ((await _application.GetVideos(
                new QuerySpec(
                    "\"" + fileName.Replace("\"", "\\\"") + "\""),
                CancellationToken.None)).Length != 0)
        {
            AnsiConsole.MarkupLine("[red]You already have this video in the DB![/]");
            return;
        }

        await import(videoManager);

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

    private async Task<(string fileName, Func<IVideoManager, Task> import)> PrepareImport(string urlOrFilePath)
    {
        if (!Uri.IsWellFormedUriString(urlOrFilePath, UriKind.Absolute))
        {
            return (Path.GetFileName(urlOrFilePath), async videoManager =>
            {
                AnsiConsole.WriteLine("Uploading to cloud...");
                await videoManager.Upload(urlOrFilePath);
                AnsiConsole.WriteLine("Uploaded to cloud");
            });
        }

        var downloader = new YtDlpVideoDownloader();
        var formats = await AnsiConsole.Status()
            .StartAsync("Fetching formats...", async _ => await downloader.ListFormats(urlOrFilePath));

        string? format = null;
        if (formats.Count > 1)
            format = AskFormat(formats);

        var fileName = await downloader.GetOutputFileName(urlOrFilePath, format);
        if (CanStream(format, formats.Count))
        {
            return (fileName, async videoManager =>
            {
                AnsiConsole.WriteLine("Streaming download directly to storage...");
                await downloader.StreamDownload(urlOrFilePath,
                    (stream, ct) => videoManager.Upload(stream, fileName, ct), format);
                AnsiConsole.WriteLine("Uploaded to cloud");
            });
        }

        return (fileName, async videoManager =>
        {
            AnsiConsole.WriteLine("Downloading video...");
            var tempFile = await downloader.Download(urlOrFilePath, format);
            using var _ = new TempFile(tempFile);
            AnsiConsole.WriteLine("Uploading to cloud...");
            await videoManager.Upload(tempFile);
            AnsiConsole.WriteLine("Uploaded to cloud");
        });
    }

    private bool CanStream(string? format, int formatCount) =>
        _options.Storage == "dropbox"
        && (format is null ? formatCount <= 1 : !format.Contains('+'));

    private static string? AskFormat(IReadOnlyList<VideoFormat> formats)
    {
        var table = new Table()
            .Border(TableBorder.Rounded)
            .AddColumn("ID")
            .AddColumn("EXT")
            .AddColumn("Resolution")
            .AddColumn("Size")
            .AddColumn("Note");

        foreach (var f in formats)
            table.AddRow(Markup.Escape(f.Id), Markup.Escape(f.Ext), Markup.Escape(f.Resolution),
                Markup.Escape(f.Size), Markup.Escape(f.Note));

        AnsiConsole.Write(table);

        var validIds = formats.Select(f => f.Id).ToHashSet(StringComparer.Ordinal);
        var combined = formats.LastOrDefault(f => f.VCodec is not "none" && f.ACodec is not "none");
        var videoOnly = formats.LastOrDefault(f => f.VCodec is not "none" && f.ACodec is "none");
        var audioOnly = formats.LastOrDefault(f => f.ACodec is not "none" && f.VCodec is "none");

        var examples = new List<string>();
        if (combined is not null)
            examples.Add(combined.Id);
        else if (formats.Count > 0)
            examples.Add(formats[^1].Id);
        if (videoOnly is not null && audioOnly is not null)
            examples.Add($"{videoOnly.Id}+{audioOnly.Id}");
        var exampleText = string.Join(", ", examples);

        return AnsiConsole.Prompt(
            new TextPrompt<string>($"Select format ID (e.g. {exampleText}, or Enter = best)")
                .AllowEmpty()
                .Validate(choice =>
                {
                    if (string.IsNullOrWhiteSpace(choice))
                        return ValidationResult.Success();

                    var parts = choice.Split('+', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
                    var invalid = parts.Where(p => !validIds.Contains(p)).ToArray();
                    return invalid.Length > 0
                        ? ValidationResult.Error($"Unknown format(s): {string.Join(", ", invalid)}. Choose an ID from the table.")
                        : ValidationResult.Success();
                })) switch
        {
            { Length: > 0 } choice => string.Join('+',
                choice.Split('+', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)),
            _ => null
        };
    }

    public IRenderable Syntax()
    {
        return new Markup("[yellow]URL[/]");
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

    private static string AskRequiredText(string prompt) => AnsiConsole.Prompt(new TextPrompt<string>(prompt));
    private static string? AskText(string prompt) => AnsiConsole.Prompt(new TextPrompt<string?>(prompt).AllowEmpty());
    private static int AskInteger(string prompt) => AnsiConsole.Prompt(new TextPrompt<int>(prompt));
}