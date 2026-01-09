using VideoGallery.Interfaces;
using VideoGallery.Library.Infrastructure;

namespace VideoGallery.CommandLine;

public static class VideoManagerFactory
{
    public static IVideoManager Create(Options options) => options.Storage switch
    {
        "dropbox" => new DropboxVideoManager(new ConsoleDropboxClientFactory(Options.DropboxAppKey),
            options.VideosFolder),
        "local" => new FileSystemVideoManager(options.VideosFolder),
        _ => throw new Exception("Unknown storage")
    };
}