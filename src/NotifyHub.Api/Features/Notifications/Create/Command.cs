namespace NotifyHub.Api.Features.Notifications.Create
{
    public sealed record Command(
        string UserId,
        string Type,
        string Title,
        string Message);
}
