using VideoGallery.Interfaces;

namespace VideoGallery.Library;

public class Watch : IWatch
{
    public Watch(Guid videoId, DateOnly? date, string? description = null)
    {
        Id = Guid.Empty;
        VideoId = videoId;
        Date = date;
        Description = description;
    }
    internal Watch(Guid id, Guid videoId, DateOnly? date, string? description = null)
    {
        Id = id;
        VideoId = videoId;
        Date = date;
        Description = description;
    }

    public Guid Id { get; }
    public Guid VideoId { get; }
    public DateOnly? Date { get; set; }
    public string? Description { get; set; }
}