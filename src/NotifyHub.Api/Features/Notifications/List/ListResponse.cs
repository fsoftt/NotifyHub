namespace NotifyHub.Api.Features.Notifications.List;

public record ListResponse(List<NotificationSummary> Items, string? NextCursor);
