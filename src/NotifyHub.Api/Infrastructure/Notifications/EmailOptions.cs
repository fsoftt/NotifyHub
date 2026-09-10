namespace NotifyHub.Api.Infrastructure.Notifications
{
    public sealed class EmailOptions
    {
        public const string SectionName = "Email";
        public required string ApiKey { get; init; }
        public required string Sender { get; init; }
    }
}
