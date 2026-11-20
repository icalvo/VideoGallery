namespace VideoGallery.Library.Parsing;

public static class QueryReferences
{
    public static bool IsReference(string atom) =>
        atom.Length >= 3 && atom[0] == '`' && atom[^1] == '`';

    public static bool TryGetReferencedName(string atom, out string name)
    {
        if (IsReference(atom))
        {
            name = atom[1..^1];
            return name.Length > 0 && !name.Contains('`');
        }

        name = "";
        return false;
    }

    public static IEnumerable<string> GetReferencedNames(string search)
    {
        if (string.IsNullOrWhiteSpace(search))
            yield break;

        var tokenizeResult = Lexer.Tokenize(search, "and", "or", "not");
        if (!tokenizeResult.IsSuccess)
            yield break;

        var tokens = tokenizeResult.Match(t => t, _ => []);
        foreach (var token in tokens)
        {
            if (token.Type == TokenType.Atom && TryGetReferencedName(token.Value, out var name))
                yield return name;
        }
    }

    public static bool ContainsReference(string search, string queryName) =>
        GetReferencedNames(search).Contains(queryName);

    public static string RewriteReferences(string search, string oldName, string newName) =>
        search.Replace($"`{oldName}`", $"`{newName}`", StringComparison.Ordinal);

    public static HashSet<string> GetAllReferencedNames(IEnumerable<string> searches) =>
        searches.SelectMany(GetReferencedNames).ToHashSet(StringComparer.Ordinal);
}
