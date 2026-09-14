using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace NotifyHub.Api.Infrastructure.Notifications;

public class NotificationChannel
{
    [BsonRepresentation(BsonType.String)]
    public NotificationChannelType Type { get; set; }

    [BsonRepresentation(BsonType.String)]
    public NotificationChannelStatus Status { get; set; } = NotificationChannelStatus.Pending;

    public DateTime? SentAt { get; set; }

    public DateTime? DeliveredAt { get; set; }

    // Push only - one entry per device token (see docs/project-guide.md section 7)
    public List<PushRecipientStatus>? Recipients { get; set; }

    // InApp only
    public bool? Read { get; set; }

    public DateTime? ReadAt { get; set; }
}
