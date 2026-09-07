namespace NotifyHub.Api.Features.Notifications.GetByUser
{
    public sealed record Response(
        IReadOnlyCollection<ResponseItem> Items,
        string? NextCursor);
}
