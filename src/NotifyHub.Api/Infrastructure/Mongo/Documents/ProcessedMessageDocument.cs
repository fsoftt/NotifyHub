namespace NotifyHub.Api.Infrastructure.Mongo.Documents
{
    public sealed class ProcessedMessageDocument
    {
        public string Id { get; init; } = default!;
        public string Status { get; init; } = default!;
        public DateTimeOffset UpdatedAt { get; init; }
        public DateTimeOffset? LeaseUntil { get; init; }
    }
}
