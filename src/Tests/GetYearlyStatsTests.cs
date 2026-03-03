using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Testcontainers.CockroachDb;
using VideoGallery.Interfaces;
using VideoGallery.Library;

namespace VideoGallery.Tests;

public class GetYearlyStatsTests : IAsyncLifetime
{
    private readonly CockroachDbContainer _cockroach = new CockroachDbBuilder("cockroachdb/cockroach:latest-v24.3").Build();
    private DbContextOptions<VideoContext> _options = null!;

    public async Task InitializeAsync()
    {
        await _cockroach.StartAsync();
        _options = new DbContextOptionsBuilder<VideoContext>()
            .UseNpgsql(_cockroach.GetConnectionString())
            .Options;

        await using var ctx = new VideoContext(_options);
        await ctx.Database.ExecuteSqlRawAsync(ctx.Database.GenerateCreateScript());
    }

    public async Task DisposeAsync()
    {
        await _cockroach.DisposeAsync();
    }

    private Application CreateApplication()
    {
        var factory = new TestDbContextFactory(_options);
        return new Application(
            NullLogger<Application>.Instance,
            factory,
            new NoOpTagValidation());
    }

    [Fact]
    public async Task Returns_global_and_yearly_stats()
    {
        await using var ctx = new VideoContext(_options);

        var video = new Video(Guid.NewGuid(), "test.mp4", TimeSpan.FromMinutes(5), 1, null);
        ctx.Videos.Add(video);

        ctx.Add(new Watch(video.Id, new DateOnly(2024, 3, 15)));
        ctx.Add(new Watch(video.Id, new DateOnly(2024, 6, 20)));
        ctx.Add(new Watch(video.Id, new DateOnly(2025, 1, 10)));

        ctx.NoVideoEvents.Add(new NoVideoEvent(new DateOnly(2024, 9, 1)));
        ctx.NoVideoEvents.Add(new NoVideoEvent(new DateOnly(2025, 3, 1)));

        await ctx.SaveChangesAsync();

        var asOf = new DateOnly(2026, 6, 15);
        var app = CreateApplication();
        var stats = await app.GetYearlyStats(CancellationToken.None, asOf);

        var global = Assert.Single(stats, s => s.Year == null);
        Assert.Equal(5, global.Count);
        Assert.Equal(new DateOnly(2024, 3, 15), global.MinDate);
        Assert.Equal(asOf, global.MaxDate);
        // (2026-06-15 - 2024-03-15) = 822 days. 822 / 5 = 164.4
        Assert.Equal(164.4, global.AvgFrequencyInDays, 0.1);

        var year2024 = Assert.Single(stats, s => s.Year == 2024);
        Assert.Equal(3, year2024.Count);
        Assert.Equal(new DateOnly(2024, 3, 15), year2024.MinDate);
        Assert.Equal(new DateOnly(2024, 12, 31), year2024.MaxDate);
        // (2024-12-31 - 2024-03-15) = 291 days. 291 / 3 = 97.0
        Assert.Equal(97, year2024.AvgFrequencyInDays, 0.1);

        var year2025 = Assert.Single(stats, s => s.Year == 2025);
        Assert.Equal(2, year2025.Count);
        Assert.Equal(new DateOnly(2025, 1, 1), year2025.MinDate);
        Assert.Equal(new DateOnly(2025, 12, 31), year2025.MaxDate);
        // (2025-12-31 - 2025-01-01) = 364 days. 364 / 2 = 182.0
        Assert.Equal(182, year2025.AvgFrequencyInDays, 0.1);
    }

    [Fact]
    public async Task Combines_watches_and_no_video_events()
    {
        await using var ctx = new VideoContext(_options);

        var video = new Video(Guid.NewGuid(), "combo.mp4", TimeSpan.FromMinutes(2), 1, null);
        ctx.Videos.Add(video);
        ctx.Add(new Watch(video.Id, new DateOnly(2024, 6, 1)));

        ctx.NoVideoEvents.Add(new NoVideoEvent(new DateOnly(2024, 8, 1)));

        await ctx.SaveChangesAsync();

        var asOf = new DateOnly(2026, 6, 15);
        var app = CreateApplication();
        var stats = await app.GetYearlyStats(CancellationToken.None, asOf);

        var global = Assert.Single(stats, s => s.Year == null);
        Assert.Equal(2, global.Count);
        Assert.Equal(new DateOnly(2024, 6, 1), global.MinDate);
        Assert.Equal(asOf, global.MaxDate);
    }

    [Fact]
    public async Task Current_year_uses_today_as_end_date()
    {
        await using var ctx = new VideoContext(_options);

        var asOf = new DateOnly(2025, 7, 20);

        var video = new Video(Guid.NewGuid(), "current.mp4", TimeSpan.FromMinutes(4), 1, null);
        ctx.Videos.Add(video);
        ctx.Add(new Watch(video.Id, new DateOnly(2025, 1, 15)));
        ctx.Add(new Watch(video.Id, new DateOnly(2025, 2, 10)));

        await ctx.SaveChangesAsync();

        var app = CreateApplication();
        var stats = await app.GetYearlyStats(CancellationToken.None, asOf);

        var yearStat = Assert.Single(stats, s => s.Year == 2025);
        Assert.Equal(2, yearStat.Count);
        Assert.Equal(new DateOnly(2025, 1, 15), yearStat.MinDate);
        Assert.Equal(asOf, yearStat.MaxDate);
        // (2025-07-20 - 2025-01-15) = 186 days. 186 / 2 = 93.0
        Assert.Equal(93, yearStat.AvgFrequencyInDays, 0.1);
    }

    [Fact]
    public async Task Single_event_year_returns_range_as_avg()
    {
        await using var ctx = new VideoContext(_options);

        var video = new Video(Guid.NewGuid(), "single.mp4", TimeSpan.FromMinutes(3), 1, null);
        ctx.Videos.Add(video);
        ctx.Add(new Watch(video.Id, new DateOnly(2024, 5, 10)));

        await ctx.SaveChangesAsync();

        var asOf = new DateOnly(2026, 6, 15);
        var app = CreateApplication();
        var stats = await app.GetYearlyStats(CancellationToken.None, asOf);

        var year2024 = Assert.Single(stats, s => s.Year == 2024);
        Assert.Equal(1, year2024.Count);
        Assert.Equal(new DateOnly(2024, 5, 10), year2024.MinDate);
        Assert.Equal(new DateOnly(2024, 12, 31), year2024.MaxDate);
        // (2024-12-31 - 2024-05-10) = 235 days. 235 / 1 = 235.0
        Assert.Equal(235, year2024.AvgFrequencyInDays, 0.1);
    }

    [Fact]
    public async Task Single_event_in_current_year_returns_range_to_today()
    {
        await using var ctx = new VideoContext(_options);

        var video = new Video(Guid.NewGuid(), "single-current.mp4", TimeSpan.FromMinutes(3), 1, null);
        ctx.Videos.Add(video);
        ctx.Add(new Watch(video.Id, new DateOnly(2025, 3, 1)));

        await ctx.SaveChangesAsync();

        var asOf = new DateOnly(2025, 7, 20);
        var app = CreateApplication();
        var stats = await app.GetYearlyStats(CancellationToken.None, asOf);

        var year2025 = Assert.Single(stats, s => s.Year == 2025);
        Assert.Equal(1, year2025.Count);
        Assert.Equal(new DateOnly(2025, 3, 1), year2025.MinDate);
        Assert.Equal(asOf, year2025.MaxDate);
        // (2025-07-20 - 2025-03-01) = 141 days. 141 / 1 = 141.0
        Assert.Equal(141, year2025.AvgFrequencyInDays, 0.1);
    }

    private class TestDbContextFactory(DbContextOptions<VideoContext> options)
        : IDbContextFactory<VideoContext>
    {
        public VideoContext CreateDbContext() => new(options);
    }

    private class NoOpTagValidation : ITagValidation
    {
        public string? ValidateTags(IEnumerable<ITag> tags) => null;

        public IEnumerable<(Func<IVideo, bool> cond, Func<IVideo, string[]> tags)> CalculatedTagRules
            => [];

        public string VideoEventTitle(IVideo video) => "";
        public string VideoEventTooltip(IVideo video) => "";
    }
}
