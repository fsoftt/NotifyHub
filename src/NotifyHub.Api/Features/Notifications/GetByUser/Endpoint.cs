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
                    int? pageSize,
                    string? cursor,
                    Handler handler,
                    CancellationToken cancellationToken) =>
            {
                var query = new Query(
                    userId,
                    pageSize ?? 20,
                    cursor);

                var notifications = await handler.HandleAsync(
                    query,
                    cancellationToken);

                return Results.Ok(notifications);
            });

            return endpoints;
        }
    }
}
