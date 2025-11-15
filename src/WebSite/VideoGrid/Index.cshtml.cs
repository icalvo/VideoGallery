using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using VideoGallery.Library;
using VideoGallery.Library.Parsing;

namespace VideoGallery.Website.VideoGrid;

internal class VideoGridModel : PageModel
{
    private readonly Application _application;

    [BindProperty(SupportsGet = true)] public string? Search { get; set; }

    [BindProperty(SupportsGet = true)] public SortingType SortType { get; set; }

    [BindProperty(SupportsGet = true)] public string? SortField { get; set; } = nameof(Video.LastViewDate);

    public VideoDto[] Videos { get; private set; } = [];
    public int TotalVideosCount { get; private set; }

public string[] AllTags { get; private set; } = [];

    [BindProperty]
    public string? ReturnUrl { get; set; }
    
    [BindProperty(SupportsGet = true)]
    public string? ErrorMessage { get; set; }

    public IEnumerable<CustomQuery> CustomQueries { get; set; } = [];

    public VideoGridModel(Application application)
    {
        _application = application;
    }

    public async Task<IActionResult> OnGet(CancellationToken ct)
    {
        var standardQuery = await _application.GetStandardQuery(ct);
        var qs = Request.QueryString;
        if (!qs.HasValue)
        {
            return RedirectToPage(standardQuery);
        }
        
        var q = new QuerySpec (Search ?? "", SortType, SortField);

        CustomQueries = await _application.GetCustomQueries(ct);
        AllTags = await _application.GetAllTagNames(ct);
        Videos = await _application.GetVideos(q, ct);
        TotalVideosCount = await _application.GetVideosCount(ct);
        return Page();
    }

    public async Task<IActionResult> OnPostEditVideoNumSequences(Guid videoId, int numsequences, CancellationToken ct)
    {
        await _application.SetNumSequences(videoId, numsequences, ct);
        return RedirectToReturnUrl();
    }
    
    public async Task<IActionResult> OnPostWatchVideo(Guid videoId, DateOnly newDate, CancellationToken ct)
    {
        await _application.WatchVideo(videoId, newDate, ct);
        return RedirectToReturnUrl();
    }

    public async Task<IActionResult> OnPostUnwatchVideo(Guid videoId, CancellationToken ct)
    {
        await _application.UnwatchVideo(videoId, ct);
        return RedirectToReturnUrl();
    }

    public async Task<IActionResult> OnPostDeleteVideo(Guid videoId, CancellationToken ct)
    {
        await _application.DeleteVideo(videoId, ct);
        return RedirectToReturnUrl();
    }

    public async Task<IActionResult> OnPostDeleteVideoTag(Guid videoId, Guid tagId, CancellationToken ct)
    {
        return RedirectToReturnUrl(await _application.DeleteTagsFromVideo(videoId, tagId, ct));
    }

    public async Task<IActionResult> OnPostAddVideoTags(Guid videoId, string tags, CancellationToken ct)
    {
        var tagNames = tags.Split(',', StringSplitOptions.RemoveEmptyEntries|StringSplitOptions.TrimEntries);
        return RedirectToReturnUrl(await _application.AddTagsToVideo(videoId, tagNames, ct));
    }

    public async Task<IActionResult> OnPostSaveQuery(string queryName, CancellationToken ct)
    {
        var querySpec = new QuerySpec(Search ?? "", SortType, SortField);
        await _application.UpsertCustomQuery(queryName, querySpec, ct);
        return RedirectToReturnUrl();
    }
    
    public async Task<IActionResult> OnPostDeleteQuery(string queryName, CancellationToken ct)
    {
        var error = await _application.DeleteCustomQuery(queryName, ct);
        return RedirectToReturnUrl(error);
    }
    
    private RedirectResult RedirectToReturnUrl(string? error = null) => 
        Redirect(
            string.IsNullOrEmpty(ReturnUrl)
            ? ""
            : ReplaceError(ReturnUrl, error));

    public static string ReplaceError(string url, string? error)
    {
        return url.ModifyQueryString(newQueryString =>
            {
                newQueryString.Remove(nameof(ErrorMessage));
                if (error is not null)
                    newQueryString.Add(nameof(ErrorMessage), error);
            });
    }
}