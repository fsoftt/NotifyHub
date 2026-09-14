namespace NotifyHub.Api.Infrastructure.Messaging;

public record NotificationCreatedMessage(string MessageId, string NotificationId);
