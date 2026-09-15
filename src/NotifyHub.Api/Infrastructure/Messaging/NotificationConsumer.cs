using MongoDB.Bson;
using MongoDB.Driver;
using NotifyHub.Api.Infrastructure.Mongo;
using NotifyHub.Api.Infrastructure.Notifications;

namespace NotifyHub.Api.Infrastructure.Messaging;

// Responsibility (section 9): receive the event and coordinate Notification
// processing. Must not contain provider-specific details - IEmailSender/
// IPushSender and their adapters are 5.9/5.10, not built yet. What this class
// can legitimately do without them: find which channels still need work, and
// mark that work as started (Pending -> Sending).
public class NotificationConsumer
{
    private readonly MongoContext mongoContext;
    private readonly ILogger<NotificationConsumer> logger;

    public NotificationConsumer(MongoContext mongoContext, ILogger<NotificationConsumer> logger)
    {
        this.mongoContext = mongoContext;
        this.logger = logger;
    }

    public async Task ProcessAsync(NotificationCreatedMessage message, CancellationToken cancellationToken = default)
    {
        var collection = mongoContext.Database.GetCollection<NotificationDocument>(NotificationDocument.CollectionName);
        var filter = Builders<NotificationDocument>.Filter.Eq(x => x.Id, message.NotificationId);

        var notification = await collection.Find(filter).FirstOrDefaultAsync(cancellationToken);

        if (notification is null)
        {
            logger.LogWarning(
                "NotificationConsumer received {MessageId} for notification {NotificationId}, but no such notification exists.",
                message.MessageId,
                message.NotificationId);
            return;
        }

        var pendingChannelTypes = notification.Channels
            .Where(channel => channel.Status == NotificationChannelStatus.Pending)
            .Select(channel => channel.Type)
            .ToList();

        if (pendingChannelTypes.Count == 0)
        {
            logger.LogInformation(
                "Notification {NotificationId} has no Pending channels left to process (message {MessageId}).",
                message.NotificationId,
                message.MessageId);
            return;
        }

        foreach (var channelType in pendingChannelTypes)
        {
            var update = Builders<NotificationDocument>.Update
                .Set("Channels.$[elem].Status", NotificationChannelStatus.Sending.ToString())
                .Set(x => x.UpdatedAt, DateTime.UtcNow);

            var arrayFilters = new List<ArrayFilterDefinition>
            {
                new BsonDocumentArrayFilterDefinition<BsonDocument>(
                    new BsonDocument("elem.Type", channelType.ToString()))
            };

            await collection.UpdateOneAsync(
                filter,
                update,
                new UpdateOptions { ArrayFilters = arrayFilters },
                cancellationToken);

            // Provider dispatch (call IEmailSender/IPushSender) is 5.9/5.10 -
            // this log line marks where that plugs in, nothing more yet.
            logger.LogInformation(
                "Notification {NotificationId} channel {ChannelType} moved to Sending (message {MessageId}).",
                message.NotificationId,
                channelType,
                message.MessageId);
        }
    }
}
