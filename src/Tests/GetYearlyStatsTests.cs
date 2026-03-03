using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Testcontainers.PostgreSql;
using VideoGallery.Interfaces;
using VideoGallery.Library;

namespace VideoGallery.Tests;

public class GetYearlyStatsTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:17-alpine").Build();
    private DbContextOptions<VideoContext> _options = null!;

    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();
        _options = new DbContextOptionsBuilder<VideoContext>()
            .UseNpgsql(_postgres.GetConnectionString())
            .Options;

        await using var ctx = new VideoContext(_options);
        await ctx.Database.EnsureCreatedAsync();
    }

    public async Task DisposeAsync()
    {
        await _postgres.DisposeAsync();
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

        var app = CreateApplication();
        var stats = await app.GetYearlyStats(new DateOnly(2024, 1, 1), CancellationToken.None);

        var global = Assert.Single(stats, s => s.Year == null);
        Assert.Equal(5, global.Count);
        Assert.Equal(new DateOnly(2024, 3, 15), global.MinDate);
        Assert.Equal(new DateOnly(2025, 3, 1), global.MaxDate);
        Assert.Equal(70.2, global.AvgSepInDays, 0.1);

        var year2024 = Assert.Single(stats, s => s.Year == 2024);
        Assert.Equal(3, year2024.Count);
        Assert.Equal(new DateOnly(2024, 3, 15), year2024.MinDate);
        Assert.Equal(new DateOnly(2024, 12, 31), year2024.MaxDate);
        Assert.Equal(97, year2024.AvgSepInDays, 0.1);

        var year2025 = Assert.Single(stats, s => s.Year == 2025);
        Assert.Equal(2, year2025.Count);
        Assert.Equal(new DateOnly(2025, 1, 1), year2025.MinDate);
        Assert.Equal(new DateOnly(2025, 3, 1), year2025.MaxDate);
        Assert.Equal(29.5, year2025.AvgSepInDays, 0.1);
    }

    [Fact]
    public async Task Filters_watches_before_start_date()
    {
        await using var ctx = new VideoContext(_options);

        var video = new Video(Guid.NewGuid(), "old.mp4", TimeSpan.FromMinutes(3), 1, null);
        ctx.Videos.Add(video);

        ctx.Add(new Watch(video.Id, new DateOnly(2023, 5, 1)));
        ctx.Add(new Watch(video.Id, new DateOnly(2024, 5, 1)));

        await ctx.SaveChangesAsync();

        var app = CreateApplication();
        var stats = await app.GetYearlyStats(new DateOnly(2024, 1, 1), CancellationToken.None);

        var global = Assert.Single(stats, s => s.Year == null);
        Assert.Equal(1, global.Count);
        Assert.Equal(new DateOnly(2024, 5, 1), global.MinDate);
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

        var app = CreateApplication();
        var stats = await app.GetYearlyStats(new DateOnly(2024, 1, 1), CancellationToken.None);

        var global = Assert.Single(stats, s => s.Year == null);
        Assert.Equal(2, global.Count);
        Assert.Equal(new DateOnly(2024, 6, 1), global.MinDate);
        Assert.Equal(new DateOnly(2024, 8, 1), global.MaxDate);
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
