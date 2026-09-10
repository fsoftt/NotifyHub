namespace NotifyHub.Api.Infrastructure.Notifications
{
    public interface IPushSender
    {
        Task SendAsync(
            string idempotencyKey,
            string recipient,
            string title,
            string body,
            CancellationToken cancellationToken);
    }
}
