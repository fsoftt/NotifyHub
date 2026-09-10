using Microsoft.Extensions.Options;
using MongoDB.Driver;
using NotifyHub.Api.Infrastructure.Messaging.Contracts;
using NotifyHub.Api.Infrastructure.Mongo;
using NotifyHub.Api.Infrastructure.Mongo.Documents;
using NotifyHub.Api.Infrastructure.Notifications;

namespace NotifyHub.Api.Infrastructure.Messaging
{
    public sealed class NotificationConsumer
    {
        private readonly MongoContext context;
        private readonly IPushSender pushSender;
        private readonly IEmailSender emailSender;
        private readonly IdempotencyStore idempotencyStore;
        private readonly IdempotencyOptions idempotencyOptions;

        public NotificationConsumer(
            MongoContext context,
            IPushSender pushSender,
            IEmailSender emailSender,
            IdempotencyStore idempotencyStore,
            IOptions<IdempotencyOptions> idempotencyOptions)
        {
            this.context = context;
            this.pushSender = pushSender;
            this.emailSender = emailSender;
            this.idempotencyStore = idempotencyStore;
            this.idempotencyOptions = idempotencyOptions.Value;
        }

        public async Task HandleAsync(
            NotificationCreatedMessage message,
            CancellationToken cancellationToken)
        {
            var acquired = await idempotencyStore.TryStartProcessingAsync(
                message.MessageId,
                cancellationToken);
            if (!acquired)
            {
                Console.WriteLine($"Message {message.MessageId} is already being processed.");
                return;
            }

            using var heartbeatCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            var heartbeatTask = RunLeaseHeartbeatAsync(message.MessageId, heartbeatCts.Token);

            try
            {
                Console.WriteLine($"Processing message: {message.MessageId}");

                var notification = await GetNotificationAsync(
                    message.NotificationId,
                    cancellationToken);
                if (notification is null)
                {
                    Console.WriteLine($"Notification not found: {message.NotificationId}");
                    return;
                }

                var errors = new List<Exception>();

                try
                {
                    if (!string.IsNullOrEmpty(notification.Email))
                    {
                        await ProcessEmailAsync(notification, cancellationToken);
                    }
                }
                catch (Exception ex)
                {
                    errors.Add(ex);
                }

                try
                {
                    if (!string.IsNullOrEmpty(notification.PushRecipient))
                    {
                        await ProcessPushAsync(notification, cancellationToken);
                    }
                }
                catch (Exception ex)
                {
                    errors.Add(ex);
                }

                if (errors.Count > 0)
                {
                    throw new AggregateException(
                        "One or more notification channels failed.",
                        errors);
                }

                await idempotencyStore.MarkProcessedAsync(
                    message.MessageId,
                    cancellationToken);
            }
            finally
            {
                heartbeatCts.Cancel();

                try
                {
                    await heartbeatTask;
                }
                catch (OperationCanceledException)
                {
                }
            }
        }

        private async Task RunLeaseHeartbeatAsync(
            string messageId,
            CancellationToken cancellationToken)
        {
            var interval = TimeSpan.FromSeconds(
                Math.Max(
                    1, 
                    idempotencyOptions.LeaseSeconds / 3));

            while (!cancellationToken.IsCancellationRequested)
            {
                await Task.Delay(interval, cancellationToken);

                var renewed = await idempotencyStore.RenewLeaseAsync(
                    messageId,
                    cancellationToken);
                if (!renewed)
                {
                    Console.WriteLine($"Failed to renew lease for message {messageId}. It may have been processed by another instance.");
                    break;
                }
            }
        }

        private async Task<NotificationDocument?> GetNotificationAsync(
            string notificationId,
            CancellationToken cancellationToken)
        {
            var collection = context.GetCollection<NotificationDocument>("notifications");

            var filter =
                Builders<NotificationDocument>
                    .Filter.Eq(x => x.Id, notificationId);

            return await collection
                .Find(filter)
                .FirstOrDefaultAsync(cancellationToken);
        }

        private async Task ProcessEmailAsync(
            NotificationDocument notification,
            CancellationToken cancellationToken)
        {
            var emailChannel = notification.Channels.FirstOrDefault(x => x.Type == "Email");
            if (emailChannel is null
                || emailChannel.Status == NotificationChannelStatus.Sent)
            {
                return;
            }

            await MarkAsSendingAsync(
                notification.Id,
                "Email",
                cancellationToken);

            var idempotencyKey = $"{notification.Id}:Email";

            try
            {
                await emailSender.SendAsync(
                    idempotencyKey,
                    notification.Email,
                    notification.Content.Title,
                    notification.Content.Message,
                    cancellationToken);

                await MarkAsSentAsync(
                    notification.Id,
                    "Email",
                    cancellationToken);
            }
            catch
            {
                await MarkAsFailedAsync(
                    notification.Id,
                    "Email",
                    cancellationToken);

                throw;
            }
        }

        private async Task ProcessPushAsync(
            NotificationDocument notification,
            CancellationToken cancellationToken)
        {
            var emailChannel = notification.Channels.FirstOrDefault(x => x.Type == "Push");
            if (emailChannel is null
                || emailChannel.Status == NotificationChannelStatus.Sent)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(notification.PushRecipient))
            {
                return;
            }

            await MarkAsSendingAsync(
                notification.Id,
                "Push",
                cancellationToken);

            var idempotencyKey = $"{notification.Id}:Push";

            try
            {
                await pushSender.SendAsync(
                    idempotencyKey,
                    notification.PushRecipient,
                    notification.Content.Title,
                    notification.Content.Message,
                    cancellationToken);

                await MarkAsSentAsync(
                    notification.Id,
                    "Push",
                    cancellationToken);
            }
            catch (Exception)
            {
                await MarkAsFailedAsync(
                    notification.Id,
                    "Push",
                    cancellationToken);

                throw;
            }
        }

        private async Task MarkAsSendingAsync(
            string notificationId,
            string notificationType,
            CancellationToken cancellationToken)
        {
            var collection = context.GetCollection<NotificationDocument>("notifications");

            var filter =
                Builders<NotificationDocument>
                    .Filter.And(
                        Builders<NotificationDocument>
                            .Filter.Eq(x => x.Id, notificationId),

                        Builders<NotificationDocument>
                            .Filter.Eq("Channels.Type", notificationType),

                        Builders<NotificationDocument>
                            .Filter.Eq("Channels.Status", NotificationChannelStatus.Pending));

            var update = Builders<NotificationDocument>
                .Update
                .Set(
                    "Channels.$.Status",
                    NotificationChannelStatus.Sending);

            UpdateResult updated = await collection.UpdateOneAsync(
                filter,
                update,
                cancellationToken: cancellationToken);
        }
        
        private async Task MarkAsSentAsync(
            string notificationId,
            string notificationType,
            CancellationToken cancellationToken)
        {
            var collection = context.GetCollection<NotificationDocument>("notifications");

            var filter =
                Builders<NotificationDocument>
                    .Filter.And(
                        Builders<NotificationDocument>
                            .Filter.Eq(x => x.Id, notificationId),

                        Builders<NotificationDocument>
                            .Filter.Eq("Channels.Type", notificationType));

            var update = Builders<NotificationDocument>
                .Update
                .Set(
                    "Channels.$.Status",
                    NotificationChannelStatus.Sent)
                .Set(
                    "channels.$.sentAt",
                    DateTimeOffset.UtcNow);

            UpdateResult updated = await collection.UpdateOneAsync(
                filter,
                update,
                cancellationToken: cancellationToken);
        }

        private async Task MarkAsFailedAsync(
            string notificationId,
            string notificationType,
            CancellationToken cancellationToken)
        {
            var collection = context.GetCollection<NotificationDocument>("notifications");

            var filter =
                Builders<NotificationDocument>
                    .Filter.And(
                        Builders<NotificationDocument>
                            .Filter.Eq(x => x.Id, notificationId),

                        Builders<NotificationDocument>
                            .Filter.Eq("Channels.Type", notificationType));

            var update = Builders<NotificationDocument>
                .Update
                .Set(
                    "Channels.$.Status",
                    NotificationChannelStatus.Failed);

            UpdateResult updated = await collection.UpdateOneAsync(
                filter,
                update,
                cancellationToken: cancellationToken);
        }
    }
}
