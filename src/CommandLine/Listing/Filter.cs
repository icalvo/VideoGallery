using Spectre.Console;
using Spectre.Console.Rendering;
using VideoGallery.Library;

namespace VideoGallery.CommandLine.Listing;

public class Filter : ICommand
{
    private readonly VideoContext _context;
    private readonly ShellContext _shellContext;

    public Filter(VideoContext context, ShellContext shellContext)
    {
        _context = context;
        _shellContext = shellContext;
    }


    public IRenderable Description() =>
        new Rows([
            new Text("Modifies the filtering/sorting"),
            ..GridSettings.ParseMap.Syntax.SyntaxDescriptions()
        ]);

    public async Task Run(string[] args)
    {
        _shellContext.GridSettings = _shellContext.GridSettings.Parse(args);
        
        _shellContext.Videos = await ListVideoService.ShowVideos(_context, _shellContext.GridSettings);
    }

    public IRenderable Syntax() => GridSettings.ParseMap.Syntax.SyntaxMarkup();
}