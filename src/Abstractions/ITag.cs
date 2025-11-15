namespace VideoGallery.Interfaces;

public interface ITag
{
    string Name { get; }
    char TagCategoryId { get; }
    string TagText { get; }
}