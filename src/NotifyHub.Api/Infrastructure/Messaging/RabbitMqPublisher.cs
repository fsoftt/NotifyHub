using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace NotifyHub.Api.Infrastructure.Messaging
{
    public sealed class RabbitMqPublisher
    {
        private readonly RabbitMqOptions options;

        public RabbitMqPublisher(IOptions<RabbitMqOptions> options) 
        {
            this.options = options.Value;
        }

        public async Task PublishAsync<T>(
            string queueName,
            T message,
            CancellationToken cancellationToken)
        {
            var factory = new ConnectionFactory
            {
                HostName = options.Host,
                Port = options.Port,
                UserName = options.Username,
                Password = options.Password
            };

            await using var connection = await factory.CreateConnectionAsync(cancellationToken);
            await using var channel = await connection.CreateChannelAsync(cancellationToken: cancellationToken);

            await channel.QueueDeclareAsync(
                queue: queueName, 
                durable: true, 
                exclusive: false, 
                autoDelete: false, 
                arguments: null, 
                cancellationToken: cancellationToken);

            var json = JsonSerializer.Serialize(message);
            var body = Encoding.UTF8.GetBytes(json);

            await channel.BasicPublishAsync(
                exchange: string.Empty,
                routingKey: queueName,
                body: body,
                cancellationToken: cancellationToken);
        }
    }
}
