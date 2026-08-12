// Copyright (c) 2026 Sergio Hernandez. All rights reserved.
//
//  Licensed under the Apache License, Version 2.0 (the "License").
//  You may not use this file except in compliance with the License.
//  You may obtain a copy of the License at
//
//      http://www.apache.org/licenses/LICENSE-2.0
//
//  Unless required by applicable law or agreed to in writing, software
//  distributed under the License is distributed on an "AS IS" BASIS,
//  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  See the License for the specific language governing permissions and
//  limitations under the License.
//

using System.Text.Json;

namespace TrackHub.Geofencing.Infrastructure.ManagerApi;

/// <summary>
/// Emits geofence alert events to Manager's <c>recordAlertEvent</c> under the service's own
/// <c>geofence_client</c> identity (never the caller's token). Event-type and severity literals
/// match Manager's AlertEventTypes/AlertSeverities catalogs.
/// </summary>
public class AlertEmitter(IGraphQLClientFactory graphQLClient)
    : GraphQLService(graphQLClient.CreateClient(Clients.Manager, asService: true)), IAlertEmitter
{
    internal const string RecordAlertEventMutation = @"
                mutation($command: RecordAlertEventCommandInput!) {
                    recordAlertEvent(command: $command) { alertEventId }
                }";

    private const string SourceModule = "TrackHub.Geofencing";

    private static readonly JsonSerializerOptions PayloadOptions = new(JsonSerializerDefaults.Web);

    public async Task EmitGeofenceEnteredAsync(GeofenceAlertDto alert, CancellationToken cancellationToken)
        => await RecordAsync("GeofenceEntered", "Info", $"geofence-enter:{alert.GeofenceEventId:N}", alert, cancellationToken);

    public async Task EmitGeofenceExitedAsync(GeofenceAlertDto alert, CancellationToken cancellationToken)
        => await RecordAsync("GeofenceExited", "Info", $"geofence-exit:{alert.GeofenceEventId:N}", alert, cancellationToken);

    public async Task EmitGeofenceDwellExceededAsync(GeofenceAlertDto alert, CancellationToken cancellationToken)
        => await RecordAsync("GeofenceDwellExceeded", "Warning", $"geofence-dwell:{alert.GeofenceEventId:N}", alert, cancellationToken);

    private async Task RecordAsync(
        string eventType,
        string severity,
        string deduplicationKey,
        GeofenceAlertDto alert,
        CancellationToken cancellationToken)
    {
        var request = new GraphQLRequest
        {
            Query = RecordAlertEventMutation,
            Variables = new
            {
                command = new
                {
                    alertEvent = new
                    {
                        accountId = alert.AccountId,
                        eventType,
                        severity,
                        sourceModule = SourceModule,
                        resourceType = "Geofence",
                        resourceId = alert.GeofenceId.ToString(),
                        status = "Open",
                        payloadJson = JsonSerializer.Serialize(new
                        {
                            alert.GeofenceId,
                            alert.GeofenceName,
                            alert.GeofenceType,
                            alert.TransporterId,
                            alert.AccountId,
                            alert.GeofenceEventId,
                            alert.EnteredAt,
                            alert.ExitedAt,
                            alert.DwellSeconds,
                            alert.Latitude,
                            alert.Longitude
                        }, PayloadOptions),
                        deduplicationKey
                    }
                }
            }
        };
        await MutationAsync<object>(request, cancellationToken);
    }
}
