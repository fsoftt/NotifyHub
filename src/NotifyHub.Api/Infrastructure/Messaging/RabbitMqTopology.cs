using RabbitMQ.Client;

namespace NotifyHub.Api.Infrastructure.Messaging
{
    public static class RabbitMqTopology
    {
        public const string Exchange = "notifications";
        public const string CreatedQueue = "notifications.created";
        public const string RetryQueue1 = "notifications.created.retry.1";
        public const string RetryQueue2 = "notifications.created.retry.2";
        public const string RetryQueue3 = "notifications.created.retry.3";
        public const string DeadLetterQueue = "notifications.created.dlq";

        public static async Task ConfigureAsync(
            IChannel channel, 
            CancellationToken cancellationToken)
        {
            await channel.ExchangeDeclareAsync(
                exchange: Exchange,
                type: ExchangeType.Topic,
                durable: true,
                autoDelete: false,
                arguments: null,
                cancellationToken: cancellationToken);

            await channel.QueueDeclareAsync(
                queue: CreatedQueue,
                durable: true,
                exclusive: false,
                autoDelete: false,
                cancellationToken: cancellationToken);

            await channel.QueueBindAsync(
                queue: CreatedQueue,
                exchange: Exchange,
                routingKey: "notifications.created",
                arguments: null,
                cancellationToken: cancellationToken);

            await AddRetry(channel, RetryQueue1, 5000, cancellationToken);
            await AddRetry(channel, RetryQueue2, 15000, cancellationToken);
            await AddRetry(channel, RetryQueue3, 30000, cancellationToken);

            await channel.QueueDeclareAsync(
                queue: DeadLetterQueue,
                durable: true,
                exclusive: false,
                autoDelete: false,
                cancellationToken: cancellationToken);

            await channel.QueueBindAsync(
                queue: DeadLetterQueue,
                exchange: Exchange,
                routingKey: DeadLetterQueue,
                arguments: null,
                cancellationToken: cancellationToken);
        }

        private static async Task AddRetry(IChannel channel, string queue, int timeout, CancellationToken cancellationToken)
        {
            var retryArguments = new Dictionary<string, object?>
            {
                { "x-message-ttl", timeout },
                { "x-dead-letter-exchange", Exchange },
                { "x-dead-letter-routing-key", DeadLetterQueue }
            };

            await channel.QueueDeclareAsync(
                queue: queue,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: retryArguments,
                cancellationToken: cancellationToken);

            await channel.QueueBindAsync(
                queue: queue,
                exchange: Exchange,
                routingKey: queue,
                arguments: null,
                cancellationToken: cancellationToken);
        }
    }
}
