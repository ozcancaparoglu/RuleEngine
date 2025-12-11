using RuleEngine.Compiler;
using RuleEngine.Core;

namespace RuleEngine.Tests;

public class CompilerTests
{
    [Fact]
    public void CompileRule_ShouldReturnTrue_WhenConditionMet()
    {
        var compiler = new RuleCompiler();
        var rule = "Input.Amount > 100 && Input.Country == \"US\"";
        var func = compiler.CompileRule(rule);

        var context = new TransactionContext { Amount = 150, Country = "US" };
        Assert.True(func(context));
    }

    [Fact]
    public void CompileRule_ShouldReturnFalse_WhenConditionFailed()
    {
        var compiler = new RuleCompiler();
        var func = compiler.CompileRule("Input.Amount > 100");

        var context = new TransactionContext { Amount = 50 };
        Assert.False(func(context));
    }
}