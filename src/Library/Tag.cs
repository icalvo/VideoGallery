using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using VideoGallery.Interfaces;

namespace VideoGallery.Library;

public partial class Tag : ITag
{
    private string _name;
    private char _tagCategoryId;
    private string _tagText;

    [GeneratedRegex(@"[a-z]:[a-z0-9\-_]+")]
    private static partial Regex TagRegex();

    public Tag(char tagCategoryId, string tagText, string name)
    {
        Name = name;
    }
    
    public Tag(string name)
    {
        Name = name;
    }

    public static string Compose(char cat, string text) => $"{cat}:{text}";
    public static bool IsValid(string name) => TagRegex().IsMatch(name);

    public Guid Id { get; private set; }

    public string Name
    {
        get => _name;
        [MemberNotNull(nameof(_tagCategoryId), nameof(_tagText), nameof(_name))]
        private set
        {
            ArgumentNullException.ThrowIfNull(value);
            ArgumentOutOfRangeException.ThrowIfLessThan(value.Length, 3);
            SetStuff(value[0], value[2..]);
        }
    }

    public char TagCategoryId
    {
        get => _tagCategoryId;
        set => SetStuff(value, TagText);
    }

    public string TagText
    {
        get => _tagText;
        set => SetStuff(TagCategoryId, value);
    }

    [MemberNotNull(nameof(_tagCategoryId), nameof(_tagText), nameof(_name))]
    private void SetStuff(char cat, string text)
    {
        ArgumentNullException.ThrowIfNull(text);
        var name = Compose(cat, text);
        if (!TagRegex().IsMatch(name))
            throw new ArgumentException("Tag name must be in the format x:a3b-c_d", nameof(name));
        _tagCategoryId = cat;
        _tagText = text;
        _name = name;
    }

    private bool Equals(Tag other)
    {
        return Id.Equals(other.Id);
    }

    public override bool Equals(object? obj)
    {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((Tag)obj);
    }

    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }
}
