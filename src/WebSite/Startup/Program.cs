using Microsoft.Extensions.DependencyInjection.Extensions;
using VideoGallery.Interfaces;
using VideoGallery.Library;
using VideoGallery.Library.DefaultPlugin;
using VideoGallery.Website.Auth;
using VideoGallery.Website.Startup;
using VideoGallery.Website.VideoGrid;

var app = await BuildTools.BuildApp(args);
MiscStartup.ConfigurePipeline(app);
AuthStartup.ConfigurePipeline(app);
AuthStartup.MapEndpoints(app);
Startup.MapEndpoints(app);
MiscStartup.MapRazor(app);

app.Run();

namespace VideoGallery.Website.Startup
{
    public static class BuildTools
    {
        public static async Task<WebApplication> BuildApp(params string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            var configuration = builder.Configuration;
            var logger = builder.Services.BuildServiceProvider().GetRequiredService<ILogger<PluginLoader>>();
            logger.LogInformation("Starting up...");
            Environment.CurrentDirectory = Path.GetDirectoryName(typeof(Program).Assembly.Location) ?? Environment.CurrentDirectory;
            if (!await PluginLoader.LoadExtensions(logger, configuration, type => builder.Services.TryAddScoped(typeof(ITagValidation), type)))
                builder.Services.TryAddScoped<ITagValidation, DefaultTagValidation>();

            MiscStartup.SetupBuilder(builder);
            AuthStartup.RegisterServices(builder, builder.Configuration);
            global::VideoGallery.Website.VideoGrid.Startup.RegisterServices(builder.Services, builder.Configuration); 
            global::VideoGallery.Website.Calendar.CalendarStartup.RegisterServices(builder.Services, builder.Configuration);

            var webApplication = builder.Build();
            return webApplication;
        }

    }
}