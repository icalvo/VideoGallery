using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Npgsql;
using VideoGallery.Library;

namespace VideoGallery.CommandLine;

public class VideoContextFactory : IDesignTimeDbContextFactory<VideoContext>
{
    public VideoContext CreateDbContext(string[] args) =>
        new(
            new DbContextOptionsBuilder<VideoContext>()
                .UseNpgsql(new NpgsqlConnection())
                .Options);
}