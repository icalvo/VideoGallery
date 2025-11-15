using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using VideoGallery.Library;

namespace VideoGallery.Website.Calendar;

public record CalendarEvent(string Title, string Tooltip, Guid? Id = null, string? Description = null);
public record CalendarDay(CalendarEvent[] Events, DateOnly Date);
public class CalendarModel : PageModel
{
    private readonly Application _application;

    public CalendarModel(Application application)
    {
        _application = application;
    }

    public CalendarDay[][]? Weeks { get; private set; }
    public YearlyStat[]? YearlyStats { get; private set; }

    public async Task OnGet(CancellationToken ct)
    {
        var startDate = new DateOnly(2024, 4, 29);
        YearlyStats = await _application.GetYearlyStats(startDate, ct);
        var vidsPerDate = (await _application.GetVideosPerDate(startDate, ct))
            .SelectMany(v => v.Watches.Select(w => new {w, v}))
            .WithoutNullsStr(x => x.w.Date, (x, d) => (x.w, x.v, d))
            .GroupBy(
                x => x.d, 
                x => new CalendarEvent(VideoEventTitle(x.v), VideoEventTooltip(x.v), x.v.Id, x.w.Description))
            .ToDictionary(x => x.Key, x => x.ToList());

        var noVidEvents = await _application.GetNoVideoEvents(ct);
        foreach (var noVideoEvent in noVidEvents)
        {
            if (!vidsPerDate.ContainsKey(noVideoEvent.Date))
                vidsPerDate[noVideoEvent.Date] = [];
            vidsPerDate[noVideoEvent.Date].Add(new CalendarEvent("NO VID", ""));
        }

        Weeks = Enumerate(startDate, DateOnly.FromDateTime(DateTime.Today))
            .Chunk(7)
            .Select(week => week
                .Select(d => new CalendarDay(vidsPerDate.GetValueOrDefault(d)?.ToArray() ?? [], d)).ToArray()).ToArray();
        return;

        static string VideoEventTitle(VideoDto video) =>
            $"{video.Actors} {video.Composition} {video.Duration.TotalMinutes:0}'";
        static string VideoEventTooltip(VideoDto video) => video.Filename + "\n" + video.TagsRep;
    }

    public async Task<IActionResult> OnPostUpdateWatchDescription(Guid videoId, DateOnly date, string? description, CancellationToken ct)
    {
        try
        {
            await _application.UpdateWatchDescription(videoId, date, description, ct);
            return new JsonResult(new { success = true });
        }
        catch (Exception ex)
        {
            return new JsonResult(new { success = false, error = ex.Message });
        }
    }

    private static IEnumerable<DateOnly> Enumerate(DateOnly d1, DateOnly d2)
    {
        while (d1 <= d2)
        {
            yield return d1;
            d1 = d1.AddDays(1);
        }
    }
}