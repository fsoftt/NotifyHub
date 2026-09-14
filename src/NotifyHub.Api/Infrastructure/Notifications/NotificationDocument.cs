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

    public string Email { get; set; } = string.Empty;

    public string Type { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }
}
