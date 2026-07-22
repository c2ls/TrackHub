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

namespace TrackHub.TripManagement.Infrastructure.ManagerApi;

/// <summary>
/// Emits trip alert events to Manager's <c>recordAlertEvent</c> under the service's own
/// <c>trip_client</c> identity (never the caller's token). Event-type and severity literals are
/// supplied by the caller and must match Manager's AlertEventTypes/AlertSeverities catalogs
/// (spec 11 §12).
/// </summary>
public class AlertEmitter(IGraphQLClientFactory graphQLClient)
    : GraphQLService(graphQLClient.CreateClient(Clients.Manager, asService: true)), IAlertEmitter
{
    internal const string RecordAlertEventMutation = @"
                mutation($command: RecordAlertEventCommandInput!) {
                    recordAlertEvent(command: $command) { alertEventId }
                }";

    private static readonly JsonSerializerOptions PayloadOptions = new(JsonSerializerDefaults.Web);

    public async Task EmitAsync(string eventType, string severity, string deduplicationKey, TripAlertDto alert, CancellationToken cancellationToken)
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
                        sourceModule = TripSharing.SourceModule,
                        resourceType = TripSharing.ResourceType,
                        resourceId = alert.TripId.ToString(),
                        status = "Open",
                        payloadJson = JsonSerializer.Serialize(new
                        {
                            alert.AccountId,
                            alert.TripId,
                            alert.TripStopId,
                            alert.TripCode,
                            alert.TransporterId,
                            alert.DriverId,
                            alert.StopName,
                            alert.OccurredAt,
                            alert.EtaAt,
                            alert.PlannedArrivalTo,
                            alert.DelayMinutes,
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
