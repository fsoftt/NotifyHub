using Microsoft.Extensions.Options;
using NotifyHub.Api.Infrastructure.Messaging.Contracts;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace NotifyHub.Api.Infrastructure.Messaging
{
    public sealed class RabbitMqConsumerWorker: BackgroundService
    {
        private readonly RabbitMqOptions options;
        private readonly NotificationConsumer consumer;

        public RabbitMqConsumerWorker(
            IOptions<RabbitMqOptions> options,
            NotificationConsumer consumer)
        {
            this.options = options.Value;
            this.consumer = consumer;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Console.WriteLine("RabbitMQ Consumer: starting...");

            var factory = new ConnectionFactory
            {
                HostName = options.Host,
                Port = options.Port,
                UserName = options.Username,
                Password = options.Password
            };

            await using var connection = await factory.CreateConnectionAsync(stoppingToken);
            Console.WriteLine("RabbitMQ Consumer: connection created...");

            await using var channel = await connection.CreateChannelAsync(cancellationToken: stoppingToken);
            Console.WriteLine("RabbitMQ Consumer: channel created...");

            await RabbitMqTopology.ConfigureAsync(channel, stoppingToken);
            Console.WriteLine("RabbitMQ Consumer: queue configured");

            var rabbitConsumer = new AsyncEventingBasicConsumer(channel);

            rabbitConsumer.ReceivedAsync += async (_, args) =>
            {
                try
                {
                    var json = Encoding.UTF8.GetString(args.Body.ToArray());
                    Console.WriteLine("RabbitMQ Consumer: message received: " + json);

                    var message = JsonSerializer.Deserialize<NotificationCreatedMessage>(json);
                    if (message is null)
                    {
                        Console.WriteLine("RabbitMQ Consumer: message deserialization failed");
                        return;
                    }

                    await consumer.HandleAsync(message, stoppingToken);

                    await channel.BasicAckAsync(
                        args.DeliveryTag,
                        multiple: false,
                        cancellationToken: stoppingToken);

                    Console.WriteLine("RabbitMQ Consumer: message processed and acknowledged");
                }
                catch (Exception)
                {
                    Console.WriteLine("RabbitMQ Consumer: message processing failed, message will be requeued");

                    var retryCount = RetryPolicy.GetRetryCount(args);
                    if (retryCount < RetryPolicy.MaxRetries)
                    {
                        await PublishRetryAsync(
                            channel,
                            args.Body,
                            retryCount,
                            stoppingToken);
                        Console.WriteLine($"RabbitMQ Consumer: message published to retry queue (retry count: {retryCount + 1})");
                    }
                    else
                    {
                        await PublishDeadLetterAsync(
                            channel,
                            args.Body,
                            stoppingToken);
                        Console.WriteLine("RabbitMQ Consumer: message reached max retry count, will be sent to dead letter queue");
                    }

                    await channel.BasicAckAsync(
                        args.DeliveryTag,
                        multiple: false,
                        cancellationToken: stoppingToken);
                }
            };

            await channel.BasicConsumeAsync(
                queue: RabbitMqTopology.CreatedQueue,
                autoAck: false,
                consumer: rabbitConsumer,
                cancellationToken: stoppingToken);

            Console.WriteLine("RabbitMQ Consumer: consuming messages...");

            await Task.Delay(
                Timeout.Infinite, 
                stoppingToken);
        }

        private async Task PublishRetryAsync(
            IChannel channel,
            ReadOnlyMemory<byte> body,
            int retryCount,
            CancellationToken cancellationToken)
        {
            var nextRetryCount = retryCount + 1;

            var queue = RetryPolicy.GetQueue(nextRetryCount);

            var properties = new BasicProperties
            {
                Persistent = true,
                Headers = new Dictionary<string, object?>
                {
                    [RetryPolicy.RetryCountHeader] = nextRetryCount
                }
            };

            await channel.BasicPublishAsync(
                exchange: RabbitMqTopology.Exchange,
                routingKey: queue,
                mandatory: false,
                basicProperties: properties,
                body: body,
                cancellationToken: cancellationToken);
        }

        private static async Task PublishDeadLetterAsync(
            IChannel channel,
            ReadOnlyMemory<byte> body,
            CancellationToken cancellationToken)
        {
            var properties = new BasicProperties
            {
                Persistent = true
            };

            await channel.BasicPublishAsync(
                exchange: RabbitMqTopology.Exchange,
                routingKey: RabbitMqTopology.DeadLetterQueue,
                mandatory: false,
                basicProperties: properties,
                body: body,
                cancellationToken: cancellationToken);
        }
    }
}
