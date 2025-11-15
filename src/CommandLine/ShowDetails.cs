using Spectre.Console;
using Spectre.Console.Rendering;
using VideoGallery.Library;

namespace VideoGallery.CommandLine;

public class ShowDetails : ICommand
{
    private readonly VideoContext _context;
    private readonly ShellContext _shellContext;

    public ShowDetails(VideoContext context, ShellContext shellContext)
    {
        _context = context;
        _shellContext = shellContext;
    }

    public IRenderable Description()
    {
        return new Text("Show video details");
    }

    public async Task Run(string[] args)
    {
        var videoIndexArg = args.ElementAtOrDefault(0);
        if (videoIndexArg == null || !int.TryParse(videoIndexArg, out var videoIndex))
        {
            throw new CommandArgumentException("Need an video number");
        }

        var video = await _context.Videos.FindAsync(_shellContext.SelectedVideo(videoIndex))
            ?? throw new Exception("Video not found");
        var grid = new Grid();

        grid.AddColumns(2);
        
        AddRow("Filename", video.Filename);
        AddRow("Duration", video.Duration, null, x => x.ToString("mm':'ss"));
        AddRow("NumSequences", video.NumSequences);
        AddRow("Actors", video.Actors);
        AddRow("Tags", video.TagsRep);
        AddRow("Comments", video.Comments);
        AddRow("ViewDate", video.LastViewDate);

        AddRow(
            "Average sequence duration",
            video.AverageSequenceDuration(),
            x => x > TimeSpan.FromMinutes(5),
            x => x.ToString("mm':'ss"));
        AnsiConsole.Write(grid);
        
        T AddRow<T>(string title, T value, Func<T, bool>? warning = null, Func<T, string>? toString = null)
        {
            if (warning?.Invoke(value) ?? false)
            {
                grid.AddRow(
                    new Text(title, new Style(Color.Red)),
                    new Text((toString ?? ToStringDefault)(value), new Style(Color.Red)));
            }
            else
            {
                grid.AddRow(title, (toString ?? ToStringDefault)(value));
            }

            return value;

            string ToStringDefault(T v) => v?.ToString() ?? "";
        }
    }

    public IRenderable Syntax() => new Text("video_number");
}