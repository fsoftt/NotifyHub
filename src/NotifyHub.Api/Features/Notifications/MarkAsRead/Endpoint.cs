namespace NotifyHub.Api.Features.Notifications.MarkAsRead
{
    public static class Endpoint
    {
        public static IEndpointRouteBuilder MapMarkNotificationAsRead(
            this IEndpointRouteBuilder endpoints)
        {
            endpoints.MapPatch(
                "/notifications/{id}/read",
                async (
                    string id,
                    Handler handler,
                    CancellationToken cancellationToken) =>
            {
                var command = new Command(id);

                var found = await handler.HandleAsync(command, cancellationToken);

                return found
                    ? Results.NoContent()
                    : Results.NotFound();
            });

            return endpoints;
        }
    }
}
