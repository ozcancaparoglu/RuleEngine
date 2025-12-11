using MongoDB.Driver;
using RuleEngine.Core;
using RuleEngine.Core.Abstractions;

namespace RuleEngine.Data.Repositories;

public class MongoRuleRepository : IRuleRepository
{
    private readonly IMongoCollection<RuleDefinition> _collection;

    public MongoRuleRepository(IMongoDatabase database)
    {
        _collection = database.GetCollection<RuleDefinition>("Rules");
    }

    public async Task<IEnumerable<RuleDefinition>> GetAllActiveRulesAsync()
    {
        return await _collection.Find(r => r.IsEnabled).ToListAsync();
    }

    public async Task CreateRuleAsync(RuleDefinition rule)
    {
        await _collection.InsertOneAsync(rule);
    }

    public async Task UpdateRuleAsync(string id, RuleDefinition rule)
    {
        await _collection.ReplaceOneAsync(r => r.Id == id, rule);
    }

    public async Task DeleteRuleAsync(string id)
    {
        await _collection.DeleteOneAsync(r => r.Id == id);
    }
}