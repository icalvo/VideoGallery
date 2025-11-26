using Spectre.Console;
using Spectre.Console.Rendering;
using VideoGallery.Library;

namespace VideoGallery.CommandLine.Listing;

public class ListVideos(VideoContext context) : ICommand
{
    public IRenderable Description()
    {
        return new Text("List videos with specific filtering/sorting");
    }

    public async Task Run(string[] args)
    {
        var defaultSettings = new GridSettings(
            WatchedVideoFilter.All,
            [],
            PrintIndexes: false);

        var withParsedArgs = defaultSettings.Parse(args);
        await ListVideoService.ShowVideos(context, withParsedArgs);
    }

    public IRenderable Syntax() => GridSettings.ParseMap.Syntax.SyntaxMarkup();
}