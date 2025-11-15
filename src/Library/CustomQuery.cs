using System.Linq.Expressions;
using VideoGallery.Library.Parsing;

namespace VideoGallery.Library;

public class CustomQuery
{
    public CustomQuery(Guid id, string label)
    {
        Id = id;
        Label = label;
        Spec = new QuerySpec("");
    }
    public CustomQuery(Guid id, string label, QuerySpec spec)
    {
        Id = id;
        Label = label;
        Spec = spec;
    }
    public Guid Id { get; private set; }
    public string Label { get; private set; }
    public QuerySpec Spec { get; set; }
    
    public static readonly Expression<Func<CustomQuery, bool>> DefaultQueryExpression = q => q.Label == "Standard";
    private static readonly Func<CustomQuery, bool> CompiledDefaultQueryExpression = DefaultQueryExpression.Compile();
    public bool IsDefaultQuery => CompiledDefaultQueryExpression(this);
}