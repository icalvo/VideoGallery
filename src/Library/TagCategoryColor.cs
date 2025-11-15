namespace VideoGallery.Library;

public record TagCategoryColor
{
    public TagCategoryColor(string cssName)
    {
        if (string.IsNullOrWhiteSpace(cssName)) throw new ArgumentNullException(nameof(cssName));
        CssName = cssName;
    }

    public string CssName { get; }
    public static implicit operator TagCategoryColor(string cssName) => new(cssName);
}