using Spectre.Console;
using Spectre.Console.Rendering;
using VideoGallery.Library;

namespace VideoGallery.CommandLine;

public class WatchVideo : ICommand
{
    private readonly VideoContext _context;
    private readonly ShellContext _shellContext;

    public WatchVideo(VideoContext context, ShellContext shellContext)
    {
        _context = context;
        _shellContext = shellContext;
    }

    public IRenderable Description()
    {
        return new Text("Marks a video as watched in a specific date");
    }

    public async Task Run(string[] args)
    {
        var videoIndexArg = args.ElementAtOrDefault(0);
        if (videoIndexArg == null || !int.TryParse(videoIndexArg, out var videoIndex))
        {
            throw new CommandArgumentException("Need an video number");
        }
        
        var chosenVideo =
            await _context.Videos.FindAsync(_shellContext.SelectedVideo(videoIndex))
            ?? throw new Exception("Video not found");
        var watchedDate =
            ReadDate1(args)
            ?? ReadDate();

        chosenVideo.Watch(watchedDate);
        await _context.SaveChangesAsync();
    }

    public IRenderable Syntax() => new Text("video_number [watch_date]");

    private static DateOnly? ReadDate1(string[] args)
    {
        if (args.Length < 2) return null;
        if (!DateOnly.TryParse(args[1], out var wd))
        {
            throw new CommandArgumentException("Invalid date");
        }
        return wd;
    }

    private static DateOnly? ReadDate() =>
        AnsiConsole.Prompt(new TextPrompt<DateOnly?>("Watch date")
            .DefaultValue(DateOnly.FromDateTime(DateTime.Now)));
}