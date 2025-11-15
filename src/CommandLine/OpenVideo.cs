using System.Diagnostics;
using Spectre.Console;
using Spectre.Console.Rendering;
using VideoGallery.Interfaces;
using VideoGallery.Library;
using VideoGallery.Library.Infrastructure;

namespace VideoGallery.CommandLine;

public class OpenVideo : ICommand
{
    private readonly VideoContext _context;
    private readonly ShellContext _shellContext;
    private readonly Options _options;

    public OpenVideo(VideoContext context, ShellContext shellContext, Options options)
    {
        _context = context;
        _shellContext = shellContext;
        _options = options;
    }

    public IRenderable Description()
    {
        return new Text("Opens a video with the default viewer");
    }

    public async Task Run(string[] args)
    {
        var videoIndexArg = args.ElementAtOrDefault(0);
        if (videoIndexArg == null || !int.TryParse(videoIndexArg, out var videoIndex))
        {
            throw new CommandArgumentException("Need an video number");
        }

        var video = await _context.Videos.FindAsync(_shellContext.SelectedVideo(videoIndex))
                    ?? throw new Exception("Video not found");

        IVideoManager videoManager = _options.Storage switch
        {
            "dropbox" => new DropboxVideoManager(new ConsoleDropboxClientFactory(Options.DropboxAppKey), _options.VideosFolder),
            "local" => new FileSystemVideoManager(_options.VideosFolder),
            _ => throw new Exception("Unknown storage")
        };
        var fileToOpen = await videoManager.GetVideoSharedLink(video.Filename);
        ProcessStartInfo psi = new ProcessStartInfo
        {
            FileName = fileToOpen,
            UseShellExecute = true,
            WindowStyle = ProcessWindowStyle.Normal
        };
        Process.Start(psi);
    }

    public IRenderable Syntax() => new Text("video_number");
}