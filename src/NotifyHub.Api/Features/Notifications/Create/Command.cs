namespace NotifyHub.Api.Features.Notifications.Create
{
    public sealed record Command(
        string UserId,
        string Email,
        string Type,
        string Title,
        string Message);
}
