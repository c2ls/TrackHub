using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using TrackHub.Manager.Domain.Constants;

namespace TrackHub.Manager.Infrastructure.ManagerDB.Notifications;

/// <summary>
/// Outbound webhook channel: POSTs a JSON envelope to the rule's URL with an
/// HMAC-SHA256 signature header over the exact body, signed with the rule's shared secret.
/// Delivery status is the HTTP response; there is no inbound callback in this slice.
/// </summary>
public sealed class WebhookNotificationProvider(IHttpClientFactory httpClientFactory) : INotificationChannelProvider
{
    public const string HttpClientName = "NotificationWebhook";
    public const string SignatureHeader = "X-TrackHub-Signature";

    public string Channel => NotificationChannels.Webhook;

    public async Task<NotificationSendResult> SendAsync(NotificationMessage message, CancellationToken cancellationToken)
    {
        if (!Uri.TryCreate(message.Recipient, UriKind.Absolute, out var url))
        {
            return new NotificationSendResult(false, null, "Webhook delivery has no valid target URL.");
        }
        if (string.IsNullOrEmpty(message.WebhookSecret))
        {
            return new NotificationSendResult(false, null, "Webhook rule has no shared secret configured.");
        }

        var body = JsonSerializer.Serialize(new
        {
            deliveryId = message.NotificationDeliveryId,
            accountId = message.AccountId,
            subject = message.Subject,
            message = message.Body,
            payload = message.EventPayloadJson,
            sentAt = DateTimeOffset.UtcNow
        });

        var signature = Convert.ToHexStringLower(HMACSHA256.HashData(Encoding.UTF8.GetBytes(message.WebhookSecret), Encoding.UTF8.GetBytes(body)));

        using var request = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = new StringContent(body, Encoding.UTF8, "application/json")
        };
        request.Headers.TryAddWithoutValidation(SignatureHeader, $"sha256={signature}");

        var client = httpClientFactory.CreateClient(HttpClientName);
        using var response = await client.SendAsync(request, cancellationToken);
        return response.IsSuccessStatusCode
            ? new NotificationSendResult(true, null, null)
            : new NotificationSendResult(false, null, $"Webhook endpoint returned {(int)response.StatusCode}.");
    }
}
