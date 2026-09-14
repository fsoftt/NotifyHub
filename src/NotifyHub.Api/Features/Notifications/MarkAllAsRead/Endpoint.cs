using MongoDB.Bson;
using MongoDB.Driver;
using NotifyHub.Api.Infrastructure.Mongo;
using NotifyHub.Api.Infrastructure.Notifications;

namespace NotifyHub.Api.Features.Notifications.MarkAllAsRead;

public static class Endpoint
{
    public static void MapMarkAllNotificationsAsRead(this IEndpointRouteBuilder app)
    {
        app.MapPost("/notifications/mark-all-read", async (string? email, MongoContext mongoContext) =>
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return Results.BadRequest(new { error = "email is required." });
            }

            var collection = mongoContext.Database.GetCollection<NotificationDocument>(NotificationDocument.CollectionName);

            var now = DateTime.UtcNow;

            var filter = Builders<NotificationDocument>.Filter.And(
                Builders<NotificationDocument>.Filter.Eq(x => x.Email, email),
                Builders<NotificationDocument>.Filter.ElemMatch(
                    x => x.Channels,
                    channel => channel.Type == NotificationChannelType.InApp && channel.Read == false));

            var update = Builders<NotificationDocument>.Update
                .Set("Channels.$[elem].Read", true)
                .Set("Channels.$[elem].ReadAt", now)
                .Set(x => x.UpdatedAt, now);

            var arrayFilters = new List<ArrayFilterDefinition>
            {
                new BsonDocumentArrayFilterDefinition<BsonDocument>(
                    new BsonDocument
                    {
                        { "elem.Type", NotificationChannelType.InApp.ToString() },
                        { "elem.Read", false }
                    })
            };

            var result = await collection.UpdateManyAsync(
                filter,
                update,
                new UpdateOptions { ArrayFilters = arrayFilters });

            return Results.Ok(new { matchedCount = result.MatchedCount, modifiedCount = result.ModifiedCount });
        });
    }
}
