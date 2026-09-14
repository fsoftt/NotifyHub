using MongoDB.Driver;
using NotifyHub.Api.Infrastructure.Mongo;
using NotifyHub.Api.Infrastructure.Notifications;

namespace NotifyHub.Api.Features.Notifications.GetById;

public static class Endpoint
{
    public static void MapGetNotificationById(this IEndpointRouteBuilder app)
    {
        app.MapGet("/notifications/{id}", async (string id, MongoContext mongoContext) =>
        {
            var collection = mongoContext.Database.GetCollection<NotificationDocument>(NotificationDocument.CollectionName);
            var filter = Builders<NotificationDocument>.Filter.Eq(x => x.Id, id);
            var cursor = await collection.FindAsync(filter);
            var document = await cursor.FirstOrDefaultAsync();

            return document is null ? Results.NotFound() : Results.Ok(document);
        });
    }
}
