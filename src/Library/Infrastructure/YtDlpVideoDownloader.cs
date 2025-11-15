using CliWrap;
using VideoGallery.Interfaces;

namespace VideoGallery.Library.Infrastructure;

public class YtDlpVideoDownloader : IVideoDownloader
{
    public async Task<string> Download(string url, CancellationToken ct = default)
    {
        var tempDir = Directory.CreateTempSubdirectory().FullName;
        var cmd = Cli.Wrap("yt-dlp").WithArguments(args => args.Add(url).Add("-o").Add("%(title)s-%(id)s.%(ext)s"))
            .WithWorkingDirectory(tempDir);
        cmd |= (Console.OpenStandardOutput(), Console.OpenStandardError());
        await cmd.ExecuteAsync(ct);
        return Directory.GetFiles(tempDir).First();
    }
}