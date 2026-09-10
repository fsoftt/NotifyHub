using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace NotifyHub.Api.Infrastructure.Messaging
{
    public sealed class RabbitMqPublisher : IAsyncDisposable
    {
        private const int MaxPublishAttempts = 2;

        private readonly RabbitMqOptions options;
        private readonly SemaphoreSlim connectionLock = new(1, 1);

        private IConnection? connection;
        private IChannel? channel;

        public RabbitMqPublisher(IOptions<RabbitMqOptions> options) 
        {
            this.options = options.Value;
        }

        private async Task<IChannel> GetChannelAsync(CancellationToken cancellationToken)
        {
            if (connection is not null
                && connection.IsOpen
                && channel is not null
                && channel.IsOpen) 
            {
                return channel;
            }

            await connectionLock.WaitAsync(cancellationToken);

            try
            {
                if (connection is not null
                    && connection.IsOpen
                    && channel is not null
                    && channel.IsOpen)
                {
                    return channel;
                }

                return await CreateChannelAsync(cancellationToken);
            }
            finally
            {
                connectionLock.Release();
            }
        }

        private async Task<IChannel> CreateChannelAsync(
            CancellationToken cancellationToken)
        {
            var factory = new ConnectionFactory
            {
                HostName = options.Host,
                Port = options.Port,
                UserName = options.Username,
                Password = options.Password,
            };

            connection = await factory.CreateConnectionAsync(cancellationToken);

            channel = await connection.CreateChannelAsync(
                new CreateChannelOptions(
                    publisherConfirmationsEnabled: true,
                    publisherConfirmationTrackingEnabled: true),
                cancellationToken);

            return channel;
        }

        public async Task PublishAsync<T>(
            string exchangeName,
            string routingKey,
            T message,
            CancellationToken cancellationToken)
        {
            for (var attempt = 1;
                attempt <= MaxPublishAttempts;
                attempt++)
            {
                try
                {
                    var channel = await GetChannelAsync(cancellationToken);

                    var json = JsonSerializer.Serialize(message);
                    var body = Encoding.UTF8.GetBytes(json);

                    await channel.BasicPublishAsync(
                        exchange: exchangeName,
                        routingKey: routingKey,
                        body: body,
                        cancellationToken: cancellationToken);

                    return;
                }
                catch (Exception) when (attempt < MaxPublishAttempts)
                {
                    await InvalidateConnectionAsync();

                    await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
                }
            }

            throw new InvalidOperationException("RabbitMQ publish failed.");
        }

        private async Task InvalidateConnectionAsync()
        {
            await connectionLock.WaitAsync();

            try
            {
                await CleanConnectionAsync();
            }
            finally
            {
                connectionLock.Release();
            }
        }

        public async ValueTask DisposeAsync()
        {
            await CleanConnectionAsync();

            connectionLock.Dispose();
        }

        private async Task CleanConnectionAsync()
        {
            if (channel is not null)
            {
                await channel.DisposeAsync();
                channel = null;
            }

            if (connection is not null)
            {
                await connection.DisposeAsync();
                connection = null;
            }
        }
    }
}
