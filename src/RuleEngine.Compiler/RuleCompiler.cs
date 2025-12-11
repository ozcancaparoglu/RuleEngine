using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using RuleEngine.Core;

namespace RuleEngine.Compiler;

public class RuleCompiler
{
    public Func<TransactionContext, bool> CompileRule(string expression)
    {
        if (string.IsNullOrWhiteSpace(expression)) return _ => false;

        var param = Expression.Parameter(typeof(TransactionContext), "Input");
            
        try 
        {
            var lambda = DynamicExpressionParser.ParseLambda(
                [param], typeof(bool), expression);
            return (Func<TransactionContext, bool>)lambda.Compile();
        }
        catch
        {
            // In prod, log this properly
            return _ => false;
        }
    }
}