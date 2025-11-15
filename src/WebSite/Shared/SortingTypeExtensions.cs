using VideoGallery.Library.Parsing;

namespace VideoGallery.Website.Shared;

public static class SortingTypeExtensions
{
    extension(SortingType t)
    {
        public SortingType Next() =>
            t switch
            {
                SortingType.None => SortingType.Ascending,
                SortingType.Ascending => SortingType.Descending,
                SortingType.Descending => SortingType.None,
                _ => throw new ArgumentOutOfRangeException(nameof(t), t, null)
            };
    }
}