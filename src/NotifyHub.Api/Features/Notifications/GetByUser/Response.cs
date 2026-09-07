namespace NotifyHub.Api.Features.Notifications.GetByUser
{
    public sealed record Response(
        string Id,
        string Type,
        string Title,
        string Message,
        bool Read,
        DateTimeOffset CreatedAt);
}
