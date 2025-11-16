using VideoGallery.Interfaces;

namespace VideoGallery.Library;

public class Watch : IWatch
{
    private static DateOnly UnknownDate => new(2020, 1, 1);
    public Watch(Guid videoId, DateOnly? date, string? description = null)
    {
        VideoId = videoId;
        Date = date;
        Description = description;
    }

    private Watch(Guid videoId, DateOnly storeDate, bool isDateUnknown, string? description = null)
    {
        VideoId = videoId;
        StoreDate = storeDate;
        IsDateUnknown = isDateUnknown;
        Description = description;
    }
    public Guid VideoId { get; }

    public DateOnly? Date
    {
        get
        {
            if (IsDateUnknown) return null;
            return StoreDate;
        }
        set
        {
            StoreDate = value ?? UnknownDate;
            IsDateUnknown = !value.HasValue;
        }
    }

    internal DateOnly StoreDate { get; set; }
    internal bool IsDateUnknown { get; set; }
    public string? Description { get; set; }
}