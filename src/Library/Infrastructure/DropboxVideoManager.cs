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

    // Dropbox simple upload is limited to 150 MB; larger files need an upload session.
    private const long SimpleUploadLimitBytes = 150L * 1024 * 1024;
    private const int ChunkSizeBytes = 8 * 1024 * 1024;

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

        await using var stream = File.OpenRead(filePath);
        FileMetadata updated;
        if (stream.Length <= SimpleUploadLimitBytes)
        {
            updated = await client.Files.UploadAsync(
                dropboxFilePath,
                WriteMode.Overwrite.Instance,
                body: stream);
        }
        else
        {
            Console.WriteLine("Large file ({0:N1} MB); using chunked upload", stream.Length / (1024.0 * 1024.0));
            updated = await UploadChunked(client, dropboxFilePath, stream, ct);
        }

        Console.WriteLine("Saved {0}{1} rev {2}", _baseFolder, fileName, updated.Rev);
    }

    private static async Task<FileMetadata> UploadChunked(
        DropboxClient client,
        string dropboxFilePath,
        Stream stream,
        CancellationToken ct)
    {
        var buffer = new byte[ChunkSizeBytes];
        var fileSize = (ulong)stream.Length;
        var bytesRead = await stream.ReadAsync(buffer.AsMemory(0, ChunkSizeBytes), ct);
        string sessionId;
        await using (var firstChunk = new MemoryStream(buffer, 0, bytesRead))
        {
            var start = await client.Files.UploadSessionStartAsync(body: firstChunk);
            sessionId = start.SessionId;
        }

        var offset = (ulong)bytesRead;
        while (offset < fileSize)
        {
            ct.ThrowIfCancellationRequested();
            bytesRead = await stream.ReadAsync(buffer.AsMemory(0, ChunkSizeBytes), ct);
            await using var chunk = new MemoryStream(buffer, 0, bytesRead);
            var cursor = new UploadSessionCursor(sessionId, offset);
            var nextOffset = offset + (ulong)bytesRead;

            if (nextOffset >= fileSize)
            {
                var commit = new CommitInfo(dropboxFilePath, WriteMode.Overwrite.Instance);
                return await client.Files.UploadSessionFinishAsync(cursor, commit, body: chunk);
            }

            await client.Files.UploadSessionAppendV2Async(cursor, body: chunk);
            offset = nextOffset;
            var percent = offset * 100.0 / fileSize;
            Console.WriteLine("Uploaded {0:N1} / {1:N1} MB ({2:N0}%)",
                offset / (1024.0 * 1024.0),
                fileSize / (1024.0 * 1024.0),
                percent);
        }

        throw new InvalidOperationException("Upload finished without a final chunk.");
    }
}