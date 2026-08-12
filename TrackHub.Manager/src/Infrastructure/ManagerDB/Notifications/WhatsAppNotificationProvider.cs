using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using TrackHub.Manager.Domain.Constants;

namespace TrackHub.Manager.Infrastructure.ManagerDB.Notifications;

/// <summary>
/// WhatsApp channel — Meta WhatsApp Business Platform Cloud API, direct. Outbound,
/// template-based (utility category) only: the pre-approved template receives the rendered text as
/// its single body parameter. Stores the provider message id for billing metering.
/// </summary>
public sealed class WhatsAppNotificationProvider(IHttpClientFactory httpClientFactory, IOptions<WhatsAppOptions> options) : INotificationChannelProvider
{
    public const string HttpClientName = "WhatsAppCloudApi";

    public string Channel => NotificationChannels.WhatsApp;

    public async Task<NotificationSendResult> SendAsync(NotificationMessage message, CancellationToken cancellationToken)
    {
        var whatsApp = options.Value;
        if (string.IsNullOrWhiteSpace(whatsApp.PhoneNumberId) || string.IsNullOrWhiteSpace(whatsApp.AccessToken))
        {
            return new NotificationSendResult(false, null, "WhatsApp Cloud API is not configured (AppSettings:WhatsApp:PhoneNumberId/AccessToken).");
        }

        var body = JsonSerializer.Serialize(new
        {
            messaging_product = "whatsapp",
            to = message.Recipient.TrimStart('+'),
            type = "template",
            template = new
            {
                name = whatsApp.TemplateName,
                language = new { code = message.Locale },
                components = new object[]
                {
                    new { type = "body", parameters = new object[] { new { type = "text", text = message.Body } } }
                }
            }
        });

        using var request = new HttpRequestMessage(HttpMethod.Post, $"{whatsApp.ApiBaseUrl.TrimEnd('/')}/{whatsApp.PhoneNumberId}/messages")
        {
            Content = new StringContent(body, Encoding.UTF8, "application/json")
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", whatsApp.AccessToken);

        var client = httpClientFactory.CreateClient(HttpClientName);
        using var response = await client.SendAsync(request, cancellationToken);
        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return new NotificationSendResult(false, null, $"WhatsApp Cloud API returned {(int)response.StatusCode}: {Truncate(responseBody)}");
        }

        return new NotificationSendResult(true, ExtractMessageId(responseBody), null);
    }

    private static string? ExtractMessageId(string responseBody)
    {
        try
        {
            using var document = JsonDocument.Parse(responseBody);
            return document.RootElement.TryGetProperty("messages", out var messages) && messages.GetArrayLength() > 0
                && messages[0].TryGetProperty("id", out var id)
                    ? id.GetString()
                    : null;
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private static string Truncate(string value) => value.Length <= 500 ? value : value[..500];
}
