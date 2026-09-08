using Microsoft.Extensions.Options;
using NotifyHub.Api.Infrastructure.Messaging.Contracts;
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
            var factory = new RabbitMQ.Client.ConnectionFactory
            {
                HostName = options.Host,
                Port = options.Port,
                UserName = options.Username,
                Password = options.Password
            };

            await using var connection = await factory.CreateConnectionAsync(stoppingToken);
            await using var channel = await connection.CreateChannelAsync(cancellationToken: stoppingToken);

            await channel.QueueDeclareAsync(
                queue: "notifications.created",
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null,
                cancellationToken: stoppingToken);

            var rabbitConsumer = new AsyncEventingBasicConsumer(channel);

            rabbitConsumer.ReceivedAsync += async (_, args) =>
            {
                try
                {
                    var json = Encoding.UTF8.GetString(args.Body.ToArray());
                    var message = JsonSerializer.Deserialize<NotificationCreatedMessage>(json);
                    if (message is null)
                    {
                        return;
                    }

                    await consumer.HandleAsync(message, stoppingToken);

                    await channel.BasicAckAsync(
                        args.DeliveryTag,
                        multiple: false,
                        cancellationToken: stoppingToken);
                }
                catch (Exception)
                {
                    await channel.BasicNackAsync(
                        args.DeliveryTag,
                        multiple: false,
                        requeue: true,
                        cancellationToken: stoppingToken);
                }
            };

            await channel.BasicConsumeAsync(
                queue: "notifications.created",
                autoAck: false,
                consumer: rabbitConsumer,
                consumerTag: string.Empty,
                noLocal: false,
                exclusive: false,
                arguments: null,
                cancellationToken: stoppingToken);

            await Task.Delay(
                Timeout.Infinite, 
                stoppingToken);
        }
    }
}
