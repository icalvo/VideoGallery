namespace VideoGallery.Interfaces;

public sealed class TempFile(string filePath) : IDisposable
{
    public void Dispose() => File.Delete(filePath);
}