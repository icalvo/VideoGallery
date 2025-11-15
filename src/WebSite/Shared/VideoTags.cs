using VideoGallery.Library;

namespace VideoGallery.Website.Shared;

public record VideoTags(Guid VideoId, TagDto[] Tags, string[] AllTags);
