using System.Text.Json;
using Confluent.Kafka;
using RuleEngine.Core.Abstractions;
using StackExchange.Redis;

namespace RuleEngine.SyncWorker;

public class Worker : BackgroundService
{
    private readonly string _kafkaTopic = "dbserver1.inventory.Rules"; // Debezium topic format
    private readonly IConnectionMultiplexer _redis;
    private readonly IRuleRepository _mongoRepo;
    private readonly ILogger<Worker> _logger;

    public Worker(IConnectionMultiplexer redis, IRuleRepository mongoRepo, ILogger<Worker> logger)
    {
        _redis = redis;
        _mongoRepo = mongoRepo;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var config = new ConsumerConfig
        {
            BootstrapServers = "kafka:9092",
            GroupId = "sync-worker-group",
            AutoOffsetReset = AutoOffsetReset.Earliest
        };

        using var consumer = new ConsumerBuilder<Ignore, string>(config).Build();
        consumer.Subscribe(_kafkaTopic);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var result = consumer.Consume(stoppingToken);
                _logger.LogInformation("Received CDC Event from Kafka");

                // 1. We don't parse the CDC JSON diff. It's safer to just fetch the "Full State" 
                //    from Mongo to ensure absolute consistency in Redis.
                var allRules = await _mongoRepo.GetAllActiveRulesAsync();
                
                // 2. Serialize for Redis
                var json = JsonSerializer.Serialize(allRules);
                var db = _redis.GetDatabase();
                
                // 3. Update Redis "Source of Truth"
                await db.StringSetAsync("rules:active_set", json);
                
                // 4. Notify Processors to reload their L1 RAM cache
                // We use Redis Pub/Sub for internal signaling (faster than loopback Kafka)
                await db.PublishAsync("rules:notification", "reload");
                
                _logger.LogInformation("Redis Updated & Notification Sent");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in SyncWorker");
            }
        }
    }
}