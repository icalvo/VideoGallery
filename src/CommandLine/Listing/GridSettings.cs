using VideoGallery.CommandLine.Utils.Parsing;
using VideoGallery.Library;
using static VideoGallery.CommandLine.Utils.Parsing.Parser;
namespace VideoGallery.CommandLine.Listing;

public record GridSettings(
    WatchedVideoFilter WatchedFilter,
    IEnumerable<SortField> SortFields,
    bool PrintIndexes,
    TimeSpan? MinDuration = null,
    TimeSpan? MaxDuration = null)
{
    public GridSettings WithPending => this with { WatchedFilter = WatchedVideoFilter.Pending }; 
    public GridSettings WithWatched => this with { WatchedFilter = WatchedVideoFilter.Watched }; 
    public GridSettings WithAll => this with { WatchedFilter = WatchedVideoFilter.All };
    
    public static readonly IParser<GridSettings> ParseMap =
        Repeat(
            separator: null,
            Either(
                Constant<GridSettings>("pending", "Include only pending to watch", s => s.WithPending),
                Constant<GridSettings>("watched", "Include only watched", s => s.WithWatched),
                Constant<GridSettings>("all", "Include pending and watched", s => s.WithAll),
                Concat(
                    "duration",
                    "Duration range",
                    Transform<GridSettings, ObjectRange<TimeSpan>>(
                        new RangeParser<TimeSpan>(new TimeSpanParser(["m':'ss"])),
                        new ObjectRange<TimeSpan>(TimeSpan.Zero, TimeSpan.Zero),
                        (s, r) => s with { MinDuration = r.Min, MaxDuration = r.Max })),
                Concat(
                    "sort",
                    "Sorting options",
                    Transform<GridSettings, SortField[]>(
                        Repeat(
                            null,
                            Transform<SortField[], SortField>(
                                Either(
                                    Constant("duration", "By duration", SortField.Duration),
                                    Constant("name", "By name", SortField.Name)),
                                default,
                                (arr, s) => [..arr, s])),
                        [],
                        (g, a) => g with { SortFields = a }))));

    public GridSettings Parse(string[] args)
    {
        if (args.Length == 0)
        {
            return this;
        }

        var result = ParseMap.Parse(new ParseStatus<GridSettings>(this, args));

        return
            result.ErrorMessage == null
                ? result.Value
                : throw new CommandArgumentException(result.ErrorMessage);
    }
    //
    // private static ParseStatus<GridSettings> ParseDuration(ParseStatus<GridSettings> former)
    // {
    //     if (former.Args.Length == 0)
    //     {
    //         return former with { Value = former.Value with { MinDuration = null, MaxDuration = null } };
    //     }
    //
    //     if (!former.Args[0].Contains(".."))
    //         return former with { ErrorMessage = "Must be a TimeSpan range: '5:00..', '..12:00', '4:00..11:00'" };
    //
    //     var endpoints = former.Args[0].Split("..");
    //     TimeSpan? min = null;
    //     TimeSpan? max = null;
    //     if (!string.IsNullOrEmpty(endpoints[0]))
    //     {
    //         min = TimeSpan.ParseExact(endpoints[0], ["m':'ss"], CultureInfo.InvariantCulture);
    //     }
    //     if (!string.IsNullOrEmpty(endpoints[1]))
    //     {
    //         max = TimeSpan.ParseExact(endpoints[1], ["m':'ss"], CultureInfo.InvariantCulture);
    //     }
    //         
    //     return former with
    //     {
    //         Value = former.Value with { MinDuration = min, MaxDuration = max },
    //         Args = former.Args[1..]
    //     };
    // }

    public override string ToString()
    {
        var minduration =
            MinDuration != null
                ? $"{MinDuration:m':'ss}<"
                : "";
        var maxduration =
            MaxDuration != null
                ? $"<{MaxDuration:m':'ss}"
                : "";

        var duration =
            minduration != "" || maxduration != ""
                ? $"[{minduration}dur{maxduration}]"
                : "";
        return $"[{WatchedFilter}]{duration}[sort:{SortFields.StrJoin(",")}]";
    }
}