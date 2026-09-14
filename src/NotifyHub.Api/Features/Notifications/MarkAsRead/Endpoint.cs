using MongoDB.Bson;
using MongoDB.Driver;
using NotifyHub.Api.Infrastructure.Mongo;
using NotifyHub.Api.Infrastructure.Notifications;

namespace NotifyHub.Api.Features.Notifications.MarkAsRead;

public static class Endpoint
{
    public static void MapMarkNotificationAsRead(this IEndpointRouteBuilder app)
    {
        app.MapPatch("/notifications/{id}/read", async (string id, MongoContext mongoContext) =>
        {
            var collection = mongoContext.Database.GetCollection<NotificationDocument>(NotificationDocument.CollectionName);

            var now = DateTime.UtcNow;

            var filter = Builders<NotificationDocument>.Filter.Eq(x => x.Id, id);
            var update = Builders<NotificationDocument>.Update
                .Set("Channels.$[elem].Read", true)
                .Set("Channels.$[elem].ReadAt", now)
                .Set(x => x.UpdatedAt, now);

            var arrayFilters = new List<ArrayFilterDefinition>
            {
                new BsonDocumentArrayFilterDefinition<BsonDocument>(
                    new BsonDocument("elem.Type", NotificationChannelType.InApp.ToString()))
            };

            var document = await collection.FindOneAndUpdateAsync(
                filter,
                update,
                new FindOneAndUpdateOptions<NotificationDocument>
                {
                    ArrayFilters = arrayFilters,
                    ReturnDocument = ReturnDocument.After
                });

            if (document is null)
            {
                return Results.NotFound();
            }

            if (!document.Channels.Any(channel => channel.Type == NotificationChannelType.InApp))
            {
                return Results.BadRequest(new { error = "This notification has no InApp channel to mark as read." });
            }

            return Results.Ok(document);
        });
    }
}
