using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace NotifyHub.Api.Infrastructure.Messaging;

// Responsibility (section 9): keep a RabbitMQ consumer running as a
// BackgroundService. Must not contain Notification business logic itself -
// that's NotificationConsumer's job (5.8), which this class only delegates to.
public class RabbitMqConsumerWorker : BackgroundService
{
    private readonly RabbitMqConnection connection;
    private readonly NotificationConsumer notificationConsumer;
    private readonly ILogger<RabbitMqConsumerWorker> logger;

    public RabbitMqConsumerWorker(
        RabbitMqConnection connection,
        NotificationConsumer notificationConsumer,
        ILogger<RabbitMqConsumerWorker> logger)
    {
        this.connection = connection;
        this.notificationConsumer = notificationConsumer;
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

            if (message is null)
            {
                logger.LogError("Received a message on {Queue} that could not be deserialized; leaving it unacked.", NotificationsTopology.QueueName);
                return;
            }

            try
            {
                await notificationConsumer.ProcessAsync(message, stoppingToken);
                await channel.BasicAckAsync(args.DeliveryTag, multiple: false, cancellationToken: stoppingToken);
            }
            catch (Exception ex)
            {
                // No retry/DLQ yet (5.11/5.12) - deliberately not acking on failure so the
                // message stays pending rather than being silently lost.
                logger.LogError(ex, "Failed to process {MessageId}; leaving it unacked.", message.MessageId);
            }
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
