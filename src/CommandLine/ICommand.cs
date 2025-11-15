using Spectre.Console.Rendering;

namespace VideoGallery.CommandLine;

public interface ICommand
{
    IRenderable Description();
    Task Run(string[] args);
    IRenderable Syntax();
}