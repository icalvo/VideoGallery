using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using VideoGallery.Interfaces;
using VideoGallery.Library;

namespace VideoGallery.Website.VideoDetail;

internal class VideoDetailModel : PageModel
{
    private readonly IVideoManager _manager;
    private readonly Application _application;

    public VideoDetailModel(
        IDbContextFactory<VideoContext> dbFactory,
        IVideoManager manager,
        Application application)
    {
        _manager = manager;
        _application = application;
    }

    public Guid Id { get; set; }
    [BindProperty]
    public string? Filename { get; private set; }
    [BindProperty]
    public string? Duration { get; private set; }
    [BindProperty]
    public int NumSequences { get; set; }
    [BindProperty]
    public string? Comments { get; set; }
    [BindProperty]
    public DateOnly? LastViewDate { get; private set; }
    public ICollection<DateOnly?> Watches { get; set; } = new List<DateOnly?>();
    public TagDto[] Tags { get; set; } = [];

    [BindProperty]
    public string? PreviewLink { get; private set; }
    public string[] Categories { get; private set; } = [];
    public string[] AllTags { get; private set; } = [];
    public async Task OnGet(Guid id, CancellationToken ct)
    {
        var video = await _application.GetVideoById(id, ct) ?? throw new Exception("Video not found");
        await PopulateVideoDetails(video, ct);
    }

    private async Task PopulateVideoDetails(VideoDto video, CancellationToken ct)
    {
        PreviewLink = await _manager.GetVideoSharedLink(video.Filename, ct);
        Id = video.Id;
        Filename = video.Filename;
        Duration = video.Duration < TimeSpan.FromHours(1) ? video.Duration.ToString(@"m\:ss") : video.Duration.ToString(@"h\:mm\:ss");
        NumSequences = video.NumSequences;
        Comments = video.Comments;
        LastViewDate = video.LastViewDate();
        Watches = video.Watches.Select(w => w.Date).ToArray();
        Tags = video.Tags.ToArray();
        AllTags = await _application.GetAllTagNames(ct);
    }

    public async Task<IActionResult> OnPostEditVideo(Guid id, CancellationToken ct)
    {
        await _application.UpdateVideo(
            id, 
            Duration == null ? null : TimeSpan.ParseExact(Duration, [@"m\:ss", @"h\:mm\:ss"], null), 
            NumSequences, 
            Comments, ct);
        return RedirectToPage("Index", new { Id = id });
    }

    public async Task<IActionResult> OnPostWatchVideo(Guid videoId, DateOnly watchDate, CancellationToken ct)
    {
        await _application.WatchVideo(videoId, watchDate, ct);
        return RedirectToPage("Index", new { Id = videoId });
    }

    public async Task<IActionResult> OnPostDeleteWatch(Guid videoId, DateOnly watchDate, CancellationToken ct)
    {
        await _application.DeleteWatch(videoId, watchDate, ct);
        return RedirectToPage("Index", new { Id = videoId });
    }

    public async Task<IActionResult> OnPostDeleteVideo(Guid videoId, CancellationToken ct)
    {
        await _application.DeleteVideo(videoId, ct);
        return RedirectToPage("/");
    }

    public async Task<IActionResult> OnPostDeleteVideoTag(Guid videoId, Guid tagId, CancellationToken ct)
    {
        await _application.DeleteTagsFromVideo(videoId, tagId, ct);
        return RedirectToPage("Index", new { Id = videoId });
    }

    public async Task<IActionResult> OnPostAddVideoTags(Guid videoId, string tags, CancellationToken ct)
    {
        await _application.AddTagsToVideo(videoId, tags.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries), ct);
        return RedirectToPage("Index", new { Id = videoId });
    }
    private async Task<string> GetVideoSharedLink(string video, CancellationToken ct)
    {
        var link = await _manager.GetVideoSharedLink(video, ct);
        return link.Replace("dl=0", "raw=1");
    }
}
