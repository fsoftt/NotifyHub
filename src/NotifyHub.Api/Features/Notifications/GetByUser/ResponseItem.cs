namespace NotifyHub.Api.Features.Notifications.GetByUser
{
    public sealed record ResponseItem(
        string Id,
        string Type,
        string Title,
        string Message,
        bool Read,
        DateTimeOffset CreatedAt);
}
