namespace VideoGallery.Interfaces;

public interface IVideoDownloader
{
    Task<IReadOnlyList<VideoFormat>> ListFormats(string url, CancellationToken ct = default);
    Task<string> GetOutputFileName(string url, string? format = null, CancellationToken ct = default);
    Task StreamDownload(string url, Func<Stream, CancellationToken, Task> consume, string? format = null, CancellationToken ct = default);
    Task<string> Download(string url, string? format = null, CancellationToken ct = default);
}
