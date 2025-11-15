using System.Text.RegularExpressions;
using VideoGallery.Interfaces;
using VideoGallery.Library;

namespace VideoGallery.CommandLine;

public partial record TagProposal : ITag
{
    public TagProposal(string name)
    {
        if (!Tag.IsValid(name))
            throw new ArgumentException("Tag name must be in the format x:a3b-c_d", nameof(name));
        Name = name;
    }

    public string Name { get; }
    public char TagCategoryId => Name[0];
    public string TagText => Name[2..];

    [GeneratedRegex(@"[a-z]:[a-z0-9\-_]+")]
    private static partial Regex TagRegex();

    public static implicit operator TagProposal(string name) => new(name);
}