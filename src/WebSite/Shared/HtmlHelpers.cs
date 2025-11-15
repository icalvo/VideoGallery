using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace VideoGallery.Website.Shared;

public static class HtmlHelpers
{
    private const string ScriptsKey = "__PageScripts";

    extension(HttpContext httpContext)
    {
        public void AddCachedPageScripts(string script)
        {
            var scripts = httpContext.GetCachedPageScripts();
            scripts.Add(script);
            httpContext.Items[ScriptsKey] = scripts;
        }
        private List<string> GetCachedPageScripts()
        {
            return (List<string>?)httpContext.Items[ScriptsKey] ?? [];
        }
    }

    extension(IHtmlHelper helper)
    {
        public HtmlString RegisterPageScripts()
        {
            return new HtmlString(string.Join(Environment.NewLine, GetCachedPageScripts(helper.ViewContext.HttpContext)));
        }
    }
}