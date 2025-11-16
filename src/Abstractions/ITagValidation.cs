namespace VideoGallery.Interfaces;

public interface ITagValidation
{
    string? ValidateTags(IEnumerable<ITag> tags);
    IEnumerable<(Func<IVideo, bool> cond, Func<IVideo, string[]> tags)> CalculatedTagRules { get; }
    
    string VideoEventTitle(IVideo video);
    string VideoEventTooltip(IVideo video);
}