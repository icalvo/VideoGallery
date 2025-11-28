using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Npgsql;
using VideoGallery.Library;

namespace VideoGallery.Website.Tags;

public static class Startup
{
    public static void RegisterServices(IServiceCollection sc, IConfiguration config)
    {
        var cnxstr = config["ConnectionStrings:Db"] ?? throw new Exception("No connection string");

        sc.AddDbContextFactory<VideoContext>(opt =>
        {
            opt.UseNpgsql(new NpgsqlConnection(cnxstr), pgopt =>
            {
                pgopt.EnableRetryOnFailure(4);
            });
        });        
        sc.TryAddScoped<Application>();
    }
}