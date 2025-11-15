using Microsoft.EntityFrameworkCore;
using Spectre.Console;
using VideoGallery.Library;

namespace VideoGallery.CommandLine.Listing;

public static class ListVideoService
{
    public static async Task<Video[]> ShowVideos(VideoContext context, GridSettings settings)
    {
        var query = context.Videos.AsQueryable();

        query =
            settings.WatchedFilter switch
            {
                WatchedVideoFilter.Pending => query.Where(v => v.LastViewDate == null),
                WatchedVideoFilter.Watched => query.Where(v => v.LastViewDate != null),
                WatchedVideoFilter.All => query,
                _ => query.Where(v => v.LastViewDate == null)
            };

        if (settings.MinDuration != null)
        {
            query = query.Where(v => v.Duration >= settings.MinDuration.Value);
        }

        if (settings.MaxDuration != null)
        {
            query = query.Where(v => v.Duration <= settings.MaxDuration.Value);
        }

        query = settings.SortFields.Aggregate(
            (query, false),
            (x, sortField) => sortField switch
            {
                SortField.Duration => (x.query.OrderBySimple(v => v.Duration, x.Item2), true),
                SortField.Name => (x.query.OrderBySimple(v => v.Filename, x.Item2), true),
                _ => throw new Exception()
            }).query;

        var videos = await query.ToArrayAsync();
        var grid = new Grid();

        if (settings.PrintIndexes) grid.AddColumn();
        grid.AddColumn(new GridColumn { Width = 80 });
        grid.AddColumns(3);

        grid.AddRow([
            ..settings.PrintIndexes ? new[] {new Text("")} : [],
            new Text("File", new Style(Color.Red, Color.Black)).LeftJustified(),
            new Text("Duration", new Style(Color.Red, Color.Black)).LeftJustified(),
            new Text("Actors", new Style(Color.Red, Color.Black)).LeftJustified(),
            ]);
        var i = 0;
        foreach (var video in videos)
        {
            grid.AddRow([
                ..settings.PrintIndexes ? new[] {new Text(i.ToString())} : [],
                new Text(video.Filename) { Overflow = Overflow.Ellipsis },
                new Text(video.Duration.ToString("hh':'mm':'ss")),
                new Text(video.Actors ?? string.Empty),
                ]);
            i++;
        }

        AnsiConsole.Write(grid);

        return videos;
    }
}