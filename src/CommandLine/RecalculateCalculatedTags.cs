using Spectre.Console;
using Spectre.Console.Rendering;
using VideoGallery.Library;

namespace VideoGallery.CommandLine;

public class RecalculateCalculatedTags : ICommand
{
    private readonly Application _application;

    public RecalculateCalculatedTags(Application application)
    {
        _application = application;
    }


    public IRenderable Description()
    {
        return new Text("Recalculates the calculated tags for all videos");
    }

    public Task Run(string[] args)
    {
        return _application.RecalculateCalculatedTags(CancellationToken.None);
    }

    public IRenderable Syntax() => Text.Empty;
}