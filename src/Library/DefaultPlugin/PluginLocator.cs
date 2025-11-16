using VideoGallery.Interfaces;

namespace VideoGallery.Library.DefaultPlugin;

public static class PluginLocator
{
    public static Type? TryFindPluginType() =>
        AppDomain.CurrentDomain.GetAssemblies()
            .Where(a => a != typeof(PluginLocator).Assembly)
            .SelectMany(a => a.GetTypes())
            .FirstOrDefault(t =>
                t is { IsClass: true, IsPublic: true } &&
                t.IsAssignableTo(typeof(ITagValidation)));
}