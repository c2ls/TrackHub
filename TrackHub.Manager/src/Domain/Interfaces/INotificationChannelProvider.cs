namespace TrackHub.Manager.Domain.Interfaces;

/// <summary>
/// Channel delivery provider contract. Implementations: InApp, Email (MailKit),
/// Webhook (HMAC-signed), WhatsApp (Meta Cloud API). Push is contract-only — the FCM
/// implementation, unmasked-token query, and credentials are not yet implemented.
/// </summary>
public interface INotificationChannelProvider
{
    /// <summary>One of <c>NotificationChannels</c>.</summary>
    string Channel { get; }

    Task<NotificationSendResult> SendAsync(NotificationMessage message, CancellationToken cancellationToken);
}
