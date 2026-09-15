using System.Text.Json;
using MongoDB.Driver;
using NotifyHub.Api.Infrastructure.Mongo;

namespace NotifyHub.Api.Infrastructure.Messaging;

public class OutboxProcessor : BackgroundService
{
    private static readonly TimeSpan pollInterval = TimeSpan.FromSeconds(2);

    private readonly MongoContext mongoContext;
    private readonly RabbitMqPublisher publisher;
    private readonly ILogger<OutboxProcessor> logger;

    public OutboxProcessor(MongoContext mongoContext, RabbitMqPublisher publisher, ILogger<OutboxProcessor> logger)
    {
        this.mongoContext = mongoContext;
        this.publisher = publisher;
        this.logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var collection = mongoContext.Database.GetCollection<OutboxMessage>(OutboxMessage.CollectionName);

        while (!stoppingToken.IsCancellationRequested)
        {
            var pendingFilter = Builders<OutboxMessage>.Filter.Eq(x => x.Processed, false);

            var pending = await collection.Find(pendingFilter)
                .SortBy(x => x.CreatedAt)
                .Limit(20)
                .ToListAsync(stoppingToken);

            foreach (var outboxMessage in pending)
            {
                try
                {
                    var message = JsonSerializer.Deserialize<NotificationCreatedMessage>(outboxMessage.Payload)
                        ?? throw new InvalidOperationException($"Outbox message {outboxMessage.Id} has an unreadable payload.");

                    await publisher.PublishAsync(message, stoppingToken);

                    var update = Builders<OutboxMessage>.Update
                        .Set(x => x.Processed, true)
                        .Set(x => x.ProcessedAt, DateTime.UtcNow);

                    await collection.UpdateOneAsync(
                        Builders<OutboxMessage>.Filter.Eq(x => x.Id, outboxMessage.Id),
                        update,
                        cancellationToken: stoppingToken);
                }
                catch (Exception ex)
                {
                    // No retry/backoff classification yet - that's 5.11/5.12 (Error
                    // classification, Retry). For now a failed publish is simply retried
                    // on the next poll, since the message is still marked Processed = false.
                    logger.LogError(ex, "Failed to process outbox message {OutboxMessageId}", outboxMessage.Id);
                }
            }

            await Task.Delay(pollInterval, stoppingToken);
        }
    }
}
