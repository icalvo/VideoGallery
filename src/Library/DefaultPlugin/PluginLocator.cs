using System.Runtime.Loader;
using VideoGallery.Interfaces;

namespace VideoGallery.Library.DefaultPlugin;

public static class PluginLocator
{
    public static Type? TryFindPluginType()
    {
        var ctx = new AssemblyLoadContext("PluginLoader", isCollectible: true);
        var thisTypeAssemblyLocation = typeof(PluginLocator).Assembly.Location;
        var assemblyPath = 
            Directory.GetFiles(Path.GetDirectoryName(thisTypeAssemblyLocation) ?? Environment.CurrentDirectory, "*.dll")
            .FirstOrDefault(filePath =>
            {
                var asm = ctx.LoadFromAssemblyPath(filePath);
                if (asm.Location == thisTypeAssemblyLocation)
                    return false;
                return asm.GetTypes()
                .Any(t =>
                    t is { IsClass: true, IsPublic: true } &&
                    t.IsAssignableTo(typeof(ITagValidation)));
            });
        ctx.Unload();
        if (assemblyPath is null) return null;
        var asm = AssemblyLoadContext.Default.LoadFromAssemblyPath(assemblyPath);
        return asm.GetTypes()
            .First(t =>
                t is { IsClass: true, IsPublic: true } &&
                t.IsAssignableTo(typeof(ITagValidation)));
    }
}