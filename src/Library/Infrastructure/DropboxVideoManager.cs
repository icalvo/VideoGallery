using Dropbox.Api;
using Dropbox.Api.Files;
using Dropbox.Api.Sharing;
using VideoGallery.Interfaces;

namespace VideoGallery.Library.Infrastructure;

public class DropboxVideoManager : IVideoManager
{
    // Dropbox simple upload is limited to 150 MB; larger files need an upload session.
    private const long SimpleUploadLimitBytes = 150L * 1024 * 1024;
    private const int ChunkSizeBytes = 8 * 1024 * 1024;

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
        await using var stream = File.OpenRead(filePath);
        await Upload(stream, Path.GetFileName(filePath), ct);
    }

    public async Task Upload(Stream content, string fileName, CancellationToken ct = default)
    {
        var client = await ClientInstance(ct);
        var dropboxFilePath = _baseFolder + fileName;

        Console.WriteLine("Uploading to {0}", dropboxFilePath);
        if (await client.ExistsAsync(dropboxFilePath))
        {
            Console.WriteLine("File already uploaded");
            return;
        }

        FileMetadata updated;
        if (content.CanSeek && content.Length <= SimpleUploadLimitBytes)
        {
            updated = await client.Files.UploadAsync(
                dropboxFilePath,
                WriteMode.Overwrite.Instance,
                body: content);
        }
        else
        {
            if (content.CanSeek)
                Console.WriteLine("Large file ({0:N1} MB); using chunked upload", content.Length / (1024.0 * 1024.0));
            updated = await UploadChunked(client, dropboxFilePath, content, ct);
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
        var knownLength = stream.CanSeek ? (ulong?)stream.Length : null;
        var bytesRead = await ReadExactOrEofAsync(stream, buffer, ct);
        if (bytesRead == 0)
            throw new InvalidOperationException("Upload stream was empty");

        if (bytesRead < ChunkSizeBytes && knownLength is null)
        {
            await using var body = new MemoryStream(buffer, 0, bytesRead, writable: false);
            return await client.Files.UploadAsync(
                dropboxFilePath,
                WriteMode.Overwrite.Instance,
                body: body);
        }

        string sessionId;
        await using (var firstChunk = new MemoryStream(buffer, 0, bytesRead, writable: false))
        {
            var start = await client.Files.UploadSessionStartAsync(new UploadSessionStartArg(), firstChunk);
            sessionId = start.SessionId;
        }

        var offset = (ulong)bytesRead;
        var commit = new CommitInfo(dropboxFilePath, WriteMode.Overwrite.Instance);

        while (true)
        {
            ct.ThrowIfCancellationRequested();
            bytesRead = await ReadExactOrEofAsync(stream, buffer, ct);
            var cursor = new UploadSessionCursor(sessionId, offset);

            if (bytesRead == 0)
            {
                await using var empty = new MemoryStream();
                return await client.Files.UploadSessionFinishAsync(cursor, commit, contentHash: null!, empty);
            }

            await using var chunk = new MemoryStream(buffer, 0, bytesRead, writable: false);
            if (bytesRead < ChunkSizeBytes)
                return await client.Files.UploadSessionFinishAsync(cursor, commit, contentHash: null!, chunk);

            await client.Files.UploadSessionAppendV2Async(cursor, close: false, contentHash: null!, chunk);
            offset += (ulong)bytesRead;

            if (knownLength is { } fileSize)
            {
                Console.WriteLine("Uploaded {0:N1} / {1:N1} MB ({2:N0}%)",
                    offset / (1024.0 * 1024.0),
                    fileSize / (1024.0 * 1024.0),
                    offset * 100.0 / fileSize);
            }
        }
    }

    private static async Task<int> ReadExactOrEofAsync(Stream stream, byte[] buffer, CancellationToken ct)
    {
        var total = 0;
        while (total < buffer.Length)
        {
            var read = await stream.ReadAsync(buffer.AsMemory(total, buffer.Length - total), ct);
            if (read == 0)
                return total;
            total += read;
        }

        return total;
    }
}
