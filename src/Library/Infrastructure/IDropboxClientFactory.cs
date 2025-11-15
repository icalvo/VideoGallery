using Dropbox.Api;

namespace VideoGallery.Library.Infrastructure;

public interface IDropboxClientFactory
{
    Task<DropboxClient> Build(CancellationToken ct);
}