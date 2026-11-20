namespace VideoGallery.Library.Parsing;

public abstract record BooleanExpression
{
    public static BooleanExpression Atom(string name) => new AtomExpression(name);
    public static BooleanExpression And(BooleanExpression left, BooleanExpression right) => new AndExpression(left, right);
    public static BooleanExpression Or(BooleanExpression left, BooleanExpression right) => new OrExpression(left, right);
    public static BooleanExpression Implies(BooleanExpression left, BooleanExpression right) => new ImpliesExpression(left, right);
    public static BooleanExpression Not(BooleanExpression expression) => new NotExpression(expression);
    public string ToString(Func<string, string> alias) => this switch
    {
        AtomExpression n => FormatAtom(n.Name, alias),
        AndExpression a => $"({a.Left.ToString(alias)} and {a.Right.ToString(alias)})",
        OrExpression a => $"({a.Left.ToString(alias)} or {a.Right.ToString(alias)})",
        ImpliesExpression a => $"({a.Left.ToString(alias)} implies {a.Right.ToString(alias)})",
        NotExpression a => $"not ({a.Expression.ToString(alias)})",
        _ => throw new ArgumentException($"Unknown expression type: {this.GetType()}")       
    };

    private static string FormatAtom(string name, Func<string, string> alias)
    {
        var aliased = alias(name);
        // Expanded query references must be spliced as subexpressions, not quoted atoms.
        if (QueryReferences.IsReference(name) && aliased != name)
            return $"({aliased})";
        return $"\"{aliased.Replace("\"", "\\\"")}\"";
    }
}