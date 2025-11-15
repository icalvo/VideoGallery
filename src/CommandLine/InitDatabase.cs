using Spectre.Console;
using Spectre.Console.Rendering;
using VideoGallery.Library;

namespace VideoGallery.CommandLine;

public class InitDatabase : ICommand
{
    private readonly VideoContext _context;

    public InitDatabase(VideoContext context)
    {
        _context = context;
    }

    public IRenderable Description()
    {
        return new Text("Initializes and migrates the DB");
    }

    public async Task Run(string[] args)
    {
        await _context.Database.EnsureCreatedAsync();
    }

    public IRenderable Syntax() => Text.Empty;
}