using NotifyHub.Api.Infrastructure.Notifications;

namespace NotifyHub.Api.Features.Notifications.Create;

public record Command(
    string? UserId,
    string Email,
    string Type,
    string ContentTitle,
    string ContentMessage,
    List<NotificationChannelType> Channels,
    List<string>? PushRecipients);
