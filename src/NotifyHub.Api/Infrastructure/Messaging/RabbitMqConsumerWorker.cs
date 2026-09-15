using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace NotifyHub.Api.Infrastructure.Messaging;

// Responsibility (section 9): keep a RabbitMQ consumer running as a
// BackgroundService. Must not contain Notification business logic - that's
// NotificationConsumer / notification processing, 5.8, not built yet. For now
// this only proves the receive-and-ACK mechanism works.
public class RabbitMqConsumerWorker : BackgroundService
{
    private readonly RabbitMqConnection connection;
    private readonly ILogger<RabbitMqConsumerWorker> logger;

    public RabbitMqConsumerWorker(RabbitMqConnection connection, ILogger<RabbitMqConsumerWorker> logger)
    {
        this.connection = connection;
        this.logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await using var channel = await connection.CreateChannelAsync(cancellationToken: stoppingToken);

        await channel.BasicQosAsync(prefetchSize: 0, prefetchCount: 10, global: false, cancellationToken: stoppingToken);

        var consumer = new AsyncEventingBasicConsumer(channel);
        consumer.ReceivedAsync += async (sender, args) =>
        {
            var payload = Encoding.UTF8.GetString(args.Body.ToArray());
            var message = JsonSerializer.Deserialize<NotificationCreatedMessage>(payload);

            logger.LogInformation(
                "Received NotificationCreatedMessage {MessageId} for notification {NotificationId}",
                message?.MessageId,
                message?.NotificationId);

            await channel.BasicAckAsync(args.DeliveryTag, multiple: false, cancellationToken: stoppingToken);
        };

        await channel.BasicConsumeAsync(
            NotificationsTopology.QueueName,
            autoAck: false,
            consumer: consumer,
            cancellationToken: stoppingToken);

        try
        {
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        catch (OperationCanceledException)
        {
            // Expected on shutdown.
        }
    }
}
