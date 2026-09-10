namespace NotifyHub.Api.Infrastructure.Notifications
{
    public sealed class LoggingEmailSender : IEmailSender
    {
        public Task SendAsync(
            string idempotencyKey,
            string recipient, 
            string subject, 
            string body, 
            CancellationToken cancellationToken)
        {
            Console.WriteLine($"IdempotencyKey: {idempotencyKey}");
            Console.WriteLine($"Email: {recipient}");
            Console.WriteLine($"Subject: {subject}");
            Console.WriteLine($"Body: {body}");

            return Task.CompletedTask;
        }
    }
}
