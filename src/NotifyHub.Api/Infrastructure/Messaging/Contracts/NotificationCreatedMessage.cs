namespace NotifyHub.Api.Infrastructure.Messaging.Contracts
{
    public sealed record NotificationCreatedMessage(
        string MessageId,
        string NotificationId);
}
