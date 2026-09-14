using RabbitMQ.Client;

namespace NotifyHub.Api.Infrastructure.Messaging;

public static class NotificationsTopology
{
    public const string ExchangeName = "notifications.exchange";
    public const string QueueName = "notifications.queue";
    public const string RoutingKey = "notification.created";

    // Declarations are idempotent - safe to run on every startup.
    public static async Task DeclareAsync(IChannel channel, CancellationToken cancellationToken = default)
    {
        await channel.ExchangeDeclareAsync(
            ExchangeName,
            ExchangeType.Direct,
            durable: true,
            autoDelete: false,
            cancellationToken: cancellationToken);

        await channel.QueueDeclareAsync(
            QueueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            cancellationToken: cancellationToken);

        await channel.QueueBindAsync(
            QueueName,
            ExchangeName,
            RoutingKey,
            cancellationToken: cancellationToken);
    }
}
