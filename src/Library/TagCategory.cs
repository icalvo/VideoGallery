namespace VideoGallery.Library;

public class TagCategory
{
    public TagCategory(string name, char code, TagCategoryColor foregroundColor, TagCategoryColor backgroundColor)
    {
        Name = name;
        Code = code;
        ForegroundColor = foregroundColor;
        BackgroundColor = backgroundColor;
    }
    public string Name { get; private set; }
    public char Code { get; private set; }
    public TagCategoryColor ForegroundColor { get; private set; }
    public TagCategoryColor BackgroundColor { get; private set; }
    public ICollection<Tag> Tags { get; } = new List<Tag>();
    public static TagCategory Default(char code) => new("", code, "white", "black");
}