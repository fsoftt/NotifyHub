using Microsoft.Extensions.Options;
using NotifyHub.Api.Infrastructure.Messaging.Contracts;

namespace NotifyHub.Api.Infrastructure.Messaging
{
    public sealed class NotificationConsumer
    {
        private readonly IdempotencyStore idempotencyStore;
        private readonly IdempotencyOptions idempotencyOptions;

        public NotificationConsumer(
            IdempotencyStore idempotencyStore,
            IOptions<IdempotencyOptions> idempotencyOptions)
        {
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
                Console.WriteLine($"Processing message: {message.MessageId} for user: {message.UserId}");
                // TODO: processing

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
    }
}
