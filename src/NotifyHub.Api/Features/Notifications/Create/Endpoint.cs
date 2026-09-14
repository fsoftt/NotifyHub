using MongoDB.Driver;
using NotifyHub.Api.Infrastructure.Mongo;
using NotifyHub.Api.Infrastructure.Notifications;

namespace NotifyHub.Api.Features.Notifications.Create;

public static class Endpoint
{
    public static void MapCreateNotification(this IEndpointRouteBuilder app)
    {
        app.MapPost("/notifications", async (Request request, MongoContext mongoContext) =>
        {
            var collection = mongoContext.Database.GetCollection<NotificationDocument>(NotificationDocument.CollectionName);

            var document = new NotificationDocument
            {
                Email = request.Email,
                Type = request.Type,
                CreatedAt = DateTime.UtcNow
            };

            await collection.InsertOneAsync(document);

            return Results.Created($"/notifications/{document.Id}", document);
        });
    }

    public record Request(string Email, string Type);
}
