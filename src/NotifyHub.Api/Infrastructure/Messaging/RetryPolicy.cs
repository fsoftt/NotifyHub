using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace NotifyHub.Api.Infrastructure.Messaging
{
    public static class RetryPolicy
    {
        public const int MaxRetries = 3;
        public const string RetryCountHeader = "x-retry-count";

        public static string GetQueue(int retryCount)
        {
            return retryCount switch
            {
                0 => RabbitMqTopology.CreatedQueue,
                1 => RabbitMqTopology.RetryQueue1,
                2 => RabbitMqTopology.RetryQueue2,
                3 => RabbitMqTopology.RetryQueue3,
                _ => RabbitMqTopology.DeadLetterQueue
            };
        }

        public static int GetRetryCount(BasicDeliverEventArgs args)
        {
            if (args.BasicProperties.Headers == null)
            {
                return 0;
            }
            if (!args.BasicProperties.Headers.TryGetValue(RetryCountHeader, out var value))
            {
                return 0;
            }

            return value switch
            {
                int count => count,
                long count => checked((int)count),
                byte[] bytes when
                    int.TryParse(Encoding.UTF8.GetString(bytes), out var count) => count,
                _ => 0
            };
        }

        public static void SetRetryCount(IBasicProperties properties, int retryCount)
        {
            properties.Headers ??= new Dictionary<string, object?>();
            properties.Headers[RetryCountHeader] = retryCount;
        }
    }
}
