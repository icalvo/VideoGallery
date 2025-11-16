using Microsoft.Extensions.DependencyInjection.Extensions;
using NReco.Logging.File;
using VideoGallery.Interfaces;
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
        public static Task<WebApplication> BuildApp(params string[] args)
        {
            Environment.CurrentDirectory = Path.GetDirectoryName(typeof(Program).Assembly.Location) ?? Environment.CurrentDirectory;
            var builder = WebApplication.CreateBuilder(args);
            builder.Logging.AddFile("app.log", o =>
            {
                o.FileSizeLimitBytes = 1_000_000;
                o.MaxRollingFiles = 20;
            });
            var sc = new ServiceCollection();
            sc.AddLogging(b => b.AddFile("startup.log", append: true));
            var logger = builder.Services.BuildServiceProvider().GetRequiredService<ILogger<Program>>();
            logger.LogInformation("Starting up...");
            var extensionType = PluginLocator.TryFindPluginType() ?? typeof(DefaultTagValidation);

            logger.LogInformation("Using tag validation extension: {ExtensionType}", extensionType.FullName);
            builder.Services.TryAddScoped(typeof(ITagValidation), extensionType);

            MiscStartup.SetupBuilder(builder);
            AuthStartup.RegisterServices(builder, builder.Configuration);
            global::VideoGallery.Website.VideoGrid.Startup.RegisterServices(builder.Services, builder.Configuration); 
            global::VideoGallery.Website.Calendar.CalendarStartup.RegisterServices(builder.Services, builder.Configuration);

            var webApplication = builder.Build();
            return Task.FromResult(webApplication);
        }
    }
}