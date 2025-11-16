using System.Reflection;
using System.Runtime.Loader;
using Microsoft.Extensions.Configuration;
using NuGet.Common;
using NuGet.Configuration;
using NuGet.Packaging;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using NuGet.Versioning;
using VideoGallery.Interfaces;

namespace VideoGallery.Library;

public record PackageSpec(string PackageId, string Version);

public class PluginLoader(string packagesFolder, ILogger logger)
{
    public static async Task<bool> LoadExtensions(
        Microsoft.Extensions.Logging.ILogger logger,
        IConfiguration configuration,
        Action<Type> tagValidationType)
    {
        var loggerProxy = new LoggerProxy(logger);
        var extensionCache = configuration.GetValue<string>("ExtensionFolder") ?? "Extensions";
        extensionCache = Path.GetFullPath(extensionCache);
        var extension = configuration.GetSection("ExtensionPackage").Get<PackageSpec>();
        var sources = configuration.GetSection("ExtensionSources").Get<string[]>() ?? [];
        if (extension == null)
        {
            loggerProxy.LogInformation("No extension package specified.");
            return false;
        }

        var loader = new PluginLoader(extensionCache, loggerProxy);
        var tagValidationLoaded = false;
        await loader.LoadPackages(
            [extension], 
            sources,
            assembly =>
            {
                var type = assembly.ExportedTypes.FirstOrDefault(x => 
                    x is { IsClass: true, IsPublic: true } && x.IsAssignableTo(typeof(ITagValidation)));
                if (type == null)
                {
                    loggerProxy.LogInformation($"No tag validation class found in {assembly.FullName}");
                    return;
                }

                loggerProxy.LogInformation($"Tag validation class {type.Name} found in {assembly.FullName}");
                tagValidationType(type);
                tagValidationLoaded = true;
            });
        return tagValidationLoaded;

    }

    private async Task LoadPackages(
        IEnumerable<PackageSpec> packages,
        IEnumerable<string> packageSources,
        Action<Assembly> processAssembly,
        IList<string>? loadedNugetPackages = null,
        IList<string>? failedNugetPackages = null,
        CancellationToken ct = default)
    {
        var packageSourcesArray = packageSources as string[] ?? packageSources.ToArray();
        if (packageSourcesArray.Length == 0)
        {
            logger.LogWarning("No package sources specified.");
            return;
        }

        foreach (var package in packages)
        {
            try
            {
                await LoadPackage(
                    package.PackageId,
                    package.Version,
                    processAssembly,
                    packageSourcesArray,
                    loadedNugetPackages, failedNugetPackages, ct); // lists
            }
            catch (Exception ex)
            {
                logger.LogError($"Failed to load {package.PackageId}: {ex.Message}");
                failedNugetPackages?.Add(package.PackageId);
            }
        }
    }

    private async Task LoadPackage(
        string packageId, 
        string versionRange, 
        Action<Assembly> processAssembly, 
        string[] packageSources,
        IList<string>? successPackages = null,
        IList<string>? failedPackages = null,
        CancellationToken ct = default)
    {
        successPackages ??= new List<string>();
        failedPackages ??= new List<string>();
        logger.LogInformation($"Loading {packageId} {versionRange}");

        var cache = new SourceCacheContext();
        var repositories = Repository.Provider.GetCoreV3().ToArray(); // gets the default nuget.org store

        var packageVersionRange = VersionRange.Parse(versionRange);

        using var packageStream = new MemoryStream();

        NuGetVersion? packageVersion = null;
        foreach (var source in packageSources)
        {
            FindPackageByIdResource? resource;
            if (!source.StartsWith("http"))
            {
                // local packages
                var fullSource = Path.GetFullPath(source);
                logger.LogInformation($"Loading {packageId} from {fullSource}");
                var sourceRepository = Repository.Factory.GetCoreV3(fullSource); // local path
                try
                {
                    resource = await sourceRepository.GetResourceAsync<FindPackageByIdResource>(ct);
                }
                catch (Exception ex)
                {
                    logger.LogError($"Failed to load {packageId} from {fullSource}: {ex.Message}");
                    continue;
                }
            }
            else
            {
                // only packages
                var packageSource = new PackageSource(source);
                var sourceRepository = new SourceRepository(packageSource, repositories);
                try
                {
                    resource = await sourceRepository.GetResourceAsync<FindPackageByIdResource>(ct);
                }
                catch (Exception ex)
                {
                    logger.LogError($"Failed to load {packageId} from {source}: {ex.Message}");
                    continue;
                }
            }

            var versions = (await resource.GetAllVersionsAsync(packageId, cache, logger, ct))?.ToArray() ?? [];
            packageVersion = packageVersionRange.FindBestMatch(versions);
            if (packageVersion == null)
            {
                logger.LogWarning($"No matching version found for {packageId} in {source}. Requested range: {packageVersionRange}, Versions found: {versions.StrJoin(",")}");
                continue;
            }

            logger.LogInformation($"Found {packageId} {packageVersion} from {source}");
            if (await resource.CopyNupkgToStreamAsync(packageId, packageVersion, packageStream, cache, logger, ct))
            {
                packageStream.Seek(0, SeekOrigin.Begin);
                break;
            }
            logger.LogError("Couldn't open nupkg stream");
    
            failedPackages.Add(packageId);
            return;
        }

        if (packageVersion == null || packageStream.Length < 1)
        {
            logger.LogError("Couldn't find package version or open nupkg stream");
            failedPackages.Add(packageId);
            return;
        }

        var packageReader = new PackageArchiveReader(packageStream);

        // find the highest compatible framework
        string? framework = GetTargetFramework(packageReader);

        // load dependencies
        var dependencies = 
            (await packageReader
                .GetPackageDependenciesAsync(ct))
                    .Where(d => d.TargetFramework.ToString() == framework)
                    .ToList();

        var packageDependencies = 
            dependencies
                .Select(packRef => packRef.Packages.FirstOrDefault())
                .WithoutNullsCls()
                .Where(pkg => pkg.Id != null); // framework references?
        foreach (var pkg in packageDependencies)
        {
            await LoadPackage(pkg.Id, pkg.VersionRange.ToNormalizedString(), processAssembly, packageSources, successPackages, failedPackages, ct);
        }

        bool error = false;
        var files = packageReader.GetFiles().Where(f => f.Contains($"/{framework}/"));
        var packagePath = Path.Combine(packagesFolder, packageId, packageVersion.ToNormalizedString());
        Directory.CreateDirectory(packagePath);
        foreach (var file in files)
        {
            var filePath = Path.Combine(packagePath, file.Replace("/", "\\"));
            if (!File.Exists(filePath))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(filePath) ?? throw new InvalidOperationException($"Couldn't get directory name for {filePath}"));  
                try
                {
                    await using var fileStream = File.Create(filePath);
                    await packageReader.GetStream(file).CopyToAsync(fileStream, ct);
                }
                catch { /* ignore - most likely the file exists already  */ }
            }

            if (filePath.EndsWith(".dll") && File.Exists(filePath))
            {
                try
                {
                    var assembly = AssemblyLoadContext.Default.LoadFromAssemblyPath(filePath);
                    processAssembly(assembly);
                }
                catch (Exception ex)
                {
                    logger.LogError($"Failed to load {packageId} from {filePath}: {ex.Message}");
                    error = true;
                    failedPackages.Add("-- " + Path.GetFileName(filePath));
                }
            }
        }

        if (error)
            failedPackages.Add(packageId);
        else
            successPackages.Add(packageId);
    }

    private static string? GetTargetFramework(PackageArchiveReader packageReader)
    {
        var frameworks = packageReader.GetReferenceItems()?.ToArray();
        if (frameworks == null) return null;

        string? framework = frameworks
            .Where(f => f.TargetFramework.ToString().StartsWith("net"))
            .OrderByDescending(f=> f.TargetFramework.ToString())
            .Select(f => f.TargetFramework.ToString())              
            .FirstOrDefault()
            ?? frameworks
            .FirstOrDefault(f => f.TargetFramework.ToString().StartsWith("netstandard"))
            ?.TargetFramework.ToString();

        return framework;
    }
}