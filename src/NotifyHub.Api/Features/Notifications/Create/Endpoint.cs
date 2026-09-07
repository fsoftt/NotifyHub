namespace NotifyHub.Api.Features.Notifications.Create
{
    public static class Endpoint
    {
        public static IEndpointRouteBuilder MapCreateNotification(
            this IEndpointRouteBuilder endpoints)
        {
            endpoints.MapPost(
                "/notifications",
                async (
                    Command request,
                    Handler handler,
                    CancellationToken cancellationToken) =>
            {
                var id = await handler.Handle(request, cancellationToken);

                return Results.Ok(new
                {
                    id,
                });
            });

            return endpoints;
        }
    }
}
