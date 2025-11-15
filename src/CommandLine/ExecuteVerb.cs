using Spectre.Console;
using Spectre.Console.Rendering;
using VideoGallery.Library;

namespace VideoGallery.CommandLine;

public class ExecuteVerb : ICommand
{
    private readonly Verb[] _verbs;
    public ExecuteVerb(params IEnumerable<Verb> verbs)
    {
        var helpCommand = new Help(verbs);
        _verbs = helpCommand.Verbs;
    }

    public IRenderable Description() => Text.Empty;

    public async Task Run(string[] args)
    {
        var verbName = args.ElementAtOrDefault(0);
        if (verbName == null)
        {
            throw new CommandArgumentException($"I need a verb ({_verbs.Select(x => x.Name).StrJoin(", ")})");
        }

        var verb = MatchingVerb(verbName);

        var command = verb.BuildCommand();
        await command.Run(args[1..]);
    }

    public IRenderable Syntax() => Text.Empty;

    public Verb MatchingVerb(string arg)
    {
        var matches = _verbs.Where(x => x.Name.StartsWith(arg)).ToArray();
        return matches.Length switch
        {
            0 => throw new CommandArgumentException(
                $"Unknown verb {arg}, try one of: {_verbs.Select(v => v.Name).StrJoin(",")}"),
            1 => matches[0],
            _ => throw new CommandArgumentException(
                $"Ambiguous verb {arg}, could be: {matches.Select(v => v.Name).StrJoin(",")}")
        };
    }

    public bool CanExecute(string[] args)
    {
        return args.Length > 0 && _verbs.Any(x => x.Name == args[0]);
    }
}