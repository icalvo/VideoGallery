using Spectre.Console;
using Spectre.Console.Rendering;

namespace VideoGallery.CommandLine;

public class Exit : ICommand
{
    public IRenderable Description()
    {
        return new Text("Exits the application"); 
    }

    public Task Run(string[] args) => Task.CompletedTask;

    public IRenderable Syntax() => Text.Empty;
}