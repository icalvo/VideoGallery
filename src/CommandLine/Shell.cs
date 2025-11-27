using Icm;
using Icm.Commands;
using Spectre.Console;
using Spectre.Console.Rendering;
using VideoGallery.CommandLine.Listing;
using VideoGallery.Library;

namespace VideoGallery.CommandLine;

public class Shell : ICommand
{
    private readonly VideoContext _context;
    private readonly Func<ShellContext, Verb[]> _shellVerbsBuilder;
    private readonly Verb[] _generalVerbs;

    public Shell(
        VideoContext context,
        Func<ShellContext, Verb[]> shellVerbsBuilder,
        IEnumerable<Verb> generalVerbs)
    {
        _context = context;
        _shellVerbsBuilder = shellVerbsBuilder;
        _generalVerbs = generalVerbs.ToArray();
    }

    public IRenderable Description() => Text.Empty;

    public async Task Run(string[] args)
    {
        var shellContext = await BuildShellContext();
        var shellVerbs = _shellVerbsBuilder(shellContext);
        var allVerbs = _generalVerbs.Concat(shellVerbs).ToArray();
        var cmd = new ExecuteVerb(allVerbs);
        while (true)
        {
            var completions = allVerbs.Select(v => v.Name).ToArray();
            var subCommand = shellContext.Prompt(completions);
            
            if (string.IsNullOrWhiteSpace(subCommand)) return;
            
            var subargs = Tokenizer.TokenizeCommandLineToStringArray(subCommand);
            try
            {
                var executedCommand = await cmd.RunAndReturnCommand(subargs);
                if (executedCommand is Exit) return;
            }
            catch (CommandArgumentException ex)
            {
                AnsiConsole.Write(new Text(ex.Message, new Style(Color.Red)));
                AnsiConsole.WriteLine();
            }
            catch (Exception ex)
            {
                AnsiConsole.WriteException(ex);
            }
        }
    }

    public IRenderable Syntax() => Text.Empty;

    private async Task<ShellContext> BuildShellContext()
    {
        var settings = new GridSettings(
            WatchedVideoFilter.Pending,
            [SortField.Duration, SortField.Name],
            PrintIndexes: true);
        var shellContext = new ShellContext(
            settings,
            await ListVideoService.ShowVideos(_context, settings));
        return shellContext;
    }
}