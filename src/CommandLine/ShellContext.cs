using Spectre.Console;
using VideoGallery.CommandLine.Listing;
using VideoGallery.Library;

namespace VideoGallery.CommandLine;

public class ShellContext
{
    public GridSettings GridSettings { get; set; }
    public Video[] Videos { get; set; }

    public string Prompt()
    {
        AnsiConsole.Write(ToString());
        var subCommand = AnsiConsole.Prompt(new TextPrompt<string>("> ").AllowEmpty());
        return subCommand;
    }
    public Guid? SelectedVideo(int idx) =>
        Videos.ElementAtOrDefault(idx)?.Id;

    public ShellContext(GridSettings gridSettings, Video[] videos)
    {
        GridSettings = gridSettings;
        Videos = videos;
    }

    public override string ToString()
    {
        return $"{GridSettings}";
    }
}