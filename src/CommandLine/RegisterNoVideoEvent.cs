using Spectre.Console;
using Spectre.Console.Rendering;
using VideoGallery.Library;

namespace VideoGallery.CommandLine;

public class RegisterNoVideoEvent : ICommand
{
    private readonly VideoContext _context;

    public RegisterNoVideoEvent(VideoContext context)
    {
        _context = context;
    }

    public IRenderable Description()
    {
        return new Text("Registers an event without a watched video");
    }

    public async Task Run(string[] args)
    {
        var date = AnsiConsole.Prompt(
            new SelectionPrompt<DateOnly>()
                .Title("When did the no video event happen?"));

        _context.NoVideoEvents.Add(new NoVideoEvent(date));
        await _context.SaveChangesAsync();
    }

    public IRenderable Syntax() => new Text("date");
}