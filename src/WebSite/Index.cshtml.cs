using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace VideoGallery.Website;

internal class HomeModel : PageModel
{
    public IActionResult OnGet()
    {
        return RedirectToPage("/VideoGrid/Index");
    }
}