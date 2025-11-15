namespace VideoGallery.Library;

public class NoVideoEvent
{
    public NoVideoEvent(DateOnly date)
    {
        Date = date;
    }

    public DateOnly Date { get; private set; }
}