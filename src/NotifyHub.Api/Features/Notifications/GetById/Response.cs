namespace NotifyHub.Api.Features.Notifications.GetById
{
    public sealed record Response(
        string Id,
        string UserId,
        string Type,
        string Title,
        string Message,
        bool Read,
        DateTimeOffset? ReadAt,
        DateTimeOffset CreatedAt);
}
