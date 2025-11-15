using System.Globalization;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using MockQueryable;
using VideoGallery.Library.Parsing;

namespace VideoGallery.Library;

public class VideoContext : DbContext
{
    public VideoContext(DbContextOptions<VideoContext> options) : base(options)
    {
    }

    public DbSet<Video> Videos { get; set; }
    public DbSet<NoVideoEvent> NoVideoEvents { get; set; }
    public DbSet<Watch> Watches { get; set; }
    public DbSet<Tag> Tags { get; set; }
    public DbSet<TagCategory> Categories { get; set; }
    public DbSet<CustomQuery> Queries { get; set; }
    
    private static readonly string[] DurationFormats = ["%m", "m':'ss"];

    private static Expression<Func<Video, bool>> MaxDurExpression(string a)
    {
        var timeSpan = TimeSpan.ParseExact(a, DurationFormats, CultureInfo.InvariantCulture);
        return v => v.Duration <= timeSpan;
    }

    private static Expression<Func<Video, bool>> MinDurExpression(string a)
    {
        var timeSpan = TimeSpan.ParseExact(a, DurationFormats, CultureInfo.InvariantCulture);
        return v => v.Duration >= timeSpan;
    }

    private Expression<Func<Video, bool>> FilenameExpression(string a)
    {
        return Database.GetDbConnection().GetType().Name switch
        {
            "NpgsqlConnection" => v => EF.Functions.ILike(v.Filename, $"%{a}%"),
            _ => v => v.Filename.Contains(a)
        };
    }

    private static Expression<Func<Video, bool>> LastViewExpression(string a)
    {
        var date =
            a.EndsWith('m')
                ? DateOnly.FromDateTime(DateTime.Today).AddMonths(-int.Parse(a[..^1]))
                : a.EndsWith('y')
                    ? DateOnly.FromDateTime(DateTime.Today).AddYears(-int.Parse(a[..^1]))
                    : throw new Exception("Invalid last view format");
        return v => v.Watches.Max(w => w.StoreDate) >= date;
    }
    public IQueryable<CustomQueryExpression<Video>> CustomQueryExpressions => new[]
    {
        CustomQueryExpression<Video>.Prefix("maxdur:", MaxDurExpression),
        CustomQueryExpression<Video>.Prefix("mindur:", MinDurExpression),
        CustomQueryExpression<Video>.Prefix("lastview:", LastViewExpression),
        new(x => x.EndsWith(':'), x => v => v.Tags.Any(t => t.Name.StartsWith(x))),
        new(x => x.Contains(':'), x => v => v.Tags.Any(t => t.Name == x)),
        CustomQueryExpression<Video>.Default(FilenameExpression)
    }.BuildMock();
    public static IQueryable<Video> AddSorting(IQueryable<Video> q, QuerySpec querySpec) =>
        querySpec.SortField switch
        {
            nameof(Video.Filename) => q.Sort(querySpec.SortType, x => x.Filename),
            nameof(Video.Comments) => q.Sort(querySpec.SortType, x => x.Comments),
            nameof(Video.Duration) => q.Sort(querySpec.SortType, x => x.Duration),
            nameof(Video.NumSequences) => q.Sort(querySpec.SortType, x => x.NumSequences),
            nameof(Video.LastViewDate) => q.OrderBy(x => x.Watches.Count()).Sort(querySpec.SortType, x => x.Watches.Max(w => w.StoreDate)),
            _ => q
        };

    internal class TagCategoryColorConverter : ValueConverter<TagCategoryColor, string>
    {
        public TagCategoryColorConverter()
        : base(c => c.CssName, c => new TagCategoryColor(c))
        {
        }
    }
    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.Properties<TagCategoryColor>()
            .HaveConversion<TagCategoryColorConverter>();
        configurationBuilder.Properties<SortingType>()
            .HaveConversion<EnumToStringConverter<SortingType>>();
    }

    protected override void OnModelCreating(ModelBuilder b)
    {
        _ = b.Entity<Video>(v =>
        {
            v.HasKey(x => x.Id);
            v.Property(x => x.Filename).HasMaxLength(255).HasComment("File name");
            v.Property(x => x.Duration).HasComment("Video duration");
            v.Property(x => x.NumSequences).HasComment("# of sequences");
            v.Property(x => x.Comments).HasComment("Comments");
            v.Navigation(x => x.Watches).AutoInclude();
            v.HasMany(x => x.Tags).WithMany();
            v.Navigation(x => x.Tags).AutoInclude();
        });
        b.Entity<TagCategory>()
            .HasMany<Tag>()
            .WithOne()
            .HasForeignKey(e => e.TagCategoryId)
            .IsRequired();
        b.Entity<NoVideoEvent>().HasKey(x => x.Date);

        b.Entity<Watch>().Ignore(x => x.Date);
        b.Entity<Watch>().HasKey(x => new { x.VideoId, x.StoreDate, x.IsDateUnknown });
        b.Entity<Tag>().HasKey(x => x.Id);
        b.Entity<Tag>().ToTable("Tag");
        b.Entity<Tag>().Property(x => x.TagText).HasMaxLength(60);
        b.Entity<TagCategory>().HasKey(c => c.Code);
        b.Entity<CustomQuery>().ComplexProperty(q => q.Spec, x => x.IsRequired());
    }
}
