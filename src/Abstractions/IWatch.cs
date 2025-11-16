namespace VideoGallery.Interfaces;

public interface IWatch
{
    public Guid VideoId { get; }
    public DateOnly? Date { get; }
    public string? Description { get; set; }
}