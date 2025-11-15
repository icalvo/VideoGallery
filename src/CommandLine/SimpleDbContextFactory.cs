using Microsoft.EntityFrameworkCore;
using Npgsql;
using VideoGallery.Library;

namespace VideoGallery.CommandLine;

public class SimpleDbContextFactory : IDbContextFactory<VideoContext>
{
    private readonly string _cnxstr;

    public SimpleDbContextFactory(string cnxstr)
    {
        _cnxstr = cnxstr;
    }
    public VideoContext CreateDbContext() =>
        new(
            new DbContextOptionsBuilder<VideoContext>()
                .UseNpgsql(new NpgsqlConnection(_cnxstr))
                .Options);
}