using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace VideoGallery.Website.Shared;

/// <summary>
/// Extends the default script tag by adding support write scripts to @Html.RegisterPageScripts(), usually placed at the end of _Layout.cshtml.
/// </summary>
/// <example>
/// Example _ViewImports.cshtml:
///     @namespace DefaultNamespace.Pages
///     @addTagHelper *, Microsoft.AspNetCore.Mvc.TagHelpers
///     @removeTagHelper Microsoft.AspNetCore.Mvc.TagHelpers.TextAreaTagHelper, Microsoft.AspNetCore.Mvc.TagHelpers
///     @addTagHelper DefaultNamespace.TagHelpers.CustomTextAreaTagHelper, DefaultNamespace
///
/// Example Markup Usage:
///     &lt;script location="page">foo();&lt;/script>
///
/// Example _Layout.cshtml Registration:
///     @Html.RegisterPageScripts()
/// </example>
[HtmlTargetElement("script", Attributes = LocationAttributeName, TagStructure = TagStructure.NormalOrSelfClosing)]
public class CustomScriptTagHelper : TagHelper
{
    private const string LocationAttributeName = "location";
    private const string ScriptEndTag = "</script>";
    private const string ScriptLocationMovedMessage = $"<!-- Script tag moved to @Html.{nameof(HtmlHelpers.RegisterPageScripts)}(). -->";

    //public CustomScriptTagHelper(IHtmlGenerator generator) : base()
    //{
    //}

    public enum Locations
    {
        inline,
        page
    }

    [HtmlAttributeName(LocationAttributeName)]
    public Locations Location { get; set; }

    [HtmlAttributeNotBound]
    [ViewContext]
    public ViewContext ViewContext { get; set; } = null!;

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        if (Location == Locations.page)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            if (output == null) throw new ArgumentNullException(nameof(output));

            //get rendered script tag//
            string renderedTag;
            using (var writer = new StringWriter())
            {
                output.WriteTo(writer, NullHtmlEncoder.Default);
                renderedTag = writer.ToString();
                var index = renderedTag.LastIndexOf(ScriptEndTag, StringComparison.Ordinal);
                renderedTag = renderedTag.Remove(index, ScriptEndTag.Length);
            }

            //register content as script//
            var childContext = output.GetChildContentAsync().Result;
            var script = childContext.GetContent();
            ViewContext.HttpContext.AddCachedPageScripts($"{renderedTag}\n{script}\n{ScriptEndTag}");

            //replace content with comment//
            output.SuppressOutput();
            output.Content.AppendHtml(ScriptLocationMovedMessage);
        }
        base.Process(context, output);
    }
}