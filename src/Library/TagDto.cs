namespace VideoGallery.Library;

public record TagDto
{
    public TagDto(Tag t, Func<char, TagCategory> catColors)
    {
        Id = t.Id;
        Name = t.Name;
        Category = t.TagCategoryId;
        Text = t.Name;
        ForegroundColor = catColors(t.TagCategoryId).ForegroundColor;
        BackgroundColor = catColors(t.TagCategoryId).BackgroundColor;
    }
    public Guid Id { get; init; }
    public string Name { get; init; }
    public char Category { get; init; }
    public string Text { get; init; }
    public TagCategoryColor ForegroundColor { get; init; }
    public TagCategoryColor BackgroundColor { get; init; }
}