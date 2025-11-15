using System.Linq.Expressions;

namespace VideoGallery.Library.Parsing;

public record CustomQueryExpression<T>(Func<string, bool> Condition, Func<string, Expression<Func<T, bool>>> Expression)
{
    public static CustomQueryExpression<T> Exact(string constant, Expression<Func<T, bool>> expression) => 
        new(s => s == constant, _ => expression);

    public static CustomQueryExpression<T> Prefix(string prefix, Func<string, Expression<Func<T, bool>>> expressionFunc) => 
        new(s => s.StartsWith(prefix), s => expressionFunc(s[prefix.Length..]));
    public static CustomQueryExpression<T> Default(Func<string, Expression<Func<T, bool>>> expressionFunc) => 
        new(_ => true, expressionFunc);
}


