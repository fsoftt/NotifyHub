using Microsoft.Extensions.Options;
using Resend;

namespace NotifyHub.Api.Infrastructure.Notifications
{
    public sealed class ResendEmailSender : IEmailSender
    {
        private readonly EmailOptions options;

        public ResendEmailSender(
            IOptions<EmailOptions> options)
        {
            this.options = options.Value;
        }

        public async Task SendAsync(
            string idempotencyKey,
            string recipient,
            string subject,
            string body,
            CancellationToken cancellationToken)
        {
            const int maxAttempts = 3;

            for (var attempt = 1; attempt <= maxAttempts; attempt++)
            {
                try
                {
                    await SendOnceAsync(idempotencyKey, recipient, subject, body, cancellationToken);
                    return;
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Attempt {attempt} to send email to {recipient} failed. Exception: {ex.Message}");

                    if (attempt == maxAttempts)
                    {
                        Console.WriteLine($"Giving up sending email to {recipient} after {maxAttempts} attempts.: {ex.Message}");
                        throw;
                    }

                    var delay = TimeSpan.FromSeconds(Math.Pow(2, attempt));
                    await Task.Delay(delay, cancellationToken);
                }
            }
        }

        private async Task SendOnceAsync(
            string idempotencyKey,
            string recipient,
            string subject,
            string body,
            CancellationToken cancellationToken)
        {
            IResend resend = ResendClient.Create(options.ApiKey);

            var resp = await resend.EmailSendAsync(idempotencyKey, new EmailMessage()
            {
                From = options.Sender,
                To = recipient,
                Subject = subject,
                HtmlBody = body,
            });
            Console.WriteLine("Email Id={0}", resp.Content);
        }
    }
}
