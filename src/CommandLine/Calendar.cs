using Microsoft.EntityFrameworkCore;
using Spectre.Console;
using Spectre.Console.Rendering;
using VideoGallery.CommandLine.Utils;
using VideoGallery.Library;

namespace VideoGallery.CommandLine;

public class Calendar : ICommand
{
    private readonly VideoContext _context;

    public Calendar(VideoContext context)
    {
        _context = context;
    }

    public IRenderable Description()
    {
        return new Text("Prints a calendar with recent events");
    }

    public async Task Run(string[] args)
    {
        var startDate = new DateOnly(2024, 4, 29);
        var vidsPerDate = (await _context.Videos.Where(v => v.Watches.Any(w => w.Date >= startDate)).ToArrayAsync())
            .SelectMany(v => v.Watches.WithoutNullsStr(w => w.Date, (_, d) => new {Date = d, Video = v}))
            .GroupBy(
                x => x.Date, 
                x => "VID")
            .ToDictionary(x => x.Key, x => x.ToList());
        
        var noVidEvents = _context.NoVideoEvents;
        foreach (var noVideoEvent in noVidEvents)
        {
            vidsPerDate[noVideoEvent.Date] = [""];
        }

        string[] x = ["M", "T", "W", "T", "F", "S", "S"];
        const int padding = 4;
        foreach (var dayName in x)
        {
            
            Console.Write($"{dayName, padding}");
        }
        Console.WriteLine();

        var weeks = Enumerate(startDate, DateOnly.FromDateTime(DateTime.Today))
            .Chunk(7)
            .Select(week => week.Select(d => (vidsPerDate.GetValueOrDefault(d), d)).ToArray());

        foreach (var week in weeks)
        {
            foreach (var (events, day) in week)
            {
                string text;
                ConsoleColor bg;
                if (events != null)
                {
                    text = events.Count.ToString();
                    bg = events.Count switch
                    {
                        1 => ConsoleColor.Yellow,
                        2 => ConsoleColor.Cyan,
                        _ => ConsoleColor.Red
                    };
                }
                else
                {
                    text = day.Day.ToString();
                    bg =
                        day.Month % 2 == 0
                            ? ConsoleColor.Blue
                            : ConsoleColor.Green;
                }

                using var _ = ConsoleExtensions.Color(ConsoleColor.Black, bg);
                Console.Write($"{text, padding}");
            }

            if (week.First().d.Day == 1 || week.First().d.Month != week.Last().d.Month)
            {
                Console.WriteLine(week.Last().d.ToString(@" MMM \'yy"));
            }
            else
                Console.WriteLine();
        }
    }

    public IRenderable Syntax() => Text.Empty;

    private static IEnumerable<DateOnly> Enumerate(DateOnly d1, DateOnly d2)
    {
        while (d1 <= d2)
        {
            yield return d1;
            d1 = d1.AddDays(1);
        }        
    }
}