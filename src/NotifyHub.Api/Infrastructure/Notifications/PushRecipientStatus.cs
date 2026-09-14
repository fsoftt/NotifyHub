using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace NotifyHub.Api.Infrastructure.Notifications;

public class PushRecipientStatus
{
    public string Token { get; set; } = string.Empty;

    [BsonRepresentation(BsonType.String)]
    public NotificationChannelStatus Status { get; set; } = NotificationChannelStatus.Pending;

    public DateTime? SentAt { get; set; }

    public DateTime? DeliveredAt { get; set; }
}
