using System.Text.Json;
using CliWrap;
using CliWrap.Buffered;
using VideoGallery.Interfaces;

namespace VideoGallery.Library.Infrastructure;

public class YtDlpVideoDownloader : IVideoDownloader
{
    private const string OutputTemplate = "%(title)s-%(id)s.%(ext)s";

    public async Task<IReadOnlyList<VideoFormat>> ListFormats(string url, CancellationToken ct = default)
    {
        var cmd = Cli.Wrap("yt-dlp").WithArguments(args => args
            .Add("-J")
            .Add("--no-download")
            .Add("--no-warnings")
            .Add(url));
        var result = await cmd.ExecuteBufferedAsync(ct);
        using var doc = JsonDocument.Parse(result.StandardOutput);
        var root = doc.RootElement;

        if (root.TryGetProperty("formats", out var formatsEl) && formatsEl.ValueKind == JsonValueKind.Array)
        {
            var formats = new List<VideoFormat>();
            foreach (var f in formatsEl.EnumerateArray())
            {
                if (f.TryGetProperty("protocol", out var protocol) && protocol.GetString() == "mhtml")
                    continue;

                formats.Add(ParseFormat(f));
            }

            if (formats.Count > 0)
                return formats;
        }

        if (root.TryGetProperty("format_id", out var formatId) && formatId.GetString() is { } id)
            return [ParseFormat(root, id)];

        return [];
    }

    public async Task<string> GetOutputFileName(string url, string? format = null, CancellationToken ct = default)
    {
        var cmd = Cli.Wrap("yt-dlp").WithArguments(args =>
        {
            args.Add("--print").Add("filename")
                .Add("-o").Add(OutputTemplate)
                .Add("--no-download")
                .Add("--no-warnings");
            if (format is not null)
                args.Add("-f").Add(format);
            args.Add(url);
        });
        var result = await cmd.ExecuteBufferedAsync(ct);
        var fileName = result.StandardOutput.Trim();
        if (string.IsNullOrWhiteSpace(fileName))
            throw new InvalidOperationException("yt-dlp did not return an output filename");
        return Path.GetFileName(fileName);
    }

    public async Task StreamDownload(string url, Func<Stream, CancellationToken, Task> consume, string? format = null,
        CancellationToken ct = default)
    {
        var cmd = Cli.Wrap("yt-dlp").WithArguments(args =>
            {
                args.Add(url);
                if (format is not null)
                    args.Add("-f").Add(format);
                args.Add("-o").Add("-").Add("--no-part").Add("--no-warnings");
            })
            .WithStandardOutputPipe(PipeTarget.Create(consume))
            .WithStandardErrorPipe(PipeTarget.ToStream(Console.OpenStandardError()));
        await cmd.ExecuteAsync(ct);
    }

    public async Task<string> Download(string url, string? format = null, CancellationToken ct = default)
    {
        var tempDir = Directory.CreateTempSubdirectory().FullName;
        var cmd = Cli.Wrap("yt-dlp").WithArguments(args =>
            {
                args.Add(url);
                if (format is not null)
                    args.Add("-f").Add(format);
                args.Add("-o").Add(OutputTemplate);
            })
            .WithWorkingDirectory(tempDir);
        cmd |= (Console.OpenStandardOutput(), Console.OpenStandardError());
        await cmd.ExecuteAsync(ct);
        return Directory.GetFiles(tempDir).First();
    }

    private static VideoFormat ParseFormat(JsonElement f, string? fallbackId = null)
    {
        var id = f.TryGetProperty("format_id", out var formatId) ? formatId.GetString() ?? fallbackId ?? "" : fallbackId ?? "";
        var ext = f.TryGetProperty("ext", out var extEl) ? extEl.GetString() ?? "" : "";
        var vcodec = f.TryGetProperty("vcodec", out var vc) ? vc.GetString() ?? "none" : "none";
        var acodec = f.TryGetProperty("acodec", out var ac) ? ac.GetString() ?? "none" : "none";

        string resolution;
        if (f.TryGetProperty("resolution", out var res) && res.GetString() is { Length: > 0 } resStr)
            resolution = resStr;
        else if (f.TryGetProperty("height", out var height) && height.ValueKind == JsonValueKind.Number)
        {
            var w = f.TryGetProperty("width", out var width) && width.ValueKind == JsonValueKind.Number
                ? width.GetInt32().ToString()
                : "?";
            resolution = $"{w}x{height.GetInt32()}";
        }
        else if (vcodec is "none")
            resolution = "audio only";
        else
            resolution = "-";

        string size = "";
        if (f.TryGetProperty("filesize", out var fs) && fs.ValueKind == JsonValueKind.Number)
            size = $"{fs.GetInt64() / (1024.0 * 1024.0):N1} MiB";
        else if (f.TryGetProperty("filesize_approx", out var fsa) && fsa.ValueKind == JsonValueKind.Number)
            size = $"~{fsa.GetInt64() / (1024.0 * 1024.0):N1} MiB";

        var note = f.TryGetProperty("format_note", out var noteEl) ? noteEl.GetString() ?? "" : "";

        return new VideoFormat(id, ext, resolution, size, note, vcodec, acodec);
    }
}
