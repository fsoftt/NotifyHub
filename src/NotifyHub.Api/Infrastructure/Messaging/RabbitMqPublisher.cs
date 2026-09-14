using System.Text.Json;
using RabbitMQ.Client;

namespace NotifyHub.Api.Infrastructure.Messaging;

public sealed class RabbitMqPublisher : IAsyncDisposable
{
    private readonly IChannel channel;

    private RabbitMqPublisher(IChannel channel)
    {
        this.channel = channel;
    }

    // Publisher confirms enabled (section 14): BasicPublishAsync below only completes once
    // the broker has actually confirmed the publish, not just handed it off locally.
    public static async Task<RabbitMqPublisher> CreateAsync(
        RabbitMqConnection connection,
        CancellationToken cancellationToken = default)
    {
        var channel = await connection.CreateChannelAsync(
            new CreateChannelOptions(
                publisherConfirmationsEnabled: true,
                publisherConfirmationTrackingEnabled: true),
            cancellationToken);

        return new RabbitMqPublisher(channel);
    }

    public async Task PublishAsync(NotificationCreatedMessage message, CancellationToken cancellationToken = default)
    {
        var body = JsonSerializer.SerializeToUtf8Bytes(message);

        var properties = new BasicProperties
        {
            Persistent = true,
            ContentType = "application/json"
        };

        await channel.BasicPublishAsync(
            NotificationsTopology.ExchangeName,
            NotificationsTopology.RoutingKey,
            mandatory: true,
            properties,
            body,
            cancellationToken);
    }

    public ValueTask DisposeAsync() => channel.DisposeAsync();
}
