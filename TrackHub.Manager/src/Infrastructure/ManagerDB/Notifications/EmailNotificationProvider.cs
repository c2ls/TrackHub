using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using TrackHub.Manager.Domain.Constants;

namespace TrackHub.Manager.Infrastructure.ManagerDB.Notifications;

/// <summary>Email channel over SMTP via MailKit.</summary>
public sealed class EmailNotificationProvider(IOptions<SmtpOptions> options) : INotificationChannelProvider
{
    public string Channel => NotificationChannels.Email;

    public async Task<NotificationSendResult> SendAsync(NotificationMessage message, CancellationToken cancellationToken)
    {
        var smtp = options.Value;
        if (string.IsNullOrWhiteSpace(smtp.Host))
        {
            return new NotificationSendResult(false, null, "SMTP is not configured (AppSettings:Smtp:Host).");
        }

        var mime = new MimeMessage();
        mime.From.Add(new MailboxAddress(smtp.FromName, smtp.FromAddress));
        mime.To.Add(MailboxAddress.Parse(message.Recipient));
        mime.Subject = message.Subject ?? "TrackHub notification";
        mime.Body = new TextPart("plain") { Text = message.Body };

        using var client = new SmtpClient();
        await client.ConnectAsync(smtp.Host, smtp.Port, smtp.UseStartTls ? SecureSocketOptions.StartTls : SecureSocketOptions.Auto, cancellationToken);
        if (!string.IsNullOrEmpty(smtp.Username))
        {
            await client.AuthenticateAsync(smtp.Username, smtp.Password ?? string.Empty, cancellationToken);
        }

        var response = await client.SendAsync(mime, cancellationToken);
        await client.DisconnectAsync(true, cancellationToken);
        return new NotificationSendResult(true, response, null);
    }
}
