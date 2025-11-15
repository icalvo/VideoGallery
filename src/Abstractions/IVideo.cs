namespace VideoGallery.Interfaces;

public interface IVideo
{
    Guid Id { get; set; }
    string Filename { get; }
    TimeSpan Duration { get; set; }
    int NumSequences { get; set; }
    string? Actors { get; }
    string? Composition { get; }
    string TagsRep { get; }
    bool IsSolo { get; }
    string? Comments { get; set; }
    DateOnly? LastViewDate { get; }
    // ICollection<Watch> Watches { get; set; }
    IEnumerable<ITag> ITags { get; }
    TimeSpan AverageSequenceDuration();
}