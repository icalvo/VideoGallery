using VideoGallery.Interfaces;

namespace VideoGallery.Library;

public record VideoDto : IVideo
{
    public VideoDto(Video video, Func<char, TagCategory> catColors)
    {
        Id = video.Id;
        Filename = video.Filename;
        Duration = video.Duration;
        NumSequences = video.NumSequences;
        Comments = video.Comments;
        Watches = video.Watches.Select(w => new WatchDto(w)).ToArray();
        Tags = video.Tags.Select(t => new TagDto(t, catColors)).ToArray();
    }
    public VideoDto(Video video) : this(video, TagCategory.Default) { }

    public Guid Id { get; init; }
    public string Filename { get; }
    public TimeSpan Duration { get; set; }
    public int NumSequences { get; set; }
    public string? Comments { get; set; }
    IEnumerable<IWatch> IVideo.Watches => Watches;
    IEnumerable<ITag> IVideo.Tags => Tags;
    public IEnumerable<WatchDto> Watches { get; } = new List<WatchDto>();
    public IEnumerable<TagDto> Tags { get; }
}