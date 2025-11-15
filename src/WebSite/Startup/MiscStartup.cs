using Microsoft.Extensions.DependencyInjection.Extensions;
using VideoGallery.Library.Infrastructure;
using VideoGallery.Website.Auth;
using VideoGallery.Website.Shared;

namespace VideoGallery.Website.Startup;

public static class MiscStartup
{
    public static void SetupBuilder(WebApplicationBuilder builder)
    {
        builder.Services.AddRazorPages(options =>
        {
            options.RootDirectory = "/";
            options.Conventions.AuthorizeFolder("/");
        });
        builder.Services.AddHttpContextAccessor();
        builder.Services.AddTransient<IUserTokenStore, ServerSideTokenStore>();
        builder.Services.TryAddScoped<IDropboxClientFactory, WebDropboxClientFactory>();
    }

    public static void ConfigurePipeline(WebApplication app)
    {
        // Configure the HTTP request pipeline.
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Error", createScopeForErrors: true);
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }

        app.UseHttpsRedirection();
        app.UseAntiforgery();
        app.MapStaticAssets();
    }

    public static void MapRazor(WebApplication app)
    {
        app.MapRazorPages()
            .WithStaticAssets();
    }
}
