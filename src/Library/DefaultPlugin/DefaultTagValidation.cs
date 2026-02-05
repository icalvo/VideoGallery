using VideoGallery.Interfaces;

namespace VideoGallery.Library.DefaultPlugin;

public class DefaultTagValidation : ITagValidation
{
    public string? ValidateTags(IEnumerable<ITag> tags) => null;
    public IEnumerable<TagRule> CalculatedTagRules { get; } = [];
    public IEnumerable<TagRule> WatchTagRules { get; } = [];

    public string VideoEventTitle(IVideo video) =>
        $"{video.Duration.TotalMinutes:0}'";
    public string VideoEventTooltip(IVideo video) => video.Filename + "\n" + video.TagsRep;
}