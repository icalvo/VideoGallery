namespace VideoGallery.Interfaces;

public static class VideoExtensions
{
    extension(IVideo video)
    {
        public DateOnly? LastViewDate() => !video.Watches.Any() ? null : video.Watches.Max(w => w.Date);
    }
}