namespace RuleEngine.Core;

public class ExecutableRule
{
    public string RuleId { get; set; }
    public string RuleName { get; set; }
    public int Priority { get; set; }

    // The Fast Delegate
    public Func<TransactionContext, bool> CompiledCondition { get; set; }

    public List<RuleAction> SuccessActions { get; set; }
}