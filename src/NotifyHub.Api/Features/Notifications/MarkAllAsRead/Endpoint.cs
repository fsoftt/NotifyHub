namespace NotifyHub.Api.Features.Notifications.MarkAllAsRead
{
    public static class Endpoint
    {
        public static IEndpointRouteBuilder MapMarkAllNotificationsAsRead(
            this IEndpointRouteBuilder endpoints)
        {
            endpoints.MapPatch(
                "/users/{userId}/notifications/read",
                async (
                    string userId,
                    Handler handler,
                    CancellationToken cancellationToken) =>
            {
                var command = new Command(userId);

                var modifiedCount = await handler.HandleAsync(command, cancellationToken);

                return Results.Ok(new
                {
                    modifiedCount
                });
            });

            return endpoints;
        }
    }
}
