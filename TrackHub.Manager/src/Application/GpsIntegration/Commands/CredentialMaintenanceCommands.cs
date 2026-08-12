// Copyright (c) 2026 Sergio Hernandez. All rights reserved.
//
//  Licensed under the Apache License, Version 2.0 (the "License").
//  See the License for the specific language governing permissions and
//  limitations under the License.
//

using System.Text.Json;
using Microsoft.Extensions.Logging;
using TrackHub.Manager.Application.AlertEvents.Events;

namespace TrackHub.Manager.Application.GpsIntegration.Commands;

[Authorize(Resource = Resources.Credentials, Action = Actions.Custom, PrincipalTypes = "ServiceClient")]
[RequireFeature(FeatureKeys.GpsIntegration, AllowGlobalServiceClient = true)]
public readonly record struct EmitExpiringCredentialAlertsCommand(int WithinDays = 7) : IRequest<int>;

public class EmitExpiringCredentialAlertsCommandHandler(
    ICredentialReader credentialReader,
    IAlertEventWriter alertWriter,
    IPublisher publisher,
    ILogger<EmitExpiringCredentialAlertsCommandHandler> logger)
    : IRequestHandler<EmitExpiringCredentialAlertsCommand, int>
{
    public async Task<int> Handle(EmitExpiringCredentialAlertsCommand request, CancellationToken cancellationToken)
    {
        var withinDays = request.WithinDays <= 0 ? 7 : Math.Min(request.WithinDays, 90);
        var cutoff = DateTimeOffset.UtcNow.AddDays(withinDays);

        var expiring = await credentialReader.GetExpiringCredentialsAsync(cutoff, cancellationToken);
        var emitted = 0;

        foreach (var credential in expiring)
        {
            if (credential.AccountId == Guid.Empty)
            {
                continue;
            }

            try
            {
                var bucket = DateTimeOffset.UtcNow.ToString("yyyyMMdd");
                var alertEvent = await alertWriter.RecordAlertEventAsync(new AlertEventDto(
                    credential.AccountId,
                    EventType: "GpsCredentialExpiring",
                    Severity: "Warning",
                    SourceModule: "GpsIntegration",
                    ResourceType: "Operator",
                    ResourceId: credential.OperatorId.ToString(),
                    Status: "Open",
                    PayloadJson: JsonSerializer.Serialize(new
                    {
                        credential.CredentialId,
                        credential.OperatorId,
                        credential.TokenExpiration,
                        credential.RefreshTokenExpiration,
                        credential.EarliestExpirationAt,
                        WithinDays = withinDays
                    }),
                    DeduplicationKey: $"gps-credential-expiring:{credential.OperatorId:N}:{bucket}"),
                    cancellationToken);
                // Notification fan-out; the event handler is non-blocking.
                await publisher.Publish(new AlertEventRecorded.Notification(alertEvent), cancellationToken);
                emitted++;
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex,
                    "Failed to emit expiring-credential alert for operator {OperatorId}.",
                    credential.OperatorId);
            }
        }

        return emitted;
    }
}
