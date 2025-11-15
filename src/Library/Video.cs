using VideoGallery.Interfaces;

namespace VideoGallery.Library;

public class Video : IVideo
{
    private readonly HashSet<Tag> _tags = [];

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
    public string? Actors => Tags.FirstOrDefault(t => t.TagCategoryId == 'a')?.Name;
    public string? Composition => Tags.FirstOrDefault(t => t.TagCategoryId == 'c')?.Name;
    public string TagsRep => Tags.Select(t => t.Name).OrderBy(x => x).StrJoin(", ");
    public bool IsSolo => Composition?.Length == 3;
    public string? Comments { get; set; }
    public DateOnly? LastViewDate => Watches.Count == 0 ? null : Watches.Max(w => w.Date);
    public ICollection<Watch> Watches { get; set; } = new List<Watch>();
    public IEnumerable<ITag> ITags => _tags;
    public IEnumerable<Tag> Tags => _tags;
    public void Watch(DateOnly? now)
    {
        Watches.Add(new Watch(Id, now));
    }

    public void Unwatch()
    {
        Watches.Clear();
    }

    public TimeSpan AverageSequenceDuration()
    {
        return Duration / NumSequences;
    }

    public void RemoveWatch(DateOnly watchDate)
    {
        Watches.Where(w => w.Date == watchDate)
            .ToList()
            .ForEach(w => Watches.Remove(w));
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