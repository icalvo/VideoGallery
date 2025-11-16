namespace VideoGallery.Interfaces;

public interface IVideo
{
    Guid Id { get; }
    string Filename { get; }
    TimeSpan Duration { get; }
    int NumSequences { get; }
    string? Comments { get; }
    DateOnly? LastViewDate { get; }
    IEnumerable<IWatch> Watches { get; }
    IEnumerable<ITag> Tags { get; }
}