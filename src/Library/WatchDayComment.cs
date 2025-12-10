namespace VideoGallery.Library;

public class WatchDayComment
{
    public WatchDayComment(DateOnly date, string comment)
    {
        Date = date;
        Comment = comment;
    }
    public string Comment { get; set; }
    public DateOnly Date { get; private set; }
}