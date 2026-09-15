using System.Net.Http.Json;
using Microsoft.Extensions.Options;

namespace NotifyHub.Api.Infrastructure.Email;

// Translates our internal abstraction into Resend's API (section 9). Nothing
// upstream of this class (EmailNotificationProcessor, NotificationConsumer)
// knows Resend exists.
//
// Uses IHttpClientFactory rather than an injected HttpClient directly: this
// class (and everything above it - EmailNotificationProcessor,
// NotificationConsumer) is a singleton, and holding one HttpClient forever
// would prevent the factory's handler rotation (DNS changes, etc.) from ever
// taking effect. CreateClient() per call keeps that working correctly.
public class ResendEmailSender : IEmailSender
{
    public const string HttpClientName = "Resend";

    private readonly IHttpClientFactory httpClientFactory;
    private readonly ResendOptions options;

    public ResendEmailSender(IHttpClientFactory httpClientFactory, IOptions<ResendOptions> options)
    {
        this.httpClientFactory = httpClientFactory;
        this.options = options.Value;
    }

    public async Task SendAsync(
        string idempotencyKey,
        string recipient,
        string subject,
        string body,
        CancellationToken cancellationToken)
    {
        var httpClient = httpClientFactory.CreateClient(HttpClientName);

        using var request = new HttpRequestMessage(HttpMethod.Post, "emails")
        {
            Content = JsonContent.Create(new
            {
                from = options.FromAddress,
                to = new[] { recipient },
                subject,
                text = body
            })
        };

        // Resend's own idempotency mechanism (section 16): a retried send with the
        // same key returns the original result instead of sending a second email.
        request.Headers.Add("Idempotency-Key", idempotencyKey);

        using var response = await httpClient.SendAsync(request, cancellationToken);

        response.EnsureSuccessStatusCode();
    }
}
