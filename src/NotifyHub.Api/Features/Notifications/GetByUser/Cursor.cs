namespace NotifyHub.Api.Features.Notifications.GetByUser
{
    public sealed class Cursor
    {
        public DateTimeOffset CreatedAt { get; set; }
        public string Id { get; set; }

        public Cursor()
        {
            CreatedAt = DateTimeOffset.UtcNow;
            Id = string.Empty;
        }

        public Cursor(DateTimeOffset createdAt, string id)
        {
            CreatedAt = createdAt;
            Id = id ?? throw new ArgumentNullException(nameof(id));
        }
    }
}
