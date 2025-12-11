namespace RuleEngine.Core.Abstractions;

public interface IRuleRepository
{
    Task<IEnumerable<RuleDefinition>> GetAllActiveRulesAsync();
    Task CreateRuleAsync(RuleDefinition rule);
    Task UpdateRuleAsync(string id, RuleDefinition rule);
    Task DeleteRuleAsync(string id);
}