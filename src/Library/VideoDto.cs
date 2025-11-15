namespace VideoGallery.Library;

public record VideoDto
{
    public VideoDto(Video video, Func<char, TagCategory> catColors)
    {
        Id = video.Id;
        Filename = video.Filename;
        Duration = video.Duration;
        NumSequences = video.NumSequences;
        Comments = video.Comments;
        Watches = video.Watches;
        LastViewDate = video.LastViewDate;
        Tags = video.Tags.Select(t => new TagDto(t, catColors)).ToArray();
    }
    public VideoDto(Video video) : this(video, TagCategory.Default) { }

    public Guid Id { get; init; }
    public string Filename { get; init; }
    public TimeSpan Duration { get; set; }
    public int NumSequences { get; set; }
    public string? Actors => Tags.FirstOrDefault(t => t.Category == 'a')?.Name;
    public string? Composition => Tags.FirstOrDefault(t => t.Category == 'c')?.Name;
    public string TagsRep => Tags.Select(t => t.Name).OrderBy(x => x).StrJoin(", ");
    public bool IsSolo => Composition?.Length == 3;
    public string? Comments { get; set; }
    public DateOnly? LastViewDate { get; init; }
    public ICollection<Watch> Watches { get; set; } = new List<Watch>();
    public IEnumerable<TagDto> Tags { get; }
}