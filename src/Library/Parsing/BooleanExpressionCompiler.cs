using System.Linq.Expressions;
using System.Reflection;

namespace VideoGallery.Library.Parsing;

public static class BooleanExpressionCompiler
{
    /// <summary>
    /// Converts a BooleanExpression to a LINQ Expression that takes an object of type T
    /// and returns a boolean result. Uses a provided atom resolver expression to handle atoms.
    /// </summary>
    /// <typeparam name="T">The type of the object to evaluate</typeparam>
    /// <param name="expression">The boolean expression to compile</param>
    /// <param name="atomResolver">Expression that takes (T obj, string atomName) and returns bool</param>
    /// <returns>A LINQ Expression&lt;Func&lt;T, bool&gt;&gt;</returns>
    public static Expression<Func<T, bool>> Compile<T>(
        BooleanExpression expression,
        Func<string, Expression<Func<T, bool>>> atomResolver)
    {
        var parameter = Expression.Parameter(typeof(T), "obj");
        var body = CompileExpression(expression, parameter, atomResolver);
        return Expression.Lambda<Func<T, bool>>(body, parameter);
    }

    private static Expression CompileExpression<T>(
        BooleanExpression expression,
        ParameterExpression objParam,
        Func<string, Expression<Func<T, bool>>> atomResolver)
    {
        return expression switch
        {
            AtomExpression atom => CompileAtom(atom.Name, objParam, atomResolver),
            AndExpression and => Expression.AndAlso(
                CompileExpression(and.Left, objParam, atomResolver),
                CompileExpression(and.Right, objParam, atomResolver)
            ),
            OrExpression or => Expression.OrElse(
                CompileExpression(or.Left, objParam, atomResolver),
                CompileExpression(or.Right, objParam, atomResolver)
            ),
            NotExpression not => Expression.Not(
                CompileExpression(not.Expression, objParam, atomResolver)
            ),
            _ => throw new ArgumentException($"Unknown expression type: {expression.GetType()}")
        };
    }

    private static Expression CompileAtom<T>(
        string atomName,
        ParameterExpression objParam,
        Func<string, Expression<Func<T, bool>>> atomResolverFunc)
    {
        var atomResolver = atomResolverFunc(atomName);
        var closureResolver = new ClosureResolver(atomResolver.Parameters[0], objParam);
        var resolvedAtomResolver = closureResolver.Visit(atomResolver.Body);
        return resolvedAtomResolver;

        // Replace the parameters in the atom resolver with our parameter and the atom name constant
    }

    private class ClosureResolver(
        ParameterExpression oldParam,
        Expression newParam
        ) : ExpressionVisitor
    {

        protected override Expression VisitParameter(ParameterExpression node)
            => node == oldParam ? newParam : base.VisitParameter(node);

        protected override Expression VisitMember(MemberExpression node)
        {
            // Check if this is a field access on a closure (compiler-generated class)
            var x = TryGetConstantField(node);
            if (x == null) return base.VisitMember(node);
            var (constantExpression, field) = x.Value;
            var closureInstance = constantExpression.Value;
                
            // Check if this looks like a compiler-generated closure
            if (closureInstance == null || !IsClosureType(closureInstance.GetType())) return base.VisitMember(node);
            try
            {
                // Get the value of the field from the closure instance
                var fieldValue = field.GetValue(closureInstance);
                        
                // Return a constant expression with the captured value
                return Expression.Constant(fieldValue, node.Type);
            }
            catch
            {
                // If we can't get the value, return the original expression
                return base.VisitMember(node);
            }

            static (ConstantExpression, FieldInfo)? TryGetConstantField(MemberExpression node) => 
                node is { Member: FieldInfo field, Expression: ConstantExpression ce } ? (ce, field) : null;
        }
        
        protected override Expression VisitConstant(ConstantExpression node)
        {
            // Handle the case where the constant itself is a closure
            if (node.Value != null && IsClosureType(node.Value.GetType()))
            {
                // For display classes that contain delegates or other complex types,
                // we might want to keep them as-is to avoid issues
                return node;
            }
            
            return base.VisitConstant(node);
        }
        
        private static bool IsClosureType(Type type)
        {
            // Check for compiler-generated closure classes
            // These typically have names like "<>c__DisplayClass" or similar patterns
            return type.Name.Contains("DisplayClass") ||
                   type.Name.Contains("<>c") ||
                   type.Name.StartsWith("<>") ||
                   type.GetCustomAttribute<System.Runtime.CompilerServices.CompilerGeneratedAttribute>() != null;
        }
    }
}