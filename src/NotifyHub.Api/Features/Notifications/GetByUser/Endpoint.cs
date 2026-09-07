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
                if (pageSize < 0 || pageSize > 100)
                {
                    return Results.BadRequest("Page size must be between 0 and 100.");
                }

                bool isEmpty = string.IsNullOrEmpty(cursor);
                bool decoded = CursorEncoder.TryDecode(cursor, out var decodedCursor);
                if (!isEmpty && !decoded)
                {
                    return Results.BadRequest("Invalid cursor.");
                }

                var query = new Query(
                    userId,
                    pageSize ?? 20,
                    decodedCursor);

                var notifications = await handler.HandleAsync(
                    query,
                    cancellationToken);

                return Results.Ok(notifications);
            });

            return endpoints;
        }
    }
}
