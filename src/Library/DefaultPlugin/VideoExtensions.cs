using VideoGallery.Interfaces;

namespace VideoGallery.Library.DefaultPlugin;

public static class VideoExtensions
{
    extension(IVideo video)
    {
        public string TagsRep => video.Tags.Select(t => t.Name).OrderBy(x => x).StrJoin(", ");
    }
}