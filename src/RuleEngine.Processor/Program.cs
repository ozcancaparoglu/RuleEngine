using MongoDB.Driver;
using RuleEngine.Compiler;
using RuleEngine.Core;
using RuleEngine.Core.Abstractions;
using RuleEngine.Data.Repositories;
using RuleEngine.Processor.Listeners;
using RuleEngine.Processor.Managers;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

// 1. Infrastructure Setup
var mongoClient = new MongoClient(builder.Configuration.GetConnectionString("MongoDb"));
builder.Services.AddSingleton<IMongoDatabase>(sp => mongoClient.GetDatabase("RuleEngineDb"));
builder.Services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect("redis:6379"));

// 2. Dependency Injection
builder.Services.AddSingleton<IRuleRepository, MongoRuleRepository>();
builder.Services.AddSingleton<RuleCompiler>();
builder.Services.AddSingleton<RuleCacheManager>(); // The In-Memory Manager

// 3. Background Listener (Redis Pub/Sub for Hot Reload)
builder.Services.AddHostedService<RedisNotificationListener>();

var app = builder.Build();

// --- ENDPOINT 1: FAST EVALUATION ---
app.MapPost("/evaluate", (TransactionContext input, RuleCacheManager cache) =>
{
    var rules = cache.GetActiveRules(); // 0ms (RAM)
    var actions = new List<RuleAction>();

    foreach (var rule in rules)
    {
        if (rule.CompiledCondition(input))
            actions.AddRange(rule.SuccessActions);
    }

    return Results.Ok(new { MatchCount = actions.Count, Actions = actions });
});

// --- ENDPOINT 2: ADMIN CRUD (Writes to Mongo) ---
// Note: These are "slow" because they hit the DB. That is fine for Admin.
app.MapPost("/rules", async (RuleDefinition rule, IRuleRepository repo) =>
{
    await repo.CreateRuleAsync(rule);
    return Results.Created($"/rules/{rule.Id}", rule);
});

app.MapPut("/rules/{id}", async (string id, RuleDefinition rule, IRuleRepository repo) =>
{
    await repo.UpdateRuleAsync(id, rule);
    return Results.Ok();
});

app.MapDelete("/rules/{id}", async (string id, IRuleRepository repo) =>
{
    await repo.DeleteRuleAsync(id);
    return Results.Ok();
});

app.Run();