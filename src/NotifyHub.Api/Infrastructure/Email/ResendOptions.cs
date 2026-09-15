namespace NotifyHub.Api.Infrastructure.Email;

public class ResendOptions
{
    public const string SectionName = "Email";

    public string ApiKey { get; set; } = string.Empty;

    // onboarding@resend.dev works for testing without configuring a custom
    // domain first (section 10).
    public string FromAddress { get; set; } = "onboarding@resend.dev";
}
