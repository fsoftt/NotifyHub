namespace NotifyHub.Api.Features.Notifications.GetSummary
{
    public static class Endpoint
    {
        public static IEndpointRouteBuilder MapGetNotificationsSummary(
            this IEndpointRouteBuilder endpoints)
        {
            endpoints.MapGet("/users/{userId}/notifications/summary",
                async (
                    string userId,
                    Handler handler,
                    CancellationToken cancellationToken) =>
                {
                    var query = new Query(userId);

                    var result = await handler.HandleAsync(
                        query,
                        cancellationToken);

                    return Results.Ok(result);
                });

            return endpoints;
        }
    }
}
