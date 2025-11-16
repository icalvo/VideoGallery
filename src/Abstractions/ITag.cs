namespace VideoGallery.Interfaces;

/// <summary>
/// Video Tag
/// </summary>
/// <remarks>
/// Video tags are used to categorize videos. They are formed by a Category and a Name. The category is
/// a single alphabetic character, and the name is a string formed by alphanumeric characters and dashes.
/// The full tag text is formed by the category and the name, separated by a colon.
/// </remarks>
public interface ITag
{
    string Name { get; }
    char TagCategoryId { get; }
    string TagText { get; }
}