using VideoGallery.Interfaces;

namespace VideoGallery.Library;

public class DefaultTagValidation : ITagValidation
{
    public string? ValidateTags(IEnumerable<ITag> tags)
    {
        return null;
    }

    public IEnumerable<(Func<IVideo, bool> cond, Func<IVideo, string[]> tags)> CalculatedTagRules { get; } = [];
}