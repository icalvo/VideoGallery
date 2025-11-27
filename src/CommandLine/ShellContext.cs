using Icm;
using Icm.Commands;
using VideoGallery.CommandLine.Listing;
using VideoGallery.Library;

namespace VideoGallery.CommandLine;

public class CustomPrompt
{
    private readonly InteractivePrompt _ip;
    private readonly BasicStatus _basicStatus;
    private readonly AutocompleteStatus _completionStatus;

    public CustomPrompt()
    {
        _basicStatus = new BasicStatus();
        _completionStatus = new AutocompleteStatus();
        var h = new HistoryStatus();
        KeyMap[] keyMaps = [
            ..BasicStatus.DefaultKeyMap(_basicStatus),
            ..HistoryStatus.DefaultKeyMap(_basicStatus, h),
            ..AutocompleteStatus.DefaultKeyMap(_completionStatus, _basicStatus)
        ];
        _ip = new InteractivePrompt(keyMaps);
    }

    public string Prompt(string prompt, string[] completions)
    {
        _basicStatus.Prompt = $"{prompt}> ";
        _completionStatus.Completions = completions;
        var subCommand = _ip.Run();
        return subCommand;
    }
}
public class ShellContext
{
    private readonly CustomPrompt _ip;

    public ShellContext(GridSettings gridSettings, Video[] videos)
    {
        GridSettings = gridSettings;
        Videos = videos;
        _ip = new();
    }

    public GridSettings GridSettings { get; set; }

    public Video[] Videos { get; set; }

    public string Prompt(string[] completions) => _ip.Prompt(GridSettings.ToString(), completions);

    public Guid? SelectedVideo(int idx) =>
        Videos.ElementAtOrDefault(idx)?.Id;

    public override string ToString()
    {
        return $"{GridSettings}";
    }
}