namespace NotifyHub.Api.Infrastructure.Notifications
{
    public interface IEmailSender
    {
        Task SendAsync(
            string idempotencyKey,
            string recipient,
            string subject,
            string body,
            CancellationToken cancellationToken);
    }
}
