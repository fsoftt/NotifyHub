namespace NotifyHub.Api.Features.Notifications.List;

public record NotificationSummary(string Id, string Email, string Type, string ContentTitle, DateTime CreatedAt);
