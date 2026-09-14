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
        app.MapGet("/notifications", async (string? email, int? limit, string? cursor, MongoContext mongoContext) =>
        {
            var filters = new List<FilterDefinition<NotificationDocument>>();

            if (!string.IsNullOrWhiteSpace(email))
            {
                filters.Add(Builders<NotificationDocument>.Filter.Eq(x => x.Email, email));
            }

            if (!string.IsNullOrWhiteSpace(cursor))
            {
                if (!Cursor.TryDecode(cursor, out var afterCreatedAt, out var afterId))
                {
                    return Results.BadRequest(new { error = "Invalid cursor." });
                }

                // Keyset pagination: strictly "older" than the last item of the previous page.
                // CreatedAt alone can tie within the same millisecond, so Id (an ObjectId hex
                // string, which sorts lexicographically the same as its underlying byte order)
                // breaks the tie deterministically.
                filters.Add(Builders<NotificationDocument>.Filter.Or(
                    Builders<NotificationDocument>.Filter.Lt(x => x.CreatedAt, afterCreatedAt),
                    Builders<NotificationDocument>.Filter.And(
                        Builders<NotificationDocument>.Filter.Eq(x => x.CreatedAt, afterCreatedAt),
                        Builders<NotificationDocument>.Filter.Lt(x => x.Id, afterId))));
            }

            var filter = filters.Count > 0
                ? Builders<NotificationDocument>.Filter.And(filters)
                : Builders<NotificationDocument>.Filter.Empty;

            var effectiveLimit = Math.Clamp(limit ?? DefaultLimit, 1, MaxLimit);

            var collection = mongoContext.Database.GetCollection<NotificationDocument>(NotificationDocument.CollectionName);

            var page = await collection.Find(filter)
                .SortByDescending(x => x.CreatedAt)
                .ThenByDescending(x => x.Id)
                .Limit(effectiveLimit + 1)
                .Project(x => new NotificationSummary(x.Id, x.Email, x.Type, x.Content.Title, x.CreatedAt))
                .ToListAsync();

            var hasMore = page.Count > effectiveLimit;
            var items = hasMore ? page.Take(effectiveLimit).ToList() : page;
            var nextCursor = hasMore ? Cursor.Encode(items[^1].CreatedAt, items[^1].Id) : null;

            return Results.Ok(new ListResponse(items, nextCursor));
        });
    }
}
