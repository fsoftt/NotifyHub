using MongoDB.Driver;
using NotifyHub.Api.Infrastructure.Mongo;
using NotifyHub.Api.Infrastructure.Notifications;

namespace NotifyHub.Api.Infrastructure.Email;

// Responsibility (section 9): process the Email channel. Does not know
// Resend's internal details - only talks to IEmailSender.
public class EmailNotificationProcessor
{
    private readonly IEmailSender emailSender;
    private readonly MongoContext mongoContext;
    private readonly ILogger<EmailNotificationProcessor> logger;

    public EmailNotificationProcessor(IEmailSender emailSender, MongoContext mongoContext, ILogger<EmailNotificationProcessor> logger)
    {
        this.emailSender = emailSender;
        this.mongoContext = mongoContext;
        this.logger = logger;
    }

    public async Task ProcessAsync(NotificationDocument notification, string messageId, CancellationToken cancellationToken = default)
    {
        var collection = mongoContext.Database.GetCollection<NotificationDocument>(NotificationDocument.CollectionName);
        var filter = Builders<NotificationDocument>.Filter.Eq(x => x.Id, notification.Id);

        try
        {
            // NotificationId + Channel as the idempotency key (section 16).
            await emailSender.SendAsync(
                idempotencyKey: $"{notification.Id}:Email",
                recipient: notification.Email,
                subject: notification.Content.Title,
                body: notification.Content.Message,
                cancellationToken);

            await NotificationChannelUpdates.SetStatusAsync(
                collection, filter, NotificationChannelType.Email, NotificationChannelStatus.Sent,
                sentAt: DateTime.UtcNow, cancellationToken: cancellationToken);

            logger.LogInformation(
                "Notification {NotificationId} Email channel sent (message {MessageId}).",
                notification.Id,
                messageId);
        }
        catch (Exception ex)
        {
            await NotificationChannelUpdates.SetStatusAsync(
                collection, filter, NotificationChannelType.Email, NotificationChannelStatus.Failed,
                cancellationToken: cancellationToken);

            logger.LogError(
                ex,
                "Notification {NotificationId} Email channel failed (message {MessageId}).",
                notification.Id,
                messageId);
        }
    }
}
