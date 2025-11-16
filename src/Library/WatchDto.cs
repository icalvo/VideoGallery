using VideoGallery.Interfaces;

namespace VideoGallery.Library;

public record WatchDto : IWatch
{
    public WatchDto(Watch watch)
    {
        VideoId = watch.VideoId;
        Date = watch.Date;
        Description = watch.Description;
    }

    public Guid VideoId { get; }
    public DateOnly? Date { get; }
    public string? Description { get; set; }
}