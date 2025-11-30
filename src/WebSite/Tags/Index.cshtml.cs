using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using VideoGallery.Library;

namespace VideoGallery.Website.Tags;

internal class TagsModel(Application application) : PageModel
{
    public TagDetail[] TagDetails { get; set; } = [];

    public async Task OnGet(CancellationToken ct)
    {
        TagDetails = await application.GetTags(ct);
    }

    public async Task<IActionResult> OnPostDeleteTag(Guid tagId, CancellationToken ct)
    {
        await application.DeleteTag(tagId, ct);
        return Page();
    }

    public async Task<IActionResult> OnPostDeleteCategory(char categoryId, CancellationToken ct)
    {
        await application.DeleteCategory(categoryId, ct);
        return Page();
    }

    public async Task<IActionResult> OnPostCreateCategory(
        char categoryId, string categoryName, string foregroundColor, string backgroundColor, CancellationToken ct)
    {
        await application.CreateCategory(categoryId, categoryName, foregroundColor, backgroundColor, ct);
        return Page();
    }

    public async Task<IActionResult> OnPostRenameTag(Guid tagId, string newTagText, CancellationToken ct)
    {
        await application.RenameTag(tagId, newTagText, ct);
        return Page();
    }
}
