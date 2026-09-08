using NotifyHub.Api.Infrastructure.Messaging.Contracts;

namespace NotifyHub.Api.Infrastructure.Messaging
{
    public sealed class NotificationConsumer
    {
        public Task HandleAsync(
            NotificationCreatedMessage message,
            CancellationToken cancellationToken)
        {
            Console.WriteLine($"Notification created: {message.NotificationId} for user: {message.UserId}");
         
            return Task.CompletedTask;
        }
    }
}
