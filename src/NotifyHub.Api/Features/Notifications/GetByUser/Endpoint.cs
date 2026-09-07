namespace NotifyHub.Api.Features.Notifications.GetByUser
{
    public static class Endpoint
    {
        public static IEndpointRouteBuilder MapGetNotificationsByUser(
            this IEndpointRouteBuilder endpoints)
        {
            endpoints.MapGet(
                "/users/{userId}/notifications",
                async (
                    string userId, 
                    Handler handler,
                    CancellationToken cancellationToken) =>
                {
                    var query = new Query(userId);
                    var notifications = await handler.HandleAsync(
                    query,
                    cancellationToken);

                    return Results.Ok(notifications);
                });

            return endpoints;
        }
    }
}
