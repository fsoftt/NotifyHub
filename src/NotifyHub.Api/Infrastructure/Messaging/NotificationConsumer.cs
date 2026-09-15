using MongoDB.Driver;
using NotifyHub.Api.Infrastructure.Email;
using NotifyHub.Api.Infrastructure.Mongo;
using NotifyHub.Api.Infrastructure.Notifications;

namespace NotifyHub.Api.Infrastructure.Messaging;

// Responsibility (section 9): receive the event and coordinate Notification
// processing - route each Pending channel to what handles it, without
// knowing provider details itself.
public class NotificationConsumer
{
    private readonly MongoContext mongoContext;
    private readonly EmailNotificationProcessor emailProcessor;
    private readonly ILogger<NotificationConsumer> logger;

    public NotificationConsumer(
        MongoContext mongoContext,
        EmailNotificationProcessor emailProcessor,
        ILogger<NotificationConsumer> logger)
    {
        this.mongoContext = mongoContext;
        this.emailProcessor = emailProcessor;
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
            switch (channelType)
            {
                case NotificationChannelType.Email:
                    await NotificationChannelUpdates.SetStatusAsync(
                        collection, filter, channelType, NotificationChannelStatus.Sending, cancellationToken: cancellationToken);
                    await emailProcessor.ProcessAsync(notification, message.MessageId, cancellationToken);
                    break;

                case NotificationChannelType.InApp:
                    // No provider dispatch needed - an in-app notification exists the
                    // moment it's persisted, so it goes straight to Sent.
                    await NotificationChannelUpdates.SetStatusAsync(
                        collection, filter, channelType, NotificationChannelStatus.Sent,
                        sentAt: DateTime.UtcNow, cancellationToken: cancellationToken);
                    logger.LogInformation(
                        "Notification {NotificationId} InApp channel sent (message {MessageId}).",
                        message.NotificationId,
                        message.MessageId);
                    break;

                case NotificationChannelType.Push:
                default:
                    // Push provider dispatch is 5.10, not built yet - stays Sending.
                    await NotificationChannelUpdates.SetStatusAsync(
                        collection, filter, channelType, NotificationChannelStatus.Sending, cancellationToken: cancellationToken);
                    logger.LogInformation(
                        "Notification {NotificationId} channel {ChannelType} moved to Sending; no processor yet (message {MessageId}).",
                        message.NotificationId,
                        channelType,
                        message.MessageId);
                    break;
            }
        }
    }
}
