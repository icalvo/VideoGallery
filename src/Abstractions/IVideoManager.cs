namespace VideoGallery.Interfaces;

public interface IVideoManager
{
    Task<TimeSpan> GetDuration(string video, CancellationToken ct = default);
    Task Upload(string filePath, CancellationToken ct = default);
    Task<bool> Exists(string video, CancellationToken ct = default);
    Task<string> GetVideoSharedLink(string video, CancellationToken ct = default);
    Task<Stream> GetThumbnail(string video, CancellationToken ct = default);
}