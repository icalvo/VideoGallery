using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Npgsql;
using VideoGallery.Interfaces;
using VideoGallery.Library;
using VideoGallery.Library.Infrastructure;

namespace VideoGallery.Website.VideoGrid;

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
        
        sc.TryAddScoped<IVideoManager>(sp => new DropboxVideoManager(sp.GetRequiredService<IDropboxClientFactory>(), config["DropboxFolder"] ?? throw new Exception("No dropbox folder configured")));
        sc.TryAddScoped<Application>();
    }

    public static void MapEndpoints(WebApplication app)
    {
        // app.MapGet("/api/list", async (HttpContext context, IUserTokenStore svc, CancellationToken ct) =>
        // {
        //     var accessToken = await svc.GetTokenAsync(context.User, ct);
        //     var client = new DropboxClient(accessToken.AccessToken);
        //     var filePath = "/Disco/Video/Otros/sugerencias";
        //     var files = (await client.Files.ListFolderAsync(filePath)).Entries
        //         .Select(e => new { e.AsFile.Id, e.Name, e.PreviewUrl });
        //     return Results.Ok(files);
        // });
        //

        app.MapGet("/api/thumb/{video}", async (string video, IVideoManager manager, CancellationToken ct) =>
        {
            try
            {
                var thumbnailStream = await manager.GetThumbnail(video, ct);
                return Results.File(thumbnailStream, "image/png");
            }
            catch (Exception ex)
            {
                return Results.Problem(ex.Message, statusCode: 404);
            }
        });

        app.MapGet("/api/video/{video}", async (string video, IVideoManager manager, CancellationToken ct) =>
            Results.Redirect(await manager.GetVideoSharedLink(video, ct)));
        
        // app.MapGet("/api/link/{video}", async (string video, HttpContext context, IUserTokenStore tokenStore, CancellationToken ct) =>
        //     Results.Ok(await GetVideoSharedLink(video, context, tokenStore, ct)));
    }
}