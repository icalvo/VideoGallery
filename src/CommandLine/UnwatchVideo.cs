using Spectre.Console;
using Spectre.Console.Rendering;
using VideoGallery.Library;

namespace VideoGallery.CommandLine;

public class UnwatchVideo : ICommand
{
    private readonly VideoContext _context;
    private readonly ShellContext _shellContext;

    public UnwatchVideo(VideoContext context, ShellContext shellContext)
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

        chosenVideo.Unwatch();
        await _context.SaveChangesAsync();
    }

    public IRenderable Syntax() => new Text("video_number");
}