namespace NotifyHub.Api.Infrastructure.Mongo.Documents
{
    public sealed class NotificationDocument
    {
        public string Id { get; init; } = default!;
        public string? UserId { get; init; } = default!;
        public string Email { get; init; } = default!;
        public string Type { get; init; } = default!;
        public string? PushRecipient { get; init; }

        public NotificationContentDocument Content { get; init; } = default!;

        public IReadOnlyCollection<NotificationChannelDocument> Channels { get; init; } = [];

        public bool Read { get; init; }
        public DateTimeOffset? ReadAt { get; init; }
        public DateTimeOffset CreatedAt { get; init; }
    }
}
