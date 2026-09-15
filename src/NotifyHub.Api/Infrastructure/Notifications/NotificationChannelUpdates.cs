using MongoDB.Bson;
using MongoDB.Driver;

namespace NotifyHub.Api.Infrastructure.Notifications;

// Shared by NotificationConsumer and the channel processors, which all need
// to flip one channel's status via the same arrayFilters pattern.
public static class NotificationChannelUpdates
{
    public static Task SetStatusAsync(
        IMongoCollection<NotificationDocument> collection,
        FilterDefinition<NotificationDocument> filter,
        NotificationChannelType channelType,
        NotificationChannelStatus status,
        DateTime? sentAt = null,
        CancellationToken cancellationToken = default)
    {
        var update = Builders<NotificationDocument>.Update
            .Set("Channels.$[elem].Status", status.ToString())
            .Set(x => x.UpdatedAt, DateTime.UtcNow);

        if (sentAt.HasValue)
        {
            update = update.Set("Channels.$[elem].SentAt", sentAt.Value);
        }

        var arrayFilters = new List<ArrayFilterDefinition>
        {
            new BsonDocumentArrayFilterDefinition<BsonDocument>(new BsonDocument("elem.Type", channelType.ToString()))
        };

        return collection.UpdateOneAsync(filter, update, new UpdateOptions { ArrayFilters = arrayFilters }, cancellationToken);
    }
}
