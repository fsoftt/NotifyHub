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

            var now = DateTime.UtcNow;

            var document = new NotificationDocument
            {
                UserId = request.UserId,
                Email = request.Email,
                Type = request.Type,
                Content = new NotificationContent
                {
                    Title = request.ContentTitle,
                    Message = request.ContentMessage
                },
                Channels = request.Channels.Select(channelType => BuildChannel(channelType, request.PushRecipients)).ToList(),
                CreatedAt = now,
                UpdatedAt = now
            };

            await collection.InsertOneAsync(document);

            return Results.Created($"/notifications/{document.Id}", document);
        });
    }

    private static NotificationChannel BuildChannel(NotificationChannelType channelType, List<string>? pushRecipients)
    {
        var channel = new NotificationChannel
        {
            Type = channelType,
            Status = NotificationChannelStatus.Pending
        };

        if (channelType == NotificationChannelType.Push)
        {
            channel.Recipients = (pushRecipients ?? [])
                .Select(token => new PushRecipientStatus { Token = token, Status = NotificationChannelStatus.Pending })
                .ToList();
        }

        if (channelType == NotificationChannelType.InApp)
        {
            channel.Read = false;
        }

        return channel;
    }

    public record Request(
        string? UserId,
        string Email,
        string Type,
        string ContentTitle,
        string ContentMessage,
        List<NotificationChannelType> Channels,
        List<string>? PushRecipients);
}
