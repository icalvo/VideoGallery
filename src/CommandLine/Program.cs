using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Npgsql;
using VideoGallery.CommandLine;
using VideoGallery.CommandLine.Listing;
using VideoGallery.Interfaces;
using VideoGallery.Library;
using VideoGallery.Library.DefaultPlugin;

var config = BuildConfiguration();

var mainCommand = BuildMainCommand();
await mainCommand.Run(args);
return;

IConfiguration BuildConfiguration() =>
    new ConfigurationBuilder()
        .AddUserSecrets(Assembly.GetEntryAssembly()!)
        .Build();
ICommand BuildMainCommand()
{
    var cnxstr = config["ConnectionStrings:Db"] ?? throw new Exception("No connection string");
    var options = new Options(
        config["Storage:Folder"] ?? throw new Exception("Storage:Folder must be defined"), 
        config["Storage:Type"] ?? throw new Exception("Storage:Type must be defined"));
    var pluginType = PluginLocator.TryFindPluginType();
    ITagValidation tagValidation =
        pluginType == null ? new DefaultTagValidation() :
        (ITagValidation)(Activator.CreateInstance(pluginType) ?? throw new Exception($"Failed to create plugin {pluginType.FullName}"));
    var application = new Application(NullLogger<Application>.Instance, new SimpleDbContextFactory(cnxstr), tagValidation);
    Verb[] generalVerbs =
    [
        new("add", () => new AddVideo(options, application)),
        new("list", () => new ListVideos(BuildContext())),
        new("init", () => new InitDatabase(BuildContext())),
        new("calctags", () => new RecalculateCalculatedTags(application)),
        new("calendar", () => new Calendar(BuildContext())),
        new("novideo", () => new RegisterNoVideoEvent(BuildContext())),
    ];

    if (args.Length == 0)
    {
        Console.WriteLine("Cnxstr: " + cnxstr);
        Console.WriteLine("Options: " + options);
        Console.WriteLine("Plugin: " + (pluginType?.FullName ?? "Default"));
        return new Shell(BuildContext(), ShellVerbs,
            generalVerbs.Where(x => x.Name != "list"));
    }

    return new ExecuteVerb(generalVerbs);

    Verb[] ShellVerbs(ShellContext sc) =>
    [
        new("list", () => new Filter(BuildContext(), sc)),
        new("details", () => new ShowDetails(BuildContext(), sc)),
        new("open", () => new OpenVideo(BuildContext(), sc, options)),
        new("update", () => new UpdateVideo(BuildContext(), sc)),
        new("watch", () => new WatchVideo(BuildContext(), sc)),
        new("unwatch", () => new UnwatchVideo(BuildContext(), sc)),
        new("exit", () => new Exit())
    ];

    VideoContext BuildContext() =>
        new(
            new DbContextOptionsBuilder<VideoContext>()
                .UseNpgsql(new NpgsqlConnection(cnxstr))
                .Options);
}
