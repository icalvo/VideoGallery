using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using VideoGallery.Interfaces;
using VideoGallery.Library;

namespace VideoGallery.Website.Tags;

internal class TagsModel : PageModel
{
    private readonly IVideoManager _manager;
    private readonly Application _application;

    public TagsModel(
        IDbContextFactory<VideoContext> dbFactory,
        IVideoManager manager,
        Application application)
    {
        _manager = manager;
        _application = application;
    }

    
    public async Task OnGet(CancellationToken ct)
    {
        TagDetails = await _application.GetTags(ct);
    }

    public TagDetail[] TagDetails { get; set; } = [];
}
