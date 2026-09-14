using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace NotifyHub.Api.Infrastructure.Notifications;

public class NotificationDocument
{
    public const string CollectionName = "notifications";

    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;

    public int SchemaVersion { get; set; } = 1;

    public string? UserId { get; set; }

    public string Email { get; set; } = string.Empty;

    public string Type { get; set; } = string.Empty;

    public NotificationContent Content { get; set; } = new();

    public List<NotificationChannel> Channels { get; set; } = new();

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}
