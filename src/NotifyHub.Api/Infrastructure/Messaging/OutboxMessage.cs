using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace NotifyHub.Api.Infrastructure.Messaging;

public class OutboxMessage
{
    public const string CollectionName = "outbox_messages";

    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;

    public string NotificationId { get; set; } = string.Empty;

    // Serialized NotificationCreatedMessage - stored as the wire payload the
    // OutboxProcessor will publish as-is, not re-derived from the notification.
    public string Payload { get; set; } = string.Empty;

    public bool Processed { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? ProcessedAt { get; set; }
}
