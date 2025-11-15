namespace VideoGallery.CommandLine;

public record Verb(string Name, Func<ICommand> BuildCommand);