namespace NotifyHub.Api.Features.Notifications.GetByUser
{
    public sealed record Query(
        string UserId,
        int PageSize,
        Cursor? Cursor);
}
