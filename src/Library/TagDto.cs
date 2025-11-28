using VideoGallery.Interfaces;

namespace VideoGallery.Library;

public record TagDto : ITag
{
    public TagDto(Tag t, TagCategory category)
    {
        Id = t.Id;
        Name = t.Name;
        Category = t.TagCategoryId;
        Text = t.Name;
        TagCategoryId = t.TagCategoryId;
        TagText = t.TagText;
        ForegroundColor = category.ForegroundColor;
        BackgroundColor = category.BackgroundColor;
    }
    public TagDto(Tag t, Func<char, TagCategory> catColors) : this(t, catColors(t.TagCategoryId))
    {
    }
    public Guid Id { get; init; }
    public string Name { get; init; }
    public char TagCategoryId { get; }
    public string TagText { get; }
    public char Category { get; init; }
    public string Text { get; init; }
    public TagCategoryColor ForegroundColor { get; init; }
    public TagCategoryColor BackgroundColor { get; init; }
}