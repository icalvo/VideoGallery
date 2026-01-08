using CliWrap;
using CliWrap.Buffered;
using VideoGallery.Interfaces;

namespace VideoGallery.Library.Infrastructure;

public class FileSystemVideoManager : IVideoManager
{
    private readonly string _videosFolder;

    public FileSystemVideoManager(string videosFolder)
    {
        _videosFolder = videosFolder;
    }

    public Task Upload(string filePath, CancellationToken ct = default)
    {
        var fileName = Path.GetFileName(filePath);
        var destination = Path.Combine(_videosFolder, $"{fileName}");
        File.Move($"{filePath}", destination);
        Console.WriteLine($"Moved to {destination}");
        return Task.CompletedTask;
    }

    public async Task<TimeSpan> GetDuration(string filePath, CancellationToken ct = default)
    public async Task<TimeSpan> GetDuration(string video, CancellationToken ct = default)
    {
        var filePath = Path.Combine(_videosFolder, $"{video}");
        var cmd = Cli.Wrap("ffprobe").WithArguments(args =>
            args.Add("-v").Add("error").Add("-show_entries").Add("format=duration").Add("-sexagesimal").Add("-of")
                .Add("default=noprint_wrappers=1:nokey=1").Add(filePath));
        var result = await cmd.ExecuteBufferedAsync(ct);
        var t = TimeSpan.Parse(result.StandardOutput.Trim());
        return t;
    }

    public Task<string> GetVideoSharedLink(string video, CancellationToken ct = default)
    {
        return Task.FromResult(Path.Combine(_videosFolder, video));
    }

    public Task<Stream> GetThumbnail(string video, CancellationToken ct = default)
    {
        throw new NotSupportedException("Not supported on file system");
    }
}