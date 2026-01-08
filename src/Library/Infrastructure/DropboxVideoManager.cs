using Dropbox.Api;
using Dropbox.Api.Files;
using Dropbox.Api.Sharing;
using VideoGallery.Interfaces;

namespace VideoGallery.Library.Infrastructure;

public class DropboxVideoManager : IVideoManager
{
    private readonly IDropboxClientFactory _clientFactory;
    private readonly string _baseFolder;
    private DropboxClient? _client;
    public DropboxVideoManager(IDropboxClientFactory clientFactory, string baseFolder)
    {
        _clientFactory = clientFactory;
        _baseFolder = baseFolder + (baseFolder.EndsWith('/') ? "" : "/");
    }

    private async Task<DropboxClient> ClientInstance(CancellationToken ct)
    {
        _client ??= await _clientFactory.Build(ct);
        return _client;
    }
    public async Task<TimeSpan> GetDuration(string video, CancellationToken ct = default)
    {
        var client = await ClientInstance(ct);
        var dropboxFilePath = _baseFolder + video;
        var a = await client.Files.GetMetadataAsync(dropboxFilePath, includeMediaInfo: true);
        var duration = a.AsFile.MediaInfo.AsMetadata.Value.AsVideo.Duration;
        if (duration == null) throw new Exception("No duration");
        return TimeSpan.FromMilliseconds(duration.Value);
    }

    public async Task<bool> Exists(string video, CancellationToken ct = default)
    {
        var client = await ClientInstance(ct);
        var dropboxFilePath = _baseFolder + video;
        try
        {
            await client.Files.GetMetadataAsync(dropboxFilePath);
            return true;
        }
        catch (ApiException<GetMetadataError> ex)
        {
            if (ex.Message == "path/not_found/...") return false;
            throw;
        }
    }

    public async Task<string> GetVideoSharedLink(string video, CancellationToken ct = default)
    {
        var client = await ClientInstance(ct);
        var filePath = _baseFolder + video;
        var link = (await client.Sharing.ListSharedLinksAsync(filePath)).Links.FirstOrDefault(x => x.LinkPermissions.AudienceOptions.Any(o => o.Audience == LinkAudience.NoOne.Instance));
        link ??= await client.Sharing.CreateSharedLinkWithSettingsAsync(filePath,
            new SharedLinkSettings(audience: LinkAudience.NoOne.Instance));
        return link.Url;
    }

    public async Task<Stream> GetThumbnail(string video, CancellationToken ct = default)
    {
        var client = await ClientInstance(ct);
        var filePath = _baseFolder + video;
        var thumbnailArg = new ThumbnailArg(filePath, new ThumbnailFormat().AsPng);
        var downloadResponse = await client.Files.GetThumbnailAsync(thumbnailArg);
        return await downloadResponse.GetContentAsStreamAsync();
    }

    public async Task Upload(string filePath, CancellationToken ct = default)
    {
        var client = await ClientInstance(ct);
        var fileName = Path.GetFileName(filePath);
        var dropboxFilePath = _baseFolder + fileName;
        
        Console.WriteLine("Uploading to {0}", dropboxFilePath);
        if (await client.ExistsAsync(dropboxFilePath))
        {
            Console.WriteLine("File already uploaded");
            return;
        }
        var stream = File.OpenRead(filePath);

        var updated = await client.Files.UploadAsync(
            dropboxFilePath,
            WriteMode.Overwrite.Instance,
            body: stream);
        Console.WriteLine("Saved {0}{1} rev {2}", _baseFolder, fileName, updated.Rev);
    }
}