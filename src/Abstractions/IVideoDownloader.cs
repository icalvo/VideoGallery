namespace VideoGallery.Interfaces;

public interface IVideoDownloader
{
    Task<string> Download(string url, CancellationToken ct = default);
}