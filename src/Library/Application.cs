using System.Collections.Frozen;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using VideoGallery.Interfaces;
using VideoGallery.Library.Parsing;

namespace VideoGallery.Library;

public class Application : ITagValidation
{
    private readonly ILogger<Application> _logger;
    private readonly IDbContextFactory<VideoContext> _dbFactory;
    private readonly ITagValidation _tagValidation;

    public Application(ILogger<Application> logger, IDbContextFactory<VideoContext> dbFactory, ITagValidation tagValidation)
    {
        _logger = logger;
        _dbFactory = dbFactory;
        _tagValidation = tagValidation;
    }

    public static async Task<IDictionary<char, TagCategory>> CategoriesCodeDictionary(VideoContext ctx, CancellationToken ct) =>
        (await ctx.Categories.ToArrayAsync(ct)).ToFrozenDictionary(c => c.Code);
    public async Task<VideoDto[]> GetVideos(QuerySpec q, CancellationToken ct)
    {
        await using var context = await _dbFactory.CreateDbContextAsync(ct);
        var catColors = await CategoriesCodeDictionary(context, ct);
        var videos = await QueryBuilder.BuildQuery(
            context.Videos.Include(v => v.Tags).Include(v => v.Watches), 
            context.CustomQueryExpressions, 
            VideoContext.AddSorting, 
            q).ToArrayAsync(ct);
        return videos
            .Select(v => new VideoDto(v, c => catColors[c]))
            .ToArray();
    }
    public async Task<string[]> GetAllTagNames(CancellationToken ct)
    {
        await using var context = await _dbFactory.CreateDbContextAsync(ct);
        return await context.Tags.Select(t => t.Name).ToArrayAsync(ct);
    }

    public async Task SetNumSequences(Guid videoId, int numsequences, CancellationToken ct)
    {
        _logger.LogInformation(" [{Video}]", videoId);
        await using var context = await _dbFactory.CreateDbContextAsync(ct);
        var chosenVideo =
            await context.Videos.FindAsync([videoId], ct)
            ?? throw new Exception("Video not found");
        chosenVideo.NumSequences = numsequences;
        await RecalculateCalculatedTags(chosenVideo, context, ct);
        await context.SaveChangesAsync(ct);
    }

    public async Task WatchVideo(Guid videoId, DateOnly? newDate, CancellationToken ct)
    {
        _logger.LogInformation("Watching [{Video}] at {Date}", videoId, newDate);
        await using var context = await _dbFactory.CreateDbContextAsync(ct);
        var chosenVideo =
            await context.Videos.FindAsync([videoId], ct)
            ?? throw new Exception("Video not found");

        var watchedDate = newDate;

        chosenVideo.Watch(watchedDate);
        await RecalculateCalculatedTags(chosenVideo, context, ct);
        await context.SaveChangesAsync(ct);
    }

    public async Task DeleteWatch(Guid videoId, DateOnly watchDate, CancellationToken ct)
    {
        _logger.LogInformation("Deleting watch of [{Video}] at {Date}", videoId, watchDate);
        await using var context = await _dbFactory.CreateDbContextAsync(ct);
        var video =
            await context.Videos.FindAsync([videoId], ct)
            ?? throw new Exception("Video not found");

        video.RemoveWatch(watchDate);
        await RecalculateCalculatedTags(video, context, ct);
        await context.SaveChangesAsync(ct);
    }

    public async Task UnwatchVideo(Guid videoId, CancellationToken ct)
    {
        _logger.LogInformation("Unwatching [{Video}]", videoId);
        await using var context = await _dbFactory.CreateDbContextAsync(ct);
        var chosenVideo =
            await context.Videos.FindAsync([videoId], ct)
            ?? throw new Exception("Video not found");

        if (chosenVideo.LastViewDate() == null)
            return;

        chosenVideo.Unwatch();
        await RecalculateCalculatedTags(chosenVideo, context, ct);
        await context.SaveChangesAsync(ct);
    }

    public async Task DeleteVideo(Guid videoId, CancellationToken ct)
    {
        _logger.LogInformation("Deleting [{Video}]", videoId);
        await using var context = await _dbFactory.CreateDbContextAsync(ct);
        var video = await context.Videos.FindAsync([videoId], ct);
        if (video != null)
            context.Videos.Remove(video);
        await context.SaveChangesAsync(ct);
    }

    public async Task<string?> AddVideo(Video newVideo, string[] tagNames, CancellationToken ct)
    { 
        await using var context = await _dbFactory.CreateDbContextAsync(ct);
        context.Videos.Add(newVideo);
        var (tags, error) = await AddTags(tagNames, context, ct);
        if (error is not null) return error;
        newVideo.AddTags(tags);
        error = await RecalculateCalculatedTags(newVideo, context, ct);
        await context.SaveChangesAsync(ct);
        return error;
    }

    public async Task<string?> AddTagsToVideo(Guid videoId, string[] tagNames, CancellationToken ct)
    {
        _logger.LogInformation("Adding tags [{Tags}] from video [{Video}]", tagNames, videoId);
        await using var context = await _dbFactory.CreateDbContextAsync(ct);
        var video = await context.Videos.FindAsync([videoId], ct) ?? throw new Exception("Video not found");
        var (tags, error) = await AddTags(tagNames, context, ct);
        if (error is not null) return error;
        video.AddTags(tags);
        var error2 = await RecalculateCalculatedTags(video, context, ct);
        await context.SaveChangesAsync(ct);
        return error2;
    }

    public async Task<string?> DeleteTagsFromVideo(Guid videoId, Guid tagId, CancellationToken ct)
    {
        _logger.LogInformation("Deleting tag [{Tag}] from video [{Video}]", tagId, videoId);
        await using var context = await _dbFactory.CreateDbContextAsync(ct);
        var video = await context.Videos.FindAsync([videoId], ct) ?? throw new Exception("Video not found");
        video.RemoveTags(video.Tags.Single(x => x.Id == tagId));
        var error = await RecalculateCalculatedTags(video, context, ct);
        await context.SaveChangesAsync(ct);
        return error;
    }

    private static async Task<(Tag[] tags, string? error)> AddTags(
        string[] tagNames,
        VideoContext context, CancellationToken ct)
    {
        var existingTags = await context.Tags.Where(t => tagNames.Contains(t.Name)).ToArrayAsync(ct);
        var tagNamesToCreate = tagNames.Except(existingTags.Select(t => t.Name)).ToArray();
        var createdTags = new List<Tag>();
        foreach (var tagName in tagNamesToCreate)
        {
            var tag = new Tag(tagName);
            createdTags.Add(tag);
            await context.Tags.AddAsync(tag, ct);
        }

        return (createdTags.Concat(existingTags).ToArray(), null);
    }

    public async Task UpdateVideo(Guid id, TimeSpan? duration, int numSequences, string? comments, CancellationToken ct)
    {
        _logger.LogInformation("Updating [{Video}]", id);
        await using var context = await _dbFactory.CreateDbContextAsync(ct);
        var chosenVideo =
            await context.Videos.FindAsync([id], ct)
            ?? throw new Exception("Video not found");
        if (duration != null)
            chosenVideo.Duration = duration.Value;
        chosenVideo.NumSequences = numSequences;
        chosenVideo.Comments = comments;
        await RecalculateCalculatedTags(chosenVideo, context, ct);
        await context.SaveChangesAsync(ct);
    }

    public async Task UpdateWatchDescription(Guid videoId, DateOnly date, string? description, CancellationToken ct)
    {
        _logger.LogInformation("Updating watch description for video [{Video}] on {Date}", videoId, date);
        await using var context = await _dbFactory.CreateDbContextAsync(ct);
        var video = await context.Videos.FindAsync([videoId], ct) ?? throw new Exception("Video not found");
        var watch = await context.Watches.FirstAsync(w => w.VideoId == videoId && w.Date == date, ct);
        watch.Description = description;
        await RecalculateCalculatedTags(video, context, ct);
        await context.SaveChangesAsync(ct);
    }
    public async Task SetWatchDayComment(DateOnly date, string? comment, CancellationToken ct)
    {
        _logger.LogInformation("Updating watch day comment for {Date}", date);
        await using var context = await _dbFactory.CreateDbContextAsync(ct);
        var watchDayComment = await context.WatchDayComments.FindAsync([date], ct);
        if (watchDayComment == null)
        {
            if (string.IsNullOrWhiteSpace(comment)) return;
            watchDayComment = new WatchDayComment(date, comment);
            context.WatchDayComments.Add(watchDayComment);
        }
        else
        {
            if (string.IsNullOrWhiteSpace(comment)) 
                context.WatchDayComments.Remove(watchDayComment);
            else
                watchDayComment.Comment = comment;
        } 

        await context.SaveChangesAsync(ct);
    }

    public async Task<VideoDto> GetVideoById(Guid id, CancellationToken ct)
    {
        await using var context = await _dbFactory.CreateDbContextAsync(ct);
        var video = await context.Videos.FindAsync([id], ct);
        if (video == null)
            throw new Exception("Video not found");
        var catColors = await CategoriesCodeDictionary(context, ct);
        return new VideoDto(video, c => catColors[c]);
    }

    public async Task<QuerySpec> GetStandardQuery(CancellationToken ct)
    {
        await using var context = await _dbFactory.CreateDbContextAsync(ct);
        return (await context.Queries.SingleAsync(CustomQuery.DefaultQueryExpression, ct)).Spec;
    }

    public async Task<CustomQuery[]> GetCustomQueries(CancellationToken ct)
    {
        await using var context = await _dbFactory.CreateDbContextAsync(ct);
        return await context.Queries.ToArrayAsync(ct);
    }

    public async Task RecalculateCalculatedTags(CancellationToken ct)
    {
        await using var context = await _dbFactory.CreateDbContextAsync(ct);
        var videos = await context.Videos.ToArrayAsync(ct);
        var unprocessed = await context.Tags.SingleAsync(t => t.Name == "x:unprocessed", ct);
        var isError = false;
        foreach (var video in videos)
        {
            var error = await RecalculateCalculatedTags(video, context, ct);
            if (error is not null)
            {
                video.AddTags(unprocessed);
                _logger.LogError(error);
                isError = true;
            }
        }
        await context.SaveChangesAsync(ct);
        if (isError) throw new Exception("Error recalculating calculated tags; videos have been marked as unprocessed");
    }

    private async Task<string?> RecalculateCalculatedTags(Video chosenVideo, VideoContext context, CancellationToken ct)
    {
        var calculatedTagNames = new List<string>();
        foreach (var calculatedTagRule in _tagValidation.CalculatedTagRules)
        {
            if (calculatedTagRule.cond(chosenVideo))
            {
                calculatedTagNames.AddRange(calculatedTagRule.tags(chosenVideo));
            }
        }
        var (calculatedTags, error) = await AddTags(calculatedTagNames.ToArray(), context, ct);
        if (error is not null) return error;
        var newTagSet = chosenVideo.Tags.UnionBy(calculatedTags.ToArray(), t => t.Id).ToArray();
        error = _tagValidation.ValidateTags(newTagSet);
        if (error is null)
        {
            chosenVideo.AddTags(calculatedTags.ToArray());
        }

        return error;
    }

    public string? ValidateTags(IEnumerable<ITag> tags)
    {
        return _tagValidation.ValidateTags(tags);
    }

    public IEnumerable<(Func<IVideo, bool> cond, Func<IVideo, string[]> tags)> CalculatedTagRules => _tagValidation.CalculatedTagRules;
    public string VideoEventTitle(IVideo video)
    {
        return _tagValidation.VideoEventTitle(video);
    }

    public string VideoEventTooltip(IVideo video)
    {
        return _tagValidation.VideoEventTooltip(video);
    }

    public async Task<int> GetVideosCount(CancellationToken ct)
    {
        await using var context = await _dbFactory.CreateDbContextAsync(ct);
        return await context.Videos.CountAsync(ct);
    }

    public async Task UpsertCustomQuery(string queryName, QuerySpec querySpec, CancellationToken ct)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(queryName);
        await using var context = await _dbFactory.CreateDbContextAsync(ct);
        if (context.Queries.FirstOrDefault(q => q.Label == queryName) is {} existingQuery)
        {
            existingQuery.Spec = querySpec;
            context.Queries.Update(existingQuery);
        }
        else
        {
            await context.Queries.AddAsync(new CustomQuery(Guid.NewGuid(), queryName, querySpec), ct);
        }
        
        await context.SaveChangesAsync(ct);
    }
    
    public async Task<string?> DeleteCustomQuery(string queryName, CancellationToken ct)
    {
        await using var context = await _dbFactory.CreateDbContextAsync(ct);
        var query = context.Queries.SingleOrDefault(q => q.Label == queryName);
        if (query != null)
        {
            if (query.IsDefaultQuery) return "Cannot delete default query";
            context.Queries.Remove(query);
        }

        await context.SaveChangesAsync(ct);
        return null;
    }

    public async Task<YearlyStat[]> GetYearlyStats(DateOnly startDate, CancellationToken ct)
    {
        await using var context = await _dbFactory.CreateDbContextAsync(ct);
        var globalStats = await context.Database.SqlQueryRaw<YearlyStat>("""
                                                                            with all_dates as 
                                                                                (select "StoreDate" as "Date" from "Watches" where "StoreDate" >= {0} union select "Date" from "NoVideoEvents")
                                                                            select 
                                                                                   null "year",
                                                                                   count("Date") count,
                                                                                   min("Date") mindate,
                                                                                   max("Date") maxdate,
                                                                                   (max("Date")-min("Date")) / count("Date") avgSepInDays
                                                                            from all_dates
                                                                            """, startDate).ToArrayAsync(ct);
        var yearlyStats = await context.Database.SqlQueryRaw<YearlyStat>("""
                                                  with all_dates as 
                                                      (select "StoreDate" as "Date" from "Watches" where "StoreDate" >= {0} union select "Date" from "NoVideoEvents")
                                                  select 
                                                         counts.dyear::int "year", 
                                                         count,
                                                         if(limitType = 'min', ldate, make_date(counts.dyear::int, 1, 1)) mindate,
                                                         if(limitType = 'max', ldate, make_date(counts.dyear::int, 12, 31)) maxdate, 
                                                         (if(limitType = 'max', ldate, make_date(counts.dyear::int, 12, 31)) -
                                                         if(limitType = 'min', ldate, make_date(counts.dyear::int, 1, 1))) / count avgSepInDays
                                                  from
                                                  (
                                                      select min("Date") ldate, date_part('year', min("Date")) dyear, 'min' limitType
                                                      from all_dates 
                                                      union
                                                      select max("Date") ldate, date_part('year', max("Date")) dyear, 'max' limitType
                                                      from all_dates) years
                                                  right join
                                                       (select  date_part('year', "Date") dyear, count("Date") count
                                                        from all_dates
                                                        group by date_part('year', "Date")) counts
                                                  on years.dyear = counts.dyear
                                                  """, startDate).ToArrayAsync(ct);
        return globalStats.Concat(yearlyStats).ToArray();
    }

    public async Task<VideoDto[]> GetVideosPerDate(DateOnly startDate, CancellationToken ct)
    {
        await using var context = await _dbFactory.CreateDbContextAsync(ct);
        return (await context.Videos.Where(v => v.Watches.Any(w => w.Date >= startDate)).ToArrayAsync(ct))
            .Select(v => new VideoDto(v)).ToArray();
    }

    public async Task<NoVideoEvent[]> GetNoVideoEvents(CancellationToken ct)
    {
        await using var context = await _dbFactory.CreateDbContextAsync(ct);
        return await context.NoVideoEvents.ToArrayAsync(ct);
    }

    public async Task<DateOnly?> FirstEvent(CancellationToken ct)
    {
        await using var context = await _dbFactory.CreateDbContextAsync(ct);
        var d1 = (await context.NoVideoEvents.OrderBy(e => e.Date).FirstOrDefaultAsync(ct))?.Date;
        var d2 = (await context.Watches.Where(e => e.Date.HasValue).OrderBy(e => e.Date).FirstOrDefaultAsync(ct))?.Date;
        if (d1 is not null && d2 is not null) return d1 < d2 ? d1 : d2;
        return d1 ?? d2;
    }

    public async Task<TagDetail[]> GetTags(CancellationToken ct)
    {
        await using var context = await _dbFactory.CreateDbContextAsync(ct);
        var x = context.Tags.Select(t => new TagDetail(new TagDto(t, t.Category!), t.Category!, t.Videos.Count));
        return await x.ToArrayAsync(ct);
    }

    public async Task DeleteTag(Guid tagId, CancellationToken ct)
    {
        await using var context = await _dbFactory.CreateDbContextAsync(ct);
        var tag = new Tag(tagId);
        context.Tags.Remove(tag);
        await context.SaveChangesAsync(ct);
    }
    
    public async Task DeleteCategory(char categoryId, CancellationToken ct)
    {
        await using var context = await _dbFactory.CreateDbContextAsync(ct);
        var category = new TagCategory("", categoryId, new TagCategoryColor(""), new TagCategoryColor(""));
        context.Categories.Remove(category);
        await context.SaveChangesAsync(ct);
    }

    public async Task CreateCategory(char categoryId, string name, string foregroundColor, string backgroundColor, CancellationToken ct)
    {
        await using var context = await _dbFactory.CreateDbContextAsync(ct);
        var category = new TagCategory(name, categoryId, new TagCategoryColor(foregroundColor), new TagCategoryColor(backgroundColor));
        context.Categories.Remove(category);
        await context.SaveChangesAsync(ct);
    }

    public async Task RenameTag(Guid tagId, string newTagText, CancellationToken ct)
    {
        await using var context = await _dbFactory.CreateDbContextAsync(ct);
        var tag = await context.Tags.FindAsync([tagId], ct);
        if (tag is null) return;
        tag.TagText = newTagText;
        await context.SaveChangesAsync(ct);
    }

    public async Task<WatchDayComment[]> GetWatchDayComments(CancellationToken ct)
    {
        await using var context = await _dbFactory.CreateDbContextAsync(ct);
        return await context.WatchDayComments.ToArrayAsync(ct);
    }
}

public record TagDetail(TagDto Tag, TagCategory Category, int VideosCount);