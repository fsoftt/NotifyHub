namespace NotifyHub.Api.Features.Notifications.GetById
{
    public static class Endpoint
    {
        public static IEndpointRouteBuilder MapGetNotificationById(this IEndpointRouteBuilder endpoints)
        {
            endpoints.MapGet(
                "/notifications/{id}",
                async (
                    string id,
                    Handler handler,
                    CancellationToken cancellationToken) =>
            {
                var query = new Query(id);

                var notification = await handler.HandleAsync(
                    query,
                    cancellationToken);

                return notification is null
                    ? Results.NotFound()
                    : Results.Ok(notification);
            });

            return endpoints;
        }
    }
}
