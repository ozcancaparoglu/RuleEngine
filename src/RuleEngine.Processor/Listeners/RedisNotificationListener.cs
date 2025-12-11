using System.Text.Json;
using RuleEngine.Core;
using RuleEngine.Processor.Managers;
using StackExchange.Redis;

namespace RuleEngine.Processor.Listeners;

public class RedisNotificationListener : BackgroundService
{
    private readonly IConnectionMultiplexer _redis;
    private readonly RuleCacheManager _cacheManager;
    private readonly IServiceProvider _serviceProvider; // To resolve Scoped/Transient if needed

    public RedisNotificationListener(IConnectionMultiplexer redis, RuleCacheManager cacheManager)
    {
        _redis = redis;
        _cacheManager = cacheManager;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var sub = _redis.GetSubscriber();
        
        // Subscribe to the channel that SyncWorker publishes to
        await sub.SubscribeAsync("rules:notification", async (channel, message) => 
        {
            if (message == "reload")
            {
                // Fetch JSON from Redis and Update RAM
                var db = _redis.GetDatabase();
                var json = await db.StringGetAsync("rules:active_set");
                if (!json.IsNullOrEmpty)
                {
                    var rules = JsonSerializer.Deserialize<List<RuleDefinition>>(json);
                    _cacheManager.ReloadRules(rules);
                }
            }
        });
    }
}