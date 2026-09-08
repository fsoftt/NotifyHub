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

            await channel.ExchangeDeclareAsync(
                exchange: "notifications",
                type: ExchangeType.Topic,
                durable: true,
                autoDelete: false,
                arguments: null,
                cancellationToken: stoppingToken);

            await channel.QueueDeclareAsync(
                queue: "notifications.created",
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null,
                cancellationToken: stoppingToken);

            await channel.QueueBindAsync(
                queue: "notifications.created",
                exchange: "notifications",
                routingKey: "notifications.created",
                arguments: null,
                cancellationToken: stoppingToken);

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
                cancellationToken: stoppingToken);

            Console.WriteLine("RabbitMQ Consumer: consuming messages...");

            await Task.Delay(
                Timeout.Infinite, 
                stoppingToken);
        }
    }
}
