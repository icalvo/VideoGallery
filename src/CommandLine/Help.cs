using Spectre.Console;
using Spectre.Console.Rendering;

namespace VideoGallery.CommandLine;

public class Help : ICommand
{
    public Verb[] Verbs { get; }
    public Help(IEnumerable<Verb> availableVerbs)
    {
        Verb selfVerb = new("help", () => this);
        Verbs = [ ..availableVerbs, selfVerb ];
    }

    public IRenderable Description()
    {
        return new Text("Show help");
    }

    public Task Run(string[] args)
    {
        var verbName = args.ElementAtOrDefault(0);

        if (verbName == null)
        {
            foreach (var verb in Verbs)
            {
                AnsiConsole.Write(verb.Name + ": ");
                AnsiConsole.Write(verb.BuildCommand().Description());
                AnsiConsole.WriteLine();
            }
        }
        else
        {
            var cmd = new ExecuteVerb(Verbs);
            if (cmd.CanExecute(args))
            {
                var matchingVerb = cmd.MatchingVerb(verbName);
                var matchingCmd = matchingVerb.BuildCommand();
                AnsiConsole.MarkupInterpolated($"[green]{matchingVerb.Name}[/] ");
                AnsiConsole.Write(matchingCmd.Syntax());
                AnsiConsole.WriteLine();
                AnsiConsole.Write(matchingCmd.Description());
                AnsiConsole.WriteLine();
            }
            else
            {
                AnsiConsole.WriteLine($"Unknown verb {verbName}");
            }
        }

        return Task.CompletedTask;
    }

    public IRenderable Syntax() => Text.Empty;
}