using MongoDB.Driver;
using NotifyHub.Api.Infrastructure.Mongo;
using NotifyHub.Api.Infrastructure.Notifications;

namespace NotifyHub.Api.Features.Notifications.List;

public static class Endpoint
{
    private const int DefaultLimit = 20;
    private const int MaxLimit = 100;

    public static void MapListNotifications(this IEndpointRouteBuilder app)
    {
        app.MapGet("/notifications", async (string? email, int? limit, MongoContext mongoContext) =>
        {
            var collection = mongoContext.Database.GetCollection<NotificationDocument>(NotificationDocument.CollectionName);

            var filter = string.IsNullOrWhiteSpace(email)
                ? Builders<NotificationDocument>.Filter.Empty
                : Builders<NotificationDocument>.Filter.Eq(x => x.Email, email);

            var effectiveLimit = Math.Clamp(limit ?? DefaultLimit, 1, MaxLimit);

            var documents = await collection.Find(filter)
                .SortByDescending(x => x.CreatedAt)
                .Limit(effectiveLimit)
                .ToListAsync();

            return Results.Ok(documents);
        });
    }
}
