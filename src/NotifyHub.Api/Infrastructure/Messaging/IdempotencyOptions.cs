namespace NotifyHub.Api.Infrastructure.Messaging
{
    public sealed class IdempotencyOptions
    {
        public const string SectionName = "Idempotency";
        public int LeaseSeconds { get; init; } = 30;
    }
}
