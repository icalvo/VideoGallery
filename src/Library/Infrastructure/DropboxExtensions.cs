using Dropbox.Api;
using Dropbox.Api.Files;

namespace VideoGallery.Library.Infrastructure;

public static class DropboxExtensions
{
    extension(DropboxClient client)
    {
        public async Task<bool> ExistsAsync(string dropboxFilePath)
        {
            
            Metadata? metadata = null;
            try
            {
                metadata = await client.Files.GetMetadataAsync(dropboxFilePath);
            }
            catch (ApiException<GetMetadataError>)
            {
            }
            return metadata is not null;
        }
    }
}