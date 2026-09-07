namespace NotifyHub.Api.Infrastructure.Mongo.Documents
{
    public sealed class NotificationChannelDocument
    {
        public string Type { get; init; } = default!;
        public string Status { get; init; } = default!;
        public DateTimeOffset? SentAt { get; init; }
        public DateTimeOffset? DeliveredAt { get; init; }
    }
}
