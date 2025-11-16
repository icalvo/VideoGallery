using VideoGallery.Interfaces;

namespace VideoGallery.Library;

public class Video : IVideo
{
    private readonly HashSet<Tag> _tags = [];
    private readonly HashSet<Watch> _watches = [];

    public Video(
        Guid id,
        string filename,
        TimeSpan duration,
        int numSequences,
        string? comments)
    {
        Id = id;
        Filename = filename;
        Duration = duration;
        NumSequences = numSequences;
        Comments = comments;
    }

    public Guid Id { get; set; }
    public string Filename { get; private set; }
    public TimeSpan Duration { get; set; }
    public int NumSequences { get; set; }
    public string TagsRep => Tags.Select(t => t.Name).OrderBy(x => x).StrJoin(", ");
    public string? Comments { get; set; }
    public DateOnly? LastViewDate => !Watches.Any() ? null : Watches.Max(w => w.Date);
    public IEnumerable<Watch> Watches => _watches;
    IEnumerable<ITag> IVideo.Tags => _tags;
    IEnumerable<IWatch> IVideo.Watches => _watches;
    public IEnumerable<Tag> Tags => _tags;
    public void Watch(DateOnly? now)
    {
        _watches.Add(new Watch(Id, now));
    }

    public void Unwatch()
    {
        _watches.Clear();
    }

    public TimeSpan AverageSequenceDuration()
    {
        return Duration / NumSequences;
    }

    public void RemoveWatch(DateOnly watchDate)
    {
        _watches.Where(w => w.Date == watchDate)
            .ToList()
            .ForEach(w => _watches.Remove(w));
    }

    public void RemoveTags(params ITag[] tagsToRemove)
    {
        foreach (var tag in tagsToRemove)
        {
            _tags.Remove((Tag)tag);
        }
    }

    public void AddTags(params Tag[] tagsToAdd)
    {
        foreach (var tag in tagsToAdd) _tags.Add(tag);
    }
}