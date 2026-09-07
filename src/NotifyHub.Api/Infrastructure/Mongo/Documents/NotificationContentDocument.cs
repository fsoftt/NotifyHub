namespace NotifyHub.Api.Infrastructure.Mongo.Documents
{
    public sealed class NotificationContentDocument
    {
        public string Title { get; init; } = default!;
        public string Message { get; init; } = default!;
    }
}
