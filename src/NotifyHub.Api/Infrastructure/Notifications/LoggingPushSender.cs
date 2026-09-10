namespace NotifyHub.Api.Infrastructure.Notifications
{
    public class LoggingPushSender : IPushSender
    {
        public Task SendAsync(
            string idempotencyKey, 
            string recipient, 
            string title, 
            string body, 
            CancellationToken cancellationToken)
        {
            Console.WriteLine($"Push");
            Console.WriteLine($"Recipient: {recipient}");
            Console.WriteLine($"IdempotencyKey: {idempotencyKey}");
            Console.WriteLine($"Title: {title}");
            Console.WriteLine($"Body: {body}");

            return Task.CompletedTask;
        }
    }
}
