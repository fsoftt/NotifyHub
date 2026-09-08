namespace NotifyHub.Api.Infrastructure.Messaging.Contracts
{
    public sealed record NotificationCreatedMessage(
        string NotificationId,
        string UserId);
}
