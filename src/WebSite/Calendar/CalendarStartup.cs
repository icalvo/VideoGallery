using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Npgsql;
using VideoGallery.Library;

namespace VideoGallery.Website.Calendar;

public static class CalendarStartup
{
    public static void RegisterServices(IServiceCollection sc, IConfiguration config)
    {
        var cnxstr = config["ConnectionStrings:Db"] ?? throw new Exception("No connection string");

        sc.AddRazorComponents()
            .AddInteractiveServerComponents();
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